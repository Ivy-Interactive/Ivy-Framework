import { cva } from 'class-variance-authority';

export const selectTriggerVariants = cva(
  'flex w-full items-center justify-between whitespace-nowrap rounded-md border border-input bg-transparent shadow-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50 [&>span:first-child]:flex-1 [&>span:first-child]:min-w-0 [&>span:first-child]:truncate [&>span:first-child]:text-left cursor-pointer',
  {
    variants: {
      scale: {
        Small: 'h-7 px-2 py-1 text-xs font-medium',
        Medium: 'h-9 px-3 py-1 text-sm font-medium',
        Large: 'h-11 px-4 py-2 text-base font-medium',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const selectContentVariants = cva(
  'relative z-50 max-h-96 min-w-[8rem] overflow-hidden rounded-md border bg-popover text-popover-foreground shadow-md data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95 data-[side=bottom]:slide-in-from-top-2 data-[side=left]:slide-in-from-right-2 data-[side=right]:slide-in-from-left-2 data-[side=top]:slide-in-from-bottom-2',
  {
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
  }
);

export const selectItemVariants = cva(
  'relative flex w-full cursor-pointer select-none items-center rounded-sm py-1.5 pl-2 pr-8 outline-none focus:bg-accent focus:text-accent-foreground data-[disabled]:pointer-events-none data-[disabled]:opacity-50',
  {
    variants: {
      scale: {
        Small: 'py-1 pl-1.5 pr-6 text-xs font-medium',
        Medium: 'py-1.5 pl-2 pr-8 text-sm font-medium',
        Large: 'py-2 pl-2.5 pr-10 text-base font-medium',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);
