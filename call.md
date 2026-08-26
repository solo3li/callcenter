# Tandem — AI + Human Call Center Platform

**Complete technical reference** · generated from a fully verified end-to-end session (build, deploy, endpoint audit, live smoke tests). **Kept current**: updated for the v0 LiveKit-SIP integration (Asterisk removed, inbound PSTN→AI routing, cold-swap transfers).

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Architecture](#2-architecture)
3. [Database & Enums](#3-database--enums)
4. [API Reference — All 158 Endpoints](#4-api-reference)
5. [Business Logic & Scenarios](#5-business-logic--scenarios)
6. [Dashboard Frontend](#6-dashboard-frontend)
7. [Agent App (Expo / React Native)](#7-agent-app)
8. [AI Worker & Legacy Pages](#8-ai-worker--legacy-pages)
9. [Operations Guide](#9-operations-guide)
10. [Verified E2E Scenarios](#10-verified-e2e-scenarios)
11. [Fix Log](#11-fix-log)
12. [LiveKit-SIP Integration Guide (v0)](#12-livekit-sip-integration-guide-v0)

---

## 1. Project Overview

Tandem is a multi-tenant **AI-first call center platform**: AI voice agents (Gemini Live via LiveKit) answer phone calls, resolve most of them end-to-end, and escalate the rest to human agents with handoff context. A React dashboard gives operators live monitoring, analytics, platform setup (personas, workflows, knowledge bases, call configurations) and business views (usage, API keys, licensing).

### Tech Stack

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core (**.NET 10**), Minimal APIs, EF Core 9 + Npgsql |
| Database | PostgreSQL 15 + **pgvector** (1536-dim embeddings for RAG) |
| Background jobs | Hangfire (+ PostgreSQL storage) |
| Realtime | SignalR hub (`/hubs/call`) |
| Media/RTC | LiveKit (WebRTC SFU), **LiveKit SIP — sole SIP gateway (:5061)** |
| Recording | LiveKit Egress → MinIO (S3-compatible) |
| AI | Python worker · Gemini Live (`models/gemini-3.1-flash-live-preview`), OpenAI `text-embedding-3-small` |
| Dashboard | React 19 · Vite 7 · Tailwind CSS 4 · react-router 7 · @microsoft/signalr |
| Agent app | Expo SDK 57 · React Native 0.86 · @livekit/react-native (native) / livekit-client (web) |
| Reverse proxy | Caddy |

### Repository Layout

```
testviop/
├── call.md                  ← this file
├── erd.md                   ← original DB design spec
├── frontend/                ← operator dashboard (React/Vite)
│   └── src/
│       ├── api/             client.ts, endpoints.ts
│       ├── auth/            AuthContext, RequireAuth
│       ├── hooks/           useApi, useLiveHub
│       ├── pages/           LoginPage
│       └── dashboard/
│           ├── pages/       13 page components (incl. SipDestinationsPage)
│           ├── components/  StatCard, Badge, ui.tsx primitives…
│           └── data.ts      demo/mock dataset
└── back/
    ├── docker-compose.yml   10-service stack (+ sip-setup one-shot profile)
    ├── livekit.yaml         LiveKit config incl. webhooks → backend
    ├── sip-setup.py         idempotent trunk/dispatch-rule bootstrap
    ├── Caddyfile            :8080 router
    ├── backend/             ASP.NET API (+ legacy wwwroot pages)
    │   ├── Endpoints/       22 endpoint groups (158 routes)
    │   ├── Services/        business logic (incl. InboundRoutingService)
    │   ├── Data/            AppDbContext + DbPatchRunner (patch migrations)
    │   ├── Models/          Domain entities + Enums
    │   ├── Hubs/            CallHub (SignalR)
    │   └── Migrations/Patches/  embedded ordered SQL patches
    ├── agent-app/           Expo mobile+web agent client
    └── python-ai-worker/    Gemini Live voice agent
```

---

## 2. Architecture

### Service Topology

```
                          ┌─────────────────────────────┐
   Browser :5173 ───────▶│  Caddy :8080                │
   Expo web :8081 ──────▶│  /rtc,/twirp → livekit:7880 │
                          │  else        → backend:5000 │
                          └─────────────────────────────┘
                                          │
                              ┌───────────▼────────────┐
                              │  backend (.NET 10)     │
                              │  REST · SignalR ·      │
                              │  Hangfire · webhooks   │
                              └───┬─────────┬──────────┘
                     ┌────────────┘         │
               ┌─────▼─────┐          ┌────▼────────┐
               │ Postgres  │          │   Redis     │
               │ +pgvector │          │ (LiveKit,   │
               │  :5432    │          │  egress,    │
               └───────────┘          │  livekit-sip)│
                                       └─────────────┘
                                           │
        ┌─────────────────────┬────────────┼───────────────────┐
        │                     │            │                   │
  ┌─────▼──────┐   ┌──────────▼──┐   ┌─────▼─────┐   ┌─────────▼───┐
  │  LiveKit   │   │ LiveKit SIP │   │  Egress   │   │ MinIO S3    │
  │ :7880-7882 │◄─►│   :5061     │   │ recorder  │   │ :9000/:9001 │
  │  dispatch/ │   │  UDP+TCP    │   └───────────┘   └─────────────┘
  │  webhooks  │   └──────▲──────┘        │ recordings
  └─────▲──────┘          │ SIP           ▼
        │                 │         MinIO bucket "recordings"
  ┌─────┴────────┐  ┌─────┴─────────┐
  │ python-ai-   │  │ Customer PBX  │
  │ worker       │  │ (Issabel)     │
  │ (Gemini)     │  │ queues/IVR    │
  └──────────────┘  └───────────────┘
```

**Inbound call path**: PBX → `livekit-sip:5061` → trunk match (IP allow-list) → dispatch rule creates room `call_u{userId}<rand>` and answers → LiveKit **webhook** `participant_joined` → backend resolves owner from the room prefix, looks up `users.default_persona_id`, calls **CreateAgentDispatch** (`voice-agent`) with `{sessionId, personaId}` metadata → worker joins as identity `ai-agent` and greets.

### Request Paths

| Client | URL | Path |
|---|---|---|
| Dashboard (Vite dev) | `http://localhost:5173` → `VITE_API_URL=http://localhost:8080` | browser → Caddy → backend |
| Agent app (web) | `http://localhost:8081` → `http://127.0.0.1:5000` | browser → published port → backend |
| Agent app (Android emu) | `http://10.0.2.2:5000` | emulator loopback → host → backend |
| Agent app (iOS sim) | `http://localhost:5000` | direct |
| AI worker | `ws://livekit:7880`, `BACKEND_URL=http://backend:5000` | internal docker net |
| Customer PBX | SIP INVITE → host `:5061/udp+tcp` | published livekit-sip port |
| LiveKit server | webhook POSTs → `http://backend:5000/api/webhooks/livekit` | internal docker net |

### CORS (critical)

Backend runs in Production mode inside Docker; policy = `WithOrigins(CORS_ORIGINS).AllowAnyHeader().AllowAnyMethod().AllowCredentials()`.

Allowed origins (docker-compose `CORS_ORIGINS`):
`http://localhost:5173` (dashboard) · `http://localhost:3000` · `http://localhost:8081` (Expo web)

SignalR browser clients default `withCredentials:true` → `AllowCredentials()` is mandatory. The agent-app's web SignalR sets `withCredentials:false`.

### Realtime Events (SignalR `/hubs/call`)

| Event | Audience | Payload |
|---|---|---|
| `QueueUpdate` | ALL clients | `{ activeCount, agentsOnline, activeCalls:[{id,roomName,status,startTime,durationSeconds}], agents:[{id,name,Status}] }` |
| `IncomingTransfer` | group `agent_{id}` | transfer + room details (triggers ring screen) |
| `TransferExpired` | group `agent_{id}` | `{ transferId }` — clears ringing UI on timeout |

Hub methods: `RegisterAgent(agentId)` (joins group, creates session row, status→Available); `OnDisconnectedAsync` (leaves group, session row closed, status→Offline).

---

## 3. Database & Enums

PostgreSQL database `callcenter` (user `admin`). Schema is applied by **`Data/DbPatchRunner`**: embedded ordered SQL patches from `Migrations/Patches/*.sql`, tracked in the `_schema_patches` history table. Fresh databases receive every patch (`000_base` → …); legacy deployments (base tables already present) are auto-baselined — the runner records `000_base` without re-executing it. New schema changes ship as a new numbered patch file. Column names are PascalCase (quoted identifiers). EF model is the source of truth.

### Core Tables

| Table | Purpose | Key columns |
|---|---|---|
| `users` | tenant owners (email login) | Id, Email, DisplayName, Status, IsPartner, StandardCredits, PremiumCredits, **DefaultPersonaId** (inbound routing persona) |
| `sip_connections` | inbound customer PBX trunks | UserId, Name, AllowedIps[], Numbers[], LkTrunkId (unique), DispatchRuleId, IsActive, MaxConcurrentCalls |
| `sip_destinations` | named external transfer targets | UserId, Name (unique per user), CallTo (**never exposed to AI layer**), IsEnabled |
| `call_legs` | ordered media sides per session | CallSessionId, LegIndex (unique pair), Kind (`PstnIn/PstnOut/WebrtcAgent/AiWorker/SipExternal` as varchar), ParticipantIdentity, StartedAt/AnsweredAt/EndedAt, HangupCause |
| `call_sessions` | every call | Id, UserId, CallConfigurationId, PersonaVersionId, WorkflowVersionId, ApiKeyId, LivekitRoomName, DialedNumber?, OriginSipConnectionId?, Status, Direction, StartedAt, AnsweredAt, EndedAt, DurationSeconds, MetadataJson (jsonb) |
| `call_participants` | joiners per call | CallSessionId, HumanAgentId?, ParticipantType (AiAgent/Human), LivekitIdentity, JoinedAt, LeftAt |
| `call_transfers` | escalations + external transfers | CallSessionId, FromParticipantId, **ToHumanAgentId (nullable)**, **Mode (`Cold/Warm`)**, **TargetType (`HumanAgent/ExternalDestination`)**, **DestinationId?**, **TargetSnapshotJson**, Status, Reason, FailureReason, RequestedAt/AcceptedAt/CompletedAt/FailedAt |
| `call_handoffs` | AI summary package | CallTransferId, ToHumanAgentId, Summary, ContextDataJson, Status, DeliveredAt, AcceptedAt |
| `call_recordings` | egress artifacts | StorageProvider, ObjectKey, DurationSeconds, SizeBytes, Status |
| `human_agents` | staff | OwnerUserId, Name, Email, Status, IsActive, MaxConcurrentCalls |
| `human_agent_access_keys` | mobile login keys | HumanAgentId, KeyHash (SHA-256 hex), Status, ExpiresAt, LastUsedAt |
| `human_agent_sessions` | SignalR presence | HumanAgentId, LivekitIdentity, ConnectedAt, DisconnectedAt |
| `personas` / `persona_versions` | AI identity + prompt versions | versions carry SystemPrompt, ConfigurationJson, VersionNumber, IsPublished |
| `workflows` / `workflow_versions` | call flow definitions | versions carry DefinitionJson, IsPublished |
| `call_configurations` | binds line→persona/workflow | PersonaId?, WorkflowId?, ConfigJson, IsActive |
| `call_configuration_actions` | actions exposed per config | CallConfigurationId, ActionDefinitionId |
| `action_definitions` | system + custom tools | ActionType (System/Integration/Webhook), InputSchemaJson… |
| `persona_actions` | persona↔action links | |
| `knowledge_bases` / `knowledge_documents` / `knowledge_chunks` | RAG store | chunks: Content, **Embedding vector(1536)**, MetadataJson jsonb |
| `persona_knowledge_bases` | persona↔KB links | |
| `api_keys` | machine auth | KeyPrefix, KeyHash, Scopes[], Status, ExpiresAt |
| `licenses` | entitlement windows | PartnerId?, Status, StartsAt, EndsAt, LimitsJson |
| `partners` / `partner_plans` / `partner_relationships` / `partner_customers` | B2B tier | |
| `plans` / `subscriptions` | billing tiers | EntitlementsJson; sub has TrialEndsAt |
| `usage_records` | metering events | MetricType, Quantity, Unit, OccurredAt, IdempotencyKey |
| `action_executions` | tool run log | Status, InputJson, OutputJson, Error |
| `hangfire.*` | job storage | Hash holds recurring-job cron entries |
| `_schema_patches` | patch-runner history | name (PK), applied_at |

### Enums (numeric wire values — verified from source)

```csharp
CallSessionStatus   0 Queued · 1 Ringing · 2 Active · 3 Transferred · 4 Completed · 5 Failed · 6 Cancelled
CallDirection       0 Inbound · 1 Outbound
CallTransferStatus  0 Requested · 1 Ringing · 2 Accepted · 3 Completed · 4 Rejected · 5 Failed · 6 Cancelled
HumanAgentStatus    0 Offline · 1 Available · 2 Break · 3 NotReady · 4 InCall
AccessKeyStatus     0 Active · 1 Revoked · 2 Expired
ParticipantType     AiAgent · Human            (serialized as string)
```

> ⚠️ Serialization is inconsistent by endpoint: `/api/human-agents` returns `Status` as a **number**, while list/detail DTOs built with `.ToString()` return strings. The dashboard normalizes both (see §6).

---

## 4. API Reference

Conventions:
- **Auth** = JWT bearer (`Authorization: Bearer …`) resolved by `AuthMiddleware` into `HttpContext.Items["UserId"]`; anonymous routes are marked.
- Errors: `401` unauthorized · `404` not-found · `400 {error}` or ValidationProblemDetails · `409` conflicts.
- Success bodies are camelCase JSON.

### 4.1 Auth — `/api/auth` (6)

| Method & Route | Auth | Contract |
|---|---|---|
| POST `/register` | anon (rate-limited) | req `{email,password,displayName}` → 200 `AuthResponse{accessToken,refreshToken,expiresAt,user{…}}`; 409 if email exists |
| POST `/login` | anon | same response; 401 bad creds |
| POST `/refresh` | Bearer (old token) | → fresh `AuthResponse` |
| POST `/logout` | anon | `{message:"Logged out"}` (stateless no-op) |
| GET `/me` | ✔ | `UserDto{id,email,displayName,companyName,status,isPartner,standardCredits,premiumCredits,createdAt}` |
| POST `/agent-login` | anon | req `{accessKey}` → SHA-256 lookup in `human_agent_access_keys` → 200 `AgentLoginResponse{agentId,name,livekitToken,livekitUrl,ownerUserId}`; updates LastUsedAt; 401 invalid/expired/revoked |

### 4.2 Stats — `/api/stats` (7) + health (1)

| Route | Contract |
|---|---|
| GET `/today` | `TodayStatsResponse{totalCalls,activeCalls,answeredCalls,transferredCalls,missedCalls,avgDurationSeconds,agentsOnline,hourly[{hour,count}]}` (scoped to UserId) |
| GET `/queue` | `QueueStatsResponse{activeCount,agentsOnline,activeCalls[{id,roomName,callerId,status,startTime,durationSeconds}],agents[{id,name,Status}]}` |
| GET `/agents` | `AgentStatsDto[]{agentId,name,status,totalCalls,avgDurationSeconds,lastActiveAt}` (status here = `"Active"/"Inactive"` string) |
| GET `/period?from&to` | `PeriodStatsResponse{from,to,totalCalls,completedCalls,avgDurationSeconds,hourly[]}` |
| GET `/summary` | `SummaryStatsResponse{totalCallsToday,thisWeek,thisMonth,totalUsageHours,activeSubscriptions,totalKnowledgeBases}` |
| GET `/hourly?date` | `HourlyDataPoint[]{hour,count}` (24 buckets) |
| GET `/intents?from&to` | `IntentStatsDto[]{intent,count,percentage}` |
| GET `/api/health` | anon · `HealthCheckResponse{status,database,redis,livekit,uptime,version}` |

### 4.3 Call Sessions — `/api/calls` (7)

| Method & Route | Contract |
|---|---|
| GET `/?status&direction&from&to&page&limit` | `{items:[CallSessionListItem],totalCount,page,limit}` — item: `{id,userId,callConfigurationId,livekitRoomName,status,direction,startedAt,answeredAt,endedAt,durationSeconds,metadataJson,participantCount,createdAt}` |
| GET `/{id}` | `CallSessionDetail`: list item fields + `callConfigurationName`, `participants[]`, `transfers[]`, `recordings[]`, `handoff?` (full child DTOs, §4.4–4.6) |
| GET `/active` | `ActiveCallDto[]` where status ∈ {Queued,Ringing,Active,Transferred} |
| POST `/{id}/end` | sets EndedAt/DurationSeconds/Status=Completed → `{id,status,durationSeconds,endedAt}` |
| PATCH `/{id}/metadata` | req `{metadataJson:"<string>"}` (dashboard stores agent notes here) |
| GET `/{id}/participants[/{pid}]` | participant DTOs `{id,humanAgentId,participantType,livekitIdentity,livekitParticipantSid,displayName,joinedAt,leftAt,createdAt}` |

### 4.4 Transfers — `/api/calls/{callSessionId}/transfers` (6)

| Method & Route | Contract |
|---|---|
| POST `/` | req `{reason?}` — picks next Available agent, creates Transfer(Requested)+Handoff(summary), fires `IncomingTransfer` → 201 `{transfer:{id,…},handoff?}`; 400 `"No human agents available"` etc. |
| GET `/` , GET `/{transferId}` | transfer DTOs incl. `toHumanAgentName,status,requestedExceptions…` |
| POST `/{transferId}/accept` | req `{humanAgentId}` → Accepted (agent-app answers) |
| POST `/{transferId}/reject` | req `{humanAgentId}` → cascade to next available agent |
| POST `/{transferId}/complete` | agent finished → Completed |

### 4.5 Handoffs — `/api/calls/{callSessionId}/handoffs` (5)

POST `/{transferId}` create (`CreateHandoffRequest{summary,contextDataJson?,reason?}`) · GET `/` list · GET `/{handoffId}` · POST `/{handoffId}/deliver` · POST `/{handoffId}/accept`. Handoff detail adds `toHumanAgentName,status,deliveredAt,acceptedAt`.

### 4.6 Recordings — `/api/calls/{callSessionId}/recordings` (5)

GET `/` list · GET `/{recordingId}` · GET `/{recordingId}/download` → `{url}` (MinIO presigned) · POST `/` egress callback (`RecordingCallbackRequest`) · DELETE `/{recordingId}`.

### 4.7 Human Agents — `/api/human-agents` (11)

| Route | Notes |
|---|---|
| GET `/` · GET `/{id}` | ⚠ `status` serialized as **number** (§3 enum) |
| POST `/` | `{name,email?,maxConcurrentCalls?=1}` → 201 |
| PATCH `/{id}` · DELETE `/{id}` | update / soft-delete |
| PATCH `/{id}/status` | body accepts enum number or name |
| GET `/{id}/access-keys` | key list (prefix only — raw shown once at creation) |
| POST `/{id}/access-keys` | `{name,expiresAt?}` → 201 `CreateAccessKeyResponse{rawKey(44ch),keyPrefix,…}` |
| DELETE `/{id}/access-keys/{keyId}` | revoke |
| GET `/{id}/sessions[/current]` | SignalR presence rows |

### 4.8 Personas — `/api/personas` (15)

CRUD `/` + `/{id}` (PATCH `{name?,description?,isActive?}`, DELETE) ·
**GET `/{id}/published`** — worker contract `{personaName, systemPrompt, configurationJson}` (latest IsPublished version; service-token or owner auth) ·
**GET/PUT `/default`** — inbound-routing persona (`{personaId|null}`; PUT validates ownership) ·
`/{pid}/actions` GET list / POST `/{actionDefId}` link / DELETE unlink ·
`/{pid}/versions` GET / POST `{systemPrompt,configurationJson?}` / GET `/{versionId}` / POST `/{versionId}/publish` ·
`/{pid}/knowledge-bases` GET linked KBs · POST `/{kbId}` link · DELETE `/{kbId}` unlink.

### 4.9 Workflows — `/api/workflows` (9)

CRUD · `/{wid}/versions` GET / POST `{definitionJson:"<json string>"}` · GET `/api/workflow-versions/{versionId}` · POST `/api/workflow-versions/{versionId}/publish`.
⚠ Body wrapper matters: version-create expects the definition **as a string field**, not a raw object.

### 4.10 Call Configurations — `/api/call-configurations` (8)

GET `/` (resolves personaName/workflowName/actionCount) · GET `/{id}` · POST `{name,description?,personaId?,workflowId?,configJson?}` · PATCH (adds `isActive?`) · DELETE · POST `/{id}/activate` · GET/PUT `/{id}/actions` (`SetConfigActionsRequest{actionDefinitionIds[]}` replaces set).

### 4.11 Knowledge Bases — 15 routes

KB CRUD (`/api/knowledge-bases`, PUT update, DELETE) · documents: GET `/{kbId}/documents`, POST same (`UploadDocumentRequest{name,sourceUri,contentType,content,metadataJson?}` — content auto-chunked at 1000 chars, status→ready) · GET/DELETE `/api/knowledge-documents/{docId}` · chunks POST `/api/knowledge-documents/{docId}/chunks` (`CreateChunkRequest{content,chunkIndex,metadataJson?}`) / DELETE chunk · search POST `/api/knowledge-bases/{kbId}/search` (`SearchRequest{query,topK?=5}` → cosine-distance over vector(1536)) · persona links (§4.8).
Embeddings require `OPENAI_API_KEY`/embedding config; without it search returns `[]`.

### 4.12 Actions Engine — `/api/actions` (8)

GET `/?type` · GET `/system` (system defs seeded) · GET `/{id}` · POST (only Integration/Webhook types creatable) · PATCH · DELETE (system protected) · executions: GET `/executions/{id}` · GET `/executions/by-call/{callSessionId}`.

### 4.13 Usage — `/api/usage` (5)

GET `/?metricType&from&to&callSessionId&licenseId&partnerId` → `UsageRecordDto[]` · GET `/summary` → `UsageSummaryDto[]{metricType,totalQuantity,unit,count}` · GET `/metric/{type}` · GET `/call/{callSessionId}` · POST `/` record event (worker-side metering).

### 4.14 API Keys — `/api/api-keys` (4)

GET `/` (`ApiKeyListItem{id,name,keyPrefix,status,scopes,lastUsedAt,expiresAt,createdAt}`) · POST `{name,scopes?,expiresAt?}` → `CreateApiKeyResponse{rawKey(72ch) shown once}` · DELETE `/{id}` revoke · PATCH `/{id}/scopes` `{scopes[]}`.

### 4.15 Licenses — `/api/licenses` (5)

GET `/` · GET `/{id}` · POST `{userId,partnerId?,partnerPlanId?,startsAt,endsAt?,limitsJson?,metadataJson?}` · PATCH · DELETE.

### 4.16 Partners — 12 routes

GET `/api/partners` · GET `/{id}` · GET `/me` · PUT `/{id}` · customers GET/POST under `/{partnerId}/customers` · partner-relationships GET/PUT/DELETE · provisioning GET `/{partnerId}/provision/{externalCustomerId}` + POST provision · GET `/{partnerId}/stats`.

### 4.17 Plans & Subscriptions (15)

Plans: GET `/api/plans` (active) · `/all` · `/{id}` · POST/PATCH/DELETE · partner plans: GET `/{partnerId}/plans`, POST, GET/PUT `/api/partner-plans/{id}`.
Subscriptions: GET `/api/subscriptions` (own) · GET `/{id}` · POST `{planId,startsAt,endsAt?,trialEndsAt?}` · PATCH `{status?,endsAt?}` · POST `/{id}/cancel`.

### 4.18 LiveKit — `/api/livekit` (5)

POST `/token` `{identity,roomName,canPublish?,canSubscribe?}` — **guard:** identities starting `agent_` must have an Accepted transfer into that room, else 403. Returns `{token}`. · POST `/room` create · DELETE `/room/{roomName}` · POST `/room/{roomName}/egress/start|stop`.

### 4.19 Webhooks — `/api/webhooks` (4)

POST `/recording-complete` · `/call-started` · `/call-ended` → ack stubs ·
**POST `/livekit`** — LiveKit server webhook (HS256 JWT in `Authorization`, verified against `LIVEKIT_API_SECRET`; optional `X-Service-Token`). Drives the inbound lifecycle: `participant_joined` (session create + dispatch for callers; cold-swap completion for `agent_*`/`dest_*`; answer-marking for `ai-agent`) · `participant_left` (leg close; caller-left ends session) · `room_finished`.

### 4.20 SIP Destinations — `/api/sip/destinations` (5)

| Method & Route | Contract |
|---|---|
| GET `/` | user's named destinations |
| POST `/` | `{name, callTo, description?}` → 201; 409 duplicate name |
| PATCH `/{id}` | `{name?, callTo?, description?, isEnabled?}` |
| DELETE `/{id}` | remove |
| GET `/options` | transfer options for the AI layer: `{agents:[{type,name,available}], destinations:[{type,name}]}` — **CallTo values never leave the backend** |

### 4.21 Worker Shims — `/api/call/*` (anon + optional service token)

POST `/transfer` `{RoomName, TargetType?("human"|"destination"), TargetName?, AgentId?, Reason?}` → 200 `{transferId, agentName?/status}` or 400 `{error}` (no agent / unknown destination) ·
POST `/active` `{RoomName}` registers legacy rooms only (`call_u*` rooms are webhook-owned) ·
POST `/end` `{RoomName}` · POST `/summary` `{RoomName, Summary}` → handoff context.
All four honor `BACKEND_SERVICE_TOKEN` via `X-Service-Token` when configured.

---

## 5. Business Logic & Scenarios

### 5.1 Call Lifecycle State Machine

```
Queued(0) → Ringing(1) → Active(2) → Transferred(3) → Completed(4)
     \            \                        \
      `→ Failed(5) `→ Failed(5)/Cancelled(6) `→ Completed(4)
```

Who drives transitions:

| Transition | Driver |
|---|---|
| create Queued | `CallSessionService.CreateAsync` (AI worker / inbound flow) |
| → Ringing/Active | LiveKit room events via AI worker + participants rows |
| → Transferred | supervisor or AI triggers `POST …/transfers` (§4.4) |
| → Completed | `POST /end` (dashboard/agent) or worker hangup |
| Failed/Cancelled | worker error paths / explicit cancel |

Every query is **tenant-scoped** (`UserId == owner`) — multi-tenancy is enforced at the service layer, not just the UI.

### 5.2 Transfers (v0: cold silent swap — the core scenario)

**Human target (internal agent app):**
1. Trigger: customer asks the AI → Gemini tool `transfer_to_human(name?, reason?)` → worker POSTs `/api/call/transfer {TargetType:"human"}`.
2. Service resolves the named agent (exact, then prefix match — never substitutes another agent) with `Status==Available`, owner-scoped, respecting MaxConcurrentCalls; no name → best available.
3. Creates `CallTransfer(Cold, HumanAgent, Requested)` + `CallHandoff`; SignalR `IncomingTransfer` → device rings (30s countdown).
4. Agent **accepts** → Accepted; backend removes `ai-agent` from the room 3s later (webhook fallback) — **cold silent swap**: the caller's leg never moves.
5. Webhook sees `agent_{id}` join → transfer Completed, webrtc leg opens, AI leg closes (`RemoveParticipant` authoritative).
6. **Reject/timeout** (Hangfire `TransferTimeoutProcessor`, ~10s cadence) → Failed + cascade/`TransferExpired`.

**External destination (customer PBX):**
1. Tool `transfer_to_department(name)` → `/api/call/transfer {TargetType:"destination"}`.
2. Backend looks up `sip_destinations` by name (enabled) → creates Transfer(ExternalDestination) → background `CreateSIPParticipant` via outbound trunk, identity `dest_{id}`, ringback played to room.
3. Issabel answers and runs its own queue/ring-group/IVR distribution — platform knows nothing about internal extensions.
4. `dest_*` participant joined (webhook or 45s poller) → same cold swap. No answer → Failed, AI resumes.

Failures at step 2 return HTTP 400 and the AI stays on the call speaking the error text. Every transfer stores a `TargetSnapshotJson` so routing edits can't corrupt in-flight calls.

### 5.3 Access-Key Agent Login

`POST /api/auth/agent-login` hashes the presented key (SHA-256 hex, lowercase) and matches an Active, non-expired row; success bumps LastUsedAt and returns a **LiveKit JWT** (HS256, `video.roomJoin`, 2h exp) plus `livekitUrl`. Revoking the key instantly blocks future logins (sessions already connected are dropped by SignalR disconnect handler setting Offline).

### 5.4 RAG Pipeline

Upload doc (text) → split into ≤1000-char chunks → each chunk gets an embedding via OpenAI `text-embedding-3-small` (1536 dims stored as pgvector) → persona-linked KBs are searched during calls with `<=>` cosine distance, topK results injected into the AI prompt. Dashboard provides upload UI + a semantic-search tester. Without embeddings configured, search returns empty rather than erroring.

### 5.5 Versioning & Publish Semantics

Personas and Workflows are immutable-versioned: drafts append (`VersionNumber` increments), exactly one version carries `IsPublished=true` (publishing flips others off). Call Configurations bind to personas/workflows by Id — runtime uses the **published** version.

### 5.6 Metering & Licensing

Worker records usage events (per-call minutes, inference units…) idempotently (`IdempotencyKey`). Licenses define windows (`StartsAt/EndsAt/LimitsJson`); plans/subscriptions define purchasable tiers with trial support. Summary endpoints aggregate by metric type.

### 5.7 Recording Flow

Egress started per room (`/egress/start`) → writes to MinIO `recordings` bucket → callback marks `call_recordings.Available` → dashboard fetches presigned download URL.

---

## 6. Dashboard Frontend

Stack: React 19, Vite 7, Tailwind 4, react-router 7, @microsoft/signalr. Dev server `:5173`; `VITE_API_URL=http://localhost:8080`.

### Routes (17)

| Path | Page | Data sources |
|---|---|---|
| `/` | LandingPage (marketing) | static |
| `/login` | LoginPage (login⇄register toggle) | authApi |
| `/dashboard` index & `/live` | LiveBoard | stats/today · calls/active · SignalR QueueUpdate |
| `/dashboard/queue` | QueuePage | stats/queue |
| `/dashboard/roster` | AgentRoster | human-agents + stats/agents + personas |
| `/dashboard/analytics` | Analytics | stats/hourly · period · intents |
| `/dashboard/history` | CallHistory | calls list (server pagination) |
| `/dashboard/call/:id` | CallDetail | calls/{id} · end · transfers · recordings/download |
| `/dashboard/configs` | CallConfigsPage | call-configurations CRUD/activate/actions |
| `/dashboard/personas` | PersonasPage | personas CRUD/versions/publish/actions/KB-links |
| `/dashboard/sip-destinations` | SipDestinationsPage | sip-destinations CRUD + default-persona routing picker |
| `/dashboard/workflows` | WorkflowsPage | workflows CRUD/versions/publish |
| `/dashboard/knowledge` | KnowledgePage | KBs/documents/search |
| `/dashboard/usage` | UsagePage | usage summary/records |
| `/dashboard/api-keys` | ApiKeysPage | api-keys create/revoke |
| `/dashboard/agents-admin` | AgentsAdminPage | human-agents + access keys |
| `/dashboard/business` | BusinessPage | licenses + partners lists |

`/dashboard/*` wrapped in `RequireAuth` (redirects to `/login`, remembers origin path).

### Component Inventory

- **Layout**: `DashboardLayout` (sticky header, clock, live dot), `DashboardSidebar` (3 grouped sections, user card, sign out)
- **Primitives** (`dashboard/components/ui.tsx`): `PageHeader, Panel, Field, TextInput, TextArea, Select, Btn(mint/amber/coral/ghost), ErrorBox, Empty, Th/Td, Modal`
- **Domain widgets**: `StatCard` (pulse tones), `Badge` (mint/amber/coral/dim), `StatusDot`, `ProgressBar`, `CallRow`, `AgentCard`
- **Landing sections**: Nav, Hero, Pricing, Agents, Faq, Footer, Calculator, HybridLoop, ApiStatus…

### API Client Internals (`src/api/client.ts`)

- Single source of truth for the JWT (`localStorage: tandem_access_token`)
- **Transparent refresh**: any 401 (except auth paths) → one shared in-flight `POST /api/auth/refresh` → retry once → on failure clears token
- **Error parsing**: ValidationProblemDetails `errors{field:[msgs]}` flattened to readable text
- `useApi(fetcher, deps)` hook supplies `{data,loading,error,refetch}`
- `useLiveHub(enabled)` builds the SignalR connection (`withCredentials:false`, auto-reconnect ladder) exposing `{connected, queue, tick}` — `tick` is used as a refetch dependency for near-realtime pages

### Normalization Tables (`dashboard/statusMap.ts`)

```
API → UI status:  Queued→queued · Ringing/Active→active-ai · Transferred→active-human
                  Completed→completed-ai · Missed→missed · Failed→abandoned
HumanAgent numeric→UI: 1→online · 4→busy · 2/3→break · 0/other→offline
```

Demo-mode mocks (`data.ts`) render **only** when `VITE_API_URL` is unset; with it set, pages show honest loading/error/empty states.

---

## 7. Agent App

Expo SDK 57 entry (`index.js` → `App.js`). Backend URL by platform: iOS `http://localhost:5000` · Android-emu `http://10.0.2.2:5000` · **web `http://127.0.0.1:5000`** (requires published 5000 + the `:8081` CORS origin).

### Platform Split (Metro resolution)

```js
const ActiveCall = require('./AgentCall').default;
// AgentCall.web.js  → livekit-client (browser WebRTC audio-only)
// AgentCall.js      → @livekit/react-native <LiveKitRoom>
```
Never import `@livekit/react-native` unconditionally — its webrtc package needs `requireNativeComponent` and crashes web bundles.

### Screens / States

login → dashboard(idle | history tabs, status pills Available/Break/NotReady, connection dot, pending-transfer badge) → ringing (vibration loop + 30s countdown + Answer/Reject) → call (timer MM:SS, mute toggle via mic enable, AI-handoff summary card, End) → notes (disposition saved to `metadata_json`) .

Second incoming while busy → queued in `pendingTransfers[]` with tab badge. `TransferExpired` clears a stale ring. Logout stops SignalR and resets all state.

Web call specifics: `resolveLiveKitUrl()` rewrites internal hosts (`ws://livekit:` / `127.0.0.1`) to the browser hostname; remote audio auto-attached; mic granted via getUserMedia on connect.

---

## 8. AI Worker & Legacy Pages

### python-ai-worker

Joins rooms as identity **`ai-agent`** using **Google Gemini Live** (`AGENT_MODEL=models/gemini-3.1-flash-live-preview`, voice `Aoede`, temperature 0.7). Registers as a **named agent** (`agent_name=voice-agent`) → only explicit dispatches create jobs (auto-dispatch disabled). Persona comes from dispatch metadata `{sessionId, personaId}` → `GET /api/personas/{id}/published` (env `PERSONA_ID` still works as fallback). Tools: `transfer_to_human(name?, reason?)` and `transfer_to_department(name, reason?)` POST the transfer shim with `X-Service-Token`; on `agent_*`/`dest_*` join after a requested transfer the worker posts the handoff summary and disconnects (backend removal is authoritative/idempotent). Env: `LIVEKIT_URL/KEY/SECRET`, `GEMINI_API_KEY`, `BACKEND_URL=http://backend:5000`, `BACKEND_SERVICE_TOKEN`, `AGENT_NAME`.

### Legacy wwwroot

`backend/wwwroot/` still serves first-generation admin/agent HTML (`admin.html`, `agent.html`, `index.html`, `app.js`) — kept for reference, superseded by the React dashboard and Expo app.

---

## 9. Operations Guide

### Run Everything

```powershell
cd back
# .env: BACKEND_SERVICE_TOKEN, PBX_IP, SIP_DIDS, OWNER_USER_ID (users.id)
docker compose up -d --build          # 10 services + one-shot sip-setup profile
OWNER_USER_ID=<guid> docker compose --profile setup run --rm sip-setup
cd ../frontend ; npm install ; npm run dev        # :5173
cd ../back/agent-app ; npx expo start --port 8081 # web/mobile
```

### Ports

| Port | Service |
|---|---|
| 8080 | Caddy → backend API + LiveKit signaling |
| 5000 | backend (published for agent-app) |
| 5432 | Postgres (callcenter/admin/adminpassword) |
| 7880–7882 | LiveKit signaling (`livekit.yaml`, devkey/secret) |
| 7882–7892/udp | LiveKit WebRTC media range |
| **5061/udp+tcp** | LiveKit-SIP (customer PBX peers here) |
| 9000/9001 | MinIO S3 / console |
| 5173 / 8081 | dashboard / expo dev servers |

### Key env vars

Backend: `ConnectionStrings__DefaultConnection` · `JWT_SECRET` · `LIVEKIT_URL/KEY/SECRET` · `REDIS_CONNECTION` · `MINIO_*` · `CORS_ORIGINS` · `BACKEND_SERVICE_TOKEN` · `LIVEKIT_OUTBOUND_TRUNK_ID` (destination transfers).
Worker/setup: `AGENT_NAME=voice-agent` · `PERSONA_ID` (fallback) · `PBX_IP` · `SIP_DIDS` · `OWNER_USER_ID`.

Hangfire dashboards: `/hangfire`. Swagger: `/swagger`. Recurring jobs registered at startup with `SchedulePollingInterval=2s` (required for sub-15s crons).

### Tests

```powershell
dotnet test back/backend.Tests        # 21/21 passing
```

> ⚠ On hosts with Windows Application Control (Smart App Control), the local vstest/xunit host can fail to load freshly built DLLs (`0x800711C7`). Run the suite in a Linux container instead:
> ```powershell
> docker run --rm -v "${PWD}\back:/src" -w /src/backend.Tests mcr.microsoft.com/dotnet/sdk:10.0 dotnet test
> ```

---

## 10. Verified E2E Scenarios (executed against the running stack)

| # | Scenario | Result |
|---|---|---|
| 1 | Docker stack boot ×10 services healthy | ✅ |
| 2 | Register → JWT → all scoped reads return zeros | ✅ |
| 3 | Login/refresh chain rotates tokens | ✅ |
| 4 | Seed call → appears in list/active/detail; stats increment | ✅ |
| 5 | SignalR WebSocket through Caddy receives QueueUpdate (~3s cadence after scheduler fix) | ✅ 8 events/25s |
| 6 | CORS preflight for both `5173` and `8081` origins incl. credentials | ✅ |
| 7 | Supervisor force-end + transfer initiation → `Requested` routed to Available agent | ✅ |
| 8 | Persona create → v1 publish → workflow version publish → config bind + activate + name resolution | ✅ |
| 9 | KB create → doc upload (auto-chunk) → search → link/unlink persona | ✅ |
| 10 | API key issue (72-char secret, show-once) + revoke | ✅ |
| 11 | Human-agent onboard + access key issue (44-char) → agent-login validates hash | ✅ |
| 12 | Usage summary/metrics; licenses/partners lists | ✅ |
| 13 | Agent-app web bundle excludes native-webrtc; android/iOS include it | ✅ proven by bundle diff |
| 14 | Backend test suite | ✅ 21/21 |
| 15 | v0 build: backend compile clean · compose config valid · `main.py`/`sip-setup.py` py_compile (container) · frontend `tsc --noEmit` | ✅ |
| 16 | Patch runner: fresh-DB + legacy-baseline paths (`_schema_patches`) | ✅ code-verified; live DB pending |
| 17 | **Live PSTN→AI→transfer call over SIP** | ⏳ pending first real deployment with `PBX_IP` |

## 11. Fix Log (session bugs → root cause → fix)

1. compose YAML crash — unquoted colon value → quote `"LIVEKIT_KEYS=devkey: secret"`
2. .NET 9 base images vs net10 csproj → sdk/aspnet `:10.0`
3. plain postgres lacks pgvector → `pgvector/pgvector:pg15`
4. EF unmappable `float[]`↔vector → `Pgvector.EntityFrameworkCore` + `Vector?` + `UseVector()`
5. tests broke (InMemory can't map Vector) → provider-aware conversion in OnModelCreating
6. fresh DB missing extension → `CREATE EXTENSION IF NOT EXISTS vector` before EnsureCreated
7. `/api/calls/active` 500 — enum-array `.Contains()` funcletizer ReadOnlySpan crash → `List<T>`
8. same endpoint — OrderBy **after** correlated-count projection untranslated → reorder before Select
9. frontend `TodayStats` field mismatch (`total` vs `totalCalls`) → aligned to real DTOs
10. dashboard pointed at unpublished 5000 → `VITE_API_URL=:8080` + publish `5000:5000`
11. missing vite-env types → added `src/vite-env.d.ts`
12. CORS blocked SignalR (credentials) → `AllowCredentials()` + client `withCredentials:false`
13. broadcasts stuck at 15s cadence → `SchedulePollingInterval=2s`
14. `GET /call-configurations` 500 — nav-join + correlated count untranslated → fetch-then-resolve `ProjectAsync`
15. workflow version create 500 — body needed `{definitionJson:"…"}` string wrapper
16. agent-app web crash (`requireNativeComponent`) → unsuffixed platform-split `require('./AgentCall')`
17. first platform-split attempt leaked native module into web graph → single specifier + suffixed files
18. roster showed mocks despite real data → removed silent fallback; honest empty/error states
19. numeric enum statuses rendered wrong (`0..4`) → dual numeric/string decoder
20. Expo-web CORS 8081 rejected → added origin to `CORS_ORIGINS`

---

## 12. LiveKit-SIP Integration Guide (v0)

Asterisk has been removed entirely. LiveKit-SIP is the only SIP gateway; the customer PBX (e.g. Issabel) peers directly into it by IP.

### 12.1 Architecture

```
PBX ──INVITE──▶ livekit-sip:5061 ──trunk+rule──▶ room "call_u{userId}<rand>" (answered here)
     webhook participant_joined ──▶ backend: owner from room prefix → DefaultPersonaId
                                   → CreateAgentDispatch("voice-agent", {sessionId,personaId})
worker joins as identity "ai-agent", fetches persona, greets caller.
```

Transfers are cold silent swaps inside the SAME room (the caller's leg never moves):

| Target | Mechanism | Swap trigger |
|---|---|---|
| Internal human agent | SignalR invite (`IncomingTransfer`) → agent app Accept → WebRTC join with identity `agent_{id}` | webhook on join (fallback: backend removes AI 3s after accept) |
| External PBX destination | `CreateSIPParticipant` via outbound trunk, identity `dest_{id}`; Issabel does its own queue/ringgroup/IVR distribution | webhook on join or 3s poller |

AI tools: `transfer_to_human(name?, reason?)`, `transfer_to_department(name, reason?)`
→ POST `/api/call/transfer` `{RoomName, TargetType, TargetName}`.
Failures return 400 and the AI stays on the call.

### 12.2 Deployment steps

```bash
# .env
BACKEND_SERVICE_TOKEN=<random>          # optional but recommended
PBX_IP=<customer pbx ip>
SIP_DIDS=+15551234567,+15557654321      # optional DIDs
OWNER_USER_ID=<users.id guid>

docker compose up -d --build            # asterisk service no longer exists
docker compose exec db psql -U admin -d callcenter -c \
  "SELECT id FROM users WHERE email='you@example.com';"
OWNER_USER_ID=<that-guid> docker compose --profile setup run --rm sip-setup
```

`sip-setup.py` creates (idempotently, matched by name):
- inbound trunk `inbound-pbx` (allow-listed to PBX_IP/32)
- dispatch rule `inbound-rooms` (individual rooms, prefix `call_u{OWNER_USER_ID}`)
- outbound trunk `outbound-pbx` (PBX_IP:5060) — print its id into `LIVEKIT_OUTBOUND_TRUNK_ID`

Then in the dashboard: Personas page → set persona → **SIP Destinations** page → set Default AI Persona + add named destinations (Support/Sales/…). The AI only ever sees names; `call_to` values stay server-side.

### 12.3 Schema / migrations

`backend/Data/DbPatchRunner.cs` applies embedded patches `Migrations/Patches/*.sql` in order, tracked in `_schema_patches`. Fresh DBs get `000_base` + everything after; legacy DBs are baselined automatically. New schema objects ship as a new numbered patch file.

### 12.4 Identity conventions

| Identity | Meaning |
|---|---|
| *(sip caller)* | any participant not matching below |
| `ai-agent` | python-ai-worker |
| `agent_{humanAgentId}` | internal agent app (WebRTC) |
| `dest_{destinationId}` | bridged external PBX leg |

Rooms MUST start with `call_u{ownerUserId}` to be routed; other rooms are ignored by the inbound path.

### 12.5 Verification checklist

1. Dial DID → session row created, `pstn_in` leg open, AI greets ≤ ~3s.
2. "transfer me to \<agent name\>" → app rings → Accept → AI removed instantly, transfer Completed.
3. Decline/30s timeout → transfer Failed, AI continues.
4. "transfer me to support" → Issabel receives call at mapped target → answer swaps instantly.
5. Caller hangs up mid-transfer → transfers Cancelled, legs closed, agent freed.

---

*End of report.*
