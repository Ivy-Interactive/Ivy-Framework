import { cva } from 'class-variance-authority';

export const expandableTriggerVariants = cva(
  'w-full flex justify-between items-center cursor-pointer hover:bg-accent/50 rounded-sm transition-colors disabled:cursor-not-allowed disabled:hover:bg-transparent overflow-hidden box-border shrink-0',
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
  'flex-1 min-w-0 pointer-events-none [&_button]:pointer-events-auto [&_input]:pointer-events-auto [&_select]:pointer-events-auto [&_[role="button"]]:pointer-events-auto [&_[role="switch"]]:pointer-events-auto [&_[role="checkbox"]]:pointer-events-auto [&_a[href]]:pointer-events-auto [&_button]:cursor-pointer [&_input]:cursor-default [&_[role="switch"]]:cursor-pointer [&_[role="checkbox"]]:cursor-pointer',
  {
    variants: {
      scale: {
        Small: 'ml-1 pr-7 [&_*]:text-xs',
        Medium: 'pr-9 [&_*]:text-sm',
        Large: '-ml-1 pr-11 [&_*]:text-base',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const expandableChevronContainerVariants = cva(
  'absolute top-0 bottom-0 right-0 flex items-center justify-center pointer-events-none shrink-0 z-10',
  {
    variants: {
      scale: {
        Small: 'w-7',
        Medium: 'w-9',
        Large: 'w-11',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const expandableChevronVariants = cva(
  'opacity-50 shrink-0 transition-transform duration-200 ease-in-out',
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
      Small: 'pl-3 pr-2 py-2 space-y-2 [&_*]:text-xs',
      Medium: 'pl-3 pr-3 py-4 space-y-4 [&_*]:text-sm',
      Large: 'pl-3 pr-4 py-6 space-y-5 [&_*]:text-base',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});
