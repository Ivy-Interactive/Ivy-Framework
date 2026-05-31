import { cva } from "class-variance-authority";

/** Gap between control and label — scales with density. */
export const boolInputControlGapVariant = cva("", {
  variants: {
    density: {
      Small: "gap-1.5",
      Medium: "gap-2",
      Large: "gap-2.5",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

/** Inner padding when affix shell has no prefix/suffix on that side. */
export const boolInputAffixEdgePaddingVariant = cva("", {
  variants: {
    side: {
      start: "",
      end: "",
    },
    density: {
      Small: "",
      Medium: "",
      Large: "",
    },
  },
  compoundVariants: [
    { side: "start", density: "Small", class: "pl-2" },
    { side: "start", density: "Medium", class: "pl-3" },
    { side: "start", density: "Large", class: "pl-4" },
    { side: "end", density: "Small", class: "pr-2" },
    { side: "end", density: "Medium", class: "pr-3" },
    { side: "end", density: "Large", class: "pr-4" },
  ],
  defaultVariants: {
    side: "start",
    density: "Medium",
  },
});

// Row min-height variants - matches TextInput heights for consistent form field alignment
export const boolInputRowMinHeightVariant = cva("", {
  variants: {
    density: {
      Small: "min-h-7",
      Medium: "min-h-9",
      Large: "min-h-11",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

// Size variants for BoolInput components
export const boolInputSizeVariant = {
  Small: "text-xs",
  Medium: "text-sm",
  Large: "text-base",
};

// Label size variants
export const labelSizeVariant = cva(
  "text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70",
  {
    variants: {
      density: {
        Small: "text-xs",
        Medium: "text-sm",
        Large: "text-base",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

// Description size variants
export const descriptionSizeVariant = cva("text-muted-foreground", {
  variants: {
    density: {
      Small: "text-xs",
      Medium: "text-sm",
      Large: "text-base",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});
