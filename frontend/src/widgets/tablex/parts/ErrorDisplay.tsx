import React from 'react';
import { tableStyles } from '../styles';
import { useTable } from '../context/TableContext';

export const ErrorDisplay: React.FC = () => {
  const { error } = useTable();
  return (
    <div className={tableStyles.padding.container}>
      <div className={tableStyles.text.error}>Error: {error}</div>
      <button
        onClick={() => window.location.reload()}
        className={tableStyles.button.primary}
      >
        Retry
      </button>
    </div>
  );
};
