import { useRef, useState } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { validateImageUrl } from '@/lib/url';
import { VideoPlayerError } from './VideoPlayerError';
import type { VideoPlayerProps } from './types';

export const NativeVideoPlayer: React.FC<VideoPlayerProps> = ({
  id,
  validatedSrc,
  poster,
  width,
  height,
  autoplay = false,
  loop = false,
  muted = false,
  preload = 'metadata',
  controls = true,
  onError,
}) => {
  const [hasError, setHasError] = useState(false);
  const videoRef = useRef<HTMLVideoElement>(null);
  const validatedPoster = poster ? validateImageUrl(poster) : null;

  const handleError = () => {
    setHasError(true);
    onError?.();
  };

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  if (hasError) {
    return (
      <VideoPlayerError
        id={id}
        message="Failed to load video file"
        width={width}
        height={height}
      />
    );
  }

  return (
    <video
      ref={videoRef}
      id={id}
      src={validatedSrc}
      style={styles}
      autoPlay={autoplay}
      loop={loop}
      muted={muted}
      preload={preload}
      controls={controls}
      poster={validatedPoster || undefined}
      className="w-full rounded"
      onError={handleError}
      aria-label="Video player"
    >
      Your browser does not support the video element.
    </video>
  );
};
