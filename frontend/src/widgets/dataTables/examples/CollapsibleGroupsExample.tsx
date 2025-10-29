import React from 'react';
import { DataTable } from '../DataTableWidget';
import {
  DataColumn,
  DataTableConnection,
  DataTableConfiguration,
  ColType,
} from '../types/types';

/**
 * Example demonstrating how to use collapsible column groups in DataTable
 *
 * This feature allows users to collapse/expand column groups by clicking on
 * the group headers, providing a way to manage tables with many columns.
 */

// Example columns with groups
const columns: DataColumn[] = [
  // Customer Info group
  {
    name: 'id',
    header: 'ID',
    type: ColType.Number,
    width: 100,
    group: 'Customer Info',
  },
  {
    name: 'firstName',
    header: 'First Name',
    type: ColType.Text,
    width: 150,
    group: 'Customer Info',
  },
  {
    name: 'lastName',
    header: 'Last Name',
    type: ColType.Text,
    width: 150,
    group: 'Customer Info',
  },
  {
    name: 'email',
    header: 'Email',
    type: ColType.Text,
    width: 200,
    group: 'Customer Info',
  },

  // Order Details group
  {
    name: 'orderId',
    header: 'Order ID',
    type: ColType.Text,
    width: 120,
    group: 'Order Details',
  },
  {
    name: 'orderDate',
    header: 'Order Date',
    type: ColType.Date,
    width: 120,
    group: 'Order Details',
  },
  {
    name: 'product',
    header: 'Product',
    type: ColType.Text,
    width: 200,
    group: 'Order Details',
  },
  {
    name: 'quantity',
    header: 'Quantity',
    type: ColType.Number,
    width: 100,
    group: 'Order Details',
  },

  // Financial group
  {
    name: 'unitPrice',
    header: 'Unit Price',
    type: ColType.Number,
    width: 120,
    group: 'Financial',
  },
  {
    name: 'totalAmount',
    header: 'Total Amount',
    type: ColType.Number,
    width: 120,
    group: 'Financial',
  },
  {
    name: 'tax',
    header: 'Tax',
    type: ColType.Number,
    width: 100,
    group: 'Financial',
  },
  {
    name: 'discount',
    header: 'Discount',
    type: ColType.Number,
    width: 100,
    group: 'Financial',
  },

  // Shipping group
  {
    name: 'shippingAddress',
    header: 'Shipping Address',
    type: ColType.Text,
    width: 250,
    group: 'Shipping',
  },
  {
    name: 'shippingCity',
    header: 'City',
    type: ColType.Text,
    width: 120,
    group: 'Shipping',
  },
  {
    name: 'shippingCountry',
    header: 'Country',
    type: ColType.Text,
    width: 120,
    group: 'Shipping',
  },
  {
    name: 'shippingStatus',
    header: 'Status',
    type: ColType.Text,
    width: 100,
    group: 'Shipping',
  },
];

// Example connection configuration
const connection: DataTableConnection = {
  port: 3000,
  path: '/api/data',
  connectionId: 'example-connection',
  sourceId: 'example-source',
};

// Configuration with collapsible groups enabled
const configuration: DataTableConfiguration = {
  showGroups: true, // Must be true to show groups
  enableCollapsibleGroups: true, // Enable the collapsible feature
  allowColumnReordering: true,
  allowColumnResizing: true,
  allowSorting: true,
  allowFiltering: true,
  showIndexColumn: true,
};

/**
 * Basic example of collapsible column groups
 */
export const BasicCollapsibleGroupsExample: React.FC = () => {
  return (
    <div style={{ padding: '20px', height: '600px' }}>
      <h2>Collapsible Column Groups Example</h2>
      <p>
        Click on the group headers to collapse/expand column groups. The
        collapsed groups will show a ▶ indicator, and expanded groups will show
        a ▼ indicator.
      </p>
      <DataTable
        columns={columns}
        connection={connection}
        config={configuration}
        width="100%"
        height="500px"
      />
    </div>
  );
};

