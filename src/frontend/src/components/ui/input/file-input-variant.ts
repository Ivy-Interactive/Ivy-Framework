import { cva } from 'class-variance-authority';

// Size variants for FileInput
export const fileInputVariant = cva(
  'relative rounded-field border-dashed transition-colors',
  {
    variants: {
      density: {
        Small: 'min-h-[80px] max-h-[200px] p-2 border-2',
        Medium: 'min-h-[100px] max-h-[300px] p-4 border-2',
        Large: 'min-h-[120px] max-h-[400px] p-6 border-3',
      },
    },
    defaultVariants: {
      density: 'Medium',
    },
  }
);

// Size variants for upload icon
export const uploadIconVariant = cva('text-primary', {
  variants: {
    density: {
      Small: 'h-4 w-4 mb-1',
      Medium: 'h-6 w-6 mb-2',
      Large: 'h-8 w-8 mb-3',
    },
  },
  defaultVariants: {
    density: 'Medium',
  },
});

// Size variants for text
export const textVariant = cva('text-muted-foreground', {
  variants: {
    density: {
      Small: 'text-xs px-2',
      Medium: 'text-sm px-3',
      Large: 'text-base px-4',
    },
  },
  defaultVariants: {
    density: 'Medium',
  },
});
