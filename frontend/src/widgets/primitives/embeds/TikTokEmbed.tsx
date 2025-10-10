import React, { useEffect, useState } from 'react';
import { sanitizeUrl, sanitizeId, loadScript } from './shared';
import EmbedErrorFallback from './EmbedErrorFallback';

interface TikTokEmbedProps {
  url: string;
}

const TikTokEmbed: React.FC<TikTokEmbedProps> = ({ url }) => {
  const [videoId, setVideoId] = useState<string | null>(null);
  const [scriptLoaded, setScriptLoaded] = useState(false);
  const [scriptError, setScriptError] = useState(false);

  useEffect(() => {
    const extractVideoId = (tiktokUrl: string): string | null => {
      // TikTok video URL: https://www.tiktok.com/@username/video/1234567890
      const match = tiktokUrl.match(/tiktok\.com\/@[^/]+\/video\/(\d+)/);
      return match ? sanitizeId(match[1]) : null;
    };

    setVideoId(extractVideoId(url));
  }, [url]);

  useEffect(() => {
    if (videoId) {
      loadScript('https://www.tiktok.com/embed.js')
        .then(() => {
          setScriptLoaded(true);
        })
        .catch(() => {
          setScriptError(true);
        });
    }
  }, [videoId]);

  const sanitizedUrl = sanitizeUrl(url);
  if (!videoId || scriptError || !sanitizedUrl) {
    return <EmbedErrorFallback url={url} platform="TikTok" />;
  }

  return (
    <div className="tiktok-embed">
      <blockquote
        className="tiktok-embed w-full h-96 sm:h-[500px] md:h-[600px] border-0 rounded-lg shadow-md"
        cite={sanitizedUrl}
        data-video-id={videoId}
        style={{ maxWidth: '605px', minWidth: '325px' }}
      >
        <section>
          <a
            href={sanitizedUrl}
            target="_blank"
            rel="noopener noreferrer"
            title="@tiktok"
          >
            {scriptLoaded ? 'Loading TikTok video...' : 'Loading script...'}
          </a>
        </section>
      </blockquote>
    </div>
  );
};

export default TikTokEmbed;
