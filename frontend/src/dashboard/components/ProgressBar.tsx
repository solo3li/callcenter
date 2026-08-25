export default function ProgressBar({
  value,
  max = 100,
  tone = "mint",
  label,
}: {
  value: number;
  max?: number;
  tone?: "mint" | "amber" | "coral";
  label?: string;
}) {
  const pct = Math.min(100, Math.max(0, (value / max) * 100));
  const colorMap = {
    mint: "var(--color-mint)",
    amber: "var(--color-amber)",
    coral: "var(--color-coral)",
  };

  return (
    <div className="w-full">
      {label && (
        <div className="mb-1 flex justify-between font-mono text-[9px] uppercase tracking-[0.14em] text-dim">
          <span>{label}</span>
          <span>{Math.round(pct)}%</span>
        </div>
      )}
      <div className="h-[3px] w-full bg-line">
        <div
          className="numflip h-full"
          style={{ width: `${pct}%`, backgroundColor: colorMap[tone] }}
        />
      </div>
    </div>
  );
}