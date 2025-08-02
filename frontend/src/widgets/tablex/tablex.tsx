import { Filter } from '@/services/grpcTableService';
import DataEditor, {
  DataEditorRef,
  GridCell,
  GridCellKind,
  GridColumn,
  Item,
} from '@glideapps/glide-data-grid';
import '@glideapps/glide-data-grid/dist/index.css';
import React, { useCallback, useEffect, useRef, useState } from 'react';

// Local imports
import { ErrorDisplay } from './parts/ErrorDisplay';
import { Footer } from './parts/Footer';
import { Header } from './parts/Header';
import { LoadingDisplay } from './parts/LoadingDisplay';
import { tableStyles } from './styles';
import { tableTheme } from './styles/theme';
import { DataColumn, DataRow, InfiniteScrollGlideGridProps } from './types';
import { fetchTableData } from './utils';

export const InfiniteScrollGlideGrid: React.FC<
  InfiniteScrollGlideGridProps
> = ({ connection, editable = false }) => {
  const [data, setData] = useState<DataRow[]>([]);
  const [columns, setColumns] = useState<DataColumn[]>([]);
  const [columnWidths, setColumnWidths] = useState<Record<string, number>>({});
  const [visibleRows, setVisibleRows] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeFilter] = useState<Filter | null>(null);
  const loadingRef = useRef(false);
  const scrollTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const gridRef = useRef<DataEditorRef>(null);
  const batchSize = 20;
  const scrollThreshold = 10;

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

  // Handle scroll events
  const handleVisibleRegionChanged = useCallback(
    (range: { x: number; y: number; width: number; height: number }) => {
      // Clear any existing timeout
      if (scrollTimeoutRef.current) {
        clearTimeout(scrollTimeoutRef.current);
      }

      // Debounce the scroll check
      scrollTimeoutRef.current = setTimeout(() => {
        const bottomRow = range.y + range.height;
        const shouldLoadMore = bottomRow >= visibleRows - scrollThreshold;

        if (shouldLoadMore && hasMore && !loadingRef.current) {
          loadMoreData();
        }
      }, 100);
    },
    [visibleRows, hasMore, loadMoreData]
  );

  // Get cell content
  const getCellContent = useCallback(
    (cell: Item): GridCell => {
      const [col, row] = cell;

      // Safety check
      if (row >= data.length || col >= columns.length) {
        return {
          kind: GridCellKind.Text,
          data: '',
          displayData: '',
          allowOverlay: false,
          readonly: true,
        };
      }

      const rowData = data[row];
      const cellValue = rowData.values[col];
      const columnType = columns[col].type;

      // Handle null/undefined values
      if (cellValue === null || cellValue === undefined) {
        return {
          kind: GridCellKind.Text,
          data: '',
          displayData: 'null',
          allowOverlay: editable,
          readonly: !editable,
          style: 'faded',
        };
      }

      // Determine cell type based on Arrow data type and value
      if (typeof cellValue === 'number' && columnType.includes('int')) {
        return {
          kind: GridCellKind.Number,
          data: cellValue,
          displayData: cellValue.toString(),
          allowOverlay: editable,
          readonly: !editable,
        };
      } else if (typeof cellValue === 'boolean') {
        return {
          kind: GridCellKind.Boolean,
          data: cellValue,
          allowOverlay: false,
          readonly: !editable,
        };
      } else {
        // Default to text for strings and other types
        return {
          kind: GridCellKind.Text,
          data: String(cellValue),
          displayData: String(cellValue),
          allowOverlay: editable,
          readonly: !editable,
        };
      }
    },
    [data, columns, editable]
  );

  // Convert our columns to GridColumn format with current widths
  const gridColumns: GridColumn[] = columns.map((col, index) => ({
    title: col.name,
    width: columnWidths[index.toString()] || col.width,
  }));

  // Handle column resize
  const handleColumnResize = useCallback(
    (column: GridColumn, newSize: number) => {
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
    [gridColumns]
  );

  if (error) {
    return <ErrorDisplay error={error} />;
  }

  return (
    <div className={tableStyles.container}>
      <Header
        visibleRows={visibleRows}
        columnsLength={columns.length}
        editable={editable}
        isLoading={isLoading}
        hasMore={hasMore}
      />

      {gridColumns.length > 0 ? (
        <div style={tableStyles.gridContainer}>
          <DataEditor
            ref={gridRef}
            columns={gridColumns}
            rows={visibleRows}
            getCellContent={getCellContent}
            onColumnResize={handleColumnResize}
            onVisibleRegionChanged={handleVisibleRegionChanged}
            smoothScrollX={true}
            smoothScrollY={true}
            theme={tableTheme}
            rowHeight={36}
            headerHeight={36}
            freezeColumns={1}
            getCellsForSelection={true}
            keybindings={{ search: false }}
            rightElement={<div className={tableStyles.rightPadding} />}
            columnSelect="single"
            rangeSelect="rect"
          />
        </div>
      ) : (
        <LoadingDisplay isLoading={isLoading} />
      )}

      <Footer editable={editable} />
    </div>
  );
};

export default InfiniteScrollGlideGrid;
