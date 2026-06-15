import { cva } from "class-variance-authority";

export const colorInputVariant = cva(
  "w-full justify-start text-left cursor-pointer bg-transparent",
  {
    variants: {
      density: {
        Small: "h-7 py-1 text-xs",
        Medium: "h-9 py-1 text-sm",
        Large: "h-11 py-2 text-base",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

/** Affix row min-height — matches bool/text field heights. */
export const colorInputRowMinHeightVariant = cva("", {
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

export const colorInputPickerVariant = cva("", {
  variants: {
    density: {
      Small: "size-7",
      Medium: "size-9",
      Large: "size-11",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});
