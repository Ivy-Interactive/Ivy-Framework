import React, { useState } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { getIvyHost } from '@/lib/utils';

interface VideoPlayerWidgetProps {
  id: string;
  source: string | undefined | null;
  width?: string;
  height?: string;
  autoplay?: boolean;
  loop?: boolean;
  muted?: boolean;
  preload?: 'none' | 'metadata' | 'auto';
  controls?: boolean;
  poster?: string; // optional preview image before playback
}

const getVideoUrl = (url: string): string => {
  if (
    url.startsWith('http://') ||
    url.startsWith('https://') ||
    url.startsWith('data:')
  ) {
    return url;
  }

  return `${getIvyHost()}${url.startsWith('/') ? '' : '/'}${url}`;
};

const isYouTube = (url: string): boolean => {
  try {
    const u = new URL(url);
    return (
      u.hostname.includes('youtube.com') || u.hostname.includes('youtu.be')
    );
  } catch {
    return false;
  }
};

export const VideoPlayerWidget: React.FC<VideoPlayerWidgetProps> = ({
  id,
  source,
  width,
  height,
  autoplay = false,
  loop = false,
  muted = false,
  preload = 'metadata',
  controls = true,
  poster,
}) => {
  const [hasError, setHasError] = useState(false);

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  if (!source) {
    return (
      <div
        id={id}
        style={styles}
        className="flex items-center justify-center bg-muted text-muted-foreground rounded border-2 border-dashed border-muted-foreground/25 p-4"
        role="alert"
        aria-label="Video player error"
      >
        <span className="text-sm">No video source provided</span>
      </div>
    );
  }

  if (hasError) {
    return (
      <div
        id={id}
        style={styles}
        className="flex items-center justify-center bg-destructive/10 text-destructive rounded border-2 border-dashed border-destructive/25 p-4"
        role="alert"
        aria-label="Video loading error"
      >
        <span className="text-sm">Failed to load video file</span>
      </div>
    );
  }

  if (isYouTube(source)) {
    const url = new URL(source);
    const videoId =
      url.searchParams.get('v') ??
      url.pathname.split('/').filter(Boolean).pop();
    const timeParam = parseInt(url.searchParams.get('t') ?? '', 10).toString();
    const embedUrl = `https://www.youtube.com/embed/${videoId}`;
    const params = new URLSearchParams();
    params.append('start', timeParam ?? '0');
    params.append('autoplay', autoplay ? '1' : '0');
    params.append('loop', loop ? '1' : '0');
    params.append('muted', muted ? '1' : '0');
    params.append('controls', controls ? '1' : '0');
    return (
      <iframe
        id={id}
        style={styles}
        src={`${embedUrl}?${params.toString()}`}
        title="YouTube video player"
        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
        allowFullScreen
        className="w-full rounded"
      ></iframe>
    );
  }

  return (
    <video
      id={id}
      src={getVideoUrl(source)}
      style={styles}
      autoPlay={autoplay}
      loop={loop}
      muted={muted}
      preload={preload}
      controls={controls}
      poster={poster}
      className="w-full rounded"
      onError={() => setHasError(true)}
      aria-label="Video player"
    >
      Your browser does not support the video element.
    </video>
  );
};
