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

export const parseInvalidQuery = async (
  invalidQuery: string,
  connection?: DataTableConnection
): Promise<ParseFilterResult> => {
  try {
    // If connection is provided, configure the server URL
    // let serverUrl = '';
    // if (connection) {
    //   const backendUrl = new URL(getIvyHost());
    //   serverUrl = `${backendUrl.protocol}//${backendUrl.hostname}:${connection.port}`;
    //   // Configure grpcTableService with the server URL
    //   (grpcTableService as any).serverUrl = serverUrl;
    // }

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
    console.error('Failed to parse invalid query:', error);
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
  const serverUrl = `${backendUrl.protocol}//${backendUrl.hostname}:${connection.port}`;

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
