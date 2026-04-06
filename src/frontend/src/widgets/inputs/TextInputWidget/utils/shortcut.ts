import React from "react";

/** Detects if the current platform is Mac/iOS */
export const isMac =
  typeof navigator !== "undefined" && /Mac|iPod|iPhone|iPad/.test(navigator.userAgent);

export interface ParsedShortcut {
  ctrl: boolean;
  shift: boolean;
  alt: boolean;
  meta: boolean;
  key: string;
}

/**
 * Parses a shortcut string (e.g., "Ctrl+Shift+K") into its component parts.
 * Handles platform-specific modifier key mappings.
 */
export const parseShortcut = (shortcutStr?: string): ParsedShortcut | null => {
  if (!shortcutStr) return null;
  const parts = shortcutStr.toLowerCase().split("+");
  return {
    ctrl: !isMac && parts.includes("ctrl"),
    shift: parts.includes("shift"),
    alt: parts.includes("alt"),
    meta: isMac
      ? parts.includes("ctrl") ||
        parts.includes("meta") ||
        parts.includes("cmd") ||
        parts.includes("command")
      : false,
    key: parts[parts.length - 1],
  };
};

/**
 * Formats a shortcut string for display as React nodes.
 * Converts modifier keys to platform-appropriate symbols (e.g., ⌘ on Mac).
 */
/**
 * Maps a key name to a KeyboardEvent.code value.
 * Uses event.code for matching so modifiers like Alt don't produce special characters on Mac.
 */
export const keyToCode = (key: string): string => {
  const k = key.toLowerCase();
  // Single letter keys: a-z → KeyA-KeyZ
  if (k.length === 1 && k >= "a" && k <= "z") {
    return `Key${k.toUpperCase()}`;
  }
  // Single digit keys: 0-9 → Digit0-Digit9
  if (k.length === 1 && k >= "0" && k <= "9") {
    return `Digit${k}`;
  }
  // Special keys
  const specialKeys: Record<string, string> = {
    enter: "Enter",
    return: "Enter",
    delete: "Delete",
    backspace: "Backspace",
    space: "Space",
    " ": "Space",
    tab: "Tab",
    escape: "Escape",
    esc: "Escape",
    arrowup: "ArrowUp",
    arrowdown: "ArrowDown",
    arrowleft: "ArrowLeft",
    arrowright: "ArrowRight",
    home: "Home",
    end: "End",
    pageup: "PageUp",
    pagedown: "PageDown",
    insert: "Insert",
  };
  // F-keys: f1-f12
  const fKeyMatch = k.match(/^f(\d{1,2})$/);
  if (fKeyMatch) {
    return `F${fKeyMatch[1]}`;
  }
  return specialKeys[k] ?? key;
};

export const formatShortcutForDisplay = (shortcutStr?: string): React.ReactNode[] => {
  if (!shortcutStr) return [];
  const parts = shortcutStr.split("+").map((p) => p.trim());
  const result: React.ReactNode[] = [];

  parts.forEach((part, index) => {
    if (index > 0) {
      result.push("+");
    }

    if (
      isMac &&
      (part.toLowerCase() === "ctrl" ||
        part.toLowerCase() === "cmd" ||
        part.toLowerCase() === "command" ||
        part.toLowerCase() === "meta")
    ) {
      result.push(
        React.createElement(
          "span",
          {
            key: `meta-${index}`,
            className: "inline-flex items-center justify-center",
          },
          "⌘",
        ),
      );
    } else if (!isMac && part.toLowerCase() === "ctrl") {
      result.push("Ctrl");
    } else {
      result.push(part.charAt(0).toUpperCase() + part.slice(1));
    }
  });

  return result;
};
