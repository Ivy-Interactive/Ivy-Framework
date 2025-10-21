import '@glideapps/glide-data-grid/dist/index.css';
import './styles/checkbox.css';
import React from 'react';
import { TableProvider, useTable } from './DataTableContext';
import { ErrorDisplay } from '@/components/ErrorDisplay';
import { Loading } from '@/components/Loading';
import { DataTableEditor } from './DataTableEditor';
import { DataTableOptions } from './DataTableOptions';
import { tableStyles } from './styles/style';
import { TableProps } from './types/types';
import { getWidth, getHeight } from '@/lib/styles';

interface TableLayoutProps {
  children?: React.ReactNode;
}

const TableLayout: React.FC<TableLayoutProps> = ({ children }) => {
  const { error, columns } = useTable();
  const showTableEditor = columns.length > 0;

  if (error) {
    return <ErrorDisplay title="Table Error" message={error} />;
  }

  return (
    <div style={tableStyles.table.container}>
      {showTableEditor ? children : <Loading />}
    </div>
  );
};

export const DataTable: React.FC<TableProps> = ({
  columns,
  connection,
  config = {},
  editable = false,
  width,
  height,
}) => {
  // Apply default configuration values
  const finalConfig = {
    // allowSearch: config.allowSearch ?? true,
    filterType: config.filterType,
    freezeColumns: config.freezeColumns ?? null,
    allowSorting: config.allowSorting ?? true,
    allowFiltering: config.allowFiltering ?? true,
    allowColumnReordering: config.allowColumnReordering ?? true,
    allowColumnResizing: config.allowColumnResizing ?? true,
    allowCopySelection: config.allowCopySelection ?? true,
    selectionMode: config.selectionMode,
    showIndexColumn: config.showIndexColumn ?? false,
    showGroups: config.showGroups ?? false,
  };

  // Create styles object with width and height if provided
  const containerStyle: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <div style={containerStyle}>
      <TableProvider
        columns={columns}
        connection={connection}
        config={finalConfig}
        editable={editable}
      >
        <TableLayout>
          <>
            <DataTableOptions
              hasOptions={{
                allowFiltering: finalConfig.allowFiltering,
              }}
            />

            <DataTableEditor hasOptions={finalConfig.allowFiltering} />
          </>
        </TableLayout>
      </TableProvider>
    </div>
  );
};

export default DataTable;
