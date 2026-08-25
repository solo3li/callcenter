You are a senior Database Architect and PostgreSQL expert.

Design a complete, production-oriented, normalized PostgreSQL database schema and ERD for a multi-tenant AI Voice / AI Calling SaaS platform.

IMPORTANT:
I do NOT want an image.
I want a detailed TEXT-BASED ERD containing:
- All tables
- All columns
- PostgreSQL data types
- Primary keys
- Foreign keys
- Unique constraints
- Check constraints where necessary
- Important indexes
- Relationship cardinalities
- Recommended PostgreSQL enums
- JSONB fields only where flexibility is genuinely needed
- Clear explanation of why each table exists

The system must be designed for an MVP first, but the database architecture must remain clean and extensible for future scale.

==================================================
1. CORE ARCHITECTURE AND MULTI-TENANCY
==================================================

The main authenticated account is called USER.

A USER can use the platform directly.

A USER may optionally belong to a PARTNER.

Do NOT create separate customer account systems for direct customers and partner customers.

There must be ONE unified user/account model.

A user may have:
- No partner relationship
- A partner relationship
- A license associated with a partner
- A direct platform subscription
- Partner-provided usage limits

The database must support:

USER
  1:N API_KEYS
  1:N PERSONAS
  1:N WORKFLOWS
  1:N CALL_CONFIGURATIONS
  1:N HUMAN_AGENTS
  1:N CALL_SESSIONS
  1:N USAGE_RECORDS
  1:N LICENSES

A USER can also act as a PARTNER.

A PARTNER can have many customer users.

Design this cleanly without duplicating the USER table.

Consider a table such as:

PARTNER_RELATIONSHIPS

with:
- id
- partner_id -> users.id
- customer_user_id -> users.id
- status
- created_at
- updated_at

A user may be a direct platform customer and may have no partner.

==================================================
2. PARTNER PROGRAM AND LICENSE SYSTEM
==================================================

The partner system must be flexible.

A partner may:

1. Manually add customers.
2. Integrate programmatically using an API.
3. Automatically provision customers.
4. Automatically create API keys for sub-customers.
5. Include platform usage inside the partner's own packages.
6. Sell their own plans/packages.
7. Apply usage limits such as:
   - Minutes
   - Call count
   - Feature access
8. Have a license associated with each customer.

IMPORTANT BILLING MODEL:

The platform owner may bill the PARTNER directly instead of billing every partner customer individually.

The partner must be able to see usage of its customers.

The platform owner may see aggregated or administrative usage information.

The database must clearly distinguish:

- Platform billing relationship
- Partner/customer relationship
- License
- Plan
- Usage limit
- Actual usage

Design appropriate tables for:

PARTNERS or partner profile/organization
PARTNER_RELATIONSHIPS
PARTNER_PLANS
LICENSES
LICENSE_LIMITS or ENTITLEMENTS

However, keep the MVP simple.

Do NOT over-engineer subscriptions.

A LICENSE may optionally contain:
- user_id
- partner_id
- partner_plan_id
- status
- starts_at
- ends_at
- usage limits
- metadata

Determine the best normalized design.

==================================================
3. AUTHENTICATION AND API KEYS
==================================================

The main platform owns authentication.

Customers must always have an identity on the platform.

Partners do NOT fully own the platform identity.

A partner integration may provision a customer automatically, but that customer still has an account/identity in the main platform.

Support OAuth/OIDC or external identity linking conceptually if needed.

Do NOT make partner systems the primary identity provider by default.

API keys are used for platform integrations.

Design:

API_KEYS

Recommended fields:
- id UUID PK
- user_id FK
- name
- key_prefix
- key_hash
- status
- scopes
- last_used_at
- expires_at
- revoked_at
- created_at

Never store raw API keys.

Consider scopes as either normalized permissions or JSONB/array depending on best PostgreSQL design.

==================================================
4. HUMAN AGENT SYSTEM
==================================================

The platform has internal human agents.

IMPORTANT:

Human agents are NOT external SIP destinations.

There is NO external transfer system.

There is NO transfer to:
- phone numbers
- PSTN destinations
- external SIP endpoints

Human agents belong to a USER / account.

Example:

USER / ACCOUNT
  ├── AI Personas
  ├── Workflows
  └── Human Agents
        ├── Agent A
        ├── Agent B
        └── Agent C

Design:

HUMAN_AGENTS

