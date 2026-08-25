import { useMemo, useState } from "react";

type Mode = "ai" | "hybrid" | "human";

const MODES: { id: Mode; label: string; dot: string }[] = [
  { id: "ai", label: "AI only", dot: "var(--color-mint)" },
  { id: "hybrid", label: "Hybrid", dot: "var(--color-amber)" },
  { id: "human", label: "Human only", dot: "var(--color-dim)" },
];

function Slider(props: {
  label: string;
  value: number;
  min: number;
  max: number;
  step: number;
  format: (v: number) => string;
  onChange: (v: number) => void;
}) {
  return (
    <label className="block">
      <div className="mb-2 flex items-baseline justify-between gap-3">
        <span className="font-mono text-[10px] uppercase tracking-[0.2em] text-dim">{props.label}</span>
        <span className="font-display text-lg font-bold tabular-nums text-mist">{props.format(props.value)}</span>
      </div>
      <input
        type="range"
        min={props.min}
        max={props.max}
        step={props.step}
        value={props.value}
        onChange={(e) => props.onChange(Number(e.target.value))}
        className="w-full cursor-pointer"
        style={{ accentColor: "var(--color-mint)" }}
      />
    </label>
  );
}

const money = (n: number) =>
  n >= 1_000_000 ? `$${(n / 1_000_000).toFixed(2)}M` : `$${Math.round(n).toLocaleString("en-US")}`;

