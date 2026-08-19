using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Jose;
using backend.Data;
using backend.Models;
using backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Hangfire;
using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=db;Port=5432;Database=callcenter;Username=admin;Password=adminpassword";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSignalR();

// Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    
    // Reset all agents to offline on startup
    var onlineAgents = db.Agents.Where(a => a.IsOnline).ToList();
    foreach(var a in onlineAgents) { a.IsOnline = false; a.Status = "Offline"; }
    db.SaveChanges();
    
    // Seed default agent
    if (!db.Agents.Any()) {
        db.Agents.Add(new AgentUser { Username = "admin", PasswordHash = "admin", IsOnline = false, Status = "Offline" });
        db.SaveChanges();
    }
}

app.UseCors();
app.UseDefaultFiles();
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".ogg"] = "audio/ogg";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

// Hangfire Dashboard (admin only)
app.UseHangfireDashboard("/hangfire");

// Recurring job: broadcast queue stats every 3 seconds
RecurringJob.AddOrUpdate<QueueBroadcaster>(
    "broadcast-queue-stats",
    j => j.BroadcastAsync(),
    "*/3 * * * * *",
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
);

// ── TOKEN ──────────────────────────────────────────────────────────────────
app.MapGet("/api/token", async (string? identity, string? room, AppDbContext db) => {
    if (string.IsNullOrEmpty(identity))
        identity = "web-user-" + Guid.NewGuid().ToString("N")[..8];

    var apiKey    = Environment.GetEnvironmentVariable("LIVEKIT_API_KEY")    ?? "devkey";
    var apiSecret = Environment.GetEnvironmentVariable("LIVEKIT_API_SECRET") ?? "secret";
    var roomName  = room ?? ("web-room-" + Guid.NewGuid().ToString("N")[..8]);
    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    var payload = new Dictionary<string, object> {
        { "iss", apiKey }, { "sub", identity }, { "name", identity },
        { "nbf", now }, { "exp", now + 7200 },
        { "video", new Dictionary<string, object> {
            { "roomJoin", true }, { "room", roomName },
            { "canPublish", true }, { "canSubscribe", true }
        }}
    };
    var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(apiSecret), JwsAlgorithm.HS256);

    if (!identity.StartsWith("admin_") && !identity.StartsWith("agent_")) {
        var existing = await db.Calls.FirstOrDefaultAsync(c => c.RoomName == roomName);
        if (existing == null) {
            db.Calls.Add(new CallRecord { RoomName = roomName, CallerId = identity, Status = "Active",
                RecordingUrl = $"/recordings/{roomName}.ogg" });
            await db.SaveChangesAsync();
            _ = StartRecording(roomName);
        }
    }
    return Results.Json(new { token, url = "ws://127.0.0.1:7880", roomName });
});

// ── AGENT LOGIN ─────────────────────────────────────────────────────────────
app.MapPost("/api/agent/login", async (LoginDto login, AppDbContext db) => {
    var agent = await db.Agents.FirstOrDefaultAsync(a => a.Username == login.Username && a.PasswordHash == login.Password);
    if (agent == null) return Results.Unauthorized();
    return Results.Ok(new { agent.Id, agent.Username });
});

// ── AGENT STATUS UPDATE ──────────────────────────────────────────────────────
app.MapPost("/api/agent/status", async (AgentStatusDto req, AppDbContext db, IHubContext<CallHub> hub) => {
    var agent = await db.Agents.FirstOrDefaultAsync(a => a.Username == req.Username);
    if (agent == null) return Results.NotFound();
    agent.Status = req.Status; // "Available" | "Break" | "NotReady"
    agent.IsOnline = (req.Status != "Offline");
    await db.SaveChangesAsync();
    await hub.Clients.All.SendAsync("AgentStatusChanged", agent.Username, agent.Status);
    return Results.Ok();
});

