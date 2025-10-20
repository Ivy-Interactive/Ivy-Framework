import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import * as arrow from 'apache-arrow';
import { logger } from '../lib/logger';
import {
  GrpcTableService,
  grpcTableService,
  type TableQuery,
  type SortOrder,
  type Filter,
  type Condition,
  type Aggregation,
} from './grpcTableService';

// Mock dependencies
vi.mock('apache-arrow');
vi.mock('../lib/logger');

const mockArrow = vi.mocked(arrow);
const mockLogger = vi.mocked(logger);

// Mock fetch globally
const mockFetch = vi.fn();
global.fetch = mockFetch;

// Type definitions for accessing private methods in tests
interface GrpcTableServicePrivate {
  createGrpcMessage(data: Uint8Array): Uint8Array;
  serializeDataTableQuery(query: TableQuery): Uint8Array;
  encodeVarint(value: number): Uint8Array;
  decodeVarint(data: Uint8Array, offset: number): number;
  getVarintLength(value: number): number;
  serializeSortOrder(sort: SortOrder): Uint8Array;
  serializeCondition(condition: Condition): Uint8Array;
  serializeFilter(filter: Filter): Uint8Array;
  serializeAggregation(aggregation: Aggregation): Uint8Array;
  parseGrpcMessage(data: Uint8Array): Uint8Array;
  parseTableResultProtobuf(data: Uint8Array): Uint8Array;
  parseGrpcResponse(
    response: Response
  ): Promise<{ arrow_ipc_stream: Uint8Array; table?: arrow.Table }>;
  combineChunks(chunks: Uint8Array[]): Uint8Array;
}

