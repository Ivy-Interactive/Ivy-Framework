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

export const expandableHeaderVariants = cva('flex-1 min-w-0', {
  variants: {
    scale: {
      Small: 'ml-1.5 pr-8 [&_*]:text-xs',
      Medium: 'ml-2 pr-9 [&_*]:text-sm',
      Large: 'ml-2.5 pr-11 [&_*]:text-base',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

export const expandableChevronContainerVariants = cva(
  'absolute top-0 bottom-0 border-l flex items-center justify-end shrink-0 right-2.5 pointer-events-none',
  {
    variants: {
      scale: {
        Small: 'w-5',
        Medium: 'w-6',
        Large: 'w-8',
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
      Small: 'p-2 space-y-2 [&_*]:text-xs',
      Medium: 'p-4 space-y-4 [&_*]:text-sm',
      Large: 'p-6 space-y-5 [&_*]:text-base',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});
