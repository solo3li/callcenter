import { useState, useEffect, useMemo } from "react";
import StatCard from "../components/StatCard";
import CallRow from "../components/CallRow";
import Badge from "../components/Badge";
import { LIVEMETRICS, CALLS } from "../data";
import { useNavigate } from "react-router-dom";
import { statsApi } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

const SPARK_POINTS = "0,78 52,74 104,76 156,62 208,66 260,50 312,54 364,38 416,42 468,28 520,32 572,18 624,22 676,14 728,18 780,8";

export default function LiveBoard() {
  const navigate = useNavigate();
  const [now, setNow] = useState(new Date());
  const { data: todayStats } = useApi(
    () => statsApi.today(),
    []
  );

  const metrics = API_ENABLED && todayStats ? {
    activeCalls: todayStats.active,
    aiHandling: todayStats.total - todayStats.transferred - todayStats.missed,
    humanActive: todayStats.transferred,
    queued: todayStats.active,
    avgWait: 0.8,
    escalationRate: todayStats.total > 0 ? (todayStats.transferred / todayStats.total) * 100 : 0,
    callsToday: todayStats.total,
    autoResolveRate: todayStats.total > 0 ? ((todayStats.total - todayStats.transferred - todayStats.missed) / todayStats.total) * 100 : 94,
    csatScore: 4.7,
    avgInferenceMs: 182,
    callsPerHour: todayStats.hourly.map(h => h.count),
    callsPerHourYesterday: todayStats.hourly.map(h => h.count),
    callsPerDay: [todayStats.total],
    heatmapData: LIVEMETRICS.heatmapData,
  } : LIVEMETRICS;

  useEffect(() => {
    const id = setInterval(() => setNow(new Date()), 1000);
    return () => clearInterval(id);
  }, []);

  const liveCalls = useMemo(
    () => CALLS.filter((c) => c.status === "active-ai" || c.status === "active-human" || c.status === "queued"),
    []
  );

  return (
    <div className="space-y-8">
      <div>
        <p className="kicker mb-1">// live wallboard</p>
        <h1 className="font-display text-3xl font-bold tracking-tight text-mist">
          Floor overview — {now.toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit" })}
          {API_ENABLED && todayStats && (
            <span className="ml-3 inline-flex items-center gap-1.5 rounded-full border border-mint/30 bg-mint/10 px-2.5 py-0.5 text-[10px] font-mono font-normal uppercase tracking-[0.12em] text-mint">
              <span className="h-1.5 w-1.5 rounded-full bg-mint animate-pulse" />
              Live API
            </span>
          )}
        </h1>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
        <StatCard value={metrics.activeCalls} label="Active calls" pulse tone="mint" />
        <StatCard value={metrics.aiHandling} label="AI handling" tone="mint" sublabel={`${Math.round((metrics.aiHandling / metrics.activeCalls) * 100)}% of total`} />
        <StatCard value={metrics.humanActive} label="Human active" tone="amber" />
        <StatCard value={metrics.queued} label="Queued" tone={metrics.queued > 5 ? "coral" : "dim"} sublabel={metrics.queued > 0 ? `${metrics.avgWait}s avg wait` : undefined} />
        <StatCard value={metrics.escalationRate} decimals={1} suffix="%" label="Esc. rate" tone="amber" sublabel="AI → human" />
      </div>

      <div className="grid gap-6 lg:grid-cols-[1fr_0.55fr]">
        <div className="border border-line bg-panel/40 p-5">
          <div className="mb-4 flex items-center justify-between">
            <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim">
              call volume · last 24h
            </p>
            <Badge label="▲ 12% vs yesterday" tone="mint" />
          </div>
          <svg viewBox="0 0 780 100" className="h-24 w-full" preserveAspectRatio="none" aria-hidden="true">
            <defs>
              <linearGradient id="livefill" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="var(--color-mint)" stopOpacity="0.25" />
                <stop offset="100%" stopColor="var(--color-mint)" stopOpacity="0" />
              </linearGradient>
            </defs>
            <path d={`M${SPARK_POINTS.replace(/ /g, " L")} L780,100 L0,100 Z`} fill="url(#livefill)" />
            <path
              d={`M${SPARK_POINTS.replace(/ /g, " L")}`}
              fill="none"
              stroke="var(--color-mint)"
              strokeWidth="2"
              className="spark-draw"
            />
          </svg>
        </div>

        <div className="space-y-3">
          <div className="border border-line bg-panel/40 p-5">
            <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim mb-3">system health</p>
            <div className="space-y-2.5">
              {[
                { label: "LiveKit", ok: true },
                { label: "Asterisk trunk", ok: true },
                { label: "AI worker", ok: true, sub: "182ms p50" },
                { label: "DB latency", ok: true, sub: "4ms" },
                { label: "Redis", ok: true },
              ].map((s) => (
                <div key={s.label} className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className={`h-1.5 w-1.5 rounded-full ${s.ok ? "pulse-dot bg-mint" : "bg-coral"}`} />
                    <span className="font-mono text-[10px] text-dim">{s.label}</span>
                  </div>
                  <span className="font-mono text-[9px] uppercase tracking-[0.12em] text-mint">
                    {s.sub ?? "ok"}
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="border border-line bg-panel/40 p-5">
            <div className="flex items-center justify-between mb-3">
              <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim">queue health</p>
            </div>
            <div className="space-y-2">
              {["billing", "tech", "sales", "logistics", "escalation"].map((q, i) => (
                <div key={q} className="flex items-center justify-between">
                  <span className="font-mono text-[10px] capitalize text-dim">{q}</span>
                  <span className="font-mono text-[10px] tabular-nums text-mist">
                    {i === 3 ? "0 waiting" : `${i + 1} waiting · ${(0.2 + i * 0.15).toFixed(1)}s`}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      <div>
        <div className="mb-4 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim">
              live call feed · {liveCalls.length} active
            </p>
            <span className="flex items-center gap-1">
              <span className="h-1.5 w-1.5 rounded-full bg-mint" />
              <span className="font-mono text-[9px] text-mint">updating live</span>
            </span>
          </div>
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
              {liveCalls.map((c) => (
                <CallRow key={c.id} call={c} isLive onClick={() => navigate(`/dashboard/call/${c.id}`)} />
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}