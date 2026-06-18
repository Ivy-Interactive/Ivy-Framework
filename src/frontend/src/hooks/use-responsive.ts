import { RefObject, useEffect, useState } from "react";

export interface Responsive<T> {
  default?: T;
  mobile?: T;
  tablet?: T;
  desktop?: T;
  wide?: T;
}

// Lower bounds of each band, in px. NOTE: `wide` is the open-ended top band — any
// width >= desktop (1024) resolves to "wide" (see getBreakpoint). The 1280 value is
// retained for reference only and is intentionally NOT used as a threshold.
const BREAKPOINTS = { mobile: 640, tablet: 768, desktop: 1024, wide: 1280 };

export type BreakpointName = "mobile" | "tablet" | "desktop" | "wide";

const RESIZE_DEBOUNCE_MS = 100;

/**
 * Resolves the current breakpoint from a width.
 *
 * Pass a `containerRef` to derive the breakpoint from that element's content-box
 * width (via ResizeObserver) instead of `window.innerWidth`. This is what makes
 * responsive resolution account for chrome that steals horizontal space — most
 * importantly an expanded sidebar, which shrinks the app content area without
 * changing the viewport width. When the ref is null/unset (or its element has not
 * measured yet) the hook falls back to the window width.
 */
export function useBreakpoint(containerRef?: RefObject<HTMLElement | null>): BreakpointName {
  const [bp, setBp] = useState<BreakpointName>(() => getBreakpoint(widthFromRef(containerRef)));

  useEffect(() => {
    let timeoutId: number | undefined;

    const measure = () => {
      setBp(getBreakpoint(widthFromRef(containerRef)));
    };

    const onResize = () => {
      if (timeoutId !== undefined) clearTimeout(timeoutId);
      timeoutId = window.setTimeout(measure, RESIZE_DEBOUNCE_MS);
    };

    window.addEventListener("resize", onResize);

    // Take an immediate reading: the useState initializer ran during the first render
    // pass before refs were attached, so it could only see the window width. Now that
    // the element exists we can measure it.
    measure();

    // When a container is supplied, observe it directly so sidebar open/close and
    // resize-drag (which change the content width but not the viewport) re-evaluate
    // the breakpoint.
    let observer: ResizeObserver | undefined;
    const el = containerRef?.current;
    if (el && typeof ResizeObserver !== "undefined") {
      observer = new ResizeObserver(onResize);
      observer.observe(el);
    }

    return () => {
      window.removeEventListener("resize", onResize);
      observer?.disconnect();
      if (timeoutId !== undefined) clearTimeout(timeoutId);
    };
  }, [containerRef]);

  return bp;
}

function widthFromRef(containerRef?: RefObject<HTMLElement | null>): number | undefined {
  const el = containerRef?.current;
  if (el && el.clientWidth > 0) return el.clientWidth;
  return undefined;
}

function getBreakpoint(width?: number): BreakpointName {
  const w = width !== undefined ? width : typeof window !== "undefined" ? window.innerWidth : 1024;
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