Recommended fields:
- id UUID PK
- owner_user_id FK -> users.id
- optional application_user_id if future login support is desired
- name
- email optional
- status
- is_active
- max_concurrent_calls
- created_at
- updated_at

For MVP, a human agent may authenticate using an access key.

Design:

HUMAN_AGENT_ACCESS_KEYS

Fields:
- id UUID PK
- human_agent_id FK
- name
- key_prefix
- key_hash
- status
- expires_at
- last_used_at
- revoked_at
- created_at

Never store raw keys.

The human agent application uses the access key to authenticate with the backend.

The backend validates the key and issues a short-lived LiveKit token.

The access key itself must NOT be a LiveKit token.

Optionally design:

HUMAN_AGENT_SESSIONS

for persistent session history:
- id
- human_agent_id
- livekit_identity
- status
- connected_at
- disconnected_at
- last_heartbeat_at
- metadata

IMPORTANT:
Do not use PostgreSQL as the primary real-time presence system.

Real-time availability and active call counters may be managed in Redis.

PostgreSQL stores persistent configuration and historical data.

==================================================
5. LIVEKIT INTERNAL CALL ARCHITECTURE
==================================================

The system uses:

- LiveKit Server
- LiveKit SIP only for initial inbound phone entry
- LiveKit Python Agents for AI
- LiveKit Egress for recording
- Human Agents connect through the platform application using LiveKit Client SDK / WebRTC

The call flow is:

Customer
   ->
Phone/SIP Entry
   ->
LiveKit SIP
   ->
LiveKit Room
   ->
AI Agent

If AI decides to transfer:

AI Agent
   ->
Built-in transfer_to_human Action
   ->
Action Engine / Backend
   ->
Find eligible internal Human Agent
   ->
Notify Human Agent Application
   ->
Human accepts
   ->
Backend issues short-lived LiveKit token
   ->
Human joins the SAME LiveKit Room
   ->
Customer continues talking to Human Agent

IMPORTANT:

The call remains inside LiveKit.

There is no external transfer after the call enters the platform.

Do NOT design:
- SIP transfer destinations
- PSTN transfer destinations
- External phone transfer tables
- External transfer endpoints

The LiveKit room must be stored in the call session.

==================================================
6. CALL CONFIGURATION
==================================================

A USER may have multiple call configurations.

Design:

CALL_CONFIGURATIONS

Suggested fields:
- id UUID PK
- user_id FK
- name
- description
- persona_id FK nullable
- workflow_id FK nullable
- active_version_id or configuration version if needed
- is_active
- config JSONB only for flexible settings
- created_at
- updated_at

A call configuration may select:
- Persona
- Workflow
- Allowed Actions
- Human transfer behavior
- Other runtime settings

Avoid excessive duplication.

==================================================
7. CALL SESSIONS
==================================================

Design the central CALL_SESSIONS table.

Suggested relationships:

CALL_SESSION
  belongs to USER
  belongs to CALL_CONFIGURATION
  may reference PERSONA / PERSONA_VERSION
  may reference WORKFLOW / WORKFLOW_VERSION
  may reference API_KEY
  has many CALL_PARTICIPANTS
  has many CALL_TRANSFERS
  has many CALL_HANDOFFS
  has many ACTION_EXECUTIONS
  has many USAGE_RECORDS
  may have recordings

Suggested fields:

- id UUID PK
- user_id FK -> users.id
- call_configuration_id FK
- persona_id or persona_version_id
- workflow_id or workflow_version_id
- api_key_id nullable
- livekit_room_name
- livekit_room_sid nullable
- status
- direction
- started_at
- answered_at
- ended_at
- duration_seconds
- metadata JSONB
- created_at

The schema must preserve historical correctness.

If a Persona or Workflow changes later, an old call should still be associated with the exact version used during that call.

Therefore, determine whether version tables and version references are required.

==================================================
8. CALL PARTICIPANTS
==================================================

A call can have multiple participants.

Participant types:

- customer
- ai_agent
- human_agent

Design:

CALL_PARTICIPANTS

Fields:
- id UUID PK
- call_session_id FK
- human_agent_id nullable FK
- participant_type
- livekit_identity
- livekit_participant_sid nullable
- display_name nullable
- joined_at
- left_at
- created_at

A human agent participant must reference HUMAN_AGENTS.

A customer participant must not necessarily be a platform user.

Use appropriate nullable FKs and constraints.

Consider CHECK constraints to guarantee data consistency.

==================================================
9. INTERNAL HUMAN TRANSFER ONLY
==================================================

Design:

