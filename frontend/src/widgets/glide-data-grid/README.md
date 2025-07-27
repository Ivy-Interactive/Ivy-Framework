# GlideDataGrid Widget

A high-performance, feature-rich data grid component built on top of `@glideapps/glide-data-grid` for the Ivy Framework.

## Features

- ✅ **Virtual Scrolling** - Handle thousands of rows efficiently
- ✅ **Column Resizing** - User-friendly column width adjustment
- ✅ **Cell Editing** - Optional in-cell editing capabilities
- ✅ **Type Detection** - Automatic cell type detection (text, number, boolean)
- ✅ **Sorting** - Column-based sorting (future implementation)
- ✅ **Real-time Data** - Integration with gRPC streaming via grpcTableService
- ✅ **Theme Support** - Customizable themes matching Ivy design system
- ✅ **Performance Optimized** - Debounced operations and memoization
- ✅ **Modular Components** - Clean separation of concerns with focused hooks and components

## Usage

### Basic Usage

```tsx
import { GlideDataGridWidget } from '@/widgets/glide-data-grid';

function MyApp() {
  return (
    <GlideDataGridWidget
      id="my-grid"
      connection={{
        port: 5000,
        path: "/api/data",
        connectionId: "conn-123",
        sourceId: "users"
      }}
      title="Users Data"
      width="full"
      height="600px"
      resizableColumns={true}
      editable={false}
    />
  );
}
```

### Advanced Usage

```tsx
import { GlideDataGridWidget, DataTableData } from '@/widgets/glide-data-grid';

function MyAdvancedGrid() {
  const handleDataReceived = (data: DataTableData) => {
    console.log('Received data:', data);
  };

  const handleCellEdited = (cell: any, newValue: any) => {
    console.log('Cell edited:', cell, newValue);
  };

  return (
    <GlideDataGridWidget
      id="advanced-grid"
      connection={{
        port: 5001,
        path: "/api/advanced-data",
        connectionId: "conn-456",
        sourceId: "products"
      }}
      title="Product Catalog"
      description="Manage your product inventory"
      width="1200px"
      height="800px"
      resizableColumns={true}
      sortable={true}
      editable={true}
      virtualScrolling={true}
      rowMarkers="both"
      smoothScrollX={true}
      smoothScrollY={true}
      pageSize={200}
      onDataReceived={handleDataReceived}
      onCellEdited={handleCellEdited}
      showRefreshButton={true}
      showStatus={true}
    />
  );
}
```

## Props

| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `id` | `string` | Required | Unique identifier for the widget |
| `connection` | `DataTableConnection` | Required | gRPC connection configuration |
| `width` | `string` | Optional | Widget width (CSS value) |
| `height` | `string` | Optional | Widget height (CSS value) |
| `title` | `string` | Optional | Grid title displayed in header |
| `description` | `string` | Optional | Grid description displayed in header |
| `showRefreshButton` | `boolean` | `true` | Show refresh button in header |
| `showStatus` | `boolean` | `true` | Show status badge in header |
| `resizableColumns` | `boolean` | `true` | Enable column resizing |
| `sortable` | `boolean` | `true` | Enable column sorting |
| `editable` | `boolean` | `false` | Enable cell editing |
| `virtualScrolling` | `boolean` | `true` | Enable virtual scrolling |
| `rowMarkers` | `'both' \| 'number' \| 'checkbox' \| 'none'` | `'both'` | Row marker type |
| `smoothScrollX` | `boolean` | `true` | Enable smooth horizontal scrolling |
| `smoothScrollY` | `boolean` | `true` | Enable smooth vertical scrolling |
| `pageSize` | `number` | `100` | Number of rows to fetch per request |

## Events

| Event | Parameters | Description |
|-------|------------|-------------|
| `onDataReceived` | `(data: DataTableData)` | Fired when new data is received |
| `onError` | `(error: string)` | Fired when an error occurs |
| `onStreamComplete` | `()` | Fired when streaming completes |
| `onCellEdited` | `(cell: Item, newValue: EditableGridCell)` | Fired when a cell is edited |
| `onColumnResize` | `(column: GridColumn, newSize: number)` | Fired when a column is resized |

## Architecture

The component follows a clean, modular architecture with separation of concerns:

### **Hooks** (`hooks/`)
- **`useDataTableFetch`** - Focused data fetching and connection management
- **`useGlideColumns`** - Column configuration and memoization
- **`useGridSelection`** - Selection state management
- **`useColumnResizing`** - Column width management
- **`useCellFormatting`** - Cell content formatting

### **Components** (`components/`)
- **`GridHeader`** - Header with title, description, status, and refresh button
- **`StatusBadge`** - Connection and data status indicator
- **`LoadingSpinner`** - Loading state display
- **`ErrorAlert`** - Error message display
- **`ConnectionAlert`** - Connection configuration error display

### **Utils** (`utils/`)
- **`arrowDataTransforms`** - Apache Arrow to grid data conversion
- **`cellTypeMapping`** - Cell type detection and formatting
- **`glideGridColumns`** - Glide column configuration
- **`gridDimensions`** - Grid size calculation helpers

### **Styles** (`styles/`)
- **`glideTheme`** - Predefined themes matching Ivy design system
- **`classNames`** - Centralized className definitions

### **Types** (`types/`)
- Complete TypeScript type definitions for all interfaces and props

## Performance

- **Virtual Scrolling**: Only renders visible rows for optimal performance
- **Memoization**: Expensive computations are memoized with useMemo
- **Focused Hooks**: Each hook has a single responsibility for better performance
- **Lazy Loading**: Components load only when needed
- **Debounced Operations**: User interactions are optimized

## Integration with Ivy Framework

The component integrates seamlessly with the Ivy Framework:

- Uses `grpcTableService` for data fetching via `useDataTableFetch`
- Integrates with Ivy's event system via `useEventHandler`
- Follows Ivy's styling patterns with `getWidth`/`getHeight`
- Uses Ivy's design system colors and components
- Supports Ivy's widget registration system

## Customization

### Custom Themes

```tsx
import { createCustomTheme, defaultGlideTheme } from '@/widgets/glide-data-grid';

const customTheme = createCustomTheme(defaultGlideTheme, {
  accentColor: '#ff6b6b',
  headerFontStyle: '700 16px',
});
```

### Custom Cell Formatting

The component automatically detects cell types based on data and column metadata. You can extend formatting by modifying the `useCellFormatting` hook or utility functions.

### Using Individual Hooks

```tsx
import { 
  useDataTableFetch, 
  useGlideColumns, 
  useGridSelection 
} from '@/widgets/glide-data-grid';

function CustomGrid() {
  const { data, loading, error } = useDataTableFetch(connection);
  const { gridSelection, setGridSelection } = useGridSelection();
  // Build your own grid implementation
}
```

## Dependencies

- `@glideapps/glide-data-grid` - Core grid component
- `apache-arrow` - Data format handling
- React 19+ - Framework
- Ivy Framework services and utilities

## Migration Notes

The component has been refactored for better maintainability:

- `useGlideDataGrid` → `useDataTableFetch` (focused on data fetching only)
- UI concerns extracted into separate focused hooks
- Event handlers moved into the main component
- Better component composition with extracted UI components