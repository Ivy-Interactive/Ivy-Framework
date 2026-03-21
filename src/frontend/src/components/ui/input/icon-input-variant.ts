import { cva } from 'class-variance-authority';

export const iconInputTriggerVariant = cva(
  'justify-start font-normal min-w-[120px] max-w-[200px]',
  {
    variants: {
      density: {
        Small: 'h-8 px-3',
        Medium: 'h-9 px-4 py-2',
        Large: 'h-10 px-5 py-2',
      },
    },
    defaultVariants: {
      density: 'Medium',
    },
  }
);

export const iconInputIconVariant = cva('', {
  variants: {
    density: {
      Small: 'h-3 w-3',
      Medium: 'h-4 w-4',
      Large: 'h-5 w-5',
    },
  },
  defaultVariants: {
    density: 'Medium',
  },
});

export const iconInputTextVariant = cva('', {
  variants: {
    density: {
      Small: 'text-xs',
      Medium: 'text-sm',
      Large: 'text-base',
    },
  },
  defaultVariants: {
    density: 'Medium',
  },
});

export const iconInputPopoverVariant = cva('p-0', {
  variants: {
    density: {
      Small: 'w-[280px]',
      Medium: 'w-[320px]',
      Large: 'w-[360px]',
    },
  },
  defaultVariants: {
    density: 'Medium',
  },
});

export const iconInputPopoverScrollVariant = cva('overflow-auto', {
  variants: {
    density: {
      Small: 'h-[240px]',
      Medium: 'h-[280px]',
      Large: 'h-[320px]',
    },
  },
  defaultVariants: {
    density: 'Medium',
  },
});

export const iconInputPopoverHeaderVariant = cva('border-b', {
  variants: {
    density: {
      Small: 'p-1',
      Medium: 'p-2',
      Large: 'p-3',
    },
  },
  defaultVariants: {
    density: 'Medium',
  },
});

export const iconInputPopoverFooterVariant = cva('border-t', {
  variants: {
    density: {
      Small: 'p-1',
      Medium: 'p-2',
      Large: 'p-3',
    },
  },
  defaultVariants: {
    density: 'Medium',
  },
});

export const iconInputGridVariant = cva('grid', {
  variants: {
    density: {
      Small: 'gap-1 p-1',
      Medium: 'gap-2 p-2',
      Large: 'gap-3 p-3',
    },
  },
  defaultVariants: {
    density: 'Medium',
  },
});

export const iconInputSearchIconVariant = cva(
  'absolute top-1/2 -translate-y-1/2 text-muted-foreground',
  {
    variants: {
      density: {
        Small: 'left-3 h-3 w-3',
        Medium: 'left-2.5 h-4 w-4',
        Large: 'left-3 h-5 w-5',
      },
    },
    defaultVariants: {
      density: 'Medium',
    },
  }
);

export const iconInputSearchInputVariant = cva('', {
  variants: {
    density: {
      Small: 'pl-6',
      Medium: 'pl-8',
      Large: 'pl-9',
    },
  },
  defaultVariants: {
    density: 'Medium',
  },
});

export const iconInputEmptyStateVariant = cva(
  'flex items-center justify-center text-muted-foreground',
  {
    variants: {
      density: {
        Small: 'h-[120px] text-xs',
        Medium: 'h-[140px] text-sm',
        Large: 'h-[160px] text-base',
      },
    },
    defaultVariants: {
      density: 'Medium',
    },
  }
);
