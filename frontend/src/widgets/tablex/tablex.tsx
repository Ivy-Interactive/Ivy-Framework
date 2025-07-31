import { getIvyHost } from '@/lib/utils';
import { grpcTableService, TableQuery } from '@/services/grpcTableService';
import DataEditor, {
  DataEditorRef,
  EditableGridCell,
  GridCell,
  GridCellKind,
  GridColumn,
  Item,
  Theme,
} from '@glideapps/glide-data-grid';
import '@glideapps/glide-data-grid/dist/index.css';
import * as arrow from 'apache-arrow';
import React, { useCallback, useEffect, useRef, useState } from 'react';

// Types
interface DataRow {
  values: (string | number | boolean | null)[];
}

interface DataColumn {
  name: string;
  type: string;
  width: number;
}

interface DataTableConnection {
  port: number;
  path: string;
  connectionId: string;
  sourceId: string;
}

interface InfiniteScrollGlideGridProps {
  connection: DataTableConnection;
  editable?: boolean; // Allow component to be configurable
  onCellUpdate?: (row: number, col: number, value: unknown) => void; // Callback for cell updates
}

// Helper function to convert Arrow table to our data format
function convertArrowTableToData(
  table: arrow.Table,
  requestedCount: number
): {
  columns: DataColumn[];
  rows: DataRow[];
  hasMore: boolean;
} {
  const columns: DataColumn[] = table.schema.fields.map(field => ({
    name: field.name,
    type: field.type.toString(),
    width: 150, // Default width, can be made configurable
  }));

  const rows: DataRow[] = [];
  for (let i = 0; i < table.numRows; i++) {
    const values: (string | number | boolean | null)[] = [];
    for (let j = 0; j < table.numCols; j++) {
      const column = table.getChildAt(j);
      if (column) {
        const value = column.get(i);
        values.push(value);
      }
    }
    rows.push({ values });
  }

  // If we received exactly the requested amount, there might be more
  // If we received less, we've reached the end
  const hasMore = table.numRows === requestedCount;

  return {
    columns,
    rows,
    hasMore,
  };
}

// Fetch data using grpcTableService
const fetchTableData = async (
  connection: DataTableConnection,
  startIndex: number,
  count: number
): Promise<{ columns: DataColumn[]; rows: DataRow[]; hasMore: boolean }> => {
  const backendUrl = new URL(getIvyHost());
  const serverUrl = `${backendUrl.protocol}//${backendUrl.hostname}:${connection.port}`;

  const query: TableQuery = {
    limit: count,
    offset: startIndex,
    connectionId: connection.connectionId,
    sourceId: connection.sourceId,
  };

  try {
    const result = await grpcTableService.queryTable({
      serverUrl,
      query,
    });

    if (result.arrow_ipc_stream) {
      const table = arrow.tableFromIPC(result.arrow_ipc_stream);
      return convertArrowTableToData(table, count);
    }

    return { columns: [], rows: [], hasMore: false };
  } catch (error) {
    console.error('Failed to fetch table data:', error);
    throw error;
  }
};

// Custom theme
const theme: Partial<Theme> = {
  bgCell: '#fff',
  bgHeader: '#f9fafb',
  bgHeaderHasFocus: '#f3f4f6',
  bgHeaderHovered: '#e5e7eb',
  textDark: '#111827',
  textMedium: '#6b7280',
  textLight: '#9ca3af',
  borderColor: '#e5e7eb',
  linkColor: '#e5e7eb',
  cellHorizontalPadding: 8,
  cellVerticalPadding: 3,
};

export const InfiniteScrollGlideGrid: React.FC<
  InfiniteScrollGlideGridProps
