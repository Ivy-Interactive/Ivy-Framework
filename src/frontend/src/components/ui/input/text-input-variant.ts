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

/** Horizontal gap between trailing icons — scales with field density. */
export const textInputTrailingClusterGapVariant = cva("", {
  variants: {
    density: {
      Small: "gap-0.5",
      Medium: "gap-1.5",
      Large: "gap-2.5",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

export function textInputTrailingClusterGapClasses(density: Densities = Densities.Medium): string {
  return textInputTrailingClusterGapVariant({ density });
}

/** Trailing icon hit target — scales with field height (Large keeps Medium width to avoid overlap). */
export const textInputTrailingHitTargetVariant = cva(
  "inline-flex shrink-0 items-center justify-center overflow-hidden leading-none",
  {
    variants: {
      density: {
        Small: "size-5",
        Medium: "size-6",
        Large: "size-6",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

export function textInputTrailingShortcutWrapperClasses(
  density: Densities = Densities.Medium,
): string {
  return cn(textInputTrailingHitTargetVariant({ density }), "w-auto px-0");
}

/** Suffix glyph in a trailing cluster — same hit target; cap Lucide default 24px icons. */
export const textInputSuffixGlyphSlotVariant = cva(
  "inline-flex shrink-0 items-center justify-center overflow-hidden leading-none [&_svg]:block [&_svg]:shrink-0",
  {
    variants: {
      density: {
        Small: "size-5 [&_svg]:size-3",
        Medium: "size-6 [&_svg]:size-4",
        Large: "size-6 [&_svg]:size-5",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

export function textInputSuffixGlyphSlotClasses(density: Densities = Densities.Medium): string {
  return textInputSuffixGlyphSlotVariant({ density });
}

/** Trailing icons + suffix glyph in one affix cell — single gap between all icons. */
export function textInputSuffixWithTrailingClusterClasses(
  density: Densities = Densities.Medium,
): string {
  return cn(
    "relative z-10 flex shrink-0 flex-nowrap items-center self-stretch",
    textInputTrailingClusterGapVariant({ density }),
  );
}

/** Textarea: trailing stack + suffix icon, same horizontal gap to suffix glyph. */
export function textareaSuffixWithTrailingClusterClasses(
  density: Densities = Densities.Medium,
): string {
  return cn(textInputSuffixWithTrailingClusterClasses(density), "items-start self-start pt-2");
}

/** Standalone trailing cluster (no suffix affix content in the same cell). */
export function textInputTrailingBesideSuffixClasses(
  density: Densities = Densities.Medium,
): string {
  return cn(
    "relative z-10 flex shrink-0 items-center self-stretch text-muted-foreground",
    textInputTrailingClusterGapVariant({ density }),
  );
}

/** Multiline fields: trailing stack top-aligned when not merged into suffix cell. */
export function textareaTrailingBesideSuffixClasses(density: Densities = Densities.Medium): string {
  return cn(textInputTrailingBesideSuffixClasses(density), "flex-col items-center self-start pt-2");
}

/** Clear / shortcut / invalid / password-eye cluster overlaid inside the field. */
export const textInputTrailingOverlayPositionVariant = cva("", {
  variants: {
    density: {
      Small: "right-1.5",
      Medium: "right-2",
      Large: "right-2.5",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

export function textInputTrailingOverlayClasses(density: Densities = Densities.Medium): string {
  return cn(
    "pointer-events-none absolute top-1/2 flex -translate-y-1/2 flex-row flex-nowrap items-center",
    textInputTrailingOverlayPositionVariant({ density }),
    textInputTrailingClusterGapVariant({ density }),
  );
}

/** Multiline overlay controls (top-right). */
export const textareaTrailingOverlayPositionVariant = cva(
  "pointer-events-none absolute z-10 flex flex-col items-center",
  {
    variants: {
      density: {
        Small: "right-2 top-1.5",
        Medium: "right-2.5 top-2",
        Large: "right-3 top-2.5",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

export function textareaTrailingOverlayClasses(density: Densities = Densities.Medium): string {
  return cn(
    textareaTrailingOverlayPositionVariant({ density }),
    textInputTrailingClusterGapVariant({ density }),
  );
}

/** Vertical trailing stack beside textarea suffix. */
export function textareaTrailingStackClasses(density: Densities = Densities.Medium): string {
  return cn("flex flex-col items-center", textInputTrailingClusterGapVariant({ density }));
}

/** Uniform trailing icon button — fixed hit target keeps eye/clear aligned beside suffix. */
export function textInputTrailingIconButtonClasses(
  overlay = false,
  density: Densities = Densities.Medium,
): string {
  return cn(
    textInputTrailingHitTargetVariant({ density }),
    "cursor-pointer rounded text-muted-foreground hover:bg-accent hover:text-foreground focus:outline-none",
    overlay && "pointer-events-auto",
  );
}

/** Invalid icon slot — same footprint as eye/clear so cluster gaps stay even. */
export function textInputTrailingInvalidSlotClasses(
  overlay = false,
  density: Densities = Densities.Medium,
): string {
  return cn(
    textInputTrailingIconButtonClasses(overlay, density),
    "hover:bg-transparent hover:text-inherit",
  );
}

/** Icon glyph size inside trailing buttons (no absolute top offsets). */
export const textInputTrailingIconSizeVariant = cva("block shrink-0 leading-none", {
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
