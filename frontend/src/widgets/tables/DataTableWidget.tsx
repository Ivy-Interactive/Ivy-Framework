import React, {
  useState,
  useEffect,
  useCallback,
  useMemo,
  useRef,
} from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, RefreshCw, Database, AlertCircle } from 'lucide-react';
import { cn, getIvyHost } from '@/lib/utils';
import { getWidth, getHeight } from '@/lib/styles';
import { useEventHandler } from '@/components/EventHandlerContext';
import {
  grpcTableService,
  TableQuery,
  TableResult,
} from '@/services/grpcTableService';
import * as arrow from 'apache-arrow';

interface DataTableColumn {
  name: string;
  type: string;
}

interface DataTableRow {
  values: (string | number | boolean | null)[];
}

interface DataTableData {
  columns: DataTableColumn[];
  rows: DataTableRow[];
  totalRows: number;
  hasMore: boolean;
}

interface DataTableConnection {
  port: number;
  path: string;
  connectionId: string;
  sourceId: string;
}

interface DataTableWidgetProps {
  id: string;
  connection: DataTableConnection;
  width?: string;
  height?: string;
  title?: string;
  description?: string;
  showRefreshButton?: boolean;
  showStatus?: boolean;
  resizableColumns?: boolean;
}

// Helper function to convert Arrow table to DataTableData
function convertArrowTableToDataTableData(table: arrow.Table): DataTableData {
  const columns: DataTableColumn[] = table.schema.fields.map(field => ({
    name: field.name,
    type: field.type.toString(),
  }));

  const rows: DataTableRow[] = [];
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

  return {
    columns,
    rows,
    totalRows: table.numRows,
    hasMore: false, // This would be determined by the server response
  };
}

