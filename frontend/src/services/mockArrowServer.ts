// Mock gRPC server for testing DataTable widget
// This simulates a server providing Apache Arrow tables stream service

export interface MockArrowTableData {
  columns: string[];
  rows: unknown[][];
  totalRows: number;
  hasMore: boolean;
}

export class MockArrowServer {
  private static instance: MockArrowServer;
  private port: number;

  constructor(port: number = 50051) {
    this.port = port;
  }

  static getInstance(port: number = 50051): MockArrowServer {
    if (!MockArrowServer.instance) {
      MockArrowServer.instance = new MockArrowServer(port);
    }
    return MockArrowServer.instance;
  }

  async start(): Promise<void> {
    // In a real implementation, this would start an actual HTTP server
    // For now, we'll just log that the server is "started"
    console.log(`Mock Arrow Server started on port ${this.port}`);
  }

  async stop(): Promise<void> {
    console.log('Mock Arrow Server stopped');
  }

  // Mock data generators
  private generateUsersData(limit: number = 10): MockArrowTableData {
    const columns = ['id', 'name', 'email', 'created_at', 'status'];
    const rows: unknown[][] = [];

    for (let i = 1; i <= limit; i++) {
      rows.push([
        i,
        `User ${i}`,
        `user${i}@example.com`,
        new Date(Date.now() - Math.random() * 10000000000).toISOString(),
        Math.random() > 0.5 ? 'active' : 'inactive',
      ]);
    }

    return {
      columns,
      rows,
      totalRows: limit,
      hasMore: false,
    };
  }

  private generateProductsData(limit: number = 10): MockArrowTableData {
    const columns = ['product_id', 'name', 'price', 'category', 'in_stock'];
    const rows: unknown[][] = [];
    const categories = ['Electronics', 'Clothing', 'Books', 'Home', 'Sports'];

    for (let i = 1; i <= limit; i++) {
      rows.push([
        `PROD-${i.toString().padStart(3, '0')}`,
        `Product ${i}`,
        Math.round((Math.random() * 1000 + 10) * 100) / 100,
        categories[Math.floor(Math.random() * categories.length)],
        Math.random() > 0.3,
      ]);
    }

    return {
      columns,
      rows,
      totalRows: limit,
      hasMore: false,
    };
  }

  private generateAnalyticsData(limit: number = 10): MockArrowTableData {
    const columns = ['date', 'revenue', 'orders', 'avg_order_value'];
    const rows: unknown[][] = [];

    for (let i = 0; i < limit; i++) {
      const date = new Date();
      date.setDate(date.getDate() - i);

      const revenue = Math.round((Math.random() * 10000 + 1000) * 100) / 100;
      const orders = Math.floor(Math.random() * 100 + 10);
      const avgOrderValue = Math.round((revenue / orders) * 100) / 100;

      rows.push([
        date.toISOString().split('T')[0],
        revenue,
        orders,
        avgOrderValue,
      ]);
    }

    return {
      columns,
      rows,
      totalRows: limit,
      hasMore: false,
    };
  }

  // Mock query processor
  processQuery(query: string, limit: number = 100): MockArrowTableData {
    const lowerQuery = query.toLowerCase();

    if (lowerQuery.includes('users')) {
      return this.generateUsersData(limit);
    } else if (lowerQuery.includes('products')) {
      return this.generateProductsData(limit);
    } else if (
      lowerQuery.includes('analytics') ||
      lowerQuery.includes('revenue')
    ) {
      return this.generateAnalyticsData(limit);
    } else {
      // Default to users data
      return this.generateUsersData(limit);
    }
  }

  // Simulate streaming with delays
  async streamData(
    query: string,
    limit: number = 100
  ): Promise<MockArrowTableData> {
    // Simulate network delay
    await new Promise(resolve =>
      setTimeout(resolve, 500 + Math.random() * 1000)
    );

    return this.processQuery(query, limit);
  }
}

// Export singleton instance
export const mockArrowServer = MockArrowServer.getInstance();