// ── CALL TRANSFER ────────────────────────────────────────────────────────────
app.MapPost("/api/call/transfer", async (TransferDto req, AppDbContext db, IHubContext<CallHub> hub) => {
    var availableAgent = await db.Agents.FirstOrDefaultAsync(a => a.IsOnline && a.Status == "Available");
    if (availableAgent == null) return Results.BadRequest("No agents available");

    var call = await db.Calls.OrderByDescending(c => c.Id).FirstOrDefaultAsync(c => c.RoomName == req.RoomName);
    if (call != null) {
        call.HandledByAgentId = availableAgent.Id;
        call.Status = "Transferred";
    } else {
        call = new CallRecord { RoomName = req.RoomName, HandledByAgentId = availableAgent.Id, Status = "Transferred",
            CallerId = "SIP Caller", RecordingUrl = $"/recordings/{req.RoomName}.ogg" };
        db.Calls.Add(call);
    }
    await db.SaveChangesAsync();
    await hub.Clients.All.SendAsync("IncomingTransfer", req.RoomName);
    return Results.Ok(new { agentId = availableAgent.Id });
});

// ── CALL ACTIVE (SIP) ────────────────────────────────────────────────────────
app.MapPost("/api/call/active", async (TransferDto req, AppDbContext db) => {
    var existing = await db.Calls.FirstOrDefaultAsync(c => c.RoomName == req.RoomName);
    if (existing == null) {
        db.Calls.Add(new CallRecord { RoomName = req.RoomName, CallerId = "SIP Caller",
            Status = "Active", RecordingUrl = $"/recordings/{req.RoomName}.ogg" });
        await db.SaveChangesAsync();
        _ = StartRecording(req.RoomName);
    }
    return Results.Ok();
});

// ── CALL END ─────────────────────────────────────────────────────────────────
app.MapPost("/api/call/end", async (TransferDto req, AppDbContext db) => {
    var call = await db.Calls.OrderByDescending(c => c.Id).FirstOrDefaultAsync(c => c.RoomName == req.RoomName);
    if (call != null && call.Status == "Active") {
        call.Status = "Completed";
        call.EndTime = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
    return Results.Ok();
});

// ── CALL SUMMARY ─────────────────────────────────────────────────────────────
app.MapPost("/api/call/summary", async (SummaryDto req, AppDbContext db) => {
    var call = await db.Calls.OrderByDescending(c => c.Id).FirstOrDefaultAsync(c => c.RoomName == req.RoomName);
    if (call == null) {
        db.Calls.Add(new CallRecord { RoomName = req.RoomName, Summary = req.Summary,
            EndTime = DateTime.UtcNow, Status = "Completed" });
    } else {
        call.Summary = req.Summary;
        if (call.EndTime == null) call.EndTime = DateTime.UtcNow;
    }
    await db.SaveChangesAsync();
    return Results.Ok();
});

// ── CALLS LIST (CDR) ──────────────────────────────────────────────────────────
app.MapGet("/api/calls", async (AppDbContext db, string? status, string? date) => {
    var query = db.Calls.AsQueryable();
    if (!string.IsNullOrEmpty(status)) query = query.Where(c => c.Status == status);
    if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var d)) {
        var utcDate = DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);
        query = query.Where(c => c.StartTime.Date == utcDate);
    }
    var calls = await query.OrderByDescending(c => c.StartTime).Take(200).ToListAsync();
    return Results.Ok(calls);
});

// ── CALLS DELETE ──────────────────────────────────────────────────────────────
app.MapDelete("/api/calls/{roomName}", async (string roomName, AppDbContext db) => {
    var call = await db.Calls.FirstOrDefaultAsync(c => c.RoomName == roomName);
    if (call != null) { db.Calls.Remove(call); await db.SaveChangesAsync(); }
    await DeleteLiveKitRoom(roomName);
    return Results.Ok();
});

