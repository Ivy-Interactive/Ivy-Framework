import React, { useState } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { getIvyHost } from '@/lib/utils';
import {
  validateVideoUrl,
  validateImageUrl,
  isFullUrl,
  normalizeRelativePath,
} from '@/lib/urlValidation';

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

const getVideoUrl = (url: string | undefined | null): string | null => {
  if (!url) return null;

  // Validate and sanitize video URL to prevent open redirect vulnerabilities
  const validatedUrl = validateVideoUrl(url);
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

  // getVideoUrl handles null/undefined and validates the URL internally
  const validatedVideoSrc = getVideoUrl(source);
  if (!validatedVideoSrc) {
    // Show error message for missing or invalid URLs
    return (
      <div
        id={id}
        style={styles}
        className="flex items-center justify-center bg-destructive/10 text-destructive rounded border-2 border-dashed border-destructive/25 p-4"
        role="alert"
        aria-label="Invalid video URL"
      >
        <span className="text-sm">
          {!source ? 'No video source provided' : 'Invalid video URL'}
        </span>
      </div>
    );
  }

  // Validate poster URL if provided
  const validatedPoster = poster ? validateImageUrl(poster) : null;

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

  if (isYouTube(validatedVideoSrc)) {
    const url = new URL(validatedVideoSrc);
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
      src={validatedVideoSrc}
      style={styles}
      autoPlay={autoplay}
      loop={loop}
      muted={muted}
      preload={preload}
      controls={controls}
      poster={validatedPoster || undefined}
      className="w-full rounded"
      onError={() => setHasError(true)}
      aria-label="Video player"
    >
      Your browser does not support the video element.
    </video>
  );
};
