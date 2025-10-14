import React from 'react';

interface EmbedErrorFallbackProps {
  url: string;
  platform?: string;
}

const EmbedErrorFallback: React.FC<EmbedErrorFallbackProps> = ({
  url,
  platform,
}) => {
  return (
    <div className="embed-error border rounded-lg p-4 bg-card shadow-sm">
      <div className="flex items-center space-x-3">
        <div className="flex-1 min-w-0">
          <h3 className="text-lg font-semibold text-card-foreground truncate">
            {platform === 'Unsupported'
              ? 'Unsupported URL'
              : platform
                ? `${platform} Embed Error`
                : 'Embed Error'}
          </h3>
          <p className="text-sm text-muted-foreground">
            {platform === 'Unsupported'
              ? 'This URL is not supported for embedding. Please visit the link directly.'
              : 'Failed to load embed. Please try again or visit the link directly.'}
          </p>
        </div>
        <div className="flex-shrink-0">
          <a
            href={url}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center px-3 py-2 border shadow-sm text-sm font-medium rounded-md text-card-foreground bg-card hover:bg-accent focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary"
          >
            View Original
          </a>
        </div>
      </div>
    </div>
  );
};

export default EmbedErrorFallback;