CALL_TRANSFERS

There is ONLY internal transfer to HUMAN_AGENTS.

Suggested fields:

- id UUID PK
- call_session_id FK
- from_participant_id FK nullable
- to_human_agent_id FK
- status
- reason
- failure_reason
- requested_at
- accepted_at
- completed_at
- failed_at
- created_at
- updated_at

Recommended statuses:

- requested
- ringing
- accepted
- completed
- rejected
- failed
- cancelled

A call may have multiple transfer attempts.

For example:

AI attempts Agent A
   ->
Agent A rejects
   ->
AI/Backend attempts Agent B

Therefore CALL_TRANSFERS must support multiple rows per CALL_SESSION.

==================================================
10. CALL HANDOFF CONTEXT
==================================================

When AI transfers a call to a human, the human should receive context.

Design:

CALL_HANDOFFS

Suggested fields:

- id UUID PK
- call_session_id FK
- call_transfer_id FK UNIQUE
- from_participant_id FK nullable
- to_human_agent_id FK
- reason
- summary TEXT
- context_data JSONB
- status
- created_at
- delivered_at
- accepted_at

The context may contain:

- AI-generated summary
- Customer intent
- Actions already executed
- Relevant workflow state
- Important variables
- Customer information
- Order or business context

Do not store sensitive data unnecessarily.

==================================================
11. PERSONA ENGINE
==================================================

The platform has a Persona Engine.

A USER can create multiple PERSONAS.

A persona may contain:

- name
- description
- system prompt
- model/runtime configuration
- voice configuration
- behavior settings
- metadata

The system must support versioning.

Design:

PERSONAS
PERSONA_VERSIONS

A call should reference the exact Persona Version used.

Suggested PERSONA_VERSION fields:

- id UUID PK
- persona_id FK
- version_number
- system_prompt
- configuration JSONB
- is_published
- created_at

Determine the best way to mark current/published versions.

==================================================
12. WORKFLOW ENGINE
==================================================

The platform has a Workflow Engine.

A workflow is owned by a USER.

A workflow may be attached to:

- Call Configuration
- Persona if appropriate
- Action execution

Support versioning if required for historical correctness.

Design:

WORKFLOWS
WORKFLOW_VERSIONS
WORKFLOW_EXECUTIONS

Suggested execution fields:

- id UUID PK
- workflow_id or workflow_version_id FK
- call_session_id nullable FK
- status
- input JSONB
- output JSONB
- state JSONB if necessary
- started_at
- completed_at
- error

A workflow definition may use JSONB because workflow graphs/nodes are flexible.


Do not unnecessarily normalize every workflow node unless there is a strong reason.

Explain the design choice.

==================================================
13. ACTION ENGINE
==================================================

The AI can invoke actions.

Actions may be:

- Built-in system actions
- User-defined workflow actions
- Integration actions
- Webhook/API actions

Built-in actions include:

- transfer_to_human
- end_call

The AI does NOT directly perform sensitive backend operations.

Instead:

AI Agent
   ->
Requests Action
   ->
Backend Action Engine validates
   ->
Action executes
   ->
Result returned

Design:

ACTION_DEFINITIONS

Fields may include:

- id UUID PK
- name
- display_name
- description
- action_type
- is_system
- input_schema JSONB
- output_schema JSONB
- configuration JSONB if needed
- is_active
- created_at
- updated_at

Design the relationship between:

PERSONAS
CALL_CONFIGURATIONS
ACTION_DEFINITIONS

Use a junction table such as:

PERSONA_ACTIONS or CALL_CONFIGURATION_ACTIONS

if appropriate.

Also design:

ACTION_EXECUTIONS

Fields:

- id UUID PK
- call_session_id FK
- action_definition_id FK
- workflow_execution_id nullable FK
- status
- input JSONB
- output JSONB
- error
- started_at
- completed_at

For transfer_to_human, the Action Engine creates a CALL_TRANSFER and orchestrates the internal handoff.

==================================================
14. RAG / KNOWLEDGE BASE SUPPORT
==================================================

The platform may have a RAG system.

A USER can own:

- Knowledge Bases
- Documents
- Document chunks

A Persona or Call Configuration may attach one or more knowledge bases.

Design a clean schema for:

KNOWLEDGE_BASES
KNOWLEDGE_DOCUMENTS
KNOWLEDGE_CHUNKS
PERSONA_KNOWLEDGE_BASES or CALL_CONFIGURATION_KNOWLEDGE_BASES

