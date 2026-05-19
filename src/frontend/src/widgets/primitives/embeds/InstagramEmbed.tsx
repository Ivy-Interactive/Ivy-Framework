import React, { useEffect, useState } from "react";
import { sanitizeUrl, sanitizeId, loadScript } from "./shared";
import EmbedErrorFallback from "./EmbedErrorFallback";

interface InstagramEmbedProps {
  url: string;
}

const InstagramEmbed: React.FC<InstagramEmbedProps> = ({ url }) => {
  const [scriptState, setScriptState] = useState<"loading" | "loaded" | "error">("loading");

  const postId = React.useMemo(() => {
    // Instagram post URL: https://www.instagram.com/p/ABC123/
    const match = url.match(/instagram\.com\/p\/([^/?]+)/);
    return match ? sanitizeId(match[1]) : null;
  }, [url]);

  const loadWidgetScript = React.useCallback(() => {
    if (postId) {
      loadScript("https://www.instagram.com/embed.js")
        .then(() => {
          setScriptState("loaded");
        })
        .catch(() => {
          setScriptState("error");
        });
    }
  }, [postId]);

  useEffect(() => {
    loadWidgetScript();
  }, [loadWidgetScript]);

  const sanitizedUrl = sanitizeUrl(url);
  if (!postId || scriptState === "error" || !sanitizedUrl) {
    return <EmbedErrorFallback url={url} platform="Instagram" />;
  }

  return (
    <div className="instagram-embed w-full">
      <blockquote
        className="instagram-media w-full"
        data-instgrm-captioned
        data-instgrm-permalink={sanitizedUrl}
        data-instgrm-version="14"
      >
        <a href={sanitizedUrl} target="_blank" rel="noopener noreferrer">
          {scriptState === "loaded" ? "Loading Instagram post..." : "Loading script..."}
        </a>
      </blockquote>
    </div>
  );
};

export default InstagramEmbed;
