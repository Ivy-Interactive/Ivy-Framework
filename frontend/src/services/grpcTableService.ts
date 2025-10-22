// Browser-compatible gRPC client for Apache Arrow table service
import * as arrow from 'apache-arrow';
import { logger } from '../lib/logger';

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

export interface DataTableQuery {
  sort?: SortOrder[];
  filter?: Filter;
  offset?: number;
  limit?: number;
  select_columns?: string[];
  aggregations?: Aggregation[];
  connectionId?: string;
  sourceId?: string;
}

// Alias for backward compatibility with tests
export type TableQuery = DataTableQuery;

export interface DataTableResult {
  arrow_ipc_stream: Uint8Array;
  offset: number;
  row_count: number;
  total_rows: number;
  table?: arrow.Table;
}

export interface DataTableDistinctQuery {
  column: string;
  search?: string;
  limit?: number;
  connectionId: string;
  sourceId: string;
}

export interface DataTableDistinctResult {
  values: string[];
}

export interface ParseFilterRequest {
  payload: string;
  connectionId?: string;
  sourceId?: string;
}

export interface ParseFilterResult {
  filterExpression: string;
}

export interface GrpcTableStreamOptions {
  serverUrl: string;
  query: DataTableQuery;
  onData?: (data: DataTableResult) => void;
  onError?: (error: Error) => void;
  onComplete?: () => void;
}

export class GrpcTableService extends EventEmitter {
  private serverUrl: string;
  private isConnected = false;

  constructor(serverUrl: string) {
    super();
    this.serverUrl = serverUrl;
    logger.debug(
      'GrpcTableService constructor - Connecting to:',
      this.serverUrl
    );
  }

  async parseFilter(
    request: ParseFilterRequest,
    serverUrl: string
  ): Promise<ParseFilterResult> {
    try {
      logger.debug('parseFilter - Request:', request);

      // Create gRPC-Web request headers
      const grpcHeaders = {
        'Content-Type': 'application/grpc-web+proto',
        Accept: 'application/grpc-web+proto',
        'X-Grpc-Web': '1',
      };

      // Serialize the request
      const serializedRequest = this.serializeParseFilterRequest(request);
      const grpcMessage = this.createGrpcMessage(serializedRequest);

      const requestUrl = `${serverUrl}/datatable.DataTableService/ParseFilter`;
      logger.debug('parseFilter - Request URL:', requestUrl);

      // Make the gRPC-Web request
      const response = await fetch(requestUrl, {
        method: 'POST',
        headers: grpcHeaders,
        body: grpcMessage as BodyInit,
      });

      logger.debug('parseFilter - Response status:', response.status);

      if (!response.ok) {
        const errorText = await response.text();
        logger.error('parseFilter - Error response:', errorText);
        throw new Error(
          `gRPC Error: ${response.status} ${response.statusText} - ${errorText}`
        );
      }
      // Parse the response
      const result = await this.parseFilterResponse(response);

      return result;
    } catch (error) {
      const errorObj =
        error instanceof Error ? error : new Error('ParseFilter failed');
      logger.error('parseFilter - Error:', errorObj);
      throw errorObj;
    }
  }

