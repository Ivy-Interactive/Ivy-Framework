import { describe, it, expect, vi, beforeEach } from 'vitest';
import { fetchTableData } from './tableDataFetcher';
import { getIvyHost } from '@/lib/utils';
import {
  grpcTableService,
  type Filter,
  type SortOrder,
} from '@/services/grpcTableService';
import { convertArrowTableToData } from './tableDataMapper';
import * as arrow from 'apache-arrow';
import type { DataTableConnection } from '../types/types';
import { ColType } from '../types/types';

// Mock dependencies
vi.mock('@/lib/utils');
vi.mock('@/services/grpcTableService');
vi.mock('./tableDataMapper');
vi.mock('apache-arrow');

const mockGetIvyHost = vi.mocked(getIvyHost);
const mockGrpcTableService = vi.mocked(grpcTableService);
const mockConvertArrowTableToData = vi.mocked(convertArrowTableToData);
const mockArrow = vi.mocked(arrow);

describe('tableDataFetcher', () => {
  const mockConnection: DataTableConnection = {
    port: 8080,
    path: '/test',
    connectionId: 'conn-123',
    sourceId: 'source-456',
  };

  const mockColumns = [
    { name: 'id', type: ColType.Number, width: 150 },
    { name: 'name', type: ColType.Text, width: 150 },
  ];

  const mockRows = [{ values: [1, 'Alice'] }, { values: [2, 'Bob'] }];

  beforeEach(() => {
    vi.clearAllMocks();
    mockGetIvyHost.mockReturnValue('https://localhost:3000');
  });

  describe('fetchTableData', () => {
    it('should fetch table data successfully', async () => {
      const mockArrowTable = {} as arrow.Table;
      const mockResult = {
        arrow_ipc_stream: new Uint8Array([1, 2, 3]),
        offset: 0,
        row_count: 2,
        total_rows: 100,
      };

      mockGrpcTableService.queryTable.mockResolvedValue(mockResult);
      mockArrow.tableFromIPC.mockReturnValue(mockArrowTable);
      mockConvertArrowTableToData.mockReturnValue({
        columns: mockColumns,
        rows: mockRows,
        hasMore: true,
      });

      const result = await fetchTableData(mockConnection, 0, 10);

      expect(mockGrpcTableService.queryTable).toHaveBeenCalledWith({
        serverUrl: 'https://localhost:3000',
        query: {
          limit: 10,
          offset: 0,
          connectionId: 'conn-123',
          sourceId: 'source-456',
        },
      });

      expect(mockArrow.tableFromIPC).toHaveBeenCalledWith(
        mockResult.arrow_ipc_stream
      );
      expect(mockConvertArrowTableToData).toHaveBeenCalledWith(
        mockArrowTable,
        10
      );

      expect(result).toEqual({
        columns: mockColumns,
        rows: mockRows,
        hasMore: true,
        arrowTable: mockArrowTable,
      });
    });

    it('should include filter in query when provided', async () => {
      const mockFilter: Filter = {
        condition: {
          column: 'name',
          function: 'equals',
          args: ['Alice'],
        },
      };

      const mockResult = {
        arrow_ipc_stream: new Uint8Array([1, 2, 3]),
        offset: 0,
        row_count: 2,
        total_rows: 100,
      };
      mockGrpcTableService.queryTable.mockResolvedValue(mockResult);
      mockArrow.tableFromIPC.mockReturnValue({} as arrow.Table);
      mockConvertArrowTableToData.mockReturnValue({
        columns: mockColumns,
        rows: mockRows,
        hasMore: false,
      });

      await fetchTableData(mockConnection, 0, 10, mockFilter);

      expect(mockGrpcTableService.queryTable).toHaveBeenCalledWith({
        serverUrl: 'https://localhost:3000',
        query: {
          limit: 10,
          offset: 0,
          connectionId: 'conn-123',
          sourceId: 'source-456',
          filter: mockFilter,
        },
      });
    });

    it('should include sort in query when provided', async () => {
      const mockSort: SortOrder[] = [{ column: 'name', direction: 'ASC' }];

      const mockResult = {
        arrow_ipc_stream: new Uint8Array([1, 2, 3]),
        offset: 0,
        row_count: 2,
        total_rows: 100,
      };
      mockGrpcTableService.queryTable.mockResolvedValue(mockResult);
      mockArrow.tableFromIPC.mockReturnValue({} as arrow.Table);
      mockConvertArrowTableToData.mockReturnValue({
        columns: mockColumns,
        rows: mockRows,
        hasMore: false,
      });

      await fetchTableData(mockConnection, 0, 10, null, mockSort);

      expect(mockGrpcTableService.queryTable).toHaveBeenCalledWith({
        serverUrl: 'https://localhost:3000',
        query: {
          limit: 10,
          offset: 0,
          connectionId: 'conn-123',
          sourceId: 'source-456',
          sort: mockSort,
        },
      });
    });

    it('should include both filter and sort when provided', async () => {
      const mockFilter: Filter = {
        condition: {
          column: 'name',
          function: 'contains',
          args: ['A'],
        },
      };
      const mockSort: SortOrder[] = [{ column: 'id', direction: 'DESC' }];

      const mockResult = {
        arrow_ipc_stream: new Uint8Array([1, 2, 3]),
        offset: 0,
        row_count: 2,
        total_rows: 100,
      };
      mockGrpcTableService.queryTable.mockResolvedValue(mockResult);
      mockArrow.tableFromIPC.mockReturnValue({} as arrow.Table);
      mockConvertArrowTableToData.mockReturnValue({
        columns: mockColumns,
        rows: mockRows,
        hasMore: false,
      });

      await fetchTableData(mockConnection, 5, 20, mockFilter, mockSort);

      expect(mockGrpcTableService.queryTable).toHaveBeenCalledWith({
        serverUrl: 'https://localhost:3000',
        query: {
          limit: 20,
          offset: 5,
          connectionId: 'conn-123',
          sourceId: 'source-456',
          filter: mockFilter,
          sort: mockSort,
        },
      });
    });

    it('should return empty data when arrow_ipc_stream is empty', async () => {
      // Create a mock result where arrow_ipc_stream is undefined (falsy)
      const mockResult = {
        arrow_ipc_stream: undefined as unknown as Uint8Array,
        offset: 0,
        row_count: 0,
        total_rows: 0,
      };
      mockGrpcTableService.queryTable.mockResolvedValue(mockResult);

      const result = await fetchTableData(mockConnection, 0, 10);

      expect(result).toEqual({
        columns: [],
        rows: [],
        hasMore: false,
        arrowTable: null,
      });

      expect(mockArrow.tableFromIPC).not.toHaveBeenCalled();
      expect(mockConvertArrowTableToData).not.toHaveBeenCalled();
    });

    it('should handle different host URLs correctly', async () => {
      mockGetIvyHost.mockReturnValue('http://example.com:9000/path');

      const mockResult = {
        arrow_ipc_stream: new Uint8Array([1, 2, 3]),
        offset: 0,
        row_count: 2,
        total_rows: 100,
      };
      mockGrpcTableService.queryTable.mockResolvedValue(mockResult);
      mockArrow.tableFromIPC.mockReturnValue({} as arrow.Table);
      mockConvertArrowTableToData.mockReturnValue({
        columns: mockColumns,
        rows: mockRows,
        hasMore: false,
      });

      await fetchTableData(mockConnection, 0, 10);

      expect(mockGrpcTableService.queryTable).toHaveBeenCalledWith({
        serverUrl: 'http://example.com:9000/path',
        query: expect.any(Object),
      });
    });

    it('should throw error when grpcTableService fails', async () => {
      const mockError = new Error('gRPC service failed');
      mockGrpcTableService.queryTable.mockRejectedValue(mockError);

      const consoleSpy = vi
        .spyOn(console, 'error')
        .mockImplementation(() => {});

      await expect(fetchTableData(mockConnection, 0, 10)).rejects.toThrow(
        'gRPC service failed'
      );

      expect(consoleSpy).toHaveBeenCalledWith(
        'Failed to fetch table data:',
        mockError
      );

      consoleSpy.mockRestore();
    });

    it('should use serverUrl from getIvyHost regardless of connection port', async () => {
      const customConnection: DataTableConnection = {
        ...mockConnection,
        port: 9999,
      };

      const mockResult = {
        arrow_ipc_stream: new Uint8Array([1, 2, 3]),
        offset: 0,
        row_count: 2,
        total_rows: 100,
      };
      mockGrpcTableService.queryTable.mockResolvedValue(mockResult);
      mockArrow.tableFromIPC.mockReturnValue({} as arrow.Table);
      mockConvertArrowTableToData.mockReturnValue({
        columns: mockColumns,
        rows: mockRows,
        hasMore: false,
      });

      await fetchTableData(customConnection, 0, 10);

      // Should use getIvyHost() return value, not connection.port
      expect(mockGrpcTableService.queryTable).toHaveBeenCalledWith({
        serverUrl: 'https://localhost:3000',
        query: expect.any(Object),
      });
    });

    it('should handle null filter and sort parameters', async () => {
      const mockResult = {
        arrow_ipc_stream: new Uint8Array([1, 2, 3]),
        offset: 0,
        row_count: 2,
        total_rows: 100,
      };
      mockGrpcTableService.queryTable.mockResolvedValue(mockResult);
      mockArrow.tableFromIPC.mockReturnValue({} as arrow.Table);
      mockConvertArrowTableToData.mockReturnValue({
        columns: mockColumns,
        rows: mockRows,
        hasMore: false,
      });

      await fetchTableData(mockConnection, 0, 10, null, null);

      expect(mockGrpcTableService.queryTable).toHaveBeenCalledWith({
        serverUrl: 'https://localhost:3000',
        query: {
          limit: 10,
          offset: 0,
          connectionId: 'conn-123',
          sourceId: 'source-456',
        },
      });
    });

    it('should use serverUrl from getIvyHost regardless of NODE_ENV', async () => {
      // Set NODE_ENV to development (this should not affect the behavior anymore)
      process.env.NODE_ENV = 'development';

      const mockResult = {
        arrow_ipc_stream: new Uint8Array([1, 2, 3]),
        offset: 0,
        row_count: 2,
        total_rows: 100,
      };
      mockGrpcTableService.queryTable.mockResolvedValue(mockResult);
      mockArrow.tableFromIPC.mockReturnValue({} as arrow.Table);
      mockConvertArrowTableToData.mockReturnValue({
        columns: mockColumns,
        rows: mockRows,
        hasMore: false,
      });

      await fetchTableData(mockConnection, 0, 10);

      // Should use getIvyHost() return value, NODE_ENV should not affect it
      expect(mockGrpcTableService.queryTable).toHaveBeenCalledWith({
        serverUrl: 'https://localhost:3000',
        query: {
          limit: 10,
          offset: 0,
          connectionId: 'conn-123',
          sourceId: 'source-456',
        },
      });

      // Clean up
      delete process.env.NODE_ENV;
    });
  });
});
