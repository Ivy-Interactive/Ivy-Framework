import React from 'react';
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
  const sanitizedUrl = sanitizeUrl(url);

  if (!sanitizedUrl) {
    return <div>Invalid {platform} URL.</div>;
  }

  return (
    <div
      className={`${platform.toLowerCase()}-embed border rounded-lg p-4 bg-card shadow-sm w-full min-w-0 overflow-hidden`}
    >
      <div className="flex items-start space-x-3 w-full min-w-0">
        <div className="flex-shrink-0">
          <Icon name={iconName} size={32} className={iconColor} />
        </div>
        <div className="flex-1 min-w-0 overflow-hidden">
          <h3 className="text-lg font-semibold text-card-foreground truncate">
            {title}
          </h3>
          <p className="text-sm text-muted-foreground truncate">
            {description}
          </p>
        </div>
      </div>
      <div className="mt-3 w-full">
        <a
          href={sanitizedUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex items-center justify-center w-full px-3 py-2 border shadow-sm text-sm font-medium rounded-md text-card-foreground bg-card hover:bg-accent focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary truncate"
        >
          {linkText}
        </a>
      </div>
    </div>
  );
};

export default EmbedCard;
