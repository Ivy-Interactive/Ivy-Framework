import React, {
  useState,
  useMemo,
  useCallback,
  useEffect,
  useRef,
} from 'react';
import { useTable } from '../dataTableContext';
import { tableStyles } from '../styles/style';
import {
  QueryEditor,
  QueryEditorChangeEvent,
  parseQuery,
  useDropdownState,
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
  isExpanded?: boolean;
}> = ({ allowLlmFiltering = true, isExpanded = true }) => {
  const [query, setQuery] = useState<string>('');
  const [pendingFilter, setPendingFilter] = useState<Filter | null>(null);
  const [isParsing, setIsParsing] = useState(false);
  const [isQueryValid, setIsQueryValid] = useState(true);
  const [isFocused, setIsFocused] = useState(false);
  const dropdownState = useDropdownState();
  const editorContainerRef = useRef<HTMLDivElement>(null);

  const { columns, setActiveFilter, connection } = useTable();

  /**
   * Monitor CodeMirror's autocomplete dropdown and sync with dropdownState
   * The autocomplete is created by CodeMirror as .cm-tooltip-autocomplete
   */
  useEffect(() => {
    const observer = new MutationObserver(() => {
      const autocompleteTooltip = document.querySelector(
        '.cm-tooltip-autocomplete'
      );
      dropdownState.setIsOpen(!!autocompleteTooltip);
    });

    observer.observe(document.body, {
      childList: true,
      subtree: true,
    });

    return () => observer.disconnect();
  }, [dropdownState]);

  /**
   * Monitor focus/blur events on the CodeMirror editor
   * Use focusin/focusout which bubble up from CodeMirror
   */
  useEffect(() => {
    if (!editorContainerRef.current) return;

    const handleFocusIn = () => setIsFocused(true);
    const handleFocusOut = () => setIsFocused(false);

    const container = editorContainerRef.current;

    // Use focusin/focusout which bubble (unlike focus/blur)
    container.addEventListener('focusin', handleFocusIn);
    container.addEventListener('focusout', handleFocusOut);

    return () => {
      container.removeEventListener('focusin', handleFocusIn);
      container.removeEventListener('focusout', handleFocusOut);
    };
  }, []);

  // Filter columns to only include filterable ones
  const queryEditorColumns = useMemo(
    () =>
      columns
        .filter(col => col.filterable ?? true)
        .map(col => {
          // Map column types to filter-query-editor supported types
          let editorType = col.type.toLowerCase();
          // DateTime should be treated as date for filtering purposes
          if (editorType === 'datetime') {
            editorType = 'date';
          }

          return {
            name: col.name,
            type: editorType,
            width: typeof col.width === 'number' ? col.width : 150,
          };
        }),
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
   * Handle click on container to focus the editor
   */
  const handleContainerClick = useCallback(() => {
    if (editorContainerRef.current) {
      // Find the CodeMirror content element and focus it
      const cmContent = editorContainerRef.current.querySelector(
        '.cm-content'
      ) as HTMLElement;
      if (cmContent) {
        cmContent.focus();
      }
    }
  }, []);

  /**
   * Keyboard event handler (capture phase)
   * Checks for autocomplete BEFORE CodeMirror processes the event
   */
  const handleKeyDownCapture = useCallback(
    (event: React.KeyboardEvent) => {
      if (
        event.key === 'Enter' &&
        (event.metaKey || event.ctrlKey || !event.shiftKey)
      ) {
        // In capture phase, check if autocomplete is open
        const autocompleteInDOM = document.querySelector(
          '.cm-tooltip-autocomplete'
        );

        if (autocompleteInDOM || dropdownState.stateRef.current) {
          // Mark this event so bubble phase knows to ignore it
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          (event.nativeEvent as any).__skipFilterTrigger = true;
        }
      }
    },
    [dropdownState.stateRef]
  );

  /**
   * Keyboard event handler (bubble phase)
   * Triggers filter search if autocomplete didn't handle it
   */
  const handleKeyDown = useCallback(
    async (event: React.KeyboardEvent) => {
      if (
        event.key === 'Enter' &&
        (event.metaKey || event.ctrlKey || !event.shiftKey)
      ) {
        // Check if capture phase marked this to skip
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        if ((event.nativeEvent as any).__skipFilterTrigger) {
          return;
        }

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

    const getColumnExample = (column: typeof firstColumn) => {
      if (column.type === ColType.Number) {
        return `[${column.name}] > 100`;
      } else if (
        column.type === ColType.Date ||
        column.type === ColType.DateTime
      ) {
        return `[${column.name}] >= "2024-01-01"`;
      } else {
        return `[${column.name}] = "value"`;
      }
    };

    if (secondColumn) {
      const firstExample = getColumnExample(firstColumn);
      const secondExample = getColumnExample(secondColumn);
      return `e.g., ${firstExample} AND ${secondExample}`;
    } else if (firstColumn) {
      return `e.g., ${getColumnExample(firstColumn)}`;
    }

    return 'Enter filter expression';
  }, [columns]);

  if (columns.length === 0) {
    return null;
  }

  return (
    <div
      ref={editorContainerRef}
      onClick={handleContainerClick}
      className={cn(
        'flex items-center w-full h-full justify-between',
        'rounded-tr-md rounded-br-md border transition-colors',
        isFocused ? 'border-border' : 'border-transparent',
        isExpanded ? 'cursor-text' : 'pointer-events-none'
      )}
    >
      <div
        className="flex-1 min-w-0 max-w-[350px] query-editor-wrapper cursor-text"
        data-query-valid={isQueryValid}
        onKeyDownCapture={handleKeyDownCapture}
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
        {isParsing ? (
          <div className="flex items-center justify-center px-4">
            <Loader2 className="animate-spin h-4 w-4 text-gray-500" />
          </div>
        ) : (
          <Button
            variant="ghost"
            size="sm"
            onClick={handleClearFilter}
            disabled={!query}
            className={cn(
              'h-9 px-3 text-sm hover:bg-muted/50 transition-all',
              query
                ? 'opacity-100 visible'
                : 'opacity-0 invisible pointer-events-none'
            )}
          >
            Clear
          </Button>
        )}
      </div>
    </div>
  );
};
