import { useState } from "react";
import AgentCard from "../components/AgentCard";
import Badge from "../components/Badge";
import { AGENTS, type Agent } from "../data";
import { statsApi, personasApi, agentsAdminApi } from "../../api/endpoints";
import type {
  AgentStatsDto,
  PersonaListItem,
  HumanAgentAdminDto,
} from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

// Demo fallback ONLY when no backend URL is configured.
const MOCK_HUMANS = AGENTS.filter((a) => a.type === "human");
const MOCK_AI = AGENTS.filter((a) => a.type === "ai");

function agentStatusToUi(status: string | number, isActive: boolean): Agent["status"] {
  // /api/human-agents serializes HumanAgentStatus as a NUMBER:
  // 0 Offline · 1 Available · 2 Break · 3 NotReady · 4 InCall
  const n = typeof status === "number" ? status : Number(status);
  if (!Number.isNaN(n)) {
    if (!isActive && n !== 1) return "offline";
    switch (n) {
      case 1: return "online";
      case 4: return "busy";
      case 2:
      case 3: return "break";
      default: return "offline";
    }
  }
  const s = String(status);
  if (!isActive && s !== "Available") return "offline";
  switch (s) {
    case "Available": return "online";
    case "InCall":
    case "Busy": return "busy";
    case "Break":
    case "NotReady": return "break";
    default: return "offline";
  }
}

