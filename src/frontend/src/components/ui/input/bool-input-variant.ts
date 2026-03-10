import { cva } from 'class-variance-authority';

// Row min-height variants - matches TextInput heights for consistent form field alignment
export const boolInputRowMinHeightVariant = cva('', {
  variants: {
    density: {
      Small: 'min-h-7',
      Medium: 'min-h-9',
      Large: 'min-h-11',
    },
  },
  defaultVariants: {
    density: 'Medium',
  },
});

// Size variants for BoolInput components
export const boolInputSizeVariant = {
  Small: 'text-xs',
  Medium: 'text-sm',
  Large: 'text-base',
};

// Label size variants
export const labelSizeVariant = cva(
  'text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70',
  {
    variants: {
      density: {
        Small: 'text-xs',
        Medium: 'text-sm',
        Large: 'text-base',
      },
    },
    defaultVariants: {
      density: 'Medium',
    },
  }
);

// Description size variants
export const descriptionSizeVariant = cva('text-muted-foreground', {
  variants: {
    density: {
      Small: 'text-xs',
      Medium: 'text-sm',
      Large: 'text-base',
    },
  },
  defaultVariants: {
    density: 'Medium',
  },
});
