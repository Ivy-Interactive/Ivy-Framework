import { getHeight, getWidth } from '@/lib/styles';
import { getIvyHost, validateImageUrl } from '@/lib/utils';
import React from 'react';

interface ImageWidgetProps {
  id: string;
  src: string | undefined | null;
  width?: string;
  height?: string;
}

const getImageUrl = (url: string | undefined | null): string | null => {
  if (!url) return null;

  // Validate and sanitize image URL to prevent open redirect vulnerabilities
  const validatedUrl = validateImageUrl(url);
  if (!validatedUrl) {
    // Invalid URL, return null
    return null;
  }

  // If it's already a full URL (http/https/data/blob/app), return it
  if (validatedUrl.match(/^(https?:\/\/|data:|blob:|app:)/i)) {
    return validatedUrl;
  }

  // Construct relative URL with Ivy host
  return `${getIvyHost()}${validatedUrl.startsWith('/') ? '' : '/'}${validatedUrl}`;
};

export const ImageWidget: React.FC<ImageWidgetProps> = ({
  id,
  src,
  width,
  height,
}) => {
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  if (!src) {
    return null;
  }

  // Validate and sanitize image URL to prevent open redirect vulnerabilities
  const validatedImageSrc = getImageUrl(src);
  if (!validatedImageSrc) {
    // Invalid URL, don't render image
    return null;
  }

  // Validate the final constructed URL to ensure it's safe
  const finalValidatedSrc = validateImageUrl(validatedImageSrc);
  if (!finalValidatedSrc) {
    // Invalid constructed URL, don't render image
    return null;
  }

  return <img src={finalValidatedSrc} key={id} style={styles} />;
};
