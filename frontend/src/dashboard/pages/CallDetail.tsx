import { useParams, useNavigate } from "react-router-dom";
import Badge from "../components/Badge";
import ProgressBar from "../components/ProgressBar";
import { CALLS } from "../data";

function fmtDuration(s: number): string {
  const m = Math.floor(s / 60);
  const sec = s % 60;
  return `${m}m ${sec}s`;
}

export default function CallDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const call = CALLS.find((c) => c.id === id);

  if (!call) {
    return (
      <div className="space-y-6">
        <button onClick={() => navigate(-1)} className="flex items-center gap-2 font-mono text-[11px] uppercase tracking-[0.14em] text-dim transition-colors hover:text-mist">
          <svg width="12" height="10" viewBox="0 0 12 10" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M11 5H1M4 1 .5 5 4 9" /></svg>
          Back
        </button>
        <p className="font-display text-2xl text-mist">Call not found</p>
      </div>
    );
  }

  const statusBadge: Record<string, { label: string; tone: "mint" | "amber" | "coral" | "dim" }> = {
    "active-ai": { label: "AI — live now", tone: "mint" },
    "active-human": { label: "Human — live now", tone: "amber" },
    queued: { label: "Waiting in queue", tone: "coral" },
    "completed-ai": { label: "AI resolved", tone: "mint" },
    "completed-escalated": { label: "Escalated to human", tone: "amber" },
    missed: { label: "Missed", tone: "coral" },
    abandoned: { label: "Abandoned", tone: "dim" },
  };

  const sb = statusBadge[call.status];

  return (
    <div className="space-y-6">
      <button
        onClick={() => navigate("/dashboard/history")}
        className="flex items-center gap-2 font-mono text-[11px] uppercase tracking-[0.14em] text-dim transition-colors hover:text-mist"
      >
        <svg width="12" height="10" viewBox="0 0 12 10" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M11 5H1M4 1 .5 5 4 9" /></svg>
        Back to history
      </button>

      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <p className="kicker mb-1">// call detail</p>
          <h1 className="font-display text-2xl font-bold tracking-tight text-mist">
            {call.callerName}
          </h1>
          <p className="mt-1 font-mono text-sm text-dim">{call.callerNumber}</p>
        </div>
        <Badge label={sb.label} tone={sb.tone} />
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <div className="border border-line bg-panel/50 p-4">
          <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim">Agent</p>
          <p className="mt-1 font-display text-lg font-bold text-mist">
            {call.agentName ?? "—"}
            {call.agentType && (
              <span className="ml-2 font-mono text-[10px] uppercase tracking-[0.1em] text-dim">
                ({call.agentType})
              </span>
            )}
          </p>
        </div>
        <div className="border border-line bg-panel/50 p-4">
          <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim">Duration</p>
          <p className="mt-1 font-display text-lg font-bold tabular-nums text-mist">
            {call.duration ? fmtDuration(call.duration) : "—"}
          </p>
        </div>
        <div className="border border-line bg-panel/50 p-4">
          <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim">Wait time</p>
          <p className="mt-1 font-display text-lg font-bold tabular-nums text-mist">
            {call.waitTime}s
          </p>
        </div>
        <div className="border border-line bg-panel/50 p-4">
          <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim">CSAT</p>
          <p className="mt-1 font-display text-lg font-bold tabular-nums text-mist">
            {call.csat !== null ? `${call.csat} ★` : "—"}
          </p>
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <div className="border border-line bg-panel/50 p-4">
          <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim">Intent</p>
          <p className="mt-1">
            <Badge label={call.intent} tone={call.agentType === "human" ? "amber" : "mint"} />
          </p>
        </div>
        <div className="border border-line bg-panel/50 p-4">
          <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim">Confidence</p>
          <div className="mt-2">
            <ProgressBar value={call.confidence * 100} tone={call.confidence > 0.8 ? "mint" : call.confidence > 0.5 ? "amber" : "coral"} label="AI confidence" />
          </div>
        </div>
        <div className="border border-line bg-panel/50 p-4">
          <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim">Sentiment</p>
          <p className="mt-1 font-display text-lg font-bold text-mist">
            <span style={{ color: call.sentiment === "positive" ? "var(--color-mint)" : call.sentiment === "negative" ? "var(--color-coral)" : "var(--color-dim)" }}>
              {call.sentiment === "positive" ? "↑ Positive" : call.sentiment === "negative" ? "↓ Negative" : "— Neutral"}
            </span>
          </p>
        </div>
        <div className="border border-line bg-panel/50 p-4">
          <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim">Resolution</p>
          <p className="mt-1">
            <Badge
              label={
                call.resolution === "ai-resolved" ? "AI resolved"
                  : call.resolution === "escalated" ? "Escalated"
                  : call.resolution === "unresolved" ? "Unresolved"
                  : "In progress"
              }
              tone={
                call.resolution === "ai-resolved" ? "mint"
                  : call.resolution === "escalated" ? "amber"
                  : call.resolution === "unresolved" ? "coral"
                  : "dim"
              }
            />
          </p>
        </div>
      </div>

      <div className="border border-line">
        <div className="border-b border-line bg-panel/40 px-5 py-3">
          <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim">
            distribution details
          </p>
        </div>
        <div className="grid gap-0 divide-y divide-line/60 sm:grid-cols-2 sm:divide-y-0 sm:divide-x sm:divide-line/60">
          <div className="p-5 space-y-3">
            <div>
              <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim mb-1">Channel</p>
              <p className="text-sm text-mist">{call.channel}</p>
            </div>
            <div>
              <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim mb-1">Queue</p>
              <p className="text-sm text-mist capitalize">{call.queue.replace(/-/g, " ")}</p>
            </div>
            <div>
              <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim mb-1">Skill group</p>
              <p className="text-sm text-mist capitalize">{call.skillGroup.replace(/_/g, " ")}</p>
            </div>
            <div>
              <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim mb-1">Start time</p>
              <p className="text-sm text-mist">{new Date(call.startTime).toLocaleString()}</p>
            </div>
            {call.endTime && (
              <div>
                <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim mb-1">End time</p>
                <p className="text-sm text-mist">{new Date(call.endTime).toLocaleString()}</p>
              </div>
            )}
          </div>
          <div className="p-5 space-y-3">
            {call.escalationDelay !== null && (
              <div>
                <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim mb-1">Escalation delay</p>
                <p className="text-sm text-amber">{call.escalationDelay}s — warm transfer</p>
              </div>
            )}
            {call.apiActions.length > 0 && (
              <div>
                <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim mb-2">API actions</p>
                <div className="space-y-1.5">
                  {call.apiActions.map((a, i) => (
                    <div key={i} className="flex items-center gap-2">
                      <span className="font-mono text-[9px] text-mint tabular-nums">{a.timestamp}</span>
                      <span className="text-xs text-dim capitalize">{a.action.replace(/_/g, " ")}</span>
                    </div>
                  ))}
                </div>
              </div>
            )}
            {call.recordingUrl && (
              <div>
                <p className="font-mono text-[9px] uppercase tracking-[0.16em] text-dim mb-1">Recording</p>
                <p className="text-xs text-mint">Available — click to play</p>
              </div>
            )}
          </div>
        </div>
      </div>

      {call.transcript.length > 0 && (
        <div>
          <div className="mb-4">
            <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim mb-2">
              waveform
            </p>
            <div className="border border-line bg-panel/40 p-4">
              <div className="flex items-end gap-[2px] h-14">
                {Array.from({ length: 100 }).map((_, i) => {
                  const h = 0.2 + Math.sin(i * 0.4) * 0.3 + Math.sin(i * 0.7) * 0.25 + Math.sin(i * 0.15) * 0.2 + Math.random() * 0.1;
                  const clamped = Math.min(1, Math.max(0.15, h));
                  const isAgent = i < 15 || (i > 40 && i < 60) || i > 85;
                  return (
                    <div
                      key={i}
                      className="flex-1 wavebar"
                      style={{
                        height: `${clamped * 100}%`,
                        backgroundColor: isAgent ? "var(--color-mint)" : "var(--color-dim)",
                        opacity: 0.7,
                        animationDelay: `${i * 30}ms`,
                      }}
                    />
                  );
                })}
              </div>
              <div className="mt-2 flex justify-between font-mono text-[8px] uppercase tracking-[0.12em] text-dim/60">
                <span>00:00</span>
                <span style={{ color: "var(--color-mint)" }}>ai speaking</span>
                <span>{call.duration ? fmtDuration(call.duration) : "live"}</span>
              </div>
            </div>
          </div>

          <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim mb-3">
            transcript · {call.transcript.length} turns
          </p>
          <div className="border border-line divide-y divide-line/40">
            {call.transcript.map((t, i) => (
              <div key={i} className="flex gap-4 p-4">
                <div className="w-16 shrink-0">
                  <span className="font-mono text-[10px] tabular-nums text-dim">{t.timestamp}</span>
                </div>
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-2 mb-1">
                    <span
                      className="font-mono text-[10px] uppercase tracking-[0.12em]"
                      style={{
                        color:
                          t.role === "ai"
                            ? "var(--color-mint)"
                            : t.role === "human"
                              ? "var(--color-amber)"
                              : "var(--color-dim)",
                      }}
                    >
                      {t.role === "ai" ? call.agentName : t.role === "human" ? call.agentName : call.callerName}
                    </span>
                    <span className="font-mono text-[8px] text-dim/50">
                      {Math.round(t.confidence * 100)}% conf
                    </span>
                  </div>
                  <p className="text-sm leading-relaxed text-mist/90">{t.text}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}