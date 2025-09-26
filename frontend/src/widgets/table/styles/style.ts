// Component-based styles for table widgets
export const tableStyles = {
  // Main Table component
  table: {
    container: 'p-4',
    heading: 'text-display-25 font-bold mb-4',
  },

  // TableOptions component
  tableOptions: {
    container: {
      width: '100%',
      border: '1px solid var(--border)',
      borderBottom: 'none',
      borderRadius: 'var(--radius) var(--radius) 0 0',
      backgroundColor: 'var(--muted)',
      borderBottomColor: 'var(--border)',
      borderBottomWidth: '1px',
      borderBottomStyle: 'solid',
    },
    inner: 'flex items-center justify-between px-4 py-3',
    leftSection: 'flex items-center gap-4',
    rightSection: 'flex items-center gap-2',
  },

  // TableHeader component
  tableHeader: {
    container:
      'text-descriptive text-[color:var(--primary-foreground)] flex items-center gap-4',
    text: 'text-descriptive text-[color:var(--primary-foreground)]',
    accent: 'text-[color:var(--primary)]',
    muted: 'text-[color:var(--muted-foreground)]',
    spinner: {
      container: 'flex items-center',
      element:
        'animate-spin h-4 w-4 border-2 border-[color:var(--muted-foreground)] border-t-transparent rounded-full mr-2',
    },
  },

  // TableFooter component
  tableFooter: {
    container: 'mt-4 text-descriptive text-[color:var(--muted-foreground)]',
  },

  // TableEditor component
  tableEditor: {
    gridContainer: {
      height: '600px',
      width: '100%',
      border: '1px solid var(--border)',
      borderRadius: 'var(--radius)',
      overflow: 'hidden',
    },
    gridContainerWithOptions: {
      height: '600px',
      width: '100%',
      border: '1px solid var(--border)',
      borderTop: 'none',
      borderRadius: '0 0 var(--radius) var(--radius)',
      overflow: 'hidden',
    },
  },

  // LoadingDisplay component
  loadingDisplay: {
    container:
      'flex items-center justify-center h-64 text-[color:var(--muted-foreground)]',
  },
} as const;
