import { useState, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import CallRow from "../components/CallRow";
import { CALLS, type Call } from "../data";
import { callsApi, CallSession } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

export default function CallHistory() {
  const navigate = useNavigate();
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [dateFilter, setDateFilter] = useState<string>("all");
  const [page, setPage] = useState(0);
  const PER_PAGE = 10;

  const { data: apiCalls } = useApi(
    () => callsApi.list({ limit: 100 }),
    []
  );

  const allCalls = API_ENABLED && apiCalls?.items ? apiCalls.items.map((s: CallSession) => ({
    id: s.id,
    callerName: "Caller",
    callerNumber: s.livekitRoomName,
    status: s.status as Call["status"],
    duration: s.durationSeconds,
    agentId: null,
    agentName: null,
    agentType: null as "ai" | "human" | null,
    intent: "",
    confidence: 0,
    sentiment: "neutral" as "positive" | "neutral" | "negative",
    transcript: [],
    startTime: s.startedAt,
    endTime: s.endedAt ?? null,
    waitTime: 0,
    resolution: null as "ai-resolved" | "escalated" | "unresolved" | null,
    csat: null,
    channel: "",
    queue: "",
    skillGroup: "",
    escalationDelay: null,
    apiActions: [],
    recordingUrl: null,
  })) : CALLS;

  const filtered = useMemo(() => {
    let result = allCalls;
    if (search) {
      const q = search.toLowerCase();
      result = result.filter(
        (c) =>
          c.callerName.toLowerCase().includes(q) ||
          c.callerNumber.includes(q) ||
          (c.agentName ?? "").toLowerCase().includes(q) ||
          c.intent.toLowerCase().includes(q)
      );
    }
    if (statusFilter !== "all") {
      result = result.filter((c) => c.status === statusFilter);
    }
    if (dateFilter === "today") {
      const cutoff = Date.now() - 86400000;
      result = result.filter((c) => new Date(c.startTime).getTime() > cutoff);
    } else if (dateFilter === "week") {
      const cutoff = Date.now() - 7 * 86400000;
      result = result.filter((c) => new Date(c.startTime).getTime() > cutoff);
    }
    return result;
  }, [search, statusFilter, dateFilter, allCalls]);

  const totalPages = Math.ceil(filtered.length / PER_PAGE);
  const paged = filtered.slice(page * PER_PAGE, (page + 1) * PER_PAGE);

  const STATUSES = [
    { value: "all", label: "All" },
    { value: "active-ai", label: "AI Live" },
    { value: "active-human", label: "Human Live" },
    { value: "queued", label: "Queued" },
    { value: "completed-ai", label: "AI Resolved" },
    { value: "completed-escalated", label: "Escalated" },
    { value: "missed", label: "Missed" },
    { value: "abandoned", label: "Abandoned" },
  ];

  const DATES = [
    { value: "all", label: "All time" },
    { value: "today", label: "Today" },
    { value: "week", label: "This week" },
  ];

  return (
    <div className="space-y-6">
      <div>
        <p className="kicker mb-1">// call history</p>
        <h1 className="font-display text-3xl font-bold tracking-tight text-mist">
          Call log
          <span className="ml-3 text-base font-normal text-dim">
            {filtered.length} calls
          </span>
          {API_ENABLED && apiCalls && (
            <span className="ml-3 inline-flex items-center gap-1.5 rounded-full border border-mint/30 bg-mint/10 px-2.5 py-0.5 text-[10px] font-mono font-normal uppercase tracking-[0.12em] text-mint">
              <span className="h-1.5 w-1.5 rounded-full bg-mint animate-pulse" />
              Live API
            </span>
          )}
        </h1>
      </div>

      <div className="flex flex-wrap gap-3">
        <input
          type="text"
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(0); }}
          placeholder="Search caller, number, agent..."
          className="min-w-[260px] border border-line bg-deep px-4 py-2.5 font-mono text-xs text-mist placeholder:text-dim/60 focus:border-mint focus:outline-none"
        />
        <select
          value={statusFilter}
          onChange={(e) => { setStatusFilter(e.target.value); setPage(0); }}
          className="border border-line bg-deep px-3 py-2.5 font-mono text-[10px] uppercase tracking-[0.12em] text-mist focus:border-mint focus:outline-none"
        >
          {STATUSES.map((s) => (
            <option key={s.value} value={s.value}>{s.label}</option>
          ))}
        </select>
        <select
          value={dateFilter}
          onChange={(e) => { setDateFilter(e.target.value); setPage(0); }}
          className="border border-line bg-deep px-3 py-2.5 font-mono text-[10px] uppercase tracking-[0.12em] text-mist focus:border-mint focus:outline-none"
        >
          {DATES.map((d) => (
            <option key={d.value} value={d.value}>{d.label}</option>
          ))}
        </select>
      </div>

      <div className="border border-line">
        <table className="w-full">
          <thead>
            <tr className="border-b border-line bg-panel/40">
              <th className="py-3 pr-4 text-left font-mono text-[10px] uppercase tracking-[0.16em] text-dim pl-5">Caller</th>
              <th className="py-3 pr-4 text-left font-mono text-[10px] uppercase tracking-[0.16em] text-dim">Number</th>
              <th className="py-3 pr-4 text-left font-mono text-[10px] uppercase tracking-[0.16em] text-dim">Duration</th>
              <th className="py-3 pr-4 text-left font-mono text-[10px] uppercase tracking-[0.16em] text-dim">Agent</th>
              <th className="py-3 pr-4 text-left font-mono text-[10px] uppercase tracking-[0.16em] text-dim">Intent</th>
              <th className="py-3 text-left font-mono text-[10px] uppercase tracking-[0.16em] text-dim pr-5">Status</th>
            </tr>
          </thead>
          <tbody>
            {paged.map((c) => (
              <CallRow
                key={c.id}
                call={c}
                isLive={c.status === "active-ai" || c.status === "active-human" || c.status === "queued"}
                onClick={() => navigate(`/dashboard/call/${c.id}`)}
              />
            ))}
          </tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <span className="font-mono text-[10px] uppercase tracking-[0.14em] text-dim">
            page {page + 1} of {totalPages}
          </span>
          <div className="flex gap-2">
            <button
              onClick={() => setPage((p) => Math.max(0, p - 1))}
              disabled={page === 0}
              className="btn btn-ghost !px-3 !py-2 !text-[10px] disabled:opacity-30"
            >
              Previous
            </button>
            <button
              onClick={() => setPage((p) => Math.min(totalPages - 1, p + 1))}
              disabled={page >= totalPages - 1}
              className="btn btn-ghost !px-3 !py-2 !text-[10px] disabled:opacity-30"
            >
              Next
            </button>
          </div>
        </div>
      )}
    </div>
  );
}