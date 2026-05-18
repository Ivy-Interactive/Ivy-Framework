import { cva } from "class-variance-authority";

export const switchVariant = cva(
  "peer inline-flex shrink-0 cursor-pointer items-center rounded-full border-2 border-transparent shadow-sm transition-colors hover:data-[state=unchecked]:bg-input/80 hover:data-[state=checked]:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background disabled:cursor-not-allowed disabled:opacity-50 data-[state=checked]:bg-primary data-[state=unchecked]:bg-input",
  {
    variants: {
      density: {
        Small: "h-4 w-7",
        Medium: "h-5 w-9",
        Large: "h-6 w-11",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

export const switchThumbVariant = cva(
  "pointer-events-none block rounded-full bg-background shadow-lg ring-0 transition-transform data-[state=unchecked]:translate-x-0",
  {
    variants: {
      density: {
        Small: "size-3 data-[state=checked]:translate-x-3",
        Medium: "size-4 data-[state=checked]:translate-x-4",
        Large: "size-5 data-[state=checked]:translate-x-5",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);
