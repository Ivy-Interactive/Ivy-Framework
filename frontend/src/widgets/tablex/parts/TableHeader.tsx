import React from 'react';
import { tableStyles } from '../styles';
import { useTable } from '../context/TableContext';

export const Header: React.FC = () => {
  const { visibleRows, columns, editable, isLoading, hasMore } = useTable();
  return (
    <>
      <h1 className={tableStyles.title}>Dynamic Data Grid with gRPC</h1>
      <div className={tableStyles.infoBar}>
        <span>Showing {visibleRows} rows</span>
        {columns.length > 0 && <span>{columns.length} columns</span>}
        {editable && (
          <span className={tableStyles.editableIndicator}>✏️ Editable</span>
        )}
        {isLoading && (
          <span className={tableStyles.loadingContainer}>
            <div className={tableStyles.spinner}></div>
            Loading more...
          </span>
        )}
        {!hasMore && visibleRows > 0 && (
          <span className={tableStyles.allDataLoaded}>All data loaded</span>
        )}
      </div>
    </>
  );
};
