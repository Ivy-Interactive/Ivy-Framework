import { useCallback, useLayoutEffect, useRef, useState } from "react";
import { useCurrentBreakpoint } from "@/hooks/use-breakpoint-context";

const RESIZE_DEBOUNCE_MS = 100;

/**
 * Tracks container dimensions for the glide grid.
 * Uses layout measurement + window resize (same pattern as useBreakpoint) — no ResizeObserver.
 */
export const useContainerSize = () => {
  const containerRef = useRef<HTMLDivElement>(null);
  const [containerWidth, setContainerWidth] = useState(0);
  const [containerHeight, setContainerHeight] = useState(0);
  const [scrollAreaHeight, setScrollAreaHeight] = useState(0);
  const lastWidthRef = useRef(0);
  const lastHeightRef = useRef(0);
  const breakpoint = useCurrentBreakpoint();

  const measure = useCallback(() => {
    const el = containerRef.current;
    if (!el) return;

    const width = el.clientWidth;
    const height = el.clientHeight;
    if (width <= 0 && height <= 0) return;

    const widthChanged = Math.abs(width - lastWidthRef.current) > 1;
    const heightChanged = Math.abs(height - lastHeightRef.current) > 1;
    if (!widthChanged && !heightChanged) return;

    lastWidthRef.current = width;
    lastHeightRef.current = height;
    setContainerWidth(width);
    setContainerHeight(height);

    const scroller = el.querySelector(".dvn-scroller");
    if (scroller instanceof HTMLElement && scroller.clientHeight > 0) {
      setScrollAreaHeight(scroller.clientHeight);
    } else if (height > 0) {
      setScrollAreaHeight(height);
    }
  }, []);

  useLayoutEffect(() => {
    measure();

    let rafId = 0;
    let retries = 0;
    const tryInit = () => {
      measure();
      const el = containerRef.current;
      if (el && el.clientHeight === 0 && retries++ < 10) {
        rafId = requestAnimationFrame(tryInit);
      }
    };
    rafId = requestAnimationFrame(tryInit);

    let resizeTimeoutId: number | undefined;
    const onWindowResize = () => {
      if (resizeTimeoutId !== undefined) clearTimeout(resizeTimeoutId);
      resizeTimeoutId = window.setTimeout(() => {
        requestAnimationFrame(measure);
      }, RESIZE_DEBOUNCE_MS);
    };
    window.addEventListener("resize", onWindowResize);

    return () => {
      cancelAnimationFrame(rafId);
      window.removeEventListener("resize", onWindowResize);
      if (resizeTimeoutId !== undefined) clearTimeout(resizeTimeoutId);
    };
  }, [measure, breakpoint]);

  return {
    containerRef,
    containerWidth,
    containerHeight,
    scrollContainerHeight: scrollAreaHeight > 0 ? scrollAreaHeight : containerHeight,
  };
};
