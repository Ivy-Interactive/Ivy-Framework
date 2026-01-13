import { cva } from 'class-variance-authority';

export const colorInputVariants = cva(
  'w-full justify-start text-left cursor-pointer bg-transparent',
  {
    variants: {
      scale: {
        Small: 'h-8 px-2 py-1 text-xs',
        Medium: 'h-9 px-3 py-1 text-sm',
        Large: 'h-10 px-4 py-2 text-base',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const colorInputPickerVariants = cva('', {
  variants: {
    scale: {
      Small: 'w-8 h-8',
      Medium: 'w-10 h-10',
      Large: 'w-12 h-12',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});
