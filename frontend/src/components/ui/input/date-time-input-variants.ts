import { cva } from 'class-variance-authority';

export const dateTimeInputVariants = cva(
  'w-full justify-start text-left font-normal pr-20 cursor-pointer bg-transparent',
  {
    variants: {
      scale: {
        Small: 'h-7 px-2 text-xs',
        Medium: 'h-9 px-3 py-2 text-sm',
        Large: 'h-11 px-4 py-2 text-base',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const dateTimeInputIconVariants = cva('', {
  variants: {
    scale: {
      Small: '!size-3',
      Medium: '!size-4',
      Large: '!size-5',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

export const dateTimeInputTextVariants = cva(' ', {
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
