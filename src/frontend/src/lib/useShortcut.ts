import { useEffect, useRef, type RefObject } from "react";
import { parseShortcut } from "./shortcut";
import { registerShortcut, unregisterShortcut } from "./shortcutRegistry";

/**
 * Registers a keyboard shortcut with the centralized shortcut registry.
 * Handles registration/unregistration lifecycle via useEffect.
 */
export function useShortcut(
  id: string,
  shortcutKey: string | undefined,
  handler: () => void,
  options?: {
    disabled?: boolean;
    skipInInputs?: boolean;
    elementRef?: RefObject<HTMLElement | null>;
  },
): void {
  const handlerRef = useRef(handler);
  handlerRef.current = handler;

  const disabled = options?.disabled ?? false;
  const skipInInputs = options?.skipInInputs ?? true;
  const elementRef = options?.elementRef;

  useEffect(() => {
    if (!shortcutKey || disabled) return;

    const shortcut = parseShortcut(shortcutKey);
    if (!shortcut) return;

    registerShortcut({
      id,
      shortcut,
      handler: () => handlerRef.current(),
      isActive: () => {
        if (!elementRef?.current) return true;
        return !elementRef.current.closest('[aria-hidden="true"]');
      },
      skipInInputs,
    });

    return () => {
      unregisterShortcut(id);
    };
  }, [id, shortcutKey, disabled, skipInInputs, elementRef]);
}
