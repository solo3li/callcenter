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
using Jose; // jose-jwt
using backend.Data;
using backend.Models;
using backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // Ensure DB is created
    
    // Reset all agents to offline on startup to prevent ghost online status
    var onlineAgents = db.Agents.Where(a => a.IsOnline).ToList();
    foreach(var a in onlineAgents) a.IsOnline = false;
    db.SaveChanges();
    
    // Seed default agent if not exists
    if (!db.Agents.Any()) {
        db.Agents.Add(new backend.Models.AgentUser { Username = "admin", PasswordHash = "admin", IsOnline = false });
        db.SaveChanges();
    }
}

app.UseCors();
app.UseDefaultFiles();
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".ogg"] = "audio/ogg";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

// Token generation for LiveKit
app.MapGet("/api/token", async (string identity = null, string? room = null, AppDbContext db = null) => {
    if (string.IsNullOrEmpty(identity)) {
        identity = "web-user-" + Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    var apiKey = Environment.GetEnvironmentVariable("LIVEKIT_API_KEY") ?? "devkey";
    var apiSecret = Environment.GetEnvironmentVariable("LIVEKIT_API_SECRET") ?? "secret";
    var roomName = room ?? ("web-room-" + Guid.NewGuid().ToString("N").Substring(0, 8));

    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var exp = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds();

    var payload = new Dictionary<string, object>
    {
        { "iss", apiKey },
        { "sub", identity },
        { "name", identity },
        { "nbf", now },
        { "exp", exp },
        { "video", new Dictionary<string, object>
            {
                { "roomJoin", true },
                { "room", roomName },
                { "canPublish", true },
                { "canSubscribe", true }
            }
        }
    };

    var secretKey = Encoding.UTF8.GetBytes(apiSecret);
    var tokenString = JWT.Encode(payload, secretKey, JwsAlgorithm.HS256);
    
    // Clients outside Docker need to connect to localhost or the host's IP
    var host = "ws://127.0.0.1:7880";

    // Track in DB
    if (db != null && !identity.StartsWith("admin_") && !identity.StartsWith("agent_"))
    {
        var existing = await db.Calls.FirstOrDefaultAsync(c => c.RoomName == roomName);
        if (existing == null) {
            db.Calls.Add(new CallRecord { RoomName = roomName, CallerId = identity, Status = "Active", RecordingUrl = $"/recordings/{roomName}.mp4" });
            await db.SaveChangesAsync();
            _ = StartRecording(roomName);
        }
    }

    return Results.Json(new { token = tokenString, url = host, roomName = roomName });
});

// Agent Login
app.MapPost("/api/agent/login", async (LoginDto login, AppDbContext db) => {
    var agent = await db.Agents.FirstOrDefaultAsync(a => a.Username == login.Username && a.PasswordHash == login.Password);
    if (agent == null) return Results.Unauthorized();
    return Results.Ok(new { agent.Id, agent.Username });
});

// Transfer call endpoint (called by Python AI Worker)
app.MapPost("/api/call/transfer", async (TransferDto req, AppDbContext db, IHubContext<CallHub> hubContext) => {
    var availableAgent = await db.Agents.FirstOrDefaultAsync(a => a.IsOnline);
    if (availableAgent == null) return Results.BadRequest("No agents available");

    // Update the existing call record
    var call = await db.Calls.OrderByDescending(c => c.Id).FirstOrDefaultAsync(c => c.RoomName == req.RoomName);
    if (call != null) {
        call.HandledByAgentId = availableAgent.Id;
        call.Status = "Transferred";
    } else {
        call = new CallRecord { RoomName = req.RoomName, HandledByAgentId = availableAgent.Id, Status = "Transferred" };
        db.Calls.Add(call);
    }
    await db.SaveChangesAsync();

    // Alert the agent via SignalR
    await hubContext.Clients.All.SendAsync("IncomingTransfer", req.RoomName);

    return Results.Ok(new { agentId = availableAgent.Id });
});

// Helper for triggering egress
async Task StartRecording(string roomName) {
    var apiKey = Environment.GetEnvironmentVariable("LIVEKIT_API_KEY") ?? "devkey";
    var apiSecret = Environment.GetEnvironmentVariable("LIVEKIT_API_SECRET") ?? "secret";
    var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var exp = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
    var payload = new Dictionary<string, object> {
        { "iss", apiKey }, { "sub", apiKey }, { "nbf", now }, { "exp", exp },
        { "video", new Dictionary<string, object> { { "roomAdmin", true }, { "room", roomName }, { "roomRecord", true } } }
    };
    var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(apiSecret), JwsAlgorithm.HS256);
    
    var reqBody = new {
        room_name = roomName,
        audio_only = true,
        file = new {
            filepath = $"/recordings/{roomName}.ogg"
        }
    };

    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    var content = new StringContent(JsonSerializer.Serialize(reqBody), Encoding.UTF8, "application/json");
    try {
        var host = Environment.GetEnvironmentVariable("LIVEKIT_URL") ?? "http://livekit:7880";
        var resp = await client.PostAsync($"{host}/twirp/livekit.Egress/StartRoomCompositeEgress", content);
        var resBody = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"Egress Start Response: {resp.StatusCode} - {resBody}");
    } catch (Exception ex) {
        Console.WriteLine($"Egress Exception: {ex.Message}");
    }
}

