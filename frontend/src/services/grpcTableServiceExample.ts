// Example usage of the gRPC Table Service
import { grpcTableService, TableQuery } from './grpcTableService';

// Example 1: Simple query with sorting
async function simpleQueryExample() {
  const query: TableQuery = {
    sort: [
      { column: 'name', direction: 'ASC' },
      { column: 'created_at', direction: 'DESC' },
    ],
    limit: 100,
    offset: 0,
    selectColumns: ['id', 'name', 'email', 'created_at'],
  };

  try {
    const result = await grpcTableService.queryTable({
      serverUrl: 'http://localhost:50051',
      query,
      onData: data => {
        console.log('Received data:', data);
        if (data.table) {
          console.log(
            'Arrow table columns:',
            data.table.schema.fields.map(f => f.name)
          );
          console.log('Row count:', data.table.numRows);
        }
      },
      onError: error => {
        console.error('Query error:', error);
      },
      onComplete: () => {
        console.log('Query completed');
      },
    });

    return result;
  } catch (error) {
    console.error('Failed to execute query:', error);
    throw error;
  }
}

// Example 2: Complex query with filters and aggregations
async function complexQueryExample() {
  const query: TableQuery = {
    filter: {
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
              column: 'age',
              function: 'greaterThan',
              args: [18],
            },
          },
          {
            group: {
              op: 'OR',
              filters: [
                {
                  condition: {
                    column: 'country',
                    function: 'inSet',
                    args: ['US', 'CA', 'UK'],
                  },
                },
                {
                  condition: {
                    column: 'premium',
                    function: 'equals',
                    args: [true],
                  },
                },
              ],
            },
          },
        ],
      },
    },
    aggregations: [
      { column: 'revenue', function: 'sum' },
      { column: 'users', function: 'count' },
      { column: 'rating', function: 'avg' },
    ],
    sort: [{ column: 'revenue', direction: 'DESC' }],
    limit: 50,
  };

  try {
    const result = await grpcTableService.queryTable({
      serverUrl: 'http://localhost:50051',
      query,
      onData: data => {
        if (data.table) {
          // Process the Arrow table data
          const columns = data.table.schema.fields.map(f => f.name);
          console.log('Available columns:', columns);

          // Convert to array of objects for easier processing
          const rows = data.table.toArray().map(row => {
            const obj: Record<string, unknown> = {};
            columns.forEach((col, index) => {
              obj[col] = row[index];
            });
            return obj;
          });

          console.log('Processed rows:', rows);
        }
      },
    });

    return result;
  } catch (error) {
    console.error('Failed to execute complex query:', error);
    throw error;
  }
}

// Example 3: Using the service with event listeners
function eventListenerExample() {
  const query: TableQuery = {
    limit: 10,
    selectColumns: ['id', 'name'],
  };

  // Set up event listeners
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  grpcTableService.on('data', data => {
    console.log('Event: Received data');
  });

  grpcTableService.on('error', error => {
    console.error('Event: Query error:', error);
  });

  grpcTableService.on('complete', () => {
    console.log('Event: Query completed');
  });

  // Execute query
  grpcTableService.queryTable({
    serverUrl: 'http://localhost:50051',
    query,
  });
}

export { simpleQueryExample, complexQueryExample, eventListenerExample };
