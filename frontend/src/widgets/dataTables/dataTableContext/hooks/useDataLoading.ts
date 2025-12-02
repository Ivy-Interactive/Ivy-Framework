import { useCallback, useEffect, useRef, useState } from 'react';
import * as arrow from 'apache-arrow';
import { Filter, SortOrder } from '@/services/grpcTableService';
import {
  DataColumn,
  DataTableConnection,
  DataTableConfig,
} from '../../types/types';
import { fetchTableData } from '../../utils/tableDataFetcher';
import { parseSize } from '../utils/parseSize';

interface UseDataLoadingProps {
  connection: DataTableConnection;
  config: DataTableConfig;
  columnsProp: DataColumn[];
  activeFilter: Filter | null;
  activeSort: SortOrder[] | null;
  columnOrderLength: number;
  arrowTableRef: React.RefObject<arrow.Table | null>;
  setColumns: (columns: DataColumn[]) => void;
  setVisibleRows: (rows: number) => void;
  setError: (error: string | null) => void;
  initializeColumnOrder: (columns: DataColumn[]) => void;
  initializeColumnWidths: (columns: DataColumn[]) => void;
  initializeSortFromColumns: (columns: DataColumn[]) => boolean;
}

/**
 * Hook for managing data loading (initial load and load more)
 */
export const useDataLoading = ({
  connection,
  config,
  columnsProp,
  activeFilter,
  activeSort,
  columnOrderLength,
  arrowTableRef,
  setColumns,
  setVisibleRows,
  setError,
  initializeColumnOrder,
  initializeColumnWidths,
  initializeSortFromColumns,
}: UseDataLoadingProps) => {
  const [isLoading, setIsLoading] = useState(false);
  const [hasMore, setHasMoreState] = useState(true);
  const loadingRef = useRef(false);
  const currentRowCountRef = useRef(0);
  const batchSize = config.batchSize ?? 20;

  // Reset currentRowCountRef when filter or sort changes
  useEffect(() => {
    currentRowCountRef.current = 0;
  }, [activeFilter, activeSort]);

  // Reset row count when connection changes
  useEffect(() => {
    currentRowCountRef.current = 0;
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
        // Store Arrow table in ref for efficient access (columnar, memory-efficient)
        // This avoids storing millions of rows in React state - data is accessed directly from Arrow table
        if (result.arrowTable) {
          arrowTableRef.current = result.arrowTable;
          const rowCount = result.arrowTable.numRows;
          setVisibleRows(rowCount);
          currentRowCountRef.current = rowCount;
        }
        setHasMoreState(result.hasMore);

        // Initialize column order when columns are first loaded
        initializeColumnOrder(mergedColumns);

        // Initialize sort from column metadata (only on first load)
        const sortInitialized = initializeSortFromColumns(mergedColumns);
        if (sortInitialized) {
          // Don't fetch data again, this will trigger the effect
          return;
        }

        // Initialize column widths only if not already set (first load)
        initializeColumnWidths(mergedColumns);
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : 'Failed to load data';
        setError(errorMessage);
      } finally {
        setIsLoading(false);
      }
    };
    loadInitialData();
  }, [
    connection,
    activeFilter,
    activeSort,
    columnOrderLength,
    columnsProp,
    batchSize,
    config.loadAllRows,
    setColumns,
    setVisibleRows,
    setError,
    initializeColumnOrder,
    initializeColumnWidths,
    initializeSortFromColumns,
    arrowTableRef,
  ]);

  // Load more data
  const loadMoreData = useCallback(async () => {
    if (loadingRef.current || !hasMore || config.loadAllRows) return;

    loadingRef.current = true;
    setIsLoading(true);

    try {
      const currentRowCount = arrowTableRef.current?.numRows ?? 0;
      const result = await fetchTableData(
        connection,
        currentRowCount,
        batchSize,
        activeFilter,
        activeSort
      );

      if (result.arrowTable && result.arrowTable.numRows > 0) {
        // Concatenate Arrow tables for efficient columnar storage
        if (arrowTableRef.current) {
          // Use RecordBatch.concat to combine tables
          const batches = [
            ...arrowTableRef.current.batches,
            ...result.arrowTable.batches,
          ];
          arrowTableRef.current = new arrow.Table(batches);
        } else {
          arrowTableRef.current = result.arrowTable;
        }
        const newRowCount = arrowTableRef.current.numRows;
        setVisibleRows(newRowCount);
        currentRowCountRef.current = newRowCount;
      }

      setHasMoreState(result.hasMore);
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : 'Failed to load more data';
      setError(errorMessage);
    } finally {
      setIsLoading(false);
      loadingRef.current = false;
    }
  }, [
    connection,
    hasMore,
    activeFilter,
    activeSort,
    batchSize,
    config.loadAllRows,
    setVisibleRows,
    setError,
    arrowTableRef,
  ]);

  return {
    isLoading,
    hasMore,
    loadMoreData,
  };
};