> = ({ connection, editable = false, onCellUpdate }) => {
  const [data, setData] = useState<DataRow[]>([]);
  const [columns, setColumns] = useState<DataColumn[]>([]);
  const [columnWidths, setColumnWidths] = useState<Record<string, number>>({});
  const [visibleRows, setVisibleRows] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const loadingRef = useRef(false);
  const scrollTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const gridRef = useRef<DataEditorRef>(null);
  const batchSize = 20;
  const scrollThreshold = 10; // Load more when within 10 rows of the bottom

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
        const result = await fetchTableData(connection, 0, batchSize);
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
  }, [connection]);

  // Load more data
  const loadMoreData = useCallback(async () => {
    if (loadingRef.current || !hasMore) return;

    loadingRef.current = true;
    setIsLoading(true);

    try {
      const result = await fetchTableData(connection, data.length, batchSize);

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
  }, [connection, data.length, hasMore]);

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

  // Handle cell edits
  const handleCellEdited = useCallback(
    (cell: Item, newValue: EditableGridCell) => {
      if (!editable) return;

      const [col, row] = cell;

      // Update local data
      setData(prevData => {
        const newData = [...prevData];
        const rowData = { ...newData[row] };
        const values = [...rowData.values];

        // Extract the actual value based on cell type
        let actualValue: string | number | boolean | null;
        switch (newValue.kind) {
          case GridCellKind.Text:
            actualValue = newValue.data;
            break;
          case GridCellKind.Number:
            actualValue = newValue.data ?? 0; // Default to 0 for numbers
            break;
          case GridCellKind.Boolean:
            actualValue = newValue.data ?? false; // Default to false for booleans
            break;
          default:
            actualValue = null; // Fallback for unknown cell types
        }

        values[col] = actualValue;
        rowData.values = values;
        newData[row] = rowData;

        return newData;
      });

      // Call the update callback if provided
      if (onCellUpdate) {
        // Extract the actual value based on cell type
        let value: unknown;
        switch (newValue.kind) {
          case GridCellKind.Text:
            value = newValue.data;
            break;
          case GridCellKind.Number:
            value = newValue.data;
            break;
          case GridCellKind.Boolean:
            value = newValue.data;
            break;
          default:
            value = newValue.data;
        }

        onCellUpdate(row, col, value);
      }
    },
    [editable, onCellUpdate]
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
    return (
      <div className="p-4">
        <div className="text-red-600 mb-4">Error: {error}</div>
        <button
          onClick={() => window.location.reload()}
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
        >
          Retry
        </button>
      </div>
    );
  }

  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold mb-4">Dynamic Data Grid with gRPC</h1>

      <div className="mb-2 text-sm text-gray-600 flex items-center gap-4">
        <span>Showing {visibleRows} rows</span>
        {columns.length > 0 && <span>{columns.length} columns</span>}
        {editable && <span className="text-blue-600">✏️ Editable</span>}
        {isLoading && (
          <span className="flex items-center">
            <div className="animate-spin h-4 w-4 border-2 border-gray-500 border-t-transparent rounded-full mr-2"></div>
            Loading more...
          </span>
        )}
        {!hasMore && visibleRows > 0 && (
          <span className="text-gray-500">All data loaded</span>
        )}
      </div>

      {gridColumns.length > 0 ? (
        <div style={{ height: '600px', width: '100%' }}>
          <DataEditor
            ref={gridRef}
            columns={gridColumns}
            rows={visibleRows}
            getCellContent={getCellContent}
            onCellEdited={handleCellEdited}
            onColumnResize={handleColumnResize}
            onVisibleRegionChanged={handleVisibleRegionChanged}
            smoothScrollX={true}
            smoothScrollY={true}
            theme={theme}
            rowHeight={36}
            headerHeight={36}
            freezeColumns={1}
            getCellsForSelection={true}
            keybindings={{ search: false }}
            rightElement={<div className="pr-2" />}
            columnSelect="single"
            rangeSelect="rect"
          />
        </div>
      ) : (
        <div className="flex items-center justify-center h-64 text-gray-500">
          {isLoading ? 'Loading data...' : 'No data available'}
        </div>
      )}

      <div className="mt-4 text-sm text-gray-500">
        {editable
          ? 'Click any cell to edit. Drag column borders to resize.'
          : 'Data fetched from gRPC service. Grid grows dynamically as you scroll.'}
      </div>
    </div>
  );
};

export default InfiniteScrollGlideGrid;