Consider PostgreSQL with pgvector.

The embedding field should support vector search.

Keep document/chunk metadata flexible using JSONB.

Do not over-design the ingestion pipeline unless necessary.

==================================================
15. RECORDINGS AND MEDIA
==================================================

Media and recordings are stored in external S3-compatible object storage.

Do NOT store media blobs in PostgreSQL.

The database should only store metadata and object references.

Design:

CALL_RECORDINGS

Fields:

- id UUID PK
- call_session_id FK
- storage_provider
- object_key
- content_type
- duration_seconds
- size_bytes
- status
- created_at
- completed_at

Potentially support multiple recording segments if necessary.

==================================================
16. USAGE AND METERING
==================================================

The platform uses usage-based metering.

Track at minimum:

- Call minutes
- Call duration
- Other billable resources if needed

The usage model must support:

Direct User:
- Usage billed directly to platform user.

Partner Customer:
- Usage attributed to the customer user.
- Usage can also roll up to the partner for billing.

Design:

USAGE_RECORDS

Suggested fields:

- id UUID PK
- user_id FK
- partner_id nullable FK
- license_id nullable FK
- call_session_id nullable FK
- metric_type
- quantity
- unit
- occurred_at
- metadata JSONB

IMPORTANT:

Avoid double-counting.

Design idempotency / unique constraints where appropriate.

Explain whether an immutable usage ledger is recommended.

==================================================
17. SUBSCRIPTIONS / ENTITLEMENTS / PLANS
==================================================

The system may have:

- Platform plans
- Partner plans
- Licenses
- Usage limits

Keep these concepts separate.

Possible structure:

PLANS
PLAN_ENTITLEMENTS
SUBSCRIPTIONS
LICENSES
LICENSE_ENTITLEMENTS

But simplify if some concepts overlap.

The existing product architecture may have fixed wallets/credits, so do not force a completely separate dynamic wallet model unless justified.

The database should support entitlement checks such as:



- Is human transfer allowed?
- Maximum human agents
- Maximum concurrent calls
- Monthly minutes
- Recording access
- Knowledge base limits
- Feature access

The backend must be able to efficiently answer:

"Can this user use this feature right now?"

==================================================
18. EXISTING USER WALLET / CREDIT MODEL
==================================================

The platform already uses two fixed credit balances:

- standard_credits
- premium_credits

These are conceptually fixed wallets.

Tool configuration determines which credit type can be consumed.

Do NOT redesign the system into dynamic wallet tables unless there is a strong architectural reason.

If the calling system needs credits or billing integration, design it in a way compatible with these two fixed balances.

Usage/billing and credit deduction must be idempotent.

==================================================
19. PARTNER CUSTOMER PROVISIONING
==================================================

A partner may integrate through an API.

Example flow:

Partner System
   ->
Partner API Key
   ->
Create/Provision Customer
   ->
Platform creates or links USER
   ->
Create PARTNER_RELATIONSHIP
   ->
Assign LICENSE / PARTNER PLAN
   ->
Generate platform API Key if needed
   ->
Return provisioning result

The customer remains a platform identity.

Design appropriate idempotency support.

Consider:

PARTNER_EXTERNAL_CUSTOMERS

only if necessary to map:
- partner external customer ID
- platform user ID

Do not create this table unless it provides a real architectural benefit.

If used, enforce uniqueness:

(partner_id, external_customer_id)

==================================================
20. REAL-TIME VS DATABASE RESPONSIBILITY
==================================================

The database must store durable state.

Redis should handle real-time state such as:

- Human agent presence
- Available/busy state
- Active call counters
- Temporary routing locks
- Distributed locks
- Short-lived session state

Do NOT continuously update PostgreSQL for every heartbeat.

The ERD should clearly distinguish:

Persistent PostgreSQL state
vs
Ephemeral Redis state

==================================================
21. DATA CONSISTENCY REQUIREMENTS
==================================================

Use PostgreSQL best practices.

Prefer:

- UUID primary keys
- TIMESTAMPTZ
- Foreign keys
- UNIQUE constraints
- CHECK constraints
- Partial unique indexes where useful
- Composite indexes for common tenant queries
- Idempotency keys for external requests and usage events
- Immutable ledger patterns where appropriate

Every tenant-owned resource must have a clear ownership boundary.

For example, prevent cross-tenant references such as:

- A Call Session belonging to User A referencing a Persona owned by User B
- A Call Transfer targeting a Human Agent owned by another User

