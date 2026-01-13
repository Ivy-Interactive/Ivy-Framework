import { cva } from 'class-variance-authority';

// Size variants for TableHead padding
export const tableHeadSizeVariants = cva('w-full caption-bottom', {
  variants: {
    scale: {
      Small: 'h-8 px-1 text-xs',
      Medium: 'h-10 px-2 text-sm',
      Large: 'h-12 px-3 text-base',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

// Size variants for TableCell padding
export const tableCellSizeVariants = cva('align-middle', {
  variants: {
    scale: {
      Small: 'p-1 text-xs',
      Medium: 'p-2 text-sm',
      Large: 'p-3 text-base',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

export const tableSizeVariants = cva('', {
  variants: {
    scale: {
      Small: 'text-xs',
      Medium: 'text-sm',
      Large: 'text-base',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});
