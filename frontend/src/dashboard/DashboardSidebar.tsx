import { NavLink } from "react-router-dom";

const LINKS = [
  { to: "/dashboard/live", label: "Live Wallboard", icon: "▣" },
  { to: "/dashboard/roster", label: "Agent Roster", icon: "▥" },
  { to: "/dashboard/analytics", label: "Analytics", icon: "▦" },
  { to: "/dashboard/history", label: "Call History", icon: "▤" },
];

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `flex items-center gap-3 px-4 py-2.5 font-mono text-[11px] uppercase tracking-[0.14em] transition-all duration-200 ${
    isActive
      ? "border-l-2 border-mint bg-mint/8 text-mint"
      : "border-l-2 border-transparent text-dim hover:text-mist hover:border-line"
  }`;

export default function DashboardSidebar() {
  return (
    <aside className="fixed inset-y-0 left-0 z-40 flex w-[220px] flex-col border-r border-line bg-deep">
      <div className="flex h-16 items-center gap-2.5 border-b border-line px-5">
        <svg width="24" height="24" viewBox="0 0 32 32" aria-hidden="true">
          <circle cx="12" cy="16" r="8.5" fill="none" stroke="var(--color-mint)" strokeWidth="2.6" />
          <circle cx="20.5" cy="16" r="8.5" fill="none" stroke="var(--color-amber)" strokeWidth="2.6" opacity="0.85" />
        </svg>
        <span className="font-display text-lg font-bold tracking-tight text-mist">Tandem</span>
      </div>

      <nav className="flex-1 space-y-0.5 py-4">
        {LINKS.map((l) => (
          <NavLink key={l.to} to={l.to} end className={linkClass}>
            <span className="text-sm">{l.icon}</span>
            {l.label}
          </NavLink>
        ))}
      </nav>

      <div className="border-t border-line p-4">
        <div className="flex items-center gap-2 border border-line px-3 py-2">
          <span className="pulse-dot h-1.5 w-1.5 rounded-full bg-mint" />
          <span className="font-mono text-[9px] uppercase tracking-[0.14em] text-dim">
            all systems online
          </span>
        </div>
        <NavLink
          to="/"
          className="mt-3 flex items-center gap-2 font-mono text-[10px] uppercase tracking-[0.14em] text-dim transition-colors hover:text-mist"
        >
          <svg width="10" height="10" viewBox="0 0 12 12" fill="none" stroke="currentColor" strokeWidth="1.8">
            <path d="M11 1H1m3 3-3-3 3-3" />
          </svg>
          back to site
        </NavLink>
      </div>
    </aside>
  );
}