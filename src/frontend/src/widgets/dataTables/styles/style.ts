// Component-based styles for table widgets
export const tableStyles = {
  // Main Table component
  table: {
    container: {
      display: "flex",
      flexDirection: "column",
      height: "100%",
      flex: 1,
      minHeight: 0,
    } as React.CSSProperties,
    heading: "text-display-25 font-bold mb-4",
  },

  // TableOptions component
  tableOptions: {
    container: {
      width: "100%",
    },
    inner: "flex items-center gap-4 p-3",
    leftSection: "flex items-center gap-4",
    rightSection: "flex items-center gap-2",
    dialog: {
      content: "bg-white p-6 rounded-lg max-w-[600px] flex flex-col gap-4",
      header: "flex items-center justify-between",
      footer: "flex items-center justify-between",
      filterIcon: "size-4 mr-2",
      closeIcon: "w-[9.251px] h-[9.251px]",
      inputError: "border-red-500 focus-visible:ring-red-500",
      errorText: "text-sm text-red-500 mt-1",
      helpText: "text-xs text-muted-foreground mt-2",
      examplesList: "list-disc list-inside gap-y-1 mt-1",
    },
  },

  // TableEditor component
  tableEditor: {
    gridContainer: {
      flex: 1,
      minHeight: 0,
      width: "100%",
      border: "1px solid var(--border)",
      borderRadius: "var(--radius-boxes)",
      overflow: "hidden",
    },
    gridContainerWithOptions: {
      flex: 1,
      minHeight: 0,
      width: "100%",
      border: "1px solid var(--border)",
      borderRadius: "var(--radius-boxes)",
      overflow: "hidden",
    },
    footer: {
      flexShrink: 0,
      width: "100%",
      backgroundColor: "var(--background)",
      borderTop: "1px solid var(--border)",
      padding: "8px 0 0 0",
    } as React.CSSProperties,
  },

  // LoadingDisplay component
  loadingDisplay: {
    container: "flex items-center justify-center h-64 text-[color:var(--muted-foreground)]",
  },

  // QueryEditor component
  queryEditor: {
    css: `
      /* Fill the panel: bordered + ring box must be block-width, not content-sized (fixes shrink-wrap). */
      .query-editor-wrapper {
        display: flex !important;
        flex-direction: column !important;
        width: 100% !important;
        max-width: 100% !important;
        min-width: 0 !important;
        box-sizing: border-box !important;
      }

      .query-editor-wrapper > div {
        width: 100% !important;
        max-width: 100% !important;
        height: 100% !important;
        min-height: 0 !important;
        flex: 1 1 0%;
        display: flex !important;
        flex-direction: column !important;
        box-sizing: border-box !important;
      }

      .query-editor-wrapper .cm-editor {
        border: none !important;
        border-radius: 0 !important;
        box-shadow: none !important;
        flex: 1 1 auto !important;
        min-height: 0 !important;
        min-width: 0 !important;
        width: 100% !important;
        max-width: 100% !important;
        height: 100% !important;
        overflow: hidden !important;
      }
      .query-editor-wrapper .cm-editor.cm-focused {
        outline: none !important;
        border: none !important;
        box-shadow: none !important;
      }

      .query-editor-wrapper .cm-scroller {
        width: 100% !important;
        max-width: 100% !important;
        height: 100% !important;
        min-height: 0 !important;
        min-width: 0 !important;
        overflow-x: auto !important;
        overflow-y: hidden !important;
        overflow-y: clip !important;
        overscroll-behavior-x: contain !important;
        overscroll-behavior-y: none !important;
        /* Like a one-line text input: no vertical pan/scroll; horizontal only */
        touch-action: pan-x !important;
        scrollbar-width: none !important;
      }

      .query-editor-wrapper .cm-scroller::-webkit-scrollbar {
        display: none !important;
      }

      .query-editor-wrapper .cm-content {
        box-sizing: border-box !important;
        /* At least full scroller width; grow wider than viewport for long lines so .cm-scroller scrolls horizontally. */
        min-width: 100% !important;
        width: max-content !important;
        max-width: none !important;
        min-height: auto !important;
        overflow-x: visible !important;
        overflow-y: hidden !important;
        overflow-y: clip !important;
        padding: 8px 12px !important;
        font-size: 0.875rem !important;
        line-height: 1.25rem !important;
        caret-color: var(--foreground) !important;
      }

      .query-editor-wrapper[data-has-clear="true"] .cm-content,
      .query-editor-wrapper[data-is-parsing="true"] .cm-content {
        padding-right: 3.25rem !important;
      }

      .query-editor-wrapper .cm-line {
        padding: 0 !important;
      }

      .query-editor-wrapper .cm-placeholder {
        color: var(--muted-foreground) !important;
        font-size: 0.875rem !important;
        line-height: 1.25rem !important;
      }

      /* Match CodeInput theme: caret must contrast in light/dark (CodeMirror draws the cursor with a border) */
      .query-editor-wrapper .cm-cursor,
      .query-editor-wrapper .cm-dropCursor,
      .query-editor-wrapper .cm-cursor-primary {
        border-left-color: var(--foreground) !important;
        border-left-width: 2px !important;
      }

      /* Autocomplete dropdown styling - shadcn style */
      .cm-tooltip-autocomplete {
        background: var(--popover) !important;
        border: 1px solid var(--border) !important;
        border-radius: calc(var(--radius-boxes) - 2px) !important;
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
        border-radius: calc(var(--radius-boxes) - 4px) !important;
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
        border-radius: calc(var(--radius-boxes) - 2px) !important;
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
