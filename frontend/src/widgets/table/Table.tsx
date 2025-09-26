import '@glideapps/glide-data-grid/dist/index.css';
import React from 'react';
import { TableProvider, useTable } from './context/TableContext';
import { ErrorDisplay } from '@/components/ErrorDisplay';
import { Loading } from '@/components/Loading';
import { TableEditor } from './parts/TableEditor';
import { Footer } from './parts/TableFooter';
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
      <h1 className={tableStyles.table.heading}>Dynamic Data Grid with gRPC</h1>
      {showTableEditor ? children : <Loading />}
    </div>
  );
};

export const Table: React.FC<TableProps> = ({
  connection,
  editable = false,
}) => {
  return (
    <TableProvider connection={connection} editable={editable}>
      <TableLayout>
        <>
          <TableOptions />
          <TableEditor hasOptions={true} />
        </>
      </TableLayout>
      <Footer />
    </TableProvider>
  );
};

export default Table;
