import { cva } from 'class-variance-authority';

export const iconInputTriggerVariants = cva(
  'justify-start font-normal min-w-[120px] max-w-[200px]',
  {
    variants: {
      scale: {
        Small: 'h-8 px-3',
        Medium: 'h-9 px-4 py-2',
        Large: 'h-10 px-5 py-2',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const iconInputIconVariants = cva('', {
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
});

export const iconInputTextVariants = cva('', {
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

export const iconInputPopoverVariants = cva('p-0', {
  variants: {
    scale: {
      Small: 'w-[280px]',
      Medium: 'w-[320px]',
      Large: 'w-[360px]',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

export const iconInputPopoverScrollVariants = cva('overflow-auto', {
  variants: {
    scale: {
      Small: 'h-[240px]',
      Medium: 'h-[280px]',
      Large: 'h-[320px]',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

export const iconInputPopoverHeaderVariants = cva('border-b', {
  variants: {
    scale: {
      Small: 'p-1',
      Medium: 'p-2',
      Large: 'p-3',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

export const iconInputPopoverFooterVariants = cva('border-t', {
  variants: {
    scale: {
      Small: 'p-1',
      Medium: 'p-2',
      Large: 'p-3',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

export const iconInputGridVariants = cva('grid', {
  variants: {
    scale: {
      Small: 'gap-1 p-1',
      Medium: 'gap-2 p-2',
      Large: 'gap-3 p-3',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

export const iconInputSearchIconVariants = cva(
  'absolute top-1/2 -translate-y-1/2 text-muted-foreground',
  {
    variants: {
      scale: {
        Small: 'left-3 h-3 w-3',
        Medium: 'left-2.5 h-4 w-4',
        Large: 'left-3 h-5 w-5',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);

export const iconInputSearchInputVariants = cva('', {
  variants: {
    scale: {
      Small: 'pl-6',
      Medium: 'pl-8',
      Large: 'pl-9',
    },
  },
  defaultVariants: {
    scale: 'Medium',
  },
});

export const iconInputEmptyStateVariants = cva(
  'flex items-center justify-center text-muted-foreground',
  {
    variants: {
      scale: {
        Small: 'h-[120px] text-xs',
        Medium: 'h-[140px] text-sm',
        Large: 'h-[160px] text-base',
      },
    },
    defaultVariants: {
      scale: 'Medium',
    },
  }
);
