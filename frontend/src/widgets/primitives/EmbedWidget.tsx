import React, { useEffect, useState } from "react";
import {
  FacebookEmbed,
  InstagramEmbed,
  TikTokEmbed,
  XEmbed,
  LinkedInEmbed,
  PinterestEmbed,
} from "react-social-media-embed";

interface EmbedWidgetProps {
  url: string;
}

interface YouTubeEmbedProps {
  url: string;
  width?: string | number;
  height?: string | number;
}

const YouTubeEmbed: React.FC<YouTubeEmbedProps> = ({
  url,
  width = "100%",
  height = "100%",
}) => {
  const [videoId, setVideoId] = useState<string | null>(null);

  useEffect(() => {
    const extractVideoId = (ytUrl: string): string | null => {
      let match = ytUrl.match(/youtube\.com\/watch\?v=([^&]+)/);
      if (match) return match[1];
      match = ytUrl.match(/youtu\.be\/([^?]+)/);
      if (match) return match[1];
      match = ytUrl.match(/youtube\.com\/embed\/([^?]+)/);
      if (match) return match[1];

      return null;
    };

    setVideoId(extractVideoId(url));
  }, [url]);

  if (!videoId) {
    return <div>Invalid YouTube URL.</div>;
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

const EmbedWidget: React.FC<EmbedWidgetProps> = ({ url }) => {
  if (url.includes("facebook.com")) {
    return <FacebookEmbed url={url} />;
  }
  if (url.includes("instagram.com")) {
    return <InstagramEmbed url={url} />;
  }
  if (url.includes("tiktok.com")) {
    return <TikTokEmbed url={url} />;
  }
  if (url.includes("twitter.com") || url.includes("x.com")) {
    return <XEmbed url={url} />;
  }
  if (url.includes("youtube.com") || url.includes("youtu.be")) {
    return (
      <div className="relative w-full pt-[56.25%]">
        <div className="absolute top-0 left-0 w-full h-full">
          <YouTubeEmbed url={url} width="100%" height="100%" />
        </div>
      </div>
    );
  }
  if (url.includes("linkedin.com")) {
    return <LinkedInEmbed url={url} />;
  }
  if (url.includes("pinterest.com")) {
    return <PinterestEmbed url={url} />;
  }

  return <p>The provided URL is not supported for embedding.</p>;
};

export default EmbedWidget;
