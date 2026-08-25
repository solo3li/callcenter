import { useEffect, useMemo, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { usePRM, useScramble, useJitter } from "../hooks";

/* ---------------------------------- script ---------------------------------- */

type Line = {
  who: "sys" | "cust" | "ai" | "human";
  name?: string;
  text: string;
  conf?: number;
};

const SCRIPT: Line[] = [
  { who: "sys", text: "inbound · +1 (415) 555-0132 · intent: billing" },
  { who: "cust", name: "Dana", text: "Hi — I was charged twice for March. Can you fix it?" },
  {
    who: "ai",
    name: "Noor · AI",
    conf: 96,
    text: "Of course, Dana. I see two $49.00 charges on Mar 3 — the second is a duplicate. I've queued a refund to your Visa •• 4421.",
  },
  { who: "sys", text: "refund issued · $49.00 · eta 3–5 business days" },
  {
    who: "cust",
    name: "Dana",
    text: "Thanks… honestly the outage last weekend is what's frustrating me.",
  },
  {
    who: "ai",
    name: "Noor · AI",
    conf: 61,
    text: "That's completely fair, and I hear you. This one deserves a person — bringing in Marco with your full history.",
  },
  { who: "sys", text: "⚡ warm handoff · transcript + sentiment synced in 0.4s" },
  {
    who: "human",
    name: "Marco · Human",
    conf: 100,
    text: "Hi Dana, Marco here — the refund's on my screen, and I've credited two months for the outage. Anything else I can fix today?",
  },
  { who: "cust", name: "Dana", text: "Wow. Okay — thank you both." },
  { who: "sys", text: "call resolved · 1m 42s · csat 5★ · next call in queue" },
];

const HANDOFF_LINE = 6;
const TICK_MS = 85;

/* precompute per-line word tick schedules */
const schedule = (() => {
  let t = 4;
  return SCRIPT.map((ln) => {
    const start = t;
    const words = ln.text.split(" ");
    const ticks = words.map((_, i) => {
      t += ln.who === "sys" ? 0 : 1;
      return ln.who === "sys" ? start : start + i + 1;
    });
    t += ln.who === "sys" ? 9 : 6;
    return { start, ticks, words };
  });
})();
const TOTAL_TICKS = schedule[schedule.length - 1].ticks.at(-1)! + 14;

const WAVE = Array.from({ length: 26 }, (_, i) => 0.25 + 0.75 * Math.abs(Math.sin(i * 1.7) * Math.cos(i * 0.6)));

function fmt(s: number) {
  const m = Math.floor(s / 60);
  const ss = Math.floor(s % 60);
  return `${m}:${ss.toString().padStart(2, "0")}`;
}

/* ------------------------------- call simulator ------------------------------ */

export function CallSim() {
  const prm = usePRM();
  const [tick, setTick] = useState(prm ? TOTAL_TICKS : 0);
  const [running, setRunning] = useState(!prm);
  const [elapsed, setElapsed] = useState(0);
  const scrollRef = useRef<HTMLDivElement>(null);
  const latency = useJitter(182, 64, 900);

  useEffect(() => {
    if (prm || !running) return;
    const id = window.setInterval(() => setTick((t) => t + 1), TICK_MS);
    return () => window.clearInterval(id);
  }, [prm, running]);

  useEffect(() => {
    if (prm) return;
    if (tick < TOTAL_TICKS) return;
    const id = window.setTimeout(() => {
      setTick(0);
      setElapsed(0);
      setRunning(true);
    }, 4600);
    return () => window.clearTimeout(id);
  }, [tick, prm]);

  useEffect(() => {
    if (prm || !running) return;
    const id = window.setInterval(() => setElapsed((e) => e + 1), 1000);
    return () => window.clearInterval(id);
  }, [prm, running]);

  useEffect(() => {
    const el = scrollRef.current;
    if (el) el.scrollTop = el.scrollHeight;
  }, [tick]);

  const phase: "ai" | "human" = tick >= schedule[HANDOFF_LINE].start ? "human" : "ai";
  const ended = tick >= TOTAL_TICKS;

  const confidence = useMemo(() => {
    let c = 92;
    SCRIPT.forEach((ln, i) => {
      if (ln.conf !== undefined && tick >= schedule[i].ticks.at(-1)!) c = ln.conf;
    });
    return c;
  }, [tick]);

  const confColor =
    confidence >= 85 ? "var(--color-mint)" : confidence >= 70 ? "var(--color-amber)" : "var(--color-coral)";

  return (
    <div
      className="relative border border-line bg-panel/90 shadow-2xl transition-[box-shadow,border-color] duration-700"
      style={{
        boxShadow:
          phase === "human"
            ? "0 40px 90px -35px rgba(255,178,94,0.35), 0 0 0 1px rgba(255,178,94,0.12)"
            : "0 40px 90px -35px rgba(86,224,191,0.3), 0 0 0 1px rgba(86,224,191,0.1)",
      }}
    >
      {/* chrome */}
      <div className="flex items-center justify-between border-b border-line px-4 py-3">
        <div className="flex items-center gap-2.5">
          <span className={`h-2 w-2 rounded-full ${ended ? "bg-dim" : phase === "human" ? "bg-amber pulse-dot-amber" : "bg-mint pulse-dot"}`} />
          <span className="font-mono text-[10px] uppercase tracking-[0.22em] text-dim">
            Tandem switchboard
          </span>
        </div>
        <div className="flex items-center gap-3 font-mono text-[11px] text-dim">
          <span className="text-mist tabular-nums">{ended ? "—:—" : fmt(elapsed)}</span>
          <span className="hidden sm:inline">#CL-48213</span>
        </div>
      </div>

      {/* caller + telemetry */}
      <div className="flex items-stretch gap-3 border-b border-line px-4 py-3">
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2.5">
            <div className={`flex h-9 w-9 shrink-0 items-center justify-center border font-display text-sm font-bold ${phase === "human" ? "border-amber/60 text-amber" : "border-mint/60 text-mint"}`}>
              DR
            </div>
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold text-mist">Dana Reyes</p>
              <p className="truncate font-mono text-[10px] uppercase tracking-[0.14em] text-dim">
                +1 (415) 555-0132 · en-us
              </p>
            </div>
          </div>
          <div className="mt-2.5 flex flex-wrap gap-1.5">
            <span className="chip">intent: billing</span>
            <span className="chip">refund</span>
            <span className={`chip ${tick >= schedule[4].start ? "!border-coral/60 !text-coral" : ""}`}>
              churn-risk
            </span>
          </div>
        </div>
        <div className="hidden w-28 shrink-0 flex-col justify-between border-l border-line pl-3 sm:flex">
          <div>
            <p className="font-mono text-[9px] uppercase tracking-[0.2em] text-dim">confidence</p>
            <p className="font-display text-xl font-bold tabular-nums" style={{ color: confColor }}>
              {confidence}
            </p>
          </div>
          <div className="h-1.5 w-full bg-faint">
            <div className="numflip h-full" style={{ width: `${confidence}%`, backgroundColor: confColor }} />
          </div>
          <div className="flex justify-between font-mono text-[9px] uppercase tracking-[0.14em] text-dim">
            <span>infer {latency}ms</span>
            <span className={phase === "human" ? "text-amber" : "text-mint"}>{phase}</span>
          </div>
        </div>
      </div>

      {/* transcript */}
      <div ref={scrollRef} className="h-[330px] overflow-hidden px-4 py-4 sm:h-[350px]">
        <div className="space-y-3">
          {SCRIPT.map((ln, i) => {
            const sch = schedule[i];
            if (tick < sch.start) return null;
            const wordsShown = sch.ticks.filter((t) => t <= tick).length;
            const done = wordsShown >= sch.words.length;
            const typing = !done && running;
            const text = sch.words.slice(0, Math.max(ln.who === "sys" ? 1 : 1, wordsShown)).join(" ");

            if (ln.who === "sys") {
              const isHandoff = i === HANDOFF_LINE;
              return (
                <div key={i} className={`flex items-center gap-2.5 py-1 ${isHandoff ? "my-1" : ""}`}>
                  <span className={`h-px flex-1 ${isHandoff ? "border-t border-dashed border-amber/50" : "bg-faint"}`} />
                  <span className={`font-mono text-[10px] uppercase tracking-[0.16em] ${isHandoff ? "text-amber" : "text-dim"}`}>
                    {ln.text}
                  </span>
                  <span className={`h-px flex-1 ${isHandoff ? "border-t border-dashed border-amber/50" : "bg-faint"}`} />
                </div>
              );
            }

            const accent =
              ln.who === "ai"
                ? { bar: "bg-mint", tag: "text-mint", box: "border-mint/25" }
                : ln.who === "human"
                ? { bar: "bg-amber", tag: "text-amber", box: "border-amber/30" }
                : { bar: "bg-dim/50", tag: "text-dim", box: "border-line" };

            return (
              <div key={i} className="flex gap-2.5">
                <span className={`mt-1 w-[3px] shrink-0 self-stretch ${accent.bar}`} />
                <div className={`min-w-0 flex-1 border bg-deep/60 px-3 py-2 ${accent.box}`}>
                  <div className="mb-1 flex items-center justify-between gap-2">
                    <span className={`font-mono text-[9px] uppercase tracking-[0.2em] ${accent.tag}`}>
                      {ln.name}
                    </span>
                    {ln.conf !== undefined && done && (
                      <span className="font-mono text-[9px] tabular-nums text-dim">conf {ln.conf}</span>
                    )}
                  </div>
                  <p className="text-[13px] leading-relaxed text-mist/95">
                    {text}
                    {typing && <span className="caret ml-0.5 inline-block h-3.5 w-[7px] translate-y-[2px] bg-mint" />}
                  </p>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* waveform + controls */}
      <div className="flex items-center gap-3 border-t border-line px-4 py-3">
        <div className="flex h-7 flex-1 items-center gap-[3px]">
          {WAVE.map((h, i) => (
            <span
              key={i}
              className="wavebar w-[3px] flex-1"
              style={{
                height: `${h * 100}%`,
                backgroundColor: phase === "human" ? "var(--color-amber)" : "var(--color-mint)",
                opacity: ended ? 0.35 : 0.9,
                animationDelay: `${(i % 9) * 90}ms`,
                animationPlayState: running && !ended ? "running" : "paused",
              }}
            />
          ))}
        </div>
        <button
          onClick={() => {
            if (prm) return;
            if (ended) {
              setTick(0);
              setElapsed(0);
              setRunning(true);
            } else {
              setRunning((r) => !r);
            }
          }}
          aria-label={running ? "Pause call simulation" : "Resume call simulation"}
          className="flex h-8 w-8 items-center justify-center border border-line text-dim transition-colors hover:border-mint hover:text-mint"
        >
          {running && !ended ? (
            <svg width="10" height="12" viewBox="0 0 10 12" fill="currentColor"><rect width="3.4" height="12" /><rect x="6.6" width="3.4" height="12" /></svg>
          ) : (
            <svg width="11" height="12" viewBox="0 0 11 12" fill="currentColor"><path d="M0 0l11 6-11 6z" /></svg>
          )}
        </button>
        <button
          onClick={() => {
            if (prm) return;
            setTick(0);
            setElapsed(0);
            setRunning(true);
          }}
          aria-label="Replay call simulation"
          className="flex h-8 w-8 items-center justify-center border border-line text-dim transition-colors hover:border-mint hover:text-mint"
        >
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.4"><path d="M3 12a9 9 0 1 0 3-6.7" /><path d="M3 3v6h6" /></svg>
        </button>
      </div>
    </div>
  );
}

/* ---------------------------------- hero ---------------------------------- */

const AVATARS = [33680700, 1546912, 8867242].map(
  (id) =>
    `https://images.pexels.com/photos/${id}/pexels-photo-${id}.jpeg?auto=compress&cs=tinysrgb&fit=crop&h=96&w=96`
);

export default function Hero() {
  const kicker = useScramble("// live from the switchboard", 300);

  return (
    <section id="top" className="relative overflow-hidden px-5 pb-16 pt-32 sm:pt-36 lg:px-8">
      <div className="mx-auto grid max-w-7xl items-start gap-12 lg:grid-cols-[1.05fr_0.95fr] lg:gap-10">
        {/* copy */}
        <div data-reveal>
          <p className="kicker mb-6 flex items-center gap-2">
            <span className="text-mint">{kicker}</span>
            <span className="caret inline-block h-3.5 w-[7px] bg-mint" />
          </p>
          <h1 className="font-display text-[clamp(2.6rem,6vw,4.6rem)] font-bold leading-[1.02] tracking-tight text-mist">
            <span className="mask-line" style={{ "--d": "80ms" } as React.CSSProperties}>
              <span>Every call answered.</span>
            </span>
            <span className="mask-line" style={{ "--d": "200ms" } as React.CSSProperties}>
              <span>The hard ones, passed</span>
            </span>
            <span className="mask-line" style={{ "--d": "320ms" } as React.CSSProperties}>
              <span>
                to a <em className="font-light italic text-amber">human</em> — warm.
              </span>
            </span>
          </h1>
          <p className="mt-7 max-w-xl text-lg leading-relaxed text-dim" data-reveal style={{ "--d": "420ms" } as React.CSSProperties}>
            Tandem puts inference-fast AI agents on your phone lines. They resolve{" "}
            <span className="text-mist">9 of 10 calls end-to-end</span> — and pass the rest to your
            team with the transcript, sentiment and next-best action already on screen.
          </p>
          <div className="mt-9 flex flex-wrap items-center gap-4" data-reveal style={{ "--d": "520ms" } as React.CSSProperties}>
            <a href="#demo" className="btn btn-amber">
              Book a live demo
              <svg width="13" height="12" viewBox="0 0 13 12" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M1 6h10M7 1.5 11.5 6 7 10.5" /></svg>
            </a>
            <Link to="/login" className="btn btn-mint">
              Open live dashboard
              <svg width="13" height="12" viewBox="0 0 13 12" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M1 6h10M7 1.5 11.5 6 7 10.5" /></svg>
            </Link>
            <a href="#platform" className="btn btn-ghost">
              <svg width="11" height="12" viewBox="0 0 11 12" fill="currentColor"><path d="M0 0l11 6-11 6z" /></svg>
              See the hybrid loop
            </a>
          </div>

          {/* proof strip */}
          <div className="mt-12 flex flex-wrap items-center gap-x-6 gap-y-4" data-reveal style={{ "--d": "640ms" } as React.CSSProperties}>
            <div className="flex -space-x-2.5">
              {AVATARS.map((src, i) => (
                <img
                  key={src}
                  src={src}
                  alt="Support lead avatar"
                  className="h-9 w-9 rounded-full border-2 border-ink object-cover grayscale-[35%]"
                  style={{ zIndex: 3 - i }}
                  loading="lazy"
                />
              ))}
              <span className="z-0 flex h-9 w-9 items-center justify-center rounded-full border-2 border-ink bg-raised font-mono text-[9px] text-mint">
                240+
              </span>
            </div>
            <div className="font-mono text-[11px] uppercase tracking-[0.14em] text-dim">
              trusted on <span className="text-mist">240+ support floors</span>
            </div>
            <div className="hidden h-4 w-px bg-line sm:block" />
            <div className="flex gap-5 font-mono text-[11px] uppercase tracking-[0.14em] text-dim">
              <span><span className="text-mint">0.8s</span> pickup</span>
              <span><span className="text-mint">94%</span> contained</span>
              <span><span className="text-mint">38</span> languages</span>
            </div>
          </div>
        </div>

        {/* live call */}
        <div data-reveal style={{ "--d": "260ms" } as React.CSSProperties}>
          <CallSim />
          <div className="mt-3 flex flex-wrap items-center justify-between gap-2 border border-line bg-deep/70 px-4 py-2.5 font-mono text-[10px] uppercase tracking-[0.16em] text-dim">
            <span className="flex items-center gap-2">
              <span className="pulse-dot h-1.5 w-1.5 rounded-full bg-mint" />
              in queue: <span className="text-mist">3</span>
            </span>
            <span>longest wait <span className="text-mist">0:04</span></span>
            <span>agents online <span className="text-amber">12</span></span>
          </div>
        </div>
      </div>
    </section>
  );
}
