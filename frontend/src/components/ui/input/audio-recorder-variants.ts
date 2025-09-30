import { cva } from 'class-variance-authority';

// Size variants for AudioRecorderWidget
export const audioRecorderVariants = cva(
  'relative rounded-md border-dashed transition-colors border-muted-foreground/25',
  {
    variants: {
      size: {
        Small: 'p-3 w-24 border-2',
        Medium: 'p-4 w-28 border-2',
        Large: 'p-5 w-36 border-3',
      },
    },
    defaultVariants: {
      size: 'Medium',
    },
  }
);

// Size variants for text
export const textSizeVariants = cva('', {
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

// Size variants for timer
export const timerSizeVariants = cva('', {
  variants: {
    size: {
      Small: 'text-sm',
      Medium: 'text-base',
      Large: 'text-lg',
    },
  },
  defaultVariants: {
    size: 'Medium',
  },
});

// Size variants for icons
export const iconSizeVariants = cva('', {
  variants: {
    size: {
      Small: '!size-4',
      Medium: '!size-5',
      Large: '!size-6',
    },
  },
  defaultVariants: {
    size: 'Medium',
  },
});
