import { useEffect, useRef, useState } from "react";

type Step = {
  id: string;
  num: string;
  title: string;
  body: string;
  tags: string[];
  tone: "mint" | "amber";
  icon: React.ReactNode;
};

const stroke = { fill: "none", stroke: "currentColor", strokeWidth: 1.8, strokeLinecap: "round", strokeLinejoin: "round" } as const;

const STEPS: Step[] = [
  {
    id: "listen",
    num: "01",
    title: "Listen",
    body: "Streaming speech-to-text with barge-in, so callers interrupt like they would with a person. Language is detected in the first breath and switched mid-call if the caller does.",
    tags: ["stt 140ms", "barge-in", "38 langs", "vad"],
    tone: "mint",
    icon: (
      <svg width="26" height="26" viewBox="0 0 26 26" {...stroke}>
        <path d="M3 13h2M7 8v10M11 4v18M15 7v12M19 10v6M23 12v2" />
      </svg>
    ),
  },
  {
    id: "reason",
    num: "02",
    title: "Reason",
    body: "A small router model triages every turn in single-digit milliseconds. The large model fires only when the call deserves it — so p50 inference stays under 200ms and your token bill doesn't.",
    tags: ["router 8ms", "p50 182ms", "guardrails", "tool use"],
    tone: "mint",
    icon: (
      <svg width="26" height="26" viewBox="0 0 26 26" {...stroke}>
        <circle cx="6" cy="6" r="2.4" /><circle cx="20" cy="6" r="2.4" /><circle cx="13" cy="19" r="2.4" />
        <path d="M8 7.5 11.3 17M18 7.5 14.7 17M8.4 6h9.2" />
      </svg>
    ),
  },
  {
    id: "resolve",
    num: "03",
    title: "Resolve",
    body: "Agents act, not just talk — refunds issued, appointments moved, tickets closed through your APIs. Every write is policy-checked, rate-limited and logged for QA to replay.",
    tags: ["refunds", "crm writes", "policy engine", "audit log"],
    tone: "mint",
    icon: (
      <svg width="26" height="26" viewBox="0 0 26 26" {...stroke}>
        <path d="M4 13.5 10 19.5 22 6.5" /><path d="M4 21h18" opacity=".45" />
      </svg>
    ),
  },
  {
    id: "handoff",
    num: "04",
    title: "Hand off — warm",
    body: "When confidence dips or a caller asks for a person, the call transfers in under a second. Your human sees the transcript, sentiment arc and next-best actions before they say hello.",
    tags: ["context 0.4s", "whisper intro", "coach mode", "no repeat"],
    tone: "amber",
    icon: (
      <svg width="26" height="26" viewBox="0 0 26 26" {...stroke}>
        <circle cx="9" cy="13" r="5.2" /><circle cx="17" cy="13" r="5.2" stroke="var(--color-amber)" />
      </svg>
    ),
  },
];

export default function HybridLoop() {
  const [active, setActive] = useState(0);
  const refs = useRef<(HTMLElement | null)[]>([]);

  useEffect(() => {
    const io = new IntersectionObserver(
      (entries) => {
        entries.forEach((e) => {
          if (e.isIntersecting) setActive(Number((e.target as HTMLElement).dataset.idx ?? 0));
        });
      },
      { rootMargin: "-44% 0px -44% 0px", threshold: 0 }
    );
    refs.current.forEach((el) => el && io.observe(el));
    return () => io.disconnect();
  }, []);

  const step = STEPS[active];

  return (
    <section id="platform" className="relative scroll-mt-24 px-5 py-24 lg:px-8 lg:py-32">
      <div className="mx-auto max-w-7xl">
        <div className="mb-16 grid gap-6 lg:grid-cols-[1fr_auto] lg:items-end" data-reveal>
          <div>
            <p className="kicker mb-4">// the hybrid loop</p>
            <h2 className="font-display text-[clamp(2rem,4.5vw,3.4rem)] font-bold leading-[1.05] tracking-tight text-mist">
              One call.
              <br />
              Two intelligences.
            </h2>
          </div>
          <p className="max-w-md text-base leading-relaxed text-dim lg:pb-2 lg:text-right">
            Every Tandem call runs the same four-beat loop. Most calls end at beat three.
            The ones that don't are exactly the ones a human should take.
          </p>
        </div>

        <div className="grid gap-12 lg:grid-cols-[0.85fr_1.15fr] lg:gap-20">
          {/* sticky rail */}
          <div className="hidden lg:block">
            <div className="sticky top-32">
              <p
                className="font-display text-[150px] font-bold leading-none tracking-tight transition-colors duration-500"
                style={{ color: step.tone === "amber" ? "var(--color-amber)" : "var(--color-mint)" }}
              >
                {step.num}
              </p>
              <p className="mt-2 font-display text-3xl font-bold text-mist">{step.title}</p>
              <p className="mt-4 max-w-sm leading-relaxed text-dim">{step.body}</p>

              <div className="mt-10 space-y-0">
                {STEPS.map((s, i) => (
                  <a key={s.id} href={`#${s.id}`} className="group flex items-center gap-4 py-2.5">
                    <span
                      className="numflip h-[3px] w-10 transition-all duration-500"
                      style={{
                        backgroundColor:
                          i <= active ? (s.tone === "amber" ? "var(--color-amber)" : "var(--color-mint)") : "var(--color-line)",
                        width: i === active ? 56 : 32,
                      }}
                    />
                    <span
                      className={`font-mono text-[11px] uppercase tracking-[0.2em] transition-colors duration-300 ${
                        i === active ? "text-mist" : "text-dim group-hover:text-mist"
                      }`}
                    >
                      {s.num} — {s.title}
                    </span>
                  </a>
                ))}
              </div>
            </div>
          </div>

          {/* scrolling steps */}
          <div>
            {STEPS.map((s, i) => (
              <article
                key={s.id}
                id={s.id}
                data-idx={i}
                ref={(el) => {
                  refs.current[i] = el;
                }}
                data-reveal
                style={{ "--d": "80ms" } as React.CSSProperties}
                className={`group scroll-mt-28 border-l-2 px-6 py-12 transition-all duration-500 sm:px-10 lg:min-h-[46vh] lg:py-16 ${
                  active === i ? "border-mint/70 bg-panel/60" : "border-line bg-transparent lg:opacity-45"
                }`}
              >
                <div className="flex items-start justify-between gap-4">
                  <span
                    className="font-mono text-[11px] uppercase tracking-[0.24em]"
                    style={{ color: s.tone === "amber" ? "var(--color-amber)" : "var(--color-mint)" }}
                  >
                    beat {s.num}
                  </span>
                  <span className={`transition-colors duration-300 ${s.tone === "amber" ? "text-amber" : "text-mint"}`}>
                    {s.icon}
                  </span>
                </div>
                <h3 className="mt-5 font-display text-3xl font-bold tracking-tight text-mist sm:text-4xl">
                  {s.title}
                </h3>
                <p className="mt-4 max-w-xl leading-relaxed text-dim">{s.body}</p>
                <div className="mt-6 flex flex-wrap gap-2">
                  {s.tags.map((t) => (
                    <span
                      key={t}
                      className={`chip transition-colors duration-300 ${
                        s.tone === "amber" ? "hover:!border-amber/60 hover:!text-amber" : "hover:!border-mint/60 hover:!text-mint"
                      }`}
                    >
                      {t}
                    </span>
                  ))}
                </div>
              </article>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
