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
import { Loader2, Filter as FilterIcon, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { ColType } from './types/types';

export const DataTableOptions: React.FC<{
  hasOptions: { allowFiltering: boolean; allowLlmFiltering: boolean };
}> = ({ hasOptions }) => {
  const [query, setQuery] = useState<string>('');
  const [pendingFilter, setPendingFilter] = useState<Filter | null>(null);
  const [isParsing, setIsParsing] = useState(false);
  const [isQueryValid, setIsQueryValid] = useState(true);
  const [isExpanded, setIsExpanded] = useState(false); // Start with editor visible

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
  const handleQueryChange = useCallback((event: QueryEditorChangeEvent) => {
    setQuery(event.text);
    setIsQueryValid(event.isValid);

    if (event.text.trim() === '') {
      // Clear pending filter for empty query
      setPendingFilter(null);
    } else if (event.isValid && event.filters) {
      // Store valid filter for when user presses Enter
      setPendingFilter({ group: event.filters });
    } else {
      // Invalid query - clear pending filter
      setPendingFilter(null);
    }
  }, []);

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

  // Generate dynamic placeholder based on available columns
  const placeholderText = useMemo(() => {
    if (columns.length === 0) return 'No columns available';

    // Get first two columns for example
    const firstColumn = columns[0];
    const secondColumn = columns[1];

    if (secondColumn) {
      // If we have at least two columns, show an AND example
      const firstExample =
        firstColumn.type === ColType.Number
          ? `[${firstColumn.name}] > 100`
          : `[${firstColumn.name}] = "value"`;
      const secondExample =
        secondColumn.type === ColType.Number
          ? `[${secondColumn.name}] < 50`
          : `[${secondColumn.name}] != "text"`;
      return `e.g., ${firstExample} AND ${secondExample}`;
    } else if (firstColumn) {
      // If we only have one column, show a simple example
      return firstColumn.type === ColType.Number
        ? `e.g., [${firstColumn.name}] > 100`
        : `e.g., [${firstColumn.name}] = "value"`;
    }

    return 'Enter filter expression';
  }, [columns]);

  // Early return after all hooks
  if (columns.length === 0) {
    return null;
  }

  const queryEditorContent = (
    <div className="relative flex items-center border rounded-md mb-2 w-fit h-fit">
      <Button
        variant="ghost"
        size="icon"
        onClick={() => {
          setIsExpanded(!isExpanded);
        }}
        className={`h-9 w-9 flex-shrink-0 ${isExpanded ? 'text-primary' : ''}`}
        title={isExpanded ? 'Collapse filter' : 'Expand filter'}
      >
        <FilterIcon className="h-4 w-4" />
      </Button>
      <div
        className={`query-editor-wrapper min-h-[40px] overflow-hidden transition-all duration-300 ease-in-out ${
          isExpanded ? 'w-[400px] opacity-100' : 'w-0 opacity-0'
        }`}
        onKeyDown={handleKeyDown}
      >
        <QueryEditor
          value={query}
          columns={queryEditorColumns}
          onChange={handleQueryChange}
          placeholder={placeholderText}
          height={40}
          className="font-mono"
        />
        <style>{tableStyles.queryEditor.css}</style>
      </div>
      {isExpanded && query && (
        <Button
          variant="ghost"
          size="icon"
          onClick={() => {
            // Clear the query
            setQuery('');
            setActiveFilter(null);
            setPendingFilter(null);
            setIsQueryValid(true);
          }}
          className="h-10 w-10 flex-shrink-0"
          title="Clear filter"
        >
          <X className="h-4 w-4" />
        </Button>
      )}
      {isParsing && isExpanded && (
        <div
          className={`absolute top-1/2 -translate-y-1/2 ${
            query ? 'right-[50px]' : 'right-[10px]'
          }`}
        >
          <Loader2 className="animate-spin h-4 w-4 text-gray-500" />
        </div>
      )}
    </div>
  );

  return (
    <div className="w-full">
      <div className="flex items-center">
        {allowFiltering && queryEditorContent}
      </div>
    </div>
  );
};
