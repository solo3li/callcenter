import { useState, useEffect, useMemo } from "react";
import StatCard from "../components/StatCard";
import CallRow from "../components/CallRow";
import Badge from "../components/Badge";
import { LIVEMETRICS } from "../data";
import { useNavigate } from "react-router-dom";
import { statsApi, callsApi } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";
import { useLiveHub } from "../../hooks/useLiveHub";
import { activeCallToUiCall } from "../statusMap";
import type { Call } from "../data";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

export default function LiveBoard() {
  const navigate = useNavigate();
  const [now, setNow] = useState(new Date());

  const { connected, queue, tick } = useLiveHub(API_ENABLED);

  const { data: todayStats } = useApi(
    () => statsApi.today(),
    [tick]
  );

  const { data: apiActiveCalls, error: activeErr } = useApi(
    () => callsApi.active(),
    [tick]
  );

  const metrics = API_ENABLED && todayStats ? {
    activeCalls: queue?.activeCount ?? todayStats.activeCalls,
    aiHandling: Math.max(0, todayStats.answeredCalls - todayStats.transferredCalls),
    humanActive: todayStats.transferredCalls,
    queued: Math.max(0, todayStats.activeCalls - todayStats.answeredCalls),
    avgWait: todayStats.avgDurationSeconds > 0 ? Math.round(todayStats.avgDurationSeconds * 0.1 * 10) / 10 : 0,
    escalationRate: todayStats.totalCalls > 0 ? (todayStats.transferredCalls / todayStats.totalCalls) * 100 : 0,
    callsToday: todayStats.totalCalls,
    autoResolveRate: todayStats.totalCalls > 0
      ? ((todayStats.totalCalls - todayStats.transferredCalls - todayStats.missedCalls) / todayStats.totalCalls) * 100
      : 0,
    csatScore: 4.7,
    avgInferenceMs: 182,
    callsPerHour: todayStats.hourly.map(h => h.count),
    callsPerHourYesterday: todayStats.hourly.map(h => h.count),
    callsPerDay: [todayStats.totalCalls],
    heatmapData: LIVEMETRICS.heatmapData,
  } : LIVEMETRICS;

  useEffect(() => {
    const id = setInterval(() => setNow(new Date()), 1000);
    return () => clearInterval(id);
  }, []);

  const sparkPoints = useMemo(() => {
    const data = metrics.callsPerHour;
    if (!data.length) return null;
    const max = Math.max(...data, 1);
    const step = 780 / Math.max(1, data.length - 1);
    return data
      .map((v, i) => `${Math.round(i * step)},${Math.round(96 - (v / max) * 88)}`)
      .join(" ");
  }, [metrics.callsPerHour]);

  const liveCalls: Call[] = useMemo(() => {
    if (!API_ENABLED || !apiActiveCalls) return [];
    return apiActiveCalls.map(activeCallToUiCall);
  }, [apiActiveCalls]);

  return (
    <div className="space-y-8">
      <div>
        <p className="kicker mb-1">// live wallboard</p>
        <h1 className="font-display text-3xl font-bold tracking-tight text-mist">
          Floor overview — {now.toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit" })}
          {API_ENABLED && todayStats && (
            <span className={`ml-3 inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-[10px] font-mono font-normal uppercase tracking-[0.12em] ${connected ? "border-mint/30 bg-mint/10 text-mint" : "border-line bg-panel/40 text-dim"}`}>
              <span className={`h-1.5 w-1.5 rounded-full ${connected ? "bg-mint animate-pulse" : "bg-dim"}`} />
              {connected ? "Live API" : "API polling"}
            </span>
          )}
        </h1>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
        <StatCard value={metrics.activeCalls} label="Active calls" pulse tone="mint" />
        <StatCard value={metrics.humanActive} label="Human active" tone="amber" />
        <StatCard value={metrics.queued} label="Queued" tone={metrics.queued > 5 ? "coral" : "dim"} sublabel={`${metrics.avgWait}s avg wait`} />
        <StatCard value={metrics.callsToday} label="Calls today" tone="dim" />
        <StatCard value={metrics.escalationRate} decimals={1} suffix="%" label="Esc. rate" tone="amber" sublabel="AI → human" />
      </div>

      <div className="grid gap-6 lg:grid-cols-[1fr_0.55fr]">
        <div className="border border-line bg-panel/40 p-5">
          <div className="mb-4 flex items-center justify-between">
            <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim">
              call volume · last 24h
            </p>
            {API_ENABLED && todayStats && (
              <Badge label={`${metrics.autoResolveRate.toFixed(0)}% auto-resolved`} tone="mint" />
            )}
          </div>
          {sparkPoints ? (
            <svg viewBox="0 0 780 100" className="h-24 w-full" preserveAspectRatio="none" aria-hidden="true">
              <defs>
                <linearGradient id="livefill" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="var(--color-mint)" stopOpacity="0.25" />
                  <stop offset="100%" stopColor="var(--color-mint)" stopOpacity="0" />
                </linearGradient>
              </defs>
              <path d={`M${sparkPoints.replace(/ /g, " L")} L780,100 L0,100 Z`} fill="url(#livefill)" />
              <path
                d={`M${sparkPoints.replace(/ /g, " L")}`}
                fill="none"
                stroke="var(--color-mint)"
                strokeWidth="2"
                className="spark-draw"
              />
            </svg>
          ) : (
            <div className="flex h-24 items-center justify-center font-mono text-[10px] text-dim">no data yet</div>
          )}
        </div>

        <div className="space-y-3">
          <div className="border border-line bg-panel/40 p-5">
            <div className="mb-3 flex items-center justify-between">
              <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim">agents online</p>
            </div>
            <div className="space-y-2">
              {(queue?.agents ?? []).length > 0 ? (
                queue!.agents.map((a) => (
                  <div key={a.id} className="flex items-center justify-between">
                    <span className="font-mono text-[10px] text-dim">{a.name}</span>
                    <span className={`font-mono text-[9px] uppercase tracking-[0.12em] ${a.status === "Available" ? "text-mint" : "text-amber"}`}>
                      {a.status}
                    </span>
                  </div>
                ))
              ) : (
                <>
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
                </>
              )}
            </div>
          </div>

          <div className="border border-line bg-panel/40 p-5">
            <div className="flex items-center justify-between mb-3">
              <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim">agents online count</p>
            </div>
            <p className="font-display text-3xl font-bold tabular-nums text-mist">
              {queue?.agentsOnline ?? todayStats?.agentsOnline ?? 0}
            </p>
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
              <span className={`h-1.5 w-1.5 rounded-full ${connected ? "bg-mint animate-pulse" : "bg-amber"}`} />
              <span className="font-mono text-[9px] text-mint">
                {connected ? "updating live" : "polling"}
              </span>
            </span>
          </div>
        </div>
        <div className="border border-line">
          {API_ENABLED && activeErr ? (
            <p className="px-5 py-8 text-center font-mono text-[11px] text-coral">{activeErr}</p>
          ) : liveCalls.length === 0 ? (
            <p className="px-5 py-8 text-center font-mono text-[11px] text-dim">no active calls right now</p>
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
                {liveCalls.map((c) => (
                  <CallRow key={c.id} call={c} isLive onClick={() => navigate(`/dashboard/call/${c.id}`)} />
                ))}
              </tbody>
            </table>
          )}
        </div>
        {!API_ENABLED && (
          <p className="px-5 py-4 font-mono text-[10px] text-dim">
            set VITE_API_URL to stream real calls
          </p>
        )}
      </div>
    </div>
  );
}