// ── CALLS FORCE END ───────────────────────────────────────────────────────────
app.MapPost("/api/calls/{roomName}/end", async (string roomName, AppDbContext db) => {
    var call = await db.Calls.OrderByDescending(c => c.Id).FirstOrDefaultAsync(c => c.RoomName == roomName);
    if (call != null && (call.Status == "Active" || call.Status == "Transferred")) {
        call.Status = "Completed";
        call.EndTime = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
    await DeleteLiveKitRoom(roomName);
    return Results.Ok();
});

// ── STATS TODAY ────────────────────────────────────────────────────────────────
app.MapGet("/api/stats/today", async (AppDbContext db) => {
    var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
    var calls = await db.Calls.Where(c => c.StartTime.Date == today).ToListAsync();
    var active = await db.Calls.CountAsync(c => c.Status == "Active" || c.Status == "Transferred");
    var agentsOnline = await db.Agents.CountAsync(a => a.IsOnline);

    var completed = calls.Where(c => c.EndTime.HasValue).ToList();
    var avgDuration = completed.Any()
        ? (int)completed.Average(c => (c.EndTime!.Value - c.StartTime).TotalSeconds)
        : 0;

    // Hourly breakdown for chart (last 12 hours)
    var now = DateTime.UtcNow;
    var hourly = Enumerable.Range(0, 12).Select(i => {
        var hour = now.AddHours(-11 + i);
        var count = calls.Count(c => c.StartTime.Hour == hour.Hour && c.StartTime.Date == hour.Date);
        return new { hour = hour.ToString("HH:mm"), count };
    }).ToList();

    return Results.Ok(new {
        total = calls.Count,
        active,
        answered = calls.Count(c => c.Status == "Completed"),
        transferred = calls.Count(c => c.HandledByAgentId != null),
        missed = 0,
        avgDurationSeconds = avgDuration,
        agentsOnline,
        hourly
    });
});

// ── QUEUE STATUS ───────────────────────────────────────────────────────────────
app.MapGet("/api/stats/queue", async (AppDbContext db) => {
    var activeCalls = await db.Calls
        .Where(c => c.Status == "Active" || c.Status == "Transferred")
        .OrderByDescending(c => c.StartTime)
        .ToListAsync();
    var agents = await db.Agents.Select(a => new { a.Id, a.Username, a.IsOnline, a.Status }).ToListAsync();
    var agentCalls = await db.Calls.Where(c => c.Status == "Transferred" && c.HandledByAgentId != null).ToListAsync();

    var agentList = agents.Select(a => {
        bool isInCall = agentCalls.Any(c => c.HandledByAgentId == a.Id);
        string status = !a.IsOnline ? "Offline" : (isInCall ? "In Call" : (a.Status ?? "Available"));
        return new { a.Id, a.Username, Status = status };
    });

    return Results.Ok(new {
        activeCalls = activeCalls.Select(c => new {
            c.Id, c.RoomName, c.CallerId, c.Status,
            c.StartTime, c.HandledByAgentId,
            durationSeconds = (int)(DateTime.UtcNow - c.StartTime).TotalSeconds
        }),
        agents = agentList
    });
});

// ── AGENTS LIST ────────────────────────────────────────────────────────────────
app.MapGet("/api/agents", async (AppDbContext db) => {
    var agents = await db.Agents.Select(a => new { a.Id, a.Username, a.IsOnline, a.Status }).ToListAsync();
    var activeCalls = await db.Calls.Where(c => c.Status == "Transferred" && c.HandledByAgentId != null).ToListAsync();
    var result = agents.Select(a => {
        bool isInCall = activeCalls.Any(c => c.HandledByAgentId == a.Id);
        string status = !a.IsOnline ? "Offline" : (isInCall ? "In Call" : "Available");
        return new { a.Id, a.Username, Status = status };
    });
    return Results.Ok(result);
});

app.MapHub<CallHub>("/hubs/call");

app.Run("http://0.0.0.0:5000");

// ── HELPERS ────────────────────────────────────────────────────────────────────
async Task StartRecording(string roomName) {
    var apiKey    = Environment.GetEnvironmentVariable("LIVEKIT_API_KEY")    ?? "devkey";
    var apiSecret = Environment.GetEnvironmentVariable("LIVEKIT_API_SECRET") ?? "secret";
    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var token = JWT.Encode(new Dictionary<string, object> {
        { "iss", apiKey }, { "sub", apiKey }, { "nbf", now }, { "exp", now + 300 },
        { "video", new Dictionary<string, object> { { "roomAdmin", true }, { "room", roomName }, { "roomRecord", true } } }
    }, Encoding.UTF8.GetBytes(apiSecret), JwsAlgorithm.HS256);

    var reqBody = new { room_name = roomName, audio_only = true, file = new { filepath = $"/recordings/{roomName}.ogg" } };
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    try {
        var host = Environment.GetEnvironmentVariable("LIVEKIT_URL") ?? "http://livekit:7880";
        var resp = await client.PostAsync($"{host}/twirp/livekit.Egress/StartRoomCompositeEgress",
            new StringContent(JsonSerializer.Serialize(reqBody), Encoding.UTF8, "application/json"));
        Console.WriteLine($"Egress: {resp.StatusCode} - {await resp.Content.ReadAsStringAsync()}");
    } catch (Exception ex) { Console.WriteLine($"Egress Exception: {ex.Message}"); }
}

async Task DeleteLiveKitRoom(string roomName) {
    var apiKey    = Environment.GetEnvironmentVariable("LIVEKIT_API_KEY")    ?? "devkey";
    var apiSecret = Environment.GetEnvironmentVariable("LIVEKIT_API_SECRET") ?? "secret";
    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var token = JWT.Encode(new Dictionary<string, object> {
        { "iss", apiKey }, { "sub", apiKey }, { "nbf", now }, { "exp", now + 300 },
        { "video", new Dictionary<string, object> { { "roomCreate", true }, { "roomAdmin", true }, { "room", roomName } } }
    }, Encoding.UTF8.GetBytes(apiSecret), JwsAlgorithm.HS256);
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    try {
        var host = Environment.GetEnvironmentVariable("LIVEKIT_URL") ?? "http://livekit:7880";
        var resp = await client.PostAsync($"{host}/twirp/livekit.RoomService/DeleteRoom",
            new StringContent($"{{\"room\":\"{roomName}\"}}", Encoding.UTF8, "application/json"));
        Console.WriteLine($"DeleteRoom: {resp.StatusCode} - {await resp.Content.ReadAsStringAsync()}");
    } catch (Exception ex) { Console.WriteLine($"DeleteRoom Error: {ex.Message}"); }
}

// ── DTOs ───────────────────────────────────────────────────────────────────────
public class LoginDto        { public required string Username { get; set; } public required string Password { get; set; } }
public class TransferDto     { public required string RoomName { get; set; } public string? AgentId { get; set; } }
public class SummaryDto      { public required string RoomName { get; set; } public required string Summary { get; set; } }
public class AgentStatusDto  { public required string Username { get; set; } public required string Status  { get; set; } }

// ── HANGFIRE QUEUE BROADCASTER ─────────────────────────────────────────────────
public class QueueBroadcaster
{
    private readonly IHubContext<CallHub> _hub;
    private readonly AppDbContext _db;
    public QueueBroadcaster(IHubContext<CallHub> hub, AppDbContext db) { _hub = hub; _db = db; }

    public async Task BroadcastAsync() {
        var activeCalls = await _db.Calls
            .Where(c => c.Status == "Active" || c.Status == "Transferred")
            .ToListAsync();
        var agents = await _db.Agents.Select(a => new { a.Id, a.Username, a.IsOnline, a.Status }).ToListAsync();
        var agentCalls = await _db.Calls.Where(c => c.Status == "Transferred" && c.HandledByAgentId != null).ToListAsync();

        var agentList = agents.Select(a => {
            bool isInCall = agentCalls.Any(c => c.HandledByAgentId == a.Id);
            string st = !a.IsOnline ? "Offline" : (isInCall ? "In Call" : (a.Status ?? "Available"));
            return new { a.Id, a.Username, Status = st };
        });

        var payload = new {
            activeCount = activeCalls.Count,
            agentsOnline = agents.Count(a => a.IsOnline),
            activeCalls = activeCalls.Select(c => new {
                c.Id, c.RoomName, c.CallerId, c.Status, c.StartTime, c.HandledByAgentId,
                durationSeconds = (int)(DateTime.UtcNow - c.StartTime).TotalSeconds
            }),
            agents = agentList
        };

        await _hub.Clients.All.SendAsync("QueueUpdate", payload);
    }
}
