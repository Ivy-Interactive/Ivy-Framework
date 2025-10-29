import React from 'react';
import { DataTable } from '../DataTableWidget';
import {
  DataColumn,
  ColType,
  DataTableConnection,
  SelectionModes,
} from '../types/types';

/**
 * Demonstrates the built-in column multi-selection feature.
 *
 * Column selection is automatically enabled when selectionMode is set to Columns.
 * No external state management or callbacks required!
 *
 * Keyboard shortcuts:
 * - Click: Select single column
 * - Ctrl/Cmd + Click: Toggle column selection
 * - Shift + Click: Select range of columns
 */
export const ColumnSelectionDemo: React.FC = () => {
  const columns: DataColumn[] = [
    { name: 'id', header: 'ID', type: ColType.Number, width: 80 },
    { name: 'name', header: 'Name', type: ColType.Text, width: 150 },
    { name: 'email', header: 'Email', type: ColType.Text, width: 200 },
    {
      name: 'department',
      header: 'Department',
      type: ColType.Text,
      width: 120,
    },
    { name: 'role', header: 'Role', type: ColType.Text, width: 120 },
    { name: 'active', header: 'Active', type: ColType.Boolean, width: 80 },
  ];

  const connection: DataTableConnection = {
    port: 8080,
    path: '/api/demo',
    connectionId: 'demo-connection',
    sourceId: 'demo-source',
  };

  return (
    <div style={{ padding: '20px' }}>
      <h2>Column Multi-Selection Demo</h2>
      <p>
        Column selection is now built-in! Just set{' '}
        <code>selectionMode: SelectionModes.Columns</code>
      </p>

      <DataTable
        columns={columns}
        connection={connection}
        config={{
          selectionMode: SelectionModes.Columns, // This enables column multi-selection
          allowColumnReordering: true,
          allowColumnResizing: true,
          showIndexColumn: true,
          allowSorting: false, // Disabled to avoid conflicts with selection
        }}
        height="400px"
      />
    </div>
  );
};

export default ColumnSelectionDemo;
