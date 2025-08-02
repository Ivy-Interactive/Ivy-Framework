import React from 'react';
import { tableStyles } from '../styles';
import { useTable } from '../context/TableContext';

export const Header: React.FC = () => {
  const { visibleRows, columns, editable, isLoading, hasMore } = useTable();
  return (
    <>
      <h1 className={tableStyles.heading.primary}>
        Dynamic Data Grid with gRPC
      </h1>
      <div className={tableStyles.text.info}>
        <span>Showing {visibleRows} rows</span>
        {columns.length > 0 && <span>{columns.length} columns</span>}
        {editable && (
          <span className={tableStyles.text.accent}>✏️ Editable</span>
        )}
        {isLoading && (
          <span className={tableStyles.spinner.container}>
            <div className={tableStyles.spinner.element}></div>
            Loading more...
          </span>
        )}
        {!hasMore && visibleRows > 0 && (
          <span className={tableStyles.text.muted}>All data loaded</span>
        )}
      </div>
    </>
  );
};
