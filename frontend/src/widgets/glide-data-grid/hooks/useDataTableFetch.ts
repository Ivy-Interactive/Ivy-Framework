import * as arrow from 'apache-arrow';
import { useCallback, useEffect, useRef, useState } from 'react';

import { getIvyHost } from '@/lib/utils';
import {
  grpcTableService,
  TableQuery,
  TableResult,
} from '@/services/grpcTableService';

import { DataTableConnection, DataTableData } from '../types';
import { convertArrowTableToDataTableData } from '../utils/arrowDataTransforms';

export interface UseDataTableFetchReturn {
  data: DataTableData | null;
  loading: boolean;
  error: string | null;
  isStreaming: boolean;
  refresh: () => void;
}

export function useDataTableFetch(
  connection: DataTableConnection,
  options: {
    pageSize?: number;
    onDataReceived?: (data: DataTableData) => void;
    onError?: (error: string) => void;
    onStreamComplete?: () => void;
  } = {}
): UseDataTableFetchReturn {
  const { pageSize = 100, onDataReceived, onError, onStreamComplete } = options;

  // State
  const [data, setData] = useState<DataTableData | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isStreaming, setIsStreaming] = useState(false);

  // Use refs to store callback functions to avoid recreating fetchData on every render
  const callbacksRef = useRef({ onDataReceived, onError, onStreamComplete });
  callbacksRef.current = { onDataReceived, onError, onStreamComplete };

  // Data fetching function
  const fetchData = useCallback(async () => {
    if (!connection.port || !connection.path) {
      setError('Connection configuration is required');
      return;
    }

    setLoading(true);
    setError(null);
    setIsStreaming(true);

    try {
      const backendUrl = new URL(getIvyHost());
      const serverUrl = `${backendUrl.protocol}//${backendUrl.hostname}:${connection.port}`;

      const query: TableQuery = {
        limit: pageSize,
        offset: 0,
        connectionId: connection.connectionId,
        sourceId: connection.sourceId,
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
              callbacksRef.current.onDataReceived?.(newData);
            } catch (error) {
              console.error('Failed to parse Arrow IPC stream:', error);
              setError('Failed to parse data from server');
            }
          }
        },
        onError: (err: Error) => {
          setError(err.message);
          callbacksRef.current.onError?.(err.message);
        },
        onComplete: () => {
          setIsStreaming(false);
          callbacksRef.current.onStreamComplete?.();
        },
      });

      // Handle direct result if no streaming callback was triggered
      if (result.arrow_ipc_stream) {
        try {
          const table = arrow.tableFromIPC(result.arrow_ipc_stream);
          const newData = convertArrowTableToDataTableData(table);
          setData(newData);
          callbacksRef.current.onDataReceived?.(newData);
        } catch (error) {
          console.error('Failed to parse Arrow IPC stream:', error);
          setError('Failed to parse data from server');
        }
      }
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : 'Failed to connect to stream';
      setError(errorMessage);
      callbacksRef.current.onError?.(errorMessage);
    } finally {
      setLoading(false);
    }
  }, [connection, pageSize]);

  // Refresh function
  const refresh = useCallback(() => {
    if (isStreaming) {
      grpcTableService.disconnect();
    }
    fetchData();
  }, [fetchData, isStreaming]);

  // Auto-fetch on mount and connection changes
  useEffect(() => {
    if (connection.port && connection.path) {
      fetchData();
    }
  }, [connection.port, connection.path, fetchData]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (isStreaming) {
        grpcTableService.disconnect();
      }
    };
  }, [isStreaming]);

  return {
    data,
    loading,
    error,
    isStreaming,
    refresh,
  };
}
