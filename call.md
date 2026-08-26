# Tandem â€” AI + Human Call Center Platform

**Complete technical reference** Â· generated from a fully verified end-to-end session (build, deploy, endpoint audit, live smoke tests). **Kept current**: updated for the v0 LiveKit-SIP integration (Asterisk removed, inbound PSTNâ†’AI routing, cold-swap transfers).

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Architecture](#2-architecture)
3. [Database & Enums](#3-database--enums)
4. [API Reference â€” All 162 Endpoints](#4-api-reference)
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
| Media/RTC | LiveKit (WebRTC SFU), **LiveKit SIP â€” sole SIP gateway (:5061)** |
| Recording | LiveKit Egress â†’ MinIO (S3-compatible) |
| AI | Python worker Â· Gemini Live (`models/gemini-3.1-flash-live-preview`), OpenAI `text-embedding-3-small` |
| Dashboard | React 19 Â· Vite 7 Â· Tailwind CSS 4 Â· react-router 7 Â· @microsoft/signalr |
| Agent app | Expo SDK 57 Â· React Native 0.86 Â· @livekit/react-native (native) / livekit-client (web) |
| Reverse proxy | Caddy |

### Repository Layout

```
testviop/
â”œâ”€â”€ call.md                  â† this file
â”œâ”€â”€ erd.md                   â† original DB design spec
â”œâ”€â”€ frontend/                â† operator dashboard (React/Vite)
â”‚   â””â”€â”€ src/
â”‚       â”œâ”€â”€ api/             client.ts, endpoints.ts
â”‚       â”œâ”€â”€ auth/            AuthContext, RequireAuth
â”‚       â”œâ”€â”€ hooks/           useApi, useLiveHub
â”‚       â”œâ”€â”€ pages/           LoginPage
â”‚       â””â”€â”€ dashboard/
â”‚           â”œâ”€â”€ pages/       13 page components (incl. SipDestinationsPage)
â”‚           â”œâ”€â”€ components/  StatCard, Badge, ui.tsx primitivesâ€¦
â”‚           â””â”€â”€ data.ts      demo/mock dataset
â””â”€â”€ back/
    â”œâ”€â”€ docker-compose.yml   10-service stack (+ sip-setup one-shot profile)
    â”œâ”€â”€ livekit.yaml         LiveKit config incl. webhooks â†’ backend
    â”œâ”€â”€ sip-setup.py         idempotent trunk/dispatch-rule bootstrap
    â”œâ”€â”€ Caddyfile            :8080 router
    â”œâ”€â”€ backend/             ASP.NET API (+ legacy wwwroot pages)
    â”‚   â”œâ”€â”€ Endpoints/       22 endpoint groups (162 routes)
    â”‚   â”œâ”€â”€ Services/        business logic (incl. InboundRoutingService)
    â”‚   â”œâ”€â”€ Data/            AppDbContext + DbPatchRunner (patch migrations)
    â”‚   â”œâ”€â”€ Models/          Domain entities + Enums
    â”‚   â”œâ”€â”€ Hubs/            CallHub (SignalR)
    â”‚   â””â”€â”€ Migrations/Patches/  embedded ordered SQL patches
    â”œâ”€â”€ agent-app/           Expo mobile+web agent client
    â””â”€â”€ python-ai-worker/    Gemini Live voice agent
```

---

## 2. Architecture

### Service Topology

```
                          â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
   Browser :5173 â”€â”€â”€â”€â”€â”€â”€â–¶â”‚  Caddy :8080                â”‚
   Expo web :8081 â”€â”€â”€â”€â”€â”€â–¶â”‚  /rtc,/twirp â†’ livekit:7880 â”‚
                          â”‚  else        â†’ backend:5000 â”‚
                          â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                                          â”‚
                              â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â–¼â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
                              â”‚  backend (.NET 10)     â”‚
                              â”‚  REST Â· SignalR Â·      â”‚
                              â”‚  Hangfire Â· webhooks   â”‚
                              â””â”€â”€â”€â”¬â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¬â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                     â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜         â”‚
               â”Œâ”€â”€â”€â”€â”€â–¼â”€â”€â”€â”€â”€â”          â”Œâ”€â”€â”€â”€â–¼â”€â”€â”€â”€â”€â”€â”€â”€â”
               â”‚ Postgres  â”‚          â”‚   Redis     â”‚
               â”‚ +pgvector â”‚          â”‚ (LiveKit,   â”‚
               â”‚  :5432    â”‚          â”‚  egress,    â”‚
               â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜          â”‚  livekit-sip)â”‚
                                       â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                                           â”‚
        â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¬â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¼â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
        â”‚                     â”‚            â”‚                   â”‚
  â”Œâ”€â”€â”€â”€â”€â–¼â”€â”€â”€â”€â”€â”€â”   â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â–¼â”€â”€â”   â”Œâ”€â”€â”€â”€â”€â–¼â”€â”€â”€â”€â”€â”   â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â–¼â”€â”€â”€â”
  â”‚  LiveKit   â”‚   â”‚ LiveKit SIP â”‚   â”‚  Egress   â”‚   â”‚ MinIO S3    â”‚
  â”‚ :7880-7882 â”‚â—„â”€â–ºâ”‚   :5061     â”‚   â”‚ recorder  â”‚   â”‚ :9000/:9001 â”‚
  â”‚  dispatch/ â”‚   â”‚  UDP+TCP    â”‚   â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜   â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
  â”‚  webhooks  â”‚   â””â”€â”€â”€â”€â”€â”€â–²â”€â”€â”€â”€â”€â”€â”˜        â”‚ recordings
  â””â”€â”€â”€â”€â”€â–²â”€â”€â”€â”€â”€â”€â”˜          â”‚ SIP           â–¼
        â”‚                 â”‚         MinIO bucket "recordings"
  â”Œâ”€â”€â”€â”€â”€â”´â”€â”€â”€â”€â”€â”€â”€â”€â”  â”Œâ”€â”€â”€â”€â”€â”´â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
  â”‚ python-ai-   â”‚  â”‚ Customer PBX  â”‚
  â”‚ worker       â”‚  â”‚ (Issabel)     â”‚
  â”‚ (Gemini)     â”‚  â”‚ queues/IVR    â”‚
  â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜  â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
```

**Inbound call path**: PBX â†’ `livekit-sip:5061` â†’ trunk match (IP allow-list) â†’ dispatch rule creates room `call_u{userId}<rand>` and answers â†’ LiveKit **webhook** `participant_joined` â†’ backend resolves owner from the room prefix, looks up `users.default_persona_id`, calls **CreateAgentDispatch** (`voice-agent`) with `{sessionId, personaId}` metadata â†’ worker joins as identity `ai-agent` and greets.

### Request Paths

| Client | URL | Path |
|---|---|---|
| Dashboard (Vite dev) | `http://localhost:5173` â†’ `VITE_API_URL=http://localhost:8080` | browser â†’ Caddy â†’ backend |
| Agent app (web) | `http://localhost:8081` â†’ `http://127.0.0.1:5000` | browser â†’ published port â†’ backend |
| Agent app (Android emu) | `http://10.0.2.2:5000` | emulator loopback â†’ host â†’ backend |
| Agent app (iOS sim) | `http://localhost:5000` | direct |
| AI worker | `ws://livekit:7880`, `BACKEND_URL=http://backend:5000` | internal docker net |
| Customer PBX | SIP INVITE â†’ host `:5061/udp+tcp` | published livekit-sip port |
| LiveKit server | webhook POSTs â†’ `http://backend:5000/api/webhooks/livekit` | internal docker net |

### CORS (critical)

Backend runs in Production mode inside Docker; policy = `WithOrigins(CORS_ORIGINS).AllowAnyHeader().AllowAnyMethod().AllowCredentials()`.

Allowed origins (docker-compose `CORS_ORIGINS`):
`http://localhost:5173` (dashboard) Â· `http://localhost:3000` Â· `http://localhost:8081` (Expo web)

SignalR browser clients default `withCredentials:true` â†’ `AllowCredentials()` is mandatory. The agent-app's web SignalR sets `withCredentials:false`.

### Realtime Events (SignalR `/hubs/call`)

| Event | Audience | Payload |
|---|---|---|
| `QueueUpdate` | ALL clients | `{ activeCount, agentsOnline, activeCalls:[{id,roomName,status,startTime,durationSeconds}], agents:[{id,name,Status}] }` |
| `IncomingTransfer` | group `agent_{id}` | transfer + room details (triggers ring screen) |
| `TransferExpired` | group `agent_{id}` | `{ transferId }` â€” clears ringing UI on timeout |

Hub methods: `RegisterAgent(agentId)` (joins group, creates session row, statusâ†’Available); `OnDisconnectedAsync` (leaves group, session row closed, statusâ†’Offline).

---

## 3. Database & Enums

PostgreSQL database `callcenter` (user `admin`). Schema is applied by **`Data/DbPatchRunner`**: embedded ordered SQL patches from `Migrations/Patches/*.sql`, tracked in the `_schema_patches` history table. Fresh databases receive every patch (`000_base` â†’ â€¦); legacy deployments (base tables already present) are auto-baselined â€” the runner records `000_base` without re-executing it. New schema changes ship as a new numbered patch file. Column names are PascalCase (quoted identifiers). EF model is the source of truth.

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
| `call_configurations` | binds lineâ†’persona/workflow | PersonaId?, WorkflowId?, ConfigJson, IsActive |
| `call_configuration_actions` | actions exposed per config | CallConfigurationId, ActionDefinitionId |
| `action_definitions` | system + custom tools | ActionType (System/Integration/Webhook), InputSchemaJsonâ€¦ |
| `persona_actions` | personaâ†”action links | |
| `knowledge_bases` / `knowledge_documents` / `knowledge_chunks` | RAG store | chunks: Content, **Embedding vector(1536)**, MetadataJson jsonb |
| `persona_knowledge_bases` | personaâ†”KB links | |
| `api_keys` | machine auth | KeyPrefix, KeyHash, Scopes[], Status, ExpiresAt |
| `licenses` | entitlement windows | PartnerId?, Status, StartsAt, EndsAt, LimitsJson |
| `partners` / `partner_plans` / `partner_relationships` / `partner_customers` | B2B tier | |
| `plans` / `subscriptions` | billing tiers | EntitlementsJson; sub has TrialEndsAt |
| `usage_records` | metering events | MetricType, Quantity, Unit, OccurredAt, IdempotencyKey |
| `action_executions` | tool run log | Status, InputJson, OutputJson, Error |
| `hangfire.*` | job storage | Hash holds recurring-job cron entries |
| `_schema_patches` | patch-runner history | name (PK), applied_at |

### Enums (numeric wire values â€” verified from source)

```csharp
CallSessionStatus   0 Queued Â· 1 Ringing Â· 2 Active Â· 3 Transferred Â· 4 Completed Â· 5 Failed Â· 6 Cancelled
CallDirection       0 Inbound Â· 1 Outbound
CallTransferStatus  0 Requested Â· 1 Ringing Â· 2 Accepted Â· 3 Completed Â· 4 Rejected Â· 5 Failed Â· 6 Cancelled
HumanAgentStatus    0 Offline Â· 1 Available Â· 2 Break Â· 3 NotReady Â· 4 InCall
AccessKeyStatus     0 Active Â· 1 Revoked Â· 2 Expired
ParticipantType     AiAgent Â· Human            (serialized as string)
```

> âš ï¸ Serialization is inconsistent by endpoint: `/api/human-agents` returns `Status` as a **number**, while list/detail DTOs built with `.ToString()` return strings. The dashboard normalizes both (see Â§6).

---

## 4. API Reference

Conventions:
- **Auth** = JWT bearer (`Authorization: Bearer â€¦`) resolved by `AuthMiddleware` into `HttpContext.Items["UserId"]`; anonymous routes are marked.
- Errors: `401` unauthorized Â· `404` not-found Â· `400 {error}` or ValidationProblemDetails Â· `409` conflicts.
- Success bodies are camelCase JSON.

### 4.1 Auth â€” `/api/auth` (6)

| Method & Route | Auth | Contract |
|---|---|---|
| POST `/register` | anon (rate-limited) | req `{email,password,displayName}` â†’ 200 `AuthResponse{accessToken,refreshToken,expiresAt,user{â€¦}}`; 409 if email exists |
| POST `/login` | anon | same response; 401 bad creds |
| POST `/refresh` | Bearer (old token) | â†’ fresh `AuthResponse` |
| POST `/logout` | anon | `{message:"Logged out"}` (stateless no-op) |
| GET `/me` | âœ” | `UserDto{id,email,displayName,companyName,status,isPartner,standardCredits,premiumCredits,createdAt}` |
| POST `/agent-login` | anon | req `{accessKey}` â†’ SHA-256 lookup in `human_agent_access_keys` â†’ 200 `AgentLoginResponse{agentId,name,livekitToken,livekitUrl,ownerUserId}`; updates LastUsedAt; 401 invalid/expired/revoked |

### 4.2 Stats â€” `/api/stats` (7) + health (1)

| Route | Contract |
|---|---|
| GET `/today` | `TodayStatsResponse{totalCalls,activeCalls,answeredCalls,transferredCalls,missedCalls,avgDurationSeconds,agentsOnline,hourly[{hour,count}]}` (scoped to UserId) |
| GET `/queue` | `QueueStatsResponse{activeCount,agentsOnline,activeCalls[{id,roomName,callerId,status,startTime,durationSeconds}],agents[{id,name,Status}]}` |
| GET `/agents` | `AgentStatsDto[]{agentId,name,status,totalCalls,avgDurationSeconds,lastActiveAt}` (status here = `"Active"/"Inactive"` string) |
| GET `/period?from&to` | `PeriodStatsResponse{from,to,totalCalls,completedCalls,avgDurationSeconds,hourly[]}` |
| GET `/summary` | `SummaryStatsResponse{totalCallsToday,thisWeek,thisMonth,totalUsageHours,activeSubscriptions,totalKnowledgeBases}` |
| GET `/hourly?date` | `HourlyDataPoint[]{hour,count}` (24 buckets) |
| GET `/intents?from&to` | `IntentStatsDto[]{intent,count,percentage}` |
| GET `/api/health` | anon Â· `HealthCheckResponse{status,database,redis,livekit,uptime,version}` |

### 4.3 Call Sessions â€” `/api/calls` (7)

| Method & Route | Contract |
|---|---|
| GET `/?status&direction&from&to&page&limit` | `{items:[CallSessionListItem],totalCount,page,limit}` â€” item: `{id,userId,callConfigurationId,livekitRoomName,status,direction,startedAt,answeredAt,endedAt,durationSeconds,metadataJson,participantCount,createdAt}` |
| GET `/{id}` | `CallSessionDetail`: list item fields + `callConfigurationName`, `participants[]`, `transfers[]`, `recordings[]`, `handoff?` (full child DTOs, Â§4.4â€“4.6) |
| GET `/active` | `ActiveCallDto[]` where status âˆˆ {Queued,Ringing,Active,Transferred} |
| POST `/{id}/end` | sets EndedAt/DurationSeconds/Status=Completed â†’ `{id,status,durationSeconds,endedAt}` |
| PATCH `/{id}/metadata` | req `{metadataJson:"<string>"}` (dashboard stores agent notes here) |
| GET `/{id}/participants[/{pid}]` | participant DTOs `{id,humanAgentId,participantType,livekitIdentity,livekitParticipantSid,displayName,joinedAt,leftAt,createdAt}` |

### 4.4 Transfers â€” `/api/calls/{callSessionId}/transfers` (6)

| Method & Route | Contract |
|---|---|
| POST `/` | req `{reason?, targetType?, targetName?}` â€” picks next Available agent, creates Transfer(Requested)+Handoff(summary), or destination-dial when `targetType:"destination"`+`targetName` given; fires `IncomingTransfer` â†’ 201 `{transfer:{id,â€¦},handoff?}`; 400 `"No human agents available"` etc. |
| GET `/` , GET `/{transferId}` | transfer DTOs — name resolves ToHumanAgent ?? Destination ?? External; incl. `status,requestedExceptionsâ€¦` |
| POST `/{transferId}/accept` | req `{humanAgentId}` â†’ Accepted (agent-app answers) |
| POST `/{transferId}/reject` | req `{humanAgentId}` â†’ cascade to next available agent |
| POST `/{transferId}/complete` | agent finished â†’ Completed |

### 4.5 Handoffs â€” `/api/calls/{callSessionId}/handoffs` (5)

POST `/{transferId}` create (`CreateHandoffRequest{summary,contextDataJson?,reason?}`) Â· GET `/` list Â· GET `/{handoffId}` Â· POST `/{handoffId}/deliver` Â· POST `/{handoffId}/accept`. Handoff detail adds `toHumanAgentName,status,deliveredAt,acceptedAt`.

### 4.6 Recordings â€” `/api/calls/{callSessionId}/recordings` (5)

GET `/` list Â· GET `/{recordingId}` Â· GET `/{recordingId}/download` â†’ `{url}` (MinIO presigned) Â· POST `/` egress callback (`RecordingCallbackRequest`) Â· DELETE `/{recordingId}`.

### 4.7 Human Agents â€” `/api/human-agents` (11)

| Route | Notes |
|---|---|
| GET `/` Â· GET `/{id}` | âš  `status` serialized as **number** (Â§3 enum) |
| POST `/` | `{name,email?,maxConcurrentCalls?=1}` â†’ 201 |
| PATCH `/{id}` Â· DELETE `/{id}` | update / soft-delete |
| PATCH `/{id}/status` | body accepts enum number or name |
| GET `/{id}/access-keys` | key list (prefix only â€” raw shown once at creation) |
| POST `/{id}/access-keys` | `{name,expiresAt?}` â†’ 201 `CreateAccessKeyResponse{rawKey(44ch),keyPrefix,â€¦}` |
| DELETE `/{id}/access-keys/{keyId}` | revoke |
| GET `/{id}/sessions[/current]` | SignalR presence rows |

### 4.8 Personas â€” `/api/personas` (16)

CRUD `/` + `/{id}` (PATCH `{name?,description?,isActive?}`, DELETE) Â·
**GET `/{id}/published`** â€” worker contract `{personaName, systemPrompt, configurationJson}` (latest IsPublished version; service-token or owner auth) Â·
**GET `/{id}/knowledge-context?query&topK`** â€” RAG retrieval across persona-linked KBs for the AI's `search_knowledge` tool Â·
**GET/PUT `/default`** â€” inbound-routing persona (`{personaId|null}`; PUT validates ownership) Â·
`/{pid}/actions` GET list / POST `/{actionDefId}` link / DELETE unlink Â·
`/{pid}/versions` GET / POST `{systemPrompt,configurationJson?}` / GET `/{versionId}` / POST `/{versionId}/publish` Â·
`/{pid}/knowledge-bases` GET linked KBs Â· POST `/{kbId}` link Â· DELETE `/{kbId}` unlink.

### 4.9 Workflows â€” `/api/workflows` (9)

CRUD Â· `/{wid}/versions` GET / POST `{definitionJson:"<json string>"}` Â· GET `/api/workflow-versions/{versionId}` Â· POST `/api/workflow-versions/{versionId}/publish`.
âš  Body wrapper matters: version-create expects the definition **as a string field**, not a raw object.

### 4.10 Call Configurations â€” `/api/call-configurations` (8)

GET `/` (resolves personaName/workflowName/actionCount) Â· GET `/{id}` Â· POST `{name,description?,personaId?,workflowId?,configJson?}` Â· PATCH (adds `isActive?`) Â· DELETE Â· POST `/{id}/activate` Â· GET/PUT `/{id}/actions` (`SetConfigActionsRequest{actionDefinitionIds[]}` replaces set).

### 4.11 Knowledge Bases â€” 15 routes

KB CRUD (`/api/knowledge-bases`, PUT update, DELETE) Â· documents: GET `/{kbId}/documents`, POST same (`UploadDocumentRequest{name,sourceUri,contentType,content,metadataJson?}` â€” content auto-chunked at 1000 chars, statusâ†’ready) Â· GET/DELETE `/api/knowledge-documents/{docId}` Â· chunks POST `/api/knowledge-documents/{docId}/chunks` (`CreateChunkRequest{content,chunkIndex,metadataJson?}`) / DELETE chunk Â· search POST `/api/knowledge-bases/{kbId}/search` (`SearchRequest{query,topK?=5}` â†’ cosine-distance over vector(1536)) Â· persona links (Â§4.8).
Embeddings require `OPENAI_API_KEY`/embedding config; without it search returns `[]`.

### 4.12 Actions Engine â€” `/api/actions` (8)

GET `/?type` Â· GET `/system` (system defs seeded) Â· GET `/{id}` Â· POST (only Integration/Webhook types creatable) Â· PATCH Â· DELETE (system protected) Â· executions: GET `/executions/{id}` Â· GET `/executions/by-call/{callSessionId}`.

### 4.13 Usage â€” `/api/usage` (5)

GET `/?metricType&from&to&callSessionId&licenseId&partnerId` â†’ `UsageRecordDto[]` Â· GET `/summary` â†’ `UsageSummaryDto[]{metricType,totalQuantity,unit,count}` Â· GET `/metric/{type}` Â· GET `/call/{callSessionId}` Â· POST `/` record event â€” **two auth modes**: user JWT, or worker service-token + `callSessionId` (ownership derived from the session row). Metric types: `CallDuration`, `CallMinutes`, `TransferCount`, `RecordingMinutes`, `AgentSessionMinutes`.

### 4.14 API Keys â€” `/api/api-keys` (4)

GET `/` (`ApiKeyListItem{id,name,keyPrefix,status,scopes,lastUsedAt,expiresAt,createdAt}`) Â· POST `{name,scopes?,expiresAt?}` â†’ `CreateApiKeyResponse{rawKey(72ch) shown once}` Â· DELETE `/{id}` revoke Â· PATCH `/{id}/scopes` `{scopes[]}`.

### 4.15 Licenses â€” `/api/licenses` (5)

GET `/` Â· GET `/{id}` Â· POST `{userId,partnerId?,partnerPlanId?,startsAt,endsAt?,limitsJson?,metadataJson?}` Â· PATCH Â· DELETE.

### 4.16 Partners â€” 12 routes

GET `/api/partners` Â· GET `/{id}` Â· GET `/me` Â· PUT `/{id}` Â· customers GET/POST under `/{partnerId}/customers` Â· partner-relationships GET/PUT/DELETE Â· provisioning GET `/{partnerId}/provision/{externalCustomerId}` + POST provision Â· GET `/{partnerId}/stats`.

### 4.17 Plans & Subscriptions (15)

Plans: GET `/api/plans` (active) Â· `/all` Â· `/{id}` Â· POST/PATCH/DELETE Â· partner plans: GET `/{partnerId}/plans`, POST, GET/PUT `/api/partner-plans/{id}`.
Subscriptions: GET `/api/subscriptions` (own) Â· GET `/{id}` Â· POST `{planId,startsAt,endsAt?,trialEndsAt?}` Â· PATCH `{status?,endsAt?}` Â· POST `/{id}/cancel`.

### 4.18 LiveKit â€” `/api/livekit` (5)

POST `/token` `{identity,roomName,canPublish?,canSubscribe?}` â€” **guard:** identities starting `agent_` must have an Accepted transfer into that room, else 403. Returns `{token}`. Â· POST `/room` create Â· DELETE `/room/{roomName}` Â· POST `/room/{roomName}/egress/start|stop`.

### 4.19 Webhooks â€” `/api/webhooks` (4)

POST `/recording-complete` Â· `/call-started` Â· `/call-ended` â†’ ack stubs Â·
**POST `/livekit`** â€” LiveKit server webhook (HS256 JWT in `Authorization`, verified against `LIVEKIT_API_SECRET`; optional `X-Service-Token`). Drives the inbound lifecycle: `participant_joined` (session create + dispatch for callers; cold-swap completion for `agent_*`/`dest_*`; answer-marking for `ai-agent`) Â· `participant_left` (leg close; caller-left ends session) Â· `room_finished`.

### 4.20 SIP Destinations â€” `/api/sip/destinations` (5)

| Method & Route | Contract |
|---|---|
| GET `/` | user's named destinations |
| POST `/` | `{name, callTo, description?}` â†’ 201; 409 duplicate name |
| PATCH `/{id}` | `{name?, callTo?, description?, isEnabled?}` |
| DELETE `/{id}` | remove |
| GET `/options` | transfer options for the AI layer: `{agents:[{type,name,available}], destinations:[{type,name}]}` â€” **CallTo values never leave the backend** |

### 4.21 Worker & Agent-App Shims — `/api/call/*` (7, exempt + optional service token)

POST `/transfer` `{RoomName, TargetType?("human"|"destination"), TargetName?, AgentId?, Reason?}` → 200 `{transferId, agentName?/status}` or 400 `{error}` ·
**GET `/transfer-options?roomName&agentId`** → owner-scoped `{agents:[{id,name,available}], destinations:[{id,name}]}` for the in-call agent (validates requester + Transferred/Active session) ·
**POST `/agent-transfer`** `{roomName, fromAgentId, targetType:"human"|"destination", targetName?, reason?}` — cold swap originating from the on-call human; completion removes the requesting agent (fromIdentity snapshot) ·
**POST `/transfer-decision`** `{transferId, humanAgentId, decision:"accept"|"reject"}` — JWT-free accept/reject for the agent app ·
POST `/active` `{RoomName}` registers legacy rooms only (`call_u*` rooms are webhook-owned) ·
POST `/end` `{RoomName}` · POST `/summary` `{RoomName, Summary}` → handoff context.

---

## 5. Business Logic & Scenarios

### 5.1 Call Lifecycle State Machine

```
Queued(0) â†’ Ringing(1) â†’ Active(2) â†’ Transferred(3) â†’ Completed(4)
     \            \                        \
      `â†’ Failed(5) `â†’ Failed(5)/Cancelled(6) `â†’ Completed(4)
```

Who drives transitions:

| Transition | Driver |
|---|---|
| create Queued | `CallSessionService.CreateAsync` (AI worker / inbound flow) |
| â†’ Ringing/Active | LiveKit room events via AI worker + participants rows |
| â†’ Transferred | supervisor or AI triggers `POST â€¦/transfers` (Â§4.4) |
| â†’ Completed | `POST /end` (dashboard/agent) or worker hangup |
| Failed/Cancelled | worker error paths / explicit cancel |

Every query is **tenant-scoped** (`UserId == owner`) â€” multi-tenancy is enforced at the service layer, not just the UI.

### 5.2 Transfers (v0: cold silent swap â€” the core scenario)

**Human target (internal agent app):**
1. Trigger: customer asks the AI â†’ Gemini tool `transfer_to_human(name?, reason?)` â†’ worker POSTs `/api/call/transfer {TargetType:"human"}`.
2. Service resolves the named agent (exact, then prefix match â€” never substitutes another agent) with `Status==Available`, owner-scoped, respecting MaxConcurrentCalls; no name â†’ best available.
3. Creates `CallTransfer(Cold, HumanAgent, Requested)` + `CallHandoff`; SignalR `IncomingTransfer` â†’ device rings (30s countdown).
4. Agent **accepts** â†’ Accepted; backend removes `ai-agent` from the room 3s later (webhook fallback) â€” **cold silent swap**: the caller's leg never moves.
5. Webhook sees `agent_{id}` join â†’ transfer Completed, webrtc leg opens, AI leg closes (`RemoveParticipant` authoritative).
6. **Reject/timeout** (Hangfire `TransferTimeoutProcessor`, ~10s cadence) â†’ Failed + cascade/`TransferExpired`.

**External destination (customer PBX):**
1. Tool `transfer_to_department(name)` â†’ `/api/call/transfer {TargetType:"destination"}`.
2. Backend looks up `sip_destinations` by name (enabled) â†’ creates Transfer(ExternalDestination) â†’ background `CreateSIPParticipant` via outbound trunk, identity `dest_{id}`, ringback played to room.
3. Issabel answers and runs its own queue/ring-group/IVR distribution â€” platform knows nothing about internal extensions.
4. `dest_*` participant joined (webhook or 45s poller) â†’ same cold swap. No answer â†’ Failed, AI resumes.

Failures at step 2 return HTTP 400 and the AI stays on the call speaking the error text. **Agent-originated (agent app):** the on-call human can hand off too — a Transfer sheet lists colleagues + external destinations (`GET /api/call/transfer-options`); `POST /api/call/agent-transfer` runs the identical cold swap, storing the requesting agents identity as `fromIdentity` so completion removes *them*, not the AI. Accept/reject from the app go through JWT-free `POST /api/call/transfer-decision`. Every transfer stores a `TargetSnapshotJson` so routing edits can't corrupt in-flight calls.

### 5.3 Access-Key Agent Login

`POST /api/auth/agent-login` hashes the presented key (SHA-256 hex, lowercase) and matches an Active, non-expired row; success bumps LastUsedAt and returns a **LiveKit JWT** (HS256, `video.roomJoin`, 2h exp) plus `livekitUrl`. Revoking the key instantly blocks future logins (sessions already connected are dropped by SignalR disconnect handler setting Offline).

### 5.4 RAG Pipeline

Upload doc (text) â†’ split into â‰¤1000-char chunks â†’ each chunk gets an embedding via OpenAI `text-embedding-3-small` (1536 dims stored as pgvector). **Live calls retrieve via the AI's `search_knowledge(query)` tool** (v0): it hits `GET /api/personas/{id}/knowledge-context` which searches every persona-linked KB (`SearchPersonaKnowledgeAsync`, cosine `<=>`, top-4 merged across KBs) and returns formatted bullets to Gemini. Without embeddings configured, retrieval falls back to ILike text match; without linked KBs the tool reports no info. Dashboard provides upload UI + a semantic-search tester.

### 5.5 Versioning & Publish Semantics

Personas and Workflows are immutable-versioned: drafts append (`VersionNumber` increments), exactly one version carries `IsPublished=true` (publishing flips others off). Call Configurations bind to personas/workflows by Id â€” runtime uses the **published** version.

### 5.6 Metering & Licensing

The worker records one idempotent `call_minutes` row per AI-handled call: at customer disconnect it POSTs `/api/usage` with the session's `callSessionId`; the endpoint authenticates via service token and derives ownership from that session row. Licenses define windows (`StartsAt/EndsAt/LimitsJson`); plans/subscriptions define purchasable tiers with trial support. Summary endpoints aggregate by metric type.

### 5.7 Recording Flow

Egress started per room (`/egress/start`) â†’ writes to MinIO `recordings` bucket â†’ callback marks `call_recordings.Available` â†’ dashboard fetches presigned download URL.

---

## 6. Dashboard Frontend

Stack: React 19, Vite 7, Tailwind 4, react-router 7, @microsoft/signalr. Dev server `:5173`; `VITE_API_URL=http://localhost:8080`.

### Routes (17)

| Path | Page | Data sources |
|---|---|---|
| `/` | LandingPage (marketing) | static |
| `/login` | LoginPage (loginâ‡„register toggle) | authApi |
| `/dashboard` index & `/live` | LiveBoard | stats/today Â· calls/active Â· SignalR QueueUpdate |
| `/dashboard/queue` | QueuePage | stats/queue |
| `/dashboard/roster` | AgentRoster | human-agents + stats/agents + personas |
| `/dashboard/analytics` | Analytics | stats/hourly Â· period Â· intents |
| `/dashboard/history` | CallHistory | calls list (server pagination) |
| `/dashboard/call/:id` | CallDetail | calls/{id} Â· end Â· transfers Â· recordings/download |
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
- **Landing sections**: Nav, Hero, Pricing, Agents, Faq, Footer, Calculator, HybridLoop, ApiStatusâ€¦

### API Client Internals (`src/api/client.ts`)

- Single source of truth for the JWT (`localStorage: tandem_access_token`)
- **Transparent refresh**: any 401 (except auth paths) â†’ one shared in-flight `POST /api/auth/refresh` â†’ retry once â†’ on failure clears token
- **Error parsing**: ValidationProblemDetails `errors{field:[msgs]}` flattened to readable text
- `useApi(fetcher, deps)` hook supplies `{data,loading,error,refetch}`
- `useLiveHub(enabled)` builds the SignalR connection (`withCredentials:false`, auto-reconnect ladder) exposing `{connected, queue, tick}` â€” `tick` is used as a refetch dependency for near-realtime pages

### Normalization Tables (`dashboard/statusMap.ts`)

```
API â†’ UI status:  Queuedâ†’queued Â· Ringing/Activeâ†’active-ai Â· Transferredâ†’active-human
                  Completedâ†’completed-ai Â· Missedâ†’missed Â· Failedâ†’abandoned
HumanAgent numericâ†’UI: 1â†’online Â· 4â†’busy Â· 2/3â†’break Â· 0/otherâ†’offline
```

Demo-mode mocks (`data.ts`) render **only** when `VITE_API_URL` is unset; with it set, pages show honest loading/error/empty states.

---

## 7. Agent App

Expo SDK 57 entry (`index.js` â†’ `App.js`). Backend URL by platform: iOS `http://localhost:5000` Â· Android-emu `http://10.0.2.2:5000` Â· **web `http://127.0.0.1:5000`** (requires published 5000 + the `:8081` CORS origin).

### Platform Split (Metro resolution)

```js
const ActiveCall = require('./AgentCall').default;
// AgentCall.web.js  â†’ livekit-client (browser WebRTC audio-only)
// AgentCall.js      â†’ @livekit/react-native <LiveKitRoom>
```
Never import `@livekit/react-native` unconditionally â€” its webrtc package needs `requireNativeComponent` and crashes web bundles.

### Screens / States

login â†’ dashboard(idle | history tabs, status pills Available/Break/NotReady, connection dot, pending-transfer badge) â†’ ringing (vibration loop + 30s countdown + Answer/Reject) â†’ call (timer MM:SS, mute toggle via mic enable, AI-handoff summary card, End) â†’ notes (disposition saved to `metadata_json`) .

Second incoming while busy â†’ queued in `pendingTransfers[]` with tab badge. `TransferExpired` clears a stale ring. Logout stops SignalR and resets all state.

Web call specifics: `resolveLiveKitUrl()` rewrites internal hosts (`ws://livekit:` / `127.0.0.1`) to the browser hostname; remote audio auto-attached; mic granted via getUserMedia on connect. In-call **Transfer…** button opens colleagues/external-destination lists and hands the call off via `/api/call/*` shims.

---

## 8. AI Worker & Legacy Pages

### python-ai-worker

Joins rooms as identity **`ai-agent`** using **Google Gemini Live** (`AGENT_MODEL=models/gemini-3.1-flash-live-preview`, voice `Aoede`, temperature 0.7). SDK **pinned** (`livekit-agents==1.6.10` + matching google plugin); identity is set via `WorkerOptions(request_fnc=â€¦)` â†’ `job_request.accept(identity="ai-agent")` â€” 1.x has no `identity=` kwarg on `connect()` and otherwise defaults to `agent-{jobId}`. Registers as a named agent (`agent_name=voice-agent`) â†’ only explicit dispatches create jobs. Persona comes from dispatch metadata `{sessionId, personaId}` â†’ `GET /api/personas/{id}/published` (env `PERSONA_ID` fallback). Tools: `transfer_to_human(name?, reason?)`, `transfer_to_department(name, reason?)`, and `search_knowledge(query)` (RAG over persona-linked KBs via `/knowledge-context`). On customer disconnect the worker posts call end + one idempotent `call_minutes` usage row (ownership derived backend-side from `callSessionId`). All machine calls carry `X-Service-Token`. Env: `LIVEKIT_URL/KEY/SECRET`, `GEMINI_API_KEY`, `BACKEND_URL=http://backend:5000`, `BACKEND_SERVICE_TOKEN`, `AGENT_NAME`.

### Legacy wwwroot

`backend/wwwroot/` still serves first-generation admin/agent HTML (`admin.html`, `agent.html`, `index.html`, `app.js`) â€” kept for reference, superseded by the React dashboard and Expo app.

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
| 8080 | Caddy â†’ backend API + LiveKit signaling |
| 5000 | backend (published for agent-app) |
| 5432 | Postgres (callcenter/admin/adminpassword) |
| 7880â€“7882 | LiveKit signaling (`livekit.yaml`, devkey/secret) |
| 7882â€“7892/udp | LiveKit WebRTC media range |
| **5061/udp+tcp** | LiveKit-SIP (customer PBX peers here) |
| 9000/9001 | MinIO S3 / console |
| 5173 / 8081 | dashboard / expo dev servers |

### Key env vars

Backend: `ConnectionStrings__DefaultConnection` Â· `JWT_SECRET` Â· `LIVEKIT_URL/KEY/SECRET` Â· `REDIS_CONNECTION` Â· `MINIO_*` Â· `CORS_ORIGINS` Â· `BACKEND_SERVICE_TOKEN` Â· `LIVEKIT_OUTBOUND_TRUNK_ID` (destination transfers).
Worker/setup: `AGENT_NAME=voice-agent` Â· `PERSONA_ID` (fallback) Â· `PBX_IP` Â· `SIP_DIDS` Â· `OWNER_USER_ID`.

Hangfire dashboards: `/hangfire`. Swagger: `/swagger`. Recurring jobs registered at startup with `SchedulePollingInterval=2s` (required for sub-15s crons).

### Tests

```powershell
dotnet test back/backend.Tests        # 21/21 passing
```

> âš  On hosts with Windows Application Control (Smart App Control), the local vstest/xunit host can fail to load freshly built DLLs (`0x800711C7`). Run the suite in a Linux container instead:
> ```powershell
> docker run --rm -v "${PWD}\back:/src" -w /src/backend.Tests mcr.microsoft.com/dotnet/sdk:10.0 dotnet test
> ```

---

## 10. Verified E2E Scenarios (executed against the running stack)

| # | Scenario | Result |
|---|---|---|
| 1 | Docker stack boot Ã—10 services healthy | âœ… |
| 2 | Register â†’ JWT â†’ all scoped reads return zeros | âœ… |
| 3 | Login/refresh chain rotates tokens | âœ… |
| 4 | Seed call â†’ appears in list/active/detail; stats increment | âœ… |
| 5 | SignalR WebSocket through Caddy receives QueueUpdate (~3s cadence after scheduler fix) | âœ… 8 events/25s |
| 6 | CORS preflight for both `5173` and `8081` origins incl. credentials | âœ… |
| 7 | Supervisor force-end + transfer initiation â†’ `Requested` routed to Available agent | âœ… |
| 8 | Persona create â†’ v1 publish â†’ workflow version publish â†’ config bind + activate + name resolution | âœ… |
| 9 | KB create â†’ doc upload (auto-chunk) â†’ search â†’ link/unlink persona | âœ… |
| 10 | API key issue (72-char secret, show-once) + revoke | âœ… |
| 11 | Human-agent onboard + access key issue (44-char) â†’ agent-login validates hash | âœ… |
| 12 | Usage summary/metrics; licenses/partners lists | âœ… |
| 13 | Agent-app web bundle excludes native-webrtc; android/iOS include it | âœ… proven by bundle diff |
| 14 | Backend test suite | âœ… 21/21 |
| 15 | v0 build: backend compile clean Â· compose config valid Â· `main.py`/`sip-setup.py` py_compile (container) Â· frontend `tsc --noEmit` | âœ… |
| 16 | Patch runner: fresh-DB + legacy-baseline paths (`_schema_patches`) | âœ… code-verified; live DB pending |
| 17 | **Live PSTNâ†’AIâ†’transfer call over SIP** | â³ pending first real deployment with `PBX_IP` |

## 11. Fix Log (session bugs â†’ root cause â†’ fix)

1. compose YAML crash â€” unquoted colon value â†’ quote `"LIVEKIT_KEYS=devkey: secret"`
2. .NET 9 base images vs net10 csproj â†’ sdk/aspnet `:10.0`
3. plain postgres lacks pgvector â†’ `pgvector/pgvector:pg15`
4. EF unmappable `float[]`â†”vector â†’ `Pgvector.EntityFrameworkCore` + `Vector?` + `UseVector()`
5. tests broke (InMemory can't map Vector) â†’ provider-aware conversion in OnModelCreating
6. fresh DB missing extension â†’ `CREATE EXTENSION IF NOT EXISTS vector` before EnsureCreated
7. `/api/calls/active` 500 â€” enum-array `.Contains()` funcletizer ReadOnlySpan crash â†’ `List<T>`
8. same endpoint â€” OrderBy **after** correlated-count projection untranslated â†’ reorder before Select
9. frontend `TodayStats` field mismatch (`total` vs `totalCalls`) â†’ aligned to real DTOs
10. dashboard pointed at unpublished 5000 â†’ `VITE_API_URL=:8080` + publish `5000:5000`
11. missing vite-env types â†’ added `src/vite-env.d.ts`
12. CORS blocked SignalR (credentials) â†’ `AllowCredentials()` + client `withCredentials:false`
13. broadcasts stuck at 15s cadence â†’ `SchedulePollingInterval=2s`
14. `GET /call-configurations` 500 â€” nav-join + correlated count untranslated â†’ fetch-then-resolve `ProjectAsync`
15. workflow version create 500 â€” body needed `{definitionJson:"â€¦"}` string wrapper
16. agent-app web crash (`requireNativeComponent`) â†’ unsuffixed platform-split `require('./AgentCall')`
17. first platform-split attempt leaked native module into web graph â†’ single specifier + suffixed files
18. roster showed mocks despite real data â†’ removed silent fallback; honest empty/error states
19. numeric enum statuses rendered wrong (`0..4`) â†’ dual numeric/string decoder
20. Expo-web CORS 8081 rejected â†’ added origin to `CORS_ORIGINS`

---

## 12. LiveKit-SIP Integration Guide (v0)

Asterisk has been removed entirely. LiveKit-SIP is the only SIP gateway; the customer PBX (e.g. Issabel) peers directly into it by IP.

### 12.1 Architecture

```
PBX â”€â”€INVITEâ”€â”€â–¶ livekit-sip:5061 â”€â”€trunk+ruleâ”€â”€â–¶ room "call_u{userId}<rand>" (answered here)
     webhook participant_joined â”€â”€â–¶ backend: owner from room prefix â†’ DefaultPersonaId
                                   â†’ CreateAgentDispatch("voice-agent", {sessionId,personaId})
worker joins as identity "ai-agent", fetches persona, greets caller.
```

Transfers are cold silent swaps inside the SAME room (the caller's leg never moves):

| Target | Mechanism | Swap trigger |
|---|---|---|
| Internal human agent | SignalR invite (`IncomingTransfer`) â†’ agent app Accept â†’ WebRTC join with identity `agent_{id}` | webhook on join (fallback: backend removes AI 3s after accept) |
| External PBX destination | `CreateSIPParticipant` via outbound trunk, identity `dest_{id}`; Issabel does its own queue/ringgroup/IVR distribution | webhook on join or 3s poller |

AI tools: `transfer_to_human(name?, reason?)`, `transfer_to_department(name, reason?)`
â†’ POST `/api/call/transfer` `{RoomName, TargetType, TargetName}`.
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
- outbound trunk `outbound-pbx` (PBX_IP:5060) â€” print its id into `LIVEKIT_OUTBOUND_TRUNK_ID`

Then in the dashboard: Personas page â†’ set persona â†’ **SIP Destinations** page â†’ set Default AI Persona + add named destinations (Support/Sales/â€¦). The AI only ever sees names; `call_to` values stay server-side.

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

1. Dial DID â†’ session row created, `pstn_in` leg open, AI greets â‰¤ ~3s.
2. "transfer me to \<agent name\>" â†’ app rings â†’ Accept â†’ AI removed instantly, transfer Completed.
3. Decline/30s timeout â†’ transfer Failed, AI continues.
4. "transfer me to support" â†’ Issabel receives call at mapped target â†’ answer swaps instantly.
5. Caller hangs up mid-transfer â†’ transfers Cancelled, legs closed, agent freed.

---

*End of report.*


