import React, { useEffect, useState } from 'react';

interface EmbedWidgetProps {
  url: string;
}

interface YouTubeEmbedProps {
  url: string;
  width?: string | number;
  height?: string | number;
}

interface TwitterEmbedProps {
  url: string;
}

interface FacebookEmbedProps {
  url: string;
}

interface InstagramEmbedProps {
  url: string;
}

interface TikTokEmbedProps {
  url: string;
}

interface LinkedInEmbedProps {
  url: string;
}

interface PinterestEmbedProps {
  url: string;
}

const YouTubeEmbed: React.FC<YouTubeEmbedProps> = ({
  url,
  width = '100%',
  height = '100%',
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

const TwitterEmbed: React.FC<TwitterEmbedProps> = ({ url }) => {
  const [tweetId, setTweetId] = useState<string | null>(null);

  useEffect(() => {
    const extractTweetId = (twitterUrl: string): string | null => {
      const match = twitterUrl.match(/twitter\.com\/\w+\/status\/(\d+)/);
      return match ? match[1] : null;
    };

    setTweetId(extractTweetId(url));
  }, [url]);

  if (!tweetId) {
    return <div>Invalid Twitter/X URL.</div>;
  }

  return (
    <div className="twitter-embed">
      <blockquote className="twitter-tweet" data-theme="light" data-dnt="true">
        <a href={url}></a>
      </blockquote>
      <script
        async
        src="https://platform.twitter.com/widgets.js"
        charSet="utf-8"
      ></script>
    </div>
  );
};

const FacebookEmbed: React.FC<FacebookEmbedProps> = ({ url }) => {
  return (
    <div
      className="fb-post"
      data-href={url}
      data-width="auto"
      data-show-text="true"
    >
      <blockquote cite={url} className="fb-xfbml-parse-ignore">
        <a href={url}>View on Facebook</a>
      </blockquote>
    </div>
  );
};

const InstagramEmbed: React.FC<InstagramEmbedProps> = ({ url }) => {
  const [postId, setPostId] = useState<string | null>(null);

  useEffect(() => {
    const extractPostId = (instagramUrl: string): string | null => {
      const match = instagramUrl.match(/instagram\.com\/p\/([^/]+)/);
      return match ? match[1] : null;
    };

    setPostId(extractPostId(url));
  }, [url]);

  if (!postId) {
    return <div>Invalid Instagram URL.</div>;
  }

  return (
    <div className="instagram-embed">
      <iframe
        src={`https://www.instagram.com/p/${postId}/embed/`}
        width="400"
        height="480"
        frameBorder="0"
        scrolling="no"
        allowTransparency={true}
        title="Instagram"
      />
    </div>
  );
};

const TikTokEmbed: React.FC<TikTokEmbedProps> = ({ url }) => {
  const [videoId, setVideoId] = useState<string | null>(null);

  useEffect(() => {
    const extractVideoId = (tiktokUrl: string): string | null => {
      const match = tiktokUrl.match(/tiktok\.com\/@[\w.-]+\/video\/(\d+)/);
      return match ? match[1] : null;
    };

    setVideoId(extractVideoId(url));
  }, [url]);

  if (!videoId) {
    return <div>Invalid TikTok URL.</div>;
  }

  return (
    <div className="tiktok-embed">
      <blockquote className="tiktok-embed" cite={url} data-video-id={videoId}>
        <section></section>
      </blockquote>
      <script async src="https://www.tiktok.com/embed.js"></script>
    </div>
  );
};

const LinkedInEmbed: React.FC<LinkedInEmbedProps> = ({ url }) => {
  return (
    <div className="linkedin-embed">
      <iframe
        src={`https://www.linkedin.com/embed/feed/update/${url}`}
        width="550"
        height="400"
        frameBorder="0"
        allowFullScreen
        title="LinkedIn Post"
      />
    </div>
  );
};

const PinterestEmbed: React.FC<PinterestEmbedProps> = ({ url }) => {
  return (
    <div className="pinterest-embed">
      <a data-pin-do="embedPin" data-pin-width="medium" href={url}>
        View on Pinterest
      </a>
      <script async defer src="//assets.pinterest.com/js/pinit.js"></script>
    </div>
  );
};

const EmbedWidget: React.FC<EmbedWidgetProps> = ({ url }) => {
  if (url.includes('facebook.com')) {
    return <FacebookEmbed url={url} />;
  }
  if (url.includes('instagram.com')) {
    return <InstagramEmbed url={url} />;
  }
  if (url.includes('tiktok.com')) {
    return <TikTokEmbed url={url} />;
  }
  if (url.includes('twitter.com') || url.includes('x.com')) {
    return <TwitterEmbed url={url} />;
  }
  if (url.includes('youtube.com') || url.includes('youtu.be')) {
    return (
      <div className="relative w-full pt-[56.25%]">
        <div className="absolute top-0 left-0 w-full h-full">
          <YouTubeEmbed url={url} width="100%" height="100%" />
        </div>
      </div>
    );
  }
  if (url.includes('linkedin.com')) {
    return <LinkedInEmbed url={url} />;
  }
  if (url.includes('pinterest.com')) {
    return <PinterestEmbed url={url} />;
  }

  return <p>The provided URL is not supported for embedding.</p>;
};

export default EmbedWidget;
