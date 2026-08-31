using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_action_executions_action_definitions_ActionDefinitionId",
                table: "action_executions");

            migrationBuilder.DropForeignKey(
                name: "FK_action_executions_call_sessions_CallSessionId",
                table: "action_executions");

            migrationBuilder.DropForeignKey(
                name: "FK_action_executions_workflow_executions_WorkflowExecutionId",
                table: "action_executions");

            migrationBuilder.DropForeignKey(
                name: "FK_api_keys_users_UserId",
                schema: "identity",
                table: "api_keys");

            migrationBuilder.DropForeignKey(
                name: "FK_call_configuration_actions_action_definitions_ActionDefinit~",
                table: "call_configuration_actions");

            migrationBuilder.DropForeignKey(
                name: "FK_call_configuration_actions_call_configurations_CallConfigur~",
                table: "call_configuration_actions");

            migrationBuilder.DropForeignKey(
                name: "FK_call_configurations_personas_PersonaId",
                table: "call_configurations");

            migrationBuilder.DropForeignKey(
                name: "FK_call_configurations_users_UserId",
                table: "call_configurations");

            migrationBuilder.DropForeignKey(
                name: "FK_call_configurations_workflows_WorkflowId",
                table: "call_configurations");

            migrationBuilder.DropForeignKey(
                name: "FK_call_handoffs_call_participants_FromParticipantId",
                table: "call_handoffs");

            migrationBuilder.DropForeignKey(
                name: "FK_call_handoffs_call_sessions_CallSessionId",
                table: "call_handoffs");

            migrationBuilder.DropForeignKey(
                name: "FK_call_handoffs_call_transfers_CallTransferId",
                table: "call_handoffs");

            migrationBuilder.DropForeignKey(
                name: "FK_call_handoffs_human_agents_ToHumanAgentId",
                table: "call_handoffs");

            migrationBuilder.DropForeignKey(
                name: "FK_call_legs_call_sessions_CallSessionId",
                table: "call_legs");

            migrationBuilder.DropForeignKey(
                name: "FK_call_participants_call_sessions_CallSessionId",
                table: "call_participants");

            migrationBuilder.DropForeignKey(
                name: "FK_call_participants_human_agents_HumanAgentId",
                table: "call_participants");

            migrationBuilder.DropForeignKey(
                name: "FK_call_recordings_call_sessions_CallSessionId",
                table: "call_recordings");

            migrationBuilder.DropForeignKey(
                name: "FK_call_records_agent_users_HandledByAgentId",
                table: "call_records");

            migrationBuilder.DropForeignKey(
                name: "FK_call_sessions_api_keys_ApiKeyId",
                table: "call_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_call_sessions_call_configurations_CallConfigurationId",
                table: "call_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_call_sessions_persona_versions_PersonaVersionId",
                table: "call_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_call_sessions_sip_connections_OriginSipConnectionId",
                table: "call_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_call_sessions_users_UserId",
                table: "call_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_call_sessions_workflow_versions_WorkflowVersionId",
                table: "call_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_call_transfers_call_participants_FromParticipantId",
                table: "call_transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_call_transfers_call_sessions_CallSessionId",
                table: "call_transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_call_transfers_human_agents_ToHumanAgentId",
                table: "call_transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_call_transfers_sip_destinations_DestinationId",
                table: "call_transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_human_agent_access_keys_human_agents_HumanAgentId",
                table: "human_agent_access_keys");

            migrationBuilder.DropForeignKey(
                name: "FK_human_agent_sessions_human_agents_HumanAgentId",
                table: "human_agent_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_human_agents_users_ApplicationUserId",
                table: "human_agents");

            migrationBuilder.DropForeignKey(
                name: "FK_human_agents_users_OwnerUserId",
                table: "human_agents");

            migrationBuilder.DropForeignKey(
                name: "FK_knowledge_bases_users_UserId",
                schema: "configuration",
                table: "knowledge_bases");

            migrationBuilder.DropForeignKey(
                name: "FK_knowledge_chunks_knowledge_documents_KnowledgeDocumentId",
                table: "knowledge_chunks");

            migrationBuilder.DropForeignKey(
                name: "FK_knowledge_documents_knowledge_bases_KnowledgeBaseId",
                table: "knowledge_documents");

            migrationBuilder.DropForeignKey(
                name: "FK_licenses_partner_plans_PartnerPlanId",
                schema: "identity",
                table: "licenses");

            migrationBuilder.DropForeignKey(
                name: "FK_licenses_partners_PartnerId",
                schema: "identity",
                table: "licenses");

            migrationBuilder.DropForeignKey(
                name: "FK_licenses_users_UserId",
                schema: "identity",
                table: "licenses");

            migrationBuilder.DropForeignKey(
                name: "FK_partner_external_customers_partners_PartnerId",
                schema: "identity",
                table: "partner_external_customers");

            migrationBuilder.DropForeignKey(
                name: "FK_partner_external_customers_users_PlatformUserId",
                schema: "identity",
                table: "partner_external_customers");

            migrationBuilder.DropForeignKey(
                name: "FK_partner_plans_partners_PartnerId",
                schema: "identity",
                table: "partner_plans");

            migrationBuilder.DropForeignKey(
                name: "FK_partner_relationships_partners_PartnerId",
                schema: "identity",
                table: "partner_relationships");

            migrationBuilder.DropForeignKey(
                name: "FK_partner_relationships_users_CustomerUserId",
                schema: "identity",
                table: "partner_relationships");

            migrationBuilder.DropForeignKey(
                name: "FK_partner_relationships_users_UserId",
                schema: "identity",
                table: "partner_relationships");

            migrationBuilder.DropForeignKey(
                name: "FK_partners_users_UserId",
                schema: "identity",
                table: "partners");

            migrationBuilder.DropForeignKey(
                name: "FK_persona_actions_action_definitions_ActionDefinitionId",
                table: "persona_actions");

            migrationBuilder.DropForeignKey(
                name: "FK_persona_actions_personas_PersonaId",
                table: "persona_actions");

            migrationBuilder.DropForeignKey(
                name: "FK_persona_knowledge_bases_knowledge_bases_KnowledgeBaseId",
                table: "persona_knowledge_bases");

            migrationBuilder.DropForeignKey(
                name: "FK_persona_knowledge_bases_personas_PersonaId",
                table: "persona_knowledge_bases");

            migrationBuilder.DropForeignKey(
                name: "FK_persona_versions_personas_PersonaId",
                table: "persona_versions");

            migrationBuilder.DropForeignKey(
                name: "FK_personas_users_UserId",
                schema: "configuration",
                table: "personas");

            migrationBuilder.DropForeignKey(
                name: "FK_sip_connections_users_UserId",
                table: "sip_connections");

            migrationBuilder.DropForeignKey(
                name: "FK_sip_destinations_users_UserId",
                table: "sip_destinations");

            migrationBuilder.DropForeignKey(
                name: "FK_subscriptions_plans_PlanId",
                schema: "billing",
                table: "subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_subscriptions_users_UserId",
                schema: "billing",
                table: "subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_usage_records_call_sessions_CallSessionId",
                schema: "billing",
                table: "usage_records");

            migrationBuilder.DropForeignKey(
                name: "FK_usage_records_licenses_LicenseId",
                schema: "billing",
                table: "usage_records");

            migrationBuilder.DropForeignKey(
                name: "FK_usage_records_partners_PartnerId",
                schema: "billing",
                table: "usage_records");

            migrationBuilder.DropForeignKey(
                name: "FK_usage_records_users_UserId",
                schema: "billing",
                table: "usage_records");

            migrationBuilder.DropForeignKey(
                name: "FK_users_personas_DefaultPersonaId",
                schema: "identity",
                table: "users");

            migrationBuilder.DropForeignKey(
                name: "FK_workflow_executions_call_sessions_CallSessionId",
                table: "workflow_executions");

            migrationBuilder.DropForeignKey(
                name: "FK_workflow_executions_workflow_versions_WorkflowVersionId",
                table: "workflow_executions");

            migrationBuilder.DropForeignKey(
                name: "FK_workflow_versions_workflows_WorkflowId",
                table: "workflow_versions");

            migrationBuilder.DropForeignKey(
                name: "FK_workflows_users_UserId",
                schema: "configuration",
                table: "workflows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_workflow_versions",
                table: "workflow_versions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_workflow_executions",
                table: "workflow_executions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sip_destinations",
                table: "sip_destinations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sip_connections",
                table: "sip_connections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_persona_versions",
                table: "persona_versions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_persona_knowledge_bases",
                table: "persona_knowledge_bases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_persona_actions",
                table: "persona_actions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_knowledge_documents",
                table: "knowledge_documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_knowledge_chunks",
                table: "knowledge_chunks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_human_agents",
                table: "human_agents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_human_agent_sessions",
                table: "human_agent_sessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_human_agent_access_keys",
                table: "human_agent_access_keys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_call_transfers",
                table: "call_transfers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_call_sessions",
                table: "call_sessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_call_records",
                table: "call_records");

            migrationBuilder.DropPrimaryKey(
                name: "PK_call_recordings",
                table: "call_recordings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_call_participants",
                table: "call_participants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_call_legs",
                table: "call_legs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_call_handoffs",
                table: "call_handoffs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_call_configurations",
                table: "call_configurations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_call_configuration_actions",
                table: "call_configuration_actions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_agent_users",
                table: "agent_users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_action_executions",
                table: "action_executions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_action_definitions",
                table: "action_definitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_workflows",
                schema: "configuration",
                table: "workflows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                schema: "identity",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usage_records",
                schema: "billing",
                table: "usage_records");

            migrationBuilder.DropPrimaryKey(
                name: "PK_subscriptions",
                schema: "billing",
                table: "subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_plans",
                schema: "billing",
                table: "plans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_personas",
                schema: "configuration",
                table: "personas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_partners",
                schema: "identity",
                table: "partners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_partner_relationships",
                schema: "identity",
                table: "partner_relationships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_partner_plans",
                schema: "identity",
                table: "partner_plans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_partner_external_customers",
                schema: "identity",
                table: "partner_external_customers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_licenses",
                schema: "identity",
                table: "licenses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_knowledge_bases",
                schema: "configuration",
                table: "knowledge_bases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_api_keys",
                schema: "identity",
                table: "api_keys");

            migrationBuilder.RenameTable(
                name: "workflows",
                schema: "configuration",
                newName: "workflows");

            migrationBuilder.RenameTable(
                name: "users",
                schema: "identity",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "usage_records",
                schema: "billing",
                newName: "usage_records");

            migrationBuilder.RenameTable(
                name: "subscriptions",
                schema: "billing",
                newName: "subscriptions");

            migrationBuilder.RenameTable(
                name: "plans",
                schema: "billing",
                newName: "plans");

            migrationBuilder.RenameTable(
                name: "personas",
                schema: "configuration",
                newName: "personas");

            migrationBuilder.RenameTable(
                name: "partners",
                schema: "identity",
                newName: "partners");

            migrationBuilder.RenameTable(
                name: "partner_relationships",
                schema: "identity",
                newName: "partner_relationships");

            migrationBuilder.RenameTable(
                name: "partner_plans",
                schema: "identity",
                newName: "partner_plans");

            migrationBuilder.RenameTable(
                name: "partner_external_customers",
                schema: "identity",
                newName: "partner_external_customers");

            migrationBuilder.RenameTable(
                name: "licenses",
                schema: "identity",
                newName: "licenses");

            migrationBuilder.RenameTable(
                name: "knowledge_bases",
                schema: "configuration",
                newName: "knowledge_bases");

            migrationBuilder.RenameTable(
                name: "api_keys",
                schema: "identity",
                newName: "api_keys");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "workflow_versions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WorkflowId",
                table: "workflow_versions",
                newName: "workflow_id");

            migrationBuilder.RenameColumn(
                name: "VersionNumber",
                table: "workflow_versions",
                newName: "version_number");

            migrationBuilder.RenameColumn(
                name: "IsPublished",
                table: "workflow_versions",
                newName: "is_published");

            migrationBuilder.RenameColumn(
                name: "DefinitionJson",
                table: "workflow_versions",
                newName: "definition_json");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "workflow_versions",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_workflow_versions_WorkflowId_VersionNumber",
                table: "workflow_versions",
                newName: "ix_workflow_versions_workflow_id_version_number");

            migrationBuilder.RenameIndex(
                name: "IX_workflow_versions_WorkflowId",
                table: "workflow_versions",
                newName: "ix_workflow_versions_workflow_id");

            migrationBuilder.RenameIndex(
                name: "IX_workflow_versions_IsPublished",
                table: "workflow_versions",
                newName: "ix_workflow_versions_is_published");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "workflow_executions",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Error",
                table: "workflow_executions",
                newName: "error");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "workflow_executions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WorkflowVersionId",
                table: "workflow_executions",
                newName: "workflow_version_id");

            migrationBuilder.RenameColumn(
                name: "StateJson",
                table: "workflow_executions",
                newName: "state_json");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "workflow_executions",
                newName: "started_at");

            migrationBuilder.RenameColumn(
                name: "OutputJson",
                table: "workflow_executions",
                newName: "output_json");

            migrationBuilder.RenameColumn(
                name: "InputJson",
                table: "workflow_executions",
                newName: "input_json");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "workflow_executions",
                newName: "completed_at");

            migrationBuilder.RenameColumn(
                name: "CallSessionId",
                table: "workflow_executions",
                newName: "call_session_id");

            migrationBuilder.RenameIndex(
                name: "IX_workflow_executions_Status",
                table: "workflow_executions",
                newName: "ix_workflow_executions_status");

            migrationBuilder.RenameIndex(
                name: "IX_workflow_executions_WorkflowVersionId",
                table: "workflow_executions",
                newName: "ix_workflow_executions_workflow_version_id");

            migrationBuilder.RenameIndex(
                name: "IX_workflow_executions_CallSessionId",
                table: "workflow_executions",
                newName: "ix_workflow_executions_call_session_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "sip_destinations",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "sip_destinations",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "sip_destinations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "sip_destinations",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "sip_destinations",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "sip_destinations",
                newName: "is_enabled");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "sip_destinations",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CallTo",
                table: "sip_destinations",
                newName: "call_to");

            migrationBuilder.RenameIndex(
                name: "IX_sip_destinations_UserId_Name",
                table: "sip_destinations",
                newName: "ix_sip_destinations_user_id_name");

            migrationBuilder.RenameIndex(
                name: "IX_sip_destinations_IsEnabled",
                table: "sip_destinations",
                newName: "ix_sip_destinations_is_enabled");

            migrationBuilder.RenameColumn(
                name: "Numbers",
                table: "sip_connections",
                newName: "numbers");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "sip_connections",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "sip_connections",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "sip_connections",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "sip_connections",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "MaxConcurrentCalls",
                table: "sip_connections",
                newName: "max_concurrent_calls");

            migrationBuilder.RenameColumn(
                name: "LkTrunkId",
                table: "sip_connections",
                newName: "lk_trunk_id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "sip_connections",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "DispatchRuleId",
                table: "sip_connections",
                newName: "dispatch_rule_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "sip_connections",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AllowedIps",
                table: "sip_connections",
                newName: "allowed_ips");

            migrationBuilder.RenameIndex(
                name: "IX_sip_connections_UserId_Name",
                table: "sip_connections",
                newName: "ix_sip_connections_user_id_name");

            migrationBuilder.RenameIndex(
                name: "IX_sip_connections_LkTrunkId",
                table: "sip_connections",
                newName: "ix_sip_connections_lk_trunk_id");

            migrationBuilder.RenameIndex(
                name: "IX_sip_connections_IsActive",
                table: "sip_connections",
                newName: "ix_sip_connections_is_active");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "persona_versions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "VersionNumber",
                table: "persona_versions",
                newName: "version_number");

            migrationBuilder.RenameColumn(
                name: "SystemPrompt",
                table: "persona_versions",
                newName: "system_prompt");

            migrationBuilder.RenameColumn(
                name: "PersonaId",
                table: "persona_versions",
                newName: "persona_id");

            migrationBuilder.RenameColumn(
                name: "IsPublished",
                table: "persona_versions",
                newName: "is_published");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "persona_versions",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ConfigurationJson",
                table: "persona_versions",
                newName: "configuration_json");

            migrationBuilder.RenameIndex(
                name: "IX_persona_versions_PersonaId_VersionNumber",
                table: "persona_versions",
                newName: "ix_persona_versions_persona_id_version_number");

            migrationBuilder.RenameIndex(
                name: "IX_persona_versions_PersonaId",
                table: "persona_versions",
                newName: "ix_persona_versions_persona_id");

            migrationBuilder.RenameIndex(
                name: "IX_persona_versions_IsPublished",
                table: "persona_versions",
                newName: "ix_persona_versions_is_published");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "persona_knowledge_bases",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "KnowledgeBaseId",
                table: "persona_knowledge_bases",
                newName: "knowledge_base_id");

            migrationBuilder.RenameColumn(
                name: "PersonaId",
                table: "persona_knowledge_bases",
                newName: "persona_id");

            migrationBuilder.RenameIndex(
                name: "IX_persona_knowledge_bases_KnowledgeBaseId",
                table: "persona_knowledge_bases",
                newName: "ix_persona_knowledge_bases_knowledge_base_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "persona_actions",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ActionDefinitionId",
                table: "persona_actions",
                newName: "action_definition_id");

            migrationBuilder.RenameColumn(
                name: "PersonaId",
                table: "persona_actions",
                newName: "persona_id");

            migrationBuilder.RenameIndex(
                name: "IX_persona_actions_ActionDefinitionId",
                table: "persona_actions",
                newName: "ix_persona_actions_action_definition_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "knowledge_documents",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "knowledge_documents",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "knowledge_documents",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "knowledge_documents",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "SourceUri",
                table: "knowledge_documents",
                newName: "source_uri");

            migrationBuilder.RenameColumn(
                name: "MetadataJson",
                table: "knowledge_documents",
                newName: "metadata_json");

            migrationBuilder.RenameColumn(
                name: "KnowledgeBaseId",
                table: "knowledge_documents",
                newName: "knowledge_base_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "knowledge_documents",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "knowledge_documents",
                newName: "content_type");

            migrationBuilder.RenameIndex(
                name: "IX_knowledge_documents_Status",
                table: "knowledge_documents",
                newName: "ix_knowledge_documents_status");

            migrationBuilder.RenameIndex(
                name: "IX_knowledge_documents_KnowledgeBaseId",
                table: "knowledge_documents",
                newName: "ix_knowledge_documents_knowledge_base_id");

            migrationBuilder.RenameColumn(
                name: "Embedding",
                table: "knowledge_chunks",
                newName: "embedding");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "knowledge_chunks",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "knowledge_chunks",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "MetadataJson",
                table: "knowledge_chunks",
                newName: "metadata_json");

            migrationBuilder.RenameColumn(
                name: "KnowledgeDocumentId",
                table: "knowledge_chunks",
                newName: "knowledge_document_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "knowledge_chunks",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ChunkIndex",
                table: "knowledge_chunks",
                newName: "chunk_index");

            migrationBuilder.RenameIndex(
                name: "IX_knowledge_chunks_KnowledgeDocumentId_ChunkIndex",
                table: "knowledge_chunks",
                newName: "ix_knowledge_chunks_knowledge_document_id_chunk_index");

            migrationBuilder.RenameIndex(
                name: "IX_knowledge_chunks_KnowledgeDocumentId",
                table: "knowledge_chunks",
                newName: "ix_knowledge_chunks_knowledge_document_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "human_agents",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "human_agents",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "human_agents",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "human_agents",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "human_agents",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "OwnerUserId",
                table: "human_agents",
                newName: "owner_user_id");

            migrationBuilder.RenameColumn(
                name: "MaxConcurrentCalls",
                table: "human_agents",
                newName: "max_concurrent_calls");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "human_agents",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "human_agents",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "human_agents",
                newName: "application_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_human_agents_Status",
                table: "human_agents",
                newName: "ix_human_agents_status");

            migrationBuilder.RenameIndex(
                name: "IX_human_agents_OwnerUserId",
                table: "human_agents",
                newName: "ix_human_agents_owner_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_human_agents_IsActive",
                table: "human_agents",
                newName: "ix_human_agents_is_active");

            migrationBuilder.RenameIndex(
                name: "IX_human_agents_ApplicationUserId",
                table: "human_agents",
                newName: "ix_human_agents_application_user_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "human_agent_sessions",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "human_agent_sessions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "MetadataJson",
                table: "human_agent_sessions",
                newName: "metadata_json");

            migrationBuilder.RenameColumn(
                name: "LivekitIdentity",
                table: "human_agent_sessions",
                newName: "livekit_identity");

            migrationBuilder.RenameColumn(
                name: "LastHeartbeatAt",
                table: "human_agent_sessions",
                newName: "last_heartbeat_at");

            migrationBuilder.RenameColumn(
                name: "HumanAgentId",
                table: "human_agent_sessions",
                newName: "human_agent_id");

            migrationBuilder.RenameColumn(
                name: "DisconnectedAt",
                table: "human_agent_sessions",
                newName: "disconnected_at");

            migrationBuilder.RenameColumn(
                name: "ConnectedAt",
                table: "human_agent_sessions",
                newName: "connected_at");

            migrationBuilder.RenameIndex(
                name: "IX_human_agent_sessions_Status",
                table: "human_agent_sessions",
                newName: "ix_human_agent_sessions_status");

            migrationBuilder.RenameIndex(
                name: "IX_human_agent_sessions_HumanAgentId",
                table: "human_agent_sessions",
                newName: "ix_human_agent_sessions_human_agent_id");

            migrationBuilder.RenameIndex(
                name: "IX_human_agent_sessions_ConnectedAt",
                table: "human_agent_sessions",
                newName: "ix_human_agent_sessions_connected_at");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "human_agent_access_keys",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "human_agent_access_keys",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "human_agent_access_keys",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RevokedAt",
                table: "human_agent_access_keys",
                newName: "revoked_at");

            migrationBuilder.RenameColumn(
                name: "LastUsedAt",
                table: "human_agent_access_keys",
                newName: "last_used_at");

            migrationBuilder.RenameColumn(
                name: "KeyPrefix",
                table: "human_agent_access_keys",
                newName: "key_prefix");

            migrationBuilder.RenameColumn(
                name: "KeyHash",
                table: "human_agent_access_keys",
                newName: "key_hash");

            migrationBuilder.RenameColumn(
                name: "HumanAgentId",
                table: "human_agent_access_keys",
                newName: "human_agent_id");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "human_agent_access_keys",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "human_agent_access_keys",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_human_agent_access_keys_Status",
                table: "human_agent_access_keys",
                newName: "ix_human_agent_access_keys_status");

            migrationBuilder.RenameIndex(
                name: "IX_human_agent_access_keys_KeyHash",
                table: "human_agent_access_keys",
                newName: "ix_human_agent_access_keys_key_hash");

            migrationBuilder.RenameIndex(
                name: "IX_human_agent_access_keys_HumanAgentId",
                table: "human_agent_access_keys",
                newName: "ix_human_agent_access_keys_human_agent_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "call_transfers",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "call_transfers",
                newName: "reason");

            migrationBuilder.RenameColumn(
                name: "Mode",
                table: "call_transfers",
                newName: "mode");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "call_transfers",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "call_transfers",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "ToHumanAgentId",
                table: "call_transfers",
                newName: "to_human_agent_id");

            migrationBuilder.RenameColumn(
                name: "TargetType",
                table: "call_transfers",
                newName: "target_type");

            migrationBuilder.RenameColumn(
                name: "TargetSnapshotJson",
                table: "call_transfers",
                newName: "target_snapshot_json");

            migrationBuilder.RenameColumn(
                name: "RequestedAt",
                table: "call_transfers",
                newName: "requested_at");

            migrationBuilder.RenameColumn(
                name: "FromParticipantId",
                table: "call_transfers",
                newName: "from_participant_id");

            migrationBuilder.RenameColumn(
                name: "FailureReason",
                table: "call_transfers",
                newName: "failure_reason");

            migrationBuilder.RenameColumn(
                name: "FailedAt",
                table: "call_transfers",
                newName: "failed_at");

            migrationBuilder.RenameColumn(
                name: "DestinationId",
                table: "call_transfers",
                newName: "destination_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "call_transfers",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "call_transfers",
                newName: "completed_at");

            migrationBuilder.RenameColumn(
                name: "CallSessionId",
                table: "call_transfers",
                newName: "call_session_id");

            migrationBuilder.RenameColumn(
                name: "AcceptedAt",
                table: "call_transfers",
                newName: "accepted_at");

            migrationBuilder.RenameIndex(
                name: "IX_call_transfers_Status",
                table: "call_transfers",
                newName: "ix_call_transfers_status");

            migrationBuilder.RenameIndex(
                name: "IX_call_transfers_ToHumanAgentId",
                table: "call_transfers",
                newName: "ix_call_transfers_to_human_agent_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_transfers_RequestedAt",
                table: "call_transfers",
                newName: "ix_call_transfers_requested_at");

            migrationBuilder.RenameIndex(
                name: "IX_call_transfers_FromParticipantId",
                table: "call_transfers",
                newName: "ix_call_transfers_from_participant_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_transfers_DestinationId",
                table: "call_transfers",
                newName: "ix_call_transfers_destination_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_transfers_CallSessionId",
                table: "call_transfers",
                newName: "ix_call_transfers_call_session_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "call_sessions",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Direction",
                table: "call_sessions",
                newName: "direction");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "call_sessions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WorkflowVersionId",
                table: "call_sessions",
                newName: "workflow_version_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "call_sessions",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "call_sessions",
                newName: "started_at");

            migrationBuilder.RenameColumn(
                name: "PersonaVersionId",
                table: "call_sessions",
                newName: "persona_version_id");

            migrationBuilder.RenameColumn(
                name: "OriginSipConnectionId",
                table: "call_sessions",
                newName: "origin_sip_connection_id");

            migrationBuilder.RenameColumn(
                name: "MetadataJson",
                table: "call_sessions",
                newName: "metadata_json");

            migrationBuilder.RenameColumn(
                name: "LivekitRoomSid",
                table: "call_sessions",
                newName: "livekit_room_sid");

            migrationBuilder.RenameColumn(
                name: "LivekitRoomName",
                table: "call_sessions",
                newName: "livekit_room_name");

            migrationBuilder.RenameColumn(
                name: "EndedAt",
                table: "call_sessions",
                newName: "ended_at");

            migrationBuilder.RenameColumn(
                name: "DurationSeconds",
                table: "call_sessions",
                newName: "duration_seconds");

            migrationBuilder.RenameColumn(
                name: "DialedNumber",
                table: "call_sessions",
                newName: "dialed_number");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "call_sessions",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CallConfigurationId",
                table: "call_sessions",
                newName: "call_configuration_id");

            migrationBuilder.RenameColumn(
                name: "ApiKeyId",
                table: "call_sessions",
                newName: "api_key_id");

            migrationBuilder.RenameColumn(
                name: "AnsweredAt",
                table: "call_sessions",
                newName: "answered_at");

            migrationBuilder.RenameIndex(
                name: "IX_call_sessions_Status",
                table: "call_sessions",
                newName: "ix_call_sessions_status");

            migrationBuilder.RenameIndex(
                name: "IX_call_sessions_WorkflowVersionId",
                table: "call_sessions",
                newName: "ix_call_sessions_workflow_version_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_sessions_UserId_StartedAt",
                table: "call_sessions",
                newName: "ix_call_sessions_user_id_started_at");

            migrationBuilder.RenameIndex(
                name: "IX_call_sessions_UserId",
                table: "call_sessions",
                newName: "ix_call_sessions_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_sessions_StartedAt",
                table: "call_sessions",
                newName: "ix_call_sessions_started_at");

            migrationBuilder.RenameIndex(
                name: "IX_call_sessions_PersonaVersionId",
                table: "call_sessions",
                newName: "ix_call_sessions_persona_version_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_sessions_OriginSipConnectionId",
                table: "call_sessions",
                newName: "ix_call_sessions_origin_sip_connection_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_sessions_LivekitRoomName",
                table: "call_sessions",
                newName: "ix_call_sessions_livekit_room_name");

            migrationBuilder.RenameIndex(
                name: "IX_call_sessions_CallConfigurationId",
                table: "call_sessions",
                newName: "ix_call_sessions_call_configuration_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_sessions_ApiKeyId",
                table: "call_sessions",
                newName: "ix_call_sessions_api_key_id");

            migrationBuilder.RenameColumn(
                name: "Summary",
                table: "call_records",
                newName: "summary");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "call_records",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "call_records",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "call_records",
                newName: "start_time");

            migrationBuilder.RenameColumn(
                name: "RoomName",
                table: "call_records",
                newName: "room_name");

            migrationBuilder.RenameColumn(
                name: "RecordingUrl",
                table: "call_records",
                newName: "recording_url");

            migrationBuilder.RenameColumn(
                name: "HandledByAgentId",
                table: "call_records",
                newName: "handled_by_agent_id");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "call_records",
                newName: "end_time");

            migrationBuilder.RenameColumn(
                name: "CallerId",
                table: "call_records",
                newName: "caller_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_records_Status",
                table: "call_records",
                newName: "ix_call_records_status");

            migrationBuilder.RenameIndex(
                name: "IX_call_records_StartTime",
                table: "call_records",
                newName: "ix_call_records_start_time");

            migrationBuilder.RenameIndex(
                name: "IX_call_records_RoomName",
                table: "call_records",
                newName: "ix_call_records_room_name");

            migrationBuilder.RenameIndex(
                name: "IX_call_records_HandledByAgentId",
                table: "call_records",
                newName: "ix_call_records_handled_by_agent_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "call_recordings",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "call_recordings",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StorageProvider",
                table: "call_recordings",
                newName: "storage_provider");

            migrationBuilder.RenameColumn(
                name: "SizeBytes",
                table: "call_recordings",
                newName: "size_bytes");

            migrationBuilder.RenameColumn(
                name: "ObjectKey",
                table: "call_recordings",
                newName: "object_key");

            migrationBuilder.RenameColumn(
                name: "DurationSeconds",
                table: "call_recordings",
                newName: "duration_seconds");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "call_recordings",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "call_recordings",
                newName: "content_type");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "call_recordings",
                newName: "completed_at");

            migrationBuilder.RenameColumn(
                name: "CallSessionId",
                table: "call_recordings",
                newName: "call_session_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_recordings_Status",
                table: "call_recordings",
                newName: "ix_call_recordings_status");

            migrationBuilder.RenameIndex(
                name: "IX_call_recordings_ObjectKey",
                table: "call_recordings",
                newName: "ix_call_recordings_object_key");

            migrationBuilder.RenameIndex(
                name: "IX_call_recordings_CallSessionId",
                table: "call_recordings",
                newName: "ix_call_recordings_call_session_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "call_participants",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ParticipantType",
                table: "call_participants",
                newName: "participant_type");

            migrationBuilder.RenameColumn(
                name: "LivekitParticipantSid",
                table: "call_participants",
                newName: "livekit_participant_sid");

            migrationBuilder.RenameColumn(
                name: "LivekitIdentity",
                table: "call_participants",
                newName: "livekit_identity");

            migrationBuilder.RenameColumn(
                name: "LeftAt",
                table: "call_participants",
                newName: "left_at");

            migrationBuilder.RenameColumn(
                name: "JoinedAt",
                table: "call_participants",
                newName: "joined_at");

            migrationBuilder.RenameColumn(
                name: "HumanAgentId",
                table: "call_participants",
                newName: "human_agent_id");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "call_participants",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "call_participants",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CallSessionId",
                table: "call_participants",
                newName: "call_session_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_participants_ParticipantType",
                table: "call_participants",
                newName: "ix_call_participants_participant_type");

            migrationBuilder.RenameIndex(
                name: "IX_call_participants_HumanAgentId",
                table: "call_participants",
                newName: "ix_call_participants_human_agent_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_participants_CallSessionId",
                table: "call_participants",
                newName: "ix_call_participants_call_session_id");

            migrationBuilder.RenameColumn(
                name: "Kind",
                table: "call_legs",
                newName: "kind");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "call_legs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "call_legs",
                newName: "started_at");

            migrationBuilder.RenameColumn(
                name: "ParticipantIdentity",
                table: "call_legs",
                newName: "participant_identity");

            migrationBuilder.RenameColumn(
                name: "LegIndex",
                table: "call_legs",
                newName: "leg_index");

            migrationBuilder.RenameColumn(
                name: "HangupCause",
                table: "call_legs",
                newName: "hangup_cause");

            migrationBuilder.RenameColumn(
                name: "EndedAt",
                table: "call_legs",
                newName: "ended_at");

            migrationBuilder.RenameColumn(
                name: "CallSessionId",
                table: "call_legs",
                newName: "call_session_id");

            migrationBuilder.RenameColumn(
                name: "AnsweredAt",
                table: "call_legs",
                newName: "answered_at");

            migrationBuilder.RenameIndex(
                name: "IX_call_legs_Kind",
                table: "call_legs",
                newName: "ix_call_legs_kind");

            migrationBuilder.RenameIndex(
                name: "IX_call_legs_CallSessionId_LegIndex",
                table: "call_legs",
                newName: "ix_call_legs_call_session_id_leg_index");

            migrationBuilder.RenameIndex(
                name: "IX_call_legs_CallSessionId",
                table: "call_legs",
                newName: "ix_call_legs_call_session_id");

            migrationBuilder.RenameColumn(
                name: "Summary",
                table: "call_handoffs",
                newName: "summary");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "call_handoffs",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "call_handoffs",
                newName: "reason");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "call_handoffs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ToHumanAgentId",
                table: "call_handoffs",
                newName: "to_human_agent_id");

            migrationBuilder.RenameColumn(
                name: "FromParticipantId",
                table: "call_handoffs",
                newName: "from_participant_id");

            migrationBuilder.RenameColumn(
                name: "DeliveredAt",
                table: "call_handoffs",
                newName: "delivered_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "call_handoffs",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ContextDataJson",
                table: "call_handoffs",
                newName: "context_data_json");

            migrationBuilder.RenameColumn(
                name: "CallTransferId",
                table: "call_handoffs",
                newName: "call_transfer_id");

            migrationBuilder.RenameColumn(
                name: "CallSessionId",
                table: "call_handoffs",
                newName: "call_session_id");

            migrationBuilder.RenameColumn(
                name: "AcceptedAt",
                table: "call_handoffs",
                newName: "accepted_at");

            migrationBuilder.RenameIndex(
                name: "IX_call_handoffs_Status",
                table: "call_handoffs",
                newName: "ix_call_handoffs_status");

            migrationBuilder.RenameIndex(
                name: "IX_call_handoffs_ToHumanAgentId",
                table: "call_handoffs",
                newName: "ix_call_handoffs_to_human_agent_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_handoffs_FromParticipantId",
                table: "call_handoffs",
                newName: "ix_call_handoffs_from_participant_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_handoffs_CallTransferId",
                table: "call_handoffs",
                newName: "ix_call_handoffs_call_transfer_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_handoffs_CallSessionId",
                table: "call_handoffs",
                newName: "ix_call_handoffs_call_session_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "call_configurations",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "call_configurations",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "call_configurations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WorkflowId",
                table: "call_configurations",
                newName: "workflow_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "call_configurations",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "call_configurations",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PersonaId",
                table: "call_configurations",
                newName: "persona_id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "call_configurations",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "call_configurations",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ConfigJson",
                table: "call_configurations",
                newName: "config_json");

            migrationBuilder.RenameIndex(
                name: "IX_call_configurations_WorkflowId",
                table: "call_configurations",
                newName: "ix_call_configurations_workflow_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_configurations_UserId",
                table: "call_configurations",
                newName: "ix_call_configurations_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_configurations_PersonaId",
                table: "call_configurations",
                newName: "ix_call_configurations_persona_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_configurations_IsActive",
                table: "call_configurations",
                newName: "ix_call_configurations_is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "call_configuration_actions",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ActionDefinitionId",
                table: "call_configuration_actions",
                newName: "action_definition_id");

            migrationBuilder.RenameColumn(
                name: "CallConfigurationId",
                table: "call_configuration_actions",
                newName: "call_configuration_id");

            migrationBuilder.RenameIndex(
                name: "IX_call_configuration_actions_ActionDefinitionId",
                table: "call_configuration_actions",
                newName: "ix_call_configuration_actions_action_definition_id");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "agent_users",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "agent_users",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "agent_users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "agent_users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "IsOnline",
                table: "agent_users",
                newName: "is_online");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "agent_users",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_agent_users_Username",
                table: "agent_users",
                newName: "ix_agent_users_username");

            migrationBuilder.RenameIndex(
                name: "IX_agent_users_Status",
                table: "agent_users",
                newName: "ix_agent_users_status");

            migrationBuilder.RenameIndex(
                name: "IX_agent_users_IsOnline",
                table: "agent_users",
                newName: "ix_agent_users_is_online");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "action_executions",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Error",
                table: "action_executions",
                newName: "error");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "action_executions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WorkflowExecutionId",
                table: "action_executions",
                newName: "workflow_execution_id");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "action_executions",
                newName: "started_at");

            migrationBuilder.RenameColumn(
                name: "OutputJson",
                table: "action_executions",
                newName: "output_json");

            migrationBuilder.RenameColumn(
                name: "InputJson",
                table: "action_executions",
                newName: "input_json");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "action_executions",
                newName: "completed_at");

            migrationBuilder.RenameColumn(
                name: "CallSessionId",
                table: "action_executions",
                newName: "call_session_id");

            migrationBuilder.RenameColumn(
                name: "ActionDefinitionId",
                table: "action_executions",
                newName: "action_definition_id");

            migrationBuilder.RenameIndex(
                name: "IX_action_executions_Status",
                table: "action_executions",
                newName: "ix_action_executions_status");

            migrationBuilder.RenameIndex(
                name: "IX_action_executions_WorkflowExecutionId",
                table: "action_executions",
                newName: "ix_action_executions_workflow_execution_id");

            migrationBuilder.RenameIndex(
                name: "IX_action_executions_CallSessionId",
                table: "action_executions",
                newName: "ix_action_executions_call_session_id");

            migrationBuilder.RenameIndex(
                name: "IX_action_executions_ActionDefinitionId",
                table: "action_executions",
                newName: "ix_action_executions_action_definition_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "action_definitions",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "action_definitions",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "action_definitions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "action_definitions",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "OutputSchemaJson",
                table: "action_definitions",
                newName: "output_schema_json");

            migrationBuilder.RenameColumn(
                name: "IsSystem",
                table: "action_definitions",
                newName: "is_system");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "action_definitions",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "InputSchemaJson",
                table: "action_definitions",
                newName: "input_schema_json");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "action_definitions",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "action_definitions",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ConfigurationJson",
                table: "action_definitions",
                newName: "configuration_json");

            migrationBuilder.RenameColumn(
                name: "ActionType",
                table: "action_definitions",
                newName: "action_type");

            migrationBuilder.RenameIndex(
                name: "IX_action_definitions_Name",
                table: "action_definitions",
                newName: "ix_action_definitions_name");

            migrationBuilder.RenameIndex(
                name: "IX_action_definitions_IsActive",
                table: "action_definitions",
                newName: "ix_action_definitions_is_active");

            migrationBuilder.RenameIndex(
                name: "IX_action_definitions_ActionType",
                table: "action_definitions",
                newName: "ix_action_definitions_action_type");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "workflows",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "workflows",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "workflows",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "workflows",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "workflows",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "workflows",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "workflows",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_workflows_UserId",
                table: "workflows",
                newName: "ix_workflows_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_workflows_IsActive",
                table: "workflows",
                newName: "ix_workflows_is_active");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "users",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "users",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "StandardCredits",
                table: "users",
                newName: "standard_credits");

            migrationBuilder.RenameColumn(
                name: "PremiumCredits",
                table: "users",
                newName: "premium_credits");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "IsPartner",
                table: "users",
                newName: "is_partner");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "users",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "DefaultPersonaId",
                table: "users",
                newName: "default_persona_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "users",
                newName: "company_name");

            migrationBuilder.RenameIndex(
                name: "IX_users_Status",
                table: "users",
                newName: "ix_users_status");

            migrationBuilder.RenameIndex(
                name: "IX_users_Email",
                table: "users",
                newName: "ix_users_email");

            migrationBuilder.RenameIndex(
                name: "IX_users_DefaultPersonaId",
                table: "users",
                newName: "ix_users_default_persona_id");

            migrationBuilder.RenameIndex(
                name: "IX_users_CreatedAt",
                table: "users",
                newName: "ix_users_created_at");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "usage_records",
                newName: "unit");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "usage_records",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "usage_records",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "usage_records",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "PartnerId",
                table: "usage_records",
                newName: "partner_id");

            migrationBuilder.RenameColumn(
                name: "OccurredAt",
                table: "usage_records",
                newName: "occurred_at");

            migrationBuilder.RenameColumn(
                name: "MetricType",
                table: "usage_records",
                newName: "metric_type");

            migrationBuilder.RenameColumn(
                name: "MetadataJson",
                table: "usage_records",
                newName: "metadata_json");

            migrationBuilder.RenameColumn(
                name: "LicenseId",
                table: "usage_records",
                newName: "license_id");

            migrationBuilder.RenameColumn(
                name: "IdempotencyKey",
                table: "usage_records",
                newName: "idempotency_key");

            migrationBuilder.RenameColumn(
                name: "CallSessionId",
                table: "usage_records",
                newName: "call_session_id");

            migrationBuilder.RenameIndex(
                name: "IX_usage_records_UserId_OccurredAt",
                table: "usage_records",
                newName: "ix_usage_records_user_id_occurred_at");

            migrationBuilder.RenameIndex(
                name: "IX_usage_records_UserId",
                table: "usage_records",
                newName: "ix_usage_records_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_usage_records_PartnerId",
                table: "usage_records",
                newName: "ix_usage_records_partner_id");

            migrationBuilder.RenameIndex(
                name: "IX_usage_records_OccurredAt",
                table: "usage_records",
                newName: "ix_usage_records_occurred_at");

            migrationBuilder.RenameIndex(
                name: "IX_usage_records_MetricType",
                table: "usage_records",
                newName: "ix_usage_records_metric_type");

            migrationBuilder.RenameIndex(
                name: "IX_usage_records_LicenseId",
                table: "usage_records",
                newName: "ix_usage_records_license_id");

            migrationBuilder.RenameIndex(
                name: "IX_usage_records_IdempotencyKey",
                table: "usage_records",
                newName: "ix_usage_records_idempotency_key");

            migrationBuilder.RenameIndex(
                name: "IX_usage_records_CallSessionId",
                table: "usage_records",
                newName: "ix_usage_records_call_session_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "subscriptions",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "subscriptions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "subscriptions",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "subscriptions",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TrialEndsAt",
                table: "subscriptions",
                newName: "trial_ends_at");

            migrationBuilder.RenameColumn(
                name: "StartsAt",
                table: "subscriptions",
                newName: "starts_at");

            migrationBuilder.RenameColumn(
                name: "PlanId",
                table: "subscriptions",
                newName: "plan_id");

            migrationBuilder.RenameColumn(
                name: "EndsAt",
                table: "subscriptions",
                newName: "ends_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "subscriptions",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_subscriptions_Status",
                table: "subscriptions",
                newName: "ix_subscriptions_status");

            migrationBuilder.RenameIndex(
                name: "IX_subscriptions_UserId",
                table: "subscriptions",
                newName: "ix_subscriptions_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_subscriptions_PlanId",
                table: "subscriptions",
                newName: "ix_subscriptions_plan_id");

            migrationBuilder.RenameColumn(
                name: "Tier",
                table: "plans",
                newName: "tier");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "plans",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "plans",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "plans",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "plans",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsPlatformPlan",
                table: "plans",
                newName: "is_platform_plan");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "plans",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "EntitlementsJson",
                table: "plans",
                newName: "entitlements_json");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "plans",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_plans_Tier",
                table: "plans",
                newName: "ix_plans_tier");

            migrationBuilder.RenameIndex(
                name: "IX_plans_IsActive",
                table: "plans",
                newName: "ix_plans_is_active");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "personas",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "personas",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "personas",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "personas",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "personas",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "personas",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "personas",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_personas_UserId",
                table: "personas",
                newName: "ix_personas_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_personas_IsActive",
                table: "personas",
                newName: "ix_personas_is_active");

            migrationBuilder.RenameColumn(
                name: "Website",
                table: "partners",
                newName: "website");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "partners",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "partners",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "partners",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "partners",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "partners",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "OrganizationName",
                table: "partners",
                newName: "organization_name");

            migrationBuilder.RenameColumn(
                name: "MetadataJson",
                table: "partners",
                newName: "metadata_json");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "partners",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "partners",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ContactEmail",
                table: "partners",
                newName: "contact_email");

            migrationBuilder.RenameIndex(
                name: "IX_partners_UserId",
                table: "partners",
                newName: "ix_partners_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_partners_OrganizationName",
                table: "partners",
                newName: "ix_partners_organization_name");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "partner_relationships",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "partner_relationships",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "partner_relationships",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "partner_relationships",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PartnerId",
                table: "partner_relationships",
                newName: "partner_id");

            migrationBuilder.RenameColumn(
                name: "MetadataJson",
                table: "partner_relationships",
                newName: "metadata_json");

            migrationBuilder.RenameColumn(
                name: "CustomerUserId",
                table: "partner_relationships",
                newName: "customer_user_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "partner_relationships",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_partner_relationships_Status",
                table: "partner_relationships",
                newName: "ix_partner_relationships_status");

            migrationBuilder.RenameIndex(
                name: "IX_partner_relationships_UserId",
                table: "partner_relationships",
                newName: "ix_partner_relationships_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_partner_relationships_PartnerId_CustomerUserId",
                table: "partner_relationships",
                newName: "ix_partner_relationships_partner_id_customer_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_partner_relationships_PartnerId",
                table: "partner_relationships",
                newName: "ix_partner_relationships_partner_id");

            migrationBuilder.RenameIndex(
                name: "IX_partner_relationships_CustomerUserId",
                table: "partner_relationships",
                newName: "ix_partner_relationships_customer_user_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "partner_plans",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "partner_plans",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "partner_plans",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "partner_plans",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PartnerId",
                table: "partner_plans",
                newName: "partner_id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "partner_plans",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "EntitlementsJson",
                table: "partner_plans",
                newName: "entitlements_json");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "partner_plans",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_partner_plans_PartnerId",
                table: "partner_plans",
                newName: "ix_partner_plans_partner_id");

            migrationBuilder.RenameIndex(
                name: "IX_partner_plans_IsActive",
                table: "partner_plans",
                newName: "ix_partner_plans_is_active");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "partner_external_customers",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PlatformUserId",
                table: "partner_external_customers",
                newName: "platform_user_id");

            migrationBuilder.RenameColumn(
                name: "PartnerId",
                table: "partner_external_customers",
                newName: "partner_id");

            migrationBuilder.RenameColumn(
                name: "ExternalCustomerId",
                table: "partner_external_customers",
                newName: "external_customer_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "partner_external_customers",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_partner_external_customers_PlatformUserId",
                table: "partner_external_customers",
                newName: "ix_partner_external_customers_platform_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_partner_external_customers_PartnerId_ExternalCustomerId",
                table: "partner_external_customers",
                newName: "ix_partner_external_customers_partner_id_external_customer_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "licenses",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "licenses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "licenses",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "licenses",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "StartsAt",
                table: "licenses",
                newName: "starts_at");

            migrationBuilder.RenameColumn(
                name: "PartnerPlanId",
                table: "licenses",
                newName: "partner_plan_id");

            migrationBuilder.RenameColumn(
                name: "PartnerId",
                table: "licenses",
                newName: "partner_id");

            migrationBuilder.RenameColumn(
                name: "MetadataJson",
                table: "licenses",
                newName: "metadata_json");

            migrationBuilder.RenameColumn(
                name: "LimitsJson",
                table: "licenses",
                newName: "limits_json");

            migrationBuilder.RenameColumn(
                name: "EndsAt",
                table: "licenses",
                newName: "ends_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "licenses",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_licenses_Status",
                table: "licenses",
                newName: "ix_licenses_status");

            migrationBuilder.RenameIndex(
                name: "IX_licenses_UserId",
                table: "licenses",
                newName: "ix_licenses_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_licenses_StartsAt",
                table: "licenses",
                newName: "ix_licenses_starts_at");

            migrationBuilder.RenameIndex(
                name: "IX_licenses_PartnerPlanId",
                table: "licenses",
                newName: "ix_licenses_partner_plan_id");

            migrationBuilder.RenameIndex(
                name: "IX_licenses_PartnerId",
                table: "licenses",
                newName: "ix_licenses_partner_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "knowledge_bases",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "knowledge_bases",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "knowledge_bases",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "knowledge_bases",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "knowledge_bases",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "knowledge_bases",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "knowledge_bases",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_knowledge_bases_UserId",
                table: "knowledge_bases",
                newName: "ix_knowledge_bases_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_knowledge_bases_IsActive",
                table: "knowledge_bases",
                newName: "ix_knowledge_bases_is_active");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "api_keys",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Scopes",
                table: "api_keys",
                newName: "scopes");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "api_keys",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "api_keys",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "api_keys",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "RevokedAt",
                table: "api_keys",
                newName: "revoked_at");

            migrationBuilder.RenameColumn(
                name: "LastUsedAt",
                table: "api_keys",
                newName: "last_used_at");

            migrationBuilder.RenameColumn(
                name: "KeyPrefix",
                table: "api_keys",
                newName: "key_prefix");

            migrationBuilder.RenameColumn(
                name: "KeyHash",
                table: "api_keys",
                newName: "key_hash");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "api_keys",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "api_keys",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_api_keys_Status",
                table: "api_keys",
                newName: "ix_api_keys_status");

            migrationBuilder.RenameIndex(
                name: "IX_api_keys_UserId",
                table: "api_keys",
                newName: "ix_api_keys_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_api_keys_KeyHash",
                table: "api_keys",
                newName: "ix_api_keys_key_hash");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:access_key_status.access_key_status", "active,revoked,expired")
                .Annotation("Npgsql:Enum:action_execution_status.action_execution_status", "pending,running,completed,failed")
                .Annotation("Npgsql:Enum:action_type.action_type", "system,workflow,integration,webhook")
                .Annotation("Npgsql:Enum:api_key_status.api_key_status", "active,revoked,expired")
                .Annotation("Npgsql:Enum:call_direction.call_direction", "inbound,outbound")
                .Annotation("Npgsql:Enum:call_session_status.call_session_status", "queued,ringing,active,transferred,completed,failed,cancelled")
                .Annotation("Npgsql:Enum:call_transfer_status.call_transfer_status", "requested,ringing,accepted,completed,rejected,failed,cancelled")
                .Annotation("Npgsql:Enum:handoff_status.handoff_status", "pending,delivered,accepted,expired")
                .Annotation("Npgsql:Enum:human_agent_status.human_agent_status", "offline,available,break,not_ready,in_call")
                .Annotation("Npgsql:Enum:license_status.license_status", "active,inactive,expired,cancelled,suspended")
                .Annotation("Npgsql:Enum:metric_type.metric_type", "call_duration,call_minutes,transfer_count,recording_minutes,agent_session_minutes")
                .Annotation("Npgsql:Enum:participant_type.participant_type", "customer,ai_agent,human_agent")
                .Annotation("Npgsql:Enum:partner_relationship_status.partner_relationship_status", "active,inactive,suspended")
                .Annotation("Npgsql:Enum:plan_tier.plan_tier", "free,starter,growth,enterprise")
                .Annotation("Npgsql:Enum:recording_status.recording_status", "pending,in_progress,completed,failed")
                .Annotation("Npgsql:Enum:subscription_status.subscription_status", "active,past_due,cancelled,expired,trialing")
                .Annotation("Npgsql:Enum:user_status.user_status", "active,inactive,suspended")
                .Annotation("Npgsql:Enum:workflow_execution_status.workflow_execution_status", "pending,running,completed,failed")
                .OldAnnotation("Npgsql:Enum:access_key_status", "active,revoked,expired")
                .OldAnnotation("Npgsql:Enum:action_execution_status", "pending,running,completed,failed")
                .OldAnnotation("Npgsql:Enum:action_type", "system,workflow,integration,webhook")
                .OldAnnotation("Npgsql:Enum:api_key_status", "active,revoked,expired")
                .OldAnnotation("Npgsql:Enum:call_direction", "inbound,outbound")
                .OldAnnotation("Npgsql:Enum:call_session_status", "queued,ringing,active,transferred,completed,failed,cancelled")
                .OldAnnotation("Npgsql:Enum:call_transfer_status", "requested,ringing,accepted,completed,rejected,failed,cancelled")
                .OldAnnotation("Npgsql:Enum:handoff_status", "pending,delivered,accepted,expired")
                .OldAnnotation("Npgsql:Enum:human_agent_status", "offline,available,break,not_ready,in_call")
                .OldAnnotation("Npgsql:Enum:license_status", "active,inactive,expired,cancelled,suspended")
                .OldAnnotation("Npgsql:Enum:metric_type", "call_duration,call_minutes,transfer_count,recording_minutes,agent_session_minutes")
                .OldAnnotation("Npgsql:Enum:participant_type", "customer,ai_agent,human_agent")
                .OldAnnotation("Npgsql:Enum:partner_relationship_status", "active,inactive,suspended")
                .OldAnnotation("Npgsql:Enum:plan_tier", "free,starter,growth,enterprise")
                .OldAnnotation("Npgsql:Enum:recording_status", "pending,in_progress,completed,failed")
                .OldAnnotation("Npgsql:Enum:subscription_status", "active,past_due,cancelled,expired,trialing")
                .OldAnnotation("Npgsql:Enum:user_status", "active,inactive,suspended")
                .OldAnnotation("Npgsql:Enum:workflow_execution_status", "pending,running,completed,failed");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "human_agents",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "human_agent_access_keys",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "call_transfers",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "call_sessions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "direction",
                table: "call_sessions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "call_recordings",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "participant_type",
                table: "call_participants",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "call_handoffs",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "action_executions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "action_type",
                table: "action_definitions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "refresh_token",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "refresh_token_expiry_time",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "metric_type",
                table: "usage_records",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "subscriptions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "tier",
                table: "plans",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "partner_relationships",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "licenses",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "api_keys",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "pk_workflow_versions",
                table: "workflow_versions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_workflow_executions",
                table: "workflow_executions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sip_destinations",
                table: "sip_destinations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sip_connections",
                table: "sip_connections",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_persona_versions",
                table: "persona_versions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_persona_knowledge_bases",
                table: "persona_knowledge_bases",
                columns: new[] { "persona_id", "knowledge_base_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_persona_actions",
                table: "persona_actions",
                columns: new[] { "persona_id", "action_definition_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_knowledge_documents",
                table: "knowledge_documents",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_knowledge_chunks",
                table: "knowledge_chunks",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_human_agents",
                table: "human_agents",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_human_agent_sessions",
                table: "human_agent_sessions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_human_agent_access_keys",
                table: "human_agent_access_keys",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_call_transfers",
                table: "call_transfers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_call_sessions",
                table: "call_sessions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_call_records",
                table: "call_records",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_call_recordings",
                table: "call_recordings",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_call_participants",
                table: "call_participants",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_call_legs",
                table: "call_legs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_call_handoffs",
                table: "call_handoffs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_call_configurations",
                table: "call_configurations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_call_configuration_actions",
                table: "call_configuration_actions",
                columns: new[] { "call_configuration_id", "action_definition_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_agent_users",
                table: "agent_users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_action_executions",
                table: "action_executions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_action_definitions",
                table: "action_definitions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_workflows",
                table: "workflows",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_usage_records",
                table: "usage_records",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_subscriptions",
                table: "subscriptions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_plans",
                table: "plans",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_personas",
                table: "personas",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_partners",
                table: "partners",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_partner_relationships",
                table: "partner_relationships",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_partner_plans",
                table: "partner_plans",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_partner_external_customers",
                table: "partner_external_customers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_licenses",
                table: "licenses",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_knowledge_bases",
                table: "knowledge_bases",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_api_keys",
                table: "api_keys",
                column: "id");

            migrationBuilder.UpdateData(
                table: "action_definitions",
                keyColumn: "id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000001"),
                column: "action_type",
                value: "system");

            migrationBuilder.UpdateData(
                table: "action_definitions",
                keyColumn: "id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000002"),
                column: "action_type",
                value: "system");

            migrationBuilder.UpdateData(
                table: "agent_users",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2026, 8, 31, 10, 54, 53, 939, DateTimeKind.Utc).AddTicks(998));

            migrationBuilder.AddForeignKey(
                name: "fk_action_executions_action_definitions_action_definition_id",
                table: "action_executions",
                column: "action_definition_id",
                principalTable: "action_definitions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_action_executions_call_sessions_call_session_id",
                table: "action_executions",
                column: "call_session_id",
                principalTable: "call_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_action_executions_workflow_executions_workflow_execution_id",
                table: "action_executions",
                column: "workflow_execution_id",
                principalTable: "workflow_executions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_api_keys_users_user_id",
                table: "api_keys",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_call_configuration_actions_action_definitions_action_defini",
                table: "call_configuration_actions",
                column: "action_definition_id",
                principalTable: "action_definitions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_call_configuration_actions_call_configurations_call_configu",
                table: "call_configuration_actions",
                column: "call_configuration_id",
                principalTable: "call_configurations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_call_configurations_personas_persona_id",
                table: "call_configurations",
                column: "persona_id",
                principalTable: "personas",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_call_configurations_users_user_id",
                table: "call_configurations",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_call_configurations_workflows_workflow_id",
                table: "call_configurations",
                column: "workflow_id",
                principalTable: "workflows",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_call_handoffs_call_participants_from_participant_id",
                table: "call_handoffs",
                column: "from_participant_id",
                principalTable: "call_participants",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_call_handoffs_call_sessions_call_session_id",
                table: "call_handoffs",
                column: "call_session_id",
                principalTable: "call_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_call_handoffs_call_transfers_call_transfer_id",
                table: "call_handoffs",
                column: "call_transfer_id",
                principalTable: "call_transfers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_call_handoffs_human_agents_to_human_agent_id",
                table: "call_handoffs",
                column: "to_human_agent_id",
                principalTable: "human_agents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_call_legs_call_sessions_call_session_id",
                table: "call_legs",
                column: "call_session_id",
                principalTable: "call_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_call_participants_call_sessions_call_session_id",
                table: "call_participants",
                column: "call_session_id",
                principalTable: "call_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_call_participants_human_agents_human_agent_id",
                table: "call_participants",
                column: "human_agent_id",
                principalTable: "human_agents",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_call_recordings_call_sessions_call_session_id",
                table: "call_recordings",
                column: "call_session_id",
                principalTable: "call_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_call_records_agent_users_handled_by_agent_id",
                table: "call_records",
                column: "handled_by_agent_id",
                principalTable: "agent_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_call_sessions_api_keys_api_key_id",
                table: "call_sessions",
                column: "api_key_id",
                principalTable: "api_keys",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_call_sessions_call_configurations_call_configuration_id",
                table: "call_sessions",
                column: "call_configuration_id",
                principalTable: "call_configurations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_call_sessions_persona_versions_persona_version_id",
                table: "call_sessions",
                column: "persona_version_id",
                principalTable: "persona_versions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_call_sessions_sip_connections_origin_sip_connection_id",
                table: "call_sessions",
                column: "origin_sip_connection_id",
                principalTable: "sip_connections",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_call_sessions_users_user_id",
                table: "call_sessions",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_call_sessions_workflow_versions_workflow_version_id",
                table: "call_sessions",
                column: "workflow_version_id",
                principalTable: "workflow_versions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_call_transfers_call_participants_from_participant_id",
                table: "call_transfers",
                column: "from_participant_id",
                principalTable: "call_participants",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_call_transfers_call_sessions_call_session_id",
                table: "call_transfers",
                column: "call_session_id",
                principalTable: "call_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_call_transfers_human_agents_to_human_agent_id",
                table: "call_transfers",
                column: "to_human_agent_id",
                principalTable: "human_agents",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_call_transfers_sip_destinations_destination_id",
                table: "call_transfers",
                column: "destination_id",
                principalTable: "sip_destinations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_human_agent_access_keys_human_agents_human_agent_id",
                table: "human_agent_access_keys",
                column: "human_agent_id",
                principalTable: "human_agents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_human_agent_sessions_human_agents_human_agent_id",
                table: "human_agent_sessions",
                column: "human_agent_id",
                principalTable: "human_agents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_human_agents_users_application_user_id",
                table: "human_agents",
                column: "application_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_human_agents_users_owner_user_id",
                table: "human_agents",
                column: "owner_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_knowledge_bases_users_user_id",
                table: "knowledge_bases",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_knowledge_chunks_knowledge_documents_knowledge_document_id",
                table: "knowledge_chunks",
                column: "knowledge_document_id",
                principalTable: "knowledge_documents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_knowledge_documents_knowledge_bases_knowledge_base_id",
                table: "knowledge_documents",
                column: "knowledge_base_id",
                principalTable: "knowledge_bases",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_licenses_partner_plans_partner_plan_id",
                table: "licenses",
                column: "partner_plan_id",
                principalTable: "partner_plans",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_licenses_partners_partner_id",
                table: "licenses",
                column: "partner_id",
                principalTable: "partners",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_licenses_users_user_id",
                table: "licenses",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_partner_external_customers_partners_partner_id",
                table: "partner_external_customers",
                column: "partner_id",
                principalTable: "partners",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_partner_external_customers_users_platform_user_id",
                table: "partner_external_customers",
                column: "platform_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_partner_plans_partners_partner_id",
                table: "partner_plans",
                column: "partner_id",
                principalTable: "partners",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_partner_relationships_partners_partner_id",
                table: "partner_relationships",
                column: "partner_id",
                principalTable: "partners",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_partner_relationships_users_customer_user_id",
                table: "partner_relationships",
                column: "customer_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_partner_relationships_users_user_id",
                table: "partner_relationships",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_partners_users_user_id",
                table: "partners",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_persona_actions_action_definitions_action_definition_id",
                table: "persona_actions",
                column: "action_definition_id",
                principalTable: "action_definitions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_persona_actions_personas_persona_id",
                table: "persona_actions",
                column: "persona_id",
                principalTable: "personas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_persona_knowledge_bases_knowledge_bases_knowledge_base_id",
                table: "persona_knowledge_bases",
                column: "knowledge_base_id",
                principalTable: "knowledge_bases",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_persona_knowledge_bases_personas_persona_id",
                table: "persona_knowledge_bases",
                column: "persona_id",
                principalTable: "personas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_persona_versions_personas_persona_id",
                table: "persona_versions",
                column: "persona_id",
                principalTable: "personas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_personas_users_user_id",
                table: "personas",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sip_connections_users_user_id",
                table: "sip_connections",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sip_destinations_users_user_id",
                table: "sip_destinations",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_subscriptions_plans_plan_id",
                table: "subscriptions",
                column: "plan_id",
                principalTable: "plans",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_subscriptions_users_user_id",
                table: "subscriptions",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_usage_records_call_sessions_call_session_id",
                table: "usage_records",
                column: "call_session_id",
                principalTable: "call_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_usage_records_licenses_license_id",
                table: "usage_records",
                column: "license_id",
                principalTable: "licenses",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_usage_records_partners_partner_id",
                table: "usage_records",
                column: "partner_id",
                principalTable: "partners",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_usage_records_users_user_id",
                table: "usage_records",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_users_personas_default_persona_id",
                table: "users",
                column: "default_persona_id",
                principalTable: "personas",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_workflow_executions_call_sessions_call_session_id",
                table: "workflow_executions",
                column: "call_session_id",
                principalTable: "call_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_workflow_executions_workflow_versions_workflow_version_id",
                table: "workflow_executions",
                column: "workflow_version_id",
                principalTable: "workflow_versions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_workflow_versions_workflows_workflow_id",
                table: "workflow_versions",
                column: "workflow_id",
                principalTable: "workflows",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_workflows_users_user_id",
                table: "workflows",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_action_executions_action_definitions_action_definition_id",
                table: "action_executions");

            migrationBuilder.DropForeignKey(
                name: "fk_action_executions_call_sessions_call_session_id",
                table: "action_executions");

            migrationBuilder.DropForeignKey(
                name: "fk_action_executions_workflow_executions_workflow_execution_id",
                table: "action_executions");

            migrationBuilder.DropForeignKey(
                name: "fk_api_keys_users_user_id",
                table: "api_keys");

            migrationBuilder.DropForeignKey(
                name: "fk_call_configuration_actions_action_definitions_action_defini",
                table: "call_configuration_actions");

            migrationBuilder.DropForeignKey(
                name: "fk_call_configuration_actions_call_configurations_call_configu",
                table: "call_configuration_actions");

            migrationBuilder.DropForeignKey(
                name: "fk_call_configurations_personas_persona_id",
                table: "call_configurations");

            migrationBuilder.DropForeignKey(
                name: "fk_call_configurations_users_user_id",
                table: "call_configurations");

            migrationBuilder.DropForeignKey(
                name: "fk_call_configurations_workflows_workflow_id",
                table: "call_configurations");

            migrationBuilder.DropForeignKey(
                name: "fk_call_handoffs_call_participants_from_participant_id",
                table: "call_handoffs");

            migrationBuilder.DropForeignKey(
                name: "fk_call_handoffs_call_sessions_call_session_id",
                table: "call_handoffs");

            migrationBuilder.DropForeignKey(
                name: "fk_call_handoffs_call_transfers_call_transfer_id",
                table: "call_handoffs");

            migrationBuilder.DropForeignKey(
                name: "fk_call_handoffs_human_agents_to_human_agent_id",
                table: "call_handoffs");

            migrationBuilder.DropForeignKey(
                name: "fk_call_legs_call_sessions_call_session_id",
                table: "call_legs");

            migrationBuilder.DropForeignKey(
                name: "fk_call_participants_call_sessions_call_session_id",
                table: "call_participants");

            migrationBuilder.DropForeignKey(
                name: "fk_call_participants_human_agents_human_agent_id",
                table: "call_participants");

            migrationBuilder.DropForeignKey(
                name: "fk_call_recordings_call_sessions_call_session_id",
                table: "call_recordings");

            migrationBuilder.DropForeignKey(
                name: "fk_call_records_agent_users_handled_by_agent_id",
                table: "call_records");

            migrationBuilder.DropForeignKey(
                name: "fk_call_sessions_api_keys_api_key_id",
                table: "call_sessions");

            migrationBuilder.DropForeignKey(
                name: "fk_call_sessions_call_configurations_call_configuration_id",
                table: "call_sessions");

            migrationBuilder.DropForeignKey(
                name: "fk_call_sessions_persona_versions_persona_version_id",
                table: "call_sessions");

            migrationBuilder.DropForeignKey(
                name: "fk_call_sessions_sip_connections_origin_sip_connection_id",
                table: "call_sessions");

            migrationBuilder.DropForeignKey(
                name: "fk_call_sessions_users_user_id",
                table: "call_sessions");

            migrationBuilder.DropForeignKey(
                name: "fk_call_sessions_workflow_versions_workflow_version_id",
                table: "call_sessions");

            migrationBuilder.DropForeignKey(
                name: "fk_call_transfers_call_participants_from_participant_id",
                table: "call_transfers");

            migrationBuilder.DropForeignKey(
                name: "fk_call_transfers_call_sessions_call_session_id",
                table: "call_transfers");

            migrationBuilder.DropForeignKey(
                name: "fk_call_transfers_human_agents_to_human_agent_id",
                table: "call_transfers");

            migrationBuilder.DropForeignKey(
                name: "fk_call_transfers_sip_destinations_destination_id",
                table: "call_transfers");

            migrationBuilder.DropForeignKey(
                name: "fk_human_agent_access_keys_human_agents_human_agent_id",
                table: "human_agent_access_keys");

            migrationBuilder.DropForeignKey(
                name: "fk_human_agent_sessions_human_agents_human_agent_id",
                table: "human_agent_sessions");

            migrationBuilder.DropForeignKey(
                name: "fk_human_agents_users_application_user_id",
                table: "human_agents");

            migrationBuilder.DropForeignKey(
                name: "fk_human_agents_users_owner_user_id",
                table: "human_agents");

            migrationBuilder.DropForeignKey(
                name: "fk_knowledge_bases_users_user_id",
                table: "knowledge_bases");

            migrationBuilder.DropForeignKey(
                name: "fk_knowledge_chunks_knowledge_documents_knowledge_document_id",
                table: "knowledge_chunks");

            migrationBuilder.DropForeignKey(
                name: "fk_knowledge_documents_knowledge_bases_knowledge_base_id",
                table: "knowledge_documents");

            migrationBuilder.DropForeignKey(
                name: "fk_licenses_partner_plans_partner_plan_id",
                table: "licenses");

            migrationBuilder.DropForeignKey(
                name: "fk_licenses_partners_partner_id",
                table: "licenses");

            migrationBuilder.DropForeignKey(
                name: "fk_licenses_users_user_id",
                table: "licenses");

            migrationBuilder.DropForeignKey(
                name: "fk_partner_external_customers_partners_partner_id",
                table: "partner_external_customers");

            migrationBuilder.DropForeignKey(
                name: "fk_partner_external_customers_users_platform_user_id",
                table: "partner_external_customers");

            migrationBuilder.DropForeignKey(
                name: "fk_partner_plans_partners_partner_id",
                table: "partner_plans");

            migrationBuilder.DropForeignKey(
                name: "fk_partner_relationships_partners_partner_id",
                table: "partner_relationships");

            migrationBuilder.DropForeignKey(
                name: "fk_partner_relationships_users_customer_user_id",
                table: "partner_relationships");

            migrationBuilder.DropForeignKey(
                name: "fk_partner_relationships_users_user_id",
                table: "partner_relationships");

            migrationBuilder.DropForeignKey(
                name: "fk_partners_users_user_id",
                table: "partners");

            migrationBuilder.DropForeignKey(
                name: "fk_persona_actions_action_definitions_action_definition_id",
                table: "persona_actions");

            migrationBuilder.DropForeignKey(
                name: "fk_persona_actions_personas_persona_id",
                table: "persona_actions");

            migrationBuilder.DropForeignKey(
                name: "fk_persona_knowledge_bases_knowledge_bases_knowledge_base_id",
                table: "persona_knowledge_bases");

            migrationBuilder.DropForeignKey(
                name: "fk_persona_knowledge_bases_personas_persona_id",
                table: "persona_knowledge_bases");

            migrationBuilder.DropForeignKey(
                name: "fk_persona_versions_personas_persona_id",
                table: "persona_versions");

            migrationBuilder.DropForeignKey(
                name: "fk_personas_users_user_id",
                table: "personas");

            migrationBuilder.DropForeignKey(
                name: "fk_sip_connections_users_user_id",
                table: "sip_connections");

            migrationBuilder.DropForeignKey(
                name: "fk_sip_destinations_users_user_id",
                table: "sip_destinations");

            migrationBuilder.DropForeignKey(
                name: "fk_subscriptions_plans_plan_id",
                table: "subscriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_subscriptions_users_user_id",
                table: "subscriptions");

            migrationBuilder.DropForeignKey(
                name: "fk_usage_records_call_sessions_call_session_id",
                table: "usage_records");

            migrationBuilder.DropForeignKey(
                name: "fk_usage_records_licenses_license_id",
                table: "usage_records");

            migrationBuilder.DropForeignKey(
                name: "fk_usage_records_partners_partner_id",
                table: "usage_records");

            migrationBuilder.DropForeignKey(
                name: "fk_usage_records_users_user_id",
                table: "usage_records");

            migrationBuilder.DropForeignKey(
                name: "fk_users_personas_default_persona_id",
                table: "users");

            migrationBuilder.DropForeignKey(
                name: "fk_workflow_executions_call_sessions_call_session_id",
                table: "workflow_executions");

            migrationBuilder.DropForeignKey(
                name: "fk_workflow_executions_workflow_versions_workflow_version_id",
                table: "workflow_executions");

            migrationBuilder.DropForeignKey(
                name: "fk_workflow_versions_workflows_workflow_id",
                table: "workflow_versions");

            migrationBuilder.DropForeignKey(
                name: "fk_workflows_users_user_id",
                table: "workflows");

            migrationBuilder.DropPrimaryKey(
                name: "pk_workflow_versions",
                table: "workflow_versions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_workflow_executions",
                table: "workflow_executions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sip_destinations",
                table: "sip_destinations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sip_connections",
                table: "sip_connections");

            migrationBuilder.DropPrimaryKey(
                name: "pk_persona_versions",
                table: "persona_versions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_persona_knowledge_bases",
                table: "persona_knowledge_bases");

            migrationBuilder.DropPrimaryKey(
                name: "pk_persona_actions",
                table: "persona_actions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_knowledge_documents",
                table: "knowledge_documents");

            migrationBuilder.DropPrimaryKey(
                name: "pk_knowledge_chunks",
                table: "knowledge_chunks");

            migrationBuilder.DropPrimaryKey(
                name: "pk_human_agents",
                table: "human_agents");

            migrationBuilder.DropPrimaryKey(
                name: "pk_human_agent_sessions",
                table: "human_agent_sessions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_human_agent_access_keys",
                table: "human_agent_access_keys");

            migrationBuilder.DropPrimaryKey(
                name: "pk_call_transfers",
                table: "call_transfers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_call_sessions",
                table: "call_sessions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_call_records",
                table: "call_records");

            migrationBuilder.DropPrimaryKey(
                name: "pk_call_recordings",
                table: "call_recordings");

            migrationBuilder.DropPrimaryKey(
                name: "pk_call_participants",
                table: "call_participants");

            migrationBuilder.DropPrimaryKey(
                name: "pk_call_legs",
                table: "call_legs");

            migrationBuilder.DropPrimaryKey(
                name: "pk_call_handoffs",
                table: "call_handoffs");

            migrationBuilder.DropPrimaryKey(
                name: "pk_call_configurations",
                table: "call_configurations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_call_configuration_actions",
                table: "call_configuration_actions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_agent_users",
                table: "agent_users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_action_executions",
                table: "action_executions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_action_definitions",
                table: "action_definitions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_workflows",
                table: "workflows");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_usage_records",
                table: "usage_records");

            migrationBuilder.DropPrimaryKey(
                name: "pk_subscriptions",
                table: "subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_plans",
                table: "plans");

            migrationBuilder.DropPrimaryKey(
                name: "pk_personas",
                table: "personas");

            migrationBuilder.DropPrimaryKey(
                name: "pk_partners",
                table: "partners");

            migrationBuilder.DropPrimaryKey(
                name: "pk_partner_relationships",
                table: "partner_relationships");

            migrationBuilder.DropPrimaryKey(
                name: "pk_partner_plans",
                table: "partner_plans");

            migrationBuilder.DropPrimaryKey(
                name: "pk_partner_external_customers",
                table: "partner_external_customers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_licenses",
                table: "licenses");

            migrationBuilder.DropPrimaryKey(
                name: "pk_knowledge_bases",
                table: "knowledge_bases");

            migrationBuilder.DropPrimaryKey(
                name: "pk_api_keys",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "refresh_token",
                table: "users");

            migrationBuilder.DropColumn(
                name: "refresh_token_expiry_time",
                table: "users");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "configuration");

            migrationBuilder.EnsureSchema(
                name: "billing");

            migrationBuilder.RenameTable(
                name: "workflows",
                newName: "workflows",
                newSchema: "configuration");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "users",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "usage_records",
                newName: "usage_records",
                newSchema: "billing");

            migrationBuilder.RenameTable(
                name: "subscriptions",
                newName: "subscriptions",
                newSchema: "billing");

            migrationBuilder.RenameTable(
                name: "plans",
                newName: "plans",
                newSchema: "billing");

            migrationBuilder.RenameTable(
                name: "personas",
                newName: "personas",
                newSchema: "configuration");

            migrationBuilder.RenameTable(
                name: "partners",
                newName: "partners",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "partner_relationships",
                newName: "partner_relationships",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "partner_plans",
                newName: "partner_plans",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "partner_external_customers",
                newName: "partner_external_customers",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "licenses",
                newName: "licenses",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "knowledge_bases",
                newName: "knowledge_bases",
                newSchema: "configuration");

            migrationBuilder.RenameTable(
                name: "api_keys",
                newName: "api_keys",
                newSchema: "identity");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "workflow_versions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "workflow_id",
                table: "workflow_versions",
                newName: "WorkflowId");

            migrationBuilder.RenameColumn(
                name: "version_number",
                table: "workflow_versions",
                newName: "VersionNumber");

            migrationBuilder.RenameColumn(
                name: "is_published",
                table: "workflow_versions",
                newName: "IsPublished");

            migrationBuilder.RenameColumn(
                name: "definition_json",
                table: "workflow_versions",
                newName: "DefinitionJson");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "workflow_versions",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_workflow_versions_workflow_id_version_number",
                table: "workflow_versions",
                newName: "IX_workflow_versions_WorkflowId_VersionNumber");

            migrationBuilder.RenameIndex(
                name: "ix_workflow_versions_workflow_id",
                table: "workflow_versions",
                newName: "IX_workflow_versions_WorkflowId");

            migrationBuilder.RenameIndex(
                name: "ix_workflow_versions_is_published",
                table: "workflow_versions",
                newName: "IX_workflow_versions_IsPublished");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "workflow_executions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "error",
                table: "workflow_executions",
                newName: "Error");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "workflow_executions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "workflow_version_id",
                table: "workflow_executions",
                newName: "WorkflowVersionId");

            migrationBuilder.RenameColumn(
                name: "state_json",
                table: "workflow_executions",
                newName: "StateJson");

            migrationBuilder.RenameColumn(
                name: "started_at",
                table: "workflow_executions",
                newName: "StartedAt");

            migrationBuilder.RenameColumn(
                name: "output_json",
                table: "workflow_executions",
                newName: "OutputJson");

            migrationBuilder.RenameColumn(
                name: "input_json",
                table: "workflow_executions",
                newName: "InputJson");

            migrationBuilder.RenameColumn(
                name: "completed_at",
                table: "workflow_executions",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "call_session_id",
                table: "workflow_executions",
                newName: "CallSessionId");

            migrationBuilder.RenameIndex(
                name: "ix_workflow_executions_status",
                table: "workflow_executions",
                newName: "IX_workflow_executions_Status");

            migrationBuilder.RenameIndex(
                name: "ix_workflow_executions_workflow_version_id",
                table: "workflow_executions",
                newName: "IX_workflow_executions_WorkflowVersionId");

            migrationBuilder.RenameIndex(
                name: "ix_workflow_executions_call_session_id",
                table: "workflow_executions",
                newName: "IX_workflow_executions_CallSessionId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "sip_destinations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "sip_destinations",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "sip_destinations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "sip_destinations",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "sip_destinations",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_enabled",
                table: "sip_destinations",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "sip_destinations",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "call_to",
                table: "sip_destinations",
                newName: "CallTo");

            migrationBuilder.RenameIndex(
                name: "ix_sip_destinations_user_id_name",
                table: "sip_destinations",
                newName: "IX_sip_destinations_UserId_Name");

            migrationBuilder.RenameIndex(
                name: "ix_sip_destinations_is_enabled",
                table: "sip_destinations",
                newName: "IX_sip_destinations_IsEnabled");

            migrationBuilder.RenameColumn(
                name: "numbers",
                table: "sip_connections",
                newName: "Numbers");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "sip_connections",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "sip_connections",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "sip_connections",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "sip_connections",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "max_concurrent_calls",
                table: "sip_connections",
                newName: "MaxConcurrentCalls");

            migrationBuilder.RenameColumn(
                name: "lk_trunk_id",
                table: "sip_connections",
                newName: "LkTrunkId");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "sip_connections",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "dispatch_rule_id",
                table: "sip_connections",
                newName: "DispatchRuleId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "sip_connections",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "allowed_ips",
                table: "sip_connections",
                newName: "AllowedIps");

            migrationBuilder.RenameIndex(
                name: "ix_sip_connections_user_id_name",
                table: "sip_connections",
                newName: "IX_sip_connections_UserId_Name");

            migrationBuilder.RenameIndex(
                name: "ix_sip_connections_lk_trunk_id",
                table: "sip_connections",
                newName: "IX_sip_connections_LkTrunkId");

            migrationBuilder.RenameIndex(
                name: "ix_sip_connections_is_active",
                table: "sip_connections",
                newName: "IX_sip_connections_IsActive");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "persona_versions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "version_number",
                table: "persona_versions",
                newName: "VersionNumber");

            migrationBuilder.RenameColumn(
                name: "system_prompt",
                table: "persona_versions",
                newName: "SystemPrompt");

            migrationBuilder.RenameColumn(
                name: "persona_id",
                table: "persona_versions",
                newName: "PersonaId");

            migrationBuilder.RenameColumn(
                name: "is_published",
                table: "persona_versions",
                newName: "IsPublished");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "persona_versions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "configuration_json",
                table: "persona_versions",
                newName: "ConfigurationJson");

            migrationBuilder.RenameIndex(
                name: "ix_persona_versions_persona_id_version_number",
                table: "persona_versions",
                newName: "IX_persona_versions_PersonaId_VersionNumber");

            migrationBuilder.RenameIndex(
                name: "ix_persona_versions_persona_id",
                table: "persona_versions",
                newName: "IX_persona_versions_PersonaId");

            migrationBuilder.RenameIndex(
                name: "ix_persona_versions_is_published",
                table: "persona_versions",
                newName: "IX_persona_versions_IsPublished");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "persona_knowledge_bases",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "knowledge_base_id",
                table: "persona_knowledge_bases",
                newName: "KnowledgeBaseId");

            migrationBuilder.RenameColumn(
                name: "persona_id",
                table: "persona_knowledge_bases",
                newName: "PersonaId");

            migrationBuilder.RenameIndex(
                name: "ix_persona_knowledge_bases_knowledge_base_id",
                table: "persona_knowledge_bases",
                newName: "IX_persona_knowledge_bases_KnowledgeBaseId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "persona_actions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "action_definition_id",
                table: "persona_actions",
                newName: "ActionDefinitionId");

            migrationBuilder.RenameColumn(
                name: "persona_id",
                table: "persona_actions",
                newName: "PersonaId");

            migrationBuilder.RenameIndex(
                name: "ix_persona_actions_action_definition_id",
                table: "persona_actions",
                newName: "IX_persona_actions_ActionDefinitionId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "knowledge_documents",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "knowledge_documents",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "knowledge_documents",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "knowledge_documents",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "source_uri",
                table: "knowledge_documents",
                newName: "SourceUri");

            migrationBuilder.RenameColumn(
                name: "metadata_json",
                table: "knowledge_documents",
                newName: "MetadataJson");

            migrationBuilder.RenameColumn(
                name: "knowledge_base_id",
                table: "knowledge_documents",
                newName: "KnowledgeBaseId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "knowledge_documents",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "content_type",
                table: "knowledge_documents",
                newName: "ContentType");

            migrationBuilder.RenameIndex(
                name: "ix_knowledge_documents_status",
                table: "knowledge_documents",
                newName: "IX_knowledge_documents_Status");

            migrationBuilder.RenameIndex(
                name: "ix_knowledge_documents_knowledge_base_id",
                table: "knowledge_documents",
                newName: "IX_knowledge_documents_KnowledgeBaseId");

            migrationBuilder.RenameColumn(
                name: "embedding",
                table: "knowledge_chunks",
                newName: "Embedding");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "knowledge_chunks",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "knowledge_chunks",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "metadata_json",
                table: "knowledge_chunks",
                newName: "MetadataJson");

            migrationBuilder.RenameColumn(
                name: "knowledge_document_id",
                table: "knowledge_chunks",
                newName: "KnowledgeDocumentId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "knowledge_chunks",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "chunk_index",
                table: "knowledge_chunks",
                newName: "ChunkIndex");

            migrationBuilder.RenameIndex(
                name: "ix_knowledge_chunks_knowledge_document_id_chunk_index",
                table: "knowledge_chunks",
                newName: "IX_knowledge_chunks_KnowledgeDocumentId_ChunkIndex");

            migrationBuilder.RenameIndex(
                name: "ix_knowledge_chunks_knowledge_document_id",
                table: "knowledge_chunks",
                newName: "IX_knowledge_chunks_KnowledgeDocumentId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "human_agents",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "human_agents",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "human_agents",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "human_agents",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "human_agents",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "owner_user_id",
                table: "human_agents",
                newName: "OwnerUserId");

            migrationBuilder.RenameColumn(
                name: "max_concurrent_calls",
                table: "human_agents",
                newName: "MaxConcurrentCalls");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "human_agents",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "human_agents",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "application_user_id",
                table: "human_agents",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "ix_human_agents_status",
                table: "human_agents",
                newName: "IX_human_agents_Status");

            migrationBuilder.RenameIndex(
                name: "ix_human_agents_owner_user_id",
                table: "human_agents",
                newName: "IX_human_agents_OwnerUserId");

            migrationBuilder.RenameIndex(
                name: "ix_human_agents_is_active",
                table: "human_agents",
                newName: "IX_human_agents_IsActive");

            migrationBuilder.RenameIndex(
                name: "ix_human_agents_application_user_id",
                table: "human_agents",
                newName: "IX_human_agents_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "human_agent_sessions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "human_agent_sessions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "metadata_json",
                table: "human_agent_sessions",
                newName: "MetadataJson");

            migrationBuilder.RenameColumn(
                name: "livekit_identity",
                table: "human_agent_sessions",
                newName: "LivekitIdentity");

            migrationBuilder.RenameColumn(
                name: "last_heartbeat_at",
                table: "human_agent_sessions",
                newName: "LastHeartbeatAt");

            migrationBuilder.RenameColumn(
                name: "human_agent_id",
                table: "human_agent_sessions",
                newName: "HumanAgentId");

            migrationBuilder.RenameColumn(
                name: "disconnected_at",
                table: "human_agent_sessions",
                newName: "DisconnectedAt");

            migrationBuilder.RenameColumn(
                name: "connected_at",
                table: "human_agent_sessions",
                newName: "ConnectedAt");

            migrationBuilder.RenameIndex(
                name: "ix_human_agent_sessions_status",
                table: "human_agent_sessions",
                newName: "IX_human_agent_sessions_Status");

            migrationBuilder.RenameIndex(
                name: "ix_human_agent_sessions_human_agent_id",
                table: "human_agent_sessions",
                newName: "IX_human_agent_sessions_HumanAgentId");

            migrationBuilder.RenameIndex(
                name: "ix_human_agent_sessions_connected_at",
                table: "human_agent_sessions",
                newName: "IX_human_agent_sessions_ConnectedAt");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "human_agent_access_keys",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "human_agent_access_keys",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "human_agent_access_keys",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "revoked_at",
                table: "human_agent_access_keys",
                newName: "RevokedAt");

            migrationBuilder.RenameColumn(
                name: "last_used_at",
                table: "human_agent_access_keys",
                newName: "LastUsedAt");

            migrationBuilder.RenameColumn(
                name: "key_prefix",
                table: "human_agent_access_keys",
                newName: "KeyPrefix");

            migrationBuilder.RenameColumn(
                name: "key_hash",
                table: "human_agent_access_keys",
                newName: "KeyHash");

            migrationBuilder.RenameColumn(
                name: "human_agent_id",
                table: "human_agent_access_keys",
                newName: "HumanAgentId");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "human_agent_access_keys",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "human_agent_access_keys",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_human_agent_access_keys_status",
                table: "human_agent_access_keys",
                newName: "IX_human_agent_access_keys_Status");

            migrationBuilder.RenameIndex(
                name: "ix_human_agent_access_keys_key_hash",
                table: "human_agent_access_keys",
                newName: "IX_human_agent_access_keys_KeyHash");

            migrationBuilder.RenameIndex(
                name: "ix_human_agent_access_keys_human_agent_id",
                table: "human_agent_access_keys",
                newName: "IX_human_agent_access_keys_HumanAgentId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "call_transfers",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "reason",
                table: "call_transfers",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "mode",
                table: "call_transfers",
                newName: "Mode");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "call_transfers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "call_transfers",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "to_human_agent_id",
                table: "call_transfers",
                newName: "ToHumanAgentId");

            migrationBuilder.RenameColumn(
                name: "target_type",
                table: "call_transfers",
                newName: "TargetType");

            migrationBuilder.RenameColumn(
                name: "target_snapshot_json",
                table: "call_transfers",
                newName: "TargetSnapshotJson");

            migrationBuilder.RenameColumn(
                name: "requested_at",
                table: "call_transfers",
                newName: "RequestedAt");

            migrationBuilder.RenameColumn(
                name: "from_participant_id",
                table: "call_transfers",
                newName: "FromParticipantId");

            migrationBuilder.RenameColumn(
                name: "failure_reason",
                table: "call_transfers",
                newName: "FailureReason");

            migrationBuilder.RenameColumn(
                name: "failed_at",
                table: "call_transfers",
                newName: "FailedAt");

            migrationBuilder.RenameColumn(
                name: "destination_id",
                table: "call_transfers",
                newName: "DestinationId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "call_transfers",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "completed_at",
                table: "call_transfers",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "call_session_id",
                table: "call_transfers",
                newName: "CallSessionId");

            migrationBuilder.RenameColumn(
                name: "accepted_at",
                table: "call_transfers",
                newName: "AcceptedAt");

            migrationBuilder.RenameIndex(
                name: "ix_call_transfers_status",
                table: "call_transfers",
                newName: "IX_call_transfers_Status");

            migrationBuilder.RenameIndex(
                name: "ix_call_transfers_to_human_agent_id",
                table: "call_transfers",
                newName: "IX_call_transfers_ToHumanAgentId");

            migrationBuilder.RenameIndex(
                name: "ix_call_transfers_requested_at",
                table: "call_transfers",
                newName: "IX_call_transfers_RequestedAt");

            migrationBuilder.RenameIndex(
                name: "ix_call_transfers_from_participant_id",
                table: "call_transfers",
                newName: "IX_call_transfers_FromParticipantId");

            migrationBuilder.RenameIndex(
                name: "ix_call_transfers_destination_id",
                table: "call_transfers",
                newName: "IX_call_transfers_DestinationId");

            migrationBuilder.RenameIndex(
                name: "ix_call_transfers_call_session_id",
                table: "call_transfers",
                newName: "IX_call_transfers_CallSessionId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "call_sessions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "direction",
                table: "call_sessions",
                newName: "Direction");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "call_sessions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "workflow_version_id",
                table: "call_sessions",
                newName: "WorkflowVersionId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "call_sessions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "started_at",
                table: "call_sessions",
                newName: "StartedAt");

            migrationBuilder.RenameColumn(
                name: "persona_version_id",
                table: "call_sessions",
                newName: "PersonaVersionId");

            migrationBuilder.RenameColumn(
                name: "origin_sip_connection_id",
                table: "call_sessions",
                newName: "OriginSipConnectionId");

            migrationBuilder.RenameColumn(
                name: "metadata_json",
                table: "call_sessions",
                newName: "MetadataJson");

            migrationBuilder.RenameColumn(
                name: "livekit_room_sid",
                table: "call_sessions",
                newName: "LivekitRoomSid");

            migrationBuilder.RenameColumn(
                name: "livekit_room_name",
                table: "call_sessions",
                newName: "LivekitRoomName");

            migrationBuilder.RenameColumn(
                name: "ended_at",
                table: "call_sessions",
                newName: "EndedAt");

            migrationBuilder.RenameColumn(
                name: "duration_seconds",
                table: "call_sessions",
                newName: "DurationSeconds");

            migrationBuilder.RenameColumn(
                name: "dialed_number",
                table: "call_sessions",
                newName: "DialedNumber");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "call_sessions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "call_configuration_id",
                table: "call_sessions",
                newName: "CallConfigurationId");

            migrationBuilder.RenameColumn(
                name: "api_key_id",
                table: "call_sessions",
                newName: "ApiKeyId");

            migrationBuilder.RenameColumn(
                name: "answered_at",
                table: "call_sessions",
                newName: "AnsweredAt");

            migrationBuilder.RenameIndex(
                name: "ix_call_sessions_status",
                table: "call_sessions",
                newName: "IX_call_sessions_Status");

            migrationBuilder.RenameIndex(
                name: "ix_call_sessions_workflow_version_id",
                table: "call_sessions",
                newName: "IX_call_sessions_WorkflowVersionId");

            migrationBuilder.RenameIndex(
                name: "ix_call_sessions_user_id_started_at",
                table: "call_sessions",
                newName: "IX_call_sessions_UserId_StartedAt");

            migrationBuilder.RenameIndex(
                name: "ix_call_sessions_user_id",
                table: "call_sessions",
                newName: "IX_call_sessions_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_call_sessions_started_at",
                table: "call_sessions",
                newName: "IX_call_sessions_StartedAt");

            migrationBuilder.RenameIndex(
                name: "ix_call_sessions_persona_version_id",
                table: "call_sessions",
                newName: "IX_call_sessions_PersonaVersionId");

            migrationBuilder.RenameIndex(
                name: "ix_call_sessions_origin_sip_connection_id",
                table: "call_sessions",
                newName: "IX_call_sessions_OriginSipConnectionId");

            migrationBuilder.RenameIndex(
                name: "ix_call_sessions_livekit_room_name",
                table: "call_sessions",
                newName: "IX_call_sessions_LivekitRoomName");

            migrationBuilder.RenameIndex(
                name: "ix_call_sessions_call_configuration_id",
                table: "call_sessions",
                newName: "IX_call_sessions_CallConfigurationId");

            migrationBuilder.RenameIndex(
                name: "ix_call_sessions_api_key_id",
                table: "call_sessions",
                newName: "IX_call_sessions_ApiKeyId");

            migrationBuilder.RenameColumn(
                name: "summary",
                table: "call_records",
                newName: "Summary");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "call_records",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "call_records",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "start_time",
                table: "call_records",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "room_name",
                table: "call_records",
                newName: "RoomName");

            migrationBuilder.RenameColumn(
                name: "recording_url",
                table: "call_records",
                newName: "RecordingUrl");

            migrationBuilder.RenameColumn(
                name: "handled_by_agent_id",
                table: "call_records",
                newName: "HandledByAgentId");

            migrationBuilder.RenameColumn(
                name: "end_time",
                table: "call_records",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "caller_id",
                table: "call_records",
                newName: "CallerId");

            migrationBuilder.RenameIndex(
                name: "ix_call_records_status",
                table: "call_records",
                newName: "IX_call_records_Status");

            migrationBuilder.RenameIndex(
                name: "ix_call_records_start_time",
                table: "call_records",
                newName: "IX_call_records_StartTime");

            migrationBuilder.RenameIndex(
                name: "ix_call_records_room_name",
                table: "call_records",
                newName: "IX_call_records_RoomName");

            migrationBuilder.RenameIndex(
                name: "ix_call_records_handled_by_agent_id",
                table: "call_records",
                newName: "IX_call_records_HandledByAgentId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "call_recordings",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "call_recordings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "storage_provider",
                table: "call_recordings",
                newName: "StorageProvider");

            migrationBuilder.RenameColumn(
                name: "size_bytes",
                table: "call_recordings",
                newName: "SizeBytes");

            migrationBuilder.RenameColumn(
                name: "object_key",
                table: "call_recordings",
                newName: "ObjectKey");

            migrationBuilder.RenameColumn(
                name: "duration_seconds",
                table: "call_recordings",
                newName: "DurationSeconds");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "call_recordings",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "content_type",
                table: "call_recordings",
                newName: "ContentType");

            migrationBuilder.RenameColumn(
                name: "completed_at",
                table: "call_recordings",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "call_session_id",
                table: "call_recordings",
                newName: "CallSessionId");

            migrationBuilder.RenameIndex(
                name: "ix_call_recordings_status",
                table: "call_recordings",
                newName: "IX_call_recordings_Status");

            migrationBuilder.RenameIndex(
                name: "ix_call_recordings_object_key",
                table: "call_recordings",
                newName: "IX_call_recordings_ObjectKey");

            migrationBuilder.RenameIndex(
                name: "ix_call_recordings_call_session_id",
                table: "call_recordings",
                newName: "IX_call_recordings_CallSessionId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "call_participants",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "participant_type",
                table: "call_participants",
                newName: "ParticipantType");

            migrationBuilder.RenameColumn(
                name: "livekit_participant_sid",
                table: "call_participants",
                newName: "LivekitParticipantSid");

            migrationBuilder.RenameColumn(
                name: "livekit_identity",
                table: "call_participants",
                newName: "LivekitIdentity");

            migrationBuilder.RenameColumn(
                name: "left_at",
                table: "call_participants",
                newName: "LeftAt");

            migrationBuilder.RenameColumn(
                name: "joined_at",
                table: "call_participants",
                newName: "JoinedAt");

            migrationBuilder.RenameColumn(
                name: "human_agent_id",
                table: "call_participants",
                newName: "HumanAgentId");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "call_participants",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "call_participants",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "call_session_id",
                table: "call_participants",
                newName: "CallSessionId");

            migrationBuilder.RenameIndex(
                name: "ix_call_participants_participant_type",
                table: "call_participants",
                newName: "IX_call_participants_ParticipantType");

            migrationBuilder.RenameIndex(
                name: "ix_call_participants_human_agent_id",
                table: "call_participants",
                newName: "IX_call_participants_HumanAgentId");

            migrationBuilder.RenameIndex(
                name: "ix_call_participants_call_session_id",
                table: "call_participants",
                newName: "IX_call_participants_CallSessionId");

            migrationBuilder.RenameColumn(
                name: "kind",
                table: "call_legs",
                newName: "Kind");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "call_legs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "started_at",
                table: "call_legs",
                newName: "StartedAt");

            migrationBuilder.RenameColumn(
                name: "participant_identity",
                table: "call_legs",
                newName: "ParticipantIdentity");

            migrationBuilder.RenameColumn(
                name: "leg_index",
                table: "call_legs",
                newName: "LegIndex");

            migrationBuilder.RenameColumn(
                name: "hangup_cause",
                table: "call_legs",
                newName: "HangupCause");

            migrationBuilder.RenameColumn(
                name: "ended_at",
                table: "call_legs",
                newName: "EndedAt");

            migrationBuilder.RenameColumn(
                name: "call_session_id",
                table: "call_legs",
                newName: "CallSessionId");

            migrationBuilder.RenameColumn(
                name: "answered_at",
                table: "call_legs",
                newName: "AnsweredAt");

            migrationBuilder.RenameIndex(
                name: "ix_call_legs_kind",
                table: "call_legs",
                newName: "IX_call_legs_Kind");

            migrationBuilder.RenameIndex(
                name: "ix_call_legs_call_session_id_leg_index",
                table: "call_legs",
                newName: "IX_call_legs_CallSessionId_LegIndex");

            migrationBuilder.RenameIndex(
                name: "ix_call_legs_call_session_id",
                table: "call_legs",
                newName: "IX_call_legs_CallSessionId");

            migrationBuilder.RenameColumn(
                name: "summary",
                table: "call_handoffs",
                newName: "Summary");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "call_handoffs",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "reason",
                table: "call_handoffs",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "call_handoffs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "to_human_agent_id",
                table: "call_handoffs",
                newName: "ToHumanAgentId");

            migrationBuilder.RenameColumn(
                name: "from_participant_id",
                table: "call_handoffs",
                newName: "FromParticipantId");

            migrationBuilder.RenameColumn(
                name: "delivered_at",
                table: "call_handoffs",
                newName: "DeliveredAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "call_handoffs",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "context_data_json",
                table: "call_handoffs",
                newName: "ContextDataJson");

            migrationBuilder.RenameColumn(
                name: "call_transfer_id",
                table: "call_handoffs",
                newName: "CallTransferId");

            migrationBuilder.RenameColumn(
                name: "call_session_id",
                table: "call_handoffs",
                newName: "CallSessionId");

            migrationBuilder.RenameColumn(
                name: "accepted_at",
                table: "call_handoffs",
                newName: "AcceptedAt");

            migrationBuilder.RenameIndex(
                name: "ix_call_handoffs_status",
                table: "call_handoffs",
                newName: "IX_call_handoffs_Status");

            migrationBuilder.RenameIndex(
                name: "ix_call_handoffs_to_human_agent_id",
                table: "call_handoffs",
                newName: "IX_call_handoffs_ToHumanAgentId");

            migrationBuilder.RenameIndex(
                name: "ix_call_handoffs_from_participant_id",
                table: "call_handoffs",
                newName: "IX_call_handoffs_FromParticipantId");

            migrationBuilder.RenameIndex(
                name: "ix_call_handoffs_call_transfer_id",
                table: "call_handoffs",
                newName: "IX_call_handoffs_CallTransferId");

            migrationBuilder.RenameIndex(
                name: "ix_call_handoffs_call_session_id",
                table: "call_handoffs",
                newName: "IX_call_handoffs_CallSessionId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "call_configurations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "call_configurations",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "call_configurations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "workflow_id",
                table: "call_configurations",
                newName: "WorkflowId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "call_configurations",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "call_configurations",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "persona_id",
                table: "call_configurations",
                newName: "PersonaId");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "call_configurations",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "call_configurations",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "config_json",
                table: "call_configurations",
                newName: "ConfigJson");

            migrationBuilder.RenameIndex(
                name: "ix_call_configurations_workflow_id",
                table: "call_configurations",
                newName: "IX_call_configurations_WorkflowId");

            migrationBuilder.RenameIndex(
                name: "ix_call_configurations_user_id",
                table: "call_configurations",
                newName: "IX_call_configurations_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_call_configurations_persona_id",
                table: "call_configurations",
                newName: "IX_call_configurations_PersonaId");

            migrationBuilder.RenameIndex(
                name: "ix_call_configurations_is_active",
                table: "call_configurations",
                newName: "IX_call_configurations_IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "call_configuration_actions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "action_definition_id",
                table: "call_configuration_actions",
                newName: "ActionDefinitionId");

            migrationBuilder.RenameColumn(
                name: "call_configuration_id",
                table: "call_configuration_actions",
                newName: "CallConfigurationId");

            migrationBuilder.RenameIndex(
                name: "ix_call_configuration_actions_action_definition_id",
                table: "call_configuration_actions",
                newName: "IX_call_configuration_actions_ActionDefinitionId");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "agent_users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "agent_users",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "agent_users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "agent_users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "is_online",
                table: "agent_users",
                newName: "IsOnline");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "agent_users",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_agent_users_username",
                table: "agent_users",
                newName: "IX_agent_users_Username");

            migrationBuilder.RenameIndex(
                name: "ix_agent_users_status",
                table: "agent_users",
                newName: "IX_agent_users_Status");

            migrationBuilder.RenameIndex(
                name: "ix_agent_users_is_online",
                table: "agent_users",
                newName: "IX_agent_users_IsOnline");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "action_executions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "error",
                table: "action_executions",
                newName: "Error");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "action_executions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "workflow_execution_id",
                table: "action_executions",
                newName: "WorkflowExecutionId");

            migrationBuilder.RenameColumn(
                name: "started_at",
                table: "action_executions",
                newName: "StartedAt");

            migrationBuilder.RenameColumn(
                name: "output_json",
                table: "action_executions",
                newName: "OutputJson");

            migrationBuilder.RenameColumn(
                name: "input_json",
                table: "action_executions",
                newName: "InputJson");

            migrationBuilder.RenameColumn(
                name: "completed_at",
                table: "action_executions",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "call_session_id",
                table: "action_executions",
                newName: "CallSessionId");

            migrationBuilder.RenameColumn(
                name: "action_definition_id",
                table: "action_executions",
                newName: "ActionDefinitionId");

            migrationBuilder.RenameIndex(
                name: "ix_action_executions_status",
                table: "action_executions",
                newName: "IX_action_executions_Status");

            migrationBuilder.RenameIndex(
                name: "ix_action_executions_workflow_execution_id",
                table: "action_executions",
                newName: "IX_action_executions_WorkflowExecutionId");

            migrationBuilder.RenameIndex(
                name: "ix_action_executions_call_session_id",
                table: "action_executions",
                newName: "IX_action_executions_CallSessionId");

            migrationBuilder.RenameIndex(
                name: "ix_action_executions_action_definition_id",
                table: "action_executions",
                newName: "IX_action_executions_ActionDefinitionId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "action_definitions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "action_definitions",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "action_definitions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "action_definitions",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "output_schema_json",
                table: "action_definitions",
                newName: "OutputSchemaJson");

            migrationBuilder.RenameColumn(
                name: "is_system",
                table: "action_definitions",
                newName: "IsSystem");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "action_definitions",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "input_schema_json",
                table: "action_definitions",
                newName: "InputSchemaJson");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "action_definitions",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "action_definitions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "configuration_json",
                table: "action_definitions",
                newName: "ConfigurationJson");

            migrationBuilder.RenameColumn(
                name: "action_type",
                table: "action_definitions",
                newName: "ActionType");

            migrationBuilder.RenameIndex(
                name: "ix_action_definitions_name",
                table: "action_definitions",
                newName: "IX_action_definitions_Name");

            migrationBuilder.RenameIndex(
                name: "ix_action_definitions_is_active",
                table: "action_definitions",
                newName: "IX_action_definitions_IsActive");

            migrationBuilder.RenameIndex(
                name: "ix_action_definitions_action_type",
                table: "action_definitions",
                newName: "IX_action_definitions_ActionType");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "configuration",
                table: "workflows",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "configuration",
                table: "workflows",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "configuration",
                table: "workflows",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "configuration",
                table: "workflows",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "configuration",
                table: "workflows",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_active",
                schema: "configuration",
                table: "workflows",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "configuration",
                table: "workflows",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_workflows_user_id",
                schema: "configuration",
                table: "workflows",
                newName: "IX_workflows_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_workflows_is_active",
                schema: "configuration",
                table: "workflows",
                newName: "IX_workflows_IsActive");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "identity",
                table: "users",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "email",
                schema: "identity",
                table: "users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "identity",
                table: "users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "identity",
                table: "users",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "standard_credits",
                schema: "identity",
                table: "users",
                newName: "StandardCredits");

            migrationBuilder.RenameColumn(
                name: "premium_credits",
                schema: "identity",
                table: "users",
                newName: "PremiumCredits");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                schema: "identity",
                table: "users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "is_partner",
                schema: "identity",
                table: "users",
                newName: "IsPartner");

            migrationBuilder.RenameColumn(
                name: "display_name",
                schema: "identity",
                table: "users",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "default_persona_id",
                schema: "identity",
                table: "users",
                newName: "DefaultPersonaId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "identity",
                table: "users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "company_name",
                schema: "identity",
                table: "users",
                newName: "CompanyName");

            migrationBuilder.RenameIndex(
                name: "ix_users_status",
                schema: "identity",
                table: "users",
                newName: "IX_users_Status");

            migrationBuilder.RenameIndex(
                name: "ix_users_email",
                schema: "identity",
                table: "users",
                newName: "IX_users_Email");

            migrationBuilder.RenameIndex(
                name: "ix_users_default_persona_id",
                schema: "identity",
                table: "users",
                newName: "IX_users_DefaultPersonaId");

            migrationBuilder.RenameIndex(
                name: "ix_users_created_at",
                schema: "identity",
                table: "users",
                newName: "IX_users_CreatedAt");

            migrationBuilder.RenameColumn(
                name: "unit",
                schema: "billing",
                table: "usage_records",
                newName: "Unit");

            migrationBuilder.RenameColumn(
                name: "quantity",
                schema: "billing",
                table: "usage_records",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "billing",
                table: "usage_records",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "billing",
                table: "usage_records",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "partner_id",
                schema: "billing",
                table: "usage_records",
                newName: "PartnerId");

            migrationBuilder.RenameColumn(
                name: "occurred_at",
                schema: "billing",
                table: "usage_records",
                newName: "OccurredAt");

            migrationBuilder.RenameColumn(
                name: "metric_type",
                schema: "billing",
                table: "usage_records",
                newName: "MetricType");

            migrationBuilder.RenameColumn(
                name: "metadata_json",
                schema: "billing",
                table: "usage_records",
                newName: "MetadataJson");

            migrationBuilder.RenameColumn(
                name: "license_id",
                schema: "billing",
                table: "usage_records",
                newName: "LicenseId");

            migrationBuilder.RenameColumn(
                name: "idempotency_key",
                schema: "billing",
                table: "usage_records",
                newName: "IdempotencyKey");

            migrationBuilder.RenameColumn(
                name: "call_session_id",
                schema: "billing",
                table: "usage_records",
                newName: "CallSessionId");

            migrationBuilder.RenameIndex(
                name: "ix_usage_records_user_id_occurred_at",
                schema: "billing",
                table: "usage_records",
                newName: "IX_usage_records_UserId_OccurredAt");

            migrationBuilder.RenameIndex(
                name: "ix_usage_records_user_id",
                schema: "billing",
                table: "usage_records",
                newName: "IX_usage_records_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_usage_records_partner_id",
                schema: "billing",
                table: "usage_records",
                newName: "IX_usage_records_PartnerId");

            migrationBuilder.RenameIndex(
                name: "ix_usage_records_occurred_at",
                schema: "billing",
                table: "usage_records",
                newName: "IX_usage_records_OccurredAt");

            migrationBuilder.RenameIndex(
                name: "ix_usage_records_metric_type",
                schema: "billing",
                table: "usage_records",
                newName: "IX_usage_records_MetricType");

            migrationBuilder.RenameIndex(
                name: "ix_usage_records_license_id",
                schema: "billing",
                table: "usage_records",
                newName: "IX_usage_records_LicenseId");

            migrationBuilder.RenameIndex(
                name: "ix_usage_records_idempotency_key",
                schema: "billing",
                table: "usage_records",
                newName: "IX_usage_records_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "ix_usage_records_call_session_id",
                schema: "billing",
                table: "usage_records",
                newName: "IX_usage_records_CallSessionId");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "billing",
                table: "subscriptions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "billing",
                table: "subscriptions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "billing",
                table: "subscriptions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "billing",
                table: "subscriptions",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "trial_ends_at",
                schema: "billing",
                table: "subscriptions",
                newName: "TrialEndsAt");

            migrationBuilder.RenameColumn(
                name: "starts_at",
                schema: "billing",
                table: "subscriptions",
                newName: "StartsAt");

            migrationBuilder.RenameColumn(
                name: "plan_id",
                schema: "billing",
                table: "subscriptions",
                newName: "PlanId");

            migrationBuilder.RenameColumn(
                name: "ends_at",
                schema: "billing",
                table: "subscriptions",
                newName: "EndsAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "billing",
                table: "subscriptions",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_subscriptions_status",
                schema: "billing",
                table: "subscriptions",
                newName: "IX_subscriptions_Status");

            migrationBuilder.RenameIndex(
                name: "ix_subscriptions_user_id",
                schema: "billing",
                table: "subscriptions",
                newName: "IX_subscriptions_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_subscriptions_plan_id",
                schema: "billing",
                table: "subscriptions",
                newName: "IX_subscriptions_PlanId");

            migrationBuilder.RenameColumn(
                name: "tier",
                schema: "billing",
                table: "plans",
                newName: "Tier");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "billing",
                table: "plans",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "billing",
                table: "plans",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "billing",
                table: "plans",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "billing",
                table: "plans",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_platform_plan",
                schema: "billing",
                table: "plans",
                newName: "IsPlatformPlan");

            migrationBuilder.RenameColumn(
                name: "is_active",
                schema: "billing",
                table: "plans",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "entitlements_json",
                schema: "billing",
                table: "plans",
                newName: "EntitlementsJson");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "billing",
                table: "plans",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_plans_tier",
                schema: "billing",
                table: "plans",
                newName: "IX_plans_Tier");

            migrationBuilder.RenameIndex(
                name: "ix_plans_is_active",
                schema: "billing",
                table: "plans",
                newName: "IX_plans_IsActive");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "configuration",
                table: "personas",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "configuration",
                table: "personas",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "configuration",
                table: "personas",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "configuration",
                table: "personas",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "configuration",
                table: "personas",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_active",
                schema: "configuration",
                table: "personas",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "configuration",
                table: "personas",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_personas_user_id",
                schema: "configuration",
                table: "personas",
                newName: "IX_personas_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_personas_is_active",
                schema: "configuration",
                table: "personas",
                newName: "IX_personas_IsActive");

            migrationBuilder.RenameColumn(
                name: "website",
                schema: "identity",
                table: "partners",
                newName: "Website");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "identity",
                table: "partners",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "identity",
                table: "partners",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "identity",
                table: "partners",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "identity",
                table: "partners",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                schema: "identity",
                table: "partners",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "organization_name",
                schema: "identity",
                table: "partners",
                newName: "OrganizationName");

            migrationBuilder.RenameColumn(
                name: "metadata_json",
                schema: "identity",
                table: "partners",
                newName: "MetadataJson");

            migrationBuilder.RenameColumn(
                name: "is_active",
                schema: "identity",
                table: "partners",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "identity",
                table: "partners",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "contact_email",
                schema: "identity",
                table: "partners",
                newName: "ContactEmail");

            migrationBuilder.RenameIndex(
                name: "ix_partners_user_id",
                schema: "identity",
                table: "partners",
                newName: "IX_partners_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_partners_organization_name",
                schema: "identity",
                table: "partners",
                newName: "IX_partners_OrganizationName");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "identity",
                table: "partner_relationships",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "identity",
                table: "partner_relationships",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "identity",
                table: "partner_relationships",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "identity",
                table: "partner_relationships",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "partner_id",
                schema: "identity",
                table: "partner_relationships",
                newName: "PartnerId");

            migrationBuilder.RenameColumn(
                name: "metadata_json",
                schema: "identity",
                table: "partner_relationships",
                newName: "MetadataJson");

            migrationBuilder.RenameColumn(
                name: "customer_user_id",
                schema: "identity",
                table: "partner_relationships",
                newName: "CustomerUserId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "identity",
                table: "partner_relationships",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_partner_relationships_status",
                schema: "identity",
                table: "partner_relationships",
                newName: "IX_partner_relationships_Status");

            migrationBuilder.RenameIndex(
                name: "ix_partner_relationships_user_id",
                schema: "identity",
                table: "partner_relationships",
                newName: "IX_partner_relationships_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_partner_relationships_partner_id_customer_user_id",
                schema: "identity",
                table: "partner_relationships",
                newName: "IX_partner_relationships_PartnerId_CustomerUserId");

            migrationBuilder.RenameIndex(
                name: "ix_partner_relationships_partner_id",
                schema: "identity",
                table: "partner_relationships",
                newName: "IX_partner_relationships_PartnerId");

            migrationBuilder.RenameIndex(
                name: "ix_partner_relationships_customer_user_id",
                schema: "identity",
                table: "partner_relationships",
                newName: "IX_partner_relationships_CustomerUserId");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "identity",
                table: "partner_plans",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "identity",
                table: "partner_plans",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "identity",
                table: "partner_plans",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "identity",
                table: "partner_plans",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "partner_id",
                schema: "identity",
                table: "partner_plans",
                newName: "PartnerId");

            migrationBuilder.RenameColumn(
                name: "is_active",
                schema: "identity",
                table: "partner_plans",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "entitlements_json",
                schema: "identity",
                table: "partner_plans",
                newName: "EntitlementsJson");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "identity",
                table: "partner_plans",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_partner_plans_partner_id",
                schema: "identity",
                table: "partner_plans",
                newName: "IX_partner_plans_PartnerId");

            migrationBuilder.RenameIndex(
                name: "ix_partner_plans_is_active",
                schema: "identity",
                table: "partner_plans",
                newName: "IX_partner_plans_IsActive");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "identity",
                table: "partner_external_customers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "platform_user_id",
                schema: "identity",
                table: "partner_external_customers",
                newName: "PlatformUserId");

            migrationBuilder.RenameColumn(
                name: "partner_id",
                schema: "identity",
                table: "partner_external_customers",
                newName: "PartnerId");

            migrationBuilder.RenameColumn(
                name: "external_customer_id",
                schema: "identity",
                table: "partner_external_customers",
                newName: "ExternalCustomerId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "identity",
                table: "partner_external_customers",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_partner_external_customers_platform_user_id",
                schema: "identity",
                table: "partner_external_customers",
                newName: "IX_partner_external_customers_PlatformUserId");

            migrationBuilder.RenameIndex(
                name: "ix_partner_external_customers_partner_id_external_customer_id",
                schema: "identity",
                table: "partner_external_customers",
                newName: "IX_partner_external_customers_PartnerId_ExternalCustomerId");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "identity",
                table: "licenses",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "identity",
                table: "licenses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "identity",
                table: "licenses",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "identity",
                table: "licenses",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "starts_at",
                schema: "identity",
                table: "licenses",
                newName: "StartsAt");

            migrationBuilder.RenameColumn(
                name: "partner_plan_id",
                schema: "identity",
                table: "licenses",
                newName: "PartnerPlanId");

            migrationBuilder.RenameColumn(
                name: "partner_id",
                schema: "identity",
                table: "licenses",
                newName: "PartnerId");

            migrationBuilder.RenameColumn(
                name: "metadata_json",
                schema: "identity",
                table: "licenses",
                newName: "MetadataJson");

            migrationBuilder.RenameColumn(
                name: "limits_json",
                schema: "identity",
                table: "licenses",
                newName: "LimitsJson");

            migrationBuilder.RenameColumn(
                name: "ends_at",
                schema: "identity",
                table: "licenses",
                newName: "EndsAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "identity",
                table: "licenses",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_licenses_status",
                schema: "identity",
                table: "licenses",
                newName: "IX_licenses_Status");

            migrationBuilder.RenameIndex(
                name: "ix_licenses_user_id",
                schema: "identity",
                table: "licenses",
                newName: "IX_licenses_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_licenses_starts_at",
                schema: "identity",
                table: "licenses",
                newName: "IX_licenses_StartsAt");

            migrationBuilder.RenameIndex(
                name: "ix_licenses_partner_plan_id",
                schema: "identity",
                table: "licenses",
                newName: "IX_licenses_PartnerPlanId");

            migrationBuilder.RenameIndex(
                name: "ix_licenses_partner_id",
                schema: "identity",
                table: "licenses",
                newName: "IX_licenses_PartnerId");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "configuration",
                table: "knowledge_bases",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "configuration",
                table: "knowledge_bases",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "configuration",
                table: "knowledge_bases",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "configuration",
                table: "knowledge_bases",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "configuration",
                table: "knowledge_bases",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_active",
                schema: "configuration",
                table: "knowledge_bases",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "configuration",
                table: "knowledge_bases",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_knowledge_bases_user_id",
                schema: "configuration",
                table: "knowledge_bases",
                newName: "IX_knowledge_bases_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_knowledge_bases_is_active",
                schema: "configuration",
                table: "knowledge_bases",
                newName: "IX_knowledge_bases_IsActive");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "identity",
                table: "api_keys",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "scopes",
                schema: "identity",
                table: "api_keys",
                newName: "Scopes");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "identity",
                table: "api_keys",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "identity",
                table: "api_keys",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "identity",
                table: "api_keys",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "revoked_at",
                schema: "identity",
                table: "api_keys",
                newName: "RevokedAt");

            migrationBuilder.RenameColumn(
                name: "last_used_at",
                schema: "identity",
                table: "api_keys",
                newName: "LastUsedAt");

            migrationBuilder.RenameColumn(
                name: "key_prefix",
                schema: "identity",
                table: "api_keys",
                newName: "KeyPrefix");

            migrationBuilder.RenameColumn(
                name: "key_hash",
                schema: "identity",
                table: "api_keys",
                newName: "KeyHash");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                schema: "identity",
                table: "api_keys",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "identity",
                table: "api_keys",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_api_keys_status",
                schema: "identity",
                table: "api_keys",
                newName: "IX_api_keys_Status");

            migrationBuilder.RenameIndex(
                name: "ix_api_keys_user_id",
                schema: "identity",
                table: "api_keys",
                newName: "IX_api_keys_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_api_keys_key_hash",
                schema: "identity",
                table: "api_keys",
                newName: "IX_api_keys_KeyHash");

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
                .Annotation("Npgsql:Enum:workflow_execution_status", "pending,running,completed,failed")
                .OldAnnotation("Npgsql:Enum:access_key_status.access_key_status", "active,revoked,expired")
                .OldAnnotation("Npgsql:Enum:action_execution_status.action_execution_status", "pending,running,completed,failed")
                .OldAnnotation("Npgsql:Enum:action_type.action_type", "system,workflow,integration,webhook")
                .OldAnnotation("Npgsql:Enum:api_key_status.api_key_status", "active,revoked,expired")
                .OldAnnotation("Npgsql:Enum:call_direction.call_direction", "inbound,outbound")
                .OldAnnotation("Npgsql:Enum:call_session_status.call_session_status", "queued,ringing,active,transferred,completed,failed,cancelled")
                .OldAnnotation("Npgsql:Enum:call_transfer_status.call_transfer_status", "requested,ringing,accepted,completed,rejected,failed,cancelled")
                .OldAnnotation("Npgsql:Enum:handoff_status.handoff_status", "pending,delivered,accepted,expired")
                .OldAnnotation("Npgsql:Enum:human_agent_status.human_agent_status", "offline,available,break,not_ready,in_call")
                .OldAnnotation("Npgsql:Enum:license_status.license_status", "active,inactive,expired,cancelled,suspended")
                .OldAnnotation("Npgsql:Enum:metric_type.metric_type", "call_duration,call_minutes,transfer_count,recording_minutes,agent_session_minutes")
                .OldAnnotation("Npgsql:Enum:participant_type.participant_type", "customer,ai_agent,human_agent")
                .OldAnnotation("Npgsql:Enum:partner_relationship_status.partner_relationship_status", "active,inactive,suspended")
                .OldAnnotation("Npgsql:Enum:plan_tier.plan_tier", "free,starter,growth,enterprise")
                .OldAnnotation("Npgsql:Enum:recording_status.recording_status", "pending,in_progress,completed,failed")
                .OldAnnotation("Npgsql:Enum:subscription_status.subscription_status", "active,past_due,cancelled,expired,trialing")
                .OldAnnotation("Npgsql:Enum:user_status.user_status", "active,inactive,suspended")
                .OldAnnotation("Npgsql:Enum:workflow_execution_status.workflow_execution_status", "pending,running,completed,failed");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "human_agents",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "human_agent_access_keys",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "call_transfers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "call_sessions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Direction",
                table: "call_sessions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "call_recordings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "ParticipantType",
                table: "call_participants",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "call_handoffs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "action_executions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "ActionType",
                table: "action_definitions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "identity",
                table: "users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "MetricType",
                schema: "billing",
                table: "usage_records",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "billing",
                table: "subscriptions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Tier",
                schema: "billing",
                table: "plans",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "identity",
                table: "partner_relationships",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "identity",
                table: "licenses",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "identity",
                table: "api_keys",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_workflow_versions",
                table: "workflow_versions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_workflow_executions",
                table: "workflow_executions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sip_destinations",
                table: "sip_destinations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sip_connections",
                table: "sip_connections",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_persona_versions",
                table: "persona_versions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_persona_knowledge_bases",
                table: "persona_knowledge_bases",
                columns: new[] { "PersonaId", "KnowledgeBaseId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_persona_actions",
                table: "persona_actions",
                columns: new[] { "PersonaId", "ActionDefinitionId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_knowledge_documents",
                table: "knowledge_documents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_knowledge_chunks",
                table: "knowledge_chunks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_human_agents",
                table: "human_agents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_human_agent_sessions",
                table: "human_agent_sessions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_human_agent_access_keys",
                table: "human_agent_access_keys",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_call_transfers",
                table: "call_transfers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_call_sessions",
                table: "call_sessions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_call_records",
                table: "call_records",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_call_recordings",
                table: "call_recordings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_call_participants",
                table: "call_participants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_call_legs",
                table: "call_legs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_call_handoffs",
                table: "call_handoffs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_call_configurations",
                table: "call_configurations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_call_configuration_actions",
                table: "call_configuration_actions",
                columns: new[] { "CallConfigurationId", "ActionDefinitionId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_agent_users",
                table: "agent_users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_action_executions",
                table: "action_executions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_action_definitions",
                table: "action_definitions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_workflows",
                schema: "configuration",
                table: "workflows",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                schema: "identity",
                table: "users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_usage_records",
                schema: "billing",
                table: "usage_records",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_subscriptions",
                schema: "billing",
                table: "subscriptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_plans",
                schema: "billing",
                table: "plans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_personas",
                schema: "configuration",
                table: "personas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_partners",
                schema: "identity",
                table: "partners",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_partner_relationships",
                schema: "identity",
                table: "partner_relationships",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_partner_plans",
                schema: "identity",
                table: "partner_plans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_partner_external_customers",
                schema: "identity",
                table: "partner_external_customers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_licenses",
                schema: "identity",
                table: "licenses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_knowledge_bases",
                schema: "configuration",
                table: "knowledge_bases",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_api_keys",
                schema: "identity",
                table: "api_keys",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "action_definitions",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000001"),
                column: "ActionType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "action_definitions",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000002"),
                column: "ActionType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "agent_users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 30, 16, 39, 58, 567, DateTimeKind.Utc).AddTicks(2741));

            migrationBuilder.AddForeignKey(
                name: "FK_action_executions_action_definitions_ActionDefinitionId",
                table: "action_executions",
                column: "ActionDefinitionId",
                principalTable: "action_definitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_call_configuration_actions_action_definitions_ActionDefinit~",
                table: "call_configuration_actions",
                column: "ActionDefinitionId",
                principalTable: "action_definitions",
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
                principalSchema: "configuration",
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
                principalSchema: "configuration",
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
                name: "FK_call_records_agent_users_HandledByAgentId",
                table: "call_records",
                column: "HandledByAgentId",
                principalTable: "agent_users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_call_sessions_api_keys_ApiKeyId",
                table: "call_sessions",
                column: "ApiKeyId",
                principalSchema: "identity",
                principalTable: "api_keys",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_call_sessions_call_configurations_CallConfigurationId",
                table: "call_sessions",
                column: "CallConfigurationId",
                principalTable: "call_configurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

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
                name: "FK_call_transfers_call_participants_FromParticipantId",
                table: "call_transfers",
                column: "FromParticipantId",
                principalTable: "call_participants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_call_transfers_call_sessions_CallSessionId",
                table: "call_transfers",
                column: "CallSessionId",
                principalTable: "call_sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                schema: "configuration",
                table: "knowledge_bases",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_knowledge_chunks_knowledge_documents_KnowledgeDocumentId",
                table: "knowledge_chunks",
                column: "KnowledgeDocumentId",
                principalTable: "knowledge_documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_knowledge_documents_knowledge_bases_KnowledgeBaseId",
                table: "knowledge_documents",
                column: "KnowledgeBaseId",
                principalSchema: "configuration",
                principalTable: "knowledge_bases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_licenses_partner_plans_PartnerPlanId",
                schema: "identity",
                table: "licenses",
                column: "PartnerPlanId",
                principalSchema: "identity",
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
                schema: "identity",
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
                name: "FK_persona_actions_action_definitions_ActionDefinitionId",
                table: "persona_actions",
                column: "ActionDefinitionId",
                principalTable: "action_definitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_persona_actions_personas_PersonaId",
                table: "persona_actions",
                column: "PersonaId",
                principalSchema: "configuration",
                principalTable: "personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_persona_knowledge_bases_knowledge_bases_KnowledgeBaseId",
                table: "persona_knowledge_bases",
                column: "KnowledgeBaseId",
                principalSchema: "configuration",
                principalTable: "knowledge_bases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_persona_knowledge_bases_personas_PersonaId",
                table: "persona_knowledge_bases",
                column: "PersonaId",
                principalSchema: "configuration",
                principalTable: "personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_persona_versions_personas_PersonaId",
                table: "persona_versions",
                column: "PersonaId",
                principalSchema: "configuration",
                principalTable: "personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_personas_users_UserId",
                schema: "configuration",
                table: "personas",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sip_connections_users_UserId",
                table: "sip_connections",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sip_destinations_users_UserId",
                table: "sip_destinations",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_subscriptions_plans_PlanId",
                schema: "billing",
                table: "subscriptions",
                column: "PlanId",
                principalSchema: "billing",
                principalTable: "plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_subscriptions_users_UserId",
                schema: "billing",
                table: "subscriptions",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usage_records_call_sessions_CallSessionId",
                schema: "billing",
                table: "usage_records",
                column: "CallSessionId",
                principalTable: "call_sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_usage_records_licenses_LicenseId",
                schema: "billing",
                table: "usage_records",
                column: "LicenseId",
                principalSchema: "identity",
                principalTable: "licenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_usage_records_partners_PartnerId",
                schema: "billing",
                table: "usage_records",
                column: "PartnerId",
                principalSchema: "identity",
                principalTable: "partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_usage_records_users_UserId",
                schema: "billing",
                table: "usage_records",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_users_personas_DefaultPersonaId",
                schema: "identity",
                table: "users",
                column: "DefaultPersonaId",
                principalSchema: "configuration",
                principalTable: "personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_workflow_executions_call_sessions_CallSessionId",
                table: "workflow_executions",
                column: "CallSessionId",
                principalTable: "call_sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_workflow_executions_workflow_versions_WorkflowVersionId",
                table: "workflow_executions",
                column: "WorkflowVersionId",
                principalTable: "workflow_versions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_workflow_versions_workflows_WorkflowId",
                table: "workflow_versions",
                column: "WorkflowId",
                principalSchema: "configuration",
                principalTable: "workflows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_workflows_users_UserId",
                schema: "configuration",
                table: "workflows",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
