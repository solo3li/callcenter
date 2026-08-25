import { useEffect, useRef, useState } from "react";

/** prefers-reduced-motion */
export function usePRM(): boolean {
  const [prm, setPrm] = useState<boolean>(() =>
    typeof window !== "undefined"
      ? window.matchMedia("(prefers-reduced-motion: reduce)").matches
      : false
  );
  useEffect(() => {
    const mq = window.matchMedia("(prefers-reduced-motion: reduce)");
    const fn = () => setPrm(mq.matches);
    mq.addEventListener("change", fn);
    return () => mq.removeEventListener("change", fn);
  }, []);
  return prm;
}

/** Observe every [data-reveal] element once; add `.in` when visible. */
export function useRevealAll() {
  useEffect(() => {
    const els = Array.from(document.querySelectorAll<HTMLElement>("[data-reveal]"));
    if (!("IntersectionObserver" in window)) {
      els.forEach((el) => el.classList.add("in"));
      return;
    }
    const io = new IntersectionObserver(
      (entries) => {
        for (const e of entries) {
          if (e.isIntersecting) {
            e.target.classList.add("in");
            io.unobserve(e.target);
          }
        }
      },
      { threshold: 0.12, rootMargin: "0px 0px -6% 0px" }
    );
    els.forEach((el) => io.observe(el));
    return () => io.disconnect();
  }, []);
}

/** One-shot in-view flag for a ref. */
export function useInView<T extends Element>(threshold = 0.3) {
  const ref = useRef<T | null>(null);
  const [inView, setInView] = useState(false);
  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    if (!("IntersectionObserver" in window)) {
      setInView(true);
      return;
    }
    const io = new IntersectionObserver(
      (entries) => {
        if (entries.some((e) => e.isIntersecting)) {
          setInView(true);
          io.disconnect();
        }
      },
      { threshold }
    );
    io.observe(el);
    return () => io.disconnect();
  }, [threshold]);
  return { ref, inView };
}

const GLYPHS = "#%&$@/<>[]{}=+*0123456789";

/** Scramble-decode a string on mount. */
export function useScramble(text: string, delay = 0): string {
  const prm = usePRM();
  const [out, setOut] = useState(prm ? text : "\u00a0");
  useEffect(() => {
    if (prm) {
      setOut(text);
      return;
    }
    let raf = 0;
    let frame = 0;
    const total = text.length;
    const tick = () => {
      frame += 1;
      const revealed = Math.floor(frame / 2.2);
      let s = "";
      for (let i = 0; i < total; i++) {
        const c = text[i];
        if (c === " ") {
          s += c;
          continue;
        }
        s += i < revealed ? c : GLYPHS[(i * 7 + frame * 3) % GLYPHS.length];
      }
      setOut(s);
      if (revealed < total) raf = requestAnimationFrame(tick);
    };
    const t = window.setTimeout(() => {
      raf = requestAnimationFrame(tick);
    }, delay);
    return () => {
      window.clearTimeout(t);
      cancelAnimationFrame(raf);
    };
  }, [text, delay, prm]);
  return out;
}

/** Animated count-up once `start` is true. Returns formatted string. */
export function useCountUp(
  target: number,
  opts: { duration?: number; decimals?: number; start?: boolean; suffix?: string } = {}
): string {
  const { duration = 1500, decimals = 0, start = true } = opts;
  const prm = usePRM();
  const [val, setVal] = useState(prm ? target : 0);
  useEffect(() => {
    if (!start) return;
    if (prm) {
      setVal(target);
      return;
    }
    let raf = 0;
    const t0 = performance.now();
    const step = (t: number) => {
      const p = Math.min(1, (t - t0) / duration);
      const eased = 1 - Math.pow(1 - p, 3);
      setVal(target * eased);
      if (p < 1) raf = requestAnimationFrame(step);
    };
    raf = requestAnimationFrame(step);
    return () => cancelAnimationFrame(raf);
  }, [target, start, duration, prm]);
  return val.toLocaleString("en-US", {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  });
}

/** Periodic value jitter (e.g. latency readout). */
export function useJitter(base: number, spread: number, ms: number): number {
  const prm = usePRM();
  const [v, setV] = useState(base);
  useEffect(() => {
    if (prm) {
      setV(base);
      return;
    }
    const id = window.setInterval(
      () => setV(Math.round(base + (Math.random() - 0.5) * 2 * spread)),
      ms
    );
    return () => window.clearInterval(id);
  }, [base, spread, ms, prm]);
  return v;
}
