import { Filter, SortOrder } from '@/services/grpcTableService';
import { GridColumn } from '@glideapps/glide-data-grid';
import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from 'react';
import {
  DataColumn,
  DataRow,
  DataTableConfig,
  DataTableConnection,
  SortDirection,
} from './types/types';
import { fetchTableData } from './utils/tableDataFetcher';

/**
 * Parses a Size string (e.g., "Px:200") to a numeric pixel value
 * If input is already a number, returns it as-is
 */
function parseSize(size: number | string | undefined): number {
  if (typeof size === 'number') return size;
  if (!size) return 150; // default width

  // Parse "Px:200" or "Rem:10" format
  const match = size.match(/^(Px|Rem):(\d+\.?\d*)$/);
  if (match) {
    const [, unit, value] = match;
    const numValue = parseFloat(value);
    // For Rem, convert to pixels (assuming 16px = 1rem)
    return unit === 'Rem' ? numValue * 16 : numValue;
  }

  return 150; // fallback to default
}

interface TableContextType {
  // State
  data: DataRow[];
  columns: DataColumn[];
  columnWidths: Record<string, number>;
  visibleRows: number;
  isLoading: boolean;
  hasMore: boolean;
  error: string | null;
  editable: boolean;
  connection: DataTableConnection;
  config: DataTableConfig;
  activeFilter: Filter | null;
  activeSort: SortOrder[] | null;
  columnOrder: number[];

  // Methods
  loadMoreData: () => Promise<void>;
  handleColumnResize: (column: GridColumn, newSize: number) => void;
  handleSort: (columnName: string) => void;
  setActiveFilter: (filter: Filter | null) => void;
  setError: (error: string | null) => void;
  handleColumnReorder: (startIndex: number, endIndex: number) => void;
}

// eslint-disable-next-line react-refresh/only-export-components
export const TableContext = createContext<TableContextType | undefined>(
  undefined
);

interface TableProviderProps {
  children: React.ReactNode;
  columns: DataColumn[];
  connection: DataTableConnection;
  config: DataTableConfig;
  editable?: boolean;
}

