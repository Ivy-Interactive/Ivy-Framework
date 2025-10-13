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
        className="w-full sm:w-[600px] query-editor-wrapper"
        onKeyDown={handleKeyDown}
      >
        <QueryEditor
          value={query}
          columns={columns}
          onChange={handleQueryChange}
          placeholder='e.g., name = "John" AND age > 18 '
          height={40}
          className="font-mono rounded-lg border shadow-sm [&:focus-within]:ring-2 [&:focus-within]:ring-ring [&:focus-within]:ring-offset-2"
        />
        {/* TODO: don't force styling */}
        <style>{`
          .query-editor-wrapper .cm-editor.cm-focused {
            outline: none !important;
          }
          .query-editor-wrapper .cm-content {
            padding: 8px 16px;
            min-height: auto;
          }
        `}</style>
      </div>
      {activeFilter && (
        <Button variant="outline" onClick={() => setActiveFilter(null)}>
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
