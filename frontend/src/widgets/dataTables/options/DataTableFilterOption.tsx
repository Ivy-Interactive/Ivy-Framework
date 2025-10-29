import React, { useState, useMemo, useCallback } from 'react';
import { useTable } from '../DataTableContext';
import { tableStyles } from '../styles/style';
import {
  QueryEditor,
  QueryEditorChangeEvent,
  parseQuery,
} from 'filter-query-editor';
import { Filter } from '@/services/grpcTableService';
import { parseInvalidQuery } from '../utils/tableDataFetcher';
import { Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { ColType } from '../types/types';
import { cn } from '@/lib/utils';

/**
 * DataTableFilterOption - The content component for the filter option
 * This component provides the filter UI and logic, to be wrapped by DataTableOption
 */
export const DataTableFilterOption: React.FC<{
  allowLlmFiltering?: boolean;
}> = ({ allowLlmFiltering = true }) => {
  const [query, setQuery] = useState<string>('');
  const [pendingFilter, setPendingFilter] = useState<Filter | null>(null);
  const [isParsing, setIsParsing] = useState(false);
  const [isQueryValid, setIsQueryValid] = useState(true);

  const { columns, setActiveFilter, connection } = useTable();

  // Filter columns to only include filterable ones
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
   * Handle query editor text changes
   */
  const handleQueryChange = useCallback(
    (event: QueryEditorChangeEvent) => {
      setQuery(event.text);
      setIsQueryValid(event.isValid);

      if (event.text.trim() === '') {
        setPendingFilter(null);
        setActiveFilter(null);
      } else if (event.isValid && event.filters) {
        setPendingFilter({ group: event.filters });
      } else {
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
      const result = await parseInvalidQuery(query, connection);

      if (result.filterExpression) {
        const correctedQuery = result.filterExpression;
        const parsedResult = parseQuery(correctedQuery, queryEditorColumns);

        const isValid =
          parsedResult &&
          parsedResult.filters &&
          (!parsedResult.errors || parsedResult.errors.length === 0);

        if (isValid) {
          const newFilter = { group: parsedResult.filters };
          setQuery(correctedQuery);
          setPendingFilter(newFilter);
          setIsQueryValid(true);
          setActiveFilter(newFilter);
        }
      }
    } catch {
      // Silent error handling
    } finally {
      setIsParsing(false);
    }
  }, [query, isParsing, connection, queryEditorColumns, setActiveFilter]);

  /**
   * Handle Enter key press
   */
  const handleEnterKey = useCallback(async () => {
    if (query.trim() === '') {
      setActiveFilter(null);
      return;
    }

    if (isQueryValid && pendingFilter) {
      setActiveFilter(pendingFilter);
      return;
    }

    if (!isQueryValid && allowLlmFiltering) {
      await handleInvalidQuery();
      return;
    }
  }, [
    query,
    isQueryValid,
    pendingFilter,
    setActiveFilter,
    allowLlmFiltering,
    handleInvalidQuery,
  ]);

  /**
   * Handle clear filter
   */
  const handleClearFilter = useCallback(() => {
    setQuery('');
    setPendingFilter(null);
    setIsQueryValid(true);
    setActiveFilter(null);
  }, [setActiveFilter]);

  /**
   * Keyboard event handler
   */
  const handleKeyDown = useCallback(
    async (event: React.KeyboardEvent) => {
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

  // Generate dynamic placeholder
  const placeholderText = useMemo(() => {
    if (columns.length === 0) return 'No columns available';

    const firstColumn = columns[0];
    const secondColumn = columns[1];

    if (secondColumn) {
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
      return firstColumn.type === ColType.Number
        ? `e.g., [${firstColumn.name}] > 100`
        : `e.g., [${firstColumn.name}] = "value"`;
    }

    return 'Enter filter expression';
  }, [columns]);

  if (columns.length === 0) {
    return null;
  }

  return (
    <div className="flex items-center w-full h-full">
      <div
        className="flex-1 min-w-0 max-w-[350px] overflow-hidden query-editor-wrapper"
        onKeyDown={handleKeyDown}
      >
        <QueryEditor
          value={query}
          columns={queryEditorColumns}
          onChange={handleQueryChange}
          placeholder={placeholderText}
          height={40}
          className="font-mono border-0 shadow-none [&>.cm-editor]:border-0 [&>.cm-editor]:shadow-none [&_.cm-content]:overflow-x-auto"
        />
        <style>{tableStyles.queryEditor.css}</style>
      </div>

      {/* Fixed position container for loader and clear button */}
      <div className="flex items-center gap-2 ml-2 flex-shrink-0">
        {isParsing && (
          <div className="flex items-center justify-center">
            <Loader2 className="animate-spin h-4 w-4 text-gray-500" />
          </div>
        )}
        <Button
          variant="ghost"
          size="sm"
          onClick={handleClearFilter}
          disabled={!query}
          className={cn(
            'h-9 px-3 text-sm hover:bg-transparent hover:text-foreground transition-opacity',
            query
              ? 'opacity-100 visible'
              : 'opacity-0 invisible pointer-events-none'
          )}
        >
          Clear
        </Button>
      </div>
    </div>
  );
};
