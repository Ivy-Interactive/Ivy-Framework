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
import { cn } from '@/lib/utils';
import { getWidth, getHeight } from '@/lib/styles';
import {
  grpcTableService,
  TableQuery,
  TableResult,
  SortOrder,
  Filter,
  Aggregation,
} from '@/services/grpcTableService';
import { useEventHandler } from '@/components/EventHandlerContext';

interface GrpcDataTableWidgetProps {
  id: string;
  serverUrl?: string;
  useGrpc?: boolean;
  selectColumns?: string[];
  sortOrders?: SortOrder[];
  filter?: Filter;
  aggregations?: Aggregation[];
  limit?: number;
  offset?: number;
  showRefreshButton?: boolean;
  showStatus?: boolean;
  width?: string;
  height?: string;
  title?: string;
  description?: string;
  resizableColumns?: boolean;
}

const GrpcDataTableWidget: React.FC<GrpcDataTableWidgetProps> = ({
  id,
  serverUrl,
  useGrpc = true,
  selectColumns,
  sortOrders,
  filter,
  aggregations,
  limit = 100,
  offset = 0,
  showRefreshButton = true,
  showStatus = true,
  width,
  height,
  title,
  description,
  resizableColumns = false,
}) => {
  const eventHandler = useEventHandler();
  const [data, setData] = useState<TableResult | null>(null);
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

  const buildTableQuery = useCallback((): TableQuery => {
    return {
      sort: sortOrders,
      filter,
      offset,
      limit,
      selectColumns,
      aggregations,
    };
  }, [sortOrders, filter, offset, limit, selectColumns, aggregations]);

  const handleQueryData = useCallback(async () => {
    if (!serverUrl) {
      setError('Server URL is required');
      return;
    }

    setLoading(true);
    setError(null);
    setIsStreaming(true);

    try {
      const query = buildTableQuery();

      await grpcTableService.queryTable({
        serverUrl,
        query,
        onData: (newData: TableResult) => {
          setData(newData);
          eventHandler('OnDataReceived', id, [newData]);
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
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : 'Failed to execute query';
      setError(errorMessage);
      eventHandler('OnError', id, [errorMessage]);
    } finally {
      setLoading(false);
    }
  }, [serverUrl, buildTableQuery, eventHandler, id]);

  const handleRefresh = useCallback(() => {
    handleQueryData();
  }, [handleQueryData]);

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
    if (serverUrl && useGrpc) {
      handleQueryData();
    }
  }, [serverUrl, useGrpc, handleQueryData]);

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
    if (data?.table) {
      return (
        <Badge variant="outline" className="flex items-center gap-1">
          <Database className="h-3 w-3" />
          {data.table.numRows} rows
        </Badge>
      );
    }
    return null;
  }, [loading, isStreaming, data]);

  if (!serverUrl) {
    return (
      <div style={styles} className="p-4">
        <Alert>
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            Server URL is required for gRPC DataTable widget
          </AlertDescription>
        </Alert>
      </div>
    );
  }

  if (!useGrpc) {
    return (
      <div style={styles} className="p-4">
        <Alert>
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            gRPC mode is required for this widget
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
        ) : data?.table && data.table.schema.fields.length > 0 ? (
          <div ref={tableRef} className="max-h-[800px] flex flex-col">
            {/* Fixed Header */}
            <div className="bg-background border-b z-10">
              <table
                className="w-full caption-bottom text-sm border-collapse"
                style={{ tableLayout: 'fixed', width: 'max-content' }}
              >
                <thead className="[&_tr]:border-b">
                  <tr className="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted">
                    {data.table.schema.fields.map((field, index) => (
                      <th
                        key={index}
                        className="h-10 px-2 text-left align-middle font-medium text-muted-foreground [&:has([role=checkbox])]:pr-0 [&>[role=checkbox]]:translate-y-[2px] relative bg-background border-r border-border"
                        style={{
                          width: columnWidths[index] || 150,
                          minWidth: '50px',
                        }}
                      >
                        <code>{field.name}</code>
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
                  {data.table.toArray().map((row, rowIndex) => (
                    <tr
                      key={rowIndex}
                      className="border-b transition-colors hover:bg-muted/50 data-[state=selected]:bg-muted"
                    >
                      {row.map((cell: unknown, cellIndex: number) => (
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

export default GrpcDataTableWidget;
