// Browser-compatible gRPC client for Apache Arrow table service
import * as arrow from 'apache-arrow';

// Browser-compatible EventEmitter implementation
class EventEmitter {
  private events: Record<string, ((...args: unknown[]) => void)[]> = {};

  on(event: string, listener: (...args: unknown[]) => void): void {
    if (!this.events[event]) {
      this.events[event] = [];
    }
    this.events[event].push(listener);
  }

  emit(event: string, ...args: unknown[]): void {
    if (this.events[event]) {
      this.events[event].forEach(listener => listener(...args));
    }
  }

  off(event: string, listener: (...args: unknown[]) => void): void {
    if (this.events[event]) {
      this.events[event] = this.events[event].filter(l => l !== listener);
    }
  }
}

// Type definitions matching the proto file exactly
export interface SortOrder {
  column: string;
  direction: 'ASC' | 'DESC';
}

export interface Condition {
  column: string;
  function: string; // e.g. "equals", "greaterThan", "inSet", "contains"
  args: unknown[];
}

export interface FilterGroup {
  op: 'AND' | 'OR';
  filters: Filter[];
}

export interface Filter {
  condition?: Condition;
  group?: FilterGroup;
  negate?: boolean;
}

export interface Aggregation {
  column: string;
  function: string; // e.g. "sum", "avg", "min", "max", "count"
}

export interface TableQuery {
  sort?: SortOrder[];
  filter?: Filter;
  offset?: number;
  limit?: number;
  select_columns?: string[]; // Match proto field name
  aggregations?: Aggregation[];
  connectionId?: string; // Match proto field name
  sourceId?: string; // Match proto field name
}

export interface TableResult {
  arrow_ipc_stream: Uint8Array; // Match proto field name
  table?: arrow.Table; // Parsed Arrow table
}

export interface GrpcTableStreamOptions {
  serverUrl: string;
  query: TableQuery;
  onData?: (data: TableResult) => void;
  onError?: (error: Error) => void;
  onComplete?: () => void;
}

export class GrpcTableService extends EventEmitter {
  private serverUrl: string;
  private isConnected = false;

  constructor(serverUrl: string) {
    super();
    this.serverUrl = serverUrl;
  }

  async queryTable(options: GrpcTableStreamOptions): Promise<TableResult> {
    const { serverUrl, query, onData, onError, onComplete } = options;

    try {
      this.isConnected = true;

      // Debug: Log the server URL being used
      console.log('gRPC Table Service - Connecting to:', serverUrl);
      console.log('gRPC Table Service - Query:', query);

      // Create gRPC-Web request with proper headers
      const grpcHeaders = {
        'Content-Type': 'application/grpc-web+proto',
        Accept: 'application/grpc-web+proto',
        'X-Grpc-Web': '1',
      };

      // Serialize the query to protobuf format
      const serializedQuery = this.serializeTableQuery(query);

      const requestUrl = `${serverUrl}/datatable.TableService/Query`;
      console.log('gRPC Table Service - Request URL:', requestUrl);

      // Make the gRPC-Web request
      const response = await fetch(requestUrl, {
        method: 'POST',
        headers: grpcHeaders,
        body: serializedQuery,
      });

      console.log('gRPC Table Service - Response status:', response.status);
      console.log(
        'gRPC Table Service - Response headers:',
        Object.fromEntries(response.headers.entries())
      );

      if (!response.ok) {
        const errorText = await response.text();
        console.error('gRPC Table Service - Error response:', errorText);
        throw new Error(
          `gRPC Error: ${response.status} ${response.statusText} - ${errorText}`
        );
      }

      // Parse the gRPC-Web response
      const result = await this.parseGrpcResponse(response);

      if (onData) {
        onData(result);
      }

      this.emit('data', result);

      if (onComplete) {
        onComplete();
      }

      this.emit('complete');

      return result;
    } catch (error) {
      const errorObj =
        error instanceof Error ? error : new Error('Query failed');

      if (onError) {
        onError(errorObj);
      }

      this.emit('error', errorObj);
      throw errorObj;
    } finally {
      this.isConnected = false;
    }
  }

