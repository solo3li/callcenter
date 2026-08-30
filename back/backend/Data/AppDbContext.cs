using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Models.Domain;
using backend.Models.Enums;
using backend.Modules.Billing.Models;
using backend.Modules.Configuration.Models;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Legacy/MVP tables (kept for backward compatibility with existing endpoints)
    public DbSet<AgentUser> Agents { get; set; } = null!;
    public DbSet<CallRecord> Calls { get; set; } = null!;

    // New multi-tenant domain tables
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Partner> Partners { get; set; } = null!;
    public DbSet<PartnerRelationship> PartnerRelationships { get; set; } = null!;
    public DbSet<PartnerExternalCustomer> PartnerExternalCustomers { get; set; } = null!;
    public DbSet<PartnerPlan> PartnerPlans { get; set; } = null!;
    public DbSet<ApiKey> ApiKeys { get; set; } = null!;
    public DbSet<HumanAgent> HumanAgents { get; set; } = null!;
    public DbSet<HumanAgentAccessKey> HumanAgentAccessKeys { get; set; } = null!;
    public DbSet<HumanAgentSession> HumanAgentSessions { get; set; } = null!;
    public DbSet<Persona> Personas { get; set; } = null!;
    public DbSet<PersonaVersion> PersonaVersions { get; set; } = null!;
    public DbSet<PersonaAction> PersonaActions { get; set; } = null!;
    public DbSet<Workflow> Workflows { get; set; } = null!;
    public DbSet<WorkflowVersion> WorkflowVersions { get; set; } = null!;
    public DbSet<WorkflowExecution> WorkflowExecutions { get; set; } = null!;
    public DbSet<ActionDefinition> ActionDefinitions { get; set; } = null!;
    public DbSet<ActionExecution> ActionExecutions { get; set; } = null!;
    public DbSet<CallConfiguration> CallConfigurations { get; set; } = null!;
    public DbSet<CallConfigurationAction> CallConfigurationActions { get; set; } = null!;
    public DbSet<CallSession> CallSessions { get; set; } = null!;
    public DbSet<CallLeg> CallLegs { get; set; } = null!;
    public DbSet<SipConnection> SipConnections { get; set; } = null!;
    public DbSet<SipDestination> SipDestinations { get; set; } = null!;
    public DbSet<CallParticipant> CallParticipants { get; set; } = null!;
    public DbSet<CallTransfer> CallTransfers { get; set; } = null!;
    public DbSet<CallHandoff> CallHandoffs { get; set; } = null!;
    public DbSet<CallRecording> CallRecordings { get; set; } = null!;
    public DbSet<Plan> Plans { get; set; } = null!;
    public DbSet<Subscription> Subscriptions { get; set; } = null!;
    public DbSet<License> Licenses { get; set; } = null!;
    public DbSet<UsageRecord> UsageRecords { get; set; } = null!;
    public DbSet<KnowledgeBase> KnowledgeBases { get; set; } = null!;
    public DbSet<KnowledgeDocument> KnowledgeDocuments { get; set; } = null!;
    public DbSet<KnowledgeChunk> KnowledgeChunks { get; set; } = null!;
    public DbSet<PersonaKnowledgeBase> PersonaKnowledgeBases { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── PostgreSQL Enums ───────────────────────────────────────────
        modelBuilder.HasPostgresEnum<UserStatus>();
        modelBuilder.HasPostgresEnum<PartnerRelationshipStatus>();
        modelBuilder.HasPostgresEnum<ApiKeyStatus>();
        modelBuilder.HasPostgresEnum<HumanAgentStatus>();
        modelBuilder.HasPostgresEnum<AccessKeyStatus>();
        modelBuilder.HasPostgresEnum<CallSessionStatus>();
        modelBuilder.HasPostgresEnum<CallDirection>();
        modelBuilder.HasPostgresEnum<ParticipantType>();
        modelBuilder.HasPostgresEnum<CallTransferStatus>();
        modelBuilder.HasPostgresEnum<HandoffStatus>();
        modelBuilder.HasPostgresEnum<LicenseStatus>();
        modelBuilder.HasPostgresEnum<PlanTier>();
        modelBuilder.HasPostgresEnum<RecordingStatus>();
        modelBuilder.HasPostgresEnum<MetricType>();
        modelBuilder.HasPostgresEnum<ActionType>();
        modelBuilder.HasPostgresEnum<ActionExecutionStatus>();
        modelBuilder.HasPostgresEnum<WorkflowExecutionStatus>();
        modelBuilder.HasPostgresEnum<SubscriptionStatus>();

        // ── Legacy Tables (keep existing names/compat) ─────────────────
        modelBuilder.Entity<AgentUser>(entity =>
        {
            entity.ToTable("agent_users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.IsOnline);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<CallRecord>(entity =>
        {
            entity.ToTable("call_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoomName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.CallerId).HasMaxLength(256);
            entity.Property(e => e.StartTime).HasColumnType("timestamptz");
            entity.Property(e => e.EndTime).HasColumnType("timestamptz");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne(e => e.HandledByAgent).WithMany().HasForeignKey(e => e.HandledByAgentId);
            entity.HasIndex(e => e.RoomName);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartTime);
        });

        // ── USERS ─────────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", "identity");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(320);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.CompanyName).HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.StandardCredits).HasColumnType("numeric(18,4)").HasDefaultValue(0);
            entity.Property(e => e.PremiumCredits).HasColumnType("numeric(18,4)").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.DefaultPersona)
                .WithMany()
                .HasForeignKey(e => e.DefaultPersonaId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.DefaultPersonaId);
        });

        // ── SIP CONNECTIONS (inbound customer trunks) ─────────────────
        modelBuilder.Entity<SipConnection>(entity =>
        {
            entity.ToTable("sip_connections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(128);
            entity.Property(e => e.AllowedIps).HasColumnType("text[]");
            entity.Property(e => e.Numbers).HasColumnType("text[]");
            entity.Property(e => e.LkTrunkId).HasMaxLength(64);
            entity.Property(e => e.DispatchRuleId).HasMaxLength(64);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();
            entity.HasIndex(e => e.LkTrunkId).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        // ── SIP DESTINATIONS (named external PBX targets) ─────────────
        modelBuilder.Entity<SipDestination>(entity =>
        {
            entity.ToTable("sip_destinations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Description).HasMaxLength(256);
            entity.Property(e => e.CallTo).IsRequired().HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();
            entity.HasIndex(e => e.IsEnabled);
        });

        // ── PARTNERS ──────────────────────────────────────────────────
        modelBuilder.Entity<Partner>(entity =>
        {
            entity.ToTable("partners", "identity");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrganizationName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ContactEmail).HasMaxLength(320);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Website).HasMaxLength(512);
            entity.Property(e => e.MetadataJson).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.User)
                .WithOne(u => u.Partner)
                .HasForeignKey<Partner>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.OrganizationName);
        });

        // ── PARTNER RELATIONSHIPS ─────────────────────────────────────
        modelBuilder.Entity<PartnerRelationship>(entity =>
        {
            entity.ToTable("partner_relationships", "identity");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.MetadataJson).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.Partner)
                .WithMany(p => p.CustomerRelationships)
                .HasForeignKey(e => e.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CustomerUser)
                .WithMany()
                .HasForeignKey(e => e.CustomerUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PartnerId);
            entity.HasIndex(e => e.CustomerUserId);
            entity.HasIndex(e => new { e.PartnerId, e.CustomerUserId }).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // ── PARTNER EXTERNAL CUSTOMERS ────────────────────────────────
        modelBuilder.Entity<PartnerExternalCustomer>(entity =>
        {
            entity.ToTable("partner_external_customers", "identity");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExternalCustomerId).IsRequired().HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.Partner)
                .WithMany()
                .HasForeignKey(e => e.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PlatformUser)
                .WithMany()
                .HasForeignKey(e => e.PlatformUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.PartnerId, e.ExternalCustomerId }).IsUnique();
            entity.HasIndex(e => e.PlatformUserId);
        });

        // ── API KEYS ──────────────────────────────────────────────────
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.ToTable("api_keys", "identity");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.KeyPrefix).IsRequired().HasMaxLength(16);
            entity.Property(e => e.KeyHash).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Scopes).HasColumnType("text[]");
            entity.Property(e => e.LastUsedAt).HasColumnType("timestamptz");
            entity.Property(e => e.ExpiresAt).HasColumnType("timestamptz");
            entity.Property(e => e.RevokedAt).HasColumnType("timestamptz");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.User)
                .WithMany(u => u.ApiKeys)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.KeyHash).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // ── HUMAN AGENTS ──────────────────────────────────────────────
        modelBuilder.Entity<HumanAgent>(entity =>
        {
            entity.ToTable("human_agents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.MaxConcurrentCalls).HasDefaultValue(1);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.OwnerUser)
                .WithMany(u => u.HumanAgents)
                .HasForeignKey(e => e.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ApplicationUser)
                .WithMany()
                .HasForeignKey(e => e.ApplicationUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.OwnerUserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsActive);
        });

        // ── HUMAN AGENT ACCESS KEYS ───────────────────────────────────
        modelBuilder.Entity<HumanAgentAccessKey>(entity =>
        {
            entity.ToTable("human_agent_access_keys");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.KeyPrefix).IsRequired().HasMaxLength(16);
            entity.Property(e => e.KeyHash).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.ExpiresAt).HasColumnType("timestamptz");
            entity.Property(e => e.LastUsedAt).HasColumnType("timestamptz");
            entity.Property(e => e.RevokedAt).HasColumnType("timestamptz");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.HumanAgent)
                .WithMany(a => a.AccessKeys)
                .HasForeignKey(e => e.HumanAgentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.HumanAgentId);
            entity.HasIndex(e => e.KeyHash).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // ── HUMAN AGENT SESSIONS ──────────────────────────────────────
        modelBuilder.Entity<HumanAgentSession>(entity =>
        {
            entity.ToTable("human_agent_sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LivekitIdentity).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ConnectedAt).HasColumnType("timestamptz");
            entity.Property(e => e.DisconnectedAt).HasColumnType("timestamptz");
            entity.Property(e => e.LastHeartbeatAt).HasColumnType("timestamptz");
            entity.Property(e => e.MetadataJson).HasColumnType("jsonb");

            entity.HasOne(e => e.HumanAgent)
                .WithMany(a => a.Sessions)
                .HasForeignKey(e => e.HumanAgentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.HumanAgentId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ConnectedAt);
        });

        // ── PERSONAS ──────────────────────────────────────────────────
        modelBuilder.Entity<Persona>(entity =>
        {
            entity.ToTable("personas", "configuration");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Personas)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsActive);
        });

        // ── PERSONA VERSIONS ──────────────────────────────────────────
        modelBuilder.Entity<PersonaVersion>(entity =>
        {
            entity.ToTable("persona_versions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VersionNumber).IsRequired();
            entity.Property(e => e.SystemPrompt).IsRequired().HasColumnType("text");
            entity.Property(e => e.ConfigurationJson).IsRequired().HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.Persona)
                .WithMany(p => p.Versions)
                .HasForeignKey(e => e.PersonaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PersonaId);
            entity.HasIndex(e => new { e.PersonaId, e.VersionNumber }).IsUnique();
            entity.HasIndex(e => e.IsPublished);
        });

        // ── PERSONA ACTIONS (Junction) ────────────────────────────────
        modelBuilder.Entity<PersonaAction>(entity =>
        {
            entity.ToTable("persona_actions");
            entity.HasKey(e => new { e.PersonaId, e.ActionDefinitionId });
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.Persona)
                .WithMany(p => p.PersonaActions)
                .HasForeignKey(e => e.PersonaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ActionDefinition)
                .WithMany(a => a.PersonaActions)
                .HasForeignKey(e => e.ActionDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── WORKFLOWS ─────────────────────────────────────────────────
        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.ToTable("workflows", "configuration");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Workflows)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsActive);
        });

        // ── WORKFLOW VERSIONS ─────────────────────────────────────────
        modelBuilder.Entity<WorkflowVersion>(entity =>
        {
            entity.ToTable("workflow_versions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VersionNumber).IsRequired();
            entity.Property(e => e.DefinitionJson).IsRequired().HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.Workflow)
                .WithMany(w => w.Versions)
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.WorkflowId);
            entity.HasIndex(e => new { e.WorkflowId, e.VersionNumber }).IsUnique();
            entity.HasIndex(e => e.IsPublished);
        });

        // ── WORKFLOW EXECUTIONS ───────────────────────────────────────
        modelBuilder.Entity<WorkflowExecution>(entity =>
        {
            entity.ToTable("workflow_executions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.InputJson).HasColumnType("jsonb");
            entity.Property(e => e.OutputJson).HasColumnType("jsonb");
            entity.Property(e => e.StateJson).HasColumnType("jsonb");
            entity.Property(e => e.StartedAt).HasColumnType("timestamptz");
            entity.Property(e => e.CompletedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.WorkflowVersion)
                .WithMany()
                .HasForeignKey(e => e.WorkflowVersionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CallSession)
                .WithMany(c => c.WorkflowExecutions)
                .HasForeignKey(e => e.CallSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.WorkflowVersionId);
            entity.HasIndex(e => e.CallSessionId);
            entity.HasIndex(e => e.Status);
        });

        // ── ACTION DEFINITIONS ────────────────────────────────────────
        modelBuilder.Entity<ActionDefinition>(entity =>
        {
            entity.ToTable("action_definitions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ActionType).IsRequired();
            entity.Property(e => e.InputSchemaJson).HasColumnType("jsonb");
            entity.Property(e => e.OutputSchemaJson).HasColumnType("jsonb");
            entity.Property(e => e.ConfigurationJson).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.ActionType);
            entity.HasIndex(e => e.IsActive);

            entity.HasData(
                new ActionDefinition
                {
                    Id = new Guid("a0000000-0000-0000-0000-000000000001"),
                    Name = "transfer_to_human",
                    DisplayName = "Transfer to Human",
                    Description = "Transfers the call to an available human agent within the same LiveKit room.",
                    ActionType = ActionType.System,
                    IsSystem = true,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ActionDefinition
                {
                    Id = new Guid("a0000000-0000-0000-0000-000000000002"),
                    Name = "end_call",
                    DisplayName = "End Call",
                    Description = "Ends the current call session.",
                    ActionType = ActionType.System,
                    IsSystem = true,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        });

        // ── ACTION EXECUTIONS ─────────────────────────────────────────
        modelBuilder.Entity<ActionExecution>(entity =>
        {
            entity.ToTable("action_executions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.InputJson).HasColumnType("jsonb");
            entity.Property(e => e.OutputJson).HasColumnType("jsonb");
            entity.Property(e => e.StartedAt).HasColumnType("timestamptz");
            entity.Property(e => e.CompletedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.CallSession)
                .WithMany(c => c.ActionExecutions)
                .HasForeignKey(e => e.CallSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ActionDefinition)
                .WithMany(a => a.ActionExecutions)
                .HasForeignKey(e => e.ActionDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WorkflowExecution)
                .WithMany()
                .HasForeignKey(e => e.WorkflowExecutionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.CallSessionId);
            entity.HasIndex(e => e.ActionDefinitionId);
            entity.HasIndex(e => e.Status);
        });

        // ── CALL CONFIGURATIONS ───────────────────────────────────────
        modelBuilder.Entity<CallConfiguration>(entity =>
        {
            entity.ToTable("call_configurations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ConfigJson).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.User)
                .WithMany(u => u.CallConfigurations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Persona)
                .WithMany()
                .HasForeignKey(e => e.PersonaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Workflow)
                .WithMany()
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsActive);
        });

        // ── CALL CONFIGURATION ACTIONS (Junction) ─────────────────────
        modelBuilder.Entity<CallConfigurationAction>(entity =>
        {
            entity.ToTable("call_configuration_actions");
            entity.HasKey(e => new { e.CallConfigurationId, e.ActionDefinitionId });
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.CallConfiguration)
                .WithMany(c => c.CallConfigurationActions)
                .HasForeignKey(e => e.CallConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ActionDefinition)
                .WithMany(a => a.CallConfigurationActions)
                .HasForeignKey(e => e.ActionDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── CALL SESSIONS ─────────────────────────────────────────────
        modelBuilder.Entity<CallSession>(entity =>
        {
            entity.ToTable("call_sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LivekitRoomName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.LivekitRoomSid).HasMaxLength(256);
            entity.Property(e => e.DialedNumber).HasMaxLength(32);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Direction).IsRequired();
            entity.Property(e => e.StartedAt).HasColumnType("timestamptz");
            entity.Property(e => e.AnsweredAt).HasColumnType("timestamptz");
            entity.Property(e => e.EndedAt).HasColumnType("timestamptz");
            entity.Property(e => e.MetadataJson).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.User)
                .WithMany(u => u.CallSessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CallConfiguration)
                .WithMany(c => c.CallSessions)
                .HasForeignKey(e => e.CallConfigurationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PersonaVersion)
                .WithMany()
                .HasForeignKey(e => e.PersonaVersionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.WorkflowVersion)
                .WithMany()
                .HasForeignKey(e => e.WorkflowVersionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ApiKey)
                .WithMany()
                .HasForeignKey(e => e.ApiKeyId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.OriginSipConnection)
                .WithMany()
                .HasForeignKey(e => e.OriginSipConnectionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartedAt);
            entity.HasIndex(e => e.LivekitRoomName);
            entity.HasIndex(e => e.OriginSipConnectionId);
            entity.HasIndex(e => new { e.UserId, e.StartedAt });
        });

        // ── CALL LEGS (ordered media sides of a session) ──────────────
        modelBuilder.Entity<CallLeg>(entity =>
        {
            entity.ToTable("call_legs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ParticipantIdentity).HasMaxLength(256);
            entity.Property(e => e.HangupCause).HasMaxLength(128);
            entity.Property(e => e.Kind).HasConversion<string>().HasColumnType("varchar(32)");
            entity.Property(e => e.StartedAt).HasColumnType("timestamptz");
            entity.Property(e => e.AnsweredAt).HasColumnType("timestamptz");
            entity.Property(e => e.EndedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.CallSession)
                .WithMany(c => c.Legs)
                .HasForeignKey(e => e.CallSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.CallSessionId, e.LegIndex }).IsUnique();
            entity.HasIndex(e => e.CallSessionId);
            entity.HasIndex(e => e.Kind);
        });

        // ── CALL PARTICIPANTS ─────────────────────────────────────────
        modelBuilder.Entity<CallParticipant>(entity =>
        {
            entity.ToTable("call_participants");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ParticipantType).IsRequired();
            entity.Property(e => e.LivekitIdentity).IsRequired().HasMaxLength(256);
            entity.Property(e => e.LivekitParticipantSid).HasMaxLength(256);
            entity.Property(e => e.DisplayName).HasMaxLength(256);
            entity.Property(e => e.JoinedAt).HasColumnType("timestamptz");
            entity.Property(e => e.LeftAt).HasColumnType("timestamptz");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.CallSession)
                .WithMany(c => c.Participants)
                .HasForeignKey(e => e.CallSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.HumanAgent)
                .WithMany(a => a.CallParticipants)
                .HasForeignKey(e => e.HumanAgentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.CallSessionId);
            entity.HasIndex(e => e.HumanAgentId);
            entity.HasIndex(e => e.ParticipantType);
        });

        // ── CALL TRANSFERS ────────────────────────────────────────────
        modelBuilder.Entity<CallTransfer>(entity =>
        {
            entity.ToTable("call_transfers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Mode).HasConversion<string>().HasColumnType("varchar(16)").IsRequired();
            entity.Property(e => e.TargetType).HasConversion<string>().HasColumnType("varchar(32)").IsRequired();
            entity.Property(e => e.TargetSnapshotJson).HasColumnType("jsonb");
            entity.Property(e => e.RequestedAt).HasColumnType("timestamptz");
            entity.Property(e => e.AcceptedAt).HasColumnType("timestamptz");
            entity.Property(e => e.CompletedAt).HasColumnType("timestamptz");
            entity.Property(e => e.FailedAt).HasColumnType("timestamptz");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.CallSession)
                .WithMany(c => c.Transfers)
                .HasForeignKey(e => e.CallSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FromParticipant)
                .WithMany()
                .HasForeignKey(e => e.FromParticipantId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ToHumanAgent)
                .WithMany(a => a.CallTransfers)
                .HasForeignKey(e => e.ToHumanAgentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Destination)
                .WithMany()
                .HasForeignKey(e => e.DestinationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.CallSessionId);
            entity.HasIndex(e => e.ToHumanAgentId);
            entity.HasIndex(e => e.DestinationId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RequestedAt);
        });

        // ── CALL HANDOFFS ─────────────────────────────────────────────
        modelBuilder.Entity<CallHandoff>(entity =>
        {
            entity.ToTable("call_handoffs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Summary).HasColumnType("text");
            entity.Property(e => e.ContextDataJson).HasColumnType("jsonb");
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.DeliveredAt).HasColumnType("timestamptz");
            entity.Property(e => e.AcceptedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.CallSession)
                .WithMany(c => c.Handoffs)
                .HasForeignKey(e => e.CallSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CallTransfer)
                .WithOne(t => t.Handoff)
                .HasForeignKey<CallHandoff>(e => e.CallTransferId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FromParticipant)
                .WithMany()
                .HasForeignKey(e => e.FromParticipantId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ToHumanAgent)
                .WithMany(a => a.CallHandoffs)
                .HasForeignKey(e => e.ToHumanAgentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.CallSessionId);
            entity.HasIndex(e => e.CallTransferId).IsUnique();
            entity.HasIndex(e => e.ToHumanAgentId);
            entity.HasIndex(e => e.Status);
        });

        // ── CALL RECORDINGS ───────────────────────────────────────────
        modelBuilder.Entity<CallRecording>(entity =>
        {
            entity.ToTable("call_recordings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StorageProvider).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ObjectKey).IsRequired().HasMaxLength(1024);
            entity.Property(e => e.ContentType).HasMaxLength(128);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.CompletedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.CallSession)
                .WithMany(c => c.Recordings)
                .HasForeignKey(e => e.CallSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.CallSessionId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ObjectKey);
        });

        // ── PLANS ─────────────────────────────────────────────────────
        modelBuilder.Entity<Plan>(entity =>
        {
            entity.ToTable("plans", "billing");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Tier).IsRequired();
            entity.Property(e => e.EntitlementsJson).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasIndex(e => e.Tier);
            entity.HasIndex(e => e.IsActive);
        });

        // ── PARTNER PLANS ─────────────────────────────────────────────
        modelBuilder.Entity<PartnerPlan>(entity =>
        {
            entity.ToTable("partner_plans", "identity");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.EntitlementsJson).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.Partner)
                .WithMany(p => p.PartnerPlans)
                .HasForeignKey(e => e.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PartnerId);
            entity.HasIndex(e => e.IsActive);
        });

        // ── SUBSCRIPTIONS ─────────────────────────────────────────────
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("subscriptions", "billing");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.StartsAt).HasColumnType("timestamptz");
            entity.Property(e => e.EndsAt).HasColumnType("timestamptz");
            entity.Property(e => e.TrialEndsAt).HasColumnType("timestamptz");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(e => e.PlanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PlanId);
            entity.HasIndex(e => e.Status);
        });

        // ── LICENSES ──────────────────────────────────────────────────
        modelBuilder.Entity<License>(entity =>
        {
            entity.ToTable("licenses", "identity");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.StartsAt).HasColumnType("timestamptz");
            entity.Property(e => e.EndsAt).HasColumnType("timestamptz");
            entity.Property(e => e.LimitsJson).HasColumnType("jsonb");
            entity.Property(e => e.MetadataJson).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Licenses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Partner)
                .WithMany(p => p.Licenses)
                .HasForeignKey(e => e.PartnerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PartnerPlan)
                .WithMany(p => p.Licenses)
                .HasForeignKey(e => e.PartnerPlanId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PartnerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartsAt);
        });

        // ── USAGE RECORDS ─────────────────────────────────────────────
        modelBuilder.Entity<UsageRecord>(entity =>
        {
            entity.ToTable("usage_records", "billing");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IdempotencyKey).IsRequired().HasMaxLength(128);
            entity.Property(e => e.MetricType).IsRequired();
            entity.Property(e => e.Quantity).HasColumnType("numeric(18,4)");
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(50);
            entity.Property(e => e.OccurredAt).HasColumnType("timestamptz");
            entity.Property(e => e.MetadataJson).HasColumnType("jsonb");

            entity.HasOne(e => e.User)
                .WithMany(u => u.UsageRecords)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Partner)
                .WithMany(p => p.UsageRecords)
                .HasForeignKey(e => e.PartnerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.License)
                .WithMany(l => l.UsageRecords)
                .HasForeignKey(e => e.LicenseId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.CallSession)
                .WithMany(c => c.UsageRecords)
                .HasForeignKey(e => e.CallSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PartnerId);
            entity.HasIndex(e => e.CallSessionId);
            entity.HasIndex(e => e.MetricType);
            entity.HasIndex(e => e.OccurredAt);
            entity.HasIndex(e => e.IdempotencyKey).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.OccurredAt });
        });

        // ── KNOWLEDGE BASES ───────────────────────────────────────────
        modelBuilder.Entity<KnowledgeBase>(entity =>
        {
            entity.ToTable("knowledge_bases", "configuration");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.User)
                .WithMany(u => u.KnowledgeBases)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsActive);
        });

        // ── KNOWLEDGE DOCUMENTS ───────────────────────────────────────
        modelBuilder.Entity<KnowledgeDocument>(entity =>
        {
            entity.ToTable("knowledge_documents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.SourceUri).IsRequired().HasMaxLength(2048);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(128);
            entity.Property(e => e.MetadataJson).HasColumnType("jsonb");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.KnowledgeBase)
                .WithMany(k => k.Documents)
                .HasForeignKey(e => e.KnowledgeBaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.KnowledgeBaseId);
            entity.HasIndex(e => e.Status);
        });

        // ── KNOWLEDGE CHUNKS ──────────────────────────────────────────
        modelBuilder.Entity<KnowledgeChunk>(entity =>
        {
            entity.ToTable("knowledge_chunks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChunkIndex).IsRequired();
            entity.Property(e => e.Content).IsRequired().HasColumnType("text");
            var isNpgsql = Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";
            if (isNpgsql)
            {
                entity.Property(e => e.Embedding).HasColumnType("vector(1536)");
            }
            else
            {
                // Non-relational providers (e.g. InMemory tests) cannot map the pgvector type;
                // serialize as "[1,2,3]" text instead.
                entity.Property(e => e.Embedding).HasConversion(
                    v => v == null ? null : "[" + string.Join(",", v.ToArray()) + "]",
                    s => string.IsNullOrEmpty(s) ? null : new Pgvector.Vector(
                        s.Trim('[', ']').Split(',').Select(float.Parse).ToArray()));
            }
            entity.Property(e => e.MetadataJson).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.KnowledgeDocument)
                .WithMany(d => d.Chunks)
                .HasForeignKey(e => e.KnowledgeDocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.KnowledgeDocumentId);
            entity.HasIndex(e => new { e.KnowledgeDocumentId, e.ChunkIndex }).IsUnique();
        });

        // ── PERSONA KNOWLEDGE BASES (Junction) ────────────────────────
        modelBuilder.Entity<PersonaKnowledgeBase>(entity =>
        {
            entity.ToTable("persona_knowledge_bases");
            entity.HasKey(e => new { e.PersonaId, e.KnowledgeBaseId });
            entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");

            entity.HasOne(e => e.Persona)
                .WithMany(p => p.PersonaKnowledgeBases)
                .HasForeignKey(e => e.PersonaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.KnowledgeBase)
                .WithMany(k => k.PersonaKnowledgeBases)
                .HasForeignKey(e => e.KnowledgeBaseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Seed Data ─────────────────────────────────────────────────
        modelBuilder.Entity<AgentUser>().HasData(
            new AgentUser { Id = 1, Username = "admin", PasswordHash = "admin" }
        );
    }
}
