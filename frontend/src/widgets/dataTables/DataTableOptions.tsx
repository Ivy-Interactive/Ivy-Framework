import React, { useState, useMemo, useCallback } from 'react';
import { useTable } from './DataTableContext';
import { tableStyles } from './styles/style';
import {
  QueryEditor,
  QueryEditorChangeEvent,
  parseQuery,
} from 'filter-query-editor';
import { Filter } from '@/services/grpcTableService';
import { parseInvalidQuery } from './utils/tableDataFetcher';
import { Loader2 } from 'lucide-react';

export const DataTableOptions: React.FC<{
  hasOptions: { allowFiltering: boolean; allowLlmFiltering: boolean };
}> = ({ hasOptions }) => {
  const [query, setQuery] = useState<string>('');
  const [pendingFilter, setPendingFilter] = useState<Filter | null>(null);
  const [isParsing, setIsParsing] = useState(false);
  const [isQueryValid, setIsQueryValid] = useState(true);

  const { columns, setActiveFilter, connection } = useTable();

  const { allowFiltering, allowLlmFiltering } = hasOptions;

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

  /**
   * Handle query editor text changes - only update state, no filtering
   */
  const handleQueryChange = useCallback(
    (event: QueryEditorChangeEvent) => {
      setQuery(event.text);
      setIsQueryValid(event.isValid);

      if (event.text.trim() === '') {
        // Clear pending filter for empty query
        setPendingFilter(null);
        setActiveFilter(null);
      } else if (event.isValid && event.filters) {
        // Store valid filter for when user presses Enter
        setPendingFilter({ group: event.filters });
      } else {
        // Invalid query - clear pending filter
        setPendingFilter(null);
      }
    },
    [setActiveFilter]
  );

  /**
   * Handle invalid query by calling parseInvalidQuery service
   */
  const handleInvalidQuery = useCallback(async () => {
    if (isParsing) {
      return;
    }

    setIsParsing(true);
    try {
      // Call parseFilter endpoint with the invalid query string
      const result = await parseInvalidQuery(query, connection);

      if (result.filterExpression) {
        // Handle both possible response field names
        const correctedQuery = result.filterExpression;

        // Parse the corrected query string back to AST using parseQuery
        const parsedResult = parseQuery(correctedQuery, queryEditorColumns);

        // Check if parsing was successful
        const isValid =
          parsedResult &&
          parsedResult.filters &&
          (!parsedResult.errors || parsedResult.errors.length === 0);

        if (isValid) {
          // Create filter from parsed result
          const newFilter = { group: parsedResult.filters };

          // Update UI with corrected query
          setQuery(correctedQuery);
          setPendingFilter(newFilter);
          setIsQueryValid(true);

          // Apply the filter immediately
          setActiveFilter(newFilter);
        }
      }
    } catch {
      // TODO - unhappy path: Silent error handling - could add user notification here if needed
    } finally {
      setIsParsing(false);
    }
  }, [query, isParsing, connection, queryEditorColumns, setActiveFilter]);

  /**
   * Handle Enter key press - the main entry point for applying filters
   */
  const handleEnterKey = useCallback(async () => {
    // Case 1: Empty query - clear filter
    if (query.trim() === '') {
      setActiveFilter(null);
      return;
    }

    // Case 2: Valid query with pending filter - apply it
    if (isQueryValid && pendingFilter) {
      setActiveFilter(pendingFilter);
      return;
    }

    // Case 3: Invalid query - try to parse it
    if (!isQueryValid && allowLlmFiltering) {
      await handleInvalidQuery();
      return;
    }

    // TODO - unhappy path: Valid query but no pending filter (edge case)
  }, [query, isQueryValid, pendingFilter, setActiveFilter, handleInvalidQuery]);

  /**
   * Keyboard event handler
   */
  const handleKeyDown = useCallback(
    async (event: React.KeyboardEvent) => {
      // Check for Enter key with optional modifiers
      if (
        event.key === 'Enter' &&
        (event.metaKey || event.ctrlKey || !event.shiftKey)
      ) {
        event.preventDefault();
        await handleEnterKey();
      }
    },
    [handleEnterKey]
  );

  // Early return after all hooks
  if (columns.length === 0) {
    return null;
  }

  const queryEditorContent = (
    <div className="flex gap-2 items-center flex-col sm:flex-row">
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
      {isParsing && (
        <div className="flex items-center justify-center">
          <Loader2 className="animate-spin h-4 w-4 text-gray-500" />
        </div>
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
