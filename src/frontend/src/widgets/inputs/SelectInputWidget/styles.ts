import { cva } from "class-variance-authority";

export const selectContainerVariant = cva(
  "relative border border-input bg-transparent rounded-box focus-within:outline-none focus-within:border-ring dark:border-white/10",
  {
    variants: {
      density: {
        Small: "px-2 py-1",
        Medium: "px-3 py-2",
        Large: "px-4 py-3",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

export const selectTextVariant = {
  Small: "text-xs",
  Medium: "text-sm",
  Large: "text-base",
};

export const circleSizeVariant = {
  Small: "size-3",
  Medium: "size-4",
  Large: "size-5",
};

export const sliderLabelVariant: Record<string, string> = {
  Small: "text-xs",
  Medium: "text-sm",
  Large: "text-base",
};
