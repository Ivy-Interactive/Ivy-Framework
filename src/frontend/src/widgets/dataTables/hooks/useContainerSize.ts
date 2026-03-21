import { useEffect, useRef, useState } from 'react';

/**
 * Tracks container dimensions. Uses dvn-scroller.clientHeight for scroll area
 * (excludes scrollbar, padding; correct across zoom/OS/browser). No magic offset.
 */
export const useContainerSize = () => {
  const containerRef = useRef<HTMLDivElement>(null);
  const [containerWidth, setContainerWidth] = useState<number>(0);
  const [containerHeight, setContainerHeight] = useState<number>(0);
  const [scrollAreaHeight, setScrollAreaHeight] = useState<number>(0);
  const timeoutRef = useRef<NodeJS.Timeout | null>(null);
  const lastWidthRef = useRef<number>(0);
  const lastHeightRef = useRef<number>(0);
  const hasAppliedInitialRef = useRef<boolean>(false);
  const scrollObserverRef = useRef<ResizeObserver | null>(null);

  useEffect(() => {
    if (!containerRef.current) return;

    const observeScrollArea = () => {
      const scroller = containerRef.current?.querySelector('.dvn-scroller');
      if (!scroller || scrollObserverRef.current) return;

      const update = () => setScrollAreaHeight(scroller.clientHeight);
      update();
      scrollObserverRef.current = new ResizeObserver(update);
      scrollObserverRef.current.observe(scroller);
    };

    const apply = (width: number, height: number) => {
      hasAppliedInitialRef.current = true;
      lastWidthRef.current = width;
      lastHeightRef.current = height;
      setContainerWidth(width);
      setContainerHeight(height);
      requestAnimationFrame(observeScrollArea);
    };

    const resizeObserver = new ResizeObserver(entries => {
      for (const entry of entries) {
        const { width, height } = entry.contentRect;
        const widthChanged = Math.abs(width - lastWidthRef.current) > 1;
        const heightChanged = Math.abs(height - lastHeightRef.current) > 1;

        if (widthChanged || heightChanged) {
          const isInitial = !hasAppliedInitialRef.current;
          if (isInitial) {
            // Defer to next frame so layout (e.g. tab/modal transition) can settle
            requestAnimationFrame(() => apply(width, height));
          } else {
            if (timeoutRef.current) clearTimeout(timeoutRef.current);
            timeoutRef.current = setTimeout(() => apply(width, height), 50);
          }
        }
      }
    });

    resizeObserver.observe(containerRef.current);
    requestAnimationFrame(observeScrollArea);

    return () => {
      if (timeoutRef.current) clearTimeout(timeoutRef.current);
      scrollObserverRef.current?.disconnect();
      scrollObserverRef.current = null;
      resizeObserver.disconnect();
    };
  }, []);

  return {
    containerRef,
    containerWidth,
    containerHeight,
    scrollContainerHeight:
      scrollAreaHeight > 0 ? scrollAreaHeight : containerHeight,
  };
};
