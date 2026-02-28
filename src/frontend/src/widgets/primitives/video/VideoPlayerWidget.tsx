import {
  validateVideoUrl,
  validateEmbedUrl,
  isFullUrl,
  normalizeRelativePath,
} from '@/lib/url';
import { getIvyHost } from '@/lib/utils';
import type { VideoType } from './types';
import { PLAYERS } from './players';
import { VideoPlayerError } from './VideoPlayerError';

export interface VideoPlayerWidgetProps {
  id: string;
  source: string | undefined | null;
  width?: string;
  height?: string;
  autoplay?: boolean;
  loop?: boolean;
  muted?: boolean;
  preload?: 'none' | 'metadata' | 'auto';
  controls?: boolean;
  poster?: string | null;
}

const EMBED_TO_VIDEO_TYPE: Partial<Record<string, VideoType>> = {
  youtube: 'youtube',
};

function getValidatedVideoSrc(
  source: string | undefined | null
): string | null {
  if (!source) return null;
  const validated = validateVideoUrl(source);
  if (!validated) return null;
  if (isFullUrl(validated)) return validated;
  return `${getIvyHost()}${normalizeRelativePath(validated)}`;
}

export const VideoPlayerWidget: React.FC<VideoPlayerWidgetProps> = (
  props: VideoPlayerWidgetProps
) => {
  const { source, id, poster, ...rest } = props;
  const validatedVideoSrc = getValidatedVideoSrc(source);
  if (!validatedVideoSrc) {
    return (
      <VideoPlayerError
        id={id}
        message={!source ? 'No video source provided' : 'Invalid video URL'}
        width={props.width}
        height={props.height}
      />
    );
  }

  const embed = validateEmbedUrl(validatedVideoSrc);
  const type: VideoType = EMBED_TO_VIDEO_TYPE[embed ?? ''] ?? 'native';
  const Player = PLAYERS[type];
  return (
    <Player
      {...rest}
      id={id}
      source={validatedVideoSrc}
      validatedSrc={validatedVideoSrc}
      poster={poster ?? null}
    />
  );
};
