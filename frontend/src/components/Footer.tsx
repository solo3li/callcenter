import { useEffect, useRef, useState } from "react";
import { Logo } from "./Nav";

type CallState = "idle" | "dialing" | "done";

export function DemoCta() {
  const [phone, setPhone] = useState("");
  const [state, setState] = useState<CallState>("idle");
  const timers = useRef<number[]>([]);

  useEffect(() => () => timers.current.forEach((t) => window.clearTimeout(t)), []);

  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    if (state !== "idle" || phone.replace(/\D/g, "").length < 7) return;
    setState("dialing");
    timers.current.push(window.setTimeout(() => setState("done"), 2600));
  };

  return (
    <section id="demo" className="relative scroll-mt-24 overflow-hidden border-t border-line bg-deep/60 px-5 py-24 lg:px-8 lg:py-32">
      <div className="pointer-events-none absolute -left-24 top-10 hidden opacity-[0.07] lg:block" aria-hidden="true">
        <svg width="340" height="340" viewBox="0 0 32 32">
          <circle cx="12.5" cy="16" r="8.5" fill="none" stroke="var(--color-mint)" strokeWidth="1.2" />
          <circle cx="20.5" cy="16" r="8.5" fill="none" stroke="var(--color-amber)" strokeWidth="1.2" />
        </svg>
      </div>

      <div className="mx-auto grid max-w-7xl items-center gap-14 lg:grid-cols-[1.1fr_0.9fr]">
        <div data-reveal>
          <p className="kicker mb-5">// hear it on your own line</p>
          <h2 className="font-display text-[clamp(2.4rem,5.5vw,4.2rem)] font-bold leading-[1.02] tracking-tight text-mist">
            Put your queue
            <br />
            on <em className="font-light italic text-mint">autopilot.</em>
          </h2>
          <p className="mt-6 max-w-lg text-lg leading-relaxed text-dim">
            Leave a number. Our AI calls you back in about a minute, answers your questions,
            books a pilot — and escalates to Marco the second you ask for a person.
          </p>
          <div className="mt-8 flex flex-wrap gap-x-8 gap-y-3 font-mono text-[11px] uppercase tracking-[0.16em] text-dim">
            <span><span className="text-mint">60s</span> callback</span>
            <span><span className="text-mint">5 days</span> to live</span>
            <span><span className="text-mint">0</span> hold music</span>
          </div>
        </div>

        <div data-reveal style={{ "--d": "180ms" } as React.CSSProperties}>
          <form onSubmit={submit} className="border border-line bg-panel/80 p-8" style={{ boxShadow: "0 40px 90px -40px rgba(86,224,191,0.25)" }}>
            <label htmlFor="phone" className="font-mono text-[10px] uppercase tracking-[0.22em] text-dim">
              get a live demo call
            </label>
            <div className="mt-3 flex gap-2">
              <input
                id="phone"
                type="tel"
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
                placeholder="+1 (555) 000-0000"
                disabled={state !== "idle"}
                className="min-w-0 flex-1 border border-line bg-deep px-4 py-3.5 font-mono text-sm text-mist placeholder:text-dim/60 focus:border-mint focus:outline-none disabled:opacity-60"
              />
              <button type="submit" className={`btn ${state === "idle" ? "btn-mint" : "btn-ghost"} !px-5`}>
                {state === "idle" && "Call me"}
                {state === "dialing" && (
                  <>
                    <svg className="ringring" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <path d="M22 16.9v3a2 2 0 0 1-2.2 2 19.8 19.8 0 0 1-8.6-3.1 19.5 19.5 0 0 1-6-6A19.8 19.8 0 0 1 2.1 4.2 2 2 0 0 1 4.1 2h3a2 2 0 0 1 2 1.7c.13.96.36 1.9.7 2.8a2 2 0 0 1-.45 2.1L8.1 9.9a16 16 0 0 0 6 6l1.3-1.3a2 2 0 0 1 2.1-.45c.9.34 1.84.57 2.8.7A2 2 0 0 1 22 16.9Z" />
                    </svg>
                    dialing
                  </>
                )}
                {state === "done" && "✓ booked"}
              </button>
            </div>
            <p className="mt-4 min-h-[3em] font-mono text-[11px] leading-relaxed tracking-[0.06em] text-dim">
              {state === "idle" && "// an AI answers. ask for a human and watch the warm handoff happen live."}
              {state === "dialing" && <span className="text-mint">// ringing your line… noor is warming up the transcript.</span>}
              {state === "done" && (
                <span className="text-amber">// call booked — +1 (415) 555-0119 rings you shortly. if it doesn't, marco owes you a coffee.</span>
              )}
            </p>
          </form>
        </div>
      </div>
    </section>
  );
}

const COLS: { title: string; links: string[] }[] = [
  { title: "Product", links: ["Platform", "Hybrid handoff", "Agent roster", "Analytics", "Changelog"] },
  { title: "Company", links: ["About", "Careers — 4 open", "Press kit", "Security", "Contact"] },
  { title: "Resources", links: ["Docs", "API reference", "ROI calculator", "Floor playbooks", "Status"] },
];

export default function Footer() {
  return (
    <>
      <DemoCta />
      <footer className="border-t border-line bg-ink px-5 pb-10 pt-16 lg:px-8">
        <div className="mx-auto max-w-7xl">
          <div className="grid gap-12 lg:grid-cols-[1.2fr_repeat(3,0.8fr)]">
            <div>
              <Logo />
              <p className="mt-5 max-w-xs text-sm leading-relaxed text-dim">
                AI agents that answer every call, and humans who take the ones that matter.
                Built for the floor, not the boardroom.
              </p>
              <div className="mt-6 flex items-center gap-2 border border-line px-3 py-2 w-fit">
                <span className="pulse-dot h-1.5 w-1.5 rounded-full bg-mint" />
                <span className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim">
                  all systems answering · 99.99%
                </span>
              </div>
            </div>
            {COLS.map((c) => (
              <div key={c.title}>
                <p className="font-mono text-[10px] uppercase tracking-[0.22em] text-dim">{c.title}</p>
                <ul className="mt-4 space-y-2.5">
                  {c.links.map((l) => (
                    <li key={l}>
                      <a href="#top" className="text-sm text-mist/80 transition-colors duration-300 hover:text-mint">
                        {l}
                      </a>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
          <div className="mt-14 flex flex-wrap items-center justify-between gap-4 border-t border-line pt-6">
            <p className="font-mono text-[10px] uppercase tracking-[0.16em] text-dim">
              © 2026 Tandem Labs, Inc. · SOC 2 Type II · HIPAA · GDPR
            </p>
            <a href="#top" className="flex items-center gap-2 font-mono text-[10px] uppercase tracking-[0.2em] text-dim transition-colors hover:text-mint">
              back to the top
              <svg width="11" height="12" viewBox="0 0 12 12" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M6 11V1M1.5 5 6 .5 10.5 5" /></svg>
            </a>
          </div>
        </div>
      </footer>
    </>
  );
}
