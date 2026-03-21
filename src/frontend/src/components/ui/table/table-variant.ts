import { cva } from "class-variance-authority";

// Size variants for TableHead padding
export const tableHeadSizeVariant = cva("w-full caption-bottom", {
  variants: {
    density: {
      Small: "h-8 px-1 text-xs",
      Medium: "h-10 px-2 text-sm",
      Large: "h-12 px-3 text-base",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

// Size variants for TableCell padding
export const tableCellSizeVariant = cva("align-middle", {
  variants: {
    density: {
      Small: "p-1 text-xs",
      Medium: "p-2 text-sm",
      Large: "p-3 text-base",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

export const tableSizeVariant = cva("", {
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