export default function Calculator() {
  const [mode, setMode] = useState<Mode>("hybrid");
  const [calls, setCalls] = useState(30000);
  const [aht, setAht] = useState(6);
  const [rate, setRate] = useState(27);
  const [complexity, setComplexity] = useState(18);

  const model = useMemo(() => {
    const c = complexity / 100;
    const hours = (n: number) => (n * aht) / 60;
    const humanOnly = {
      contained: 0,
      escalated: calls,
      cost: hours(calls) * rate,
      csat: 4.4,
      wait: "2m 40s",
    };
    const aiOnly = {
      contained: Math.round(calls * 0.82),
      escalated: Math.round(calls * 0.18),
      cost: hours(calls) * 0.55 * 3.4 + hours(calls * 0.18) * rate,
      csat: 4.1,
      wait: "0.8s",
    };
    const containedShare = 1 - c * 0.45;
    const hybrid = {
      contained: Math.round(calls * containedShare),
      escalated: Math.round(calls * (1 - containedShare)),
      cost:
        hours(calls * containedShare) * 0.55 * 3.4 + hours(calls * (1 - containedShare)) * rate,
      csat: 4.7,
      wait: "0.8s",
    };
    return { ai: aiOnly, hybrid, human: humanOnly };
  }, [calls, aht, rate, complexity]);

  const current = model[mode];
  const savings = model.human.cost - current.cost;
  const maxCost = Math.max(model.ai.cost, model.hybrid.cost, model.human.cost);

  return (
    <section id="floor" className="relative scroll-mt-24 border-y border-line bg-deep/50 px-5 py-24 lg:px-8 lg:py-32">
      <div className="mx-auto max-w-7xl">
        <div className="mb-14 max-w-2xl" data-reveal>
          <p className="kicker mb-4">// model your floor</p>
          <h2 className="font-display text-[clamp(2rem,4.5vw,3.4rem)] font-bold leading-[1.05] tracking-tight text-mist">
            Do the math before your CFO does.
          </h2>
          <p className="mt-5 text-base leading-relaxed text-dim">
            Drag your real numbers in. The model splits containment, escalations and monthly cost
            across three staffing strategies — live.
          </p>
        </div>

        <div className="grid gap-8 lg:grid-cols-[0.9fr_1.1fr]" data-reveal style={{ "--d": "150ms" } as React.CSSProperties}>
          {/* inputs */}
          <div className="border border-line bg-panel/70 p-7">
            <div className="mb-7 flex gap-2">
              {MODES.map((m) => (
                <button
                  key={m.id}
                  onClick={() => setMode(m.id)}
                  className={`flex flex-1 items-center justify-center gap-2 border px-3 py-2.5 font-mono text-[10px] uppercase tracking-[0.16em] transition-all duration-300 ${
                    mode === m.id
                      ? "border-mint/70 bg-raised text-mist"
                      : "border-line text-dim hover:border-mint/40 hover:text-mist"
                  }`}
                >
                  <span className="h-1.5 w-1.5 rounded-full" style={{ backgroundColor: m.dot }} />
                  {m.label}
                </button>
              ))}
            </div>
            <div className="space-y-7">
              <Slider label="Monthly calls" value={calls} min={2000} max={120000} step={1000} format={(v) => v.toLocaleString("en-US")} onChange={setCalls} />
              <Slider label="Avg handle time" value={aht} min={2} max={14} step={1} format={(v) => `${v} min`} onChange={setAht} />
              <Slider label="Loaded agent rate" value={rate} min={16} max={48} step={1} format={(v) => `$${v}/hr`} onChange={setRate} />
              <Slider label="Complex-call share" value={complexity} min={5} max={40} step={1} format={(v) => `${v}%`} onChange={setComplexity} />
            </div>
            <p className="mt-7 border-t border-line pt-4 font-mono text-[10px] leading-relaxed tracking-[0.08em] text-dim">
              ASSUMES AI HANDLE TIME ≈ 55% OF HUMAN · $3.40/HR EQUIVALENT INFERENCE · YOUR MILEAGE, AUDITED IN PILOT.
            </p>
          </div>

          {/* outputs */}
          <div className="flex flex-col gap-5">
            <div className="grid grid-cols-2 gap-5">
              <div className="border border-mint/30 bg-panel/70 p-6">
                <p className="font-mono text-[10px] uppercase tracking-[0.2em] text-mint">ai resolves</p>
                <p className="mt-2 font-display text-4xl font-bold tabular-nums text-mist">
                  {current.contained.toLocaleString("en-US")}
                </p>
                <p className="mt-1 text-xs text-dim">calls / month, no human touch</p>
              </div>
              <div className="border border-amber/30 bg-panel/70 p-6">
                <p className="font-mono text-[10px] uppercase tracking-[0.2em] text-amber">escalated warm</p>
                <p className="mt-2 font-display text-4xl font-bold tabular-nums text-mist">
                  {current.escalated.toLocaleString("en-US")}
                </p>
                <p className="mt-1 text-xs text-dim">with full context, before hello</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-5">
              <div className="border border-line bg-panel/70 p-6">
                <p className="font-mono text-[10px] uppercase tracking-[0.2em] text-dim">monthly cost</p>
                <p className="mt-2 font-display text-4xl font-bold tabular-nums text-mist">{money(current.cost)}</p>
                <p className="mt-1 text-xs text-dim">
                  csat <span className="text-mint">{current.csat.toFixed(1)}★</span> · pickup{" "}
                  <span className="text-mist">{current.wait}</span>
                </p>
              </div>
              <div className={`border p-6 ${savings > 0 ? "border-mint/40 bg-mint/[0.06]" : "border-line bg-panel/70"}`}>
                <p className="font-mono text-[10px] uppercase tracking-[0.2em] text-dim">vs all-human</p>
                <p className={`mt-2 font-display text-4xl font-bold tabular-nums ${savings > 0 ? "text-mint" : "text-coral"}`}>
                  {savings > 0 ? `−${money(savings)}` : `+${money(-savings)}`}
                </p>
                <p className="mt-1 text-xs text-dim">
                  {savings > 0 ? `saved every month` : "extra spend every month"}
                </p>
              </div>
            </div>

            {/* comparison bars */}
            <div className="border border-line bg-panel/70 p-6">
              <p className="mb-5 font-mono text-[10px] uppercase tracking-[0.2em] text-dim">
                monthly cost by strategy
              </p>
              <div className="space-y-4">
                {MODES.map((m) => {
                  const v = model[m.id];
                  const pct = Math.max(4, (v.cost / maxCost) * 100);
                  const isSel = mode === m.id;
                  return (
                    <button key={m.id} onClick={() => setMode(m.id)} className="group block w-full text-left">
                      <div className="mb-1.5 flex items-baseline justify-between font-mono text-[10px] uppercase tracking-[0.16em]">
                        <span className={isSel ? "text-mist" : "text-dim"}>{m.label}</span>
                        <span className={`tabular-nums ${isSel ? "text-mist" : "text-dim"}`}>{money(v.cost)}</span>
                      </div>
                      <div className="h-2.5 w-full bg-faint">
                        <div
                          className="numflip h-full"
                          style={{
                            width: `${pct}%`,
                            backgroundColor: isSel
                              ? m.id === "human"
                                ? "var(--color-dim)"
                                : m.dot
                              : "var(--color-line)",
                          }}
                        />
                      </div>
                    </button>
                  );
                })}
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
