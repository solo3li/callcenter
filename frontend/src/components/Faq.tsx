import { useState } from "react";

const FAQS = [
  {
    q: "What happens when the AI doesn't know something?",
    a: "It never bluffs. Every turn carries a confidence score; below your threshold, the call transfers warm — transcript, sentiment arc and suggested actions included. The unknown question lands in a gap report so QA can teach the agent the answer once, for every future call.",
  },
  {
    q: "Does it sound like a robot reading a script?",
    a: "No script. Agents speak from neural voices with barge-in, breath pacing and backchannels, and they're tuned on your best human's calls. Where regulation requires it, the agent discloses that it's AI in the first sentence — CSAT data says callers don't mind; they mind waiting.",
  },
  {
    q: "Which phone systems and CRMs does it connect to?",
    a: "Twilio, Amazon Connect, Genesys, Five9, Talkdesk, Vonage or a plain SIP trunk on the voice side; Salesforce, Zendesk, HubSpot, Intercom, Stripe and custom REST APIs on the data side. Most floors are live inside a week.",
  },
  {
    q: "What about recordings, privacy and compliance?",
    a: "Consent flows are built into the greeting, PII is redacted in transcripts at ingest, and everything is encrypted in transit and at rest. SOC 2 Type II audited, HIPAA BAA available, EU data residency on Enterprise.",
  },
  {
    q: "How long does launch actually take?",
    a: "Five days: clone and tune voices (day 1–2), connect telephony and CRM (day 2–3), run shadow mode beside your humans (day 3–5), then go live with guardrails and a kill-switch on every policy.",
  },
  {
    q: "Do callers hate talking to an AI?",
    a: "They hate hold music. Median pickup is 0.8 seconds, resolution lands inside two minutes, and pressing zero reaches a human with full context — that combination is why CSAT runs 4.7, not because anyone misses the queue.",
  },
];

export default function Faq() {
  const [open, setOpen] = useState<number>(0);
  return (
    <section id="faq" className="relative scroll-mt-24 px-5 py-24 lg:px-8 lg:py-32">
      <div className="mx-auto grid max-w-7xl gap-12 lg:grid-cols-[0.8fr_1.2fr] lg:gap-20">
        <div className="lg:sticky lg:top-32 lg:self-start" data-reveal>
          <p className="kicker mb-4">// straight answers</p>
          <h2 className="font-display text-[clamp(2rem,4.5vw,3.4rem)] font-bold leading-[1.05] tracking-tight text-mist">
            Asked on every discovery call.
          </h2>
          <p className="mt-5 max-w-sm leading-relaxed text-dim">
            Six questions, no weasel words. For anything sharper, the demo line is open —
            an AI answers, and Marco takes over if you ask.
          </p>
          <a href="#demo" className="btn btn-ghost mt-8">Ask it live</a>
        </div>

        <div className="divide-y divide-line border-y border-line" data-reveal style={{ "--d": "150ms" } as React.CSSProperties}>
          {FAQS.map((f, i) => {
            const isOpen = open === i;
            return (
              <div key={f.q}>
                <button
                  onClick={() => setOpen(isOpen ? -1 : i)}
                  aria-expanded={isOpen}
                  className="group flex w-full items-center justify-between gap-6 py-6 text-left"
                >
                  <span className="flex items-baseline gap-4">
                    <span className={`font-mono text-[11px] tabular-nums tracking-[0.14em] transition-colors ${isOpen ? "text-mint" : "text-dim"}`}>
                      {String(i + 1).padStart(2, "0")}
                    </span>
                    <span className={`font-display text-xl font-semibold tracking-tight transition-colors duration-300 sm:text-2xl ${isOpen ? "text-mint" : "text-mist group-hover:text-mint"}`}>
                      {f.q}
                    </span>
                  </span>
                  <svg
                    width="16"
                    height="16"
                    viewBox="0 0 16 16"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="1.8"
                    strokeLinecap="round"
                    className={`shrink-0 transition-transform duration-500 ${isOpen ? "rotate-45 text-mint" : "text-dim"}`}
                    aria-hidden="true"
                  >
                    <path d="M8 2v12M2 8h12" />
                  </svg>
                </button>
                <div className={`grid transition-[grid-template-rows] duration-500 ease-[cubic-bezier(0.22,1,0.36,1)] ${isOpen ? "grid-rows-[1fr]" : "grid-rows-[0fr]"}`}>
                  <div className="overflow-hidden">
                    <p className="max-w-2xl pb-7 pl-[42px] leading-relaxed text-dim">{f.a}</p>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </section>
  );
}
