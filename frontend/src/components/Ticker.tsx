const ITEMS = [
  "median pickup 0.8s",
  "94% auto-resolved",
  "182ms p50 inference",
  "38 languages",
  "warm handoff < 1s",
  "csat 4.7 / 5",
  "soc 2 type ii",
  "hipaa ready",
  "barge-in capable",
  "zero hold music",
];

function Row() {
  return (
    <>
      {ITEMS.map((it) => (
        <span key={it} className="flex items-center">
          <span className="px-6 font-mono text-[11px] uppercase tracking-[0.22em] text-dim">
            {it}
          </span>
          <svg width="7" height="7" viewBox="0 0 8 8" className="text-mint/70" aria-hidden="true">
            <path d="M4 0 8 4 4 8 0 4Z" fill="currentColor" />
          </svg>
        </span>
      ))}
    </>
  );
}

export default function Ticker() {
  return (
    <div className="marquee border-y border-line bg-deep/60 py-3.5" aria-label="Platform metrics ticker">
      <div className="marquee-track">
        <Row />
        <Row />
      </div>
    </div>
  );
}
