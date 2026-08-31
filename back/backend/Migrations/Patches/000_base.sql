-- =============================================================================
-- AI VOICE / AI CALLING SAAS PLATFORM
-- Initial PostgreSQL Schema Migration
-- Generated from ERD v1.0
-- =============================================================================
-- Run: psql -U admin -d callcenter -f 001_initial_schema.sql
-- =============================================================================

BEGIN;

-- =============================================================================
-- 1. POSTGRESQL ENUMS
-- =============================================================================

DO $$ BEGIN
    CREATE TYPE user_status AS ENUM ('active', 'inactive', 'suspended');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE partner_relationship_status AS ENUM ('active', 'inactive', 'suspended');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE api_key_status AS ENUM ('active', 'revoked', 'expired');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE human_agent_status AS ENUM ('offline', 'available', 'break', 'not_ready', 'in_call');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE access_key_status AS ENUM ('active', 'revoked', 'expired');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE call_session_status AS ENUM ('queued', 'ringing', 'active', 'transferred', 'completed', 'failed', 'cancelled');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE call_direction AS ENUM ('inbound', 'outbound');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE participant_type AS ENUM ('customer', 'ai_agent', 'human_agent');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE call_transfer_status AS ENUM ('requested', 'ringing', 'accepted', 'completed', 'rejected', 'failed', 'cancelled');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE handoff_status AS ENUM ('pending', 'delivered', 'accepted', 'expired');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE license_status AS ENUM ('active', 'inactive', 'expired', 'cancelled', 'suspended');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE plan_tier AS ENUM ('free', 'starter', 'growth', 'enterprise');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE recording_status AS ENUM ('pending', 'in_progress', 'completed', 'failed');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE metric_type AS ENUM ('call_duration', 'call_minutes', 'transfer_count', 'recording_minutes', 'agent_session_minutes');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE action_type AS ENUM ('system', 'workflow', 'integration', 'webhook');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE action_execution_status AS ENUM ('pending', 'running', 'completed', 'failed');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE workflow_execution_status AS ENUM ('pending', 'running', 'completed', 'failed');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
    CREATE TYPE subscription_status AS ENUM ('active', 'past_due', 'cancelled', 'expired', 'trialing');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- Enable pgvector extension (for knowledge base embeddings)
CREATE EXTENSION IF NOT EXISTS vector;

-- =============================================================================
-- 2. CORE MULTI-TENANCY TABLES
-- =============================================================================

