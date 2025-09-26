import { cva } from 'class-variance-authority';

export const dateTimeInputVariants = cva(
  'w-full justify-start text-left font-normal pr-20 cursor-pointer bg-transparent',
  {
    variants: {
      size: {
        Small: 'h-8 px-3 text-xs',
        Medium: 'h-9 px-4 py-2 text-sm',
        Large: 'h-10 px-5 py-2 text-base',
      },
    },
    defaultVariants: {
      size: 'Medium',
    },
  }
);

export const dateTimeInputIconVariants = cva('', {
  variants: {
    size: {
      Small: 'h-3 w-3',
      Medium: 'h-4 w-4',
      Large: 'h-5 w-5',
    },
  },
  defaultVariants: {
    size: 'Medium',
  },
});

export const dateTimeInputTextVariants = cva(' ', {
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
