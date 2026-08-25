import { useRef, useState } from "react";

type Agent = {
  name: string;
  kind: "ai" | "human";
  role: string;
  meta: string;
  langs: string[];
  opener: string;
  initials: string;
  photo?: string;
};

const AGENTS: Agent[] = [
  {
    name: "Noor",
    kind: "ai",
    role: "Billing & refunds",
    meta: "2.1M calls resolved",
    langs: ["en", "ar", "fr", "tr"],
    opener: "I see the duplicate charge — refunding it to your card now.",
    initials: "NO",
  },
  {
    name: "Atlas",
    kind: "ai",
    role: "Technical support",
    meta: "runs diagnostics mid-sentence",
    langs: ["en", "es", "de", "pt"],
    opener: "Let's restart the router together — I'll wait on the line.",
    initials: "AT",
  },
  {
    name: "June",
    kind: "ai",
    role: "Sales & renewals",
    meta: "132% of quota, twice running",
    langs: ["en", "fr", "ja"],
    opener: "Your plan renews Tuesday — I found you a better one.",
    initials: "JU",
  },
  {
    name: "Priya",
    kind: "ai",
    role: "Bookings & logistics",
    meta: "rebooks 40 flights a night",
    langs: ["en", "hi", "it"],
    opener: "Thursday at two works — confirmed and on your calendar.",
    initials: "PR",
  },
  {
    name: "Marco & Lea",
    kind: "human",
    role: "Escalation squad",
    meta: "the last 6% — churn saves, VIPs, edge cases",
    langs: ["en", "es", "nl"],
    opener: "Hi, it's Marco — I've got your whole story already.",
    initials: "ML",
    photo:
      "https://images.pexels.com/photos/1546912/pexels-photo-1546912.jpeg?auto=compress&cs=tinysrgb&fit=crop&h=240&w=240",
  },
];

function VoiceButton({ active, onToggle }: { text?: string; active: boolean; onToggle: () => void }) {
  return (
    <button
      onClick={onToggle}
      className={`flex items-center gap-2 border px-3 py-2 font-mono text-[10px] uppercase tracking-[0.18em] transition-all duration-300 ${
        active
          ? "border-mint/70 bg-mint/10 text-mint"
          : "border-line text-dim hover:border-mint/50 hover:text-mint"
      }`}
    >
      {active ? (
        <span className="flex h-3 items-end gap-[2px]">
          {[0.6, 1, 0.45].map((h, i) => (
            <span
              key={i}
              className="wavebar w-[3px] bg-mint"
              style={{ height: `${h * 100}%`, animationDelay: `${i * 140}ms` }}
            />
          ))}
        </span>
      ) : (
        <svg width="9" height="10" viewBox="0 0 11 12" fill="currentColor"><path d="M0 0l11 6-11 6z" /></svg>
      )}
      {active ? "speaking…" : "hear voice"}
    </button>
  );
}

