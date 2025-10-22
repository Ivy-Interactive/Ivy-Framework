import { getIvyHost } from '@/lib/utils';
import {
  Filter,
  SortOrder,
  TableQuery,
  grpcTableService,
  ParseFilterResult,
} from '@/services/grpcTableService';
import * as arrow from 'apache-arrow';
import { DataColumn, DataRow, DataTableConnection } from '../types/types';
import { convertArrowTableToData } from './tableDataMapper';
import { logger } from '@/lib/logger';

export const parseInvalidQuery = async (
  invalidQuery: string,
  connection?: DataTableConnection
): Promise<ParseFilterResult> => {
  try {
    const backendUrl = new URL(getIvyHost());
    const serverUrl = `${backendUrl.protocol}//${backendUrl.hostname}:${connection?.port}`;

    const result = await grpcTableService.parseFilter(
      {
        payload: invalidQuery,
        connectionId: connection?.connectionId,
        sourceId: connection?.sourceId,
      },
      serverUrl
    );

    return result;
  } catch (error) {
    logger.error('Failed to parse invalid query:', error);
    throw error;
  }
};

export const fetchTableData = async (
  connection: DataTableConnection,
  startIndex: number,
  count: number,
  filter?: Filter | null,
  sort?: SortOrder[] | null
): Promise<{ columns: DataColumn[]; rows: DataRow[]; hasMore: boolean }> => {
  const backendUrl = new URL(getIvyHost());

  // Use environment variable for robust environment detection
  // In development, use the connection port; in production, use the current host
  const isDevelopment = process.env.NODE_ENV === 'development';
  const serverUrl = isDevelopment
    ? `${backendUrl.protocol}//${backendUrl.hostname}:${connection.port}`
    : `${backendUrl.protocol}//${backendUrl.hostname}`;

  const query: TableQuery = {
    limit: count,
    offset: startIndex,
    connectionId: connection.connectionId,
    sourceId: connection.sourceId,
    ...(filter && { filter }),
    ...(sort && { sort }),
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
