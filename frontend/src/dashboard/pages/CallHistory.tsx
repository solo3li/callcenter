import { useState, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import CallRow from "../components/CallRow";
import { CALLS, type Call } from "../data";
import { callsApi } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";
import { sessionToUiCall } from "../statusMap";
import type { ApiCallStatus } from "../../api/endpoints";

const API_ENABLED = !!import.meta.env.VITE_API_URL;
const PER_PAGE = 10;

const STATUS_TO_API: Record<string, ApiCallStatus | undefined> = {
  all: undefined,
  queued: "Queued",
  "active-ai": "Active",
  "active-human": "Transferred",
  "completed-ai": "Completed",
  missed: "Missed",
  abandoned: "Failed",
};

function dateCutoff(filter: string): string | undefined {
  if (filter === "today") return new Date(Date.now() - 86400000).toISOString();
  if (filter === "week") return new Date(Date.now() - 7 * 86400000).toISOString();
  return undefined;
}

export default function CallHistory() {
  const navigate = useNavigate();
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [dateFilter, setDateFilter] = useState<string>("all");
  const [page, setPage] = useState(0);

  const apiStatus = STATUS_TO_API[statusFilter];
  const from = dateCutoff(dateFilter);

  const { data: pageData, error: apiError, loading } = useApi(
    () =>
      API_ENABLED
        ? callsApi.list({ status: apiStatus, from, page: page + 1, limit: PER_PAGE })
        : Promise.resolve(null),
    [page, statusFilter, dateFilter]
  );

  const rows: Call[] = useMemo(() => {
    if (!API_ENABLED) return CALLS;
    return (pageData?.items ?? []).map(sessionToUiCall);
  }, [pageData]);

  const filtered = useMemo(() => {
    if (!search) return rows;
    const q = search.toLowerCase();
    return rows.filter(
      (c) =>
        c.callerName.toLowerCase().includes(q) ||
        c.callerNumber.includes(q) ||
        (c.agentName ?? "").toLowerCase().includes(q)
    );
  }, [search, rows]);

  // Server-side totals when live; client-side over mock set otherwise.
  const totalCount = API_ENABLED ? (pageData?.totalCount ?? filtered.length) : filtered.length;
  const totalPages =
    API_ENABLED && !search
      ? Math.max(1, Math.ceil(totalCount / PER_PAGE))
      : Math.max(1, Math.ceil(filtered.length / PER_PAGE));
  const paged = API_ENABLED && !search ? filtered : filtered.slice(page * PER_PAGE, (page + 1) * PER_PAGE);

  const STATUSES = [
    { value: "all", label: "All" },
    { value: "active-ai", label: "AI Live" },
    { value: "active-human", label: "Human Live" },
    { value: "queued", label: "Queued" },
    { value: "completed-ai", label: "Completed" },
    { value: "missed", label: "Missed" },
    { value: "abandoned", label: "Failed" },
  ];

  const DATES = [
    { value: "all", label: "All time" },
    { value: "today", label: "Last 24h" },
    { value: "week", label: "This week" },
  ];

  return (
    <div className="space-y-6">
      <div>
        <p className="kicker mb-1">// call history</p>
        <h1 className="font-display text-3xl font-bold tracking-tight text-mist">
          Call log
          <span className="ml-3 text-base font-normal text-dim">
            {totalCount.toLocaleString()} calls
          </span>
          {API_ENABLED && pageData && (
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
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search this page (room, number, agent)..."
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
        {API_ENABLED && apiError ? (
          <p className="px-5 py-8 text-center font-mono text-[11px] text-coral">{apiError}</p>
        ) : loading && API_ENABLED ? (
          <p className="px-5 py-8 text-center font-mono text-[11px] text-dim">loading calls…</p>
        ) : paged.length === 0 ? (
          <p className="px-5 py-8 text-center font-mono text-[11px] text-dim">no calls match your filters</p>
        ) : (
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
        )}
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <span className="font-mono text-[10px] uppercase tracking-[0.14em] text-dim">
            page {page + 1} of {totalPages} · {totalCount.toLocaleString()} total
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
              disabled={page >= totalPages - 1 || loading}
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
