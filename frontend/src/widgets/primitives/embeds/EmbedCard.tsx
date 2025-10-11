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
      className={`${platform.toLowerCase()}-embed border rounded-lg bg-card shadow-sm w-full @container`}
    >
      {/* Button layout - shown when container is wide enough */}
      <div className="@[30rem]:flex hidden items-center gap-3 p-2">
        <div className="flex-shrink-0">
          <Icon name={iconName} size={32} className={iconColor} />
        </div>
        <div className="flex-1 min-w-0">
          <h3 className="text-lg font-semibold text-card-foreground line-clamp-1">
            {title}
          </h3>
          <p className="text-sm text-muted-foreground line-clamp-1">
            {description}
          </p>
        </div>
        <div className="flex-shrink-0">
          <a
            href={sanitizedUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center px-3 py-2 border shadow-sm text-sm font-medium rounded-md text-card-foreground bg-card hover:bg-accent focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary whitespace-nowrap"
          >
            {linkText}
          </a>
        </div>
      </div>

      {/* Clickable card layout - shown when container is too narrow */}
      <a
        href={sanitizedUrl}
        target="_blank"
        rel="noopener noreferrer"
        className="@[30rem]:hidden flex items-center gap-3 p-2 hover:bg-accent transition-colors rounded-lg focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary"
      >
        <div className="flex-shrink-0">
          <Icon name={iconName} size={24} className={iconColor} />
        </div>
        <div className="flex-1 min-w-0">
          <div className="text-sm font-medium text-card-foreground line-clamp-2">
            {linkText}
          </div>
        </div>
      </a>
    </div>
  );
};

export default EmbedCard;
