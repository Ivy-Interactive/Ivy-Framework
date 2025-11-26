import { cva } from 'class-variance-authority';

export const dateRangeInputVariants = cva(
  'w-full justify-start text-left font-normal pr-20 cursor-pointer bg-transparent',
  {
    variants: {
      scale: {
        Small: 'h-8 px-3',
        Medium: 'h-9 px-4 py-2',
        Large: 'h-10 px-5 py-2',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const dateRangeInputIconVariants = cva('', {
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

export const dateRangeInputTextVariants = cva(' ', {
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
