import { cva } from 'class-variance-authority';

// Size variants for BoolInput components
export const boolInputSizeVariants = {
  Small: 'text-xs',
  Medium: 'text-sm',
  Large: 'text-base',
};

// Toggle size variants
export const toggleSizeVariants = cva(
  'inline-flex items-center justify-center rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      size: {
        Small: 'h-8 px-2 text-xs',
        Medium: 'h-10 px-3 text-sm',
        Large: 'h-12 px-4 text-base',
      },
    },
    defaultVariants: {
      size: 'Medium',
    },
  }
);

// Label size variants
export const labelSizeVariants = cva(
  'text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70',
  {
    variants: {
      size: {
        Small: 'text-xs',
        Medium: 'text-sm',
        Large: 'text-base',
      },
    },
    defaultVariants: {
      size: 'Medium',
    },
  }
);

// Description size variants
export const descriptionSizeVariants = cva('text-muted-foreground', {
  variants: {
    size: {
      Small: 'text-xs',
      Medium: 'text-sm',
      Large: 'text-base',
    },
  },
  defaultVariants: {
    size: 'Medium',
  },
});
