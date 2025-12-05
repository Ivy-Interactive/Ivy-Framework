import { useEffect } from 'react';

/**
 * Syncs the local value with the server value when the input is not focused.
 * Uses queueMicrotask to batch the state update.
 */
export const useSyncServerValue = (
  serverValue: string | undefined,
  localValue: string | undefined,
  isFocused: boolean,
  setLocalValue: (value: string | undefined) => void
): void => {
  useEffect(() => {
    if (!isFocused && serverValue !== localValue) {
      queueMicrotask(() => setLocalValue(serverValue));
    }
  }, [serverValue, isFocused, localValue, setLocalValue]);
};
