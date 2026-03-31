import { cva } from "class-variance-authority";

export const codeContainerVariant = cva("", {
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

export const codeCopyButtonVariant = cva("absolute z-50", {
  variants: {
    density: {
      Small: "top-1 right-1",
      Medium: "top-1.5 right-1.5",
      Large: "top-2 right-2",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});
