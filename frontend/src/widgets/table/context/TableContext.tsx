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
import { DataColumn, DataRow, DataTableConnection } from '../types/types';
import { fetchTableData } from '../utils/tableDataFetcher';

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
  activeFilter: Filter | null;
  activeSort: SortOrder[] | null;

  // Methods
  loadMoreData: () => Promise<void>;
  handleColumnResize: (column: GridColumn, newSize: number) => void;
  handleSort: (columnName: string) => void;
  setError: (error: string | null) => void;
}

const TableContext = createContext<TableContextType | undefined>(undefined);

interface TableProviderProps {
  children: React.ReactNode;
  connection: DataTableConnection;
  editable?: boolean;
}

export const TableProvider: React.FC<TableProviderProps> = ({
  children,
  connection,
  editable = false,
}) => {
  const [data, setData] = useState<DataRow[]>([]);
  const [columns, setColumns] = useState<DataColumn[]>([]);
  const [columnWidths, setColumnWidths] = useState<Record<string, number>>({});
  const [visibleRows, setVisibleRows] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeFilter] = useState<Filter | null>(null);
  const [activeSort, setActiveSort] = useState<SortOrder[] | null>(null);

  const loadingRef = useRef(false);
  const currentRowCountRef = useRef(0);
  const batchSize = 20;

  // Reset row count and column widths when connection changes
  useEffect(() => {
    currentRowCountRef.current = 0;
    setColumnWidths({});
  }, [connection]);

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
        // When sorting, preserve the current number of rows
        // When first loading or changing connection, use batchSize
        const rowsToFetch =
          currentRowCountRef.current > 0
            ? currentRowCountRef.current
            : batchSize;

        const result = await fetchTableData(
          connection,
          0,
          rowsToFetch,
          activeFilter,
          activeSort
        );
        setColumns(result.columns);
        setData(result.rows);
        setVisibleRows(result.rows.length);
        currentRowCountRef.current = result.rows.length;
        setHasMore(result.hasMore);

        // Initialize column widths only if not already set (first load)
        setColumnWidths(prevWidths => {
          // If we already have column widths, preserve them
          if (Object.keys(prevWidths).length > 0) {
            return prevWidths;
          }

          // First time loading, initialize with default widths
          const widths: Record<string, number> = {};
          result.columns.forEach((col, index) => {
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
  }, [connection, activeFilter, activeSort]);

  // Load more data
  const loadMoreData = useCallback(async () => {
    if (loadingRef.current || !hasMore) return;

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
      const gridColumns: GridColumn[] = columns.map((col, index) => ({
        title: col.name,
        width: columnWidths[index.toString()] || col.width,
      }));

      const columnIndex = gridColumns.findIndex(
        col => col.title === column.title
      );
      if (columnIndex !== -1) {
        setColumnWidths(prev => ({
          ...prev,
          [columnIndex.toString()]: newSize,
        }));
      }
    },
    [columns, columnWidths]
  );

  // Handle sort
  const handleSort = useCallback((columnName: string) => {
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
  }, []);

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
    activeFilter,
    activeSort,
    loadMoreData,
    handleColumnResize,
    handleSort,
    setError,
  };

  return (
    <TableContext.Provider value={value}>{children}</TableContext.Provider>
  );
};

export const useTable = () => {
  const context = useContext(TableContext);
  if (!context) {
    throw new Error('useTable must be used within a TableProvider');
  }
  return context;
};