function initialsOf(name: string): string {
  return name
    .split(/\s+/)
    .map((w) => w[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();
}

export default function AgentRoster() {
  const [tab, setTab] = useState<"ai" | "human">("ai");

  const {
    data: apiAgents,
    error: humansError,
    loading: humansLoading,
  } = useApi(() => (API_ENABLED ? agentsAdminApi.list() : Promise.resolve(null)), []);

  const { data: agentStats } = useApi(
    () => (API_ENABLED ? statsApi.agents() : Promise.resolve(null)),
    []
  );

  const {
    data: apiPersonas,
    error: aiError,
    loading: aiLoading,
  } = useApi(() => (API_ENABLED ? personasApi.list() : Promise.resolve(null)), []);

  // ── HUMANS: real rows joined with per-agent call stats ──────────────
  const statsById = new Map<string, AgentStatsDto>(
    (agentStats ?? []).map((s) => [s.agentId, s])
  );

  const humanAgents: Agent[] | null = API_ENABLED
    ? (apiAgents ?? []).map((a: HumanAgentAdminDto) => ({
        id: a.id,
        name: a.name,
        type: "human" as const,
        status: agentStatusToUi(a.status, a.isActive),
        role: a.email ?? `max ${a.maxConcurrentCalls} call${a.maxConcurrentCalls === 1 ? "" : "s"}`,
        languages: [],
        initials: initialsOf(a.name),
        avatarUrl: null,
        activeCalls: 0,
        callsToday: statsById.get(a.id)?.totalCalls ?? 0,
        avgHandleTime: statsById.get(a.id)?.avgDurationSeconds ?? 0,
        capacity: Math.min(100, Math.round(((statsById.get(a.id)?.totalCalls ?? 0) / Math.max(a.maxConcurrentCalls * 10, 1)) * 100)),
      }))
    : MOCK_HUMANS;

  // ── AI: real personas ────────────────────────────────────────────────
  const aiAgents: Agent[] | null = API_ENABLED
    ? (apiPersonas ?? []).map((p: PersonaListItem, idx: number) => ({
        id: p.id,
        name: p.name,
        type: "ai" as const,
        status: p.isActive ? ("online" as const) : ("offline" as const),
        role: p.description?.slice(0, 60) || "AI voice persona",
        languages: [],
        initials: initialsOf(p.name) || `P${idx}`,
        avatarUrl: null,
        activeCalls: 0,
        callsToday: 0,
        avgHandleTime: 0,
        capacity: p.isActive ? 100 : 0,
      }))
    : MOCK_AI;

  const showHumanMockNotice = !API_ENABLED;
  const onlineHumans = (humanAgents ?? []).filter((a) => a.status !== "offline").length;
  const activeAi = (aiAgents ?? []).filter((a) => a.status !== "offline").length;
  const agents = tab === "ai" ? (aiAgents ?? []) : (humanAgents ?? []);
  const tabLoading = tab === "ai" ? aiLoading : humansLoading;
  const tabError = tab === "ai" ? aiError : humansError;

  return (
    <div className="space-y-8">
      <div>
        <p className="kicker mb-1">// agent roster</p>
        <h1 className="font-display text-3xl font-bold tracking-tight text-mist">
          {tab === "ai" ? "AI personas" : "Human agents"}
          {API_ENABLED && (
            <span className={`ml-3 inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-[10px] font-mono font-normal uppercase tracking-[0.12em] ${
              (tab === "ai" ? apiPersonas : apiAgents)
                ? "border-mint/30 bg-mint/10 text-mint"
                : "border-line bg-panel/40 text-dim"
            }`}>
              <span className={`h-1.5 w-1.5 rounded-full ${(tab === "ai" ? apiPersonas : apiAgents) ? "bg-mint animate-pulse" : "bg-dim"}`} />
              {(tab === "ai" ? apiPersonas : apiAgents) ? "Live API" : "connecting…"}
            </span>
          )}
        </h1>
        <p className="mt-1 text-sm text-dim">
          {tab === "ai"
            ? `${activeAi} of ${aiAgents.length} AI personas active`
            : `${onlineHumans} of ${humanAgents.length} humans online`}
          {!API_ENABLED && " · demo data"}
        </p>
      </div>

      <div className="flex gap-2">
        <button
          onClick={() => setTab("ai")}
          className={`btn !px-5 !py-2.5 !text-[10px] ${tab === "ai" ? "btn-mint" : "btn-ghost"}`}
        >
          AI personas ({aiAgents.length})
        </button>
        <button
          onClick={() => setTab("human")}
          className={`btn !px-5 !py-2.5 !text-[10px] ${tab === "human" ? "btn-amber" : "btn-ghost"}`}
        >
          Humans ({humanAgents.length})
        </button>
      </div>

      {API_ENABLED && tabError && (
        <p className="border border-coral/40 bg-coral/10 px-4 py-3 font-mono text-[11px] text-coral">
          {tabError}
        </p>
      )}

      {API_ENABLED && !tabError && !tabLoading && agents.length === 0 ? (
        <div className="border border-line bg-panel/40 p-8 text-center">
          <p className="font-display text-lg font-semibold text-mist">
            No {tab === "ai" ? "personas" : "human agents"} yet
          </p>
          <p className="mt-1 font-mono text-[11px] text-dim">
            {tab === "ai"
              ? "Create one under Platform Setup → AI Personas"
              : "Onboard one under Business → Human Agents"}
          </p>
        </div>
      ) : tabLoading && API_ENABLED ? (
        <p className="font-mono text-[11px] text-dim">loading…</p>
      ) : (
        <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {agents.map((a) => (
            <AgentCard key={a.id} agent={a} />
          ))}
        </div>
      )}

      <div className="border border-line bg-panel/40 p-5">
        <p className="mb-4 font-mono text-[10px] uppercase tracking-[0.18em] text-dim">
          coverage summary
        </p>
        <div className="grid gap-6 sm:grid-cols-3">
          <div>
            <p className="font-display text-3xl font-bold tabular-nums text-mist">
              <span style={{ color: "var(--color-mint)" }}>{activeAi}</span>
              <span className="text-dim">/{aiAgents.length}</span>
            </p>
            <Badge label="AI personas active" tone="mint" />
          </div>
          <div>
            <p className="font-display text-3xl font-bold tabular-nums text-mist">
              <span style={{ color: "var(--color-amber)" }}>{onlineHumans}</span>
              <span className="text-dim">/{humanAgents.length}</span>
            </p>
            <Badge label="humans online" tone="amber" />
          </div>
          <div>
            <p className="font-display text-3xl font-bold tabular-nums text-mist">
              <span style={{ color: "var(--color-mist)" }}>
                {Math.round(
                  humanAgents.reduce((sum, a) => sum + a.callsToday, 0) /
                    Math.max(humanAgents.length, 1)
                )}
              </span>
            </p>
            <Badge label="avg calls per human" tone="dim" />
          </div>
        </div>
        {showHumanMockNotice && (
          <p className="mt-4 font-mono text-[9px] uppercase tracking-[0.14em] text-dim/60">
            demo mode · set vite_api_url for live data
          </p>
        )}
      </div>
    </div>
  );
}
