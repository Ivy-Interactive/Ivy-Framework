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
}) => {
  const [hasError, setHasError] = useState(false);

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
      preload={preload}
      controls={controls}
      className="w-full"
      onError={() => setHasError(true)}
      aria-label="Audio player"
      role="application"
    >
      Your browser does not support the audio element.
    </audio>
  );
};
