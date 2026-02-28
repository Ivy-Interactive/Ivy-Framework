/**
 * Props passed to each video player implementation.
 */
export interface VideoPlayerProps {
  id: string;
  validatedSrc: string;
  source: string;
  width?: string;
  height?: string;
  autoplay?: boolean;
  loop?: boolean;
  muted?: boolean;
  preload?: 'none' | 'metadata' | 'auto';
  controls?: boolean;
  poster?: string | null;
  onError?: () => void;
}

export type VideoType = 'native' | 'youtube';
