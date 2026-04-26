import { cva } from "class-variance-authority";

/**
 * Tab trigger variants with density support
 * Controls tab height, padding, and text size based on density
 */
export const tabTriggerVariant = cva(
  "cursor-pointer transition-colors duration-300 rounded-selector hover:bg-secondary",
  {
    variants: {
      density: {
        Small: "px-2 py-1 text-xs h-8",
        Medium: "px-3 py-1.5 text-sm h-10",
        Large: "px-4 py-2 text-base h-12",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

/**
 * Hover highlight height variants based on density
 */
export const hoverHighlightVariant = cva(
  "absolute transition-all duration-300 ease-out bg-accent/20 rounded-[6px] flex items-center",
  {
    variants: {
      density: {
        Small: "h-8",
        Medium: "h-10",
        Large: "h-12",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);
