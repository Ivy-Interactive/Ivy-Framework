import React, {
  createContext,
  useContext,
  useState,
  useCallback,
  useRef,
  useEffect,
} from 'react';
import { Filter } from '@/services/grpcTableService';
import { GridColumn } from '@glideapps/glide-data-grid';
import { DataRow, DataColumn, DataTableConnection } from '../types';
import { fetchTableData } from '../utils';

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

  // Methods
  loadMoreData: () => Promise<void>;
  handleColumnResize: (column: GridColumn, newSize: number) => void;
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

  const loadingRef = useRef(false);
  const batchSize = 20;

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
        const result = await fetchTableData(
          connection,
          0,
          batchSize,
          activeFilter
        );
        setColumns(result.columns);
        setData(result.rows);
        setVisibleRows(result.rows.length);
        setHasMore(result.hasMore);

        // Initialize column widths
        const widths: Record<string, number> = {};
        result.columns.forEach((col, index) => {
          widths[index.toString()] = col.width;
        });
        setColumnWidths(widths);
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : 'Failed to load data';
        setError(errorMessage);
      } finally {
        setIsLoading(false);
      }
    };
    loadInitialData();
  }, [connection, activeFilter]);

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
        activeFilter
      );

      if (result.rows.length > 0) {
        setData(prev => [...prev, ...result.rows]);
        setVisibleRows(prev => prev + result.rows.length);
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
  }, [connection, data.length, hasMore, activeFilter]);

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
    loadMoreData,
    handleColumnResize,
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
