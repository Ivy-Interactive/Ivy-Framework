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

  // Platform modifiers
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
    return { icon: "ArrowBigUp", label: "Shift", aria: "Shift" };
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

/** Shared base styling for a single key cap. */
const keyCapBase =
  "inline-flex h-5 min-w-5 items-center justify-center rounded-selector px-1.5 text-[0.7rem] font-medium leading-none";

/**
 * Color styling for a key cap. `inherit` adopts the surrounding text color (for use
 * on colored surfaces such as a primary button); otherwise the standard muted look.
 */
const keyCapColor = (inherit?: boolean) =>
  inherit
    ? "border border-current/30 bg-transparent text-current"
    : "border border-border bg-muted/40 text-foreground";

/**
 * Renders a single standalone key box.
 */
const KeyCap: React.FC<{ token: KeyToken; inherit?: boolean }> = ({ token, inherit }) => (
  <kbd
    title={token.icon ? token.aria : undefined}
    aria-label={token.icon ? token.aria : undefined}
    className={cn(keyCapBase, keyCapColor(inherit))}
  >
    {token.icon ? <Icon name={token.icon} size={12} aria-hidden /> : token.label}
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
 * Displays a keyboard shortcut. Each key is rendered as its own standalone box —
 * "Ctrl+Enter" becomes [⌘][↵] rather than a single [Ctrl+Enter] cap. Modifier and
 * navigation keys are shown as icons where available.
 */
export function Kbd({ children }: { children: React.ReactNode }) {
  // Plain string shortcuts get tokenized into standalone, icon-aware key caps.
  if (typeof children === "string" && children.trim().length > 0) {
    const tokens = tokenizeShortcut(children);
    return (
      <span className="inline-flex items-center gap-0.5 align-middle">
        {tokens.map((token, i) => (
          <KeyCap key={i} token={token} />
        ))}
      </span>
    );
  }

  // Non-string content (composed nodes) renders unchanged inside a single key box.
  return (
    <span className="inline-flex items-center gap-0.5 align-middle">
      <kbd className={cn(keyCapBase, keyCapColor())}>{children}</kbd>
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
}: {
  shortcut: string;
  className?: string;
  inherit?: boolean;
}) {
  const tokens = tokenizeShortcut(shortcut);
  if (tokens.length === 0) return null;
  return (
    <span className={cn("inline-flex items-center gap-0.5 align-middle", className)}>
      {tokens.map((token, i) => (
        <KeyCap key={i} token={token} inherit={inherit} />
      ))}
    </span>
  );
}
