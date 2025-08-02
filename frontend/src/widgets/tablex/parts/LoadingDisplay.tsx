import React from 'react';
import { tableStyles } from '../styles';

interface LoadingDisplayProps {
  isLoading: boolean;
}

export const LoadingDisplay: React.FC<LoadingDisplayProps> = ({
  isLoading,
}) => {
  return (
    <div className={tableStyles.noDataContainer}>
      {isLoading ? 'Loading data...' : 'No data available'}
    </div>
  );
};
