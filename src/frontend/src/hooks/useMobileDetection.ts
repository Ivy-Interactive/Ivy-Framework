import { useEffect, useState } from "react";

export interface MobileDetectionResult {
  /** True if the device supports touch input */
  isTouchDevice: boolean;
  /** True if the device supports hover (has a precise pointer like a mouse) */
  hasHover: boolean;
  /** True if the device has a coarse pointer (touch or stylus) */
  hasCoarsePointer: boolean;
}

/**
 * Detects mobile/touch device capabilities using media queries and feature detection.
 *
 * @returns Object containing device capability flags
 *
 * @example
 * ```tsx
 * const { isTouchDevice, hasHover } = useMobileDetection();
 *
 * return (
 *   <div className={hasHover ? "show-on-hover" : "always-visible"}>
 *     Actions
 *   </div>
 * );
 * ```
 */
export function useMobileDetection(): MobileDetectionResult {
  const [result, setResult] = useState<MobileDetectionResult>(() => {
    // Initial detection (SSR-safe)
    if (typeof window === "undefined") {
      return {
        isTouchDevice: false,
        hasHover: true,
        hasCoarsePointer: false,
      };
    }

    return {
      isTouchDevice: "ontouchstart" in window || navigator.maxTouchPoints > 0,
      hasHover: window.matchMedia("(hover: hover)").matches,
      hasCoarsePointer: window.matchMedia("(pointer: coarse)").matches,
    };
  });

  useEffect(() => {
    if (typeof window === "undefined") return;

    const hoverQuery = window.matchMedia("(hover: hover)");
    const pointerQuery = window.matchMedia("(pointer: coarse)");

    const updateDetection = () => {
      setResult({
        isTouchDevice: "ontouchstart" in window || navigator.maxTouchPoints > 0,
        hasHover: hoverQuery.matches,
        hasCoarsePointer: pointerQuery.matches,
      });
    };

    // Listen for changes (e.g., when external monitor is connected/disconnected)
    hoverQuery.addEventListener("change", updateDetection);
    pointerQuery.addEventListener("change", updateDetection);

    return () => {
      hoverQuery.removeEventListener("change", updateDetection);
      pointerQuery.removeEventListener("change", updateDetection);
    };
  }, []);

  return result;
}