export default function Agents() {
  const railRef = useRef<HTMLDivElement>(null);
  const [speaking, setSpeaking] = useState<number | null>(null);

  const scrollBy = (dx: number) => railRef.current?.scrollBy({ left: dx, behavior: "smooth" });

  const toggleVoice = (i: number) => {
    if (typeof window === "undefined" || !("speechSynthesis" in window)) return;
    try {
      if (speaking === i) {
        window.speechSynthesis.cancel();
        setSpeaking(null);
        return;
      }
      window.speechSynthesis.cancel();
      const u = new SpeechSynthesisUtterance(AGENTS[i].opener);
      u.rate = 1.03;
      u.pitch = AGENTS[i].kind === "human" ? 0.95 : 1.05;
      u.onend = () => setSpeaking(null);
      u.onerror = () => setSpeaking(null);
      setSpeaking(i);
      window.speechSynthesis.speak(u);
    } catch {
      setSpeaking(null);
    }
  };

  return (
    <section id="agents" className="relative scroll-mt-24 overflow-hidden px-5 py-24 lg:px-8 lg:py-32">
      <div className="mx-auto max-w-7xl">
        <div className="mb-12 flex flex-wrap items-end justify-between gap-6" data-reveal>
          <div>
            <p className="kicker mb-4">// meet the floor</p>
            <h2 className="font-display text-[clamp(2rem,4.5vw,3.4rem)] font-bold leading-[1.05] tracking-tight text-mist">
              A roster that never sleeps —<br />
              <span className="text-amber">and two who do.</span>
            </h2>
          </div>
          <div className="flex gap-2">
            <button onClick={() => scrollBy(-380)} aria-label="Scroll agents left" className="btn btn-ghost !px-4 !py-3">
              <svg width="14" height="12" viewBox="0 0 14 12" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M13 6H1M5 1.5.5 6 5 10.5" /></svg>
            </button>
            <button onClick={() => scrollBy(380)} aria-label="Scroll agents right" className="btn btn-ghost !px-4 !py-3">
              <svg width="14" height="12" viewBox="0 0 14 12" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M1 6h12M9 1.5l4.5 4.5L9 10.5" /></svg>
            </button>
          </div>
        </div>
      </div>

      <div className="relative" data-reveal style={{ "--d": "150ms" } as React.CSSProperties}>
        <div className="pointer-events-none absolute inset-y-0 left-0 z-10 w-16 bg-gradient-to-r from-ink to-transparent" />
        <div className="pointer-events-none absolute inset-y-0 right-0 z-10 w-16 bg-gradient-to-l from-ink to-transparent" />
        <div
          ref={railRef}
          className="flex snap-x gap-5 overflow-x-auto px-5 pb-4 [scrollbar-width:thin] lg:px-[max(1.25rem,calc((100vw-80rem)/2+2rem))]"
        >
          {AGENTS.map((a, i) => {
            const human = a.kind === "human";
            return (
              <article
                key={a.name}
                className={`lift group w-[290px] shrink-0 snap-start border p-6 sm:w-[320px] ${
                  human
                    ? "border-amber/40 bg-gradient-to-b from-amber/[0.08] to-panel/80"
                    : "border-line bg-panel/80 hover:border-mint/50"
                }`}
              >
                <div className="flex items-start justify-between">
                  <span className={`chip ${human ? "!border-amber/50 !text-amber" : "!border-mint/40 !text-mint"}`}>
                    {human ? "human" : "ai agent"}
                  </span>
                  {a.photo ? (
                    <img
                      src={a.photo}
                      alt={a.name}
                      loading="lazy"
                      className="h-14 w-14 rounded-full border-2 border-amber/50 object-cover"
                    />
                  ) : (
                    <span className="relative flex h-14 w-14 items-center justify-center rounded-full border border-mint/40 bg-deep">
                      <span className="font-display text-sm font-bold text-mint">{a.initials}</span>
                      <span className="pulse-dot absolute -right-0.5 -top-0.5 h-2.5 w-2.5 rounded-full border-2 border-panel bg-mint" />
                    </span>
                  )}
                </div>
                <h3 className="mt-5 font-display text-2xl font-bold tracking-tight text-mist">{a.name}</h3>
                <p className={`font-mono text-[11px] uppercase tracking-[0.16em] ${human ? "text-amber" : "text-mint"}`}>
                  {a.role}
                </p>
                <p className="mt-2 text-sm text-dim">{a.meta}</p>
                <div className="mt-4 flex flex-wrap gap-1.5">
                  {a.langs.map((l) => (
                    <span key={l} className="chip !text-[9px]">{l}</span>
                  ))}
                </div>
                <blockquote className="mt-5 border-l-2 border-line pl-3 text-sm italic leading-relaxed text-mist/85">
                  “{a.opener}”
                </blockquote>
                <div className="mt-6 flex items-center justify-between">
                  <VoiceButton text={a.opener} active={speaking === i} onToggle={() => toggleVoice(i)} />
                  <span className="font-mono text-[10px] uppercase tracking-[0.14em] text-dim">
                    {human ? "backup in <1s" : "answers in 0.8s"}
                  </span>
                </div>
              </article>
            );
          })}

          {/* trailing CTA card */}
          <a
            href="#demo"
            className="lift flex w-[290px] shrink-0 snap-start flex-col justify-between border border-dashed border-line p-6 hover:border-mint/60 sm:w-[320px]"
          >
            <div>
              <span className="chip">your floor</span>
              <h3 className="mt-5 font-display text-2xl font-bold tracking-tight text-mist">
                Hire your first AI agent this week.
              </h3>
              <p className="mt-2 text-sm leading-relaxed text-dim">
                Clone a voice, connect your CRM, shadow your best human for three days — then take calls.
              </p>
            </div>
            <span className="mt-8 flex items-center gap-2 font-mono text-[11px] uppercase tracking-[0.2em] text-mint">
              start pilot
              <svg width="13" height="12" viewBox="0 0 13 12" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M1 6h10M7 1.5 11.5 6 7 10.5" /></svg>
            </span>
          </a>
        </div>
      </div>
    </section>
  );
}
