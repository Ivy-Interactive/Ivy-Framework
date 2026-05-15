import React, { useState, useMemo, useCallback, useEffect, useRef } from "react";
import { useTable } from "../dataTableContext";
import { tableStyles } from "../styles/style";
import {
  QueryEditor,
  QueryEditorChangeEvent,
  parseQuery,
  useDropdownState,
} from "@/lib/filter-query-editor";
import { Filter } from "@/services/grpcTableService";
import { parseInvalidQuery } from "../utils/tableDataFetcher";
import { Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ColType } from "../types/types";
import { cn } from "@/lib/utils";
import { useThemeWithMonitoring } from "@/components/theme-provider";

/**
 * DataTableFilterOption - The content component for the filter option
 * This component provides the filter UI and logic, to be wrapped by DataTableOption
 */
export const DataTableFilterOption: React.FC<{
  allowLlmFiltering?: boolean;
  isExpanded?: boolean;
}> = ({ allowLlmFiltering = true, isExpanded = true }) => {
  const [query, setQuery] = useState<string>("");
  const [pendingFilter, setPendingFilter] = useState<Filter | null>(null);
  const [isParsing, setIsParsing] = useState(false);
  const [isQueryValid, setIsQueryValid] = useState(true);
  const dropdownState = useDropdownState();
  const editorContainerRef = useRef<HTMLDivElement>(null);
  const autocompleteOpenRef = useRef(false);

  const { columns, setActiveFilter, connection } = useTable();
  const { isDark } = useThemeWithMonitoring({ monitorDOM: true });

  /**
   * Monitor CodeMirror's autocomplete dropdown and sync with dropdownState
   * The autocomplete is created by CodeMirror as .cm-tooltip-autocomplete
   */
  useEffect(() => {
    const setIsOpen = dropdownState.setIsOpen;
    let raf = 0;

    const sync = () => {
      const open = !!document.querySelector(".cm-tooltip-autocomplete");
      if (open !== autocompleteOpenRef.current) {
        autocompleteOpenRef.current = open;
        setIsOpen(open);
      }
    };

    const observer = new MutationObserver(() => {
      cancelAnimationFrame(raf);
      raf = requestAnimationFrame(sync);
    });

    observer.observe(document.body, {
      childList: true,
      subtree: true,
    });

    return () => {
      cancelAnimationFrame(raf);
      observer.disconnect();
    };
  }, [dropdownState.setIsOpen]);

  // Filter columns to only include filterable ones
  const queryEditorColumns = useMemo(
    () =>
      columns
        .filter((col) => col.filterable ?? true)
        .map((col) => {
          // Map column types to filter-query-editor supported types
          // Default to 'text' if type is undefined (shouldn't happen but defensive)
          let editorType = (col.type ?? ColType.Text).toLowerCase();
          // DateTime should be treated as date for filtering purposes
          if (editorType === "datetime") {
            editorType = "date";
          }

          return {
            name: col.name,
            type: editorType,
            width: typeof col.width === "number" ? col.width : 150,
          };
        }),
    [columns],
  );

  /**
   * Handle query editor text changes
   */
  const handleQueryChange = useCallback(
    (event: QueryEditorChangeEvent) => {
      setQuery(event.text);
      setIsQueryValid(event.isValid);

      if (event.text.trim() === "") {
        setPendingFilter(null);
        setActiveFilter(null);
      } else if (event.isValid && event.filters) {
        setPendingFilter({ group: event.filters });
      } else {
        setPendingFilter(null);
      }
    },
    [setActiveFilter],
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
    if (query.trim() === "") {
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
  }, [query, isQueryValid, pendingFilter, setActiveFilter, allowLlmFiltering, handleInvalidQuery]);

  /**
   * Handle clear filter
   */
  const handleClearFilter = useCallback(() => {
    setQuery("");
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
      const cmContent = editorContainerRef.current.querySelector(".cm-content") as HTMLElement;
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
      if (event.key === "Enter" && (event.metaKey || event.ctrlKey || !event.shiftKey)) {
        // In capture phase, check if autocomplete is open
        const autocompleteInDOM = document.querySelector(".cm-tooltip-autocomplete");

        if (autocompleteInDOM || dropdownState.stateRef.current) {
          // Mark this event so bubble phase knows to ignore it
          // oxlint-disable-next-line @typescript-eslint/no-explicit-any
          (event.nativeEvent as any).__skipFilterTrigger = true;
        }
      }
    },
    [dropdownState.stateRef],
  );

  /**
   * Keyboard event handler (bubble phase)
   * Triggers filter search if autocomplete didn't handle it
   */
  const handleKeyDown = useCallback(
    async (event: React.KeyboardEvent) => {
      if (event.key === "Enter" && (event.metaKey || event.ctrlKey || !event.shiftKey)) {
        // Check if capture phase marked this to skip
        // oxlint-disable-next-line @typescript-eslint/no-explicit-any
        if ((event.nativeEvent as any).__skipFilterTrigger) {
          return;
        }

        event.preventDefault();
        await handleEnterKey();
      }
    },
    [handleEnterKey],
  );

  // Generate dynamic placeholder
  const placeholderText = useMemo(() => {
    if (columns.length === 0) return "No columns available";

    const firstColumn = columns[0];
    const secondColumn = columns[1];

    const getColumnExample = (column: typeof firstColumn) => {
      if (column.type === ColType.Number) {
        return `[${column.name}] > 100`;
      } else if (column.type === ColType.Date || column.type === ColType.DateTime) {
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

    return "Enter filter expression";
  }, [columns]);

  if (columns.length === 0) {
    return null;
  }

  return (
    <div
      ref={editorContainerRef}
      onClick={handleContainerClick}
      className={cn(
        "relative h-full min-h-9 min-w-0 w-full flex-1",
        isExpanded ? "cursor-text" : "pointer-events-none",
      )}
    >
      <div
        className={cn(
          "query-editor-wrapper relative cursor-text",
          "flex h-full min-h-9 w-full max-w-full min-w-0 flex-col",
          "min-h-0 overflow-hidden bg-background text-sm",
        )}
        data-query-valid={isQueryValid}
        data-has-clear={query ? "true" : "false"}
        data-is-parsing={isParsing ? "true" : "false"}
        onKeyDownCapture={handleKeyDownCapture}
        onKeyDown={handleKeyDown}
      >
        <QueryEditor
          value={query}
          columns={queryEditorColumns}
          onChange={handleQueryChange}
          placeholder={placeholderText}
          theme={isDark ? "dark" : "light"}
          height="100%"
          className="block min-h-0 w-full max-w-full min-w-0 font-mono border-0 shadow-none [&>.cm-editor]:min-h-0 [&>.cm-editor]:h-full [&>.cm-editor]:w-full [&>.cm-editor]:max-w-full [&>.cm-editor]:min-w-0 [&>.cm-editor]:border-0 [&>.cm-editor]:shadow-none [&>.cm-editor]:bg-transparent [&_.cm-scroller]:min-h-0"
        />
        <style>{tableStyles.queryEditor.css}</style>

        {isParsing ? (
          <div className="absolute right-0 top-1/2 z-10 flex h-9 -translate-y-1/2 items-center border-l border-input bg-background px-2.5">
            <Loader2 className="h-4 w-4 shrink-0 animate-spin text-muted-foreground" />
          </div>
        ) : (
          <Button
            variant="ghost"
            size="sm"
            onClick={handleClearFilter}
            disabled={!query}
            className={cn(
              "absolute right-0 top-1/2 z-10 h-9 -translate-y-1/2 shrink-0 border-l border-input bg-background px-3 text-sm shadow-none hover:bg-muted hover:text-accent-foreground focus-visible:ring-0 focus-visible:ring-offset-0",
              query ? "" : "hidden",
            )}
          >
            Clear
          </Button>
        )}
      </div>
    </div>
  );
};
