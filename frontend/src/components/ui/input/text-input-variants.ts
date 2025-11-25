import { cva } from 'class-variance-authority';

// Size variants for TextInputWidget
export const textInputSizeVariants = cva('w-full', {
  variants: {
    scale: {
      Small: 'text-xs px-2 h-7',
      Medium: 'text-sm px-3 h-9',
      Large: 'text-base px-4 h-11',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

// Size variants for search icon
export const searchIconVariants = cva('absolute text-muted-foreground', {
  variants: {
    scale: {
      Small: 'left-3 top-2 h-3 w-3',
      Medium: 'left-2.5 top-2.5 h-4 w-4',
      Large: 'left-2 top-3 h-5 w-5',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

// Size variants for X icon
export const xIconVariants = cva(
  'text-muted-foreground hover:text-foreground',
  {
    variants: {
      scale: {
        Small: 'h-3 w-3',
        Medium: 'h-4 w-4',
        Large: 'h-5 w-5',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

// Size variants for eye icons (password toggle)
export const eyeIconVariants = cva('', {
  variants: {
    scale: {
      Small: 'h-3 w-3',
      Medium: 'h-4 w-4',
      Large: 'h-5 w-5',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});
