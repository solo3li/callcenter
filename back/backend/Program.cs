using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Hangfire;
using Hangfire.PostgreSql;
using StackExchange.Redis;
using FluentValidation;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using backend.Data;
using backend.Hubs;
using backend.Dtos;
using backend.Services;
using backend.Endpoints;
using backend.Middleware;
using backend.Validators;
using backend.Models.Enums;
using backend.Models.Domain;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Npgsql;
using Pgvector.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    var required = new[] { "JWT_SECRET", "LIVEKIT_API_KEY", "LIVEKIT_API_SECRET" };
    foreach (var key in required)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
        {
            Console.WriteLine($"WARNING: Required environment variable '{key}' is not set!");
        }
    }
}

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            var origins = (Environment.GetEnvironmentVariable("CORS_ORIGINS") ?? "https://app.example.com").Split(',');
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=db;Port=5432;Database=callcenter;Username=admin;Password=adminpassword";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions => {
        npgsqlOptions.UseVector();
        npgsqlOptions.EnableRetryOnFailure(3);
    }));

builder.Services.AddSignalR();

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer(options =>
    options.SchedulePollingInterval = TimeSpan.FromSeconds(2));

builder.Services.AddHttpContextAccessor();

// ── MediatR ──────────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// â”€â”€ Redis â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "redis:6379"));
builder.Services.AddScoped<RedisPresenceService>();

// â”€â”€ FluentValidation â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// â”€â”€ Rate Limiting â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
    });
    options.RejectionStatusCode = 429;
});

// â”€â”€ Swagger â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AI Calling Platform API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddScoped<EmbeddingService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApiKeyService>();
builder.Services.AddScoped<HumanAgentService>();
builder.Services.AddScoped<PersonaService>();
builder.Services.AddScoped<ActionService>();
builder.Services.AddScoped<CallConfigurationService>();
builder.Services.AddScoped<CallSessionService>();
builder.Services.AddScoped<CallTransferService>();
builder.Services.AddScoped<CallHandoffService>();
builder.Services.AddScoped<InboundRoutingService>();
builder.Services.AddScoped<CallRecordingService>();
builder.Services.AddScoped<LiveKitService>();
builder.Services.AddScoped<UsageService>();
builder.Services.AddScoped<PlanService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<LicenseService>();
builder.Services.AddScoped<PartnerService>();
builder.Services.AddScoped<WorkflowService>();
builder.Services.AddScoped<KnowledgeBaseService>();
builder.Services.AddScoped<StatsService>();
builder.Services.AddSingleton<StorageService>();

var app = builder.Build();

// â”€â”€ Database Initialization â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await backend.Data.DbPatchRunner.RunAsync(db);

    var onlineAgents = db.Agents.Where(a => a.IsOnline).ToList();
    foreach (var a in onlineAgents) { a.IsOnline = false; a.Status = "Offline"; }
    db.SaveChanges();

    if (!db.Agents.Any())
    {
        db.Agents.Add(new backend.Models.AgentUser
        {
            Username = "admin", PasswordHash = "admin",
            IsOnline = false, Status = "Offline"
        });
        db.SaveChanges();
    }
}

// â”€â”€ Middleware Pipeline â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
app.UseCors();
app.UseRateLimiter();
app.UseDefaultFiles();
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".ogg"] = "audio/ogg";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

app.UseAuthMiddleware();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Calling Platform API v1"));

app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<QueueBroadcaster>(
    "broadcast-queue-stats",
    j => j.BroadcastAsync(),
    "*/3 * * * * *",
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
);

RecurringJob.AddOrUpdate<TransferTimeoutProcessor>(
    "transfer-timeout",
    j => j.ProcessTimeoutsAsync(),
    "*/10 * * * * *",
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
);

// â”€â”€ Map All Endpoint Groups (136 endpoints) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
app.MapAuthEndpoints();
app.MapApiKeyEndpoints();
app.MapHumanAgentEndpoints();
app.MapPersonaEndpoints();
app.MapSipDestinationEndpoints();
app.MapActionEndpoints();
app.MapCallConfigurationEndpoints();
app.MapCallSessionEndpoints();
app.MapCallTransferEndpoints();
app.MapCallHandoffEndpoints();
app.MapCallRecordingEndpoints();
app.MapLiveKitEndpoints();
app.MapUsageEndpoints();
app.MapPlanEndpoints();
app.MapSubscriptionEndpoints();
app.MapLicenseEndpoints();
app.MapPartnerEndpoints();
app.MapWorkflowEndpoints();
app.MapKnowledgeBaseEndpoints();
app.MapStatsEndpoints();
app.MapWebhookEndpoints();
app.MapLiveKitWebhookEndpoints();

