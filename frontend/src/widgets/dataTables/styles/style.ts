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
      borderBottomColor: 'var(--border)',
      borderBottomWidth: '1px',
      borderBottomStyle: 'solid',
    },
    inner: 'flex items-center gap-4 px-3 py-3',
    leftSection: 'flex items-center gap-4',
    rightSection: 'flex items-center gap-2',
    dialog: {
      content: 'bg-white p-6 rounded-lg max-w-[600px] flex flex-col gap-4',
      header: 'flex items-center justify-between',
      footer: 'flex items-center justify-between',
      filterIcon: 'w-4 h-4 mr-2',
      closeIcon: 'w-[9.251px] h-[9.251px]',
      inputError: 'border-red-500 focus-visible:ring-red-500',
      errorText: 'text-sm text-red-500 mt-1',
      helpText: 'text-xs text-muted-foreground mt-2',
      examplesList: 'list-disc list-inside space-y-1 mt-1',
    },
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
