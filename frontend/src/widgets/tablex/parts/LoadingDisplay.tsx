import React from 'react';
import { tableStyles } from '../styles';
import { useTable } from '../context/TableContext';

export const LoadingDisplay: React.FC = () => {
  const { isLoading } = useTable();
  return (
    <div className={tableStyles.noDataContainer}>
      {isLoading ? 'Loading data...' : 'No data available'}
    </div>
  );
};
