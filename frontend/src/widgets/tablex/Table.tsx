import '@glideapps/glide-data-grid/dist/index.css';
import React from 'react';

// Local imports
import { TableProvider, useTable } from './context/TableContext';
import { ErrorDisplay } from './parts/ErrorDisplay';
import { LoadingDisplay } from './parts/LoadingDisplay';
import { TableEditor } from './parts/TableEditor';
import { Footer } from './parts/TableFooter';
import { TableOptions } from './parts/TableOptions';
import { tableStyles } from './styles';
import { TableProps } from './types';

const TableLayout: React.FC = () => {
  const { error, columns } = useTable();
  const showTableEditor = columns.length > 0;

  if (error) {
    return <ErrorDisplay />;
  }

  return (
    <div className={tableStyles.container}>
      <h1 className={tableStyles.heading.primary}>
        Dynamic Data Grid with gRPC
      </h1>
      {showTableEditor ? (
        <>
          <TableOptions />
          <TableEditor hasOptions={true} />
        </>
      ) : (
        <LoadingDisplay />
      )}
      <Footer />
    </div>
  );
};

export const Table: React.FC<TableProps> = ({
  connection,
  editable = false,
}) => {
  return (
    <TableProvider connection={connection} editable={editable}>
      <TableLayout />
    </TableProvider>
  );
};

export default Table;
