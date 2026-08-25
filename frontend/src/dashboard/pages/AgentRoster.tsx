import { useState } from "react";
import AgentCard from "../components/AgentCard";
import { AGENTS } from "../data";

const AI_AGENTS = AGENTS.filter((a) => a.type === "ai");
const HUMAN_AGENTS = AGENTS.filter((a) => a.type === "human");

export default function AgentRoster() {
  const [tab, setTab] = useState<"ai" | "human">("ai");

  const agents = tab === "ai" ? AI_AGENTS : HUMAN_AGENTS;

  return (
    <div className="space-y-8">
      <div>
        <p className="kicker mb-1">// agent roster</p>
        <h1 className="font-display text-3xl font-bold tracking-tight text-mist">
          {tab === "ai" ? "AI agents" : "Human agents"}
        </h1>
        <p className="mt-1 text-sm text-dim">
          {tab === "ai"
            ? `${AI_AGENTS.length} AI agents handling calls around the clock`
            : `${HUMAN_AGENTS.filter((a) => a.status !== "offline").length} of ${HUMAN_AGENTS.length} humans online`}
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
          Humans ({HUMAN_AGENTS.length})
        </button>
      </div>

      <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        {agents.map((a) => (
          <AgentCard key={a.id} agent={a} />
        ))}
      </div>

      <div className="border border-line bg-panel/40 p-5">
        <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim mb-4">
          coverage summary
        </p>
        <div className="grid gap-6 sm:grid-cols-3">
          <div>
            <p className="font-display text-3xl font-bold tabular-nums text-mist">
              <span style={{ color: "var(--color-mint)" }}>4</span>
            </p>
            <p className="mt-1 font-mono text-[10px] uppercase tracking-[0.14em] text-dim">
              AI agents active
            </p>
          </div>
          <div>
            <p className="font-display text-3xl font-bold tabular-nums text-mist">
              <span style={{ color: "var(--color-amber)" }}>3/5</span>
            </p>
            <p className="mt-1 font-mono text-[10px] uppercase tracking-[0.14em] text-dim">
              humans available
            </p>
          </div>
          <div>
            <p className="font-display text-3xl font-bold tabular-nums text-mist">
              <span style={{ color: "var(--color-mint)" }}>94%</span>
            </p>
            <p className="mt-1 font-mono text-[10px] uppercase tracking-[0.14em] text-dim">
              AI auto-resolve rate
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}