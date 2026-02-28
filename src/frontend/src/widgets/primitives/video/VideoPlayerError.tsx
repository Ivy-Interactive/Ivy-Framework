import { getHeight, getWidth } from '@/lib/styles';

export interface VideoPlayerErrorProps {
  id: string;
  message: string;
  width?: string;
  height?: string;
}

/**
 * Shared error state for the video player (invalid URL, load failure, etc.).
 */
export const VideoPlayerError: React.FC<VideoPlayerErrorProps> = ({
  id,
  message,
  width,
  height,
}) => {
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };
  return (
    <div
      id={id}
      style={styles}
      className="flex items-center justify-center bg-destructive/10 text-destructive rounded border-2 border-dashed border-destructive/25 p-4"
      role="alert"
      aria-label="Video error"
    >
      <span className="text-sm">{message}</span>
    </div>
  );
};
