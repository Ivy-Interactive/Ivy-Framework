import React, { useState } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { getIvyHost } from '@/lib/utils';
import {
  validateAudioUrl,
  isFullUrl,
  normalizeRelativePath,
} from '@/lib/urlValidation';

interface AudioPlayerWidgetProps {
  id: string;
  src: string | undefined | null;
  width?: string;
  height?: string;
  autoplay?: boolean;
  loop?: boolean;
  muted?: boolean;
  preload?: 'none' | 'metadata' | 'auto';
  controls?: boolean;
  'data-testid'?: string;
}

const getAudioUrl = (url: string | undefined | null): string | null => {
  if (!url) return null;

  // Validate and sanitize audio URL to prevent open redirect vulnerabilities
  const validatedUrl = validateAudioUrl(url);
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

export const AudioPlayerWidget: React.FC<AudioPlayerWidgetProps> = ({
  id,
  src,
  width,
  height,
  autoplay = false,
  loop = false,
  muted = false,
  preload = 'metadata',
  controls = true,
  'data-testid': dataTestId,
}) => {
  const [hasError, setHasError] = useState(false);

  // Normalize preload to lowercase for HTML5 compliance
  const normalizedPreload = preload?.toLowerCase() as
    | 'none'
    | 'metadata'
    | 'auto';

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  // getAudioUrl handles null/undefined and validates the URL internally
  const validatedAudioSrc = getAudioUrl(src);
  if (!validatedAudioSrc) {
    // Show error message for missing or invalid URLs
    return (
      <div
        key={id}
        style={styles}
        className="flex items-center justify-center bg-destructive/10 text-destructive rounded border-2 border-dashed border-destructive/25 p-4"
        role="alert"
        aria-label="Invalid audio URL"
      >
        <span className="text-sm">
          {!src ? 'No audio source provided' : 'Invalid audio URL'}
        </span>
      </div>
    );
  }

  if (hasError) {
    return (
      <div
        key={id}
        style={styles}
        className="flex items-center justify-center bg-destructive/10 text-destructive rounded border-2 border-dashed border-destructive/25 p-4"
        role="alert"
        aria-label="Audio loading error"
      >
        <span className="text-sm">Failed to load audio file</span>
      </div>
    );
  }

  return (
    <audio
      key={id}
      src={validatedAudioSrc}
      style={styles}
      autoPlay={autoplay}
      loop={loop}
      muted={muted}
      preload={normalizedPreload}
      controls={controls}
      className="w-full"
      onError={() => setHasError(true)}
      aria-label="Audio player"
      role="application"
      data-testid={dataTestId}
    >
      Your browser does not support the audio element.
    </audio>
  );
};
