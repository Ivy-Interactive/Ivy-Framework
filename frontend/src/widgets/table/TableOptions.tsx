import React, { useState } from 'react';
import { useTable } from './TableContext';
import { tableStyles } from './styles/style';
import { Button } from '@/components/ui/button';
import { QueryEditor, QueryEditorChangeEvent } from 'filter-query-editor';
import { Filter } from '@/services/grpcTableService';

export const TableOptions: React.FC<{
  hasOptions: { allowFiltering: boolean };
}> = ({ hasOptions }) => {
  const [query, setQuery] = useState<string>('');
  const [pendingFilter, setPendingFilter] = useState<Filter | null>(null);

  const { columns, activeFilter, setActiveFilter } = useTable();

  const { allowFiltering } = hasOptions;

  if (columns.length === 0) {
    return null;
  }

  const handleQueryChange = (event: QueryEditorChangeEvent) => {
    setQuery(event.text);

    if (event.isValid && event.filters) {
      setPendingFilter({ group: event.filters });
    } else {
      setPendingFilter(null);
    }
  };

  const handleKeyDown = (event: React.KeyboardEvent) => {
    if (
      event.key === 'Enter' &&
      (event.metaKey || event.ctrlKey || !event.shiftKey)
    ) {
      event.preventDefault();
      if (pendingFilter) {
        setActiveFilter(pendingFilter);
      } else {
        setActiveFilter(null);
      }
    }
  };

  const queryEditorContent = (
    <div className="flex gap-2 flex-col sm:flex-row">
      <div
        className="w-full min-w-[300px] query-editor-wrapper"
        onKeyDown={handleKeyDown}
      >
        <QueryEditor
          value={query}
          columns={columns}
          onChange={handleQueryChange}
          placeholder='e.g., name = "John" AND age > 18 '
          height={40}
          className="font-mono rounded-lg border shadow-sm [&:focus-within]:ring-1 [&:focus-within]:ring-ring"
        />
        {/* TODO: we're force overwriting styling. maybe it's better to export styöing direclty from the module */}
        <style>{`
          .query-editor-wrapper .cm-editor.cm-focused {
            outline: none !important;
          }
          .query-editor-wrapper .cm-content {
            padding: 8px 16px;
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
            font-size: 14px !important;
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
            font-size: 12px !important;
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
        `}</style>
      </div>
      {activeFilter && (
        <Button
          variant="outline"
          onClick={() => {
            setActiveFilter(null);
            setQuery('');
            setPendingFilter(null);
          }}
        >
          Clear
        </Button>
      )}
    </div>
  );

  return (
    <div style={tableStyles.tableOptions.container}>
      <div className={tableStyles.tableOptions.inner}>
        {allowFiltering && queryEditorContent}
      </div>
    </div>
  );
};
