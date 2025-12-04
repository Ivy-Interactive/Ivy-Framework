import { useEffect, useRef, useState } from 'react';

/**
 * Hook to track container width and height using ResizeObserver
 * Debounces updates to prevent unnecessary rescaling when sheets/modals open
 */
export const useContainerSize = () => {
  const containerRef = useRef<HTMLDivElement>(null);
  const [containerWidth, setContainerWidth] = useState<number>(0);
  const [containerHeight, setContainerHeight] = useState<number>(0);
  const timeoutRef = useRef<NodeJS.Timeout | null>(null);
  const lastWidthRef = useRef<number>(0);
  const lastHeightRef = useRef<number>(0);

  useEffect(() => {
    if (!containerRef.current) return;

    const resizeObserver = new ResizeObserver(entries => {
      for (const entry of entries) {
        const { width, height } = entry.contentRect;

        // Only update if size actually changed meaningfully (more than 1px difference)
        // This prevents rescaling when sheet opens/closes due to viewport changes
        const widthChanged = Math.abs(width - lastWidthRef.current) > 1;
        const heightChanged = Math.abs(height - lastHeightRef.current) > 1;

        if (widthChanged || heightChanged) {
          // Clear existing timeout
          if (timeoutRef.current) {
            clearTimeout(timeoutRef.current);
          }

          // Debounce updates to prevent rapid-fire rescaling
          timeoutRef.current = setTimeout(() => {
            lastWidthRef.current = width;
            lastHeightRef.current = height;
            setContainerWidth(width);
            setContainerHeight(height);
          }, 50); // 50ms debounce
        }
      }
    });

    resizeObserver.observe(containerRef.current);

    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
      resizeObserver.disconnect();
    };
  }, []);

  return {
    containerRef,
    containerWidth,
    containerHeight,
  };
};
