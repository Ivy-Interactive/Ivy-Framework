import React, {
  useState,
  useEffect,
  useCallback,
  useMemo,
  useRef,
} from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, RefreshCw, Database, AlertCircle } from 'lucide-react';
import { cn } from '@/lib/utils';
import { getWidth, getHeight } from '@/lib/styles';
import {
  arrowTableService,
  ArrowTableData,
} from '@/services/arrowTableService';
import { mockArrowServer } from '@/services/mockArrowServer';
import { useEventHandler } from '@/components/EventHandlerContext';

interface DataTableWidgetProps {
  id: string;
  serverUrl?: string;
  query?: string;
  limit?: number;
  offset?: number;
  showQueryInput?: boolean;
  showRefreshButton?: boolean;
  showStatus?: boolean;
  width?: string;
  height?: string;
  title?: string;
  description?: string;
  resizableColumns?: boolean;
}

const DataTableWidget: React.FC<DataTableWidgetProps> = ({
  id,
  serverUrl,
  query: initialQuery = '',
  limit = 100,
  offset = 0,
  showQueryInput = true,
  showRefreshButton = true,
  showStatus = true,
  width,
  height,
  title,
  description,
  resizableColumns = false,
}) => {
  const eventHandler = useEventHandler();
  const [data, setData] = useState<ArrowTableData | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [query, setQuery] = useState(initialQuery);
  const [isStreaming, setIsStreaming] = useState(false);
  const [columnWidths, setColumnWidths] = useState<Record<number, number>>({});
  const [resizingColumn, setResizingColumn] = useState<number | null>(null);
  const tableRef = useRef<HTMLDivElement>(null);

  const styles = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const handleStreamData = useCallback(async () => {
    if (!serverUrl) {
      setError('Server URL is required');
      return;
    }

    setLoading(true);
    setError(null);
    setIsStreaming(true);

    try {
      // Use mock server for development/testing
      if (
        process.env.NODE_ENV === 'development' ||
        serverUrl.includes('localhost')
      ) {
        const mockData = await mockArrowServer.streamData(query, limit);
        setData(mockData);
        eventHandler('OnDataReceived', id, [mockData]);
        setIsStreaming(false);
        eventHandler('OnStreamComplete', id, []);
      } else {
        // Use real gRPC service

        // todo: fix this
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const service = new (arrowTableService.constructor as any)(serverUrl);

        await service.streamTableData({
          serverUrl,
          query,
          limit,
          offset,
          onData: (newData: ArrowTableData) => {
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
      }
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : 'Failed to connect to stream';
      setError(errorMessage);
      eventHandler('OnError', id, [errorMessage]);
    } finally {
      setLoading(false);
    }
  }, [serverUrl, query, limit, offset, eventHandler, id]);

  const handleRefresh = useCallback(() => {
    if (isStreaming) {
      arrowTableService.disconnect();
    }
    handleStreamData();
  }, [handleStreamData, isStreaming]);

  const handleQueryChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      setQuery(e.target.value);
    },
    []
  );

  const handleQuerySubmit = useCallback(
    (e: React.FormEvent) => {
      e.preventDefault();
      handleRefresh();
    },
    [handleRefresh]
  );

  // Column resizing handlers
  const handleResizeStart = useCallback(
    (columnIndex: number, e: React.MouseEvent) => {
      if (!resizableColumns) return;
      e.preventDefault();
      setResizingColumn(columnIndex);
    },
    [resizableColumns]
  );

  const handleResizeMove = useCallback(
    (e: MouseEvent) => {
      if (resizingColumn === null || !tableRef.current) return;

      const tableRect = tableRef.current.getBoundingClientRect();
      const newWidth = e.clientX - tableRect.left;

      setColumnWidths(prev => ({
        ...prev,
        [resizingColumn]: Math.max(100, newWidth), // Minimum width of 100px
      }));
    },
    [resizingColumn]
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
    if (serverUrl && initialQuery) {
      handleStreamData();
    }
  }, [serverUrl, initialQuery, handleStreamData]);

  useEffect(() => {
    return () => {
      if (isStreaming) {
        arrowTableService.disconnect();
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

  if (!serverUrl) {
    return (
      <div style={styles} className="p-4">
        <Alert>
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            Server URL is required for DataTable widget
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

      {/* Query Input */}
      {showQueryInput && (
        <div className="p-4 border-b">
          <form onSubmit={handleQuerySubmit} className="flex gap-2">
            <Input
              value={query}
              onChange={handleQueryChange}
              placeholder="Enter SQL query..."
              className="flex-1"
              disabled={loading}
            />
            <Button type="submit" disabled={loading}>
              Execute
            </Button>
          </form>
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
          <div ref={tableRef} className="max-h-[800px] overflow-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  {data.columns.map((column, index) => (
                    <TableHead
                      key={index}
                      className="font-medium relative"
                      style={{
                        width: columnWidths[index]
                          ? `${columnWidths[index]}px`
                          : 'auto',
                        minWidth: '100px',
                      }}
                    >
                      <code>{column}</code>
                      {resizableColumns && (
                        <div
                          className="absolute right-0 top-0 bottom-0 w-1 cursor-col-resize bg-transparent hover:bg-blue-300"
                          onMouseDown={e => handleResizeStart(index, e)}
                        />
                      )}
                    </TableHead>
                  ))}
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.rows.map((row, rowIndex) => (
                  <TableRow key={rowIndex}>
                    {row.map((cell, cellIndex) => (
                      <TableCell
                        key={cellIndex}
                        className="truncate"
                        style={{
                          width: columnWidths[cellIndex]
                            ? `${columnWidths[cellIndex]}px`
                            : 'auto',
                          minWidth: '100px',
                        }}
                      >
                        {cell !== null && cell !== undefined
                          ? String(cell)
                          : 'null'}
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            {data.hasMore && (
              <div className="p-4 text-center text-sm text-muted-foreground">
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
