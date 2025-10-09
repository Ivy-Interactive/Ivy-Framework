import React, { useRef, useState, useEffect } from 'react';

interface EmbedErrorFallbackProps {
  url: string;
  platform?: string;
}

const EmbedErrorFallback: React.FC<EmbedErrorFallbackProps> = ({
  url,
  platform,
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const [isNarrow, setIsNarrow] = useState(false);

  useEffect(() => {
    const checkWidth = () => {
      if (containerRef.current) {
        const width = containerRef.current.offsetWidth;
        // If container is less than 400px wide, show compact version
        setIsNarrow(width < 400);
      }
    };

    checkWidth();

    const resizeObserver = new ResizeObserver(checkWidth);
    if (containerRef.current) {
      resizeObserver.observe(containerRef.current);
    }

    return () => {
      resizeObserver.disconnect();
    };
  }, []);

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

  return (
    <div
      ref={containerRef}
      className="embed-error border rounded-lg p-4 bg-card shadow-sm w-full min-w-0 overflow-hidden"
    >
      {!isNarrow ? (
        // Normal width: Original horizontal layout
        <div className="flex items-center space-x-3">
          <div className="flex-1 min-w-0">
            <h3 className="text-lg font-semibold text-card-foreground truncate">
              {getErrorTitle()}
            </h3>
            <p className="text-sm text-muted-foreground">
              {getErrorDescription()}
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
      ) : (
        // Small width: Compact button style
        <div className="-m-4">
          <a
            href={url}
            target="_blank"
            rel="noopener noreferrer"
            className="flex items-center gap-3 w-full px-4 py-3 bg-card hover:bg-accent text-card-foreground rounded-lg transition-all duration-200 cursor-pointer focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary"
          >
            <div className="flex-shrink-0">
              <div className="w-5 h-5 rounded-full bg-red flex items-center justify-center">
                <span className="text-white text-xs font-bold">!</span>
              </div>
            </div>
            <div className="flex-1 min-w-0">
              <span className="text-sm font-medium block truncate">
                {getErrorTitle()}
              </span>
              <span className="text-xs text-muted-foreground block truncate">
                {getErrorDescription()}
              </span>
            </div>
          </a>
        </div>
      )}
    </div>
  );
};

export default EmbedErrorFallback;
