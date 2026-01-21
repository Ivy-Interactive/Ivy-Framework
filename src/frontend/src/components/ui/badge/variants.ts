import { cva } from 'class-variance-authority';

export const badgeVariants = cva(
  'inline-flex items-center rounded-full border font-normal leading-none transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
  {
    variants: {
      variant: {
        primary:
          'border-transparent bg-primary text-primary-foreground shadow hover:bg-primary/80',
        secondary:
          'border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80',
        destructive:
          'border-transparent bg-destructive text-destructive-foreground shadow hover:bg-destructive/80',
        outline: 'text-foreground',
        success:
          'border-transparent bg-success text-success-foreground shadow hover:bg-success/80',
        warning:
          'border-transparent bg-warning text-warning-foreground shadow hover:bg-warning/80',
        info: 'border-transparent bg-info text-info-foreground shadow hover:bg-info/80',
      },
      scale: {
        medium: 'px-2 py-0.5 text-xs',
        small: 'px-1 py-0 text-[10px]',
        large: 'px-3 py-1 text-sm',
      },
    },
    defaultVariants: {
      variant: 'primary',
      scale: 'medium',
    },
  }
);
