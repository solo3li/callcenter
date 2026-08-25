import { useEffect, useState } from "react";

const LINKS = [
  { href: "#platform", label: "Platform" },
  { href: "#floor", label: "Live floor" },
  { href: "#agents", label: "Agents" },
  { href: "#proof", label: "Proof" },
  { href: "#pricing", label: "Pricing" },
  { href: "#faq", label: "FAQ" },
];

export function Logo({ size = 30 }: { size?: number }) {
  return (
    <a href="#top" className="flex items-center gap-2.5 group">
      <svg width={size} height={size} viewBox="0 0 32 32" aria-hidden="true">
        <circle
          cx="12.5"
          cy="16"
          r="8.5"
          fill="none"
          stroke="var(--color-mint)"
          strokeWidth="2.6"
          className="transition-transform duration-500 group-hover:-translate-x-[2px]"
        />
        <circle
          cx="20.5"
          cy="16"
          r="8.5"
          fill="none"
          stroke="var(--color-amber)"
          strokeWidth="2.6"
          opacity="0.85"
          className="transition-transform duration-500 group-hover:translate-x-[2px]"
        />
      </svg>
      <span className="font-display text-xl font-bold tracking-tight text-mist">
        Tandem
      </span>
      <span className="chip hidden sm:inline-block !text-[9px]">ai + human</span>
    </a>
  );
}

export default function Nav() {
  const [scrolled, setScrolled] = useState(false);
  useEffect(() => {
    const fn = () => setScrolled(window.scrollY > 24);
    fn();
    window.addEventListener("scroll", fn, { passive: true });
    return () => window.removeEventListener("scroll", fn);
  }, []);

  return (
    <header
      className={`fixed inset-x-0 top-0 z-50 transition-all duration-500 ${
        scrolled
          ? "border-b border-line bg-ink/85 backdrop-blur-md py-3"
          : "border-b border-transparent py-5"
      }`}
    >
      <div className="mx-auto flex max-w-7xl items-center justify-between px-5 lg:px-8">
        <Logo />
        <nav className="hidden items-center gap-7 lg:flex">
          {LINKS.map((l) => (
            <a
              key={l.href}
              href={l.href}
              className="font-mono text-[11px] uppercase tracking-[0.18em] text-dim transition-colors duration-300 hover:text-mint"
            >
              {l.label}
            </a>
          ))}
        </nav>
        <div className="flex items-center gap-3">
          <span className="hidden items-center gap-2 border border-line px-3 py-1.5 md:flex">
            <span className="pulse-dot h-1.5 w-1.5 rounded-full bg-mint" />
            <span className="font-mono text-[10px] uppercase tracking-[0.18em] text-dim">
              All lines open
            </span>
          </span>
          <a href="#demo" className="btn btn-amber !px-4 !py-2.5">
            Book a demo
          </a>
        </div>
      </div>
    </header>
  );
}
