import React from 'react';
import { useTable } from '../context/TableContext';
import { tableStyles } from '../styles/style';

export const LoadingDisplay: React.FC = () => {
  const { isLoading } = useTable();
  return (
    <div className={tableStyles.loadingDisplay.container}>
      {isLoading ? 'Loading data...' : 'No data available'}
    </div>
  );
};
