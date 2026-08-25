type BadgeTone = "mint" | "amber" | "coral" | "dim";

const TONE_CLASSES: Record<BadgeTone, string> = {
  mint: "!border-mint/50 !text-mint",
  amber: "!border-amber/50 !text-amber",
  coral: "!border-coral/50 !text-coral",
  dim: "!border-line !text-dim",
};

export default function Badge({ label, tone = "dim" }: { label: string; tone?: BadgeTone }) {
  return <span className={`chip ${TONE_CLASSES[tone]}`}>{label}</span>;
}