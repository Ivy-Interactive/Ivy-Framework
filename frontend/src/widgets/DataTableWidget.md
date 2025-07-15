# DataTable Widget

The DataTable widget connects to a gRPC server providing Apache Arrow tables stream service and renders the data in a responsive table format.

## Features

- **Real-time streaming**: Connects to gRPC server and streams Apache Arrow table data
- **Query interface**: Optional SQL query input for dynamic data retrieval
- **Responsive design**: Adapts to different screen sizes with proper scrolling
- **Status indicators**: Shows loading, streaming, and data status
- **Error handling**: Displays errors gracefully with retry options
- **Event system**: Fires events for data received, errors, and stream completion

## Usage

### Basic Usage

```csharp
new DataTable()
    .ServerUrl("http://localhost:50051")
    .Query("SELECT * FROM users LIMIT 10")
    .Title("Users Table")
    .Description("Streaming user data from gRPC server")
```

### Advanced Configuration

```csharp
new DataTable()
    .ServerUrl("http://localhost:50051")
    .Query("SELECT id, name, email, created_at FROM users ORDER BY created_at DESC")
    .Limit(50)
    .Offset(0)
    .ShowQueryInput(true)
    .ShowRefreshButton(true)
    .ShowStatus(true)
    .Title("Recent Users")
    .Description("Latest user registrations")
```

## Properties

| Property            | Type      | Default | Description                                              |
| ------------------- | --------- | ------- | -------------------------------------------------------- |
| `ServerUrl`         | `string?` | `null`  | The URL of the gRPC server providing Apache Arrow tables |
| `Query`             | `string?` | `""`    | The SQL query to execute on the server                   |
| `Limit`             | `int`     | `100`   | Maximum number of rows to fetch                          |
| `Offset`            | `int`     | `0`     | Number of rows to skip                                   |
| `ShowQueryInput`    | `bool`    | `true`  | Whether to show the query input field                    |
| `ShowRefreshButton` | `bool`    | `true`  | Whether to show the refresh button                       |
| `ShowStatus`        | `bool`    | `true`  | Whether to show the status badge                         |
| `Title`             | `string?` | `null`  | Optional title for the table                             |
| `Description`       | `string?` | `null`  | Optional description for the table                       |

## Events

| Event              | Description                                     | Parameters               |
| ------------------ | ----------------------------------------------- | ------------------------ |
| `OnDataReceived`   | Fired when new data is received from the stream | `ArrowTableData`         |
| `OnError`          | Fired when an error occurs during streaming     | `string` (error message) |
| `OnStreamComplete` | Fired when the stream completes successfully    | None                     |

## Data Format

The widget expects data in the following format from the gRPC server:

```typescript
interface ArrowTableData {
  columns: string[];
  rows: unknown[][];
  totalRows: number;
  hasMore: boolean;
}
```

## Server Requirements

The gRPC server should:

1. Accept HTTP requests with query parameters (`query`, `limit`, `offset`)
2. Return Server-Sent Events (SSE) with JSON data
3. Send a `complete` event when the stream is finished
4. Handle errors gracefully and return appropriate error messages

### Example Server Response

```
data: {"columns":["id","name","email"],"rows":[[1,"John","john@example.com"],[2,"Jane","jane@example.com"]],"totalRows":2,"hasMore":false}

event: complete
data:
```

## Styling

The widget uses Tailwind CSS classes and follows the Ivy Framework design system. It includes:

- Responsive table with horizontal scrolling
- Loading states with spinners
- Status badges with icons
- Error alerts with proper styling
- Consistent spacing and typography

## Accessibility

- Proper ARIA labels for interactive elements
- Keyboard navigation support
- Screen reader friendly table structure
- High contrast mode support
- Focus management for form inputs

## Browser Support

- Modern browsers with EventSource support
- Fallback error handling for unsupported browsers
- Progressive enhancement for older browsers
