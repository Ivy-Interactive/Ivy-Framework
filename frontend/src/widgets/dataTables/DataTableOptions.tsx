import React, { useState, useMemo, useCallback, useRef } from 'react';
import { useTable } from './DataTableContext';
import { tableStyles } from './styles/style';
import {
  QueryEditor,
  QueryEditorChangeEvent,
  parseQuery,
} from 'filter-query-editor';
import { Filter } from '@/services/grpcTableService';
import { parseInvalidQuery } from './utils/tableDataFetcher';

export const DataTableOptions: React.FC<{
  hasOptions: { allowFiltering: boolean };
}> = ({ hasOptions }) => {
  const [query, setQuery] = useState<string>('');
  const [pendingFilter, setPendingFilter] = useState<Filter | null>(null);
  const [isParsing, setIsParsing] = useState(false);
  const lastInvalidQueryRef = useRef<string>('');

  const { columns, setActiveFilter, connection } = useTable();

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

  const handleQueryChange = useCallback(
    async (event: QueryEditorChangeEvent) => {
      setQuery(event.text);

      if (event.text.trim() === '') {
        setPendingFilter(null);
        setActiveFilter(null);
        lastInvalidQueryRef.current = '';
      } else if (event.isValid && event.filters) {
        setPendingFilter({ group: event.filters });
        lastInvalidQueryRef.current = '';
      } else if (!event.isValid && event.text.trim() !== '') {
        // Query is invalid and not empty
        setPendingFilter(null);

        // Avoid re-parsing the same invalid query
        if (lastInvalidQueryRef.current === event.text || isParsing) {
          return;
        }

        lastInvalidQueryRef.current = event.text;
        setIsParsing(true);

        try {
          // Call parseFilter endpoint with the invalid query string
          const result = await parseInvalidQuery(event.text, connection);

          if (result.filterExpression) {
            // Parse the corrected query string back to AST using parseQuery
            const parsedResult = parseQuery(
              result.filterExpression,
              queryEditorColumns
            );

            if (parsedResult.isValid && parsedResult.filters) {
              // Update the query editor with the corrected query
              setQuery(result.filterExpression);
              setPendingFilter({ group: parsedResult.filters });
            }
          }
        } catch (error) {
          console.error('Failed to parse invalid query:', error);
        } finally {
          setIsParsing(false);
        }
      } else {
        setPendingFilter(null);
      }
    },
    [queryEditorColumns, isParsing, connection]
  );

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
