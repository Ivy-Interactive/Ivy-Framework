import React from 'react';
import { sanitizeUrl } from './shared';
import EmbedCard from './EmbedCard';
import EmbedErrorFallback from './EmbedErrorFallback';

interface PinterestEmbedProps {
  url: string;
}

const PinterestEmbed: React.FC<PinterestEmbedProps> = ({ url }) => {
  const sanitizedUrl = sanitizeUrl(url);
  if (!sanitizedUrl) {
    return <EmbedErrorFallback url={url} platform="Pinterest" />;
  }

  return (
    <EmbedCard
      platform="Pinterest"
      iconName="Pinterest"
      iconColor="text-destructive"
      title="Pinterest Pin"
      description="View pin on Pinterest"
      url={url}
      linkText="View on Pinterest"
    />
  );
};

export default PinterestEmbed;
