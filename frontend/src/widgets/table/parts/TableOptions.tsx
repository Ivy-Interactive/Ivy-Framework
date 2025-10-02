import React, {
  useState,
  useCallback,
  useMemo,
  useRef,
  useEffect,
} from 'react';
import { useTable } from '../context/TableContext';
import { tableStyles } from '../styles/style';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import CodeMirror from '@uiw/react-codemirror';
import { EditorView, keymap } from '@codemirror/view';
import { EditorState } from '@codemirror/state';
import {
  parseFilterQuery,
  validateFilterQuery,
  FilterParseError,
} from '../utils/filterParserAntlr';
import { filterQueryLanguage } from '../utils/filterLanguage';
import { createIvyCodeTheme } from '@/widgets/inputs/code/theme';
import { Sizes } from '@/types/sizes';

export const TableOptions: React.FC = () => {
  const {
    visibleRows,
    columns,
    editable,
    hasMore,
    activeFilter,
    setActiveFilter,
  } = useTable();
  const [filterInput, setFilterInput] = useState('');
  const [filterError, setFilterError] = useState<string | null>(null);
  const [isFocused, setIsFocused] = useState(false);
  const applyFilterRef = useRef<() => void>(() => {});

  // Clear input when activeFilter is cleared externally
  useEffect(() => {
    if (!activeFilter) {
      setFilterInput('');
      setFilterError(null);
    }
  }, [activeFilter]);

  const handleFilterInputChange = (value: string) => {
    setFilterInput(value);
    // Clear error while typing
    if (filterError) {
      setFilterError(null);
    }
  };

  const handleFilterInputBlur = useCallback(() => {
    // Validate only on blur
    if (filterInput.trim().length > 0) {
      const validation = validateFilterQuery(filterInput);
      setFilterError(validation.valid ? null : validation.error || null);
    } else {
      setFilterError(null);
    }
  }, [filterInput]);

  const handleApplyFilter = useCallback(() => {
    if (!filterInput.trim()) {
      setActiveFilter(null);
      return;
    }

    try {
      const filter = parseFilterQuery(filterInput);

      // Create a map for case-insensitive column name lookup
      const columnNameMap = new Map(
        columns.map(c => [c.name.toLowerCase(), c.name])
      );

      // Function to normalize column names to match actual case
      const normalizeColumnNames = (f: typeof filter): typeof filter => {
        if (f.condition) {
          const actualColumnName = columnNameMap.get(
            f.condition.column.toLowerCase()
          );
          if (!actualColumnName) {
            throw new Error(
              `Column not found: ${f.condition.column}. Available columns: ${columns.map(c => c.name).join(', ')}`
            );
          }
          return {
            ...f,
            condition: {
              ...f.condition,
              column: actualColumnName,
            },
          };
        }
        if (f.group) {
          return {
            ...f,
            group: {
              ...f.group,
              filters: f.group.filters.map(normalizeColumnNames),
            },
          };
        }
        return f;
      };

      // Normalize and validate the filter
      const normalizedFilter = normalizeColumnNames(filter);

      setActiveFilter(normalizedFilter);
      setFilterError(null);
    } catch (error) {
      if (error instanceof FilterParseError) {
        setFilterError(error.message);
      } else if (error instanceof Error) {
        setFilterError(error.message);
      } else {
        setFilterError('Unknown error occurred while parsing filter');
      }
    }
  }, [filterInput, columns, setActiveFilter]);

  // Keep ref updated
  applyFilterRef.current = handleApplyFilter;

  // Create CodeMirror extensions
  const extensions = useMemo(() => {
    const langExtension = filterQueryLanguage();

    // Keyboard shortcuts
    const keymapExtension = keymap.of([
      {
        key: 'Mod-Enter',
        run: () => {
          applyFilterRef.current?.();
          return true;
        },
      },
      {
        key: 'Enter',
        run: () => {
          // Prevent new lines
          return true;
        },
      },
    ]);

    // Single line mode - prevent line breaks
    const singleLineExtension = EditorState.transactionFilter.of(tr => {
      if (tr.newDoc.lines > 1) {
        return [];
      }
      return tr;
    });

    // Create a minimal theme for the filter input (no backgrounds, borders, etc)
    const minimalTheme = EditorView.theme({
      '&': {
        backgroundColor: 'transparent',
        height: '100%',
      },
      '.cm-scroller': {
        backgroundColor: 'transparent',
        display: 'flex',
        alignItems: 'center',
      },
      '.cm-content': {
        backgroundColor: 'transparent',
        padding: '0',
      },
      '.cm-line': {
        backgroundColor: 'transparent',
        padding: '0',
      },
      '.cm-editor': {
        backgroundColor: 'transparent',
      },
      '.cm-focused': {
        outline: 'none',
      },
      '.cm-cursor': {
        borderLeftColor: 'var(--foreground)',
      },
    });

    const themeExtension = createIvyCodeTheme(Sizes.Small);

    // Focus tracking extension
    const focusExtension = EditorView.focusChangeEffect.of(
      (_state, focusing) => {
        setIsFocused(focusing);
        // Validate on blur
        if (!focusing) {
          handleFilterInputBlur();
        }
        return null;
      }
    );

    return [
      langExtension,
      keymapExtension,
      singleLineExtension,
      minimalTheme,
      themeExtension,
      focusExtension,
    ];
  }, [handleFilterInputBlur]);

  if (columns.length === 0) {
    return null;
  }

  return (
    <div style={tableStyles.tableOptions.container}>
      <div className={tableStyles.tableOptions.inner}>
        {/* Filter / Search */}
        <div className="flex gap-2">
          <div className="w-[600px]">
            <div
              className={`rounded-md border pt-2 transition-colors ${
                filterError
                  ? 'border-destructive'
                  : isFocused
                    ? 'border-ring ring-1 ring-ring'
                    : 'border-input'
              }`}
            >
              <CodeMirror
                value={filterInput}
                onChange={handleFilterInputChange}
                extensions={extensions}
                placeholder='Filter: e.g., name = "John" AND age > 18'
                className="[&_.cm-editor]:!outline-none [&_.cm-editor]:!bg-transparent [&_.cm-editor]:!border-0 [&_.cm-content]:!py-1 [&_.cm-content]:!px-3 [&_.cm-line]:!px-0 [&_.cm-scroller]:!outline-none [&_.cm-focused]:!outline-none"
                basicSetup={{
                  lineNumbers: false,
                  foldGutter: false,
                  highlightActiveLine: false,
                  highlightActiveLineGutter: false,
                }}
                height="auto"
                minHeight="32px"
                maxHeight="32px"
                onKeyDown={e => {
                  if ((e.metaKey || e.ctrlKey) && e.key === 'Enter') {
                    e.preventDefault();
                    handleApplyFilter();
                  }
                }}
              />
            </div>
            {filterError && (
              <p className="text-xs text-destructive mt-1">{filterError}</p>
            )}
          </div>
          {activeFilter && (
            <Button variant="outline" onClick={() => setActiveFilter(null)}>
              Clear
            </Button>
          )}
        </div>
        {/* Additional options */}
        <Label className="text-sm text-muted-foreground">
          {columns.length} {columns.length === 1 ? 'column' : 'columns'}
        </Label>
        <Label className="text-sm text-muted-foreground">
          {visibleRows} {visibleRows === 1 ? 'row' : 'rows'}
        </Label>
        {editable && (
          <Label className="text-sm text-muted-foreground">Editable</Label>
        )}
        {!hasMore && visibleRows > 0 && (
          <Label className="text-sm text-muted-foreground">
            All data loaded
          </Label>
        )}
      </div>
    </div>
  );
};