  async queryTable(options: GrpcTableStreamOptions): Promise<DataTableResult> {
    const { serverUrl, query, onData, onError, onComplete } = options;

    try {
      this.isConnected = true;

      // Debug: Log the server URL being used
      logger.debug('gRPC Table Service - Connecting to:', serverUrl);
      logger.debug('gRPC Table Service - Query:', query);

      // Create gRPC-Web request with proper headers
      const grpcHeaders = {
        'Content-Type': 'application/grpc-web+proto',
        Accept: 'application/grpc-web+proto',
        'X-Grpc-Web': '1',
      };

      // Serialize the query to protobuf format
      const serializedQuery = this.serializeDataTableQuery(query);

      // Create gRPC message with proper header
      const grpcMessage = this.createGrpcMessage(serializedQuery);

      const requestUrl = `${serverUrl}/datatable.DataTableService/Query`;
      logger.debug('gRPC Table Service - Request URL:', requestUrl);

      // Make the gRPC-Web request
      const response = await fetch(requestUrl, {
        method: 'POST',
        headers: grpcHeaders,
        body: grpcMessage as BodyInit,
      });

      logger.debug('gRPC Table Service - Response status:', response.status);
      logger.debug(
        'gRPC Table Service - Response headers:',
        Object.fromEntries(response.headers.entries())
      );

      if (!response.ok) {
        const errorText = await response.text();
        logger.error('gRPC Table Service - Error response:', errorText);
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

  // Serialize ParseFilterRequest to protobuf format
  private serializeParseFilterRequest(request: ParseFilterRequest): Uint8Array {
    logger.debug(
      'serializeParseFilterRequest: Starting serialization',
      request
    );

    const encoder = new TextEncoder();
    const chunks: Uint8Array[] = [];

    // Field 1: payload (serialized as JSON string for now)
    if (request.payload !== undefined) {
      const payloadJson = JSON.stringify(request.payload);
      const payloadData = encoder.encode(payloadJson);
      chunks.push(this.encodeField(1, 2, payloadData)); // Field 1, wire type 2 (string)
    }

    // Field 2: connectionId (string)
    if (request.connectionId) {
      const connData = encoder.encode(request.connectionId);
      chunks.push(this.encodeField(2, 2, connData)); // Field 2, wire type 2
    }

    // Field 3: sourceId (string)
    if (request.sourceId) {
      const sourceData = encoder.encode(request.sourceId);
      chunks.push(this.encodeField(3, 2, sourceData)); // Field 3, wire type 2
    }

    return this.combineChunks(chunks);
  }

  // Serialize DataTableQuery to protobuf format
  private serializeDataTableQuery(query: DataTableQuery): Uint8Array {
    logger.debug('serializeDataTableQuery: Starting serialization', query);

    // This is a simplified protobuf serialization
    // In production, you should use the generated protobuf classes
    const encoder = new TextEncoder();
    const chunks: Uint8Array[] = [];

    // Serialize sort orders (field 1, repeated message)
    if (query.sort && query.sort.length > 0) {
      logger.debug(
        'serializeDataTableQuery: Serializing sort orders',
        query.sort
      );
      query.sort.forEach(sort => {
        const sortMessage = this.serializeSortOrder(sort);
        chunks.push(this.encodeField(1, 2, sortMessage)); // Field 1, wire type 2 (length-delimited)
      });
    }

    // Serialize filter (field 2, message)
    if (query.filter) {
      logger.debug('serializeDataTableQuery: Serializing filter', query.filter);
      try {
        const filterMessage = this.serializeFilter(query.filter);
        logger.debug(
          'serializeDataTableQuery: Filter serialized successfully, length:',
          filterMessage.length
        );
        chunks.push(this.encodeField(2, 2, filterMessage)); // Field 2, wire type 2
      } catch (error) {
        logger.error(
          'serializeDataTableQuery: Filter serialization failed:',
          error
        );
        throw error;
      }
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
    logger.debug('serializeFilter: Serializing filter', filter);
    const chunks: Uint8Array[] = [];

    if (filter.condition) {
      logger.debug('serializeFilter: Serializing condition', filter.condition);
      // Field 1: condition (message)
      const conditionMessage = this.serializeCondition(filter.condition);
      logger.debug(
        'serializeFilter: Condition serialized, length:',
        conditionMessage.length
      );
      chunks.push(this.encodeField(1, 2, conditionMessage));
    }

    if (filter.group) {
      logger.debug('serializeFilter: Serializing group', filter.group);
      // Field 2: group (message)
      const groupMessage = this.serializeFilterGroup(filter.group);
      logger.debug(
        'serializeFilter: Group serialized, length:',
        groupMessage.length
      );
      chunks.push(this.encodeField(2, 2, groupMessage));
    }

    if (filter.negate !== undefined) {
      logger.debug('serializeFilter: Serializing negate', filter.negate);
      // Field 3: negate (bool)
      const negateValue = filter.negate ? 1 : 0;
      chunks.push(this.encodeField(3, 0, this.encodeVarint(negateValue)));
    }

    const result = this.combineChunks(chunks);
    logger.debug(
      'serializeFilter: Filter serialization complete, total length:',
      result.length
    );
    return result;
  }

  private serializeCondition(condition: Condition): Uint8Array {
    logger.debug('serializeCondition: Serializing condition', condition);
    const encoder = new TextEncoder();
    const chunks: Uint8Array[] = [];

    // Field 1: column (string)
    const columnData = encoder.encode(condition.column);
    logger.debug('serializeCondition: Column data length:', columnData.length);
    chunks.push(this.encodeField(1, 2, columnData));

    // Field 2: function (string)
    const functionData = encoder.encode(condition.function);
    logger.debug(
      'serializeCondition: Function data length:',
      functionData.length
    );
    chunks.push(this.encodeField(2, 2, functionData));

    // Field 3: args (repeated Any) - need to encode as proper protobuf Any messages
    if (condition.args && condition.args.length > 0) {
      logger.debug('serializeCondition: Serializing args', condition.args);
      condition.args.forEach((arg, index) => {
        const jsonArg = JSON.stringify(arg);
        logger.debug(`serializeCondition: Arg ${index}: ${jsonArg}`);

        // Create a proper protobuf Any message
        const anyMessage = this.serializeAnyValue(arg);
        logger.debug(
          `serializeCondition: Arg ${index} Any message length:`,
          anyMessage.length
        );
        chunks.push(this.encodeField(3, 2, anyMessage));
      });
    }

    const result = this.combineChunks(chunks);
    logger.debug(
      'serializeCondition: Condition serialization complete, total length:',
      result.length
    );
    return result;
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

  // Serialize a value as a protobuf Any message
  private serializeAnyValue(value: unknown): Uint8Array {
    logger.debug(
      'serializeAnyValue: Serializing value',
      value,
      'type:',
      typeof value
    );

    const encoder = new TextEncoder();
    const chunks: Uint8Array[] = [];

    let typeUrl: string;
    let valueBytes: Uint8Array;

    // Determine the appropriate protobuf type based on JavaScript type
    if (typeof value === 'number') {
      if (Number.isInteger(value)) {
        // Integer value
        typeUrl = 'type.googleapis.com/google.protobuf.Int64Value';
        const valueChunks: Uint8Array[] = [];
        // Field 1 in Int64Value is the value itself (varint for small numbers)
        valueChunks.push(this.encodeField(1, 0, this.encodeVarint(value)));
        valueBytes = this.combineChunks(valueChunks);
      } else {
        // Double value
        typeUrl = 'type.googleapis.com/google.protobuf.DoubleValue';
        const valueChunks: Uint8Array[] = [];
        // Field 1 in DoubleValue is the value itself (fixed64)
        const buffer = new ArrayBuffer(8);
        const view = new DataView(buffer);
        view.setFloat64(0, value, true); // little-endian
        const doubleBytes = new Uint8Array(buffer);
        valueChunks.push(this.encodeField(1, 1, doubleBytes));
        valueBytes = this.combineChunks(valueChunks);
      }
    } else if (typeof value === 'boolean') {
      // Boolean value
      typeUrl = 'type.googleapis.com/google.protobuf.BoolValue';
      const valueChunks: Uint8Array[] = [];
      // Field 1 in BoolValue is the value itself (varint: 0 or 1)
      valueChunks.push(
        this.encodeField(1, 0, this.encodeVarint(value ? 1 : 0))
      );
      valueBytes = this.combineChunks(valueChunks);
    } else if (value === null || value === undefined) {
      // Null value - use empty message
      typeUrl = 'type.googleapis.com/google.protobuf.Value';
      valueBytes = new Uint8Array(0);
    } else if (Array.isArray(value)) {
      // Array value - serialize as ListValue
      typeUrl = 'type.googleapis.com/google.protobuf.ListValue';
      const listChunks: Uint8Array[] = [];
      // Field 1 in ListValue is repeated Value
      value.forEach(item => {
        const itemAny = this.serializeAnyValue(item);
        listChunks.push(this.encodeField(1, 2, itemAny));
      });
      valueBytes = this.combineChunks(listChunks);
    } else {
      // String or other value - serialize as StringValue
      typeUrl = 'type.googleapis.com/google.protobuf.StringValue';
      const valueChunks: Uint8Array[] = [];
      // Field 1 in StringValue is the value itself (string)
      const stringData = encoder.encode(String(value));
      valueChunks.push(this.encodeField(1, 2, stringData));
      valueBytes = this.combineChunks(valueChunks);
    }

    // Field 1: type_url (string)
    const typeUrlData = encoder.encode(typeUrl);
    chunks.push(this.encodeField(1, 2, typeUrlData));

    // Field 2: value (bytes)
    chunks.push(this.encodeField(2, 2, valueBytes));

    const result = this.combineChunks(chunks);
    logger.debug(
      'serializeAnyValue: Any value serialized as',
      typeUrl,
      'length:',
      result.length
    );
    return result;
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

  // Parse ParseFilter response
  private async parseFilterResponse(
    response: Response
  ): Promise<ParseFilterResult> {
    const buffer = await response.arrayBuffer();
    const uint8Array = new Uint8Array(buffer);

    logger.debug('parseParseFilterResponse - Buffer size:', buffer.byteLength);

    // Extract protobuf message from gRPC wrapper
    if (uint8Array.length < 5) {
      throw new Error('Invalid gRPC message: too short');
    }

    const messageLength =
      (uint8Array[1] << 24) |
      (uint8Array[2] << 16) |
      (uint8Array[3] << 8) |
      uint8Array[4];
    const messageData = uint8Array.slice(5, 5 + messageLength);

    // Parse the protobuf message to extract the filter string
    let offset = 0;
    let filterString = '';

    while (offset < messageData.length) {
      // Read field tag
      const tag = this.decodeVarint(messageData, offset);
      offset += this.getVarintLength(tag);

      const fieldNumber = tag >>> 3;
      const wireType = tag & 0x7;

      logger.debug(
        `parseParseFilterResponse - Field ${fieldNumber}, wire type ${wireType}`
      );

      if (fieldNumber === 1 && wireType === 2) {
        // filterString field (string)
        const length = this.decodeVarint(messageData, offset);
        offset += this.getVarintLength(length);
        const decoder = new TextDecoder();
        filterString = decoder.decode(
          messageData.slice(offset, offset + length)
        );
        offset += length;
        logger.debug(
          'parseParseFilterResponse - Got filter string:',
          filterString
        );
      } else {
        // Skip other fields
        offset = this.skipField(messageData, offset, wireType);
      }
    }

    return {
      filterExpression: filterString,
    };
  }

  // Parse gRPC-Web response
  private async parseGrpcResponse(
    response: Response
  ): Promise<DataTableResult> {
    const buffer = await response.arrayBuffer();
    const uint8Array = new Uint8Array(buffer);

    logger.debug(
      'gRPC Table Service - Response buffer size:',
      buffer.byteLength
    );
    logger.debug(
      'gRPC Table Service - Response data (first 100 bytes):',
      Array.from(uint8Array.slice(0, 100))
        .map(b => b.toString(16).padStart(2, '0'))
        .join(' ')
    );

    // Parse the full DataTableResult to get all fields (including arrow data)
    const fullResult = this.parseDataTableResult(uint8Array);

    logger.debug('Parsed DataTableResult fields:', {
      has_arrow_stream: !!fullResult.arrow_ipc_stream,
      arrow_stream_size: fullResult.arrow_ipc_stream?.length || 0,
      offset: fullResult.offset,
      row_count: fullResult.row_count,
      total_rows: fullResult.total_rows,
    });

    // If we didn't get the arrow stream from fullResult, fall back to the old method
    if (!fullResult.arrow_ipc_stream) {
      logger.warn(
        'No arrow stream found in DataTableResult, trying fallback method'
      );
      try {
        const fallbackArrowData = this.parseGrpcMessage(uint8Array);
        logger.debug('Fallback arrow data size:', fallbackArrowData.length);
        if (fallbackArrowData.length > 0) {
          fullResult.arrow_ipc_stream = fallbackArrowData;
        }
      } catch (error) {
        logger.error('Fallback parsing also failed:', error);
      }
    }

    // Parse the Arrow IPC stream if we have it
    let table: arrow.Table | undefined;
    if (fullResult.arrow_ipc_stream) {
      try {
        table = arrow.tableFromIPC(fullResult.arrow_ipc_stream);
        logger.info('gRPC Table Service - Successfully parsed Arrow table:', {
          numRows: table.numRows,
          numCols: table.numCols,
          schema: table.schema.fields.map((f: arrow.Field) => f.name),
        });
      } catch (error) {
        logger.warn('Failed to parse Arrow IPC stream:', error);
      }
    }

    return {
      arrow_ipc_stream: fullResult.arrow_ipc_stream || new Uint8Array(0),
      offset: fullResult.offset || 0,
      row_count: fullResult.row_count || 0,
      total_rows: fullResult.total_rows || 0,
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
    logger.debug('gRPC Table Service - Compression flag:', compressionFlag);

    // Read message length (4 bytes, big-endian)
    const messageLength =
      (data[1] << 24) | (data[2] << 16) | (data[3] << 8) | data[4];
    logger.debug('gRPC Table Service - Message length:', messageLength);

    // Extract the message data
    const messageData = data.slice(5, 5 + messageLength);
    logger.debug('gRPC Table Service - Message data size:', messageData.length);

    // Parse the protobuf message to extract the Arrow data
    return this.parseTableResultProtobuf(messageData);
  }

  // Parse DataTableResult to get all fields
  private parseDataTableResult(grpcData: Uint8Array): Partial<DataTableResult> {
    const result: Partial<DataTableResult> = {};

    // Extract protobuf message from gRPC wrapper manually
    if (grpcData.length < 5) return result;

    const messageLength =
      (grpcData[1] << 24) |
      (grpcData[2] << 16) |
      (grpcData[3] << 8) |
      grpcData[4];
    const messageData = grpcData.slice(5, 5 + messageLength);
    let offset = 0;

    while (offset < messageData.length) {
      // Debug: show raw bytes at current offset
      if (offset < 10) {
        logger.debug(
          `Raw bytes at offset ${offset}:`,
          Array.from(messageData.slice(offset, offset + 8))
            .map(b => `0x${b.toString(16).padStart(2, '0')}`)
            .join(' ')
        );
      }

      // Read field tag
      const tag = this.decodeVarint(messageData, offset);
      offset += this.getVarintLength(tag);

      const fieldNumber = tag >>> 3;
      const wireType = tag & 0x7;

      if (fieldNumber > 1000) {
        logger.error(
          `Invalid field number ${fieldNumber} suggests corrupted data. Tag: 0x${tag.toString(16)}`
        );
        break;
      }

      switch (fieldNumber) {
        case 1: // arrow_ipc_stream (bytes)
          if (wireType === 2) {
            const length = this.decodeVarint(messageData, offset);
            offset += this.getVarintLength(length);
            result.arrow_ipc_stream = messageData.slice(
              offset,
              offset + length
            );
            offset += length;
          }
          break;
        case 2: // offset (int32)
          if (wireType === 0) {
            result.offset = this.decodeVarint(messageData, offset);
            offset += this.getVarintLength(result.offset);
          }
          break;
        case 3: // row_count (int32)
          if (wireType === 0) {
            result.row_count = this.decodeVarint(messageData, offset);
            offset += this.getVarintLength(result.row_count);
          }
          break;
        case 4: // total_rows (int32)
          if (wireType === 0) {
            result.total_rows = this.decodeVarint(messageData, offset);
            offset += this.getVarintLength(result.total_rows);
          }
          break;
        default:
          // Log unknown fields for debugging
          logger.warn(
            `Unknown field ${fieldNumber} with wire type ${wireType} at offset ${offset}`
          );
          offset = this.skipField(messageData, offset, wireType);
          break;
      }
    }

    return result;
  }

  // Helper method to skip unknown fields
  private skipField(
    data: Uint8Array,
    offset: number,
    wireType: number
  ): number {
    switch (wireType) {
      case 0: {
        // varint
        const value = this.decodeVarint(data, offset);
        return offset + this.getVarintLength(value);
      }
      case 1: // 64-bit
        return offset + 8;
      case 2: {
        // length-delimited
        const length = this.decodeVarint(data, offset);
        return offset + this.getVarintLength(length) + length;
      }
      case 3: // start group (deprecated)
      case 4: // end group (deprecated)
        logger.warn(`Deprecated wire type ${wireType} encountered`);
        return data.length; // Skip to end
      case 5: // 32-bit
        return offset + 4;
      default:
        logger.warn(`Unknown wire type: ${wireType}`);
        return data.length; // Skip to end
    }
  }

  // Alias for backward compatibility with tests
  private parseTableResultProtobuf(data: Uint8Array): Uint8Array {
    return this.parseDataTableResultProtobuf(data);
  }

  // Parse DataTableResult protobuf message to extract arrow_ipc_stream field
  private parseDataTableResultProtobuf(data: Uint8Array): Uint8Array {
    let offset = 0;

    while (offset < data.length) {
      // Read field tag
      const tag = this.decodeVarint(data, offset);
      offset += this.getVarintLength(tag);

      const fieldNumber = tag >> 3;
      const wireType = tag & 0x07;

      logger.debug(
        `gRPC Table Service - Field ${fieldNumber}, wire type ${wireType}`
      );

      if (fieldNumber === 1 && wireType === 2) {
        // arrow_ipc_stream field
        // Read length-delimited data
        const length = this.decodeVarint(data, offset);
        offset += this.getVarintLength(length);

        // Extract the Arrow data
        const arrowData = data.slice(offset, offset + length);
        logger.debug(
          'gRPC Table Service - Extracted Arrow IPC stream size:',
          arrowData.length
        );
        return arrowData;
      } else {
        // Skip other fields
        offset = this.skipField(data, offset, wireType);
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
