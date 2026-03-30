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

export const colorInputPickerVariant = cva("", {
  variants: {
    density: {
      Small: "w-7 h-7",
      Medium: "w-9 h-9",
      Large: "w-11 h-11",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});
