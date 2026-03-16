import { cva } from 'class-variance-authority';

// Size variants for FileInput
export const fileInputVariant = cva(
  'relative rounded-field border-dashed transition-colors transition-shadow duration-200',
  {
    variants: {
      variant: {
        Default: 'border-transparent bg-transparent p-0 min-h-0',
        Drop: 'border-2 border-muted-foreground/25 bg-transparent min-h-[100px] max-h-[300px] p-4',
      },
      density: {
        Small: 'gap-2',
        Medium: 'gap-4',
        Large: 'gap-6',
      },
      isDragging: {
        true: 'border-primary bg-primary/5 outline-none ring-2 ring-primary ring-offset-2',
        false: '',
      },
    },
    compoundVariants: [
      {
        variant: 'Drop',
        density: 'Small',
        className: 'min-h-[80px] max-h-[200px] p-2',
      },
      {
        variant: 'Drop',
        density: 'Medium',
        className: 'min-h-[100px] max-h-[300px] p-4',
      },
      {
        variant: 'Drop',
        density: 'Large',
        className: 'min-h-[120px] max-h-[400px] p-6 border-3',
      },
    ],
    defaultVariants: {
      variant: 'Drop',
      density: 'Medium',
      isDragging: false,
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
