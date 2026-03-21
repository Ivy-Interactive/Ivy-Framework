import React, { useState, useEffect, useRef, useCallback } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { getIvyHost } from '@/lib/utils';
import {
  validateVideoUrl,
  validateImageUrl,
  isFullUrl,
  normalizeRelativePath,
  validateEmbedUrl,
} from '@/lib/url';
import { useEventHandler } from '@/components/event-handler';

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
  volume?: number; // 0.0 to 1.0
  startTime?: number; // playback start position in seconds
  endTime?: number; // playback stop position in seconds
  playbackRate?: number; // playback speed multiplier (0.25+)
  events?: string[];
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
  // Use validateEmbedUrl to properly validate hostname (prevents substring/subdomain attacks)
  return validateEmbedUrl(url) === 'youtube';
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
  volume,
  startTime,
  endTime,
  playbackRate,
  events = [],
}) => {
  const [hasError, setHasError] = useState(false);
  const videoRef = useRef<HTMLVideoElement>(null);
  const handleEvent = useEventHandler();
  const hasOnPlay = Array.isArray(events) && events.includes('OnPlay');
  const hasOnPause = Array.isArray(events) && events.includes('OnPause');
  const hasOnEnded = Array.isArray(events) && events.includes('OnEnded');
  const hasOnLoaded = Array.isArray(events) && events.includes('OnLoaded');

  const handlePlayEvent = useCallback(() => {
    if (hasOnPlay) handleEvent('OnPlay', id, []);
  }, [hasOnPlay, handleEvent, id]);

  const handlePauseEvent = useCallback(() => {
    if (hasOnPause) handleEvent('OnPause', id, []);
  }, [hasOnPause, handleEvent, id]);

  const handleEndedEvent = useCallback(() => {
    if (hasOnEnded) handleEvent('OnEnded', id, []);
  }, [hasOnEnded, handleEvent, id]);

  const handleLoadedEvent = useCallback(() => {
    // Re-apply playbackRate after media load (browser resets it to defaultPlaybackRate during load)
    if (videoRef.current && playbackRate != null) {
      videoRef.current.playbackRate = Math.max(0.25, playbackRate);
    }
    if (hasOnLoaded) handleEvent('OnLoaded', id, []);
  }, [hasOnLoaded, handleEvent, id, playbackRate]);

  useEffect(() => {
    if (videoRef.current && volume != null) {
      videoRef.current.volume = Math.max(0, Math.min(1, volume));
    }
  }, [volume]);

  // getVideoUrl handles null/undefined and validates the URL internally
  const validatedVideoSrc = getVideoUrl(source);

  const handleTimeUpdate = useCallback(() => {
    const video = videoRef.current;
    if (video && endTime != null && video.currentTime >= endTime) {
      video.pause();
      video.currentTime = endTime;
    }
  }, [endTime]);

  useEffect(() => {
    const video = videoRef.current;
    if (!video) return;
    if (startTime != null) {
      video.currentTime = startTime;
    }
  }, [startTime, validatedVideoSrc]);

  // Apply playbackRate via defaultPlaybackRate (persists across media loads)
  // and also set playbackRate directly for immediate effect
  useEffect(() => {
    if (videoRef.current && playbackRate != null) {
      const rate = Math.max(0.25, playbackRate);
      videoRef.current.defaultPlaybackRate = rate;
      videoRef.current.playbackRate = rate;
    }
  }, [playbackRate]);

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };
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
    const timeParam = parseInt(url.searchParams.get('t') ?? '', 10);
    const embedUrl = `https://www.youtube.com/embed/${videoId}`;
    const params = new URLSearchParams();
    const effectiveStart = startTime ?? (isNaN(timeParam) ? 0 : timeParam);
    params.append('start', effectiveStart.toString());
    if (endTime != null) {
      params.append('end', endTime.toString());
    }
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

  // Build src with Media Fragments URI for time range
  let videoSrc = validatedVideoSrc;
  if (startTime != null && endTime != null) {
    videoSrc = `${validatedVideoSrc}#t=${startTime},${endTime}`;
  } else if (startTime != null) {
    videoSrc = `${validatedVideoSrc}#t=${startTime}`;
  } else if (endTime != null) {
    videoSrc = `${validatedVideoSrc}#t=0,${endTime}`;
  }

  return (
    <video
      ref={videoRef}
      id={id}
      src={videoSrc}
      style={styles}
      autoPlay={autoplay}
      loop={loop}
      muted={muted}
      preload={preload}
      controls={controls}
      poster={validatedPoster || undefined}
      className="w-full rounded"
      onError={() => setHasError(true)}
      onTimeUpdate={endTime != null ? handleTimeUpdate : undefined}
      onPlay={handlePlayEvent}
      onPause={handlePauseEvent}
      onEnded={handleEndedEvent}
      onLoadedData={handleLoadedEvent}
      aria-label="Video player"
    >
      Your browser does not support the video element.
    </video>
  );
};
