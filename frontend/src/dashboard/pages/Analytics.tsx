import { useState, useMemo } from "react";
import { LIVEMETRICS } from "../data";
import { statsApi } from "../../api/endpoints";
import type { HourlyDataPoint, IntentStats } from "../../api/endpoints";
import { useApi } from "../../hooks/useApi";

const API_ENABLED = !!import.meta.env.VITE_API_URL;

type Range = "today" | "yesterday" | "7d" | "30d";

function isoDate(d: Date): string {
  return d.toISOString().slice(0, 10);
}

export default function Analytics() {
  const [range, setRange] = useState<Range>("today");

  const hourlyDeps = useMemo(() => [range], [range]);
  const yesterday = useMemo(() => {
    const d = new Date();
    d.setDate(d.getDate() - 1);
    return isoDate(d);
  }, []);
  const periodFrom = useMemo(() => {
    const d = new Date();
    if (range === "7d") d.setDate(d.getDate() - 7);
    else if (range === "30d") d.setDate(d.getDate() - 30);
    return isoDate(d);
  }, [range]);
  const periodTo = isoDate(new Date());

  const isHourly = range === "today" || range === "yesterday";
  const { data: apiHourly, error: hourlyErr } = useApi(
    () => (isHourly ? statsApi.hourly(range === "yesterday" ? yesterday : undefined) : Promise.resolve(null)),
    hourlyDeps
  );
  const { data: apiPeriod } = useApi(
    () => (!isHourly ? statsApi.period(`${periodFrom}T00:00:00Z`, `${periodTo}T23:59:59Z`) : Promise.resolve(null)),
    hourlyDeps
  );
  const { data: apiIntents } = useApi(
    () => (API_ENABLED ? statsApi.intents() : Promise.resolve(null)),
    []
  );

  const hourlyPoints: HourlyDataPoint[] =
    API_ENABLED && isHourly && apiHourly
      ? apiHourly
      : API_ENABLED && !isHourly && apiPeriod
        ? apiPeriod.hourly
        : LIVEMETRICS.callsPerHour.map((count, i) => ({ hour: String(i), count }));

  const intents: IntentStats[] =
    API_ENABLED && apiIntents && apiIntents.length > 0
      ? apiIntents
      : [];

  const totalCalls = hourlyPoints.reduce((s, p) => s + p.count, 0);
  const max = Math.max(...hourlyPoints.map((p) => p.count), 1);
  const dayLabels = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

  const intentMax = Math.max(...intents.map((i) => i.count), 1);
  const intentTotal = intents.reduce((s, i) => s + i.count, 0);

  return (
    <div className="space-y-8">
      <div>
        <p className="kicker mb-1">// analytics</p>
        <h1 className="font-display text-3xl font-bold tracking-tight text-mist">
          Performance & trends
          {API_ENABLED && (
            <span className="ml-3 inline-flex items-center gap-1.5 rounded-full border border-mint/30 bg-mint/10 px-2.5 py-0.5 text-[10px] font-mono font-normal uppercase tracking-[0.12em] text-mint">
              <span className="h-1.5 w-1.5 rounded-full bg-mint animate-pulse" />
              Live API
            </span>
          )}
        </h1>
      </div>

      <div className="flex flex-wrap gap-2">
        {(["today", "yesterday", "7d", "30d"] as Range[]).map((r) => (
          <button
            key={r}
            onClick={() => setRange(r)}
            className={`btn !px-4 !py-2 !text-[10px] !tracking-[0.12em] ${
              range === r ? "btn-mint" : "btn-ghost"
            }`}
          >
            {r === "today" ? "Today" : r === "yesterday" ? "Yesterday" : r === "7d" ? "Last 7 days" : "Last 30 days"}
          </button>
        ))}
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <div className="border border-line bg-panel/40 p-5">
          <div className="mb-4 flex items-baseline justify-between gap-3">
            <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim">
              call volume — {range === "today" ? "today" : range === "yesterday" ? "yesterday" : range}
            </p>
            <span className="font-mono text-[10px] tabular-nums text-mist">{totalCalls.toLocaleString()} calls</span>
          </div>
          {API_ENABLED && hourlyErr ? (
            <p className="py-8 text-center font-mono text-[11px] text-coral">{hourlyErr}</p>
          ) : (
            <>
              <div className="flex items-end gap-[3px] h-[200px]">
                {hourlyPoints.map((p, i) => (
                  <div key={i} className="flex-1 flex flex-col items-center gap-1" title={`${p.hour}: ${p.count}`}>
                    <div
                      className="w-full numflip"
                      style={{
                        height: `${Math.max(2, (p.count / max) * 180)}px`,
                        backgroundColor:
                          range === "yesterday" ? "var(--color-dim)" : "var(--color-mint)",
                      }}
                    />
                    {i % Math.ceil(hourlyPoints.length / 6 || 1) === 0 && (
                      <span className="font-mono text-[8px] text-dim/60">{p.hour}</span>
                    )}
                  </div>
                ))}
              </div>
              {totalCalls === 0 && (
                <p className="mt-3 text-center font-mono text-[10px] text-dim">no calls in this range</p>
              )}
            </>
          )}
        </div>

        <div className="border border-line bg-panel/40 p-5">
          <p className="mb-4 font-mono text-[10px] uppercase tracking-[0.18em] text-dim">
            top call intents
          </p>
          {intents.length === 0 ? (
            <p className="py-8 text-center font-mono text-[11px] text-dim">no intent data yet</p>
          ) : (
            <div className="space-y-3">
              {intents.map((item) => (
                <div key={item.intent}>
                  <div className="mb-1 flex justify-between font-mono text-[10px]">
                    <span className="text-dim">{item.intent}</span>
                    <span className="tabular-nums text-mist">{item.count}</span>
                  </div>
                  <div className="h-[4px] w-full bg-line">
                    <div
                      className="numflip h-full"
                      style={{
                        width: `${Math.max(2, (item.count / intentMax) * 100)}%`,
                        backgroundColor: "var(--color-mint)",
                      }}
                    />
                  </div>
                  <p className="mt-0.5 text-right font-mono text-[9px] text-dim/60">
                    {item.percentage ?? (intentTotal > 0 ? ((item.count / intentTotal) * 100).toFixed(1) : 0)}%
                  </p>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      <div className="border border-line bg-panel/40 p-5">
        <p className="mb-4 font-mono text-[10px] uppercase tracking-[0.18em] text-dim">
          weekly volume heatmap · hours × days
        </p>
        <div>
          <div className="flex mb-1">
            <span className="w-10" />
            {["0h", "6h", "12h", "18h"].map((l) => (
              <span key={l} className="flex-1 text-center font-mono text-[8px] text-dim/60">{l}</span>
            ))}
          </div>
          {LIVEMETRICS.heatmapData.map((row, d) => (
            <div key={d} className="flex items-center gap-1 mb-[1px]">
              <span className="w-10 font-mono text-[9px] text-dim/60">{dayLabels[d]}</span>
              {row.map((v, h) => (
                <div
                  key={h}
                  className="flex-1 h-[14px]"
                  style={{
                    backgroundColor: `rgba(86,224,191,${v * 0.7})`,
                    opacity: v > 0.05 ? 1 : 0.3,
                  }}
                />
              ))}
            </div>
          ))}
        </div>
      </div>

      <div className="border border-line bg-panel/40 p-5">
        <p className="mb-4 font-mono text-[10px] uppercase tracking-[0.18em] text-dim">
          daily call volume · last 7 days
        </p>
        <DailyBars from={isoDate(new Date(Date.now() - 6 * 86400000))} to={periodTo} />
      </div>
    </div>
  );
}

function DailyBars({ from, to }: { from: string; to: string }) {
  const { data: week } = useApi(
    () => (API_ENABLED ? statsApi.period(`${from}T00:00:00Z`, `${to}T23:59:59Z`) : Promise.resolve(null)),
    [from, to]
  );

  const points = API_ENABLED && week ? week.hourly : [];
  const byDay = new Map<string, number>();
  for (let i = 6; i >= 0; i--) {
    const d = new Date(Date.now() - i * 86400000);
    byDay.set(d.toISOString().slice(0, 10), 0);
  }
  for (const p of points) {
    const day = p.hour.slice(0, 10);
    if (byDay.has(day)) byDay.set(day, (byDay.get(day) ?? 0) + p.count);
  }
  const days = Array.from(byDay.entries());
  const barMax = Math.max(...days.map(([, v]) => v), 1);
  const labels = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

  return (
    <div className="flex items-end gap-4 h-[140px]">
      {days.map(([day, v], i) => (
        <div key={day} className="flex-1 flex flex-col items-center gap-1.5" title={`${day}: ${v}`}>
          <span className="font-mono text-[9px] tabular-nums text-dim">{v.toLocaleString()}</span>
          <div
            className="w-full numflip"
            style={{
              height: `${Math.max(4, (v / barMax) * 120)}px`,
              backgroundColor: i === days.length - 1 ? "var(--color-mint)" : "var(--color-dim)",
              opacity: i === days.length - 1 ? 1 : 0.5,
            }}
          />
          <span className="font-mono text-[8px] text-dim/60">
            {labels[new Date(day + "T12:00:00Z").getUTCDay()]}
          </span>
        </div>
      ))}
    </div>
  );
}
