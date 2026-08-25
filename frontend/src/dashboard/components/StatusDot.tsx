const STATUS_MAP = {
  online: { color: "var(--color-mint)", label: "online", pulse: true },
  busy: { color: "var(--color-amber)", label: "busy", pulse: true },
  break: { color: "var(--color-dim)", label: "break", pulse: false },
  offline: { color: "var(--color-coral)", label: "offline", pulse: false },
};

export default function StatusDot({
  status,
  size = 8,
}: {
  status: keyof typeof STATUS_MAP;
  size?: number;
}) {
  const s = STATUS_MAP[status];
  return (
    <span className="relative inline-flex h-2 w-2">
      <span
        className={`absolute inset-0 rounded-full ${s.pulse ? (status === "busy" ? "pulse-dot-amber" : "pulse-dot") : ""}`}
        style={{ width: size, height: size, backgroundColor: s.color }}
      />
    </span>
  );
}