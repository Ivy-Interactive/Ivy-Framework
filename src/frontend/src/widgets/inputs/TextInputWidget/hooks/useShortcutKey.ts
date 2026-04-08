import React from "react";
import { useShortcut } from "@/lib/useShortcut";

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
  useShortcut(id, shortcutKey, () => {
    if (inputRef.current) {
      inputRef.current.focus();
      setIsFocused(true);
      if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
    }
  });
};
