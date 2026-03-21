import { cva } from "class-variance-authority";

export const colorInputVariant = cva(
  "w-full justify-start text-left cursor-pointer bg-transparent",
  {
    variants: {
      density: {
        Small: "h-8 py-1 text-xs",
        Medium: "h-9 py-1 text-sm",
        Large: "h-10 py-2 text-base",
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
      Small: "w-8 h-8",
      Medium: "w-10 h-10",
      Large: "w-12 h-12",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});