// â”€â”€ Legacy AI Worker Compat Shims â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
app.MapPost("/api/call/transfer", async (
    TransferShimDto req, HttpContext http, CallSessionService sessionService,
    CallTransferService transferService, AppDbContext db) =>
{
    if (!backend.Middleware.ServiceAuth.IsConfiguredOrValid(http))
        return Results.Unauthorized();

    var session = await db.CallSessions
        .FirstOrDefaultAsync(c => c.LivekitRoomName == req.RoomName);
    if (session == null)
        return Results.BadRequest(new { error = "Call session not found for room" });
    try
    {
        var targetType = req.TargetType?.Trim().ToLowerInvariant();

        if (targetType == "destination")
        {
            if (string.IsNullOrWhiteSpace(req.TargetName))
                return Results.BadRequest(new { error = "TargetName is required for destination transfers" });

            var destResult = await transferService.InitiateDestinationTransferAsync(
                session.Id, session.UserId, req.TargetName!, req.Reason);
            return destResult == null
                ? Results.NotFound(new { error = "Call session not found" })
                : Results.Ok(new { transferId = destResult.Id, status = destResult.Status });
        }

        // Human transfer (named agent when provided, otherwise best available).
        Guid? preferredId = Guid.TryParse(req.AgentId, out var aid) ? aid : null;
        var result = await transferService.InitiateTransferAsync(
            session.Id, session.UserId, req.Reason,
            preferredAgentId: preferredId,
            preferredAgentName: req.TargetName);

        return Results.Ok(new
        {
            transferId = result!.Transfer.Id,
            agentName = result.Transfer.ToHumanAgentName,
            status = result.Transfer.Status
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/call/active", async (
    TransferShimDto req, HttpContext http, CallSessionService sessionService,
    LiveKitService liveKit, AppDbContext db) =>
{
    if (!backend.Middleware.ServiceAuth.IsConfiguredOrValid(http))
        return Results.Unauthorized();

    var existing = await db.CallSessions
        .FirstOrDefaultAsync(c => c.LivekitRoomName == req.RoomName);
    if (existing == null && !req.RoomName.StartsWith(InboundRoutingService.RoomOwnerPrefix))
    {
        // Platform-managed SIP rooms are created by the webhook path; only
        // legacy ad-hoc rooms are registered here.
        await sessionService.CreateAsync(
            Guid.NewGuid(), null, null,
            req.RoomName, "Inbound");
    }
    return Results.Ok();
});

app.MapPost("/api/call/end", async (
    TransferShimDto req, HttpContext http, CallSessionService sessionService, AppDbContext db) =>
{
    if (!backend.Middleware.ServiceAuth.IsConfiguredOrValid(http))
        return Results.Unauthorized();

    var session = await db.CallSessions
        .FirstOrDefaultAsync(c => c.LivekitRoomName == req.RoomName);
    if (session != null)
    {
        await sessionService.EndCallAsync(session.Id, session.UserId);
    }
    return Results.Ok();
});

app.MapPost("/api/call/summary", async (
    SummaryShimDto req, HttpContext http, CallHandoffService handoffService, AppDbContext db) =>
{
    if (!backend.Middleware.ServiceAuth.IsConfiguredOrValid(http))
        return Results.Unauthorized();

    var session = await db.CallSessions
        .FirstOrDefaultAsync(c => c.LivekitRoomName == req.RoomName);
    if (session == null) return Results.Ok();
    var handoff = await db.CallHandoffs
        .FirstOrDefaultAsync(h => h.CallSessionId == session.Id);
    if (handoff != null)
    {
        await handoffService.CreateContextAsync(
            handoff.CallTransferId, req.Summary, null, null);
    }
    return Results.Ok();
});

// â”€â”€ Agent-App Transfer Shims (exempt namespace, optional service token) â”€â”€
app.MapGet("/api/call/transfer-options", async (
    string roomName, Guid agentId, HttpContext http, AppDbContext db) =>
{
    if (!backend.Middleware.ServiceAuth.IsConfiguredOrValid(http))
        return Results.Unauthorized();

    var session = await db.CallSessions
        .FirstOrDefaultAsync(c => c.LivekitRoomName == roomName);
    if (session == null ||
        (session.Status != CallSessionStatus.Transferred
            && session.Status != CallSessionStatus.Active))
        return Results.NotFound(new { error = "No active transferred call" });

    var requester = await db.HumanAgents
        .FirstOrDefaultAsync(a => a.Id == agentId && a.IsActive && a.OwnerUserId == session.UserId);
    if (requester == null)
        return Results.Forbid();

    var agents = await db.HumanAgents
        .Where(a => a.OwnerUserId == session.UserId && a.IsActive && a.Id != agentId)
        .OrderBy(a => a.Name)
        .Select(a => new { id = a.Id, name = a.Name, available = a.Status == HumanAgentStatus.Available })
        .ToListAsync();

    var destinations = await db.SipDestinations
        .Where(d => d.UserId == session.UserId && d.IsEnabled)
        .OrderBy(d => d.Name)
        .Select(d => new { id = d.Id, name = d.Name })
        .ToListAsync();

    return Results.Ok(new { agents, destinations });
});

app.MapPost("/api/call/agent-transfer", async (
    AgentTransferShimDto req, HttpContext http, AppDbContext db, CallTransferService transferService) =>
{
    if (!backend.Middleware.ServiceAuth.IsConfiguredOrValid(http))
        return Results.Unauthorized();

    var session = await db.CallSessions
        .FirstOrDefaultAsync(c => c.LivekitRoomName == req.RoomName);
    if (session == null)
        return Results.NotFound(new { error = "Call session not found" });

    try
    {
        var targetType = req.TargetType?.Trim().ToLowerInvariant();
        CallTransferDto? result;

        if (targetType == "destination")
        {
            if (string.IsNullOrWhiteSpace(req.TargetName))
                return Results.BadRequest(new { error = "TargetName is required for destination transfers" });
            result = await transferService.InitiateAgentDestinationTransferAsync(
                session.Id, req.FromAgentId, req.TargetName!, req.Reason);
        }
        else
        {
            result = await transferService.InitiateAgentHumanTransferAsync(
                session.Id, req.FromAgentId, req.TargetName, req.Reason);
        }

        return result == null
            ? Results.BadRequest(new { error = "Requesting agent is not on this call" })
            : Results.Ok(new { transferId = result.Id, targetName = result.ToHumanAgentName, status = result.Status });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/call/transfer-decision", async (
    TransferDecisionShimDto req, HttpContext http, CallTransferService transferService) =>
{
    if (!backend.Middleware.ServiceAuth.IsConfiguredOrValid(http))
        return Results.Unauthorized();

    try
    {
        object? result = req.Decision == "accept"
            ? await transferService.AcceptTransferAsync(req.TransferId, req.HumanAgentId)
            : await transferService.RejectTransferAsync(req.TransferId, req.HumanAgentId);

        return result == null
            ? Results.NotFound(new { error = "Transfer not found for this agent" })
            : Results.Ok(new { status = "ok" });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// â”€â”€ Legacy backward-compat endpoints â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
app.MapGet("/api/token", async (
    string? identity, string? room,
    LiveKitService liveKit, AppDbContext db) =>
{
    identity ??= "web-user-" + Guid.NewGuid().ToString("N")[..8];
    var roomName = room ?? CallSessionService.GenerateRoomName();

    var token = liveKit.GenerateToken(identity, roomName, true, true);

    if (!identity.StartsWith("admin_") && !identity.StartsWith("agent_"))
    {
        var existing = await db.Calls.FirstOrDefaultAsync(c => c.RoomName == roomName);
        if (existing == null)
        {
            db.Calls.Add(new backend.Models.CallRecord
            {
                RoomName = roomName, CallerId = identity,
                Status = "Active", RecordingUrl = $"/recordings/{roomName}.ogg"
            });
            await db.SaveChangesAsync();
        }
    }
    return Results.Json(new { token, url = "ws://127.0.0.1:7880", roomName });
});

app.MapHub<CallHub>("/hubs/call");

app.Run("http://0.0.0.0:5000");

// â”€â”€ Hangfire Queue Broadcaster â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class QueueBroadcaster
{
    private readonly IHubContext<CallHub> _hub;
    private readonly AppDbContext _db;
    public QueueBroadcaster(IHubContext<CallHub> hub, AppDbContext db) { _hub = hub; _db = db; }

    public async Task BroadcastAsync()
    {
        var activeSessions = await _db.CallSessions
            .Where(c => c.Status == CallSessionStatus.Active || c.Status == CallSessionStatus.Transferred)
            .OrderByDescending(c => c.StartedAt)
            .ToListAsync();
        var agents = await _db.HumanAgents
            .Where(a => a.IsActive)
            .Select(a => new { a.Id, a.Name, a.Status })
            .ToListAsync();

        var payload = new {
            activeCount = activeSessions.Count,
            agentsOnline = agents.Count(a => a.Status == HumanAgentStatus.Available),
            activeCalls = activeSessions.Select(c => new {
                id = c.Id.ToString(),
                roomName = c.LivekitRoomName,
                status = c.Status.ToString(),
                startTime = c.StartedAt,
                durationSeconds = (int)(DateTime.UtcNow - c.StartedAt).TotalSeconds
            }),
            agents = agents.Select(a => new {
                id = a.Id.ToString(),
                a.Name,
                Status = a.Status.ToString()
            })
        };

        await _hub.Clients.All.SendAsync("QueueUpdate", payload);
    }
}

public class TransferShimDto
{
    public required string RoomName { get; set; }
    public string? AgentId { get; set; }
    public string? TargetType { get; set; }
    public string? TargetName { get; set; }
    public string? Reason { get; set; }
}
public class AgentTransferShimDto
{
    public required string RoomName { get; set; }
    public required Guid FromAgentId { get; set; }
    public string? TargetType { get; set; }
    public string? TargetName { get; set; }
    public string? Reason { get; set; }
}
public class TransferDecisionShimDto
{
    public required Guid TransferId { get; set; }
    public required Guid HumanAgentId { get; set; }
    public string Decision { get; set; } = "accept";
}
public class SummaryShimDto { public required string RoomName { get; set; } public required string Summary { get; set; } }

// â”€â”€ Transfer Timeout Processor â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class TransferTimeoutProcessor
{
    private readonly AppDbContext _db;
    private readonly IHubContext<CallHub> _hub;
    public TransferTimeoutProcessor(AppDbContext db, IHubContext<CallHub> hub) { _db = db; _hub = hub; }

    public async Task ProcessTimeoutsAsync()
    {
        var timeoutThreshold = DateTime.UtcNow.AddSeconds(-30);
        var staleTransfers = await _db.CallTransfers
            .Include(t => t.ToHumanAgent)
            .Where(t => t.Status == CallTransferStatus.Requested && t.RequestedAt < timeoutThreshold)
            .ToListAsync();

        foreach (var transfer in staleTransfers)
        {
            var ownerUserId = await _db.HumanAgents
                .Where(a => a.Id == transfer.ToHumanAgentId)
                .Select(a => a.OwnerUserId)
                .FirstOrDefaultAsync();

            transfer.Status = CallTransferStatus.Failed;
            transfer.FailureReason = "Transfer timed out â€” agent did not respond";
            transfer.FailedAt = DateTime.UtcNow;
            transfer.UpdatedAt = DateTime.UtcNow;

            var handoff = await _db.CallHandoffs
                .FirstOrDefaultAsync(h => h.CallTransferId == transfer.Id);
            if (handoff != null)
                handoff.Status = HandoffStatus.Expired;

            await _db.SaveChangesAsync();

            var session = await _db.CallSessions.FindAsync(transfer.CallSessionId);

            var availableAgent = await _db.HumanAgents
                .Where(a => a.OwnerUserId == ownerUserId
                    && a.IsActive
                    && a.Status == HumanAgentStatus.Available
                    && a.Id != transfer.ToHumanAgentId)
                .FirstOrDefaultAsync();

            if (availableAgent != null)
            {
                var newTransfer = new CallTransfer
                {
                    Id = Guid.NewGuid(),
                    CallSessionId = transfer.CallSessionId,
                    ToHumanAgentId = availableAgent.Id,
                    Mode = transfer.Mode,
                    TargetType = transfer.TargetType,
                    TargetSnapshotJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        fromIdentity = backend.Services.InboundRoutingService.ExtractFromIdentity(
                            transfer.TargetSnapshotJson),
                        agentId = availableAgent.Id,
                        name = availableAgent.Name
                    }),
                    Status = CallTransferStatus.Requested,
                    Reason = transfer.Reason,
                    RequestedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.CallTransfers.Add(newTransfer);

                var newHandoff = new CallHandoff
                {
                    Id = Guid.NewGuid(),
                    CallSessionId = transfer.CallSessionId,
                    CallTransferId = newTransfer.Id,
                    ToHumanAgentId = availableAgent.Id,
                    Status = HandoffStatus.Pending,
                    Reason = transfer.Reason,
                    CreatedAt = DateTime.UtcNow
                };
                _db.CallHandoffs.Add(newHandoff);
                await _db.SaveChangesAsync();

                await _hub.Clients.Group($"agent_{availableAgent.Id}")
                    .SendAsync("IncomingTransfer", new
                    {
                        transferId = newTransfer.Id,
                        handoffId = newHandoff.Id,
                        callSessionId = transfer.CallSessionId,
                        roomName = session?.LivekitRoomName,
                        toHumanAgentId = availableAgent.Id,
                        reason = transfer.Reason
                    });
            }
        }
    }
}


