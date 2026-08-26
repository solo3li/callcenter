import { NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

const SECTIONS = [
  {
    label: "operations",
    links: [
      { to: "/dashboard/live", label: "Live Wallboard", icon: "â–£" },
      { to: "/dashboard/queue", label: "Queue", icon: "â—" },
      { to: "/dashboard/roster", label: "Agent Roster", icon: "â–¥" },
      { to: "/dashboard/analytics", label: "Analytics", icon: "â–¦" },
      { to: "/dashboard/history", label: "Call History", icon: "â–¤" },
    ],
  },
  {
    label: "platform setup",
    links: [
      { to: "/dashboard/configs", label: "Call Configs", icon: "â—ˆ" },
      { to: "/dashboard/personas", label: "AI Personas", icon: "â—‰" },
      { to: "/dashboard/workflows", label: "Workflows", icon: "â—‡" },
      { to: "/dashboard/knowledge", label: "Knowledge Bases", icon: "â" },
      { to: "/dashboard/sip-destinations", label: "SIP Destinations", icon: "☎" },
    ],
  },
  {
    label: "business",
    links: [
      { to: "/dashboard/usage", label: "Usage & Metering", icon: "â–¤" },
      { to: "/dashboard/api-keys", label: "API Keys", icon: "âš¿" },
      { to: "/dashboard/agents-admin", label: "Human Agents", icon: "â˜º" },
      { to: "/dashboard/business", label: "Licenses & Partners", icon: "â–¦" },
    ],
  },
];

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `flex items-center gap-3 px-4 py-2 font-mono text-[10px] uppercase tracking-[0.14em] transition-all duration-200 ${
    isActive
      ? "border-l-2 border-mint bg-mint/8 text-mint"
      : "border-l-2 border-transparent text-dim hover:text-mist hover:border-line"
  }`;

export default function DashboardSidebar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login", { replace: true });
  };

  return (
    <aside className="fixed inset-y-0 left-0 z-40 flex w-[220px] flex-col overflow-y-auto border-r border-line bg-deep">
      <div className="flex h-16 shrink-0 items-center gap-2.5 border-b border-line px-5">
        <svg width="24" height="24" viewBox="0 0 32 32" aria-hidden="true">
          <circle cx="12" cy="16" r="8.5" fill="none" stroke="var(--color-mint)" strokeWidth="2.6" />
          <circle cx="20.5" cy="16" r="8.5" fill="none" stroke="var(--color-amber)" strokeWidth="2.6" opacity="0.85" />
        </svg>
        <span className="font-display text-lg font-bold tracking-tight text-mist">Tandem</span>
      </div>

      <nav className="flex-1 space-y-4 py-4">
        {SECTIONS.map((section) => (
          <div key={section.label}>
            <p className="mb-1 px-4 font-mono text-[8px] uppercase tracking-[0.24em] text-dim/60">
              {section.label}
            </p>
            {section.links.map((l) => (
              <NavLink key={l.to} to={l.to} className={linkClass}>
                <span className="text-xs">{l.icon}</span>
                {l.label}
              </NavLink>
            ))}
          </div>
        ))}
      </nav>

      <div className="shrink-0 space-y-3 border-t border-line p-4">
        {user && (
          <div className="border border-line px-3 py-2.5">
            <p className="truncate font-display text-sm font-semibold text-mist">
              {user.displayName}
            </p>
            <p className="truncate font-mono text-[10px] text-dim">{user.email}</p>
          </div>
        )}
        <div className="flex items-center gap-2 border border-line px-3 py-2">
          <span className="pulse-dot h-1.5 w-1.5 rounded-full bg-mint" />
          <span className="font-mono text-[9px] uppercase tracking-[0.14em] text-dim">
            all systems online
          </span>
        </div>
        <button
          onClick={handleLogout}
          className="flex w-full items-center gap-2 border border-line px-3 py-2 font-mono text-[10px] uppercase tracking-[0.14em] text-dim transition-colors hover:border-coral/50 hover:text-coral"
        >
          <svg width="11" height="11" viewBox="0 0 12 12" fill="none" stroke="currentColor" strokeWidth="1.6">
            <path d="M4.5 1H1v10h3.5M8 8l3-2-3-2M11 6H4.5" />
          </svg>
          sign out
        </button>
        <NavLink
          to="/"
          className="flex items-center gap-2 font-mono text-[10px] uppercase tracking-[0.14em] text-dim transition-colors hover:text-mist"
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

