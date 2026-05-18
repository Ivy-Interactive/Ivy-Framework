import React, { useEffect, useState } from "react";
import { sanitizeUrl, sanitizeId, loadScript } from "./shared";
import EmbedErrorFallback from "./EmbedErrorFallback";

interface TikTokEmbedProps {
  url: string;
}

const TikTokEmbed: React.FC<TikTokEmbedProps> = ({ url }) => {
  const [scriptState, setScriptState] = useState<"loading" | "loaded" | "error">("loading");

  const videoId = React.useMemo(() => {
    // TikTok video URL: https://www.tiktok.com/@username/video/1234567890
    const match = url.match(/tiktok\.com\/@[^/]+\/video\/(\d+)/);
    return match ? sanitizeId(match[1]) : null;
  }, [url]);

  const loadWidgetScript = React.useCallback(() => {
    if (videoId) {
      loadScript("https://www.tiktok.com/embed.js")
        .then(() => {
          setScriptState("loaded");
        })
        .catch(() => {
          setScriptState("error");
        });
    }
  }, [videoId]);

  useEffect(() => {
    loadWidgetScript();
  }, [loadWidgetScript]);

  const sanitizedUrl = sanitizeUrl(url);
  if (!videoId || scriptState === "error" || !sanitizedUrl) {
    return <EmbedErrorFallback url={url} platform="TikTok" />;
  }

  return (
    <div className="tiktok-embed">
      <blockquote
        className="tiktok-embed w-full h-96 sm:h-[500px] md:h-[600px] border-0 rounded-lg shadow-md"
        cite={sanitizedUrl}
        data-video-id={videoId}
        style={{ maxWidth: "605px", minWidth: "325px" }}
      >
        <section>
          <a href={sanitizedUrl} target="_blank" rel="noopener noreferrer" title="@tiktok">
            {scriptState === "loaded" ? "Loading TikTok video..." : "Loading script..."}
          </a>
        </section>
      </blockquote>
    </div>
  );
};

export default TikTokEmbed;
