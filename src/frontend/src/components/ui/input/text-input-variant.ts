import { cva } from "class-variance-authority";

import { cn } from "@/lib/utils";

/**
 * Ivy.Button in affix: outer cell owns spacing (`px-3` or tighter for icon-only).
 * Text buttons: strip `sm` px. Icon-only (`icon-sm` / `icon`): shrink hit box — the
 * `size-7`/`size-9` target is larger than the glyph, which reads as extra padding.
 */
export const affixEmbeddedButtonClasses =
  "[&_button]:!px-0 [&_button]:text-foreground [&_button]:shadow-none [&_button]:rounded [&_button]:hover:bg-accent [&_button]:cursor-pointer [&_button]:transition-colors [&_button.size-7]:!size-4 [&_button.size-9]:!size-6";

/** Tighter affix cell padding when the slot only contains an icon-sized button. */
export const affixIconOnlyCellPaddingClasses =
  "has-[button.size-7]:px-1.5 has-[button.size-9]:px-2";

/** Affix cells: muted box by default; ghost uses transparent chrome with tight padding toward the input. */
export function textInputAffixCellClasses(
  side: "prefix" | "suffix",
  ghostWithAffixes: boolean,
): string {
  return cn(
    "flex items-center text-muted-foreground",
    affixEmbeddedButtonClasses,
    ghostWithAffixes
      ? side === "suffix"
        ? "shrink-0 bg-transparent pl-0 pr-1.5"
        : "shrink-0 bg-transparent pl-2 pr-0.5"
      : cn(
          "px-3 bg-muted",
          affixIconOnlyCellPaddingClasses,
          side === "prefix"
            ? "rounded-tl-fields rounded-bl-fields"
            : "rounded-tr-fields rounded-br-fields",
        ),
  );
}

// Size variants for TextInputWidget
export const textInputSizeVariant = cva("w-full", {
  variants: {
    density: {
      Small: "text-xs px-2 h-7",
      Medium: "text-sm px-3 h-9",
      Large: "text-base px-4 h-11",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

// Size variants for Textarea (minimum height and padding scale; no fixed height)
export const textareaSizeVariant = cva("w-full", {
  variants: {
    density: {
      Small: "min-h-[52px] p-2 text-xs",
      Medium: "min-h-[60px] py-2 px-3 text-sm",
      Large: "min-h-[72px] py-3 px-4 text-base",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

// Size variants for search icon
export const searchIconVariant = cva("absolute text-muted-foreground", {
  variants: {
    density: {
      Small: "left-3 top-2 size-3",
      Medium: "left-2.5 top-2.5 size-4",
      Large: "left-2 top-3 size-5",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

// Size variants for X icon
export const xIconVariant = cva("text-muted-foreground hover:text-foreground", {
  variants: {
    density: {
      Small: "top-2 size-3",
      Medium: "top-2.5 size-4",
      Large: "top-3 size-5",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

// Size variants for eye icons (password toggle)
export const eyeIconVariant = cva("", {
  variants: {
    density: {
      Small: "size-3",
      Medium: "size-4",
      Large: "size-5",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});
