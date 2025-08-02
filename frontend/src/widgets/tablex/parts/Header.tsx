import React from 'react';
import { tableStyles } from '../styles';

interface HeaderProps {
  visibleRows: number;
  columnsLength: number;
  editable: boolean;
  isLoading: boolean;
  hasMore: boolean;
}

export const Header: React.FC<HeaderProps> = ({
  visibleRows,
  columnsLength,
  editable,
  isLoading,
  hasMore,
}) => {
  return (
    <>
      <h1 className={tableStyles.title}>Dynamic Data Grid with gRPC</h1>
      <div className={tableStyles.infoBar}>
        <span>Showing {visibleRows} rows</span>
        {columnsLength > 0 && <span>{columnsLength} columns</span>}
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
