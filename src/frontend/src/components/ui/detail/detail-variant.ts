import { cva } from "class-variance-authority";

// Size variants for Detail label
export const detailLabelSizeVariant = cva("align-middle font-bold whitespace-nowrap pl-0", {
  variants: {
    density: {
      Small: "p-2 pl-0 text-xs",
      Medium: "p-3 pl-0 text-sm",
      Large: "p-4 pl-0 text-base",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

// Size variants for Detail value
export const detailValueSizeVariant = cva("align-middle min-w-0", {
  variants: {
    density: {
      Small: "p-2 pl-0 pr-0 text-xs",
      Medium: "p-3 pl-0 pr-0 text-sm",
      Large: "p-4 pl-0 pr-0 text-base",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

// Size variants for Detail value multiline padding bottom
export const detailValueMultiLinePaddingVariant = cva("", {
  variants: {
    density: {
      Small: "pb-2 pt-0",
      Medium: "pb-3 pt-1",
      Large: "pb-4 pt-2",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

// Size variants for Details container
export const detailsSizeVariant = cva("", {
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
