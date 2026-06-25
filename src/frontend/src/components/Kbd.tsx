import { cn } from "@/lib/utils";
import React from "react";

// Keys shown as a symbol rather than their typed word. Only these few have a
// universally recognized glyph; everything else is rendered verbatim.
const KEY_SYMBOLS: Record<string, string> = {
  enter: "↵",
  return: "↵",
  backspace: "⌫",
};

/**
 * Normalizes a single key for display. Enter/Return/Backspace render as their symbol;
 * a lone letter is uppercased ("a" → "A"); everything else is left exactly as written
 * ("cmd" → "cmd", "⌘" → "⌘", "Ctrl" → "Ctrl").
 */
const labelForKey = (raw: string): string => {
  const key = raw.trim();
  const symbol = KEY_SYMBOLS[key.toLowerCase()];
  if (symbol) return symbol;
  return key.length === 1 ? key.toUpperCase() : key;
};

/**
 * Splits a shortcut string into its keys and joins them for display. When every key
 * is a single character (e.g. "⌘+⌥+N") the keys are shown side by side with no "+";
 * if any key is multi-character (e.g. "Ctrl+Shift+N") the keys are joined with " + ".
 */
const formatShortcut = (value: string): string => {
  const keys = value
    .split("+")
    .map((p) => p.trim())
    .filter((p) => p.length > 0)
    .map(labelForKey);
  if (keys.length === 0) return "";
  const allSingle = keys.every((k) => k.length === 1);
  // Single-char keys are separated by a thin space (U+2009); multi-char keys by "+".
  return allSingle ? keys.join("\u2009") : keys.join("+");
};

/**
 * Base styling for a key cap. The height is fixed; `box-border` keeps the border inside
 * the box so the cap stays exactly sized. A single-character cap is a perfect square; a
 * longer label keeps the height and grows wider.
 */
const keyCapBase =
  "box-border inline-flex h-4 min-w-4 items-center justify-center rounded-[0.25rem] px-1 text-[10px] leading-[0.5]";

/**
 * Color/fill styling for a key cap.
 * - `ghost`   → no background or border, just the text.
 * - `inherit` → adopts the surrounding text color (for colored surfaces such as a primary button).
 * - default   → the standard muted key-cap look.
 */
const keyCapColor = ({ inherit, ghost }: { inherit?: boolean; ghost?: boolean }) => {
  if (ghost) return "border-0 bg-transparent text-current";
  return inherit
    ? "border border-current/30 bg-transparent text-current"
    : "border border-border bg-muted/40 text-foreground";
};

/**
 * Displays a keyboard shortcut as text inside a single cap. Pass `keys` (a shortcut
 * string) or a string child; modifiers and keys are rendered verbatim (no icons or
 * symbol substitution). Set `ghost` to drop the background and border.
 */
export function Kbd({
  children,
  keys,
  ghost,
}: {
  children?: React.ReactNode;
  keys?: string;
  ghost?: boolean;
}) {
  // Prefer the keys string; otherwise fall back to a string child (legacy call sites).
  const shortcut = keys ?? (typeof children === "string" ? children : undefined);

  if (shortcut && shortcut.trim().length > 0) {
    return (
      <span className="inline-flex items-center align-middle">
        <kbd className={cn(keyCapBase, keyCapColor({ ghost }))}>{formatShortcut(shortcut)}</kbd>
      </span>
    );
  }

  // Non-string content (composed nodes) renders unchanged inside a single cap.
  return (
    <span className="inline-flex items-center align-middle">
      <kbd className={cn(keyCapBase, keyCapColor({ ghost }))}>{children}</kbd>
    </span>
  );
}

/**
 * Renders a shortcut string as text inside a single cap. Exposed for widgets (e.g.
 * Button) that already hold a shortcut string and want the same look. Set `inherit`
 * when rendering on a colored surface so the cap adopts its text color.
 */
export function ShortcutKeys({
  shortcut,
  className,
  inherit,
  ghost,
}: {
  shortcut: string;
  className?: string;
  inherit?: boolean;
  ghost?: boolean;
}) {
  const text = formatShortcut(shortcut);
  if (text.length === 0) return null;
  return (
    <span className={cn("inline-flex items-center align-middle", className)}>
      <kbd className={cn(keyCapBase, keyCapColor({ inherit, ghost }))}>{text}</kbd>
    </span>
  );
}