describe('GrpcTableService', () => {
  let service: GrpcTableService;

  beforeEach(() => {
    vi.clearAllMocks();
    service = new GrpcTableService('http://localhost:8080');
  });

  afterEach(() => {
    service.disconnect();
  });

  describe('constructor', () => {
    it('should create service with server URL', () => {
      const testService = new GrpcTableService('http://test.com:9000');
      expect(testService).toBeInstanceOf(GrpcTableService);
    });
  });

  describe('queryTable', () => {
    const mockQuery: TableQuery = {
      limit: 10,
      offset: 0,
      connectionId: 'test-conn',
      sourceId: 'test-source',
    };

    const mockOptions = {
      serverUrl: 'http://localhost:8080',
      query: mockQuery,
    };

    it('should successfully query table and return result', async () => {
      const mockArrowData = new Uint8Array([1, 2, 3, 4]);
      const mockTable = {
        numRows: 2,
        numCols: 3,
        schema: { fields: [{ name: 'id' }, { name: 'name' }] },
      } as arrow.Table;

      // Mock successful response
      const mockResponse = {
        ok: true,
        status: 200,
        headers: new Map([['content-type', 'application/grpc-web+proto']]),
        arrayBuffer: vi.fn().mockResolvedValue(new ArrayBuffer(20)),
      } as unknown as Response;

      mockFetch.mockResolvedValue(mockResponse);
      mockArrow.tableFromIPC.mockReturnValue(mockTable);

      // Mock the private methods by spying on the service
      const parseGrpcResponseSpy = vi
        .spyOn(
          service as unknown as GrpcTableServicePrivate,
          'parseGrpcResponse'
        )
        .mockResolvedValue({
          arrow_ipc_stream: mockArrowData,
          table: mockTable,
        });

      const result = await service.queryTable(mockOptions);

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:8080/datatable.DataTableService/Query',
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/grpc-web+proto',
            Accept: 'application/grpc-web+proto',
            'X-Grpc-Web': '1',
          },
          body: expect.any(Uint8Array),
        }
      );

      expect(result).toEqual({
        arrow_ipc_stream: mockArrowData,
        table: mockTable,
      });

      expect(parseGrpcResponseSpy).toHaveBeenCalledWith(mockResponse);
    });

    it('should handle HTTP error responses', async () => {
      const mockResponse = {
        ok: false,
        status: 500,
        statusText: 'Internal Server Error',
        headers: new Map(),
        text: vi.fn().mockResolvedValue('Server error details'),
      } as unknown as Response;

      mockFetch.mockResolvedValue(mockResponse);

      await expect(service.queryTable(mockOptions)).rejects.toThrow(
        'gRPC Error: 500 Internal Server Error - Server error details'
      );

      expect(mockLogger.error).toHaveBeenCalledWith(
        'gRPC Table Service - Error response:',
        'Server error details'
      );
    });

    it('should handle network errors', async () => {
      const networkError = new Error('Network connection failed');
      mockFetch.mockRejectedValue(networkError);

      await expect(service.queryTable(mockOptions)).rejects.toThrow(
        networkError
      );
    });

    it('should call onData, onError, and onComplete callbacks', async () => {
      const onData = vi.fn();
      const onError = vi.fn();
      const onComplete = vi.fn();

      const mockResult = {
        arrow_ipc_stream: new Uint8Array([1, 2, 3]),
        table: {} as arrow.Table,
      };

      const mockResponse = {
        ok: true,
        status: 200,
        headers: new Map([['content-type', 'application/grpc-web+proto']]),
        arrayBuffer: vi.fn().mockResolvedValue(new ArrayBuffer(20)),
      } as unknown as Response;

      mockFetch.mockResolvedValue(mockResponse);

      vi.spyOn(
        service as unknown as GrpcTableServicePrivate,
        'parseGrpcResponse'
      ).mockResolvedValue(mockResult);

      await service.queryTable({
        ...mockOptions,
        onData,
        onError,
        onComplete,
      });

      expect(onData).toHaveBeenCalledWith(mockResult);
      expect(onComplete).toHaveBeenCalled();
      expect(onError).not.toHaveBeenCalled();
    });

    it('should handle query with all optional parameters', async () => {
      const complexQuery: TableQuery = {
        limit: 20,
        offset: 10,
        connectionId: 'test-conn',
        sourceId: 'test-source',
        sort: [{ column: 'name', direction: 'ASC' }],
        filter: {
          condition: {
            column: 'status',
            function: 'equals',
            args: ['active'],
          },
        },
        select_columns: ['id', 'name', 'status'],
        aggregations: [{ column: 'count', function: 'sum' }],
      };

      const mockResponse = {
        ok: true,
        status: 200,
        headers: new Map([['content-type', 'application/grpc-web+proto']]),
        arrayBuffer: vi.fn().mockResolvedValue(new ArrayBuffer(20)),
      } as unknown as Response;

      mockFetch.mockResolvedValue(mockResponse);
      vi.spyOn(
        service as unknown as GrpcTableServicePrivate,
        'parseGrpcResponse'
      ).mockResolvedValue({
        arrow_ipc_stream: new Uint8Array([1, 2, 3]),
        table: {} as arrow.Table,
      });

      await service.queryTable({
        serverUrl: 'http://localhost:8080',
        query: complexQuery,
      });

      expect(mockFetch).toHaveBeenCalled();
    });
  });

  describe('createGrpcMessage', () => {
    it('should create proper gRPC message with header', () => {
      const testData = new Uint8Array([1, 2, 3, 4, 5]);
      const result = (
        service as unknown as GrpcTableServicePrivate
      ).createGrpcMessage(testData);

      // Check message structure: [compression-flag][4-byte-length][data]
      expect(result).toHaveLength(10); // 1 + 4 + 5
      expect(result[0]).toBe(0); // compression flag
      expect(result[1]).toBe(0); // length high byte
      expect(result[2]).toBe(0); // length
      expect(result[3]).toBe(0); // length
      expect(result[4]).toBe(5); // length low byte (5 bytes)
      expect(result.slice(5)).toEqual(testData); // actual data
    });

    it('should handle empty data', () => {
      const testData = new Uint8Array([]);
      const result = (
        service as unknown as GrpcTableServicePrivate
      ).createGrpcMessage(testData);

      expect(result).toHaveLength(5); // 1 + 4 + 0
      expect(result[0]).toBe(0); // compression flag
      expect(result[4]).toBe(0); // length = 0
    });
  });

  describe('encodeVarint', () => {
    it('should encode small numbers correctly', () => {
      const result = (
        service as unknown as GrpcTableServicePrivate
      ).encodeVarint(42);
      expect(result).toEqual(new Uint8Array([42]));
    });

    it('should encode large numbers with continuation bits', () => {
      const result = (
        service as unknown as GrpcTableServicePrivate
      ).encodeVarint(300);
      // 300 = 0x12C = 0b100101100
      // Should be encoded as [0xAC, 0x02] (172, 2)
      expect(result).toEqual(new Uint8Array([0xac, 0x02]));
    });

    it('should encode zero', () => {
      const result = (
        service as unknown as GrpcTableServicePrivate
      ).encodeVarint(0);
      expect(result).toEqual(new Uint8Array([0]));
    });
  });

  describe('decodeVarint', () => {
    it('should decode small numbers correctly', () => {
      const data = new Uint8Array([42, 100, 200]);
      const result = (
        service as unknown as GrpcTableServicePrivate
      ).decodeVarint(data, 0);
      expect(result).toBe(42);
    });

    it('should decode multi-byte numbers', () => {
      const data = new Uint8Array([0xac, 0x02]); // 300 encoded
      const result = (
        service as unknown as GrpcTableServicePrivate
      ).decodeVarint(data, 0);
      expect(result).toBe(300);
    });

    it('should decode from specific offset', () => {
      const data = new Uint8Array([1, 2, 42, 4, 5]);
      const result = (
        service as unknown as GrpcTableServicePrivate
      ).decodeVarint(data, 2);
      expect(result).toBe(42);
    });
  });

  describe('getVarintLength', () => {
    it('should return correct length for single byte numbers', () => {
      expect(
        (service as unknown as GrpcTableServicePrivate).getVarintLength(0)
      ).toBe(1);
      expect(
        (service as unknown as GrpcTableServicePrivate).getVarintLength(127)
      ).toBe(1);
    });

    it('should return correct length for multi-byte numbers', () => {
      expect(
        (service as unknown as GrpcTableServicePrivate).getVarintLength(128)
      ).toBe(2);
      expect(
        (service as unknown as GrpcTableServicePrivate).getVarintLength(300)
      ).toBe(2);
      expect(
        (service as unknown as GrpcTableServicePrivate).getVarintLength(16384)
      ).toBe(3);
    });
  });

  describe('serializeSortOrder', () => {
    it('should serialize ASC sort order', () => {
      const sortOrder: SortOrder = { column: 'name', direction: 'ASC' };
      const result = (
        service as unknown as GrpcTableServicePrivate
      ).serializeSortOrder(sortOrder);

      expect(result).toBeInstanceOf(Uint8Array);
      expect(result.length).toBeGreaterThan(0);
    });

    it('should serialize DESC sort order', () => {
      const sortOrder: SortOrder = { column: 'id', direction: 'DESC' };
      const result = (
        service as unknown as GrpcTableServicePrivate
      ).serializeSortOrder(sortOrder);

      expect(result).toBeInstanceOf(Uint8Array);
      expect(result.length).toBeGreaterThan(0);
    });
  });

  describe('serializeCondition', () => {
    it('should serialize condition with args', () => {
      const condition: Condition = {
        column: 'status',
        function: 'equals',
        args: ['active', 123],
      };

      const result = (
        service as unknown as GrpcTableServicePrivate
      ).serializeCondition(condition);

      expect(result).toBeInstanceOf(Uint8Array);
      expect(result.length).toBeGreaterThan(0);
    });

    it('should serialize condition without args', () => {
      const condition: Condition = {
        column: 'name',
        function: 'isNull',
        args: [],
      };

      const result = (
        service as unknown as GrpcTableServicePrivate
      ).serializeCondition(condition);

      expect(result).toBeInstanceOf(Uint8Array);
      expect(result.length).toBeGreaterThan(0);
    });
  });

  describe('serializeFilter', () => {
    it('should serialize filter with condition', () => {
      const filter: Filter = {
        condition: {
          column: 'status',
          function: 'equals',
          args: ['active'],
        },
      };

      const result = (
        service as unknown as GrpcTableServicePrivate
      ).serializeFilter(filter);

      expect(result).toBeInstanceOf(Uint8Array);
      expect(result.length).toBeGreaterThan(0);
    });

    it('should serialize filter with group', () => {
      const filter: Filter = {
        group: {
          op: 'AND',
          filters: [
            {
              condition: {
                column: 'status',
                function: 'equals',
                args: ['active'],
              },
            },
            {
              condition: {
                column: 'type',
                function: 'equals',
                args: ['user'],
              },
            },
          ],
        },
      };

      const result = (
        service as unknown as GrpcTableServicePrivate
      ).serializeFilter(filter);

      expect(result).toBeInstanceOf(Uint8Array);
      expect(result.length).toBeGreaterThan(0);
    });

    it('should serialize filter with negate flag', () => {
      const filter: Filter = {
        condition: {
          column: 'deleted',
          function: 'equals',
          args: [true],
        },
        negate: true,
      };

      const result = (
        service as unknown as GrpcTableServicePrivate
      ).serializeFilter(filter);

      expect(result).toBeInstanceOf(Uint8Array);
      expect(result.length).toBeGreaterThan(0);
    });
  });

  describe('serializeAggregation', () => {
    it('should serialize aggregation', () => {
      const aggregation: Aggregation = {
        column: 'price',
        function: 'sum',
      };

      const result = (
        service as unknown as GrpcTableServicePrivate
      ).serializeAggregation(aggregation);

      expect(result).toBeInstanceOf(Uint8Array);
      expect(result.length).toBeGreaterThan(0);
    });
  });

  describe('serializeTableQuery', () => {
    it('should serialize minimal query', () => {
      const query: TableQuery = {
        limit: 10,
        offset: 0,
      };

      const result = (
        service as unknown as GrpcTableServicePrivate
      ).serializeDataTableQuery(query);

      expect(result).toBeInstanceOf(Uint8Array);
      expect(result.length).toBeGreaterThan(0);
    });

    it('should serialize complete query', () => {
      const query: TableQuery = {
        limit: 20,
        offset: 10,
        connectionId: 'test-conn',
        sourceId: 'test-source',
        sort: [
          { column: 'name', direction: 'ASC' },
          { column: 'created_at', direction: 'DESC' },
        ],
        filter: {
          condition: {
            column: 'status',
            function: 'in',
            args: ['active', 'pending'],
          },
        },
        select_columns: ['id', 'name', 'status'],
        aggregations: [
          { column: 'price', function: 'sum' },
          { column: 'id', function: 'count' },
        ],
      };

      const result = (
        service as unknown as GrpcTableServicePrivate
      ).serializeDataTableQuery(query);

      expect(result).toBeInstanceOf(Uint8Array);
      expect(result.length).toBeGreaterThan(0);
    });
  });

  describe('parseGrpcMessage', () => {
    it('should parse valid gRPC message', () => {
      // Create a mock gRPC message with header
      const messageData = new Uint8Array([10, 15, 72, 101, 108, 108, 111]); // Mock protobuf data
      const header = new Uint8Array([0, 0, 0, 0, 7]); // No compression, 7 bytes length
      const fullMessage = new Uint8Array(header.length + messageData.length);
      fullMessage.set(header);
      fullMessage.set(messageData, header.length);

      // Mock parseTableResultProtobuf to return arrow data
      const mockArrowData = new Uint8Array([1, 2, 3, 4]);
      vi.spyOn(
        service as unknown as GrpcTableServicePrivate,
        'parseTableResultProtobuf'
      ).mockReturnValue(mockArrowData);

      const result = (
        service as unknown as GrpcTableServicePrivate
      ).parseGrpcMessage(fullMessage);

      expect(result).toEqual(mockArrowData);
    });

    it('should throw error for invalid message length', () => {
      const invalidMessage = new Uint8Array([1, 2]); // Too short (< 5 bytes)

      expect(() => {
        (service as unknown as GrpcTableServicePrivate).parseGrpcMessage(
          invalidMessage
        );
      }).toThrow('Invalid gRPC message: too short');
    });
  });

  describe('parseTableResultProtobuf', () => {
    it('should parse arrow_ipc_stream field', () => {
      // Create mock protobuf data with field 1 (arrow_ipc_stream)
      const arrowData = new Uint8Array([1, 2, 3, 4, 5]);
      // Field 1, wire type 2, length 5, then data
      const protobufData = new Uint8Array([
        0x0a, // Field 1, wire type 2 (00001 010)
        0x05, // Length = 5
        ...arrowData,
      ]);

      const result = (
        service as unknown as GrpcTableServicePrivate
      ).parseTableResultProtobuf(protobufData);

      expect(result).toEqual(arrowData);
    });

    it('should throw error when arrow_ipc_stream field not found', () => {
      // Create protobuf data without field 1
      const protobufData = new Uint8Array([
        0x10, // Field 2, wire type 0
        0x42, // Some value
      ]);

      expect(() => {
        (
          service as unknown as GrpcTableServicePrivate
        ).parseTableResultProtobuf(protobufData);
      }).toThrow('Arrow IPC stream field not found in protobuf message');
    });

    it('should handle multiple fields and find arrow_ipc_stream', () => {
      const arrowData = new Uint8Array([10, 20, 30]);
      const protobufData = new Uint8Array([
        0x10,
        0x42, // Field 2, some value
        0x0a,
        0x03,
        ...arrowData, // Field 1 (arrow_ipc_stream), length 3, data
        0x18,
        0x64, // Field 3, some value
      ]);

      const result = (
        service as unknown as GrpcTableServicePrivate
      ).parseTableResultProtobuf(protobufData);

      expect(result).toEqual(arrowData);
    });
  });

  describe('parseGrpcResponse', () => {
    it('should parse successful response with Arrow table', async () => {
      const mockArrowData = new Uint8Array([1, 2, 3, 4]);
      const mockTable = {
        numRows: 2,
        numCols: 3,
        schema: { fields: [{ name: 'id' }, { name: 'name' }] },
      } as arrow.Table;

      const mockResponse = {
        arrayBuffer: vi
          .fn()
          .mockResolvedValue(new ArrayBuffer(mockArrowData.length)),
      } as unknown as Response;

      // Mock the parsing methods
      vi.spyOn(
        service as unknown as GrpcTableServicePrivate,
        'parseGrpcMessage'
      ).mockReturnValue(mockArrowData);
      mockArrow.tableFromIPC.mockReturnValue(mockTable);

      const result = await (
        service as unknown as GrpcTableServicePrivate
      ).parseGrpcResponse(mockResponse);

      expect(result).toEqual({
        arrow_ipc_stream: mockArrowData,
        table: mockTable,
        offset: 0,
        row_count: 0,
        total_rows: 0,
      });

      expect(mockArrow.tableFromIPC).toHaveBeenCalledWith(mockArrowData);
    });

    it('should handle Arrow parsing failure gracefully', async () => {
      const mockArrowData = new Uint8Array([1, 2, 3, 4]);
      const mockResponse = {
        arrayBuffer: vi
          .fn()
          .mockResolvedValue(new ArrayBuffer(mockArrowData.length)),
      } as unknown as Response;

      vi.spyOn(
        service as unknown as GrpcTableServicePrivate,
        'parseGrpcMessage'
      ).mockReturnValue(mockArrowData);
      mockArrow.tableFromIPC.mockImplementation(() => {
        throw new Error('Invalid Arrow data');
      });

      const result = await (
        service as unknown as GrpcTableServicePrivate
      ).parseGrpcResponse(mockResponse);

      expect(result).toEqual({
        arrow_ipc_stream: mockArrowData,
        table: undefined,
        offset: 0,
        row_count: 0,
        total_rows: 0,
      });

      expect(mockLogger.warn).toHaveBeenCalledWith(
        'Failed to parse Arrow IPC stream:',
        expect.any(Error)
      );
    });
  });

  describe('utility methods', () => {
    it('should report streaming status correctly', () => {
      expect(service.isStreaming()).toBe(false);
    });

    it('should disconnect properly', () => {
      service.disconnect();
      expect(service.isStreaming()).toBe(false);
    });

    it('should combine chunks correctly', () => {
      const chunk1 = new Uint8Array([1, 2, 3]);
      const chunk2 = new Uint8Array([4, 5]);
      const chunk3 = new Uint8Array([6, 7, 8, 9]);

      const result = (
        service as unknown as GrpcTableServicePrivate
      ).combineChunks([chunk1, chunk2, chunk3]);

      expect(result).toEqual(new Uint8Array([1, 2, 3, 4, 5, 6, 7, 8, 9]));
    });

    it('should handle empty chunks array', () => {
      const result = (
        service as unknown as GrpcTableServicePrivate
      ).combineChunks([]);
      expect(result).toEqual(new Uint8Array([]));
    });
  });

  describe('error handling', () => {
    it('should emit error events on failure', async () => {
      const errorSpy = vi.fn();
      service.on('error', errorSpy);

      const error = new Error('Test error');
      mockFetch.mockRejectedValue(error);

      await expect(
        service.queryTable({
          serverUrl: 'http://localhost:8080',
          query: { limit: 10 },
        })
      ).rejects.toThrow(error);

      expect(errorSpy).toHaveBeenCalledWith(error);
    });

    it('should emit data and complete events on success', async () => {
      const dataSpy = vi.fn();
      const completeSpy = vi.fn();

      service.on('data', dataSpy);
      service.on('complete', completeSpy);

      const mockResult = {
        arrow_ipc_stream: new Uint8Array([1, 2, 3]),
        table: {} as arrow.Table,
      };

      const mockResponse = {
        ok: true,
        status: 200,
        headers: new Map([['content-type', 'application/grpc-web+proto']]),
      } as unknown as Response;

      mockFetch.mockResolvedValue(mockResponse);
      vi.spyOn(
        service as unknown as GrpcTableServicePrivate,
        'parseGrpcResponse'
      ).mockResolvedValue(mockResult);

      await service.queryTable({
        serverUrl: 'http://localhost:8080',
        query: { limit: 10 },
      });

      expect(dataSpy).toHaveBeenCalledWith(mockResult);
      expect(completeSpy).toHaveBeenCalled();
    });
  });

  describe('singleton instance', () => {
    it('should export a singleton grpcTableService', () => {
      expect(grpcTableService).toBeInstanceOf(GrpcTableService);
    });
  });
});
