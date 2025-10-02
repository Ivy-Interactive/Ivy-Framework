import React, { useState, useEffect } from 'react';
import { useTable } from '../context/TableContext';
import { tableStyles } from '../styles/style';
import { Header } from './TableHeader';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent } from '@/components/ui/dialog';
import { Separator } from '@/components/ui/separator';
import { Input } from '@/components/ui/input';
import { useToast } from '@/hooks/use-toast';
import {
  parseFilterQuery,
  validateFilterQuery,
  FilterParseError,
} from '../utils/filterParser';

export const TableOptions: React.FC = () => {
  const { columns, setActiveFilter, activeFilter, error, setError } =
    useTable();
  const [isFilterOpen, setIsFilterOpen] = useState(false);
  const [filterInput, setFilterInput] = useState('');
  const [filterError, setFilterError] = useState<string | null>(null);
  const [showColumns, setShowColumns] = useState(false);
  const { toast } = useToast();

  // Show toast when table context has an error
  useEffect(() => {
    if (error && error.includes('Column') && error.includes('not found')) {
      toast({
        title: 'Filter Error',
        description: error + '. The filter has been cleared.',
        variant: 'destructive',
      });
      // Clear the error after showing toast
      setError(null);
      setFilterInput('');
      setFilterError(null);
    }
  }, [error, toast, setError]);

  if (columns.length === 0) {
    return null;
  }

  const handleFilterInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setFilterInput(value);

    // Validate in real-time
    if (value.trim().length > 0) {
      const validation = validateFilterQuery(value);
      setFilterError(validation.valid ? null : validation.error || null);
    } else {
      setFilterError(null);
    }
  };

  const handleApplyFilter = () => {
    if (!filterInput.trim()) {
      setActiveFilter(null);
      setIsFilterOpen(false);
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
      setIsFilterOpen(false);
    } catch (error) {
      if (error instanceof FilterParseError) {
        setFilterError(error.message);
      } else if (error instanceof Error) {
        setFilterError(error.message);
      } else {
        setFilterError('Unknown error occurred while parsing filter');
      }
    }
  };

  const handleClearFilter = () => {
    setFilterInput('');
    setFilterError(null);
    setActiveFilter(null);
  };

  const handleOpenDialog = () => {
    // Pre-populate with existing filter if any
    // For now, just open with empty state
    setIsFilterOpen(true);
  };

  return (
    <>
      <div style={tableStyles.tableOptions.container}>
        <div className={tableStyles.tableOptions.inner}>
          <div className={tableStyles.tableOptions.leftSection}>
            <Header />
          </div>

          <div className={tableStyles.tableOptions.rightSection}>
            {activeFilter && (
              <Button variant="outline" size="sm" onClick={handleClearFilter}>
                Clear Filter
              </Button>
            )}
            <Button variant="default" onClick={handleOpenDialog}>
              <svg
                className={tableStyles.tableOptions.dialog.filterIcon}
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.707A1 1 0 013 7V4z"
                />
              </svg>
              Filter
            </Button>
          </div>
        </div>
      </div>
      <Dialog open={isFilterOpen} onOpenChange={setIsFilterOpen}>
        <DialogContent className={tableStyles.tableOptions.dialog.content}>
          <div className={tableStyles.tableOptions.dialog.header}>
            <h2 className="text-lg font-semibold">Filter Data</h2>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setIsFilterOpen(false)}
            >
              <svg
                className={tableStyles.tableOptions.dialog.closeIcon}
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </Button>
          </div>
          <Separator />
          <div className="flex flex-col gap-2">
            <div className="flex items-center justify-between">
              <label className="text-sm font-medium">Filter Query</label>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setShowColumns(!showColumns)}
                className="h-auto py-1 px-2 text-xs"
              >
                {showColumns ? 'Hide' : 'Show'} Available Columns
              </Button>
            </div>
            {showColumns && (
              <div className="p-2 bg-muted rounded text-xs">
                <p className="font-medium mb-1">Available Columns:</p>
                <div className="flex flex-wrap gap-1">
                  {columns.map(col => (
                    <code
                      key={col.name}
                      className="bg-background px-2 py-0.5 rounded border cursor-pointer hover:bg-accent"
                      onClick={() => {
                        setFilterInput(
                          prev => prev + (prev ? ' ' : '') + col.name
                        );
                      }}
                      title={`Type: ${col.type}`}
                    >
                      {col.name}
                    </code>
                  ))}
                </div>
              </div>
            )}
            <Input
              value={filterInput}
              onChange={handleFilterInputChange}
              placeholder='e.g., name = "John" AND age > 18'
              className={
                filterError ? tableStyles.tableOptions.dialog.inputError : ''
              }
            />
            {filterError && (
              <p className={tableStyles.tableOptions.dialog.errorText}>
                {filterError}
              </p>
            )}
            <div className={tableStyles.tableOptions.dialog.helpText}>
              <p className="font-medium mb-1">Syntax examples:</p>
              <ul className={tableStyles.tableOptions.dialog.examplesList}>
                <li>
                  <code className="bg-muted px-1 py-0.5 rounded text-xs">
                    name = "John"
                  </code>{' '}
                  - Exact match
                </li>
                <li>
                  <code className="bg-muted px-1 py-0.5 rounded text-xs">
                    age &gt; 18
                  </code>{' '}
                  - Comparison
                </li>
                <li>
                  <code className="bg-muted px-1 py-0.5 rounded text-xs">
                    status IN ("active", "pending")
                  </code>{' '}
                  - Multiple values
                </li>
                <li>
                  <code className="bg-muted px-1 py-0.5 rounded text-xs">
                    name CONTAINS "test"
                  </code>{' '}
                  - String search
                </li>
                <li>
                  <code className="bg-muted px-1 py-0.5 rounded text-xs">
                    age &gt; 18 AND status = "active"
                  </code>{' '}
                  - Combine conditions
                </li>
              </ul>
            </div>
          </div>
          <Separator />
          <div className={tableStyles.tableOptions.dialog.footer}>
            <Button variant="outline" onClick={() => setIsFilterOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={handleApplyFilter}
              disabled={!!filterError || !filterInput.trim()}
            >
              Apply Filter
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
};
