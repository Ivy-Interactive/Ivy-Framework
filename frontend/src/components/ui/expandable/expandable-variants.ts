import { cva } from 'class-variance-authority';

export const expandableTriggerVariants = cva(
  'w-full flex justify-between items-center cursor-pointer hover:bg-accent/50 rounded-sm transition-colors data-[disabled=true]:cursor-not-allowed data-[disabled=true]:hover:bg-transparent overflow-hidden box-border shrink-0',
  {
    variants: {
      scale: {
        Small: 'h-7 px-2 py-1 gap-2',
        Medium: 'h-9 px-3 py-2 gap-3',
        Large: 'h-11 px-4 py-3 gap-4',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const expandableHeaderVariants = cva(
  'flex-1 min-w-0 pointer-events-none [&_button]:pointer-events-auto [&_input]:pointer-events-auto [&_select]:pointer-events-auto [&_[role="button"]]:pointer-events-auto [&_[role="switch"]]:pointer-events-auto [&_[role="checkbox"]]:pointer-events-auto [&_a[href]]:pointer-events-auto',
  {
    variants: {
      scale: {
        Small: '[&_*]:text-xs',
        Medium: '[&_*]:text-sm',
        Large: '[&_*]:text-base',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const expandableChevronContainerVariants = cva(
  'p-0 shrink-0 pointer-events-none flex items-center justify-center',
  {
    variants: {
      scale: {
        Small: 'h-7 w-7',
        Medium: 'h-9 w-9',
        Large: 'h-11 w-11',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const expandableChevronVariants = cva(
  'transition-transform duration-200 ease-in-out',
  {
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
  }
);

export const expandableContentVariants = cva('overflow-hidden transition-all', {
  variants: {
    scale: {
      Small: 'pl-1 pr-2 py-2 space-y-2 [&_*]:text-xs',
      Medium: 'pl-1 pr-3 py-4 space-y-4 [&_*]:text-sm',
      Large: 'pl-1.5 pr-4 py-6 space-y-5 [&_*]:text-base',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});
