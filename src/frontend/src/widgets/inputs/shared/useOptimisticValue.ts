import { useState, useEffect } from 'react';

/**
 * Generic optimistic value hook for input widgets.
 * Maintains local state for instant rendering, syncs server value when not active.
 *
 * @param serverValue - The value from server props
 * @param isActive - Whether the user is actively interacting (focused, open, dragging)
 * @param isEqual - Optional equality check (defaults to ===)
 * @returns [localValue, setLocalValue] - Use localValue for rendering, setLocalValue for user interactions
 */
export function useOptimisticValue<T>(
  serverValue: T,
  isActive: boolean,
  isEqual?: (a: T, b: T) => boolean
): [T, (value: T) => void] {
  const [localValue, setLocalValue] = useState<T>(serverValue);
  const eq = isEqual ?? ((a: T, b: T) => a === b);

  useEffect(() => {
    if (!isActive && !eq(serverValue, localValue)) {
      queueMicrotask(() => setLocalValue(serverValue));
    }
  }, [serverValue, isActive]); // eslint-disable-line react-hooks/exhaustive-deps

  return [localValue, setLocalValue];
}
