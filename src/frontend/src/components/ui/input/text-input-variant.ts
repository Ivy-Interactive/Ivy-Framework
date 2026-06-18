import { cva } from "class-variance-authority";
import React from "react";

import { cn } from "@/lib/utils";
import { Densities } from "@/types/density";
import { boolInputRowMinHeightVariant } from "@/components/ui/input/bool-input-variant";

export type InputDensityVariant = "Small" | "Medium" | "Large";

/** Normalize widget density for cva (handles enum, string, and responsive fallbacks). */
export function normalizeInputDensity(density?: Densities | string | null): InputDensityVariant {
  const value = density?.toString();
  if (value === Densities.Small || value === "Small") return "Small";
  if (value === Densities.Large || value === "Large") return "Large";
  return "Medium";
}

/**
 * Ivy.Button in affix: outer cell owns spacing (`px-3` or tighter for icon-only).
 * Text buttons: strip `sm` px. Icon-only (`icon-sm` / `icon`): shrink hit box — the
 * `size-7`/`size-9` target is larger than the glyph, which reads as extra padding.
 */
export const affixEmbeddedButtonClasses =
  "[&_button:not([data-invalid-icon])]:!px-0 [&_button:not([data-invalid-icon])]:shadow-none [&_button:not([data-invalid-icon])]:rounded-none [&_button:not([data-invalid-icon])]:hover:bg-accent [&_button:not([data-invalid-icon])]:cursor-pointer [&_button:not([data-invalid-icon])]:transition-colors [&_button:not([data-invalid-icon]).size-7]:!size-4 [&_button:not([data-invalid-icon]).size-9]:!size-6";

/** Tighter affix cell padding when the slot only contains an icon-sized Ivy button (not trailing invalid). */
export const affixIconOnlyCellPaddingClasses =
  "has-[button.size-7:not([data-invalid-icon])]:px-1.5 has-[button.size-9:not([data-invalid-icon])]:px-2";

