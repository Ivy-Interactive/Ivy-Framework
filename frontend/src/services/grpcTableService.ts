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

      // Create gRPC message with proper header
      const grpcMessage = this.createGrpcMessage(serializedQuery);

      const requestUrl = `${serverUrl}/datatable.TableService/Query`;
      console.log('gRPC Table Service - Request URL:', requestUrl);

      // Make the gRPC-Web request
      const response = await fetch(requestUrl, {
        method: 'POST',
        headers: grpcHeaders,
        body: grpcMessage,
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

  // Create gRPC message with proper header
  private createGrpcMessage(data: Uint8Array): Uint8Array {
    // gRPC message format: [compression-flag][message-length][message-data]
    const compressionFlag = 0; // 0 = no compression
    const messageLength = data.length;

    // Create header: 1 byte for compression flag + 4 bytes for message length
    const header = new Uint8Array(5);
    header[0] = compressionFlag; // Compression flag (0 = uncompressed)

    // Write message length as big-endian 32-bit integer
    header[1] = (messageLength >>> 24) & 0xff;
    header[2] = (messageLength >>> 16) & 0xff;
    header[3] = (messageLength >>> 8) & 0xff;
    header[4] = messageLength & 0xff;

    // Combine header and data
    const result = new Uint8Array(5 + messageLength);
    result.set(header, 0);
    result.set(data, 5);

    return result;
  }

  // Serialize TableQuery to protobuf format
  private serializeTableQuery(query: TableQuery): Uint8Array {
    // This is a simplified protobuf serialization
    // In production, you should use the generated protobuf classes
    const encoder = new TextEncoder();
    const chunks: Uint8Array[] = [];

    // Serialize sort orders (field 1, repeated message)
    if (query.sort && query.sort.length > 0) {
      query.sort.forEach(sort => {
        const sortMessage = this.serializeSortOrder(sort);
        chunks.push(this.encodeField(1, 2, sortMessage)); // Field 1, wire type 2 (length-delimited)
      });
    }

    // Serialize filter (field 2, message)
    if (query.filter) {
      const filterMessage = this.serializeFilter(query.filter);
      chunks.push(this.encodeField(2, 2, filterMessage)); // Field 2, wire type 2
    }

    // Serialize offset (field 3, int32)
    if (query.offset !== undefined) {
      chunks.push(this.encodeField(3, 0, this.encodeVarint(query.offset))); // Field 3, wire type 0
    }

    // Serialize limit (field 4, int32)
    if (query.limit !== undefined) {
      chunks.push(this.encodeField(4, 0, this.encodeVarint(query.limit))); // Field 4, wire type 0
    }

    // Serialize select_columns (field 5, repeated string)
    if (query.select_columns && query.select_columns.length > 0) {
      query.select_columns.forEach(column => {
        const columnData = encoder.encode(column);
        chunks.push(this.encodeField(5, 2, columnData)); // Field 5, wire type 2
      });
    }

    // Serialize aggregations (field 6, repeated message)
    if (query.aggregations && query.aggregations.length > 0) {
      query.aggregations.forEach(agg => {
        const aggMessage = this.serializeAggregation(agg);
        chunks.push(this.encodeField(6, 2, aggMessage)); // Field 6, wire type 2
      });
    }

    // Serialize connectionId (field 7, string)
    if (query.connectionId) {
      const connData = encoder.encode(query.connectionId);
      chunks.push(this.encodeField(7, 2, connData)); // Field 7, wire type 2
    }

    // Serialize sourceId (field 8, string)
    if (query.sourceId) {
      const sourceData = encoder.encode(query.sourceId);
      chunks.push(this.encodeField(8, 2, sourceData)); // Field 8, wire type 2
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

  // Encode a protobuf field with proper wire type
  private encodeField(
    fieldNumber: number,
    wireType: number,
    data: Uint8Array
  ): Uint8Array {
    const tag = (fieldNumber << 3) | wireType;
    const tagBytes = this.encodeVarint(tag);
    const lengthBytes =
      wireType === 2 ? this.encodeVarint(data.length) : new Uint8Array(0);

    const result = new Uint8Array(
      tagBytes.length + lengthBytes.length + data.length
    );
    result.set(tagBytes, 0);
    result.set(lengthBytes, tagBytes.length);
    result.set(data, tagBytes.length + lengthBytes.length);

    return result;
  }

  // Serialize SortOrder message
  private serializeSortOrder(sort: SortOrder): Uint8Array {
    const encoder = new TextEncoder();
    const chunks: Uint8Array[] = [];

    // Field 1: column (string)
    const columnData = encoder.encode(sort.column);
    chunks.push(this.encodeField(1, 2, columnData));

    // Field 2: direction (enum)
    const directionValue = sort.direction === 'ASC' ? 0 : 1;
    chunks.push(this.encodeField(2, 0, this.encodeVarint(directionValue)));

    return this.combineChunks(chunks);
  }

  // Serialize Aggregation message
  private serializeAggregation(agg: Aggregation): Uint8Array {
    const encoder = new TextEncoder();
    const chunks: Uint8Array[] = [];

    // Field 1: column (string)
    const columnData = encoder.encode(agg.column);
    chunks.push(this.encodeField(1, 2, columnData));

    // Field 2: function (string)
    const functionData = encoder.encode(agg.function);
    chunks.push(this.encodeField(2, 2, functionData));

    return this.combineChunks(chunks);
  }

  // Combine multiple Uint8Array chunks into one
  private combineChunks(chunks: Uint8Array[]): Uint8Array {
    const totalLength = chunks.reduce((sum, chunk) => sum + chunk.length, 0);
    const result = new Uint8Array(totalLength);
    let offset = 0;
    for (const chunk of chunks) {
      result.set(chunk, offset);
      offset += chunk.length;
    }
    return result;
  }

  private serializeFilter(filter: Filter): Uint8Array {
    const chunks: Uint8Array[] = [];

    if (filter.condition) {
      // Field 1: condition (message)
      const conditionMessage = this.serializeCondition(filter.condition);
      chunks.push(this.encodeField(1, 2, conditionMessage));
    }

    if (filter.group) {
      // Field 2: group (message)
      const groupMessage = this.serializeFilterGroup(filter.group);
      chunks.push(this.encodeField(2, 2, groupMessage));
    }

    if (filter.negate !== undefined) {
      // Field 3: negate (bool)
      const negateValue = filter.negate ? 1 : 0;
      chunks.push(this.encodeField(3, 0, this.encodeVarint(negateValue)));
    }

    return this.combineChunks(chunks);
  }

  private serializeCondition(condition: Condition): Uint8Array {
    const encoder = new TextEncoder();
    const chunks: Uint8Array[] = [];

    // Field 1: column (string)
    const columnData = encoder.encode(condition.column);
    chunks.push(this.encodeField(1, 2, columnData));

    // Field 2: function (string)
    const functionData = encoder.encode(condition.function);
    chunks.push(this.encodeField(2, 2, functionData));

    // Field 3: args (repeated Any) - simplified as JSON for now
    if (condition.args && condition.args.length > 0) {
      condition.args.forEach(arg => {
        const argData = encoder.encode(JSON.stringify(arg));
        chunks.push(this.encodeField(3, 2, argData));
      });
    }

    return this.combineChunks(chunks);
  }

  private serializeFilterGroup(group: FilterGroup): Uint8Array {
    const chunks: Uint8Array[] = [];

    // Field 1: op (enum)
    const opValue = group.op === 'AND' ? 0 : 1;
    chunks.push(this.encodeField(1, 0, this.encodeVarint(opValue)));

    // Field 2: filters (repeated Filter)
    group.filters.forEach(filter => {
      const filterMessage = this.serializeFilter(filter);
      chunks.push(this.encodeField(2, 2, filterMessage));
    });

    return this.combineChunks(chunks);
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

    // Parse gRPC message to extract the protobuf data
    const arrowData = this.parseGrpcMessage(uint8Array);

    console.log(
      'gRPC Table Service - Extracted Arrow data size:',
      arrowData.length
    );

    // Parse the Arrow IPC stream
    let table: arrow.Table | undefined;
    try {
      table = arrow.tableFromIPC(arrowData);
      console.log('gRPC Table Service - Successfully parsed Arrow table:', {
        numRows: table.numRows,
        numCols: table.numCols,
        schema: table.schema.fields.map(f => f.name),
      });
    } catch (error) {
      console.warn('Failed to parse Arrow IPC stream:', error);
    }

    return {
      arrow_ipc_stream: arrowData,
      table,
    };
  }

  // Parse gRPC message format: [compression-flag][message-length][message-data]
  private parseGrpcMessage(data: Uint8Array): Uint8Array {
    if (data.length < 5) {
      throw new Error('Invalid gRPC message: too short');
    }

    // Read compression flag (1 byte)
    const compressionFlag = data[0];
    console.log('gRPC Table Service - Compression flag:', compressionFlag);

    // Read message length (4 bytes, big-endian)
    const messageLength =
      (data[1] << 24) | (data[2] << 16) | (data[3] << 8) | data[4];
    console.log('gRPC Table Service - Message length:', messageLength);

    // Extract the message data
    const messageData = data.slice(5, 5 + messageLength);
    console.log('gRPC Table Service - Message data size:', messageData.length);

    // Parse the protobuf message to extract the Arrow data
    return this.parseTableResultProtobuf(messageData);
  }

  // Parse TableResult protobuf message to extract arrow_ipc_stream field
  private parseTableResultProtobuf(data: Uint8Array): Uint8Array {
    let offset = 0;

    while (offset < data.length) {
      // Read field tag
      const tag = this.decodeVarint(data, offset);
      offset += this.getVarintLength(tag);

      const fieldNumber = tag >> 3;
      const wireType = tag & 0x07;

      console.log(
        `gRPC Table Service - Field ${fieldNumber}, wire type ${wireType}`
      );

      if (fieldNumber === 1 && wireType === 2) {
        // arrow_ipc_stream field
        // Read length-delimited data
        const length = this.decodeVarint(data, offset);
        offset += this.getVarintLength(length);

        // Extract the Arrow data
        const arrowData = data.slice(offset, offset + length);
        console.log(
          'gRPC Table Service - Extracted Arrow IPC stream size:',
          arrowData.length
        );
        return arrowData;
      } else if (wireType === 0) {
        // varint
        offset += this.getVarintLength(this.decodeVarint(data, offset));
      } else if (wireType === 2) {
        // length-delimited
        const length = this.decodeVarint(data, offset);
        offset += this.getVarintLength(length);
        offset += length;
      } else {
        throw new Error(`Unsupported wire type: ${wireType}`);
      }
    }

    throw new Error('Arrow IPC stream field not found in protobuf message');
  }

  private decodeVarint(data: Uint8Array, offset: number): number {
    let result = 0;
    let shift = 0;

    while (offset < data.length) {
      const byte = data[offset];
      result |= (byte & 0x7f) << shift;
      offset++;

      if ((byte & 0x80) === 0) {
        break;
      }

      shift += 7;
    }

    return result;
  }

  private getVarintLength(value: number): number {
    let length = 1;
    while (value >= 0x80) {
      value >>>= 7;
      length++;
    }
    return length;
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