export const TableProvider: React.FC<TableProviderProps> = ({
  children,
  columns: columnsProp,
  connection,
  config,
  editable = false,
}) => {
  const [data, setData] = useState<DataRow[]>([]);
  const [columns, setColumns] = useState<DataColumn[]>(columnsProp);
  const [columnWidths, setColumnWidths] = useState<Record<string, number>>({});
  const [visibleRows, setVisibleRows] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeFilter, setActiveFilter] = useState<Filter | null>(null);
  const [activeSort, setActiveSort] = useState<SortOrder[] | null>(null);
  const [columnOrder, setColumnOrder] = useState<number[]>([]);

  const loadingRef = useRef(false);
  const currentRowCountRef = useRef(0);
  const isReorderingRef = useRef(false);
  const batchSize = config.batchSize ?? 20;

  const { allowColumnResizing, allowSorting } = config;

  // Update columns when columnsProp changes
  useEffect(() => {
    // Don't update columns during reordering
    if (isReorderingRef.current) return;

    setColumns(prevColumns => {
      // Only update if the column names or count changed
      if (
        prevColumns.length !== columnsProp.length ||
        prevColumns.some((col, idx) => col.name !== columnsProp[idx].name)
      ) {
        // Structure changed, reset column order
        setColumnOrder([]);
        return columnsProp;
      }
      // Same structure, just update metadata without resetting order
      return columnsProp;
    });
  }, [columnsProp]);

  // Reset row count and column widths when connection changes
  useEffect(() => {
    currentRowCountRef.current = 0;
    setColumnWidths({});
  }, [connection]);

  // Reset currentRowCountRef when filter or sort changes
  useEffect(() => {
    currentRowCountRef.current = 0;
  }, [activeFilter, activeSort]);

  // Load initial data
  useEffect(() => {
    const loadInitialData = async () => {
      if (!connection.port || !connection.path) {
        setError('Connection configuration is required');
        return;
      }

      setIsLoading(true);
      setError(null);

      try {
        // When sorting/filtering changes, currentRowCountRef is reset to 0
        // so we always start fresh with batchSize rows, or all rows if loadAllRows is true
        const rowsToFetch = config.loadAllRows
          ? 1000000 // Large number to fetch all rows
          : currentRowCountRef.current > 0
            ? currentRowCountRef.current
            : batchSize;

        const result = await fetchTableData(
          connection,
          0,
          rowsToFetch,
          activeFilter,
          activeSort
        );

        // Merge Arrow columns with columnsProp (columnsProp has all metadata)
        // Arrow columns only provide name and calculated width (type inference is unreliable)
        const mergedColumns = columnsProp.map(propCol => {
          const arrowCol = result.columns.find(ac => ac.name === propCol.name);
          // Parse width from Size string format to numeric pixels
          const parsedWidth = parseSize(propCol.width);
          return {
            ...propCol,
            // Use parsed width from prop, or calculated width from Arrow, or default
            width: parsedWidth || parseSize(arrowCol?.width) || 150,
            // IMPORTANT: Keep type from propCol, never override with Arrow's inferred type
            type: propCol.type,
          };
        });

        setColumns(mergedColumns);
        setData(result.rows);
        setVisibleRows(result.rows.length);
        currentRowCountRef.current = result.rows.length;
        setHasMore(result.hasMore);

        // Initialize column order when columns are first loaded
        if (columnOrder.length === 0) {
          setColumnOrder(mergedColumns.map((_, index) => index));
        }

        // Initialize sort from column metadata (only on first load)
        if (activeSort === null) {
          const sortedColumn = mergedColumns.find(
            col =>
              col.sortDirection &&
              col.sortDirection !== SortDirection.None &&
              (col.sortable ?? true)
          );
          if (sortedColumn) {
            const direction =
              sortedColumn.sortDirection === SortDirection.Ascending
                ? ('ASC' as const)
                : ('DESC' as const);
            setActiveSort([{ column: sortedColumn.name, direction }]);
            // Don't fetch data again, this will trigger the effect
            return;
          }
        }

        // Initialize column widths only if not already set (first load)
        setColumnWidths(prevWidths => {
          // If we already have column widths, preserve them
          if (Object.keys(prevWidths).length > 0) {
            return prevWidths;
          }

          // First time loading, initialize with default widths
          const widths: Record<string, number> = {};
          mergedColumns.forEach((col, index) => {
            widths[index.toString()] = col.width;
          });
          return widths;
        });
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : 'Failed to load data';
        setError(errorMessage);
      } finally {
        setIsLoading(false);
      }
    };
    loadInitialData();
  }, [connection, activeFilter, activeSort, columnOrder.length, columnsProp]);

  // Load more data
  const loadMoreData = useCallback(async () => {
    if (loadingRef.current || !hasMore || config.loadAllRows) return;

    loadingRef.current = true;
    setIsLoading(true);

    try {
      const result = await fetchTableData(
        connection,
        data.length,
        batchSize,
        activeFilter,
        activeSort
      );

      if (result.rows.length > 0) {
        setData(prev => [...prev, ...result.rows]);
        setVisibleRows(prev => prev + result.rows.length);
        currentRowCountRef.current += result.rows.length;
      }

      setHasMore(result.hasMore);
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : 'Failed to load more data';
      setError(errorMessage);
    } finally {
      setIsLoading(false);
      loadingRef.current = false;
    }
  }, [connection, data.length, hasMore, activeFilter, activeSort]);

  // Handle column resize
  const handleColumnResize = useCallback(
    (column: GridColumn, newSize: number) => {
      // Check if column resizing is allowed
      if (!allowColumnResizing) return;

      // Find the column by matching title (which is col.header || col.name)
      const columnIndex = columns.findIndex(
        col => (col.header || col.name) === column.title
      );

      if (columnIndex !== -1) {
        setColumnWidths(prev => ({
          ...prev,
          [columnIndex.toString()]: newSize,
        }));
      }
    },
    [columns, allowColumnResizing]
  );

  // Handle sort
  const handleSort = useCallback(
    (columnName: string) => {
      // Check if sorting is allowed
      if (!allowSorting) return;

      setActiveSort(prevSort => {
        // Check if we're already sorting by this column
        const existingSort = prevSort?.find(sort => sort.column === columnName);

        if (existingSort) {
          // Toggle direction: ASC -> DESC -> remove sort
          if (existingSort.direction === 'ASC') {
            return [{ column: columnName, direction: 'DESC' as const }];
          } else {
            // Remove sort entirely
            return null;
          }
        } else {
          // Replace current sort with new column (ASC by default)
          return [{ column: columnName, direction: 'ASC' }];
        }
      });
    },
    [allowSorting]
  );

  // Handle column reorder
  const handleColumnReorder = useCallback(
    (startIndex: number, endIndex: number) => {
      // Set flag to prevent column updates during reordering
      isReorderingRef.current = true;

      setColumnOrder(prevOrder => {
        // prevOrder contains indices into the full columns array
        // startIndex and endIndex are positions in the VISIBLE columns

        // Get the visible column indices (filtering out hidden ones)
        const visibleIndices = prevOrder.filter(idx => !columns[idx]?.hidden);

        // Reorder just the visible indices
        const newVisibleIndices = [...visibleIndices];
        const [movedIndex] = newVisibleIndices.splice(startIndex, 1);
        newVisibleIndices.splice(endIndex, 0, movedIndex);

        // Reconstruct the full order array, preserving hidden column positions
        const newOrder = [...prevOrder];
        let visiblePosition = 0;

        for (let i = 0; i < newOrder.length; i++) {
          // Check if the column at this position in the ORIGINAL order is hidden
          if (!columns[prevOrder[i]]?.hidden) {
            newOrder[i] = newVisibleIndices[visiblePosition++];
          }
        }

        // Reset flag after a short delay
        setTimeout(() => {
          isReorderingRef.current = false;
        }, 100);

        return newOrder;
      });
    },
    [columns]
  );

  const value: TableContextType = {
    data,
    columns,
    columnWidths,
    visibleRows,
    isLoading,
    hasMore,
    error,
    editable,
    connection,
    config,
    activeFilter,
    activeSort,
    columnOrder,
    loadMoreData,
    handleColumnResize,
    handleSort,
    setActiveFilter,
    setError,
    handleColumnReorder,
  };

  return (
    <TableContext.Provider value={value}>{children}</TableContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useTable = () => {
  const context = useContext(TableContext);
  if (!context) {
    throw new Error('useTable must be used within a TableProvider');
  }
  return context;
};
