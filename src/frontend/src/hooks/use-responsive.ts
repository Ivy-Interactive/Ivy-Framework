import { useEffect, useState } from "react";

export interface Responsive<T> {
  default?: T;
  mobile?: T;
  tablet?: T;
  desktop?: T;
  wide?: T;
}

const BREAKPOINTS = { mobile: 640, tablet: 768, desktop: 1024, wide: 1280 };

export type BreakpointName = "mobile" | "tablet" | "desktop" | "wide";

export function useBreakpoint(): BreakpointName {
  const [bp, setBp] = useState<BreakpointName>(() => getBreakpoint());

  useEffect(() => {
    let timeoutId: number | undefined;
    const onResize = () => {
      if (timeoutId !== undefined) clearTimeout(timeoutId);
      timeoutId = window.setTimeout(() => {
        setBp(getBreakpoint());
      }, 100);
    };
    window.addEventListener("resize", onResize);
    return () => {
      window.removeEventListener("resize", onResize);
      if (timeoutId !== undefined) clearTimeout(timeoutId);
    };
  }, []);

  return bp;
}

function getBreakpoint(): BreakpointName {
  const w = typeof window !== "undefined" ? window.innerWidth : 1024;
  if (w < BREAKPOINTS.mobile) return "mobile";
  if (w < BREAKPOINTS.tablet) return "tablet";
  if (w < BREAKPOINTS.desktop) return "desktop";
  return "wide";
}

/**
 * Check if a value is a responsive object (has breakpoint keys).
 */
function isResponsive<T>(value: unknown): value is Responsive<T> {
  if (value === undefined || value === null) return false;
  if (typeof value !== "object") return false;
  const obj = value as Record<string, unknown>;
  return (
    "mobile" in obj || "tablet" in obj || "desktop" in obj || "wide" in obj || "default" in obj
  );
}

/**
 * Resolve a responsive value with mobile-first cascading.
 * If the value is a plain T (not a Responsive object), returns it directly.
 */
export function resolveResponsive<T>(
  value: T | Responsive<T> | undefined,
  breakpoint: BreakpointName,
  fallback: T,
): T {
  if (value === undefined || value === null) return fallback;
  if (!isResponsive<T>(value)) {
    return value as T;
  }
  const r = value as Responsive<T>;
  const cascade = [r.default, r.mobile, r.tablet, r.desktop, r.wide];
  const idx = { mobile: 1, tablet: 2, desktop: 3, wide: 4 }[breakpoint];
  // Mobile-first: walk from current breakpoint down to find first defined value
  for (let i = idx; i >= 0; i--) {
    if (cascade[i] !== undefined && cascade[i] !== null) return cascade[i]!;
  }
  return fallback;
}
