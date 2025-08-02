import React from 'react';
import { tableStyles } from '../styles';

interface ErrorDisplayProps {
  error: string;
}

export const ErrorDisplay: React.FC<ErrorDisplayProps> = ({ error }) => {
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
