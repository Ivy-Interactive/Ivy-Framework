import React, { useEffect, useState } from 'react';
import { sanitizeId } from './shared';
import EmbedErrorFallback from './EmbedErrorFallback';

interface YouTubeEmbedProps {
  url: string;
  width: string;
  height: string;
}

const YouTubeEmbed: React.FC<YouTubeEmbedProps> = ({ url, width, height }) => {
  const [videoId, setVideoId] = useState<string | null>(null);

  useEffect(() => {
    const extractVideoId = (youtubeUrl: string): string | null => {
      const patterns = [
        /(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([^&\n?#]+)/,
        /youtube\.com\/watch\?.*v=([^&\n?#]+)/,
      ];

      for (const pattern of patterns) {
        const match = youtubeUrl.match(pattern);
        if (match) {
          return sanitizeId(match[1]);
        }
      }
      return null;
    };

    setVideoId(extractVideoId(url));
  }, [url]);

  if (!videoId) {
    return <EmbedErrorFallback url={url} platform="YouTube" />;
  }

  const embedUrl = `https://www.youtube.com/embed/${videoId}`;

  return (
    <iframe
      src={embedUrl}
      width={width}
      height={height}
      allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
      allowFullScreen
      title="YouTube"
      className="border-0"
    />
  );
};

export default YouTubeEmbed;
