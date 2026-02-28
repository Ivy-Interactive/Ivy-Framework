import { useEffect, useRef } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { loadYouTubeAPI } from '@/lib/youtube-api';
import type { VideoPlayerProps } from './types';

function getYouTubeVideoId(url: string): string | null {
  try {
    const urlObj = new URL(url);
    const v = urlObj.searchParams.get('v');
    if (v) return v;
    const segment = urlObj.pathname.split('/').filter(Boolean).pop();
    return segment || null;
  } catch {
    return null;
  }
}

export const YouTubePlayer: React.FC<VideoPlayerProps> = ({
  id,
  validatedSrc,
  width,
  height,
  autoplay = false,
  loop = false,
  muted = false,
  controls = true,
}: VideoPlayerProps) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const playerRef = useRef<YT.Player | null>(null);

  useEffect(() => {
    const videoId = getYouTubeVideoId(validatedSrc);
    if (!containerRef.current || !videoId) return;

    let cancelled = false;

    loadYouTubeAPI().then(() => {
      if (cancelled || !containerRef.current || !window.YT?.Player) return;

      const instance = new window.YT.Player(containerRef.current, {
        videoId,
        playerVars: {
          autoplay: autoplay ? 1 : 0,
          loop: loop ? 1 : 0,
          mute: muted ? 1 : 0,
          controls: controls ? 1 : 0,
          enablejsapi: 1,
        },
        events: {
          onReady: (event: YT.PlayerEvent) => {
            if (cancelled) return;
            playerRef.current = event.target;
          },
        },
      });
      playerRef.current = instance;
    });

    return () => {
      cancelled = true;
      if (playerRef.current?.destroy) {
        playerRef.current.destroy();
        playerRef.current = null;
      }
    };
  }, [validatedSrc, autoplay, loop, muted, controls]);

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <div
      id={id}
      ref={containerRef}
      style={styles}
      className="w-full rounded"
    />
  );
};
