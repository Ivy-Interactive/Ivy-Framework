import { cva } from 'class-variance-authority';

export const calendarVariants = cva(
  'bg-background group/calendar p-3 [[data-slot=card-content]_&]:bg-transparent [[data-slot=popover-content]_&]:bg-transparent',
  {
    variants: {
      scale: {
        Small: '[--cell-size:--spacing(6)]',
        Medium: '[--cell-size:--spacing(8)]',
        Large: '[--cell-size:--spacing(10)]',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const calendarButtonVariants = cva(
  'aria-disabled:opacity-50 p-0 select-none',
  {
    variants: {
      scale: {
        Small: 'size-(--cell-size) text-xs',
        Medium: 'size-(--cell-size) text-sm',
        Large: 'size-(--cell-size) text-base',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const calendarCaptionVariants = cva(
  'flex items-center justify-center w-full px-(--cell-size)',
  {
    variants: {
      scale: {
        Small: 'h-(--cell-size) text-xs',
        Medium: 'h-(--cell-size) text-sm',
        Large: 'h-(--cell-size) text-base',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const calendarWeekdayVariants = cva(
  'text-muted-foreground rounded-md flex-1 font-normal select-none',
  {
    variants: {
      scale: {
        Small: 'text-[0.7rem]',
        Medium: 'text-[0.8rem]',
        Large: 'text-[0.9rem]',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const calendarDayVariants = cva(
  'items-center justify-center whitespace-nowrap rounded-md transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 cursor-pointer hover:bg-accent data-[selected-single=true]:bg-primary data-[selected-single=true]:text-primary-foreground data-[range-middle=true]:bg-accent data-[range-middle=true]:text-accent-foreground data-[range-start=true]:bg-primary data-[range-start=true]:text-primary-foreground data-[range-end=true]:bg-primary data-[range-end=true]:text-primary-foreground group-data-[focused=true]/day:border-ring group-data-[focused=true]/day:ring-ring/50 dark:hover:text-accent-foreground flex aspect-square size-auto w-full min-w-(--cell-size) flex-col gap-1 leading-none font-normal group-data-[focused=true]/day:relative group-data-[focused=true]/day:z-10 group-data-[focused=true]/day:ring-[3px] data-[range-end=true]:rounded-md data-[range-end=true]:rounded-r-md data-[range-middle=true]:rounded-none data-[range-start=true]:rounded-md data-[range-start=true]:rounded-l-md',
  {
    variants: {
      scale: {
        Small: 'text-xs [&>span]:text-[0.6rem]',
        Medium: 'text-sm [&>span]:text-xs',
        Large: 'text-base [&>span]:text-sm',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);
