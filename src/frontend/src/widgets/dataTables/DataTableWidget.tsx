import '@glideapps/glide-data-grid/dist/index.css';
import './styles/checkbox.css';
import React, { useMemo } from 'react';
import { TableProvider } from './dataTableContext';
import { useTable } from './dataTableContext';
import { ErrorDisplay } from '@/components/ErrorDisplay';
import { Loading } from '@/components/Loading';
import { DataTableEditor } from './dataTableEditor';
import { DataTableHeader } from './DataTableHeader';
import { DataTableOption } from './DataTableOption';
import { DataTableFilterOption } from './options/DataTableFilterOption';
import { Filter as FilterIcon } from 'lucide-react';
import { tableStyles } from './styles/style';
import { TableProps } from './types/types';
import { getWidth, getHeight } from '@/lib/styles';
import { applyConfigDefaults, applyColumnsDefaults } from './DataTableDefaults';

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
  width = 'Full',
  height = 'Full',
  rowActions,
  'data-testid': dataTestId,
}) => {
  const finalConfig = useMemo(
    () => ({
      ...applyConfigDefaults(config),
      // Frontend-only config options (not in backend)
      filterType: config.filterType,
      enableRowHover: config.enableRowHover ?? true,
    }),
    [config]
  );

  const finalColumns = useMemo(() => applyColumnsDefaults(columns), [columns]);

  // Create styles object with width and height if provided
  const containerStyle: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  // If height is Full, ensure it can shrink and grow properly within a flex container
  // to avoid "infinite growth" when no height constraint is provided by the parent.
  if (height === 'Full') {
    containerStyle.flexGrow = 1;
    containerStyle.minHeight = 0;
  }

  return (
    <div style={containerStyle} data-testid={dataTestId}>
      <TableProvider
        columns={finalColumns}
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
          />
        </TableLayout>
      </TableProvider>
    </div>
  );
};

export default DataTable;
