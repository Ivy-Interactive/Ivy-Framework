import '@glideapps/glide-data-grid/dist/index.css';
import './styles/checkbox.css';
import React from 'react';
import { TableProvider, useTable } from './context/TableContext';
import { ErrorDisplay } from '@/components/ErrorDisplay';
import { Loading } from '@/components/Loading';
import { TableEditor } from './parts/TableEditor';
import { TableOptions } from './parts/TableOptions';
import { tableStyles } from './styles/style';
import { TableProps } from './types/types';

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
    <div className={tableStyles.table.container}>
      {showTableEditor ? children : <Loading />}
    </div>
  );
};

export const Table: React.FC<TableProps> = ({
  connection,
  config = {},
  editable = false,
}) => {
  // Apply default configuration values
  const finalConfig = {
    allowSearch: config.allowSearch ?? true,
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

  return (
    <TableProvider
      connection={connection}
      config={finalConfig}
      editable={editable}
    >
      <TableLayout>
        <>
          {finalConfig.allowFiltering && <TableOptions />}
          <TableEditor hasOptions={finalConfig.allowFiltering} />
        </>
      </TableLayout>
    </TableProvider>
  );
};

export default Table;
