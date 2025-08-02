import React from 'react';
import { useTable } from '../context/TableContext';
import { tableStyles } from '../styles/style';

export const LoadingDisplay: React.FC = () => {
  const { isLoading } = useTable();
  return (
    <div className={tableStyles.flex.center}>
      {isLoading ? 'Loading data...' : 'No data available'}
    </div>
  );
};
