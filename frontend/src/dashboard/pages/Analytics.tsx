import { useState } from "react";
import { LIVEMETRICS, TOP_INTENTS } from "../data";

type Range = "today" | "yesterday" | "7d" | "30d";

export default function Analytics() {
  const [range, setRange] = useState<Range>("today");
  const data = range === "today" ? LIVEMETRICS.callsPerHour : LIVEMETRICS.callsPerHourYesterday;
  const dayLabels = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
  const max = Math.max(...data, 1);
  const barMax = Math.max(...LIVEMETRICS.callsPerDay, 1);

  return (
    <div className="space-y-8">
      <div>
        <p className="kicker mb-1">// analytics</p>
        <h1 className="font-display text-3xl font-bold tracking-tight text-mist">
          Performance & trends
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
          <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim mb-4">
            hourly call volume — {range === "today" ? "today" : range === "yesterday" ? "yesterday" : range}
          </p>
          <div className="flex items-end gap-[3px] h-[200px]">
            {data.map((v, i) => (
              <div key={i} className="flex-1 flex flex-col items-center gap-1">
                <div
                  className="w-full numflip"
                  style={{
                    height: `${Math.max(2, (v / max) * 180)}px`,
                    backgroundColor:
                      range === "yesterday" ? "var(--color-dim)" : "var(--color-mint)",
                  }}
                />
                {i % 6 === 0 && (
                  <span className="font-mono text-[8px] text-dim/60">{i}h</span>
                )}
              </div>
            ))}
          </div>
        </div>

        <div className="border border-line bg-panel/40 p-5 flex items-center justify-center">
          <div className="flex items-center gap-10">
            <svg width="160" height="160" viewBox="0 0 160 160">
              <circle cx="80" cy="80" r="60" fill="none" stroke="var(--color-mint)" strokeWidth="28" strokeDasharray={`${0.94 * 2 * Math.PI * 60} ${0.06 * 2 * Math.PI * 60}`} strokeDashoffset={0} transform="rotate(-90 80 80)" />
              <circle cx="80" cy="80" r="60" fill="none" stroke="var(--color-amber)" strokeWidth="28" strokeDasharray={`${0.06 * 2 * Math.PI * 60} ${0.94 * 2 * Math.PI * 60}`} strokeDashoffset={`${-(0.94 * 2 * Math.PI * 60)}`} transform="rotate(-90 80 80)" opacity="0.8" />
            </svg>
            <div className="space-y-4">
              <div>
                <div className="flex items-center gap-2">
                  <span className="h-3 w-3" style={{ backgroundColor: "var(--color-mint)" }} />
                  <span className="font-mono text-xs text-mist">AI resolved</span>
                </div>
                <p className="font-display text-3xl font-bold text-mint mt-0.5">
                  94% <span className="text-base text-mint/60">2,676 calls</span>
                </p>
              </div>
              <div>
                <div className="flex items-center gap-2">
                  <span className="h-3 w-3" style={{ backgroundColor: "var(--color-amber)" }} />
                  <span className="font-mono text-xs text-mist">Escalated</span>
                </div>
                <p className="font-display text-3xl font-bold text-amber mt-0.5">
                  6% <span className="text-base text-amber/60">171 calls</span>
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <div className="border border-line bg-panel/40 p-5">
          <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim mb-4">
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
          <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim mb-4">
            top call intents — 7 days
          </p>
          <div className="space-y-3">
            {TOP_INTENTS.map((item) => (
              <div key={item.intent}>
                <div className="mb-1 flex justify-between font-mono text-[10px]">
                  <span className="text-dim">{item.intent}</span>
                  <span className="tabular-nums text-mist">{item.count}</span>
                </div>
                <div className="h-[4px] w-full bg-line">
                  <div
                    className="numflip h-full"
                    style={{
                      width: `${item.pct}%`,
                      backgroundColor:
                        item.intent === "Complaints"
                          ? "var(--color-coral)"
                          : item.intent === "Sales & upgrades"
                            ? "var(--color-amber)"
                            : "var(--color-mint)",
                    }}
                  />
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="border border-line bg-panel/40 p-5">
        <p className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim mb-4">
          daily call volume
        </p>
        <div className="flex items-end gap-4 h-[140px]">
          {LIVEMETRICS.callsPerDay.map((v, i) => (
            <div key={i} className="flex-1 flex flex-col items-center gap-1.5">
              <span className="font-mono text-[9px] tabular-nums text-dim">{v.toLocaleString()}</span>
              <div
                className="w-full numflip"
                style={{
                  height: `${Math.max(4, (v / barMax) * 120)}px`,
                  backgroundColor:
                    i === 6 ? "var(--color-mint)" : "var(--color-dim)",
                  opacity: i === 6 ? 1 : 0.5,
                }}
              />
              <span className="font-mono text-[8px] text-dim/60">{dayLabels[i]}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}