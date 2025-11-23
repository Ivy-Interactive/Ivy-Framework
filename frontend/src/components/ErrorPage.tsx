import React from 'react';
import { ErrorDisplay } from '@/components/ErrorDisplay';

export const ErrorPage: React.FC = () => {
  // Read error info from meta tags
  const titleMeta = document.querySelector('meta[name="ivy-error-title"]');
  const messageMeta = document.querySelector('meta[name="ivy-error-message"]');

  const title = titleMeta?.getAttribute('content') || 'Unknown Error';
  const message = messageMeta?.getAttribute('content') || 'An error occurred';

  return (
    <div className="flex items-center justify-center min-h-screen bg-background p-4">
      <ErrorDisplay title={title} message={message} />
    </div>
  );
};
