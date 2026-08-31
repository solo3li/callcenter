using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Scalar.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using StackExchange.Redis;
using FluentValidation;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using backend.Data;
using backend.Hubs;
using backend.Modules.Identity.Dtos;
using backend.Modules.Billing.Dtos;
using backend.Modules.CallOperations.Dtos;
using backend.Modules.Configuration.Dtos;
using backend.Modules.Analytics.Dtos;
using backend.Services;
using backend.Modules.Identity.Endpoints;
using backend.Modules.Billing.Endpoints;
using backend.Modules.CallOperations.Endpoints;
using backend.Modules.Configuration.Endpoints;
using backend.Modules.Analytics.Endpoints;
using backend.Infrastructure.Endpoints;
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

var translator = new Npgsql.NameTranslation.NpgsqlSnakeCaseNameTranslator();
#pragma warning disable CS0618
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.UserStatus>("user_status", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.PartnerRelationshipStatus>("partner_relationship_status", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.ApiKeyStatus>("api_key_status", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.HumanAgentStatus>("human_agent_status", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.AccessKeyStatus>("access_key_status", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.CallSessionStatus>("call_session_status", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.CallDirection>("call_direction", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.ParticipantType>("participant_type", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.CallTransferStatus>("call_transfer_status", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.HandoffStatus>("handoff_status", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.LicenseStatus>("license_status", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.PlanTier>("plan_tier", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.RecordingStatus>("recording_status", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.MetricType>("metric_type", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.ActionType>("action_type", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.ActionExecutionStatus>("action_execution_status", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.WorkflowExecutionStatus>("workflow_execution_status", translator);
NpgsqlConnection.GlobalTypeMapper.MapEnum<backend.Models.Enums.SubscriptionStatus>("subscription_status", translator);
#pragma warning restore CS0618

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<backend.Models.Enums.UserStatus>("user_status", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.PartnerRelationshipStatus>("partner_relationship_status", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.ApiKeyStatus>("api_key_status", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.HumanAgentStatus>("human_agent_status", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.AccessKeyStatus>("access_key_status", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.CallSessionStatus>("call_session_status", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.CallDirection>("call_direction", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.ParticipantType>("participant_type", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.CallTransferStatus>("call_transfer_status", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.HandoffStatus>("handoff_status", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.LicenseStatus>("license_status", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.PlanTier>("plan_tier", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.RecordingStatus>("recording_status", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.MetricType>("metric_type", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.ActionType>("action_type", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.ActionExecutionStatus>("action_execution_status", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.WorkflowExecutionStatus>("workflow_execution_status", translator);
dataSourceBuilder.MapEnum<backend.Models.Enums.SubscriptionStatus>("subscription_status", translator);
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource, npgsqlOptions => {
        npgsqlOptions.UseVector();
        npgsqlOptions.EnableRetryOnFailure(3);
    }).UseSnakeCaseNamingConvention());

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
builder.Services.AddSingleton<backend.Modules.Identity.Services.TokenService>();
builder.Services.AddScoped<backend.Modules.Identity.Services.ApiKeyValidationService>();
builder.Services.AddScoped<ActionService>();
builder.Services.AddScoped<CallConfigurationService>();
builder.Services.AddScoped<CallTransferService>();
builder.Services.AddScoped<CallHandoffService>();
builder.Services.AddScoped<InboundRoutingService>();
builder.Services.AddScoped<CallRecordingService>();
builder.Services.AddScoped<LiveKitService>();
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
app.MapScalarApiReference(options =>
{
    options.WithTitle("AI Calling Platform API");
    options.WithTheme(Scalar.AspNetCore.ScalarTheme.DeepSpace);
    options.WithDefaultHttpClient(Scalar.AspNetCore.ScalarTarget.CSharp, Scalar.AspNetCore.ScalarClient.HttpClient);
    options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
});

app.UseHangfireDashboard("/hangfire");

// RecurringJob.AddOrUpdate<QueueBroadcaster>(
//     "broadcast-queue-stats",
//     j => j.BroadcastAsync(),
//     "*/3 * * * * *",
//     new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
// );
// 
// RecurringJob.AddOrUpdate<TransferTimeoutProcessor>(
//     "transfer-timeout",
//     j => j.ProcessTimeoutsAsync(),
//     "*/10 * * * * *",
//     new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
// );

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
app.MapLegacyShimsEndpoints();
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


