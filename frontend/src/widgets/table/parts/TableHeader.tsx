import React from 'react';
import { useTable } from '../context/TableContext';
import { tableStyles } from '../styles/style';

export const Header: React.FC = () => {
  const { visibleRows, columns, editable, isLoading, hasMore } = useTable();
  return (
    <>
      <div className={tableStyles.tableHeader.container}>
        <h1>
          <b>Dynamic Data Grid with gRPC</b>
        </h1>
        <span className={tableStyles.tableHeader.text}>
          Showing {visibleRows} rows
        </span>
        {columns.length > 0 && (
          <span className={tableStyles.tableHeader.text}>
            {columns.length} columns
          </span>
        )}
        {editable && (
          <span className={tableStyles.tableHeader.accent}>✏️ Editable</span>
        )}
        {isLoading && (
          <span className={tableStyles.tableHeader.spinner.container}>
            <div className={tableStyles.tableHeader.spinner.element}></div>
            Loading more...
          </span>
        )}
        {!hasMore && visibleRows > 0 && (
          <span className={tableStyles.tableHeader.muted}>All data loaded</span>
        )}
      </div>
    </>
  );
};
