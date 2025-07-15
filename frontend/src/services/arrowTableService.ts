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

export interface ArrowTableData {
  columns: string[];
  rows: unknown[][];
  totalRows: number;
  hasMore: boolean;
}

export interface ArrowTableStreamOptions {
  serverUrl: string;
  query?: string;
  limit?: number;
  offset?: number;
  onData?: (data: ArrowTableData) => void;
  onError?: (error: Error) => void;
  onComplete?: () => void;
}

export class ArrowTableService extends EventEmitter {
  private serverUrl: string;
  private eventSource: EventSource | null = null;
  private isConnected = false;

  constructor(serverUrl: string) {
    super();
    this.serverUrl = serverUrl;
  }

  async streamTableData(options: ArrowTableStreamOptions): Promise<void> {
    const { query, limit, offset, onData, onError, onComplete } = options;

    // Build query parameters
    const params = new URLSearchParams();
    if (query) params.append('query', query);
    if (limit) params.append('limit', limit.toString());
    if (offset) params.append('offset', offset.toString());

    const url = `${this.serverUrl}/stream?${params.toString()}`;

    try {
      this.eventSource = new EventSource(url);
      this.isConnected = true;

      this.eventSource.onmessage = event => {
        try {
          const data: ArrowTableData = JSON.parse(event.data);

          if (onData) {
            onData(data);
          }

          this.emit('data', data);
        } catch (error) {
          const errorObj =
            error instanceof Error ? error : new Error('Failed to parse data');
          if (onError) {
            onError(errorObj);
          }
          this.emit('error', errorObj);
        }
      };

      this.eventSource.onerror = () => {
        const errorObj = new Error('EventSource error');
        if (onError) {
          onError(errorObj);
        }
        this.emit('error', errorObj);
        this.disconnect();
      };

      this.eventSource.addEventListener('complete', () => {
        if (onComplete) {
          onComplete();
        }
        this.emit('complete');
        this.disconnect();
      });
    } catch (error) {
      const errorObj =
        error instanceof Error
          ? error
          : new Error('Failed to connect to stream');
      if (onError) {
        onError(errorObj);
      }
      this.emit('error', errorObj);
    }
  }

  disconnect(): void {
    if (this.eventSource) {
      this.eventSource.close();
      this.eventSource = null;
    }
    this.isConnected = false;
    this.emit('disconnected');
  }

  isStreaming(): boolean {
    return this.isConnected && this.eventSource !== null;
  }
}

// Create a singleton instance
export const arrowTableService = new ArrowTableService(
  process.env.NODE_ENV === 'development'
    ? 'http://localhost:50051'
    : window.location.origin.replace(/^https?:\/\//, 'grpc://')
);
