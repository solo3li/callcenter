const INTEGRATIONS_A = ["Twilio", "Amazon Connect", "Genesys", "Salesforce", "Zendesk", "HubSpot", "Intercom", "Five9"];
const INTEGRATIONS_B = ["Slack", "Teams", "Vonage", "Talkdesk", "Stripe", "Shopify", "SIP trunks", "Webhooks"];

function Glyph({ i }: { i: number }) {
  const shapes = [
    <circle key="c" cx="6" cy="6" r="4.4" />,
    <rect key="r" x="1.8" y="1.8" width="8.4" height="8.4" />,
    <path key="d" d="M6 1.2 10.8 6 6 10.8 1.2 6Z" />,
    <path key="t" d="M6 1.4 11 10.4H1Z" />,
  ];
  return (
    <svg width="12" height="12" viewBox="0 0 12 12" fill="none" stroke="currentColor" strokeWidth="1.5" aria-hidden="true">
      {shapes[i % shapes.length]}
    </svg>
  );
}

function IntegrationRow({ items, reverse }: { items: string[]; reverse?: boolean }) {
  const row = (keyPrefix: string) =>
    items.map((name, i) => (
      <span key={`${keyPrefix}-${name}`} className="flex items-center">
        <span className="mx-4 flex items-center gap-2.5 border border-line bg-panel/60 px-5 py-2.5 font-mono text-[11px] uppercase tracking-[0.18em] text-dim transition-colors duration-300 hover:border-mint/50 hover:text-mist">
          <span className="text-mint/70"><Glyph i={i} /></span>
          {name}
        </span>
      </span>
    ));
  return (
    <div className="marquee">
      <div className={`marquee-track ${reverse ? "rev" : ""}`} style={{ animationDuration: reverse ? "46s" : "38s" }}>
        {row("a")}
        {row("b")}
      </div>
    </div>
  );
}

const QUOTES = [
  {
    quote:
      "We cut cost-per-resolution 61% in the first quarter — and CSAT went up. I still don't fully believe the chart.",
    name: "Renata Okafor",
    role: "VP Customer Care, Loop Mobile",
    photo: "https://images.pexels.com/photos/33680700/pexels-photo-33680700.jpeg?auto=compress&cs=tinysrgb&fit=crop&h=120&w=120",
    big: true,
  },
  {
    quote:
      "The handoff is the product. Calls arrive at my agents with their whole story attached — nobody says “can you repeat that?” anymore.",
    name: "Daniel Ferreira",
    role: "Head of Support, Fretboard",
    photo: "https://images.pexels.com/photos/38740728/pexels-photo-38740728.jpeg?auto=compress&cs=tinysrgb&fit=crop&h=120&w=120",
  },
  {
    quote: "Noor closes more renewals at 3 a.m. than the day shift does at 3 p.m.",
    name: "Sam Whitaker",
    role: "COO, Parcelry",
    photo: "https://images.pexels.com/photos/1546912/pexels-photo-1546912.jpeg?auto=compress&cs=tinysrgb&fit=crop&h=120&w=120",
  },
];

export default function Social() {
  return (
    <section className="relative overflow-hidden py-24 lg:py-28">
      <div className="mx-auto mb-14 max-w-7xl px-5 lg:px-8" data-reveal>
        <p className="kicker mb-4">// plugs into your stack</p>
        <h2 className="font-display text-[clamp(2rem,4.5vw,3.4rem)] font-bold leading-[1.05] tracking-tight text-mist">
          Answers where your phones already live.
        </h2>
      </div>

      <div className="space-y-3" data-reveal style={{ "--d": "150ms" } as React.CSSProperties}>
        <IntegrationRow items={INTEGRATIONS_A} />
        <IntegrationRow items={INTEGRATIONS_B} reverse />
      </div>

      {/* testimonials */}
      <div className="mx-auto mt-24 grid max-w-7xl gap-6 px-5 lg:grid-cols-[1.15fr_0.85fr] lg:px-8">
        {QUOTES.filter((q) => q.big).map((q) => (
          <figure
            key={q.name}
            data-reveal
            className="lift relative flex flex-col justify-between border border-line bg-panel/70 p-8 sm:p-12"
          >
            <svg width="44" height="34" viewBox="0 0 44 34" fill="var(--color-mint)" opacity="0.35" aria-hidden="true">
              <path d="M0 34V20.4C0 8.9 6.6 1.6 18 0l2.2 5.4C12.8 7 9.2 11.3 9 16.6H19V34H0Zm25 0V20.4C25 8.9 31.6 1.6 43 0l2 5.4C37.6 7 34 11.3 33.8 16.6H44V34H25Z" transform="scale(0.95)" />
            </svg>
            <blockquote className="mt-6 font-display text-2xl font-semibold leading-snug tracking-tight text-mist sm:text-[2rem]">
              “{q.quote}”
            </blockquote>
            <figcaption className="mt-8 flex items-center gap-4">
              <img src={q.photo} alt={q.name} loading="lazy" className="h-12 w-12 rounded-full border border-line object-cover grayscale-[30%]" />
              <div>
                <p className="text-sm font-semibold text-mist">{q.name}</p>
                <p className="font-mono text-[10px] uppercase tracking-[0.16em] text-dim">{q.role}</p>
              </div>
            </figcaption>
          </figure>
        ))}
        <div className="flex flex-col gap-6">
          {QUOTES.filter((q) => !q.big).map((q, i) => (
            <figure
              key={q.name}
              data-reveal
              style={{ "--d": `${150 + i * 120}ms` } as React.CSSProperties}
              className={`lift flex flex-1 flex-col justify-between border border-line bg-deep/70 p-7 ${
                i === 0 ? "hover:border-amber/50" : "hover:border-mint/50"
              }`}
            >
              <blockquote className="font-display text-lg font-medium leading-snug text-mist">“{q.quote}”</blockquote>
              <figcaption className="mt-6 flex items-center gap-3">
                <img src={q.photo} alt={q.name} loading="lazy" className="h-10 w-10 rounded-full border border-line object-cover grayscale-[30%]" />
                <div>
                  <p className="text-sm font-semibold text-mist">{q.name}</p>
                  <p className="font-mono text-[10px] uppercase tracking-[0.16em] text-dim">{q.role}</p>
                </div>
              </figcaption>
            </figure>
          ))}
        </div>
      </div>
    </section>
  );
}
