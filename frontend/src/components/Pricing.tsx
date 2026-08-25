const Check = ({ tone = "var(--color-mint)" }: { tone?: string }) => (
  <svg width="15" height="15" viewBox="0 0 16 16" fill="none" stroke={tone} strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className="mt-0.5 shrink-0">
    <path d="M2.5 8.5 6.5 12.5 13.5 4.5" />
  </svg>
);

const GROWTH = [
  "Unlimited AI agents & voices",
  "Warm handoff + whisper intros",
  "Telephony & CRM connectors",
  "QA scoring on 100% of calls",
  "Sentiment, intent & gap analytics",
  "99.95% uptime SLA",
];

export default function Pricing() {
  return (
    <section id="pricing" className="relative scroll-mt-24 border-t border-line bg-deep/40 px-5 py-24 lg:px-8 lg:py-32">
      <div className="mx-auto max-w-7xl">
        <div className="mb-14 flex flex-wrap items-end justify-between gap-6" data-reveal>
          <div>
            <p className="kicker mb-4">// pricing</p>
            <h2 className="font-display text-[clamp(2rem,4.5vw,3.4rem)] font-bold leading-[1.05] tracking-tight text-mist">
              Pay for outcomes,<br />not seats.
            </h2>
          </div>
          <p className="max-w-sm text-base leading-relaxed text-dim lg:text-right">
            You're billed for minutes your AI actually resolves. Handoff minutes to your humans?
            Free — they're yours already.
          </p>
        </div>

        <div className="grid gap-6 lg:grid-cols-[1.15fr_0.85fr]">
          {/* featured */}
          <div
            data-reveal
            className="relative flex flex-col border border-amber/50 bg-panel/80 p-8 sm:p-10"
            style={{ boxShadow: "0 40px 90px -40px rgba(255,178,94,0.3)" }}
          >
            <span className="absolute -top-3 left-8 bg-amber px-3 py-1 font-mono text-[10px] font-semibold uppercase tracking-[0.2em] text-[#211503]">
              most floors pick this
            </span>
            <div className="flex flex-wrap items-baseline justify-between gap-3">
              <h3 className="font-display text-3xl font-bold tracking-tight text-mist">Hybrid Growth</h3>
              <p className="font-mono text-[11px] uppercase tracking-[0.16em] text-dim">from 5k calls/mo</p>
            </div>
            <p className="mt-6">
              <span className="font-display text-6xl font-bold tabular-nums tracking-tight text-amber">$0.19</span>
              <span className="ml-2 font-mono text-[11px] uppercase tracking-[0.16em] text-dim">/ resolved minute</span>
            </p>
            <ul className="mt-8 grid gap-3 sm:grid-cols-2">
              {GROWTH.map((f) => (
                <li key={f} className="flex items-start gap-2.5 text-sm text-mist/90">
                  <Check tone="var(--color-amber)" />
                  {f}
                </li>
              ))}
            </ul>
            <div className="mt-10 flex flex-wrap items-center gap-4">
              <a href="#demo" className="btn btn-amber">Start 30-day pilot</a>
              <span className="font-mono text-[10px] uppercase tracking-[0.16em] text-dim">live in 5 days · cancel anytime</span>
            </div>
          </div>

          {/* stacked */}
          <div className="flex flex-col gap-6">
            <div data-reveal style={{ "--d": "120ms" } as React.CSSProperties} className="lift flex-1 border border-line bg-panel/60 p-8 hover:border-mint/50">
              <div className="flex items-baseline justify-between gap-3">
                <h3 className="font-display text-2xl font-bold tracking-tight text-mist">Starter</h3>
                <p>
                  <span className="font-display text-3xl font-bold tabular-nums text-mint">$0.29</span>
                  <span className="ml-1.5 font-mono text-[10px] uppercase tracking-[0.14em] text-dim">/ min</span>
                </p>
              </div>
              <p className="mt-3 text-sm leading-relaxed text-dim">
                For floors under 5k calls a month. One AI agent, standard handoff, community support.
              </p>
              <ul className="mt-5 space-y-2.5">
                {["1 AI agent, any voice", "Handoff with transcript", "Twilio or SIP connect"].map((f) => (
                  <li key={f} className="flex items-start gap-2.5 text-sm text-mist/85"><Check />{f}</li>
                ))}
              </ul>
              <a href="#demo" className="mt-6 inline-flex items-center gap-2 font-mono text-[11px] uppercase tracking-[0.2em] text-mint transition-colors hover:text-mist">
                start free <svg width="12" height="11" viewBox="0 0 13 12" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M1 6h10M7 1.5 11.5 6 7 10.5" /></svg>
              </a>
            </div>

            <div data-reveal style={{ "--d": "240ms" } as React.CSSProperties} className="lift flex-1 border border-line bg-panel/60 p-8 hover:border-mint/50">
              <div className="flex items-baseline justify-between gap-3">
                <h3 className="font-display text-2xl font-bold tracking-tight text-mist">Enterprise</h3>
                <span className="font-display text-3xl font-bold text-mint">custom</span>
              </div>
              <p className="mt-3 text-sm leading-relaxed text-dim">
                VPC or on-prem inference, custom models tuned on your calls, dedicated success engineer.
              </p>
              <ul className="mt-5 space-y-2.5">
                {["HIPAA BAA · EU data residency", "99.99% SLA, private cloud", "Human squad staffing add-on"].map((f) => (
                  <li key={f} className="flex items-start gap-2.5 text-sm text-mist/85"><Check />{f}</li>
                ))}
              </ul>
              <a href="#demo" className="mt-6 inline-flex items-center gap-2 font-mono text-[11px] uppercase tracking-[0.2em] text-mint transition-colors hover:text-mist">
                talk to us <svg width="12" height="11" viewBox="0 0 13 12" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M1 6h10M7 1.5 11.5 6 7 10.5" /></svg>
              </a>
            </div>
          </div>
        </div>

        <p className="mt-8 border border-dashed border-line px-5 py-3.5 text-center font-mono text-[10px] uppercase tracking-[0.2em] text-dim" data-reveal>
          every plan includes · warm handoff · full transcripts · audit logs · 38 languages · no hold music, ever
        </p>
      </div>
    </section>
  );
}
