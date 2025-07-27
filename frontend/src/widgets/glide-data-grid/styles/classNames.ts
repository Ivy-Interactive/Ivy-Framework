export const classNames = {
  // Container styles
  container: {
    main: 'flex flex-col h-full',
    error: 'p-4',
    content: 'flex-1 p-4',
    loading: 'flex items-center justify-center h-full',
    empty: 'flex items-center justify-center h-full text-muted-foreground',
  },

  // Header styles
  header: {
    wrapper: 'flex items-center justify-between p-4 border-b',
    content: 'flex-1',
    title: 'text-lg font-semibold',
    description: 'text-sm text-muted-foreground',
    actions: 'flex items-center gap-2',
  },

  // Button styles
  button: {
    refresh: 'flex items-center gap-1',
  },

  // Icon styles
  icon: {
    small: 'h-3 w-3',
    medium: 'h-4 w-4',
    spinner: 'animate-spin',
    refreshIcon: (loading: boolean) =>
      `h-3 w-3${loading ? ' animate-spin' : ''}`,
  },

  // Loading styles
  loading: {
    container: 'flex items-center gap-2',
  },

  // Badge styles
  badge: {
    container: 'flex items-center gap-1',
  },
} as const;
