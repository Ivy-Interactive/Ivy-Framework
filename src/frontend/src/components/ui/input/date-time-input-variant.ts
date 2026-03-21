import { cva } from "class-variance-authority";

export const dateTimeInputVariant = cva(
  "w-full justify-start text-left font-normal pr-20 cursor-pointer bg-transparent",
  {
    variants: {
      density: {
        Small: "h-7 px-2 text-xs",
        Medium: "h-9 px-3 py-2 text-sm",
        Large: "h-11 px-4 py-2 text-base",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

export const dateTimeInputIconVariant = cva("", {
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
