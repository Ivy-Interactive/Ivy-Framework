import React from 'react';
import { sanitizeUrl, sanitizeText } from './shared';
import EmbedCard from './EmbedCard';
import EmbedErrorFallback from './EmbedErrorFallback';

interface FacebookEmbedProps {
  url: string;
}

const FacebookEmbed: React.FC<FacebookEmbedProps> = ({ url }) => {
  const sanitizedUrl = sanitizeUrl(url);
  if (!sanitizedUrl) {
    return <EmbedErrorFallback url={url} platform="Facebook" />;
  }

  const extractPageName = (facebookUrl: string): string => {
    // Extract page name from various Facebook URL formats
    const match = facebookUrl.match(/facebook\.com\/([^/?]+)/);
    if (match) {
      return sanitizeText(match[1]);
    }
    return 'Facebook';
  };

  const pageName = extractPageName(url);

  return (
    <EmbedCard
      platform="Facebook"
      iconName="Facebook"
      iconColor="text-info"
      title={pageName}
      description="View post on Facebook"
      url={url}
      linkText="View on Facebook"
    />
  );
};

export default FacebookEmbed;
