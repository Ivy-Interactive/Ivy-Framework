import { cva } from "class-variance-authority";

export const paginationContentVariant = cva("flex flex-row items-center", {
  variants: {
    density: {
      Small: "gap-0.5",
      Medium: "gap-1",
      Large: "gap-1.5",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});
