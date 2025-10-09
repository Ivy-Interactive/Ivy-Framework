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
      className={`${platform.toLowerCase()}-embed border rounded-lg p-4 bg-card shadow-sm`}
      style={{
        width: '100%',
        maxWidth: '100%',
        minWidth: 0,
        boxSizing: 'border-box',
        overflow: 'hidden',
      }}
    >
      <div
        className="flex items-start space-x-3"
        style={{ minWidth: 0, width: '100%' }}
      >
        <div className="flex-shrink-0">
          <Icon name={iconName} size={32} className={iconColor} />
        </div>
        <div
          className="flex-1 overflow-hidden"
          style={{ minWidth: 0, maxWidth: '100%' }}
        >
          <h3
            className="text-lg font-semibold text-card-foreground"
            style={{
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
              width: '100%',
            }}
          >
            {title}
          </h3>
          <p
            className="text-sm text-muted-foreground"
            style={{
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
              width: '100%',
            }}
          >
            {description}
          </p>
        </div>
      </div>
      <div className="mt-3" style={{ width: '100%' }}>
        <a
          href={sanitizedUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex items-center justify-center px-3 py-2 border shadow-sm text-sm font-medium rounded-md text-card-foreground bg-card hover:bg-accent focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary"
          style={{
            width: '100%',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
          }}
        >
          {linkText}
        </a>
      </div>
    </div>
  );
};

export default EmbedCard;