Where PostgreSQL FKs alone cannot enforce ownership consistency, explain the recommended enforcement strategy:

- Composite foreign keys
- Tenant-aware IDs
- Database constraints
- Service-layer validation
- Row-level security if appropriate

Recommend the simplest safe approach for an MVP.

==================================================
22. SCALE REQUIREMENTS
==================================================

The initial deployment architecture is approximately:

- ASP.NET Core Backend
- PostgreSQL external/managed provider
- Redis
- LiveKit Server
- LiveKit SIP for inbound calls
- Python LiveKit AI Workers
- LiveKit Egress
- S3-compatible external object storage
- Docker deployment initially

The database should support an MVP that can grow toward hundreds and eventually thousands of concurrent calls.

Do NOT over-engineer for massive global scale from day one.

Optimize for:

- Simple implementation
- Clear ownership
- Correct billing
- Historical auditability
- Efficient indexes
- Future scalability

==================================================
23. REQUIRED OUTPUT FORMAT
==================================================


Your answer must be structured exactly as follows:

1. Architecture Summary

2. Design Principles

3. Recommended PostgreSQL Enums

For each enum:
- enum name
- values
- why it exists

4. Complete Text-Based ERD

Show all relationships in a readable format, for example:

USERS
  1:N PERSONAS
  1:N HUMAN_AGENTS
  1:N CALL_SESSIONS

PERSONAS
  1:N PERSONA_VERSIONS

CALL_SESSIONS
  1:N CALL_PARTICIPANTS
  1:N CALL_TRANSFERS
  1:N ACTION_EXECUTIONS

5. Complete Table Definitions

For EVERY table, show:

TABLE: table_name

Purpose:
...

Columns:

- column_name DATA_TYPE
  PK / FK / UNIQUE / NOT NULL / NULL
  References: table.column if applicable
  Description: ...

Then show:

Constraints:
- ...

Indexes:
- ...

Relationships:
- ...

6. Detailed Relationship Rules

Explicitly explain:
- User vs Partner
- Partner vs Customer
- User vs License
- User vs Human Agent
- Call Session vs Participants
- Call Session vs Transfer
- Transfer vs Human Agent
- Handoff vs Transfer
- Persona Version vs Call
- Workflow Version vs Execution
- Action vs Execution
- Knowledge Base ownership
- Usage attribution
- Partner billing rollup

7. Internal Human Transfer Flow

Explain step-by-step:

Customer
-> LiveKit SIP
-> LiveKit Room
-> AI Agent
-> transfer_to_human
-> Action Engine
-> Find Human Agent
-> Human Agent Application
-> Short-lived LiveKit Token
-> Human joins SAME room
-> AI exits or becomes inactive
-> Customer continues with Human

8. Partner Provisioning Flow

Explain:
- Partner API authentication
- Customer provisioning
- User creation/linking
- Partner relationship
- License assignment
- API key generation
- Idempotency

9. Billing and Usage Flow

Explain:
- Direct user billing
- Partner customer usage attribution
- Partner billing
- Usage aggregation
- Idempotency
- Credit compatibility

10. Redis vs PostgreSQL Responsibilities

Explicitly list which data belongs in Redis and which belongs in PostgreSQL.

11. MVP vs Future Tables

Classify every major table as:

MVP REQUIRED
or
FUTURE / OPTIONAL

Do not add unnecessary tables just because they are theoretically useful.

12. Final Recommended MVP Schema

At the end, provide a concise list of only the tables that should actually be implemented for the first MVP.

==================================================
FINAL CRITICAL RULES
==================================================

- PostgreSQL only.
- Use snake_case table and column names.
- Use UUID primary keys unless there is a strong reason not to.
- Use TIMESTAMPTZ for timestamps.
- Do not store passwords or raw API keys.
- Do not store raw access keys.
- Do not store media blobs in PostgreSQL.
- Do not design any external call transfer system.
- Human transfer is INTERNAL ONLY.
- Human agents connect through LiveKit WebRTC.
- The human joins the existing LiveKit call room.
- SIP is only used for initial call entry.
- The backend owns authorization and orchestration.
- The AI requests actions but does not bypass backend authorization.
- Preserve historical versions of Personas and Workflows.
- Avoid excessive micro-tables and over-normalization.
- Use JSONB only for genuinely flexible data.
- Clearly identify every PK and FK.
- Add indexes for all important foreign keys and common tenant queries.
- Design for simplicity first, but avoid architectural decisions that would require a complete database rewrite later.