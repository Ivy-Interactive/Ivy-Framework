import React from 'react';

interface EmbedErrorFallbackProps {
  url: string;
  platform?: string;
}

const EmbedErrorFallback: React.FC<EmbedErrorFallbackProps> = ({
  url,
  platform,
}) => {
  const getErrorTitle = () => {
    if (platform === 'Unsupported') {
      return 'Unsupported URL';
    }
    if (platform) {
      return `${platform} Embed Error`;
    }
    return 'Embed Error';
  };

  const getErrorDescription = () => {
    if (platform === 'Unsupported') {
      return 'This URL is not supported for embedding. Please visit the link directly.';
    }
    return 'Failed to load embed. Please try again or visit the link directly.';
  };

  const getLinkText = () => {
    if (platform === 'Unsupported') {
      return 'Visit Link';
    }
    return 'View Original';
  };

  return (
    <div className="embed-error border rounded-lg bg-card shadow-sm w-full @container">
      {/* Button layout - shown when container is wide enough */}
      <div className="@[30rem]:flex hidden items-center gap-3 p-2">
        <div className="flex-shrink-0">
          <div className="w-8 h-8 rounded-full bg-destructive flex items-center justify-center">
            <span className="text-destructive-foreground text-sm font-bold">
              !
            </span>
          </div>
        </div>
        <div className="flex-1 min-w-0">
          <h3 className="text-lg font-semibold text-card-foreground line-clamp-1">
            {getErrorTitle()}
          </h3>
          <p className="text-sm text-muted-foreground line-clamp-1">
            {getErrorDescription()}
          </p>
        </div>
        <div className="flex-shrink-0">
          <a
            href={url}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center px-3 py-2 border shadow-sm text-sm font-medium rounded-md text-card-foreground bg-card hover:bg-accent focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary whitespace-nowrap"
          >
            {getLinkText()}
          </a>
        </div>
      </div>

      {/* Clickable card layout - shown when container is too narrow */}
      <a
        href={url}
        target="_blank"
        rel="noopener noreferrer"
        className="@[30rem]:hidden flex items-center gap-3 p-2 hover:bg-accent/5 transition-colors rounded-lg focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary"
      >
        <div className="flex-shrink-0">
          <div className="w-6 h-6 rounded-full bg-destructive flex items-center justify-center">
            <span className="text-destructive-foreground text-xs font-bold">
              !
            </span>
          </div>
        </div>
        <div className="flex-1 min-w-0">
          <div className="text-sm font-medium text-card-foreground line-clamp-2">
            {getLinkText()}
          </div>
        </div>
      </a>
    </div>
  );
};

export default EmbedErrorFallback;
