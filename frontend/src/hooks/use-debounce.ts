import { useRef, useCallback } from 'react';

export function useDebounce<T extends (...args: unknown[]) => void>(
  fn: T,
  delay: number
) {
  const timeoutRef = useRef<number | undefined>(undefined);

  const debouncedFn = useCallback(
    (...args: Parameters<T>) => {
      if (timeoutRef.current !== undefined) {
        clearTimeout(timeoutRef.current);
      }
      timeoutRef.current = window.setTimeout(() => {
        fn(...args);
      }, delay);
    },
    [fn, delay]
  );

  return debouncedFn;
}
