import { cva } from "class-variance-authority";

export const dateTimeInputVariant = cva(
  "w-full justify-start text-left font-normal cursor-pointer bg-transparent",
  {
    variants: {
      density: {
        Small: "h-7 min-h-7 max-h-7 px-2 py-0 text-xs [&_svg]:!size-3",
        Medium: "h-9 min-h-9 max-h-9 px-3 py-0 text-sm [&_svg]:!size-4",
        Large: "h-11 min-h-11 max-h-11 px-4 py-0 text-base [&_svg]:!size-5",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

export const dateTimeInputIconVariant = cva("shrink-0", {
  variants: {
    density: {
      Small: "!size-3",
      Medium: "!size-4",
      Large: "!size-5",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

export const dateTimeInputTextVariant = cva(" ", {
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
