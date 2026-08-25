import { useCountUp, useInView } from "../../hooks";

export default function StatCard({
  value,
  decimals = 0,
  prefix = "",
  suffix = "",
  label,
  tone = "mint",
  sublabel,
  pulse = false,
}: {
  value: number;
  decimals?: number;
  prefix?: string;
  suffix?: string;
  label: string;
  tone?: "mint" | "amber" | "coral" | "dim";
  sublabel?: string;
  pulse?: boolean;
}) {
  const { ref, inView } = useInView<HTMLDivElement>(0.3);
  const display = useCountUp(value, { decimals, start: inView, duration: 1400 });

  const colorMap = {
    mint: "var(--color-mint)",
    amber: "var(--color-amber)",
    coral: "var(--color-coral)",
    dim: "var(--color-dim)",
  };

  return (
    <div ref={ref} className="border border-line bg-panel/60 p-5">
      <div className="flex items-center justify-between mb-1">
        <p className="font-mono text-[10px] uppercase tracking-[0.2em] text-dim">{label}</p>
        {pulse && (
          <span
            className="pulse-dot h-2 w-2 rounded-full"
            style={{ backgroundColor: colorMap[tone] }}
          />
        )}
      </div>
      <p className="font-display text-4xl font-bold tabular-nums tracking-tight text-mist">
        <span style={{ color: colorMap[tone] }}>{prefix}</span>
        {display}
        <span className="text-xl font-semibold text-dim">{suffix}</span>
      </p>
      {sublabel && (
        <p className="mt-1 font-mono text-[9px] uppercase tracking-[0.14em] text-dim">{sublabel}</p>
      )}
    </div>
  );
}