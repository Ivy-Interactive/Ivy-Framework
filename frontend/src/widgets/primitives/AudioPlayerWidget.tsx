import React, { useState } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { getIvyHost } from '@/lib/utils';

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

const getAudioUrl = (url: string): string => {
  if (
    url.startsWith('http://') ||
    url.startsWith('https://') ||
    url.startsWith('data:')
  ) {
    return url;
  }

  return `${getIvyHost()}${url.startsWith('/') ? '' : '/'}${url}`;
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

  if (!src) {
    return (
      <div
        key={id}
        style={styles}
        className="flex items-center justify-center bg-muted text-muted-foreground rounded border-2 border-dashed border-muted-foreground/25 p-4"
        role="alert"
        aria-label="Audio player error"
      >
        <span className="text-sm">No audio source provided</span>
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
      src={getAudioUrl(src)}
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
