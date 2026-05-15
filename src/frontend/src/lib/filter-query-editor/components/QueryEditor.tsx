/**
 * QueryEditor - Main React component for the filter query editor
 * Wraps CodeMirror 6 with custom extensions for syntax highlighting,
 * validation, and formatting.
 */

import React, { useEffect, useRef, useCallback, useState } from "react";
import { QueryEditorProps, QueryEditorChangeEvent } from "./types";
import { useCodeMirror } from "./useCodeMirror";
import { parseQuery } from "../parser/QueryParser";

/**
 * QueryEditor component - A simplified CodeMirror-based editor for filter queries
 *
 * @example
 * Basic usage:
 * ```tsx
 * <QueryEditor
 *   value={query}
 *   columns={columns}
 *   onChange={(event) => console.log(event.text, event.isValid)}
 *   theme="light"
 *   height={80}
 *   placeholder="Enter a filter query..."
 * />
 * ```
 *
 * @example
 * With dropdown state tracking:
 * ```tsx
 * import { QueryEditor, useDropdownState } from '@/lib/filter-query-editor';
 *
 * const MyComponent = () => {
 *   const dropdownState = useDropdownState();
 *
 *   // Track when dropdown opens/closes
 *   useEffect(() => {
 *     console.log('Dropdown is:', dropdownState.isOpen ? 'open' : 'closed');
 *   }, [dropdownState.isOpen]);
 *
 *   return (
 *     <QueryEditor
 *       value={query}
 *       columns={columns}
 *       onChange={handleChange}
 *       isOpen={dropdownState.isOpen}
 *       onOpenChange={dropdownState.setIsOpen}
 *     />
 *   );
 * };
 * ```
 */
export const QueryEditor: React.FC<QueryEditorProps> = ({
  value = "",
  columns,
  onChange,
  theme = "light",
  readOnly = false,
  height = 60,
  placeholder,
  className = "",
  autoFocus = false,
  customStyling = "",
  onApply,
  isOpen,
  onDropdownStateQuery,
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const onChangeRef = useRef(onChange);
  const onDropdownStateQueryRef = useRef(onDropdownStateQuery);
  const [container, setContainer] = useState<HTMLDivElement | null>(null);

  // Update container when ref is set
  useEffect(() => {
    if (containerRef.current) {
      setContainer(containerRef.current);
    }
  }, []);

  // Keep onChange ref up to date
  useEffect(() => {
    onChangeRef.current = onChange;
  }, [onChange]);

  // Keep onDropdownStateQuery ref up to date
  useEffect(() => {
    onDropdownStateQueryRef.current = onDropdownStateQuery;
  }, [onDropdownStateQuery]);

  // Notify consumer of current dropdown state whenever isOpen changes
  useEffect(() => {
    if (onDropdownStateQueryRef.current && isOpen !== undefined) {
      onDropdownStateQueryRef.current(isOpen);
    }
  }, [isOpen]);

  // Handle change events from CodeMirror
  const handleChange = useCallback(
    (text: string) => {
      if (!onChangeRef.current) return;

      // Parse the query to get validation state
      const parseResult = parseQuery(text, columns);

      const event: QueryEditorChangeEvent = {
        text,
        isValid: !parseResult.errors || parseResult.errors.length === 0,
        filters: parseResult.filters,
        errors: parseResult.errors,
      };

      onChangeRef.current(event);
    },
    [columns],
  );

  // Initialize CodeMirror
  useCodeMirror({
    container: container,
    value,
    columns,
    onChange: handleChange,
    onApply,
    theme,
    readOnly,
    placeholder,
    autoFocus,
  });

  return (
    <div
      ref={containerRef}
      className={className}
      style={{
        ...(typeof customStyling === "object" ? customStyling : {}),
        height: typeof height === "number" ? `${height}px` : height,
      }}
      data-theme={theme}
      data-gramm="false"
      data-dropdown-open={isOpen}
    />
  );
};

// Default export for convenience
export default QueryEditor;