  // Serialize TableQuery to protobuf format
  private serializeTableQuery(query: TableQuery): Uint8Array {
    // This is a simplified protobuf serialization
    // In production, you should use the generated protobuf classes
    const encoder = new TextEncoder();

    // Create a simple binary format that mimics protobuf
    const chunks: Uint8Array[] = [];

    // Serialize sort orders
    if (query.sort && query.sort.length > 0) {
      query.sort.forEach((sort, index) => {
        const sortData = encoder.encode(
          JSON.stringify({
            column: sort.column,
            direction: sort.direction === 'ASC' ? 0 : 1,
          })
        );
        chunks.push(new Uint8Array([1, index + 1])); // Field 1, wire type 2 (length-delimited)
        chunks.push(this.encodeVarint(sortData.length));
        chunks.push(sortData);
      });
    }

    // Serialize filter
    if (query.filter) {
      const filterData = encoder.encode(
        JSON.stringify(this.serializeFilter(query.filter))
      );
      chunks.push(new Uint8Array([2])); // Field 2
      chunks.push(this.encodeVarint(filterData.length));
      chunks.push(filterData);
    }

    // Serialize offset
    if (query.offset !== undefined) {
      chunks.push(new Uint8Array([3])); // Field 3
      chunks.push(this.encodeVarint(query.offset));
    }

    // Serialize limit
    if (query.limit !== undefined) {
      chunks.push(new Uint8Array([4])); // Field 4
      chunks.push(this.encodeVarint(query.limit));
    }

    // Serialize select_columns
    if (query.select_columns && query.select_columns.length > 0) {
      query.select_columns.forEach((column, index) => {
        const columnData = encoder.encode(column);
        chunks.push(new Uint8Array([5, index + 1])); // Field 5
        chunks.push(this.encodeVarint(columnData.length));
        chunks.push(columnData);
      });
    }

    // Serialize aggregations
    if (query.aggregations && query.aggregations.length > 0) {
      query.aggregations.forEach((agg, index) => {
        const aggData = encoder.encode(
          JSON.stringify({
            column: agg.column,
            function: agg.function,
          })
        );
        chunks.push(new Uint8Array([6, index + 1])); // Field 6
        chunks.push(this.encodeVarint(aggData.length));
        chunks.push(aggData);
      });
    }

    // Serialize connectionId
    if (query.connectionId) {
      const connData = encoder.encode(query.connectionId);
      chunks.push(new Uint8Array([7])); // Field 7
      chunks.push(this.encodeVarint(connData.length));
      chunks.push(connData);
    }

    // Serialize sourceId
    if (query.sourceId) {
      const sourceData = encoder.encode(query.sourceId);
      chunks.push(new Uint8Array([8])); // Field 8
      chunks.push(this.encodeVarint(sourceData.length));
      chunks.push(sourceData);
    }

    // Combine all chunks
    const totalLength = chunks.reduce((sum, chunk) => sum + chunk.length, 0);
    const result = new Uint8Array(totalLength);
    let offset = 0;
    for (const chunk of chunks) {
      result.set(chunk, offset);
      offset += chunk.length;
    }

    return result;
  }

  private serializeFilter(filter: Filter): Record<string, unknown> {
    if (filter.condition) {
      return {
        condition: {
          column: filter.condition.column,
          function: filter.condition.function,
          args: filter.condition.args,
        },
        negate: filter.negate,
      };
    }

    if (filter.group) {
      return {
        group: {
          op: filter.group.op === 'AND' ? 0 : 1,
          filters: filter.group.filters.map(f => this.serializeFilter(f)),
        },
        negate: filter.negate,
      };
    }

    return {};
  }

  private encodeVarint(value: number): Uint8Array {
    const bytes: number[] = [];
    while (value >= 0x80) {
      bytes.push((value & 0x7f) | 0x80);
      value >>>= 7;
    }
    bytes.push(value & 0x7f);
    return new Uint8Array(bytes);
  }

  // Parse gRPC-Web response
  private async parseGrpcResponse(response: Response): Promise<TableResult> {
    const buffer = await response.arrayBuffer();
    const uint8Array = new Uint8Array(buffer);

    console.log(
      'gRPC Table Service - Response buffer size:',
      buffer.byteLength
    );
    console.log(
      'gRPC Table Service - Response data (first 100 bytes):',
      Array.from(uint8Array.slice(0, 100))
        .map(b => b.toString(16).padStart(2, '0'))
        .join(' ')
    );

    // Parse the Arrow IPC stream
    let table: arrow.Table | undefined;
    try {
      table = arrow.tableFromIPC(uint8Array);
      console.log('gRPC Table Service - Successfully parsed Arrow table:', {
        numRows: table.numRows,
        numCols: table.numCols,
        schema: table.schema.fields.map(f => f.name),
      });
    } catch (error) {
      console.warn('Failed to parse Arrow IPC stream:', error);
    }

    return {
      arrow_ipc_stream: uint8Array,
      table,
    };
  }

  isStreaming(): boolean {
    return this.isConnected;
  }

  disconnect(): void {
    this.isConnected = false;
  }
}

// Create a singleton instance that will be configured dynamically
export const grpcTableService = new GrpcTableService('');
