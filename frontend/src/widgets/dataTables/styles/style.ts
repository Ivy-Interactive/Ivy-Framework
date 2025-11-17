// Component-based styles for table widgets
export const tableStyles = {
  // Main Table component
  table: {
    container: {
      display: 'flex',
      flexDirection: 'column',
      height: '100%',
    } as React.CSSProperties,
    heading: 'text-display-25 font-bold mb-4',
  },

  // TableOptions component
  tableOptions: {
    container: {
      width: '100%',
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
      height: '100%',
      width: '100%',
      border: '1px solid var(--border)',
      borderRadius: 'var(--radius)',
      overflow: 'hidden',
    },
    gridContainerWithOptions: {
      height: '100%',
      width: '100%',
      border: '1px solid var(--border)',
      borderRadius: 'var(--radius)',
      overflow: 'hidden',
    },
  },

  // LoadingDisplay component
  loadingDisplay: {
    container:
      'flex items-center justify-center h-64 text-[color:var(--muted-foreground)]',
  },

  // QueryEditor component
  queryEditor: {
    css: `
      .query-editor-wrapper .cm-editor {
        border: none !important;
        border-radius: 0 !important;
        box-shadow: none !important;
      }
      .query-editor-wrapper .cm-editor.cm-focused {
        outline: none !important;
        border: none !important;
        box-shadow: none !important;
      }
      .query-editor-wrapper .cm-content {
        padding: 12px 16px 10px 16px;
        min-height: auto;
      }

      /* Autocomplete dropdown styling - shadcn style */
      .cm-tooltip-autocomplete {
        background: var(--popover) !important;
        border: 1px solid var(--border) !important;
        border-radius: calc(var(--radius) - 2px) !important;
        box-shadow: var(--shadow-md) !important;
        padding: 4px !important;
        font-family: var(--font-mono) !important;
        font-size: 12px !important;
        max-height: 300px !important;
        overflow-y: auto !important;
      }

      .cm-tooltip-autocomplete > ul {
        list-style: none !important;
        margin: 0 !important;
        padding: 0 !important;
      }

      .cm-tooltip-autocomplete > ul > li {
        padding: 6px 8px !important;
        margin: 2px 0 !important;
        border-radius: calc(var(--radius) - 4px) !important;
        cursor: pointer !important;
        color: var(--foreground) !important;
        transition: background-color 0.15s ease-in-out !important;
        display: flex !important;
        justify-content: space-between !important;
        align-items: center !important;
      }

      .cm-tooltip-autocomplete > ul > li[aria-selected="true"] {
        background: var(--accent) !important;
        color: var(--accent-foreground) !important;
      }

      .cm-tooltip-autocomplete > ul > li:hover {
        background: var(--accent) !important;
      }

      /* Completion item details */
      .cm-completionLabel {
        font-family: var(--font-mono) !important;
        color: var(--foreground) !important;
      }

      .cm-completionDetail {
        font-family: var(--font-mono) !important;
        font-size: 11px !important;
        color: var(--muted-foreground) !important;
        font-style: normal !important;
        margin-left: 8px !important;
      }

      .cm-completionInfo {
        background: var(--popover) !important;
        border: 1px solid var(--border) !important;
        border-radius: calc(var(--radius) - 2px) !important;
        padding: 8px !important;
        color: var(--foreground) !important;
        font-size: 13px !important;
      }

      /* Scrollbar for dropdown */
      .cm-tooltip-autocomplete::-webkit-scrollbar {
        width: 8px;
      }

      .cm-tooltip-autocomplete::-webkit-scrollbar-track {
        background: transparent;
      }

      .cm-tooltip-autocomplete::-webkit-scrollbar-thumb {
        background: var(--border);
        border-radius: 4px;
      }

      .cm-tooltip-autocomplete::-webkit-scrollbar-thumb:hover {
        background: var(--muted-foreground);
      }

      /* Disable syntax highlighting when query is invalid */
      .query-editor-wrapper[data-query-valid="false"] [class^="cm-query-"] {
        color: currentColor !important;
        font-weight: normal !important;
      }
    `,
  },
} as const;
