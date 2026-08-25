import { useState, useEffect } from "react";
import Badge from "./Badge";
import type { Call } from "../data";

const STATUS_BADGE: Record<string, { label: string; tone: "mint" | "amber" | "coral" | "dim" }> = {
  "active-ai": { label: "AI live", tone: "mint" },
  "active-human": { label: "Human live", tone: "amber" },
  queued: { label: "Queued", tone: "coral" },
  "completed-ai": { label: "AI resolved", tone: "mint" },
  "completed-escalated": { label: "Escalated", tone: "amber" },
  missed: { label: "Missed", tone: "coral" },
  abandoned: { label: "Abandoned", tone: "dim" },
};

export default function CallRow({
  call,
  isLive = false,
  onClick,
}: {
  call: Call;
  isLive?: boolean;
  onClick?: () => void;
}) {
  const b = STATUS_BADGE[call.status];
  const [elapsed, setElapsed] = useState(call.duration ?? 0);

  useEffect(() => {
    if (!isLive) return;
    const start = Date.now() - (call.duration ?? 0) * 1000;
    const id = setInterval(() => setElapsed(Math.floor((Date.now() - start) / 1000)), 1000);
    return () => clearInterval(id);
  }, [isLive, call.duration]);

  const fmt = (s: number) => {
    const m = Math.floor(s / 60);
    const sec = s % 60;
    return `${String(m).padStart(2, "0")}:${String(sec).padStart(2, "0")}`;
  };

  return (
    <tr
      onClick={onClick}
      className={`border-b border-line/50 transition-colors duration-200 ${
        onClick ? "cursor-pointer hover:bg-raised/60" : ""
      }`}
    >
      <td className="py-3 pr-4 font-mono text-xs text-mist">{call.callerName}</td>
      <td className="py-3 pr-4 font-mono text-xs text-dim">{call.callerNumber}</td>
      <td className="py-3 pr-4 font-mono text-xs tabular-nums text-mist">
        {isLive ? fmt(elapsed) : call.duration ? fmt(call.duration) : "—"}
      </td>
      <td className="py-3 pr-4">
        <span className="text-xs text-dim">{call.agentName ?? "—"}</span>
      </td>
      <td className="py-3 pr-4">
        <Badge label={call.intent} tone={call.agentType === "human" ? "amber" : "mint"} />
      </td>
      <td className="py-3">
        <Badge label={b.label} tone={b.tone} />
      </td>
    </tr>
  );
}