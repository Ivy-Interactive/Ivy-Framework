import { cva } from 'class-variance-authority';

export const switchVariants = cva(
  'peer inline-flex shrink-0 cursor-pointer items-center rounded-full border-2 border-transparent shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background disabled:cursor-not-allowed disabled:opacity-50 data-[state=checked]:bg-primary data-[state=unchecked]:bg-input',
  {
    variants: {
      scale: {
        Small: 'h-4 w-7',
        Medium: 'h-5 w-9',
        Large: 'h-6 w-11',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const switchThumbVariants = cva(
  'pointer-events-none block rounded-full bg-background shadow-lg ring-0 transition-transform data-[state=unchecked]:translate-x-0',
  {
    variants: {
      scale: {
        Small: 'h-3 w-3 data-[state=checked]:translate-x-3',
        Medium: 'h-4 w-4 data-[state=checked]:translate-x-4',
        Large: 'h-5 w-5 data-[state=checked]:translate-x-5',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);