/** Center icon glyphs (non-button) in affix cells — min-width scales with field density. */
export const textInputAffixIconGlyphCellVariant = cva("[&:not(:has(button))]:justify-center", {
  variants: {
    density: {
      Small: "[&:not(:has(button))]:min-w-6",
      Medium: "[&:not(:has(button))]:min-w-7",
      Large: "[&:not(:has(button))]:min-w-9",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

/**
 * Affix / suffix slot icons (Ivy.Icon) — match trailing invalid icon size per density.
 * !important overrides Lucide width/height attributes (default 24px).
 */
export const textInputAffixIconGlyphSizeVariant = cva(
  "[&_svg]:block [&_svg]:shrink-0 [&_svg]:leading-none",
  {
    variants: {
      density: {
        Small: "[&_svg]:!size-3",
        Medium: "[&_svg]:!size-4",
        Large: "[&_svg]:!size-5",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

/** Shared transparent affix chrome — no muted background; padding scales with field density. */
export const textInputAffixCellVariant = cva(
  cn(
    "flex shrink-0 items-center bg-transparent text-muted-foreground",
    affixEmbeddedButtonClasses,
    affixIconOnlyCellPaddingClasses,
  ),
  {
    variants: {
      side: {
        prefix: "rounded-none",
        suffix: "rounded-none",
      },
      density: {
        Small: "",
        Medium: "",
        Large: "",
      },
    },
    compoundVariants: [
      // Outer pl tracks standalone field px-*; inner pr/pl is the prefix↔content seam.
      { side: "prefix", density: "Small", class: "pl-2 pr-0 text-xs" },
      { side: "prefix", density: "Medium", class: "pl-3 pr-0.5 text-sm" },
      { side: "prefix", density: "Large", class: "pl-4 pr-1 text-base" },
      { side: "suffix", density: "Small", class: "pl-0 pr-2 text-xs" },
      { side: "suffix", density: "Medium", class: "pl-0.5 pr-3 text-sm" },
      { side: "suffix", density: "Large", class: "pl-1 pr-4 text-base" },
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
  const d = normalizeInputDensity(density);
  return cn(
    textInputAffixCellVariant({ side, density: d }),
    textInputAffixIconGlyphCellVariant({ density: d }),
    textInputAffixIconGlyphSizeVariant({ density: d }),
  );
}

function unwrapIvyWidget(node: React.ReactNode): React.ReactElement | null {
  if (!React.isValidElement(node)) return null;
  if (node.type === "ivy-widget") return node;
  if (node.type === React.Suspense) {
    for (const child of React.Children.toArray(
      (node.props as { children?: React.ReactNode }).children,
    )) {
      const unwrapped = unwrapIvyWidget(child);
      if (unwrapped) return unwrapped;
    }
    return null;
  }
  for (const child of React.Children.toArray(
    (node.props as { children?: React.ReactNode }).children,
  )) {
    const unwrapped = unwrapIvyWidget(child);
    if (unwrapped) return unwrapped;
  }
  return null;
}

function getWidgetPropsFromIvyElement(ivyWidget: React.ReactElement): Record<string, unknown> {
  const child = (ivyWidget.props as { children?: React.ReactNode }).children;
  if (React.isValidElement(child)) {
    return (child.props as Record<string, unknown>) ?? {};
  }
  return {};
}

function isAffixIconOnlyNode(node: React.ReactNode): boolean {
  const ivyWidget = unwrapIvyWidget(node);
  if (!ivyWidget) return false;
  const widgetType = (ivyWidget.props as { type?: string }).type;
  if (!widgetType) return false;
  if (widgetType === "Ivy.Icon") return true;
  if (widgetType === "Ivy.Button") {
    const props = getWidgetPropsFromIvyElement(ivyWidget);
    const icon = props.icon as string | undefined;
    const title = props.title as string | undefined;
    return Boolean(icon && icon !== "None" && !title);
  }
  return false;
}

/** True when every affix slot child is an icon (or icon-only button). */
export function isAffixSlotIconOnly(content?: React.ReactNode[]): boolean {
  if (!content?.length) return false;
  return content.every(isAffixIconOnlyNode);
}

/** Prefix affix cell — icon-only padding only when the slot content warrants it. */
export function textInputAffixPrefixCellClasses(
  density: Densities = Densities.Medium,
  content?: React.ReactNode[],
): string {
  const d = normalizeInputDensity(density);
  return cn(
    textInputAffixCellClasses("prefix", density),
    isAffixSlotIconOnly(content) && textInputAffixIconOnlyPaddingVariant({ density: d }),
  );
}

/** Suffix affix cell — trailing cluster + conditional icon-only padding. */
export function textInputAffixSuffixCellClasses(
  density: Densities = Densities.Medium,
  content?: React.ReactNode[],
  options: { showTrailing?: boolean } = {},
): string {
  const d = normalizeInputDensity(density);
  const hasSuffix = (content?.length ?? 0) > 0;
  const showTrailing = options.showTrailing ?? false;
  return cn(
    textInputAffixCellClasses("suffix", density),
    hasSuffix && showTrailing && textInputSuffixWithTrailingClusterClasses(density),
    hasSuffix &&
      !showTrailing &&
      isAffixSlotIconOnly(content) &&
      textInputAffixIconOnlyPaddingVariant({ density: d }),
  );
}

/** Column for native inputs — padding lives on the input via {@link textInputEmbeddedInputClasses}. */
export function textInputAffixInputColumnClasses(
  options: { trailingBesideSuffix?: boolean } = {},
): string {
  return cn("relative flex-1", options.trailingBesideSuffix && "min-w-0");
}

/** Column for single-row custom controls (bool, feedback, icon, date trigger). */
export function textInputAffixControlColumnClasses(density: Densities = Densities.Medium): string {
  const d = normalizeInputDensity(density);
  return cn(
    "relative z-0 isolate flex min-w-0 flex-1 items-center overflow-hidden",
    textInputSizeVariant({ density: d }),
    boolInputRowMinHeightVariant({ density: d }),
    "w-auto",
    textInputEmbeddedContentPaddingClasses(density),
  );
}

/** Column for growing content (code editor) inside an affix shell. */
export function textInputAffixGrowContentColumnClasses(
  density: Densities = Densities.Medium,
): string {
  return cn(
    "relative z-0 isolate flex min-h-0 min-w-0 flex-1 flex-col overflow-hidden",
    textInputEmbeddedContentPaddingClasses(density),
  );
}

/** Bordered field shell for inputs with optional prefix/suffix slots. */
export function textInputFieldShellClasses(options: {
  focused?: boolean;
  invalid?: string;
  disabled?: boolean;
  ghost?: boolean;
}): string {
  return cn(
    "relative flex w-full min-w-0 items-stretch overflow-hidden rounded-field border bg-transparent shadow-sm transition-colors dark:bg-white/5",
    options.focused
      ? "border-ring outline-none dark:border-ring"
      : "border-input dark:border-white/10",
    options.invalid && "border-destructive",
    options.disabled && "cursor-not-allowed opacity-50",
    options.ghost &&
      "border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent",
  );
}

/** Horizontal padding for embedded controls — same scale as {@link textInputSizeVariant} px-*. */
export const textInputEmbeddedContentPaddingVariant = cva("", {
  variants: {
    density: {
      Small: "!pl-1 !pr-1",
      Medium: "!pl-2 !pr-2",
      Large: "!pl-3 !pr-3",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

export function textInputEmbeddedContentPaddingClasses(
  density: Densities = Densities.Medium,
): string {
  return textInputEmbeddedContentPaddingVariant({ density: normalizeInputDensity(density) });
}

/** Inner control nested inside {@link textInputFieldShellClasses} — square seams at affix joins. */
export function textInputEmbeddedInputClasses(
  hasAffixes: boolean,
  density: Densities = Densities.Medium,
): string {
  return cn(
    "border-0 shadow-none focus-visible:ring-0 focus-visible:ring-offset-0 dark:bg-transparent",
    hasAffixes ? "rounded-none" : "rounded-field",
    hasAffixes && textInputEmbeddedContentPaddingClasses(density),
  );
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
  return textInputTrailingClusterGapVariant({ density: normalizeInputDensity(density) });
}

/** Trailing icon hit target — scales with field height at each density. */
export const textInputTrailingHitTargetVariant = cva(
  "inline-flex shrink-0 items-center justify-center overflow-hidden leading-none",
  {
    variants: {
      density: {
        Small: "size-5",
        Medium: "size-6",
        Large: "size-7",
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

/** Suffix glyph in a trailing cluster — same hit target as trailing controls. */
export const textInputSuffixGlyphSlotVariant = cva(
  "inline-flex shrink-0 items-center justify-center overflow-hidden leading-none",
  {
    variants: {
      density: {
        Small: "size-5",
        Medium: "size-6",
        Large: "size-7",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

export function textInputSuffixGlyphSlotClasses(density: Densities = Densities.Medium): string {
  const d = normalizeInputDensity(density);
  return cn(
    textInputSuffixGlyphSlotVariant({ density: d }),
    textInputAffixIconGlyphSizeVariant({ density: d }),
  );
}

/** Trailing icons + suffix glyph in one affix cell — single gap between all icons. */
/** Symmetric padding when suffix holds icon + trailing (overrides asymmetric pl/pr). */
export const textInputAffixSuffixWithTrailingPaddingVariant = cva("!px-1.5", {
  variants: {
    density: {
      Small: "!px-1",
      Medium: "!px-1.5",
      Large: "!px-2",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

/** Symmetric padding for suffix/prefix cells that only contain an icon glyph. */
export const textInputAffixIconOnlyPaddingVariant = cva("!px-1", {
  variants: {
    density: {
      Small: "!px-1",
      Medium: "!px-1.5",
      Large: "!px-2.5",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

export function textInputSuffixWithTrailingClusterClasses(
  density: Densities = Densities.Medium,
): string {
  const d = normalizeInputDensity(density);
  return cn(
    "relative z-10 flex w-max max-w-full shrink-0 flex-nowrap items-center justify-start",
    textInputTrailingClusterGapVariant({ density: d }),
    textInputAffixSuffixWithTrailingPaddingVariant({ density: d }),
  );
}

/** Suffix affix shell (invalid/clear + suffix glyph) — no asymmetric pl/pr. */
export function textInputAffixSuffixTrailingShellClasses(
  density: Densities = Densities.Medium,
): string {
  const d = normalizeInputDensity(density);
  return cn(
    "flex shrink-0 items-center self-center rounded-none bg-transparent text-muted-foreground",
    affixEmbeddedButtonClasses,
    textInputAffixIconGlyphSizeVariant({ density: d }),
  );
}

/** Suffix cell with invalid/clear + suffix glyph — shell + cluster helpers. */
export function textInputAffixSuffixTrailingCellClasses(
  density: Densities = Densities.Medium,
): string {
  return cn(
    textInputAffixSuffixTrailingShellClasses(density),
    textInputSuffixWithTrailingClusterClasses(density),
  );
}

/** Invalid icon in an affix trailing cluster — glyph-sized only (no size-7 hit box). */
export function textInputAffixInvalidIconClasses(): string {
  return cn(
    "inline-flex shrink-0 grow-0 basis-auto items-center justify-center self-center",
    "size-auto min-h-0 min-w-0 p-0 m-0 border-0 bg-transparent shadow-none rounded-none",
    "cursor-pointer hover:bg-transparent hover:text-inherit",
    "focus-visible:ring-1 focus-visible:ring-ring",
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
  const d = normalizeInputDensity(density);
  return cn(
    textInputTrailingHitTargetVariant({ density: d }),
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
    "shrink-0 grow-0 basis-auto hover:bg-transparent hover:text-inherit",
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

// Left padding variants for search input when icon is shown
export const searchInputPaddingVariant = cva("", {
  variants: {
    density: {
      Small: "!pl-7",
      Medium: "!pl-8",
      Large: "!pl-9",
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
