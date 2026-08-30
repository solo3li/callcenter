using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations.Modular
{
    /// <inheritdoc />
    public partial class ModularIdentitySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:access_key_status", "active,revoked,expired")
                .Annotation("Npgsql:Enum:action_execution_status", "pending,running,completed,failed")
                .Annotation("Npgsql:Enum:action_type", "system,workflow,integration,webhook")
                .Annotation("Npgsql:Enum:api_key_status", "active,revoked,expired")
                .Annotation("Npgsql:Enum:call_direction", "inbound,outbound")
                .Annotation("Npgsql:Enum:call_session_status", "queued,ringing,active,transferred,completed,failed,cancelled")
                .Annotation("Npgsql:Enum:call_transfer_status", "requested,ringing,accepted,completed,rejected,failed,cancelled")
                .Annotation("Npgsql:Enum:handoff_status", "pending,delivered,accepted,expired")
                .Annotation("Npgsql:Enum:human_agent_status", "offline,available,break,not_ready,in_call")
                .Annotation("Npgsql:Enum:license_status", "active,inactive,expired,cancelled,suspended")
                .Annotation("Npgsql:Enum:metric_type", "call_duration,call_minutes,transfer_count,recording_minutes,agent_session_minutes")
                .Annotation("Npgsql:Enum:participant_type", "customer,ai_agent,human_agent")
                .Annotation("Npgsql:Enum:partner_relationship_status", "active,inactive,suspended")
                .Annotation("Npgsql:Enum:plan_tier", "free,starter,growth,enterprise")
                .Annotation("Npgsql:Enum:recording_status", "pending,in_progress,completed,failed")
                .Annotation("Npgsql:Enum:subscription_status", "active,past_due,cancelled,expired,trialing")
                .Annotation("Npgsql:Enum:user_status", "active,inactive,suspended")
                .Annotation("Npgsql:Enum:workflow_execution_status", "pending,running,completed,failed");

            migrationBuilder.CreateTable(
                name: "action_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    InputSchemaJson = table.Column<string>(type: "jsonb", nullable: true),
                    OutputSchemaJson = table.Column<string>(type: "jsonb", nullable: true),
                    ConfigurationJson = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_action_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agent_users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    IsPlatformPlan = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EntitlementsJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "call_records",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CallerId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    RecordingUrl = table.Column<string>(type: "text", nullable: true),
                    HandledByAgentId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_call_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_call_records_agent_users_HandledByAgentId",
                        column: x => x.HandledByAgentId,
                        principalTable: "agent_users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "action_executions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CallSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowExecutionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InputJson = table.Column<string>(type: "jsonb", nullable: true),
                    OutputJson = table.Column<string>(type: "jsonb", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_action_executions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_action_executions_action_definitions_ActionDefinitionId",
                        column: x => x.ActionDefinitionId,
                        principalTable: "action_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "api_keys",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    KeyPrefix = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    KeyHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Scopes = table.Column<string[]>(type: "text[]", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_keys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "call_configuration_actions",
                columns: table => new
                {
                    CallConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_call_configuration_actions", x => new { x.CallConfigurationId, x.ActionDefinitionId });
                    table.ForeignKey(
                        name: "FK_call_configuration_actions_action_definitions_ActionDefinit~",
                        column: x => x.ActionDefinitionId,
                        principalTable: "action_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "call_configurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    PersonaId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ConfigJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_call_configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "call_handoffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CallSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CallTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromParticipantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToHumanAgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    ContextDataJson = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_call_handoffs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "call_legs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CallSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    LegIndex = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<string>(type: "varchar(32)", nullable: false),
                    ParticipantIdentity = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    HangupCause = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_call_legs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "call_participants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CallSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    HumanAgentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParticipantType = table.Column<int>(type: "integer", nullable: false),
                    LivekitIdentity = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LivekitParticipantSid = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_call_participants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "call_recordings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CallSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_call_recordings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "call_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CallConfigurationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PersonaVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApiKeyId = table.Column<Guid>(type: "uuid", nullable: true),
                    LivekitRoomName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LivekitRoomSid = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DialedNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    OriginSipConnectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_call_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_call_sessions_api_keys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalSchema: "identity",
                        principalTable: "api_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_call_sessions_call_configurations_CallConfigurationId",
                        column: x => x.CallConfigurationId,
                        principalTable: "call_configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "call_transfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CallSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromParticipantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToHumanAgentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Mode = table.Column<string>(type: "varchar(16)", nullable: false),
                    TargetType = table.Column<string>(type: "varchar(32)", nullable: false),
                    DestinationId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_call_transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_call_transfers_call_participants_FromParticipantId",
                        column: x => x.FromParticipantId,
                        principalTable: "call_participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_call_transfers_call_sessions_CallSessionId",
                        column: x => x.CallSessionId,
                        principalTable: "call_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "human_agent_access_keys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HumanAgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    KeyPrefix = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    KeyHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_human_agent_access_keys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "human_agent_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HumanAgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    LivekitIdentity = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConnectedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    DisconnectedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    LastHeartbeatAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_human_agent_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "human_agents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MaxConcurrentCalls = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_human_agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "knowledge_bases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledge_bases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "knowledge_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KnowledgeBaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceUri = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledge_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_knowledge_documents_knowledge_bases_KnowledgeBaseId",
                        column: x => x.KnowledgeBaseId,
                        principalTable: "knowledge_bases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "knowledge_chunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KnowledgeDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkIndex = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledge_chunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_knowledge_chunks_knowledge_documents_KnowledgeDocumentId",
                        column: x => x.KnowledgeDocumentId,
                        principalTable: "knowledge_documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "licenses",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartnerPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    LimitsJson = table.Column<string>(type: "jsonb", nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_licenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "partner_external_customers",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalCustomerId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PlatformUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partner_external_customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "partner_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EntitlementsJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partner_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "partner_relationships",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partner_relationships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "partners",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Website = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "persona_actions",
                columns: table => new
                {
                    PersonaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persona_actions", x => new { x.PersonaId, x.ActionDefinitionId });
                    table.ForeignKey(
                        name: "FK_persona_actions_action_definitions_ActionDefinitionId",
                        column: x => x.ActionDefinitionId,
                        principalTable: "action_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "persona_knowledge_bases",
                columns: table => new
                {
                    PersonaId = table.Column<Guid>(type: "uuid", nullable: false),
                    KnowledgeBaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persona_knowledge_bases", x => new { x.PersonaId, x.KnowledgeBaseId });
                    table.ForeignKey(
                        name: "FK_persona_knowledge_bases_knowledge_bases_KnowledgeBaseId",
                        column: x => x.KnowledgeBaseId,
                        principalTable: "knowledge_bases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "persona_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    SystemPrompt = table.Column<string>(type: "text", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persona_versions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "personas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DefaultPersonaId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPartner = table.Column<bool>(type: "boolean", nullable: false),
                    StandardCredits = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    PremiumCredits = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_personas_DefaultPersonaId",
                        column: x => x.DefaultPersonaId,
                        principalTable: "personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "sip_connections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AllowedIps = table.Column<string[]>(type: "text[]", nullable: false),
                    Numbers = table.Column<string[]>(type: "text[]", nullable: false),
                    LkTrunkId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DispatchRuleId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MaxConcurrentCalls = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sip_connections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sip_connections_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sip_destinations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CallTo = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sip_destinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sip_destinations_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    TrialEndsAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_subscriptions_plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_subscriptions_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usage_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    LicenseId = table.Column<Guid>(type: "uuid", nullable: true),
                    CallSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MetricType = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usage_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usage_records_call_sessions_CallSessionId",
                        column: x => x.CallSessionId,
                        principalTable: "call_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_usage_records_licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalSchema: "identity",
                        principalTable: "licenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_usage_records_partners_PartnerId",
                        column: x => x.PartnerId,
                        principalSchema: "identity",
                        principalTable: "partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_usage_records_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflows_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    DefinitionJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_versions_workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_executions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CallSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InputJson = table.Column<string>(type: "jsonb", nullable: true),
                    OutputJson = table.Column<string>(type: "jsonb", nullable: true),
                    StateJson = table.Column<string>(type: "jsonb", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_executions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_executions_call_sessions_CallSessionId",
                        column: x => x.CallSessionId,
                        principalTable: "call_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_workflow_executions_workflow_versions_WorkflowVersionId",
                        column: x => x.WorkflowVersionId,
                        principalTable: "workflow_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "action_definitions",
                columns: new[] { "Id", "ActionType", "ConfigurationJson", "CreatedAt", "Description", "DisplayName", "InputSchemaJson", "IsActive", "IsSystem", "Name", "OutputSchemaJson", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0000-000000000001"), 0, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Transfers the call to an available human agent within the same LiveKit room.", "Transfer to Human", null, true, true, "transfer_to_human", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), 0, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ends the current call session.", "End Call", null, true, true, "end_call", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "agent_users",
                columns: new[] { "Id", "CreatedAt", "IsOnline", "PasswordHash", "Status", "Username" },
                values: new object[] { 1, new DateTime(2026, 8, 30, 16, 11, 25, 211, DateTimeKind.Utc).AddTicks(2249), false, "admin", "Offline", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_action_definitions_ActionType",
                table: "action_definitions",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_action_definitions_IsActive",
                table: "action_definitions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_action_definitions_Name",
                table: "action_definitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_action_executions_ActionDefinitionId",
                table: "action_executions",
                column: "ActionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_action_executions_CallSessionId",
                table: "action_executions",
                column: "CallSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_action_executions_Status",
                table: "action_executions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_action_executions_WorkflowExecutionId",
                table: "action_executions",
                column: "WorkflowExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_users_IsOnline",
                table: "agent_users",
                column: "IsOnline");

            migrationBuilder.CreateIndex(
                name: "IX_agent_users_Status",
                table: "agent_users",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_agent_users_Username",
                table: "agent_users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_KeyHash",
                schema: "identity",
                table: "api_keys",
                column: "KeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_Status",
                schema: "identity",
                table: "api_keys",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_UserId",
                schema: "identity",
                table: "api_keys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_call_configuration_actions_ActionDefinitionId",
                table: "call_configuration_actions",
                column: "ActionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_call_configurations_IsActive",
                table: "call_configurations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_call_configurations_PersonaId",
                table: "call_configurations",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_call_configurations_UserId",
                table: "call_configurations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_call_configurations_WorkflowId",
                table: "call_configurations",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_call_handoffs_CallSessionId",
                table: "call_handoffs",
                column: "CallSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_call_handoffs_CallTransferId",
                table: "call_handoffs",
                column: "CallTransferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_call_handoffs_FromParticipantId",
                table: "call_handoffs",
                column: "FromParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_call_handoffs_Status",
                table: "call_handoffs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_call_handoffs_ToHumanAgentId",
                table: "call_handoffs",
                column: "ToHumanAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_call_legs_CallSessionId",
                table: "call_legs",
                column: "CallSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_call_legs_CallSessionId_LegIndex",
                table: "call_legs",
                columns: new[] { "CallSessionId", "LegIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_call_legs_Kind",
                table: "call_legs",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_call_participants_CallSessionId",
                table: "call_participants",
                column: "CallSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_call_participants_HumanAgentId",
                table: "call_participants",
                column: "HumanAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_call_participants_ParticipantType",
                table: "call_participants",
                column: "ParticipantType");

            migrationBuilder.CreateIndex(
                name: "IX_call_recordings_CallSessionId",
                table: "call_recordings",
                column: "CallSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_call_recordings_ObjectKey",
                table: "call_recordings",
                column: "ObjectKey");

            migrationBuilder.CreateIndex(
                name: "IX_call_recordings_Status",
                table: "call_recordings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_call_records_HandledByAgentId",
                table: "call_records",
                column: "HandledByAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_call_records_RoomName",
                table: "call_records",
                column: "RoomName");

            migrationBuilder.CreateIndex(
                name: "IX_call_records_StartTime",
                table: "call_records",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_call_records_Status",
                table: "call_records",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_call_sessions_ApiKeyId",
                table: "call_sessions",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_call_sessions_CallConfigurationId",
                table: "call_sessions",
                column: "CallConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_call_sessions_LivekitRoomName",
                table: "call_sessions",
                column: "LivekitRoomName");

            migrationBuilder.CreateIndex(
                name: "IX_call_sessions_OriginSipConnectionId",
                table: "call_sessions",
                column: "OriginSipConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_call_sessions_PersonaVersionId",
                table: "call_sessions",
                column: "PersonaVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_call_sessions_StartedAt",
                table: "call_sessions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_call_sessions_Status",
                table: "call_sessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_call_sessions_UserId",
                table: "call_sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_call_sessions_UserId_StartedAt",
                table: "call_sessions",
                columns: new[] { "UserId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_call_sessions_WorkflowVersionId",
                table: "call_sessions",
                column: "WorkflowVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_call_transfers_CallSessionId",
                table: "call_transfers",
                column: "CallSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_call_transfers_DestinationId",
                table: "call_transfers",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_call_transfers_FromParticipantId",
                table: "call_transfers",
                column: "FromParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_call_transfers_RequestedAt",
                table: "call_transfers",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_call_transfers_Status",
                table: "call_transfers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_call_transfers_ToHumanAgentId",
                table: "call_transfers",
                column: "ToHumanAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_human_agent_access_keys_HumanAgentId",
                table: "human_agent_access_keys",
                column: "HumanAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_human_agent_access_keys_KeyHash",
                table: "human_agent_access_keys",
                column: "KeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_human_agent_access_keys_Status",
                table: "human_agent_access_keys",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_human_agent_sessions_ConnectedAt",
                table: "human_agent_sessions",
                column: "ConnectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_human_agent_sessions_HumanAgentId",
                table: "human_agent_sessions",
                column: "HumanAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_human_agent_sessions_Status",
                table: "human_agent_sessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_human_agents_ApplicationUserId",
                table: "human_agents",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_human_agents_IsActive",
                table: "human_agents",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_human_agents_OwnerUserId",
                table: "human_agents",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_human_agents_Status",
                table: "human_agents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_bases_IsActive",
                table: "knowledge_bases",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_bases_UserId",
                table: "knowledge_bases",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_chunks_KnowledgeDocumentId",
                table: "knowledge_chunks",
                column: "KnowledgeDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_chunks_KnowledgeDocumentId_ChunkIndex",
                table: "knowledge_chunks",
                columns: new[] { "KnowledgeDocumentId", "ChunkIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_documents_KnowledgeBaseId",
                table: "knowledge_documents",
                column: "KnowledgeBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_documents_Status",
                table: "knowledge_documents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_licenses_PartnerId",
                schema: "identity",
                table: "licenses",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_licenses_PartnerPlanId",
                schema: "identity",
                table: "licenses",
                column: "PartnerPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_licenses_StartsAt",
                schema: "identity",
                table: "licenses",
                column: "StartsAt");

            migrationBuilder.CreateIndex(
                name: "IX_licenses_Status",
                schema: "identity",
                table: "licenses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_licenses_UserId",
                schema: "identity",
                table: "licenses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_partner_external_customers_PartnerId_ExternalCustomerId",
                schema: "identity",
                table: "partner_external_customers",
                columns: new[] { "PartnerId", "ExternalCustomerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_partner_external_customers_PlatformUserId",
                schema: "identity",
                table: "partner_external_customers",
                column: "PlatformUserId");

            migrationBuilder.CreateIndex(
                name: "IX_partner_plans_IsActive",
                table: "partner_plans",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_partner_plans_PartnerId",
                table: "partner_plans",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_partner_relationships_CustomerUserId",
                schema: "identity",
                table: "partner_relationships",
                column: "CustomerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_partner_relationships_PartnerId",
                schema: "identity",
                table: "partner_relationships",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_partner_relationships_PartnerId_CustomerUserId",
                schema: "identity",
                table: "partner_relationships",
                columns: new[] { "PartnerId", "CustomerUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_partner_relationships_Status",
                schema: "identity",
                table: "partner_relationships",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_partner_relationships_UserId",
                schema: "identity",
                table: "partner_relationships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_partners_OrganizationName",
                schema: "identity",
                table: "partners",
                column: "OrganizationName");

            migrationBuilder.CreateIndex(
                name: "IX_partners_UserId",
                schema: "identity",
                table: "partners",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_persona_actions_ActionDefinitionId",
                table: "persona_actions",
                column: "ActionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_persona_knowledge_bases_KnowledgeBaseId",
                table: "persona_knowledge_bases",
                column: "KnowledgeBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_persona_versions_IsPublished",
                table: "persona_versions",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_persona_versions_PersonaId",
                table: "persona_versions",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_persona_versions_PersonaId_VersionNumber",
                table: "persona_versions",
                columns: new[] { "PersonaId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_personas_IsActive",
                table: "personas",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_personas_UserId",
                table: "personas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_plans_IsActive",
                table: "plans",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_plans_Tier",
                table: "plans",
                column: "Tier");

            migrationBuilder.CreateIndex(
                name: "IX_sip_connections_IsActive",
                table: "sip_connections",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_sip_connections_LkTrunkId",
                table: "sip_connections",
                column: "LkTrunkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sip_connections_UserId_Name",
                table: "sip_connections",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sip_destinations_IsEnabled",
                table: "sip_destinations",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_sip_destinations_UserId_Name",
                table: "sip_destinations",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_PlanId",
                table: "subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_Status",
                table: "subscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId",
                table: "subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_CallSessionId",
                table: "usage_records",
                column: "CallSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_IdempotencyKey",
                table: "usage_records",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_LicenseId",
                table: "usage_records",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_MetricType",
                table: "usage_records",
                column: "MetricType");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_OccurredAt",
                table: "usage_records",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_PartnerId",
                table: "usage_records",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_UserId",
                table: "usage_records",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_UserId_OccurredAt",
                table: "usage_records",
                columns: new[] { "UserId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_users_CreatedAt",
                schema: "identity",
                table: "users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_users_DefaultPersonaId",
                schema: "identity",
                table: "users",
                column: "DefaultPersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                schema: "identity",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Status",
                schema: "identity",
                table: "users",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_executions_CallSessionId",
                table: "workflow_executions",
                column: "CallSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_executions_Status",
                table: "workflow_executions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_executions_WorkflowVersionId",
                table: "workflow_executions",
                column: "WorkflowVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_versions_IsPublished",
                table: "workflow_versions",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_versions_WorkflowId",
                table: "workflow_versions",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_versions_WorkflowId_VersionNumber",
                table: "workflow_versions",
                columns: new[] { "WorkflowId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflows_IsActive",
                table: "workflows",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_UserId",
                table: "workflows",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_action_executions_call_sessions_CallSessionId",
                table: "action_executions",
                column: "CallSessionId",
                principalTable: "call_sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_action_executions_workflow_executions_WorkflowExecutionId",
                table: "action_executions",
                column: "WorkflowExecutionId",
                principalTable: "workflow_executions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_api_keys_users_UserId",
                schema: "identity",
                table: "api_keys",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_call_configuration_actions_call_configurations_CallConfigur~",
                table: "call_configuration_actions",
                column: "CallConfigurationId",
                principalTable: "call_configurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_call_configurations_personas_PersonaId",
                table: "call_configurations",
                column: "PersonaId",
                principalTable: "personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_call_configurations_users_UserId",
                table: "call_configurations",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_call_configurations_workflows_WorkflowId",
                table: "call_configurations",
                column: "WorkflowId",
                principalTable: "workflows",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_call_handoffs_call_participants_FromParticipantId",
                table: "call_handoffs",
                column: "FromParticipantId",
                principalTable: "call_participants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_call_handoffs_call_sessions_CallSessionId",
                table: "call_handoffs",
                column: "CallSessionId",
                principalTable: "call_sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_call_handoffs_call_transfers_CallTransferId",
                table: "call_handoffs",
                column: "CallTransferId",
                principalTable: "call_transfers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_call_handoffs_human_agents_ToHumanAgentId",
                table: "call_handoffs",
                column: "ToHumanAgentId",
                principalTable: "human_agents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_call_legs_call_sessions_CallSessionId",
                table: "call_legs",
                column: "CallSessionId",
                principalTable: "call_sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_call_participants_call_sessions_CallSessionId",
                table: "call_participants",
                column: "CallSessionId",
                principalTable: "call_sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_call_participants_human_agents_HumanAgentId",
                table: "call_participants",
                column: "HumanAgentId",
                principalTable: "human_agents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_call_recordings_call_sessions_CallSessionId",
                table: "call_recordings",
                column: "CallSessionId",
                principalTable: "call_sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_call_sessions_persona_versions_PersonaVersionId",
                table: "call_sessions",
                column: "PersonaVersionId",
                principalTable: "persona_versions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_call_sessions_sip_connections_OriginSipConnectionId",
                table: "call_sessions",
                column: "OriginSipConnectionId",
                principalTable: "sip_connections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_call_sessions_users_UserId",
                table: "call_sessions",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_call_sessions_workflow_versions_WorkflowVersionId",
                table: "call_sessions",
                column: "WorkflowVersionId",
                principalTable: "workflow_versions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_call_transfers_human_agents_ToHumanAgentId",
                table: "call_transfers",
                column: "ToHumanAgentId",
                principalTable: "human_agents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_call_transfers_sip_destinations_DestinationId",
                table: "call_transfers",
                column: "DestinationId",
                principalTable: "sip_destinations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_human_agent_access_keys_human_agents_HumanAgentId",
                table: "human_agent_access_keys",
                column: "HumanAgentId",
                principalTable: "human_agents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_human_agent_sessions_human_agents_HumanAgentId",
                table: "human_agent_sessions",
                column: "HumanAgentId",
                principalTable: "human_agents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_human_agents_users_ApplicationUserId",
                table: "human_agents",
                column: "ApplicationUserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_human_agents_users_OwnerUserId",
                table: "human_agents",
                column: "OwnerUserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_knowledge_bases_users_UserId",
                table: "knowledge_bases",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_licenses_partner_plans_PartnerPlanId",
                schema: "identity",
                table: "licenses",
                column: "PartnerPlanId",
                principalTable: "partner_plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_licenses_partners_PartnerId",
                schema: "identity",
                table: "licenses",
                column: "PartnerId",
                principalSchema: "identity",
                principalTable: "partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_licenses_users_UserId",
                schema: "identity",
                table: "licenses",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_partner_external_customers_partners_PartnerId",
                schema: "identity",
                table: "partner_external_customers",
                column: "PartnerId",
                principalSchema: "identity",
                principalTable: "partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_partner_external_customers_users_PlatformUserId",
                schema: "identity",
                table: "partner_external_customers",
                column: "PlatformUserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_partner_plans_partners_PartnerId",
                table: "partner_plans",
                column: "PartnerId",
                principalSchema: "identity",
                principalTable: "partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_partner_relationships_partners_PartnerId",
                schema: "identity",
                table: "partner_relationships",
                column: "PartnerId",
                principalSchema: "identity",
                principalTable: "partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_partner_relationships_users_CustomerUserId",
                schema: "identity",
                table: "partner_relationships",
                column: "CustomerUserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_partner_relationships_users_UserId",
                schema: "identity",
                table: "partner_relationships",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_partners_users_UserId",
                schema: "identity",
                table: "partners",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_persona_actions_personas_PersonaId",
                table: "persona_actions",
                column: "PersonaId",
                principalTable: "personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_persona_knowledge_bases_personas_PersonaId",
                table: "persona_knowledge_bases",
                column: "PersonaId",
                principalTable: "personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_persona_versions_personas_PersonaId",
                table: "persona_versions",
                column: "PersonaId",
                principalTable: "personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_personas_users_UserId",
                table: "personas",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_personas_users_UserId",
                table: "personas");

            migrationBuilder.DropTable(
                name: "action_executions");

            migrationBuilder.DropTable(
                name: "call_configuration_actions");

            migrationBuilder.DropTable(
                name: "call_handoffs");

            migrationBuilder.DropTable(
                name: "call_legs");

            migrationBuilder.DropTable(
                name: "call_recordings");

            migrationBuilder.DropTable(
                name: "call_records");

            migrationBuilder.DropTable(
                name: "human_agent_access_keys");

            migrationBuilder.DropTable(
                name: "human_agent_sessions");

            migrationBuilder.DropTable(
                name: "knowledge_chunks");

            migrationBuilder.DropTable(
                name: "partner_external_customers",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "partner_relationships",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "persona_actions");

            migrationBuilder.DropTable(
                name: "persona_knowledge_bases");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "usage_records");

            migrationBuilder.DropTable(
                name: "workflow_executions");

            migrationBuilder.DropTable(
                name: "call_transfers");

            migrationBuilder.DropTable(
                name: "agent_users");

            migrationBuilder.DropTable(
                name: "knowledge_documents");

            migrationBuilder.DropTable(
                name: "action_definitions");

            migrationBuilder.DropTable(
                name: "plans");

            migrationBuilder.DropTable(
                name: "licenses",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "call_participants");

            migrationBuilder.DropTable(
                name: "sip_destinations");

            migrationBuilder.DropTable(
                name: "knowledge_bases");

            migrationBuilder.DropTable(
                name: "partner_plans");

            migrationBuilder.DropTable(
                name: "call_sessions");

            migrationBuilder.DropTable(
                name: "human_agents");

            migrationBuilder.DropTable(
                name: "partners",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "api_keys",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "call_configurations");

            migrationBuilder.DropTable(
                name: "persona_versions");

            migrationBuilder.DropTable(
                name: "sip_connections");

            migrationBuilder.DropTable(
                name: "workflow_versions");

            migrationBuilder.DropTable(
                name: "workflows");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "personas");
        }
    }
}
