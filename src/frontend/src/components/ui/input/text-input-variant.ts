import { cva } from "class-variance-authority";

import { cn } from "@/lib/utils";
import { Densities } from "@/types/density";

/**
 * Ivy.Button in affix: outer cell owns spacing (`px-3` or tighter for icon-only).
 * Text buttons: strip `sm` px. Icon-only (`icon-sm` / `icon`): shrink hit box — the
 * `size-7`/`size-9` target is larger than the glyph, which reads as extra padding.
 */
export const affixEmbeddedButtonClasses =
  "[&_button]:!px-0 [&_button]:shadow-none [&_button]:rounded [&_button]:hover:bg-accent [&_button]:cursor-pointer [&_button]:transition-colors [&_button.size-7]:!size-4 [&_button.size-9]:!size-6";

/** Tighter affix cell padding when the slot only contains an icon-sized button. */
export const affixIconOnlyCellPaddingClasses =
  "has-[button.size-7]:px-1.5 has-[button.size-9]:px-2";

/** Center icon glyphs (non-button) in affix cells for even visual weight. */
export const affixIconGlyphCellClasses =
  "[&:not(:has(button))]:justify-center [&:not(:has(button))]:min-w-9";

/** Shared transparent affix chrome — no muted background; padding scales with field density. */
export const textInputAffixCellVariant = cva(
  cn(
    "flex shrink-0 items-center bg-transparent text-muted-foreground",
    affixEmbeddedButtonClasses,
    affixIconOnlyCellPaddingClasses,
    affixIconGlyphCellClasses,
  ),
  {
    variants: {
      side: {
        prefix: "rounded-tl-fields rounded-bl-fields",
        suffix: "rounded-tr-fields rounded-br-fields",
      },
      density: {
        Small: "",
        Medium: "",
        Large: "",
      },
    },
    compoundVariants: [
      { side: "prefix", density: "Small", class: "pl-2 pr-1 text-xs" },
      { side: "prefix", density: "Medium", class: "pl-3 pr-1.5 text-sm" },
      { side: "prefix", density: "Large", class: "pl-4 pr-2 text-base" },
      { side: "suffix", density: "Small", class: "pl-1 pr-2 text-xs" },
      { side: "suffix", density: "Medium", class: "pl-1.5 pr-3 text-sm" },
      { side: "suffix", density: "Large", class: "pl-2 pr-4 text-base" },
    ],
    defaultVariants: {
      side: "prefix",
      density: "Medium",
    },
  },
);

export function textInputAffixCellClasses(
  side: "prefix" | "suffix",
  density: Densities = Densities.Medium,
): string {
  return textInputAffixCellVariant({ side, density });
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
