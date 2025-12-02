import { getHeight, getWidth } from '@/lib/styles';
import { getIvyHost } from '@/lib/utils';
import {
  validateImageUrl,
  isFullUrl,
  normalizeRelativePath,
} from '@/lib/urlValidation';
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
    return null;
  }

  // If it's already a full URL (http/https/data/blob/app), return it
  if (isFullUrl(validatedUrl)) {
    return validatedUrl;
  }

  // For relative paths, construct full URL with Ivy host
  // validatedUrl is already a safe relative path (starts with / or was normalized)
  const relativePath = normalizeRelativePath(validatedUrl);
  return `${getIvyHost()}${relativePath}`;
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

  // getImageUrl handles null/undefined and validates the URL internally
  const validatedImageSrc = getImageUrl(src);
  if (!validatedImageSrc) {
    // Show error message for missing or invalid URLs
    return (
      <div
        key={id}
        style={styles}
        className="flex items-center justify-center bg-destructive/10 text-destructive rounded border-2 border-dashed border-destructive/25 p-4"
        role="alert"
        aria-label="Invalid image URL"
      >
        <span className="text-sm">
          {!src ? 'No image source provided' : 'Invalid image URL'}
        </span>
      </div>
    );
  }

  return <img src={validatedImageSrc} key={id} style={styles} />;
};
