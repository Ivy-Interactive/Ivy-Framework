import '@glideapps/glide-data-grid/dist/index.css';
import './styles/checkbox.css';
import React from 'react';
import { TableProvider } from './dataTableContext';
import { useTable } from './dataTableContext';
import { ErrorDisplay } from '@/components/ErrorDisplay';
import { Loading } from '@/components/Loading';
import { DataTableEditor } from './dataTableEditor';
import { DataTableHeader } from './dataTableHeader';
import { DataTableFooter } from './dataTableFooter';
import { DataTableOption } from './dataTableOption';
import { DataTableFilterOption } from './options/dataTableFilterOption';
import { Filter as FilterIcon } from 'lucide-react';
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
  id,
  columns,
  connection,
  config = {},
  editable = false,
  width,
  height,
  rowActions,
  'data-testid': dataTestId,
}) => {
  // Apply default config values
  const finalConfig = {
    filterType: config.filterType,
    freezeColumns: config.freezeColumns ?? null,
    allowLlmFiltering: config.allowLlmFiltering ?? false,
    allowSorting: config.allowSorting ?? true,
    allowFiltering: config.allowFiltering ?? false,
    allowColumnReordering: config.allowColumnReordering ?? true,
    allowColumnResizing: config.allowColumnResizing ?? true,
    allowCopySelection: config.allowCopySelection ?? false,
    selectionMode: config.selectionMode,
    showIndexColumn: config.showIndexColumn ?? false,
    showGroups: config.showGroups ?? false,
    showColumnTypeIcons: config.showColumnTypeIcons ?? false,
    showVerticalBorders: config.showVerticalBorders ?? false,
    batchSize: config.batchSize,
    loadAllRows: config.loadAllRows ?? false,
    showSearch: config.showSearch ?? false,
    enableRowHover: config.enableRowHover ?? true,
    enableCellClickEvents: config.enableCellClickEvents ?? false,
  };

  // Create styles object with width and height if provided
  const containerStyle: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <div style={containerStyle} data-testid={dataTestId}>
      <TableProvider
        columns={columns}
        connection={connection}
        config={finalConfig}
        editable={editable}
      >
        <TableLayout>
          <DataTableHeader>
            {finalConfig.allowFiltering && (
              <DataTableOption
                icon={FilterIcon}
                label="Filter"
                tooltip="Filter table data"
                displayMode="inline"
                inlineDirection="right"
                showLabel={false}
              >
                <DataTableFilterOption
                  allowLlmFiltering={finalConfig.allowLlmFiltering}
                />
              </DataTableOption>
            )}
          </DataTableHeader>

          <DataTableEditor
            widgetId={id}
            hasOptions={finalConfig.allowFiltering}
            rowActions={rowActions}
            footer={<DataTableFooter />}
          />
        </TableLayout>
      </TableProvider>
    </div>
  );
};

export default DataTable;
