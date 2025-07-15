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

// Type definitions matching your proto file
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
  selectColumns?: string[];
  aggregations?: Aggregation[];
}

export interface TableResult {
  arrowIpcStream: Uint8Array;
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
    const { query, onData, onError, onComplete } = options;

    try {
      // For browser compatibility, we'll use HTTP POST with protobuf
      // You'll need to implement a gRPC-Web gateway or use a library like grpc-web
      const response = await fetch(
        `${this.serverUrl}/datatable.TableService/Query`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/grpc-web+proto',
            Accept: 'application/grpc-web+proto',
          },
          body: this.serializeTableQuery(query),
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      const result = await this.deserializeTableResult(response);

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
    }
  }

  // Serialize TableQuery to protobuf format
  private serializeTableQuery(query: TableQuery): Uint8Array {
    // This is a simplified implementation
    // In a real implementation, you would use the generated protobuf classes
    const queryObj = {
      sort: query.sort?.map(s => ({
        column: s.column,
        direction: s.direction === 'ASC' ? 0 : 1,
      })),
      filter: this.serializeFilter(query.filter),
      offset: query.offset,
      limit: query.limit,
      selectColumns: query.selectColumns,
      aggregations: query.aggregations?.map(a => ({
        column: a.column,
        function: a.function,
      })),
    };

    // For now, we'll use JSON as a placeholder
    // In production, use proper protobuf serialization
    const jsonString = JSON.stringify(queryObj);
    return new TextEncoder().encode(jsonString);
  }

  private serializeFilter(
    filter?: Filter
  ): Record<string, unknown> | undefined {
    if (!filter) return undefined;

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

    return undefined;
  }

  // Deserialize TableResult from protobuf format
  private async deserializeTableResult(
    response: Response
  ): Promise<TableResult> {
    const buffer = await response.arrayBuffer();
    const uint8Array = new Uint8Array(buffer);

    // Parse the Arrow IPC stream
    let table: arrow.Table | undefined;
    try {
      table = arrow.tableFromIPC(uint8Array);
    } catch (error) {
      console.warn('Failed to parse Arrow IPC stream:', error);
    }

    return {
      arrowIpcStream: uint8Array,
      table,
    };
  }

  isStreaming(): boolean {
    return this.isConnected;
  }
}

// Create a singleton instance
export const grpcTableService = new GrpcTableService(
  process.env.NODE_ENV === 'development'
    ? 'http://localhost:50051'
    : window.location.origin
);