-- 2.1 USERS (unified user/account model)
CREATE TABLE IF NOT EXISTS users (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email               VARCHAR(320) NOT NULL,
    display_name         VARCHAR(256) NOT NULL,
    company_name         VARCHAR(256),
    password_hash        TEXT NOT NULL,
    status              user_status NOT NULL DEFAULT 'active',
    is_partner           BOOLEAN NOT NULL DEFAULT FALSE,
    standard_credits     NUMERIC(18,4) NOT NULL DEFAULT 0,
    premium_credits      NUMERIC(18,4) NOT NULL DEFAULT 0,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    default_persona_id    UUID
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_users_email ON users (email);
CREATE INDEX IF NOT EXISTS ix_users_status ON users (status);
CREATE INDEX IF NOT EXISTS ix_users_created_at ON users (created_at);
CREATE INDEX IF NOT EXISTS ix_users_default_persona_id ON users (default_persona_id);

-- 2.2 PARTNERS (partner profile — 1:1 extension of users)
CREATE TABLE IF NOT EXISTS partners (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id              UUID NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    organization_name    VARCHAR(256) NOT NULL,
    contact_email        VARCHAR(320),
    phone_number         VARCHAR(50),
    website             VARCHAR(512),
    description         TEXT,
    is_active            BOOLEAN NOT NULL DEFAULT TRUE,
    metadata_json        JSONB,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_partners_user_id ON partners (user_id);
CREATE INDEX IF NOT EXISTS ix_partners_organization_name ON partners (organization_name);

-- 2.3 PARTNER RELATIONSHIPS (partner → customer user mapping)
CREATE TABLE IF NOT EXISTS partner_relationships (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    partner_id           UUID NOT NULL REFERENCES partners(id) ON DELETE CASCADE,
    customer_user_id      UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    status              partner_relationship_status NOT NULL DEFAULT 'active',
    metadata_json        JSONB,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_partner_relationships_partner_customer
    ON partner_relationships (partner_id, customer_user_id);
CREATE INDEX IF NOT EXISTS ix_partner_relationships_customer_user_id
    ON partner_relationships (customer_user_id);
CREATE INDEX IF NOT EXISTS ix_partner_relationships_status
    ON partner_relationships (status);

-- 2.4 PARTNER EXTERNAL CUSTOMERS (maps partner's external ID → platform user)
CREATE TABLE IF NOT EXISTS partner_external_customers (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    partner_id           UUID NOT NULL REFERENCES partners(id) ON DELETE CASCADE,
    external_customer_id  VARCHAR(256) NOT NULL,
    platform_user_id      UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_partner_external_customers_partner_extid
    ON partner_external_customers (partner_id, external_customer_id);
CREATE INDEX IF NOT EXISTS ix_partner_external_customers_platform_user_id
    ON partner_external_customers (platform_user_id);

-- =============================================================================
-- 3. API KEYS
-- =============================================================================

CREATE TABLE IF NOT EXISTS api_keys (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id              UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name                VARCHAR(256) NOT NULL,
    key_prefix           VARCHAR(16) NOT NULL,
    key_hash             VARCHAR(256) NOT NULL,
    status              api_key_status NOT NULL DEFAULT 'active',
    scopes              TEXT[] NOT NULL DEFAULT '{}',
    last_used_at          TIMESTAMPTZ,
    expires_at           TIMESTAMPTZ,
    revoked_at           TIMESTAMPTZ,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_api_keys_key_hash ON api_keys (key_hash);
CREATE INDEX IF NOT EXISTS ix_api_keys_user_id ON api_keys (user_id);
CREATE INDEX IF NOT EXISTS ix_api_keys_status ON api_keys (status);

-- =============================================================================
-- 4. HUMAN AGENT SYSTEM
-- =============================================================================

-- 4.1 HUMAN AGENTS
CREATE TABLE IF NOT EXISTS human_agents (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    owner_user_id         UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    application_user_id   UUID REFERENCES users(id) ON DELETE SET NULL,
    name                VARCHAR(256) NOT NULL,
    email               VARCHAR(320),
    status              human_agent_status NOT NULL DEFAULT 'offline',
    is_active            BOOLEAN NOT NULL DEFAULT TRUE,
    max_concurrent_calls  INTEGER NOT NULL DEFAULT 1,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_human_agents_owner_user_id ON human_agents (owner_user_id);
CREATE INDEX IF NOT EXISTS ix_human_agents_status ON human_agents (status);
CREATE INDEX IF NOT EXISTS ix_human_agents_is_active ON human_agents (is_active);

-- 4.2 HUMAN AGENT ACCESS KEYS (agent app auth)
CREATE TABLE IF NOT EXISTS human_agent_access_keys (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    human_agent_id        UUID NOT NULL REFERENCES human_agents(id) ON DELETE CASCADE,
    name                VARCHAR(256) NOT NULL,
    key_prefix           VARCHAR(16) NOT NULL,
    key_hash             VARCHAR(256) NOT NULL,
    status              access_key_status NOT NULL DEFAULT 'active',
    expires_at           TIMESTAMPTZ,
    last_used_at          TIMESTAMPTZ,
    revoked_at           TIMESTAMPTZ,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_human_agent_access_keys_key_hash ON human_agent_access_keys (key_hash);
CREATE INDEX IF NOT EXISTS ix_human_agent_access_keys_human_agent_id ON human_agent_access_keys (human_agent_id);
CREATE INDEX IF NOT EXISTS ix_human_agent_access_keys_status ON human_agent_access_keys (status);

-- 4.3 HUMAN AGENT SESSIONS (historical session persistence)
CREATE TABLE IF NOT EXISTS human_agent_sessions (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    human_agent_id        UUID NOT NULL REFERENCES human_agents(id) ON DELETE CASCADE,
    livekit_identity     VARCHAR(256) NOT NULL,
    status              VARCHAR(50) NOT NULL DEFAULT 'active',
    connected_at         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    disconnected_at      TIMESTAMPTZ,
    last_heartbeat_at     TIMESTAMPTZ,
    metadata_json        JSONB
);

CREATE INDEX IF NOT EXISTS ix_human_agent_sessions_human_agent_id ON human_agent_sessions (human_agent_id);
CREATE INDEX IF NOT EXISTS ix_human_agent_sessions_status ON human_agent_sessions (status);
CREATE INDEX IF NOT EXISTS ix_human_agent_sessions_connected_at ON human_agent_sessions (connected_at);

-- =============================================================================
-- 5. PERSONA ENGINE
-- =============================================================================

-- 5.1 PERSONAS
CREATE TABLE IF NOT EXISTS personas (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id              UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name                VARCHAR(256) NOT NULL,
    description         TEXT,
    is_active            BOOLEAN NOT NULL DEFAULT TRUE,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_personas_user_id ON personas (user_id);
CREATE INDEX IF NOT EXISTS ix_personas_is_active ON personas (is_active);

-- 5.2 PERSONA VERSIONS (immutable snapshots for historical correctness)
CREATE TABLE IF NOT EXISTS persona_versions (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    persona_id           UUID NOT NULL REFERENCES personas(id) ON DELETE CASCADE,
    version_number       INTEGER NOT NULL,
    system_prompt        TEXT NOT NULL,
    configuration_json   JSONB NOT NULL DEFAULT '{}',
    is_published         BOOLEAN NOT NULL DEFAULT FALSE,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_persona_versions_persona_version
    ON persona_versions (persona_id, version_number);
CREATE INDEX IF NOT EXISTS ix_persona_versions_persona_id ON persona_versions (persona_id);
CREATE INDEX IF NOT EXISTS ix_persona_versions_is_published ON persona_versions (is_published);

-- =============================================================================
-- 6. WORKFLOW ENGINE
-- =============================================================================

-- 6.1 WORKFLOWS
CREATE TABLE IF NOT EXISTS workflows (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id              UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name                VARCHAR(256) NOT NULL,
    description         TEXT,
    is_active            BOOLEAN NOT NULL DEFAULT TRUE,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_workflows_user_id ON workflows (user_id);
CREATE INDEX IF NOT EXISTS ix_workflows_is_active ON workflows (is_active);

-- 6.2 WORKFLOW VERSIONS (immutable snapshots)
CREATE TABLE IF NOT EXISTS workflow_versions (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    workflow_id          UUID NOT NULL REFERENCES workflows(id) ON DELETE CASCADE,
    version_number       INTEGER NOT NULL,
    definition_json      JSONB NOT NULL DEFAULT '{}',
    is_published         BOOLEAN NOT NULL DEFAULT FALSE,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_workflow_versions_workflow_version
    ON workflow_versions (workflow_id, version_number);
CREATE INDEX IF NOT EXISTS ix_workflow_versions_workflow_id ON workflow_versions (workflow_id);
CREATE INDEX IF NOT EXISTS ix_workflow_versions_is_published ON workflow_versions (is_published);

-- 6.3 WORKFLOW EXECUTIONS (runtime execution records)
CREATE TABLE IF NOT EXISTS workflow_executions (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    workflow_version_id   UUID NOT NULL REFERENCES workflow_versions(id) ON DELETE CASCADE,
    call_session_id       UUID,
    status              VARCHAR(50) NOT NULL DEFAULT 'pending',
    input_json           JSONB,
    output_json          JSONB,
    state_json           JSONB,
    error               TEXT,
    started_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at         TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_workflow_executions_workflow_version_id ON workflow_executions (workflow_version_id);
CREATE INDEX IF NOT EXISTS ix_workflow_executions_call_session_id ON workflow_executions (call_session_id);
CREATE INDEX IF NOT EXISTS ix_workflow_executions_status ON workflow_executions (status);

-- =============================================================================
-- 7. ACTION ENGINE
-- =============================================================================

-- 7.1 ACTION DEFINITIONS (system and user-defined actions)
CREATE TABLE IF NOT EXISTS action_definitions (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name                VARCHAR(256) NOT NULL,
    display_name         VARCHAR(256) NOT NULL,
    description         TEXT,
    action_type          action_type NOT NULL DEFAULT 'system',
    is_system            BOOLEAN NOT NULL DEFAULT FALSE,
    input_schema_json     JSONB,
    output_schema_json    JSONB,
    configuration_json   JSONB,
    is_active            BOOLEAN NOT NULL DEFAULT TRUE,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_action_definitions_name ON action_definitions (name);
CREATE INDEX IF NOT EXISTS ix_action_definitions_action_type ON action_definitions (action_type);
CREATE INDEX IF NOT EXISTS ix_action_definitions_is_active ON action_definitions (is_active);

-- Seed built-in system actions
INSERT INTO action_definitions (id, name, display_name, description, action_type, is_system, is_active, created_at, updated_at)
VALUES
    ('a0000000-0000-0000-0000-000000000001', 'transfer_to_human', 'transfer to human',
     'transfers the call to an available human agent within the same live_kit room.',
     'system', TRUE, TRUE, '2025-01-01T00:00:00Z', '2025-01-01T00:00:00Z'),
    ('a0000000-0000-0000-0000-000000000002', 'end_call', 'end call',
     'ends the current call session.',
     'system', TRUE, TRUE, '2025-01-01T00:00:00Z', '2025-01-01T00:00:00Z')
ON CONFLICT (id) DO NOTHING;

-- 7.2 PERSONA ACTIONS (junction: which actions a persona is allowed to use)
CREATE TABLE IF NOT EXISTS persona_actions (
    persona_id           UUID NOT NULL REFERENCES personas(id) ON DELETE CASCADE,
    action_definition_id  UUID NOT NULL REFERENCES action_definitions(id) ON DELETE CASCADE,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (persona_id, action_definition_id)
);

-- 7.3 ACTION EXECUTIONS (audit trail of every action invocation)
CREATE TABLE IF NOT EXISTS action_executions (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    call_session_id           UUID NOT NULL,
    action_definition_id      UUID NOT NULL REFERENCES action_definitions(id) ON DELETE CASCADE,
    workflow_execution_id     UUID,
    status                  action_execution_status NOT NULL DEFAULT 'pending',
    input_json               JSONB,
    output_json              JSONB,
    error                   TEXT,
    started_at               TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at             TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_action_executions_call_session_id ON action_executions (call_session_id);
CREATE INDEX IF NOT EXISTS ix_action_executions_action_definition_id ON action_executions (action_definition_id);
CREATE INDEX IF NOT EXISTS ix_action_executions_status ON action_executions (status);

-- =============================================================================
-- 8. CALL CONFIGURATION
-- =============================================================================

CREATE TABLE IF NOT EXISTS call_configurations (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id          UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name            VARCHAR(256) NOT NULL,
    description     TEXT,
    persona_id       UUID REFERENCES personas(id) ON DELETE SET NULL,
    workflow_id      UUID REFERENCES workflows(id) ON DELETE SET NULL,
    is_active        BOOLEAN NOT NULL DEFAULT TRUE,
    config_json      JSONB,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_call_configurations_user_id ON call_configurations (user_id);
CREATE INDEX IF NOT EXISTS ix_call_configurations_is_active ON call_configurations (is_active);

-- 8.1 CALL CONFIGURATION ACTIONS (junction: allowed actions per config)
CREATE TABLE IF NOT EXISTS call_configuration_actions (
    call_configuration_id  UUID NOT NULL REFERENCES call_configurations(id) ON DELETE CASCADE,
    action_definition_id   UUID NOT NULL REFERENCES action_definitions(id) ON DELETE CASCADE,
    created_at            TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (call_configuration_id, action_definition_id)
);

-- =============================================================================
-- 9. CALL SESSIONS (central call lifecycle table)
-- =============================================================================

CREATE TABLE IF NOT EXISTS call_sessions (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id                  UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    call_configuration_id     UUID REFERENCES call_configurations(id) ON DELETE SET NULL,
    persona_version_id        UUID REFERENCES persona_versions(id) ON DELETE SET NULL,
    workflow_version_id       UUID REFERENCES workflow_versions(id) ON DELETE SET NULL,
    api_key_id                UUID REFERENCES api_keys(id) ON DELETE SET NULL,
    livekit_room_name         VARCHAR(256) NOT NULL,
    livekit_room_sid          VARCHAR(256),
    dialed_number            VARCHAR(32),
    origin_sip_connection_id   UUID,
    status                  call_session_status NOT NULL DEFAULT 'queued',
    direction               call_direction NOT NULL DEFAULT 'inbound',
    started_at               TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    answered_at              TIMESTAMPTZ,
    ended_at                 TIMESTAMPTZ,
    duration_seconds         INTEGER,
    metadata_json            JSONB,
    created_at               TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_call_sessions_user_id ON call_sessions (user_id);
CREATE INDEX IF NOT EXISTS ix_call_sessions_status ON call_sessions (status);
CREATE INDEX IF NOT EXISTS ix_call_sessions_started_at ON call_sessions (started_at);
CREATE INDEX IF NOT EXISTS ix_call_sessions_livekit_room_name ON call_sessions (livekit_room_name);
CREATE INDEX IF NOT EXISTS ix_call_sessions_user_started ON call_sessions (user_id, started_at);

-- FK for action_executions → call_sessions (deferred due to circular reference)
ALTER TABLE action_executions
    ADD CONSTRAINT fk_action_executions_call_session
    FOREIGN KEY (call_session_id) REFERENCES call_sessions(id) ON DELETE CASCADE;

ALTER TABLE workflow_executions
    ADD CONSTRAINT fk_workflow_executions_call_session
    FOREIGN KEY (call_session_id) REFERENCES call_sessions(id) ON DELETE SET NULL;

-- =============================================================================
-- 10. CALL PARTICIPANTS
-- =============================================================================

CREATE TABLE IF NOT EXISTS call_participants (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    call_session_id           UUID NOT NULL REFERENCES call_sessions(id) ON DELETE CASCADE,
    human_agent_id            UUID REFERENCES human_agents(id) ON DELETE SET NULL,
    participant_type         participant_type NOT NULL,
    livekit_identity         VARCHAR(256) NOT NULL,
    livekit_participant_sid   VARCHAR(256),
    display_name             VARCHAR(256),
    joined_at                TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    left_at                  TIMESTAMPTZ,
    created_at               TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_call_participants_human_agent
        CHECK (
            (participant_type = 'human_agent' AND human_agent_id IS NOT NULL) OR
            (participant_type <> 'human_agent')
        )
);

CREATE INDEX IF NOT EXISTS ix_call_participants_call_session_id ON call_participants (call_session_id);
CREATE INDEX IF NOT EXISTS ix_call_participants_human_agent_id ON call_participants (human_agent_id);
CREATE INDEX IF NOT EXISTS ix_call_participants_participant_type ON call_participants (participant_type);

-- =============================================================================
-- 11. CALL TRANSFERS (internal human transfer only)
-- =============================================================================

CREATE TABLE IF NOT EXISTS call_transfers (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    call_session_id           UUID NOT NULL REFERENCES call_sessions(id) ON DELETE CASCADE,
    from_participant_id       UUID REFERENCES call_participants(id) ON DELETE SET NULL,
    to_human_agent_id          UUID NOT NULL REFERENCES human_agents(id) ON DELETE CASCADE,
    status                  call_transfer_status NOT NULL DEFAULT 'requested',
    reason                  TEXT,
    failure_reason           TEXT,
    requested_at             TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    accepted_at              TIMESTAMPTZ,
    completed_at             TIMESTAMPTZ,
    failed_at                TIMESTAMPTZ,
    created_at               TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at               TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_call_transfers_call_session_id ON call_transfers (call_session_id);
CREATE INDEX IF NOT EXISTS ix_call_transfers_to_human_agent_id ON call_transfers (to_human_agent_id);
CREATE INDEX IF NOT EXISTS ix_call_transfers_status ON call_transfers (status);
CREATE INDEX IF NOT EXISTS ix_call_transfers_requested_at ON call_transfers (requested_at);

-- =============================================================================
-- 12. CALL HANDOFFS (context passed from AI → human agent)
-- =============================================================================

CREATE TABLE IF NOT EXISTS call_handoffs (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    call_session_id           UUID NOT NULL REFERENCES call_sessions(id) ON DELETE CASCADE,
    call_transfer_id          UUID NOT NULL UNIQUE REFERENCES call_transfers(id) ON DELETE CASCADE,
    from_participant_id       UUID REFERENCES call_participants(id) ON DELETE SET NULL,
    to_human_agent_id          UUID NOT NULL REFERENCES human_agents(id) ON DELETE CASCADE,
    reason                  TEXT,
    summary                 TEXT,
    context_data_json         JSONB,
    status                  handoff_status NOT NULL DEFAULT 'pending',
    created_at               TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    delivered_at             TIMESTAMPTZ,
    accepted_at              TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_call_handoffs_call_session_id ON call_handoffs (call_session_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_call_handoffs_call_transfer_id ON call_handoffs (call_transfer_id);
CREATE INDEX IF NOT EXISTS ix_call_handoffs_to_human_agent_id ON call_handoffs (to_human_agent_id);
CREATE INDEX IF NOT EXISTS ix_call_handoffs_status ON call_handoffs (status);

-- =============================================================================
-- 13. CALL RECORDINGS (metadata only — media stored in S3/MinIO)
-- =============================================================================

CREATE TABLE IF NOT EXISTS call_recordings (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    call_session_id       UUID NOT NULL REFERENCES call_sessions(id) ON DELETE CASCADE,
    storage_provider     VARCHAR(50) NOT NULL DEFAULT 's3',
    object_key           VARCHAR(1024) NOT NULL,
    content_type         VARCHAR(128),
    status              recording_status NOT NULL DEFAULT 'pending',
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at         TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_call_recordings_call_session_id ON call_recordings (call_session_id);
CREATE INDEX IF NOT EXISTS ix_call_recordings_status ON call_recordings (status);
CREATE INDEX IF NOT EXISTS ix_call_recordings_object_key ON call_recordings (object_key);

-- =============================================================================
-- 14. SUBSCRIPTIONS / ENTITLEMENTS / PLANS
-- =============================================================================

-- 14.1 PLANS (platform-defined plans)
CREATE TABLE IF NOT EXISTS plans (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name                VARCHAR(256) NOT NULL,
    tier                plan_tier NOT NULL DEFAULT 'starter',
    entitlements_json    JSONB,
    is_active            BOOLEAN NOT NULL DEFAULT TRUE,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_plans_tier ON plans (tier);
CREATE INDEX IF NOT EXISTS ix_plans_is_active ON plans (is_active);

-- 14.2 PARTNER PLANS (partner-defined plans/packages)
CREATE TABLE IF NOT EXISTS partner_plans (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    partner_id           UUID NOT NULL REFERENCES partners(id) ON DELETE CASCADE,
    name                VARCHAR(256) NOT NULL,
    entitlements_json    JSONB,
    is_active            BOOLEAN NOT NULL DEFAULT TRUE,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_partner_plans_partner_id ON partner_plans (partner_id);
CREATE INDEX IF NOT EXISTS ix_partner_plans_is_active ON partner_plans (is_active);

-- 14.3 SUBSCRIPTIONS (user ← platform plan relationship)
CREATE TABLE IF NOT EXISTS subscriptions (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id          UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    plan_id          UUID NOT NULL REFERENCES plans(id) ON DELETE CASCADE,
    status          subscription_status NOT NULL DEFAULT 'active',
    starts_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ends_at          TIMESTAMPTZ,
    trial_ends_at     TIMESTAMPTZ,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_subscriptions_user_id ON subscriptions (user_id);
CREATE INDEX IF NOT EXISTS ix_subscriptions_plan_id ON subscriptions (plan_id);
CREATE INDEX IF NOT EXISTS ix_subscriptions_status ON subscriptions (status);

-- 14.4 LICENSES (partner-assigned usage license for a customer user)
CREATE TABLE IF NOT EXISTS licenses (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id          UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    partner_id       UUID REFERENCES partners(id) ON DELETE SET NULL,
    partner_plan_id   UUID REFERENCES partner_plans(id) ON DELETE SET NULL,
    status          license_status NOT NULL DEFAULT 'active',
    starts_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ends_at          TIMESTAMPTZ,
    limits_json      JSONB,
    metadata_json    JSONB,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_licenses_user_id ON licenses (user_id);
CREATE INDEX IF NOT EXISTS ix_licenses_partner_id ON licenses (partner_id);
CREATE INDEX IF NOT EXISTS ix_licenses_status ON licenses (status);
CREATE INDEX IF NOT EXISTS ix_licenses_starts_at ON licenses (starts_at);

-- =============================================================================
-- 15. USAGE AND METERING (immutable ledger)
-- =============================================================================

CREATE TABLE IF NOT EXISTS usage_records (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id              UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    partner_id           UUID REFERENCES partners(id) ON DELETE SET NULL,
    license_id           UUID REFERENCES licenses(id) ON DELETE SET NULL,
    call_session_id       UUID REFERENCES call_sessions(id) ON DELETE SET NULL,
    idempotency_key      VARCHAR(128) NOT NULL,
    metric_type          metric_type NOT NULL,
    quantity            NUMERIC(18,4) NOT NULL,
    unit                VARCHAR(50) NOT NULL DEFAULT 'seconds',
    occurred_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    metadata_json        JSONB
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_usage_records_idempotency_key ON usage_records (idempotency_key);
CREATE INDEX IF NOT EXISTS ix_usage_records_user_id ON usage_records (user_id);
CREATE INDEX IF NOT EXISTS ix_usage_records_partner_id ON usage_records (partner_id);
CREATE INDEX IF NOT EXISTS ix_usage_records_call_session_id ON usage_records (call_session_id);
CREATE INDEX IF NOT EXISTS ix_usage_records_metric_type ON usage_records (metric_type);
CREATE INDEX IF NOT EXISTS ix_usage_records_occurred_at ON usage_records (occurred_at);
CREATE INDEX IF NOT EXISTS ix_usage_records_user_occurred ON usage_records (user_id, occurred_at);

-- =============================================================================
-- 16. RAG / KNOWLEDGE BASE
-- =============================================================================

-- 16.1 KNOWLEDGE BASES
CREATE TABLE IF NOT EXISTS knowledge_bases (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id          UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name            VARCHAR(256) NOT NULL,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_knowledge_bases_user_id ON knowledge_bases (user_id);

-- 16.2 KNOWLEDGE DOCUMENTS
CREATE TABLE IF NOT EXISTS knowledge_documents (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    knowledge_base_id     UUID NOT NULL REFERENCES knowledge_bases(id) ON DELETE CASCADE,
    name                VARCHAR(256) NOT NULL,
    source_uri           VARCHAR(2048) NOT NULL,
    content_type         VARCHAR(128) NOT NULL DEFAULT 'text/plain',
    metadata_json        JSONB,
    status              VARCHAR(50) NOT NULL DEFAULT 'pending',
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_knowledge_documents_knowledge_base_id ON knowledge_documents (knowledge_base_id);
CREATE INDEX IF NOT EXISTS ix_knowledge_documents_status ON knowledge_documents (status);

-- 16.3 KNOWLEDGE CHUNKS (embedding vectors for RAG)
CREATE TABLE IF NOT EXISTS knowledge_chunks (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    knowledge_document_id     UUID NOT NULL REFERENCES knowledge_documents(id) ON DELETE CASCADE,
    chunk_index              INTEGER NOT NULL,
    content                 TEXT NOT NULL,
    embedding               vector(1536),
    metadata_json            JSONB,
    created_at               TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_knowledge_chunks_doc_index
    ON knowledge_chunks (knowledge_document_id, chunk_index);
CREATE INDEX IF NOT EXISTS ix_knowledge_chunks_knowledge_document_id ON knowledge_chunks (knowledge_document_id);

-- 16.4 PERSONA ← KNOWLEDGE BASES (junction)
CREATE TABLE IF NOT EXISTS persona_knowledge_bases (
    persona_id           UUID NOT NULL REFERENCES personas(id) ON DELETE CASCADE,
    knowledge_base_id     UUID NOT NULL REFERENCES knowledge_bases(id) ON DELETE CASCADE,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (persona_id, knowledge_base_id)
);

-- =============================================================================
-- 17. LEGACY TABLES (kept for backward compatibility with existing endpoints)
-- =============================================================================

CREATE TABLE IF NOT EXISTS agent_users (
    id              SERIAL PRIMARY KEY,
    username        VARCHAR(256) NOT NULL UNIQUE,
    password_hash    TEXT NOT NULL,
    is_online        BOOLEAN NOT NULL DEFAULT FALSE,
    status          VARCHAR(50) NOT NULL DEFAULT 'offline',
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_agent_users_is_online ON agent_users (is_online);
CREATE INDEX IF NOT EXISTS ix_agent_users_status ON agent_users (status);

CREATE TABLE IF NOT EXISTS call_records (
    id                  SERIAL PRIMARY KEY,
    room_name            VARCHAR(256) NOT NULL,
    caller_id            VARCHAR(256),
    start_time           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    end_time             TIMESTAMPTZ,
    status              VARCHAR(50),
    summary             TEXT,
    recording_url        VARCHAR(1024),
    handled_by_agent_id    INTEGER REFERENCES agent_users(id)
);

CREATE INDEX IF NOT EXISTS ix_call_records_room_name ON call_records (room_name);
CREATE INDEX IF NOT EXISTS ix_call_records_status ON call_records (status);
CREATE INDEX IF NOT EXISTS ix_call_records_start_time ON call_records (start_time);

-- Seed default agent
INSERT INTO agent_users (username, password_hash, is_online, status)
VALUES ('admin', 'admin', FALSE, 'offline')
ON CONFLICT (username) DO NOTHING;

-- =============================================================================
-- 18. HELPER: updated_at trigger function
-- =============================================================================

CREATE OR REPLACE FUNCTION update_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply trigger to all tables with UpdatedAt column
DO $$
DECLARE
    t TEXT;
BEGIN
    FOR t IN
        SELECT table_name FROM information_schema.columns
        WHERE column_name = 'updated_at'
          AND table_schema = 'public'
          AND table_name NOT IN ('agent_users', 'call_records')
    LOOP
        EXECUTE format(
            'CREATE TRIGGER trg_%I_updated_at BEFORE UPDATE ON %I FOR EACH ROW EXECUTE FUNCTION update_timestamp()',
            t, t
        );
    END LOOP;
END $$;

COMMIT;