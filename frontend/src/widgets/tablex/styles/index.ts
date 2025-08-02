// Base styling tokens
const spacing = {
  xs: 'p-2',
  sm: 'p-4',
  md: 'p-6',
  lg: 'p-8',
  gap: {
    sm: 'gap-2',
    md: 'gap-4',
  },
  margin: {
    bottom: {
      xs: 'mb-2',
      sm: 'mb-4',
    },
    top: {
      sm: 'mt-4',
    },
    right: {
      xs: 'mr-2',
    },
  },
  padding: {
    x: {
      sm: 'px-4',
    },
    y: {
      xs: 'py-2',
    },
    right: {
      xs: 'pr-2',
    },
  },
} as const;

const typography = {
  size: {
    sm: 'text-sm',
    base: 'text-base',
    xl: 'text-2xl',
  },
  weight: {
    bold: 'font-bold',
  },
  color: {
    primary: 'text-gray-600',
    secondary: 'text-gray-500',
    error: 'text-red-600',
    accent: 'text-blue-600',
    white: 'text-white',
  },
} as const;

const layout = {
  flex: {
    center: 'flex items-center justify-center',
    row: 'flex items-center',
  },
  dimensions: {
    full: {
      height: '100%',
      width: '100%',
    },
    fixed: {
      sm: 'h-64',
      grid: {
        height: '600px',
        width: '100%',
      },
    },
  },
} as const;

const components = {
  button: {
    primary: `${spacing.padding.x.sm} ${spacing.padding.y.xs} bg-blue-500 ${typography.color.white} rounded hover:bg-blue-600`,
  },
  spinner: {
    sm: `animate-spin h-4 w-4 border-2 border-gray-500 border-t-transparent rounded-full`,
  },
  container: {
    bordered: 'border border-gray-300 rounded-lg overflow-hidden',
  },
} as const;

// Composed styles for table
export const tableStyles = {
  // Layout containers
  container: spacing.sm,
  gridContainer: {
    ...layout.dimensions.fixed.grid,
    border: '1px solid #e5e7eb',
    borderRadius: '0.5rem',
    overflow: 'hidden',
  },

  // Typography
  heading: {
    primary: `${typography.size.xl} ${typography.weight.bold} ${spacing.margin.bottom.sm}`,
  },
  text: {
    info: `${spacing.margin.bottom.xs} ${typography.size.sm} ${typography.color.primary} ${layout.flex.row} ${spacing.gap.md}`,
    footer: `${spacing.margin.top.sm} ${typography.size.sm} ${typography.color.secondary}`,
    error: `${typography.color.error} ${spacing.margin.bottom.sm}`,
    muted: typography.color.secondary,
    accent: typography.color.accent,
  },

  // Layout utilities
  flex: {
    center: `${layout.flex.center} ${layout.dimensions.fixed.sm} ${typography.color.secondary}`,
    row: layout.flex.row,
  },

  // Components
  spinner: {
    container: layout.flex.row,
    element: `${components.spinner.sm} ${spacing.margin.right.xs}`,
  },
  button: {
    primary: components.button.primary,
  },

  // Spacing utilities
  padding: {
    container: spacing.sm,
    right: spacing.padding.right.xs,
  },
} as const;
