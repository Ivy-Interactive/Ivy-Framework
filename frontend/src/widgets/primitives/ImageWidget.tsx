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
    return (
      <div
        key={id}
        style={styles}
        className="flex items-center justify-center bg-muted text-muted-foreground rounded border-2 border-dashed border-muted-foreground/25 p-4"
        role="alert"
        aria-label="Image error"
      >
        <span className="text-sm">No image source provided</span>
      </div>
    );
  }

  // Validate and sanitize image URL to prevent open redirect vulnerabilities
  const validatedImageSrc = getImageUrl(src);
  if (!validatedImageSrc) {
    // Invalid URL, show error message
    return (
      <div
        key={id}
        style={styles}
        className="flex items-center justify-center bg-destructive/10 text-destructive rounded border-2 border-dashed border-destructive/25 p-4"
        role="alert"
        aria-label="Invalid image URL"
      >
        <span className="text-sm">Invalid image URL</span>
      </div>
    );
  }

  // Validate the final constructed URL to ensure it's safe
  const finalValidatedSrc = validateImageUrl(validatedImageSrc);
  if (!finalValidatedSrc) {
    // Invalid constructed URL, show error message
    return (
      <div
        key={id}
        style={styles}
        className="flex items-center justify-center bg-destructive/10 text-destructive rounded border-2 border-dashed border-destructive/25 p-4"
        role="alert"
        aria-label="Invalid image URL"
      >
        <span className="text-sm">Invalid image URL</span>
      </div>
    );
  }

  return <img src={finalValidatedSrc} key={id} style={styles} />;
};
