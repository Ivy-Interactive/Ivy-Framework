import { cva } from 'class-variance-authority';
export const inputVariants = cva(
  'flex w-full rounded-md border border-input bg-transparent shadow-sm transition-colors file:border-0 file:bg-transparent file:text-sm file:font-medium file:text-foreground placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50',
  {
    variants: {
      size: {
        Small: 'h-7 px-2 py-1 text-xs',
        Medium: 'h-9 px-3 py-2 text-sm',
        Large: 'h-11 px-4 py-3 text-base',
      },
    },
    defaultVariants: {
      size: 'Medium',
    },
  }
);
