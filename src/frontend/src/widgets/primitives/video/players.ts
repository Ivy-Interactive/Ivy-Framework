import type { ComponentType } from 'react';
import type { VideoPlayerProps, VideoType } from './types';
import { NativeVideoPlayer } from './NativeVideoPlayer';
import { YouTubePlayer } from './YouTubePlayer';

export const PLAYERS: Record<VideoType, ComponentType<VideoPlayerProps>> = {
  native: NativeVideoPlayer,
  youtube: YouTubePlayer,
};
