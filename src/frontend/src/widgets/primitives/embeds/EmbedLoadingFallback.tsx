import React from 'react';

const EmbedLoadingFallback: React.FC = () => (
  <div className="embed-loading border rounded-lg p-4 bg-card shadow-sm">
    <div className="flex items-center space-x-3">
      <div className="flex-shrink-0">
        <div className="w-8 h-8 bg-muted animate-pulse rounded"></div>
      </div>
      <div className="flex-1 min-w-0">
        <div className="h-4 bg-muted animate-pulse rounded w-1/3 mb-2"></div>
        <div className="h-3 bg-muted animate-pulse rounded w-1/2"></div>
      </div>
    </div>
  </div>
);

export default EmbedLoadingFallback;
