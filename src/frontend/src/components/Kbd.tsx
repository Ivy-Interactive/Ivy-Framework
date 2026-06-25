import Icon from "@/components/Icon";
import { cn } from "@/lib/utils";
import { isMac } from "@/lib/shortcut";
import React from "react";

/**
 * A single key to render inside a <Kbd>. Either an icon (lucide name) or a text label.
 */
interface KeyToken {
  /** Lucide icon name to render for this key, when one is available. */
  icon?: string;
  /** Text label to render when no icon is used. */
  label: string;
  /** Accessible label, used as the title/aria for icon-only keys. */
  aria: string;
}

/**
 * Maps a normalized key name to an icon-based or text-based token.
 * Modifier and navigation keys resolve to platform-appropriate icons; everything
 * else falls back to an uppercased text label.
 */
const tokenForKey = (raw: string): KeyToken => {
  const key = raw.trim();
  const k = key.toLowerCase();

  // Platform modifiers. On macOS "Ctrl" is conventionally the Command key (⌘) — this
  // mirrors parseShortcut(), which maps ctrl→meta on Mac so the binding fires on ⌘.
  // Display and binding must agree, so render ctrl as ⌘ here too. On other platforms
  // only Shift has a conventional symbol, so the rest render as short text.
  if (k === "ctrl" || k === "control") {
    return isMac
      ? { icon: "Command", label: "⌘", aria: "Command" }
      : { label: "Ctrl", aria: "Control" };
  }
  if (k === "cmd" || k === "command" || k === "meta" || k === "win" || k === "super") {
    return isMac
      ? { icon: "Command", label: "⌘", aria: "Command" }
      : { label: "Win", aria: "Windows" };
  }
  if (k === "shift") {
    return { icon: "ArrowBigUp", label: "⇧", aria: "Shift" };
  }
  if (k === "alt" || k === "option") {
    return isMac ? { icon: "Option", label: "⌥", aria: "Option" } : { label: "Alt", aria: "Alt" };
  }

  // Navigation / editing keys
  const iconKeys: Record<string, { icon: string; aria: string }> = {
    enter: { icon: "CornerDownLeft", aria: "Enter" },
    return: { icon: "CornerDownLeft", aria: "Enter" },
    backspace: { icon: "Delete", aria: "Backspace" },
    arrowup: { icon: "ArrowUp", aria: "Arrow Up" },
    up: { icon: "ArrowUp", aria: "Arrow Up" },
    arrowdown: { icon: "ArrowDown", aria: "Arrow Down" },
    down: { icon: "ArrowDown", aria: "Arrow Down" },
    arrowleft: { icon: "ArrowLeft", aria: "Arrow Left" },
    left: { icon: "ArrowLeft", aria: "Arrow Left" },
    arrowright: { icon: "ArrowRight", aria: "Arrow Right" },
    right: { icon: "ArrowRight", aria: "Arrow Right" },
  };
  if (k in iconKeys) {
    const { icon, aria } = iconKeys[k];
    return { icon, label: aria, aria };
  }

  // Text-only special keys (rendered as words, not single chars)
  const textKeys: Record<string, string> = {
    esc: "Esc",
    escape: "Esc",
    tab: "Tab",
    space: "Space",
    delete: "Del",
    del: "Del",
    home: "Home",
    end: "End",
    pageup: "PgUp",
    pagedown: "PgDn",
    insert: "Ins",
  };
  if (k in textKeys) {
    return { label: textKeys[k], aria: textKeys[k] };
  }

  // Single character → uppercase; longer tokens keep their casing capitalized.
  const label = key.length === 1 ? key.toUpperCase() : key.charAt(0).toUpperCase() + key.slice(1);
  return { label, aria: label };
};

/**
 * Base styling for a key cap. The height is fixed; `box-border` keeps the border inside
 * the box so caps stay exactly sized. A cap holding a single glyph, letter, or icon is a
 * perfect square (see `keyCapShape`); multi-character labels keep the height and grow wider.
 */
const keyCapBase =
  "box-border inline-flex h-4 items-center justify-center rounded-[0.25rem] text-[10px] font-semibold leading-none";

/**
 * Width/padding for a key cap. A token that renders an icon or a single character is a
 * fixed square (width == height, no horizontal padding); a multi-character text label
 * grows wider with the height as its minimum and small horizontal padding.
 */
const keyCapShape = (token: KeyToken): string =>
  token.icon || token.label.length <= 1 ? "w-4" : "min-w-4 px-1";

/**
 * Color/fill styling for a key cap.
 * - `ghost`   → no background or border, just the glyph.
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
 * Renders a single standalone key cap.
 */
const KeyCap: React.FC<{
  token: KeyToken;
  inherit?: boolean;
  ghost?: boolean;
}> = ({ token, inherit, ghost }) => (
  <kbd
    title={token.icon ? token.aria : undefined}
    aria-label={token.icon ? token.aria : undefined}
    className={cn(keyCapBase, keyCapShape(token), keyCapColor({ inherit, ghost }))}
  >
    {token.icon ? (
      // `!size-2.5` (10px) matches the letter caps' font size so icons and letters
      // render at equal size. The `!` overrides the `[&_svg]:size-4` rule that container
      // widgets such as Button apply to every descendant svg.
      <Icon name={token.icon} className="!size-2.5" aria-hidden />
    ) : (
      token.label
    )}
  </kbd>
);

/**
 * Splits a shortcut string such as "Ctrl+Enter" into its individual key tokens.
 */
const tokenizeShortcut = (value: string): KeyToken[] =>
  value
    .split("+")
    .map((p) => p.trim())
    .filter((p) => p.length > 0)
    .map(tokenForKey);

/**
 * Displays a keyboard shortcut. Each key is rendered as its own standalone cap —
 * "Ctrl+Enter" becomes [⌘][↵] rather than a single [Ctrl+Enter] cap. Modifier and
 * navigation keys are shown as icons where available.
 *
 * Pass `keys` (a shortcut string) for the tokenized, standalone-cap rendering; pass
 * `children` for arbitrary composed content inside a single cap. Set `ghost` to drop
 * the background and border.
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
    const tokens = tokenizeShortcut(shortcut);
    return (
      <span className="inline-flex items-center gap-0.5 align-middle">
        {tokens.map((token, i) => (
          <KeyCap key={i} token={token} ghost={ghost} />
        ))}
      </span>
    );
  }

  // Non-string content (composed nodes) renders unchanged inside a single cap that grows
  // to fit its content rather than staying square.
  return (
    <span className="inline-flex items-center gap-0.5 align-middle">
      <kbd className={cn(keyCapBase, "min-w-4 px-1", keyCapColor({ ghost }))}>{children}</kbd>
    </span>
  );
}

/**
 * Renders the tokens for a shortcut string as standalone key caps. Exposed for
 * widgets (e.g. Button) that already hold a shortcut string and want the same look.
 * Set `inherit` when rendering on a colored surface so the caps adopt its text color.
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
  const tokens = tokenizeShortcut(shortcut);
  if (tokens.length === 0) return null;
  return (
    <span className={cn("inline-flex items-center gap-0.5 align-middle", className)}>
      {tokens.map((token, i) => (
        <KeyCap key={i} token={token} inherit={inherit} ghost={ghost} />
      ))}
    </span>
  );
}
