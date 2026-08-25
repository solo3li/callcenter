import { useState } from "react";
import AgentCard from "../components/AgentCard";
import Badge from "../components/Badge";
import { AGENTS, type Agent } from "../data";
import { statsApi } from "../../api/endpoints";
import type { AgentStatsDto } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

const AI_AGENTS = AGENTS.filter((a) => a.type === "ai");

function mapAgentStats(a: AgentStatsDto, idx: number): Agent {
  const status =
    a.status === "Available" ? "online" :
    a.status === "InCall" || a.status === "Busy" ? "busy" :
    a.status === "Break" ? "break" : "offline";

  const initials = a.name
    .split(" ")
    .map((w) => w[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  return {
    id: a.agentId,
    name: a.name,
    type: "human",
    status: status as Agent["status"],
    role: `Human agent`,
    languages: [],
    initials: initials || `A${idx}`,
    avatarUrl: null,
    activeCalls: 0,
    callsToday: a.totalCalls,
    avgHandleTime: a.avgDurationSeconds,
    capacity: Math.min(100, Math.round((a.totalCalls / Math.max(a.totalCalls + (a.lastActiveAt ? 5 : 15), 1)) * 100)),
  };
}

export default function AgentRoster() {
  const [tab, setTab] = useState<"ai" | "human">("ai");

  const { data: apiAgents, error, loading } = useApi(
    () => statsApi.agents(),
    []
  );

  const humanAgents: Agent[] =
    API_ENABLED && apiAgents && apiAgents.length > 0
      ? apiAgents.map(mapAgentStats)
      : AGENTS.filter((a) => a.type === "human");

  const agents = tab === "ai" ? AI_AGENTS : humanAgents;
  const onlineHumans = humanAgents.filter((a) => a.status !== "offline").length;

  return (
    <div className="space-y-8">
      <div>
        <p className="kicker mb-1">// agent roster</p>
        <h1 className="font-display text-3xl font-bold tracking-tight text-mist">
          {tab === "ai" ? "AI agents" : "Human agents"}
          {API_ENABLED && tab === "human" && apiAgents && (
            <span className="ml-3 inline-flex items-center gap-1.5 rounded-full border border-mint/30 bg-mint/10 px-2.5 py-0.5 text-[10px] font-mono font-normal uppercase tracking-[0.12em] text-mint">
              <span className="h-1.5 w-1.5 rounded-full bg-mint animate-pulse" />
              Live API
            </span>
          )}
        </h1>
        <p className="mt-1 text-sm text-dim">
          {tab === "ai"
            ? `${AI_AGENTS.length} AI agents handling calls around the clock`
            : `${onlineHumans} of ${humanAgents.length} humans online`}
        </p>
      </div>

      <div className="flex gap-2">
        <button
          onClick={() => setTab("ai")}
          className={`btn !px-5 !py-2.5 !text-[10px] ${
            tab === "ai" ? "btn-mint" : "btn-ghost"
          }`}
        >
          AI agents ({AI_AGENTS.length})
        </button>
        <button
          onClick={() => setTab("human")}
          className={`btn !px-5 !py-2.5 !text-[10px] ${
            tab === "human" ? "btn-amber" : "btn-ghost"
          }`}
        >
          Humans ({humanAgents.length})
        </button>
      </div>

      {API_ENABLED && tab === "human" && error && (
        <p className="border border-coral/40 bg-coral/10 px-4 py-3 font-mono text-[11px] text-coral">
          {error}
        </p>
      )}

      {loading && API_ENABLED && tab === "human" ? (
        <p className="font-mono text-[11px] text-dim">loading agents…</p>
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
              <span style={{ color: "var(--color-mint)" }}>{AI_AGENTS.length}</span>
            </p>
            <Badge label="AI agents active" tone="mint" />
          </div>
          <div>
            <p className="font-display text-3xl font-bold tabular-nums text-mist">
              <span style={{ color: "var(--color-amber)" }}>{onlineHumans}</span>
              <span className="text-dim">/{humanAgents.length}</span>
            </p>
            <Badge label={`humans online`} tone="amber" />
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
            <Badge label="avg calls today" tone="dim" />
          </div>
        </div>
      </div>
    </div>
  );
}
