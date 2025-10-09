import React, { useRef, useState, useEffect } from 'react';
import Icon from '@/components/Icon';
import { sanitizeUrl } from './shared';

interface EmbedCardProps {
  platform: string;
  iconName: string;
  iconColor: string;
  title: string;
  description: string;
  url: string;
  linkText: string;
}

const EmbedCard: React.FC<EmbedCardProps> = ({
  platform,
  iconName,
  iconColor,
  title,
  description,
  url,
  linkText,
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const [isNarrow, setIsNarrow] = useState(false);
  const sanitizedUrl = sanitizeUrl(url);

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

  if (!sanitizedUrl) {
    return <div>Invalid {platform} URL.</div>;
  }

  return (
    <div
      ref={containerRef}
      className={`${platform.toLowerCase()}-embed border rounded-lg p-4 bg-card shadow-sm w-full min-w-0 overflow-hidden`}
    >
      {!isNarrow ? (
        // Normal width: Original horizontal layout
        <div className="flex items-center space-x-3">
          <div className="flex-shrink-0">
            <Icon name={iconName} size={32} className={iconColor} />
          </div>
          <div className="flex-1 min-w-0">
            <h3 className="text-lg font-semibold text-card-foreground truncate">
              {title}
            </h3>
            <p className="text-sm text-muted-foreground truncate">
              {description}
            </p>
          </div>
          <div className="flex-shrink-0">
            <a
              href={sanitizedUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center px-3 py-2 border shadow-sm text-sm font-medium rounded-md text-card-foreground bg-card hover:bg-accent focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary"
            >
              {linkText}
            </a>
          </div>
        </div>
      ) : (
        // Small width: Horizontal compact button matching the image
        <div className="-m-4">
          <a
            href={sanitizedUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="flex items-center gap-3 w-full px-4 py-3 bg-card hover:bg-accent text-card-foreground rounded-lg transition-all duration-200 cursor-pointer focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-white"
          >
            <div className="flex-shrink-0">
              <Icon name={iconName} size={20} className={iconColor} />
            </div>
            <span className="text-sm font-medium flex-1 text-left truncate">
              {linkText}
            </span>
          </a>
        </div>
      )}
    </div>
  );
};

export default EmbedCard;
