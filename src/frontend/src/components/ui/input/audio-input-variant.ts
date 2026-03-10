import { cva } from 'class-variance-authority';

// Size variants for AudioInputWidget
export const audioInputVariant = cva(
  'relative rounded-field border-dashed transition-colors border-muted-foreground/25',
  {
    variants: {
      scale: {
        Small: 'p-3 w-24 border-2',
        Medium: 'p-4 w-28 border-2',
        Large: 'p-5 w-36 border-3',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

// Size variants for text
export const textSizeVariant = cva('', {
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

// Size variants for timer
export const timerSizeVariant = cva('', {
  variants: {
    scale: {
      Small: 'text-sm',
      Medium: 'text-base',
      Large: 'text-lg',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

// Size variants for icons
export const iconSizeVariant = cva('', {
  variants: {
    scale: {
      Small: '!size-4',
      Medium: '!size-5',
      Large: '!size-6',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});
