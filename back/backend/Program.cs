using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Hangfire;
using Hangfire.PostgreSql;
using backend.Data;
using backend.Hubs;
using backend.Services;
using backend.Endpoints;
using backend.Middleware;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=db;Port=5432;Database=callcenter;Username=admin;Password=adminpassword";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions => {
        npgsqlOptions.EnableRetryOnFailure(3);
    }));

builder.Services.AddSignalR();

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();

builder.Services.AddHttpContextAccessor();

// ── Register All Services ───────────────────────────────────────────────
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
app.UseDefaultFiles();
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".ogg"] = "audio/ogg";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

app.UseAuthMiddleware();

app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<QueueBroadcaster>(
    "broadcast-queue-stats",
    j => j.BroadcastAsync(),
    "*/3 * * * * *",
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

// ── Legacy backward-compat endpoints ────────────────────────────────────
app.MapGet("/api/token", async (
    string? identity, string? room,
    LiveKitService liveKit, CallSessionService callSession,
    CallRecordingService recording, AppDbContext db) =>
{
    identity ??= "web-user-" + Guid.NewGuid().ToString("N")[..8];
    var roomName = room ?? callSession.GenerateRoomName();

    var result = liveKit.GenerateToken(identity, roomName, true, true);

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
    return Results.Json(new { token = result.Token, url = result.Url, roomName });
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
        var activeCalls = await _db.Calls
            .Where(c => c.Status == "Active" || c.Status == "Transferred")
            .ToListAsync();
        var agents = await _db.Agents
            .Select(a => new { a.Id, a.Username, a.IsOnline, a.Status }).ToListAsync();
        var agentCalls = await _db.Calls
            .Where(c => c.Status == "Transferred" && c.HandledByAgentId != null).ToListAsync();

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