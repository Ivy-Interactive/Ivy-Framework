import { cva } from "class-variance-authority";

export const selectContainerVariant = cva(
  "relative border border-input bg-transparent rounded-box shadow-sm focus-within:ring-1 focus-within:ring-ring dark:border-white/10",
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
  Small: "h-3 w-3",
  Medium: "h-4 w-4",
  Large: "h-5 w-5",
};

export const sliderLabelVariant: Record<string, string> = {
  Small: "text-xs",
  Medium: "text-sm",
  Large: "text-base",
};