const DataTableWidget: React.FC<DataTableWidgetProps> = ({
  id,
  connection,
  width,
  height,
  title,
  description,
  showRefreshButton = true,
  showStatus = true,
  resizableColumns = false,
}) => {
  const eventHandler = useEventHandler();
  const [data, setData] = useState<DataTableData | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isStreaming, setIsStreaming] = useState(false);
  const [columnWidths, setColumnWidths] = useState<Record<number, number>>({});
  const [resizingColumn, setResizingColumn] = useState<number | null>(null);
  const [resizeStartX, setResizeStartX] = useState<number>(0);
  const [resizeStartWidth, setResizeStartWidth] = useState<number>(0);
  const tableRef = useRef<HTMLDivElement>(null);

  const styles = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const handleStreamData = useCallback(async () => {
    if (!connection.port || !connection.path) {
      setError('Connection configuration is required');
      return;
    }

    setLoading(true);
    setError(null);
    setIsStreaming(true);

    try {
      // Connect directly to the gRPC service using the connection info
      const backendUrl = new URL(getIvyHost());
      const serverUrl = `${backendUrl.protocol}//${backendUrl.hostname}:${connection.port}`;
      const query: TableQuery = {
        limit: 100,
        offset: 0,
        connectionId: connection.connectionId,
        sourceId: connection.sourceId,
        // Use the sourceId from the connection to identify the data source
        // The backend gRPC service should use this to return the correct data
      };

      const result = await grpcTableService.queryTable({
        serverUrl,
        query,
        onData: (tableResult: TableResult) => {
          if (tableResult.arrow_ipc_stream) {
            try {
              const table = arrow.tableFromIPC(tableResult.arrow_ipc_stream);
              const newData = convertArrowTableToDataTableData(table);
              setData(newData);
              eventHandler('OnDataReceived', id, [newData]);
            } catch (error) {
              console.error('Failed to parse Arrow IPC stream:', error);
              setError('Failed to parse data from server');
            }
          }
        },
        onError: (err: Error) => {
          setError(err.message);
          eventHandler('OnError', id, [err.message]);
        },
        onComplete: () => {
          setIsStreaming(false);
          eventHandler('OnStreamComplete', id, []);
        },
      });

      // Handle the result if no onData callback was provided
      if (result.arrow_ipc_stream) {
        try {
          const table = arrow.tableFromIPC(result.arrow_ipc_stream);
          const newData = convertArrowTableToDataTableData(table);
          setData(newData);
          eventHandler('OnDataReceived', id, [newData]);
        } catch (error) {
          console.error('Failed to parse Arrow IPC stream:', error);
          setError('Failed to parse data from server');
        }
      }
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : 'Failed to connect to stream';
      setError(errorMessage);
      eventHandler('OnError', id, [errorMessage]);
    } finally {
      setLoading(false);
    }
  }, [connection, eventHandler, id]);

  const handleRefresh = useCallback(() => {
    if (isStreaming) {
      grpcTableService.disconnect();
    }
    handleStreamData();
  }, [handleStreamData, isStreaming]);

  // Column resizing handlers
  const handleResizeStart = useCallback(
    (columnIndex: number, e: React.MouseEvent) => {
      if (!resizableColumns) return;
      e.preventDefault();
      setResizingColumn(columnIndex);
      setResizeStartX(e.clientX);
      setResizeStartWidth(columnWidths[columnIndex] || 100);
    },
    [resizableColumns, columnWidths]
  );

  const handleResizeMove = useCallback(
    (e: MouseEvent) => {
      if (resizingColumn === null) return;

      const deltaX = e.clientX - resizeStartX;
      const newWidth = Math.max(50, resizeStartWidth + deltaX);

      setColumnWidths(prev => ({
        ...prev,
        [resizingColumn]: newWidth,
      }));
    },
    [resizingColumn, resizeStartX, resizeStartWidth]
  );

  const handleResizeEnd = useCallback(() => {
    setResizingColumn(null);
  }, []);

  useEffect(() => {
    if (resizingColumn !== null) {
      document.addEventListener('mousemove', handleResizeMove);
      document.addEventListener('mouseup', handleResizeEnd);

      return () => {
        document.removeEventListener('mousemove', handleResizeMove);
        document.removeEventListener('mouseup', handleResizeEnd);
      };
    }
  }, [resizingColumn, handleResizeMove, handleResizeEnd]);

  useEffect(() => {
    if (connection.port && connection.path) {
      handleStreamData();
    }
  }, [connection.port, connection.path, handleStreamData]);

  useEffect(() => {
    return () => {
      if (isStreaming) {
        grpcTableService.disconnect();
      }
    };
  }, [isStreaming]);

  const statusBadge = useMemo(() => {
    if (loading) {
      return (
        <Badge variant="secondary" className="flex items-center gap-1">
          <Loader2 className="h-3 w-3 animate-spin" />
          Loading...
        </Badge>
      );
    }
    if (isStreaming) {
      return (
        <Badge variant="default" className="flex items-center gap-1">
          <Database className="h-3 w-3" />
          Streaming
        </Badge>
      );
    }
    if (data) {
      return (
        <Badge variant="outline" className="flex items-center gap-1">
          <Database className="h-3 w-3" />
          {data.totalRows} rows
        </Badge>
      );
    }
    return null;
  }, [loading, isStreaming, data]);

  if (!connection.port || !connection.path) {
    return (
      <div style={styles} className="p-4">
        <Alert>
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            Connection configuration is required for DataTable widget
          </AlertDescription>
        </Alert>
      </div>
    );
  }

  return (
    <div style={styles} className="flex flex-col h-full">
      {/* Header */}
      {(title || description || showStatus || showRefreshButton) && (
        <div className="flex items-center justify-between p-4 border-b">
          <div className="flex-1">
            {title && <h3 className="text-lg font-semibold">{title}</h3>}
            {description && (
              <p className="text-sm text-muted-foreground">{description}</p>
            )}
          </div>
          <div className="flex items-center gap-2">
            {showStatus && statusBadge}
            {showRefreshButton && (
              <Button
                variant="outline"
                size="sm"
                onClick={handleRefresh}
                disabled={loading}
                className="flex items-center gap-1"
              >
                <RefreshCw
                  className={cn('h-3 w-3', loading && 'animate-spin')}
                />
                Refresh
              </Button>
            )}
          </div>
        </div>
      )}

      {/* Error Display */}
      {error && (
        <div className="p-4">
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        </div>
      )}

      {/* Table Content */}
      <div className="flex-1 overflow-hidden">
        {loading && !data ? (
          <div className="flex items-center justify-center h-full">
            <div className="flex items-center gap-2">
              <Loader2 className="h-4 w-4 animate-spin" />
              <span>Loading data...</span>
            </div>
          </div>
        ) : data && data.columns.length > 0 ? (
          <div ref={tableRef} className="max-h-[800px] flex flex-col">
            {/* Fixed Header */}
            <div className="bg-background border-b z-10">
              <table
                className="w-full caption-bottom text-sm border-collapse"
                style={{ tableLayout: 'fixed', width: 'max-content' }}
              >
                <thead className="[&_tr]:border-b">
                  <tr className="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted">
                    {data.columns.map((column, index) => (
                      <th
                        key={index}
                        className="h-10 px-2 text-left align-middle font-medium text-muted-foreground [&:has([role=checkbox])]:pr-0 [&>[role=checkbox]]:translate-y-[2px] relative bg-background border-r border-border"
                        style={{
                          width: columnWidths[index] || 150,
                          minWidth: '50px',
                        }}
                      >
                        <code>{column.name}</code>
                        {resizableColumns && (
                          <div
                            className="absolute right-0 top-0 bottom-0 w-1 cursor-col-resize bg-transparent hover:bg-blue-300"
                            onMouseDown={e => handleResizeStart(index, e)}
                          />
                        )}
                      </th>
                    ))}
                  </tr>
                </thead>
              </table>
            </div>

            {/* Scrollable Body */}
            <div className="flex-1 overflow-auto">
              <table
                className="w-full caption-bottom text-sm border-collapse"
                style={{ tableLayout: 'fixed', width: 'max-content' }}
              >
                <tbody className="[&_tr:last-child]:border-0">
                  {data.rows.map((row, rowIndex) => (
                    <tr
                      key={rowIndex}
                      className="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted"
                    >
                      {row.values.map((cell, cellIndex) => (
                        <td
                          key={cellIndex}
                          className="p-2 align-middle [&:has([role=checkbox])]:pr-0 [&>[role=checkbox]]:translate-y-[2px] truncate border-r border-border"
                          style={{
                            width: columnWidths[cellIndex] || 150,
                            minWidth: '50px',
                          }}
                        >
                          {cell !== null && cell !== undefined
                            ? String(cell)
                            : 'null'}
                        </td>
                      ))}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {data.hasMore && (
              <div className="p-4 text-center text-sm text-muted-foreground border-t">
                More data available...
              </div>
            )}
          </div>
        ) : (
          <div className="flex items-center justify-center h-full text-muted-foreground">
            No data available
          </div>
        )}
      </div>
    </div>
  );
};

export default DataTableWidget;
