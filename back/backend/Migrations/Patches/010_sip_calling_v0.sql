-- v0 SIP calling foundation: inbound trunk ownership, named destinations, call legs,
-- cold-transfer extensions. Fully idempotent.

-- ── users.default_persona_id ────────────────────────────────────────────────
ALTER TABLE users ADD COLUMN IF NOT EXISTS default_persona_id uuid REFERENCES personas(id) ON DELETE SET NULL;
CREATE INDEX IF NOT EXISTS IX_users_default_persona_id ON users(default_persona_id);

-- ── sip_connections ────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS sip_connections (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name varchar(128) NOT NULL,
    allowed_ips text[] NOT NULL DEFAULT '{}',
    numbers text[] NOT NULL DEFAULT '{}',
    lk_trunk_id varchar(64) NULL,
    dispatch_rule_id varchar(64) NULL,
    is_active boolean NOT NULL DEFAULT true,
    max_concurrent_calls integer NOT NULL DEFAULT 10,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_sip_connections_user_id_name ON sip_connections(user_id, name);
CREATE UNIQUE INDEX IF NOT EXISTS IX_sip_connections_lk_trunk_id ON sip_connections(lk_trunk_id);
CREATE INDEX IF NOT EXISTS IX_sip_connections_is_active ON sip_connections(is_active);

-- ── sip_destinations ───────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS sip_destinations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name varchar(128) NOT NULL,
    description varchar(256) NULL,
    call_to varchar(256) NOT NULL,
    is_enabled boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_sip_destinations_user_id_name ON sip_destinations(user_id, name);
CREATE INDEX IF NOT EXISTS IX_sip_destinations_is_enabled ON sip_destinations(is_enabled);

-- ── call_sessions additions ────────────────────────────────────────────────
ALTER TABLE call_sessions ADD COLUMN IF NOT EXISTS dialed_number varchar(32) NULL;
ALTER TABLE call_sessions ADD COLUMN IF NOT EXISTS origin_sip_connection_id uuid REFERENCES sip_connections(id) ON DELETE SET NULL;
CREATE INDEX IF NOT EXISTS IX_call_sessions_origin_sip_connection_id ON call_sessions(origin_sip_connection_id);

-- ── call_legs ──────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS call_legs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    call_session_id uuid NOT NULL REFERENCES call_sessions(id) ON DELETE CASCADE,
    leg_index integer NOT NULL,
    kind varchar(32) NOT NULL,
    participant_identity varchar(256) NULL,
    started_at timestamptz NOT NULL DEFAULT now(),
    answered_at timestamptz NULL,
    ended_at timestamptz NULL,
    hangup_cause varchar(128) NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_call_legs_session_leg ON call_legs(call_session_id, leg_index);
CREATE INDEX IF NOT EXISTS IX_call_legs_kind ON call_legs(kind);

-- ── call_transfers extensions ──────────────────────────────────────────────
ALTER TABLE call_transfers ALTER COLUMN to_human_agent_id DROP NOT NULL;
ALTER TABLE call_transfers ADD COLUMN IF NOT EXISTS mode varchar(16) NOT NULL DEFAULT 'Cold';
ALTER TABLE call_transfers ADD COLUMN IF NOT EXISTS target_type varchar(32) NOT NULL DEFAULT 'HumanAgent';
ALTER TABLE call_transfers ADD COLUMN IF NOT EXISTS destination_id uuid REFERENCES sip_destinations(id) ON DELETE SET NULL;
ALTER TABLE call_transfers ADD COLUMN IF NOT EXISTS target_snapshot_json jsonb NULL;
CREATE INDEX IF NOT EXISTS IX_call_transfers_destination_id ON call_transfers(destination_id);

-- Rebuild human-agent FK as SetNull (was Cascade when column was NOT NULL).
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'FK_call_transfers_human_agents_ToHumanAgentId'
    ) THEN
        ALTER TABLE call_transfers DROP CONSTRAINT FK_call_transfers_human_agents_ToHumanAgentId;
    END IF;
END $$;
ALTER TABLE call_transfers
    DROP CONSTRAINT IF EXISTS FK_call_transfers_human_agents_ToHumanAgentId;
ALTER TABLE call_transfers
    ADD CONSTRAINT FK_call_transfers_human_agents_ToHumanAgentId
    FOREIGN KEY (to_human_agent_id) REFERENCES human_agents(id) ON DELETE SET NULL;
