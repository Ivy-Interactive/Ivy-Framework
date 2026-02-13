import { cva } from 'class-variance-authority';

// Row min-height variants - matches TextInput heights for consistent form field alignment
export const boolInputRowMinHeightVariants = cva('', {
  variants: {
    scale: {
      Small: 'min-h-7',
      Medium: 'min-h-9',
      Large: 'min-h-11',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

// Size variants for BoolInput components
export const boolInputSizeVariants = {
  Small: 'text-xs',
  Medium: 'text-sm',
  Large: 'text-base',
};

// Label size variants
export const labelSizeVariants = cva(
  'text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70',
  {
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
  }
);

// Description size variants
export const descriptionSizeVariants = cva('text-muted-foreground', {
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