// Register active call (called by Python AI Worker for SIP calls)
app.MapPost("/api/call/active", async (TransferDto req, AppDbContext db) => {
    var existing = await db.Calls.FirstOrDefaultAsync(c => c.RoomName == req.RoomName);
    if (existing == null) {
        db.Calls.Add(new CallRecord { RoomName = req.RoomName, CallerId = "SIP Caller", Status = "Active", RecordingUrl = $"/recordings/{req.RoomName}.ogg" });
        await db.SaveChangesAsync();
        _ = StartRecording(req.RoomName);
    }
    return Results.Ok();
});

// End call (called by Python AI Worker when user disconnects)
app.MapPost("/api/call/end", async (TransferDto req, AppDbContext db) => {
    var call = await db.Calls.OrderByDescending(c => c.Id).FirstOrDefaultAsync(c => c.RoomName == req.RoomName);
    if (call != null && call.Status == "Active") {
        call.Status = "Completed";
        call.EndTime = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
    return Results.Ok();
});

app.MapPost("/api/call/summary", async (SummaryDto req, AppDbContext db) => {
    var call = await db.Calls.OrderByDescending(c => c.Id).FirstOrDefaultAsync(c => c.RoomName == req.RoomName);
    if (call == null) {
        call = new CallRecord { RoomName = req.RoomName, Summary = req.Summary, EndTime = DateTime.UtcNow, Status = "Completed_AI" };
        db.Calls.Add(call);
    } else {
        call.Summary = req.Summary;
        call.EndTime = DateTime.UtcNow;
    }
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapGet("/api/calls", async (AppDbContext db) => {
    var calls = await db.Calls.OrderByDescending(c => c.StartTime).Take(50).ToListAsync();
    return Results.Ok(calls);
});

app.MapDelete("/api/calls/{roomName}", async (string roomName, AppDbContext db) => {
    var call = await db.Calls.FirstOrDefaultAsync(c => c.RoomName == roomName);
    if (call != null) {
        db.Calls.Remove(call);
        await db.SaveChangesAsync();
    }

    var apiKey = Environment.GetEnvironmentVariable("LIVEKIT_API_KEY") ?? "devkey";
    var apiSecret = Environment.GetEnvironmentVariable("LIVEKIT_API_SECRET") ?? "secret";
    var token = JWT.Encode(new Dictionary<string, object> {
        { "iss", apiKey }, { "sub", apiKey }, { "nbf", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }, { "exp", DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds() },
        { "video", new Dictionary<string, object> { { "roomCreate", true }, { "roomAdmin", true }, { "room", roomName } } }
    }, Encoding.UTF8.GetBytes(apiSecret), JwsAlgorithm.HS256);
    
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    try { 
        var resp = await client.PostAsync($"{Environment.GetEnvironmentVariable("LIVEKIT_URL") ?? "http://livekit:7880"}/twirp/livekit.RoomService/DeleteRoom", new StringContent($"{{\"room\":\"{roomName}\"}}", Encoding.UTF8, "application/json")); 
        var body = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"Twirp DeleteRoom Response (DELETE): {resp.StatusCode} - {body}");
    } catch (Exception e) {
        Console.WriteLine($"Twirp Error: {e.Message}");
    }
    return Results.Ok();
});

// Force end a call without deleting it
app.MapPost("/api/calls/{roomName}/end", async (string roomName, AppDbContext db) => {
    var call = await db.Calls.OrderByDescending(c => c.Id).FirstOrDefaultAsync(c => c.RoomName == roomName);
    if (call != null && (call.Status == "Active" || call.Status == "Transferred")) {
        call.Status = "Completed";
        call.EndTime = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    var apiKey = Environment.GetEnvironmentVariable("LIVEKIT_API_KEY") ?? "devkey";
    var apiSecret = Environment.GetEnvironmentVariable("LIVEKIT_API_SECRET") ?? "secret";
    var token = JWT.Encode(new Dictionary<string, object> {
        { "iss", apiKey }, { "sub", apiKey }, { "nbf", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }, { "exp", DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds() },
        { "video", new Dictionary<string, object> { { "roomCreate", true }, { "roomAdmin", true }, { "room", roomName } } }
    }, Encoding.UTF8.GetBytes(apiSecret), JwsAlgorithm.HS256);
    
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    try { 
        var resp = await client.PostAsync($"{Environment.GetEnvironmentVariable("LIVEKIT_URL") ?? "http://livekit:7880"}/twirp/livekit.RoomService/DeleteRoom", new StringContent($"{{\"room\":\"{roomName}\"}}", Encoding.UTF8, "application/json")); 
        var body = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"Twirp DeleteRoom Response: {resp.StatusCode} - {body}");
    } catch (Exception e) {
        Console.WriteLine($"Twirp Error: {e.Message}");
    }
    
    return Results.Ok();
});

app.MapGet("/api/agents", async (AppDbContext db) => {
    var agents = await db.Agents.Select(a => new { a.Id, a.Username, a.IsOnline }).ToListAsync();
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

public class LoginDto { public required string Username { get; set; } public required string Password { get; set; } }
public class TransferDto { public required string RoomName { get; set; } public string? AgentId { get; set; } }
public class SummaryDto { public required string RoomName { get; set; } public required string Summary { get; set; } }
