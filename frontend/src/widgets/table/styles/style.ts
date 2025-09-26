// Base styling tokens using CSS variables
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
    sm: 'text-descriptive', // 12px from index.css
    base: 'text-semi-lead', // 15px from index.css
    xl: 'text-display-25', // 25px from index.css
  },
  weight: {
    bold: 'font-bold',
  },
  color: {
    primary: 'text-[color:var(--primary-foreground)]',
    secondary: 'text-[color:var(--muted-foreground)]',
    error: 'text-[color:var(--destructive)]',
    accent: 'text-[color:var(--primary)]',
    white: 'text-[color:var(--background)]',
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
    primary: `${spacing.padding.x.sm} ${spacing.padding.y.xs} bg-[color:var(--primary)] ${typography.color.white} rounded hover:bg-[color:var(--primary)]/90`,
    secondary: `inline-flex items-center px-3 py-1.5 border border-[color:var(--border)] shadow-sm ${typography.size.sm} font-medium rounded ${typography.color.primary} bg-[color:var(--background)] hover:bg-[color:var(--muted)] focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-[color:var(--ring)]`,
  },
  spinner: {
    sm: `animate-spin h-4 w-4 border-2 border-[color:var(--muted-foreground)] border-t-transparent rounded-full`,
  },
  container: {
    bordered: 'border border-[color:var(--border)] rounded-lg overflow-hidden',
  },
} as const;

// Composed styles for table
export const tableStyles = {
  // Layout containers
  container: spacing.sm,
  gridContainer: {
    ...layout.dimensions.fixed.grid,
    border: '1px solid var(--border)',
    borderRadius: 'var(--radius)',
    overflow: 'hidden',
  },
  gridContainerWithOptions: {
    ...layout.dimensions.fixed.grid,
    border: '1px solid var(--border)',
    borderTop: 'none',
    borderRadius: '0 0 var(--radius) var(--radius)',
    overflow: 'hidden',
  },
  optionsContainer: {
    width: '100%',
    border: '1px solid var(--border)',
    borderBottom: 'none',
    borderRadius: 'var(--radius) var(--radius) 0 0',
    backgroundColor: 'var(--muted)',
    borderBottomColor: 'var(--border)',
    borderBottomWidth: '1px',
    borderBottomStyle: 'solid',
  },

  // Typography
  heading: {
    primary: `${typography.size.xl} ${typography.weight.bold} ${spacing.margin.bottom.sm}`,
  },
  text: {
    info: `${typography.size.sm} ${typography.color.primary} ${layout.flex.row} ${spacing.gap.md}`,
    footer: `${spacing.margin.top.sm} ${typography.size.sm} ${typography.color.secondary}`,
    error: `${typography.color.error} ${spacing.margin.bottom.sm}`,
    muted: typography.color.secondary,
    accent: typography.color.accent,
    optionsHeader: `${typography.size.sm} font-medium ${typography.color.primary}`,
  },

  // Layout utilities
  flex: {
    center: `${layout.flex.center} ${layout.dimensions.fixed.sm} ${typography.color.secondary}`,
    row: layout.flex.row,
  },
  options: {
    container: `${layout.flex.row} justify-between ${spacing.padding.x.sm} py-3`,
    leftSection: `${layout.flex.row} ${spacing.gap.md}`,
    rightSection: `${layout.flex.row} gap-2`,
  },

  // Components
  spinner: {
    container: layout.flex.row,
    element: `${components.spinner.sm} ${spacing.margin.right.xs}`,
  },
  button: {
    primary: components.button.primary,
    secondary: components.button.secondary,
  },

  // Spacing utilities
  padding: {
    container: spacing.sm,
    right: spacing.padding.right.xs,
  },
} as const;
