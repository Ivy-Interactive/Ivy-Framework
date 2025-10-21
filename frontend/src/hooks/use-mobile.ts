import { useState, useEffect } from 'react';

const MOBILE_BREAKPOINT = 768;

export function useIsMobile(width?: number) {
  const mobileBreakpoint = width ?? MOBILE_BREAKPOINT;

  const [isMobile, setIsMobile] = useState<boolean | undefined>(() => {
    if (typeof window === 'undefined') return undefined;
    return window.innerWidth < mobileBreakpoint;
  });

  useEffect(() => {
    const mql = window.matchMedia(`(max-width: ${mobileBreakpoint - 1}px)`);
    const onChange = () => {
      setIsMobile(window.innerWidth < mobileBreakpoint);
    };
    mql.addEventListener('change', onChange);
    // Trigger onChange immediately to ensure state is in sync
    onChange();
    return () => mql.removeEventListener('change', onChange);
  }, [mobileBreakpoint]);

  return !!isMobile;
}
