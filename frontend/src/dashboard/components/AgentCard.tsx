import StatusDot from "./StatusDot";
import ProgressBar from "./ProgressBar";
import type { Agent } from "../data";

const STATUS_LABELS = {
  online: "available",
  busy: "on call",
  break: "on break",
  offline: "offline",
};

export default function AgentCard({ agent }: { agent: Agent }) {
  const isHuman = agent.type === "human";
  const accent = isHuman ? "amber" : "mint";

  return (
    <article
      className={`lift group border p-5 ${
        isHuman
          ? "border-amber/30 bg-gradient-to-b from-amber/[0.06] to-panel/70"
          : "border-line bg-panel/70 hover:border-mint/40"
      }`}
    >
      <div className="flex items-start justify-between">
        <span className={`chip ${isHuman ? "!border-amber/50 !text-amber" : "!border-mint/40 !text-mint"}`}>
          {isHuman ? "human" : "ai agent"}
        </span>
        <div className="flex items-center gap-1.5">
          <StatusDot status={agent.status} />
          <span className="font-mono text-[9px] uppercase tracking-[0.14em] text-dim">
            {STATUS_LABELS[agent.status]}
          </span>
        </div>
      </div>

      <div className="mt-4 flex items-center gap-3">
        {agent.avatarUrl ? (
          <img
            src={agent.avatarUrl}
            alt={agent.name}
            className={`h-12 w-12 rounded-full border-2 object-cover ${
              isHuman ? "border-amber/50" : "border-mint/40"
            }`}
          />
        ) : (
          <span
            className={`relative flex h-12 w-12 items-center justify-center rounded-full border bg-deep ${
              isHuman ? "border-amber/40" : "border-mint/40"
            }`}
          >
            <span
              className="font-display text-sm font-bold"
              style={{ color: `var(--color-${accent})` }}
            >
              {agent.initials}
            </span>
            {agent.status !== "offline" && (
              <span
                className="pulse-dot absolute -right-0.5 -top-0.5 h-2.5 w-2.5 rounded-full border-2 border-panel"
                style={{ backgroundColor: `var(--color-${accent})` }}
              />
            )}
          </span>
        )}
        <div>
          <h3 className="font-display text-lg font-bold tracking-tight text-mist">{agent.name}</h3>
          <p
            className="font-mono text-[10px] uppercase tracking-[0.14em]"
            style={{ color: `var(--color-${accent})` }}
          >
            {agent.role}
          </p>
        </div>
      </div>

      <div className="mt-4 grid grid-cols-2 gap-3">
        <div>
          <p className="font-mono text-[9px] uppercase tracking-[0.14em] text-dim">active calls</p>
          <p className="font-display text-xl font-bold tabular-nums text-mist">{agent.activeCalls}</p>
        </div>
        <div>
          <p className="font-mono text-[9px] uppercase tracking-[0.14em] text-dim">today</p>
          <p className="font-display text-xl font-bold tabular-nums text-mist">{agent.callsToday}</p>
        </div>
        <div>
          <p className="font-mono text-[9px] uppercase tracking-[0.14em] text-dim">avg handle</p>
          <p className="font-display text-xl font-bold tabular-nums text-mist">
            {Math.floor(agent.avgHandleTime / 60)}m{agent.avgHandleTime % 60}s
          </p>
        </div>
        <div>
          <p className="font-mono text-[9px] uppercase tracking-[0.14em] text-dim">capacity</p>
          <p className="font-display text-xl font-bold tabular-nums text-mist">{agent.capacity}%</p>
        </div>
      </div>

      <div className="mt-4">
        <ProgressBar value={agent.capacity} tone={agent.capacity > 80 ? "coral" : agent.capacity > 60 ? "amber" : "mint"} label="capacity" />
      </div>

      <div className="mt-3 flex flex-wrap gap-1">
        {agent.languages.map((l) => (
          <span key={l} className="chip !text-[8px] !px-2 !py-1">
            {l}
          </span>
        ))}
      </div>

      {agent.model && (
        <p className="mt-3 font-mono text-[9px] text-dim/70">{agent.model}</p>
      )}
    </article>
  );
}