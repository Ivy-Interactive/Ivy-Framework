import React from 'react';
import { useGlideDataGrid } from '../hooks/useGlideDataGrid';
import { classNames } from '../styles';
import { ConnectionAlert } from './ConnectionAlert';
import { ErrorAlert } from './ErrorAlert';
import { LoadingSpinner } from './LoadingSpinner';

export function GridContainer({ children }: { children: React.ReactNode }) {
  const { data, loading, error, isValidConnection } = useGlideDataGrid();

  if (!isValidConnection) {
    return <ConnectionAlert />;
  }

  const renderChildren = () => {
    if (error) return <ErrorAlert error={error} />;
    if (loading) return <LoadingSpinner />;
    if (data) return children;
    return <div className={classNames.container.empty}>No data available</div>;
  };

  return <div className={classNames.container.content}>{renderChildren()}</div>;
}
