import { Outlet } from "react-router-dom";
import DashboardSidebar from "./DashboardSidebar";

export default function DashboardLayout() {
  return (
    <div className="min-h-screen bg-ink">
      <div className="bg-grid" aria-hidden="true" />
      <div className="bg-tint" aria-hidden="true" />

      <DashboardSidebar />

      <div className="ml-[220px]">
        <header className="sticky top-0 z-30 flex h-16 items-center justify-between border-b border-line bg-ink/85 backdrop-blur-md px-6">
          <div>
            <span className="font-mono text-[10px] uppercase tracking-[0.2em] text-dim">dashboard</span>
          </div>
          <div className="flex items-center gap-4">
            <span className="font-mono text-[10px] uppercase tracking-[0.14em] text-dim">
              {new Date().toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit", second: "2-digit", timeZoneName: "short" })}
            </span>
            <span className="flex items-center gap-1.5">
              <span className="h-1.5 w-1.5 rounded-full bg-mint" />
              <span className="font-mono text-[9px] uppercase tracking-[0.14em] text-mint">live</span>
            </span>
          </div>
        </header>

        <main className="p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}