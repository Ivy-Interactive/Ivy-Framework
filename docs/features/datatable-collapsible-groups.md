# DataTable Collapsible Column Groups

## Overview

The DataTable component now supports collapsible column groups, allowing users to collapse and expand groups of related columns. This feature is particularly useful for tables with many columns, enabling users to focus on specific data while hiding less relevant information temporarily.

## How to Enable

To enable collapsible column groups in your DataTable:

1. Set `showGroups: true` in your DataTableConfiguration
2. Set `enableCollapsibleGroups: true` in your DataTableConfiguration
3. Ensure your columns have the `group` property defined

```typescript
const configuration: DataTableConfiguration = {
  showGroups: true,                    // Required to show groups
  enableCollapsibleGroups: true,       // Enable collapsible functionality
  // ... other configuration options
};
```

## Column Configuration

Define groups by adding the `group` property to your columns:

```typescript
const columns: DataColumn[] = [
  {
    name: 'firstName',
    header: 'First Name',
    type: ColType.Text,
    width: 150,
    group: 'Customer Info',  // Assign column to a group
  },
  {
    name: 'lastName',
    header: 'Last Name',
    type: ColType.Text,
    width: 150,
    group: 'Customer Info',  // Same group
  },
  // ... more columns
];
```

## User Interaction

### Clicking Group Headers
- Click on any group header to toggle its collapsed/expanded state
- Visual indicators:
  - ▼ (down arrow) indicates an expanded group
  - ▶ (right arrow) indicates a collapsed group

### Visual Feedback
- Collapsed groups show a minimal header with the group name
- Expanded groups display all columns within the group
- Group headers have hover effects for better user experience

## Programmatic Control

For advanced use cases, you can use the `useCollapsableColumnGroups` hook directly:

```typescript
import { useCollapsableColumnGroups } from '@/widgets/dataTables/hooks';

const {
  columns,               // Filtered columns (hiding collapsed groups)
  onGroupHeaderClicked, // Handler for group header clicks
  drawGroupHeader,      // Custom group header renderer
  toggleGroup,          // Toggle specific group
  expandAllGroups,      // Expand all groups
  collapseAllGroups,    // Collapse all groups
  isGroupCollapsed,     // Check if group is collapsed
  collapsedGroups,      // Array of collapsed group names
} = useCollapsableColumnGroups(gridColumns);
```

### Available Methods

- `toggleGroup(groupName: string)` - Toggle a specific group's state
- `expandAllGroups()` - Expand all column groups
- `collapseAllGroups()` - Collapse all column groups
- `isGroupCollapsed(groupName: string)` - Check if a group is collapsed

## Example Implementation

### Basic Usage

```typescript
import { DataTable } from '@/widgets/dataTables';

const MyDataTable = () => {
  const columns: DataColumn[] = [
    // Define columns with groups
    { name: 'id', group: 'Basic Info', ... },
    { name: 'name', group: 'Basic Info', ... },
    { name: 'price', group: 'Financial', ... },
    { name: 'tax', group: 'Financial', ... },
  ];

  const config: DataTableConfiguration = {
    showGroups: true,
    enableCollapsibleGroups: true,
  };

  return (
    <DataTable
      columns={columns}
      connection={connection}
      config={config}
    />
  );
};
```

### Advanced Usage with Controls

```typescript
const AdvancedDataTable = () => {
  // ... setup columns and config

  return (
    <div>
      <div>
        <button onClick={() => /* expand all */}>Expand All</button>
        <button onClick={() => /* collapse all */}>Collapse All</button>
      </div>
      <DataTable
        columns={columns}
        connection={connection}
        config={config}
      />
    </div>
  );
};
```

## Features Preserved

The collapsible groups feature works seamlessly with existing DataTable functionality:

- ✅ Column sorting
- ✅ Column filtering
- ✅ Column resizing
- ✅ Column reordering (within visible columns)
- ✅ Row selection
- ✅ Cell selection
- ✅ Data scrolling and pagination

## Performance Considerations

- Collapsing groups reduces the number of rendered columns, improving performance for large tables
- State changes are optimized to only re-render affected components
- The collapsed state is maintained in memory (not persisted by default)

## Customization

The feature uses the existing theme system for styling. Customize the appearance through the theme configuration:

```typescript
const customTheme = {
  bgHeader: '#f0f0f0',           // Group header background
  bgHeaderHovered: '#e0e0e0',    // Group header hover state
  textGroupHeader: '#666',        // Group header text color
  // ... other theme properties
};
```

## Migration Guide

If you're upgrading from a version without collapsible groups:

1. No breaking changes - the feature is opt-in
2. Existing tables without `enableCollapsibleGroups: true` will continue to work as before
3. To enable the feature, add the configuration options as shown above

## Troubleshooting

### Groups not collapsible
- Ensure both `showGroups: true` and `enableCollapsibleGroups: true` are set
- Verify columns have the `group` property defined

### Visual indicators not showing
- Check that the theme includes proper colors for group headers
- Ensure the `groupHeaderHeight` is sufficient (default: 36px)

### Columns not hiding when collapsed
- Verify the group names match exactly (case-sensitive)
- Check that the columns are properly assigned to groups

## Related Issues

This feature was implemented to address issue #1242: "Group collapse functionality for DataTables"