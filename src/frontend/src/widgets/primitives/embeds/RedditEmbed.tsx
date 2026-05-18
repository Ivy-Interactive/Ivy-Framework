import React, { useEffect, useState } from "react";
import { sanitizeUrl, sanitizeId, loadScript } from "./shared";
import EmbedErrorFallback from "./EmbedErrorFallback";

interface RedditEmbedProps {
  url: string;
}

const RedditEmbed: React.FC<RedditEmbedProps> = ({ url }) => {
  const [scriptState, setScriptState] = useState<"loading" | "loaded" | "error">("loading");

  const postId = React.useMemo(() => {
    const match = url.match(/reddit\.com\/r\/[^/]+\/comments\/([^/]+)/);
    return match ? sanitizeId(match[1]) : null;
  }, [url]);

  const loadWidgetScript = React.useCallback(() => {
    if (postId) {
      loadScript("https://embed.redditmedia.com/widgets/platform.js")
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
    return <EmbedErrorFallback url={url} platform="Reddit" />;
  }

  return (
    <div className="redditwrapper">
      <blockquote className="reddit-card">
        <a href={sanitizedUrl}>
          <p>Posted by u/reddit</p>
          <p>{scriptState === "loaded" ? "Loading Reddit post..." : "Loading script..."}</p>
        </a>
      </blockquote>
    </div>
  );
};

export default RedditEmbed;
