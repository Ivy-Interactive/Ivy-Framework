import React, { useEffect, useState } from "react";
import { sanitizeUrl, sanitizeId, loadScript } from "./shared";
import EmbedErrorFallback from "./EmbedErrorFallback";

interface TwitterEmbedProps {
  url: string;
}

const TwitterEmbed: React.FC<TwitterEmbedProps> = ({ url }) => {
  const [scriptState, setScriptState] = useState<"loading" | "loaded" | "error">("loading");

  const tweetId = React.useMemo(() => {
    // Support both twitter.com and x.com URLs, with or without @ prefix
    const match = url.match(/(?:twitter\.com|x\.com)\/\w+\/status\/(\d+)/);
    return match ? sanitizeId(match[1]) : null;
  }, [url]);

  const loadWidgetScript = React.useCallback(() => {
    if (tweetId) {
      loadScript("https://platform.twitter.com/widgets.js")
        .then(() => {
          setScriptState("loaded");
        })
        .catch(() => {
          setScriptState("error");
        });
    }
  }, [tweetId]);

  useEffect(() => {
    loadWidgetScript();
  }, [loadWidgetScript]);

  const sanitizedUrl = sanitizeUrl(url);
  if (!tweetId || scriptState === "error" || !sanitizedUrl) {
    return <EmbedErrorFallback url={url} platform="Twitter" />;
  }

  return (
    <div className="twitter-embed w-full">
      <blockquote className="twitter-tweet w-full" data-tweet-id={tweetId} data-theme="light">
        <a href={sanitizedUrl} target="_blank" rel="noopener noreferrer">
          {scriptState === "loaded" ? "Loading Twitter post..." : "Loading script..."}
        </a>
      </blockquote>
    </div>
  );
};

export default TwitterEmbed;
