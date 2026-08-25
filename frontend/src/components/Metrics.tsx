import { useCountUp, useInView } from "../hooks";

const PHOTO =
  "https://images.pexels.com/photos/8866725/pexels-photo-8866725.jpeg?auto=compress&cs=tinysrgb&fit=crop&h=900&w=760";

function Stat({
  value,
  decimals = 0,
  prefix = "",
  suffix = "",
  label,
  tone,
  delay,
}: {
  value: number;
  decimals?: number;
  prefix?: string;
  suffix?: string;
  label: string;
  tone?: "mint" | "amber";
  delay: number;
}) {
  const { ref, inView } = useInView<HTMLDivElement>(0.4);
  const display = useCountUp(value, { decimals, start: inView, duration: 1600 });
  const color = tone === "amber" ? "var(--color-amber)" : "var(--color-mint)";
  return (
    <div ref={ref} data-reveal style={{ "--d": `${delay}ms` } as React.CSSProperties} className="border-t border-line py-6 pr-4">
      <p className="font-display text-5xl font-bold tabular-nums tracking-tight text-mist">
        <span style={{ color }}>{prefix}</span>
        {display}
        <span className="text-2xl font-semibold text-dim">{suffix}</span>
      </p>
      <p className="mt-2 font-mono text-[10px] uppercase tracking-[0.2em] text-dim">{label}</p>
    </div>
  );
}

const SPARK_POINTS =
  "0,86 60,78 120,80 180,66 240,70 300,54 360,58 420,42 480,46 540,30 600,34 660,20 720,24 780,12";

export default function Metrics() {
  const { ref, inView } = useInView<HTMLDivElement>(0.25);

  return (
    <section id="proof" className="relative scroll-mt-24 border-t border-line bg-deep/40 px-5 py-24 lg:px-8 lg:py-32">
      <div className="mx-auto max-w-7xl">
        <div className="grid gap-12 lg:grid-cols-[0.9fr_1.1fr] lg:gap-16">
          {/* photo */}
          <div data-reveal className="relative">
            <div className="relative h-[420px] overflow-hidden border border-line lg:h-[560px]">
              <img
                src={PHOTO}
                alt="A support floor running on Tandem at night"
                loading="lazy"
                className="kenburns h-full w-full object-cover opacity-80 saturate-[0.75]"
              />
              <div className="absolute inset-0 bg-gradient-to-t from-ink via-ink/25 to-transparent" />
              <div className="absolute inset-x-0 bottom-0 flex items-center justify-between gap-4 p-5">
                <p className="font-mono text-[10px] uppercase leading-relaxed tracking-[0.18em] text-mist/80">
                  floor 4 · 02:14 am — noor handling
                  <br />
                  eleven calls. zero humans woken.
                </p>
                <span className="chip !border-mint/50 !text-mint">live shift</span>
              </div>
            </div>
            <div className="floaty absolute -right-3 -top-4 hidden border border-amber/50 bg-ink px-3 py-2 font-mono text-[10px] uppercase tracking-[0.18em] text-amber sm:block">
              csat 4.7★ this week
            </div>
          </div>

          {/* stats */}
          <div>
            <div data-reveal>
              <p className="kicker mb-4">// proof from the floor</p>
              <h2 className="font-display text-[clamp(2rem,4.5vw,3.4rem)] font-bold leading-[1.05] tracking-tight text-mist">
                Numbers your night shift<br />can verify.
              </h2>
            </div>
            <div className="mt-10 grid grid-cols-2 gap-x-8 sm:grid-cols-3">
              <Stat value={2.4} decimals={1} suffix="M" label="calls / month" delay={0} />
              <Stat value={94} suffix="%" label="auto-resolved" delay={80} />
              <Stat value={0.8} decimals={1} suffix="s" label="median pickup" delay={160} />
              <Stat value={182} suffix="ms" label="p50 inference" delay={240} tone="amber" />
              <Stat value={4.7} decimals={1} suffix="★" label="csat, trailing 90d" delay={320} tone="amber" />
              <Stat value={61} prefix="−" suffix="%" label="cost / resolution" delay={400} />
            </div>

            {/* sparkline */}
            <div ref={ref} data-reveal style={{ "--d": "200ms" } as React.CSSProperties} className="mt-10 border border-line bg-panel/60 p-5">
              <div className="mb-3 flex items-center justify-between">
                <span className="font-mono text-[10px] uppercase tracking-[0.2em] text-dim">
                  resolutions per hour · last 14 days
                </span>
                <span className={`font-mono text-[10px] uppercase tracking-[0.14em] ${inView ? "text-mint" : "text-dim"}`}>
                  ▲ 38%
                </span>
              </div>
              <svg viewBox="0 0 780 100" className="h-20 w-full" preserveAspectRatio="none" aria-hidden="true">
                <defs>
                  <linearGradient id="sparkfill" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="var(--color-mint)" stopOpacity="0.28" />
                    <stop offset="100%" stopColor="var(--color-mint)" stopOpacity="0" />
                  </linearGradient>
                </defs>
                <path d={`M${SPARK_POINTS.replace(/ /g, " L")} L780,100 L0,100 Z`} fill="url(#sparkfill)" />
                <path
                  d={`M${SPARK_POINTS.replace(/ /g, " L")}`}
                  fill="none"
                  stroke="var(--color-mint)"
                  strokeWidth="2"
                  className="spark-draw"
                />
              </svg>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