/**
 * Advanced example using the useCollapsableColumnGroups hook directly
 * This demonstrates programmatic control over the collapsible groups
 */
import { useCollapsableColumnGroups } from '../hooks/useCollapsableColumnGroups';
import { convertToGridColumns } from '../utils/columnHelpers';

export const AdvancedCollapsibleGroupsExample: React.FC = () => {
  // Convert columns to grid format (normally done internally)
  const gridColumns = convertToGridColumns(
    columns,
    [], // columnOrder
    {}, // columnWidths
    1000, // containerWidth
    true // showGroups
  );

  // Use the hook directly for more control
  const {
    toggleGroup,
    expandAllGroups,
    collapseAllGroups,
    isGroupCollapsed,
    collapsedGroups,
    totalColumns,
    visibleColumnCount,
    hiddenColumnCount,
  } = useCollapsableColumnGroups(gridColumns);

  return (
    <div style={{ padding: '20px' }}>
      <h2>Advanced Collapsible Groups Example</h2>

      <div style={{ marginBottom: '20px' }}>
        <h3>Group Controls</h3>
        <button onClick={expandAllGroups} style={{ marginRight: '10px' }}>
          Expand All Groups
        </button>
        <button onClick={collapseAllGroups} style={{ marginRight: '10px' }}>
          Collapse All Groups
        </button>

        <div style={{ marginTop: '10px' }}>
          <h4>Toggle Individual Groups:</h4>
          <button
            onClick={() => toggleGroup('Customer Info')}
            style={{ marginRight: '10px' }}
          >
            Toggle Customer Info{' '}
            {isGroupCollapsed('Customer Info') ? '(Collapsed)' : '(Expanded)'}
          </button>
          <button
            onClick={() => toggleGroup('Order Details')}
            style={{ marginRight: '10px' }}
          >
            Toggle Order Details{' '}
            {isGroupCollapsed('Order Details') ? '(Collapsed)' : '(Expanded)'}
          </button>
          <button
            onClick={() => toggleGroup('Financial')}
            style={{ marginRight: '10px' }}
          >
            Toggle Financial{' '}
            {isGroupCollapsed('Financial') ? '(Collapsed)' : '(Expanded)'}
          </button>
          <button
            onClick={() => toggleGroup('Shipping')}
            style={{ marginRight: '10px' }}
          >
            Toggle Shipping{' '}
            {isGroupCollapsed('Shipping') ? '(Collapsed)' : '(Expanded)'}
          </button>
        </div>
      </div>

      <div style={{ marginBottom: '20px' }}>
        <h3>Statistics</h3>
        <ul>
          <li>Total Columns: {totalColumns}</li>
          <li>Visible Columns: {visibleColumnCount}</li>
          <li>Hidden Columns: {hiddenColumnCount}</li>
          <li>Collapsed Groups: {collapsedGroups.join(', ') || 'None'}</li>
        </ul>
      </div>

      <div style={{ height: '400px' }}>
        <DataTable
          columns={columns}
          connection={connection}
          config={configuration}
          width="100%"
          height="100%"
        />
      </div>
    </div>
  );
};

/**
 * Usage Notes:
 *
 * 1. To enable collapsible column groups:
 *    - Set `showGroups: true` in the configuration
 *    - Set `enableCollapsibleGroups: true` in the configuration
 *    - Ensure your columns have the `group` property defined
 *
 * 2. Users can click on group headers to collapse/expand them
 *
 * 3. Visual indicators:
 *    - ▼ indicates an expanded group
 *    - ▶ indicates a collapsed group
 *
 * 4. The hook provides programmatic control:
 *    - toggleGroup(groupName) - Toggle a specific group
 *    - expandAllGroups() - Expand all groups
 *    - collapseAllGroups() - Collapse all groups
 *    - isGroupCollapsed(groupName) - Check if a group is collapsed
 *
 * 5. The feature preserves column order and other functionality like
 *    sorting, filtering, and column resizing.
 */

export default BasicCollapsibleGroupsExample;
