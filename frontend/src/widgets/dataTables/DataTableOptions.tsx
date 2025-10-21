import React, { useState, useMemo } from 'react';
import { useTable } from './DataTableContext';
import { tableStyles } from './styles/style';
import { QueryEditor, QueryEditorChangeEvent } from 'filter-query-editor';
import { Filter } from '@/services/grpcTableService';

export const DataTableOptions: React.FC<{
  hasOptions: { allowFiltering: boolean };
}> = ({ hasOptions }) => {
  const [query, setQuery] = useState<string>('');
  const [pendingFilter, setPendingFilter] = useState<Filter | null>(null);

  const { columns, setActiveFilter } = useTable();

  const { allowFiltering } = hasOptions;

  // Filter columns to only include filterable ones (defaults to true if not specified)
  // Map DataColumn to ColumnDef format expected by QueryEditor
  const queryEditorColumns = useMemo(
    () =>
      columns
        .filter(col => col.filterable ?? true)
        .map(col => ({
          name: col.name,
          type: col.type.toLowerCase(),
          width: typeof col.width === 'number' ? col.width : 150,
        })),
    [columns]
  );

  if (columns.length === 0) {
    return null;
  }

  const handleQueryChange = (event: QueryEditorChangeEvent) => {
    setQuery(event.text);

    if (event.text.trim() === '') {
      setPendingFilter(null);
      setActiveFilter(null);
    } else if (event.isValid && event.filters) {
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
          columns={queryEditorColumns}
          onChange={handleQueryChange}
          placeholder='e.g., [Name] = "John" AND [Age] > 18'
          height={40}
          className="font-mono rounded-lg border shadow-sm [&:focus-within]:ring-1 [&:focus-within]:ring-ring"
        />
        <style>{tableStyles.queryEditor.css}</style>
      </div>
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
