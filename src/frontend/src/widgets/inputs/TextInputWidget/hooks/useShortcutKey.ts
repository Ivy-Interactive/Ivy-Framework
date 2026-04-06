import React, { useEffect } from "react";
import { parseShortcut, keyToCode } from "@/lib/shortcut";

interface UseShortcutKeyParams {
  shortcutKey: string | undefined;
  inputRef: React.RefObject<HTMLInputElement | HTMLTextAreaElement | null>;
  setIsFocused: (focused: boolean) => void;
  id: string;
  events: string[];
  eventHandler: (event: string, id: string, args: unknown[]) => void;
}

/**
 * Handles keyboard shortcut to focus the input element.
 * Listens for the configured shortcut key combination and focuses the input when triggered.
 */
export const useShortcutKey = ({
  shortcutKey,
  inputRef,
  setIsFocused,
  id,
  events,
  eventHandler,
}: UseShortcutKeyParams): void => {
  useEffect(() => {
    if (!shortcutKey) return;

    const shortcutObj = parseShortcut(shortcutKey);
    if (!shortcutObj) return;

    const handleKeyDown = (event: KeyboardEvent) => {
      const modifierMatch =
        (shortcutObj.meta && event.metaKey) ||
        (shortcutObj.ctrl && event.ctrlKey) ||
        (!shortcutObj.meta && !shortcutObj.ctrl && !event.metaKey && !event.ctrlKey);

      const expectedCode = keyToCode(shortcutObj.key);

      const isShortcutPressed =
        modifierMatch &&
        event.shiftKey === shortcutObj.shift &&
        event.altKey === shortcutObj.alt &&
        event.code === expectedCode;

      if (isShortcutPressed) {
        event.preventDefault();
        if (inputRef.current) {
          inputRef.current.focus();
          setIsFocused(true);
          if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
        }
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => {
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [shortcutKey, id, events, eventHandler, inputRef, setIsFocused]);
};
