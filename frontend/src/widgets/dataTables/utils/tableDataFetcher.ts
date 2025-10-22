import { getIvyHost } from '@/lib/utils';
import {
  Filter,
  SortOrder,
  TableQuery,
  grpcTableService,
} from '@/services/grpcTableService';
import * as arrow from 'apache-arrow';
import { DataColumn, DataRow, DataTableConnection } from '../types/types';
import { convertArrowTableToData } from './tableDataMapper';

export const fetchTableData = async (
  connection: DataTableConnection,
  startIndex: number,
  count: number,
  filter?: Filter | null,
  sort?: SortOrder[] | null
): Promise<{ columns: DataColumn[]; rows: DataRow[]; hasMore: boolean }> => {
  const backendUrl = new URL(getIvyHost());

  // For development (localhost), use the connection port
  // For production (deployed), use the current host without port
  const isDevelopment =
    backendUrl.hostname === 'localhost' || backendUrl.hostname === '127.0.0.1';
  const serverUrl = isDevelopment
    ? `${backendUrl.protocol}//${backendUrl.hostname}:${connection.port}`
    : `${backendUrl.protocol}//${backendUrl.hostname}${backendUrl.port ? `:${backendUrl.port}` : ''}`;

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
