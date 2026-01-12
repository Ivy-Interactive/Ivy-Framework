import { cva } from 'class-variance-authority';

// Size variants for Detail label
export const detailLabelSizeVariants = cva(
  'align-middle font-bold whitespace-nowrap pl-0',
  {
    variants: {
      scale: {
        Small: 'p-2 pl-0 text-xs',
        Medium: 'p-3 pl-0 text-sm',
        Large: 'p-4 pl-0 text-base',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

// Size variants for Detail value
export const detailValueSizeVariants = cva('align-middle min-w-0', {
  variants: {
    scale: {
      Small: 'p-2 pl-0 pr-0 text-xs',
      Medium: 'p-3 pl-0 pr-0 text-sm',
      Large: 'p-4 pl-0 pr-0 text-base',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

// Size variants for Detail value multiline padding bottom
export const detailValueMultiLinePaddingVariants = cva('', {
  variants: {
    scale: {
      Small: 'pb-2 pt-0',
      Medium: 'pb-3 pt-1',
      Large: 'pb-4 pt-2',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

// Size variants for Details container
export const detailsSizeVariants = cva('', {
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
