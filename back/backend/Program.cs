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

// ── Redis ─────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "redis:6379"));
builder.Services.AddScoped<RedisPresenceService>();

// ── FluentValidation ──────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// ── Rate Limiting ─────────────────────────────────────────────────────────
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

// ── Swagger ───────────────────────────────────────────────────────────────
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

// ── Database Initialization ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await RunSqlMigrationAsync(db, connectionString);

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

// ── Middleware Pipeline ──────────────────────────────────────────────────
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

// ── Map All Endpoint Groups (136 endpoints) ─────────────────────────────
app.MapAuthEndpoints();
app.MapApiKeyEndpoints();
app.MapHumanAgentEndpoints();
app.MapPersonaEndpoints();
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

// ── Legacy AI Worker Compat Shims ────────────────────────────────────────
app.MapPost("/api/call/transfer", async (
    TransferShimDto req, CallSessionService sessionService,
    CallTransferService transferService, AppDbContext db) =>
{
    var session = await db.CallSessions
        .FirstOrDefaultAsync(c => c.LivekitRoomName == req.RoomName);
    if (session == null)
        return Results.BadRequest(new { error = "Call session not found for room" });
    try
    {
        var result = await transferService.InitiateTransferAsync(
            session.Id, session.UserId, null);
        return Results.Ok(new { transferId = result!.Transfer.Id, agentName = result.Transfer.ToHumanAgentName });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/call/active", async (
    TransferShimDto req, CallSessionService sessionService,
    LiveKitService liveKit, AppDbContext db) =>
{
    var existing = await db.CallSessions
        .FirstOrDefaultAsync(c => c.LivekitRoomName == req.RoomName);
    if (existing == null)
    {
        await sessionService.CreateAsync(
            Guid.NewGuid(), null, null,
            req.RoomName, "Inbound");
    }
    return Results.Ok();
});

app.MapPost("/api/call/end", async (
    TransferShimDto req, CallSessionService sessionService, AppDbContext db) =>
{
    var session = await db.CallSessions
        .FirstOrDefaultAsync(c => c.LivekitRoomName == req.RoomName);
    if (session != null)
    {
        await sessionService.EndCallAsync(session.Id, session.UserId);
    }
    return Results.Ok();
});

app.MapPost("/api/call/summary", async (
    SummaryShimDto req, CallHandoffService handoffService, AppDbContext db) =>
{
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

// ── Legacy backward-compat endpoints ────────────────────────────────────
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

// ── Migration Runner ────────────────────────────────────────────────────
static async Task RunSqlMigrationAsync(AppDbContext db, string connectionString)
{
    var migrationSqlPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..",
        "Migrations", "Sql", "001_initial_schema.sql");

    var pathsToTry = new[]
    {
        migrationSqlPath,
        Path.Combine(Directory.GetCurrentDirectory(), "Migrations", "Sql", "001_initial_schema.sql"),
        "Migrations/Sql/001_initial_schema.sql"
    };

    string? sql = null;
    foreach (var p in pathsToTry)
    {
        if (File.Exists(p)) { sql = await File.ReadAllTextAsync(p); break; }
    }

    if (sql == null)
    {
        Console.WriteLine("[MIGRATION] SQL migration file not found. Using EnsureCreated fallback.");
        await db.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS vector;");
        db.Database.EnsureCreated();
        return;
    }

    try
    {
        await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync(sql);
        Console.WriteLine("[MIGRATION] Schema created successfully.");
    }
    catch (PostgresException ex) when (ex.SqlState is "42710" or "42P07")
    {
        Console.WriteLine($"[MIGRATION] Some objects already exist (idempotent run): {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[MIGRATION] SQL migration failed ({ex.Message}), falling back to EnsureCreated...");
        db.Database.EnsureCreated();
    }
}

// ── Hangfire Queue Broadcaster ──────────────────────────────────────────
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

public class TransferShimDto { public required string RoomName { get; set; } public string? AgentId { get; set; } }
public class SummaryShimDto { public required string RoomName { get; set; } public required string Summary { get; set; } }

// ── Transfer Timeout Processor ──────────────────────────────────────────
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
            transfer.FailureReason = "Transfer timed out — agent did not respond";
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