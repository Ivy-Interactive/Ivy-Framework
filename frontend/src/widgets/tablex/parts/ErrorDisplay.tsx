import React from 'react';
import { tableStyles } from '../styles';
import { useTable } from '../context/TableContext';

export const ErrorDisplay: React.FC = () => {
  const { error } = useTable();
  return (
    <div className={tableStyles.errorContainer}>
      <div className={tableStyles.errorText}>Error: {error}</div>
      <button
        onClick={() => window.location.reload()}
        className={tableStyles.retryButton}
      >
        Retry
      </button>
    </div>
  );
};
