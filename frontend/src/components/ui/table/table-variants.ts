import { cva } from 'class-variance-authority';
import { Sizes } from '@/types/sizes';

// Size variants for TableHead padding
export const tableHeadSizeVariants = cva('w-full caption-bottom', {
  variants: {
    size: {
      Small: 'h-8 px-1 text-xs',
      Medium: 'h-10 px-2 text-sm',
      Large: 'h-12 px-3 text-base',
    },
  },
  defaultVariants: {
    size: Sizes.Medium,
  },
});

// Size variants for TableCell padding
export const tableCellSizeVariants = cva('align-middle', {
  variants: {
    size: {
      Small: 'p-1 text-xs',
      Medium: 'p-2 text-sm',
      Large: 'p-3 text-base',
    },
  },
  defaultVariants: {
    size: Sizes.Medium,
  },
});

export const tableSizeVariants = cva('', {
  variants: {
    size: {
      Small: 'text-xs',
      Medium: 'text-sm',
      Large: 'text-base',
    },
  },
  defaultVariants: {
    size: Sizes.Medium,
  },
});
