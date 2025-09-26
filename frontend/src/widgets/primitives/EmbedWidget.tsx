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

interface GitHubEmbedProps {
  url: string;
}

interface RedditEmbedProps {
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

const GitHubEmbed: React.FC<GitHubEmbedProps> = ({ url }) => {
  const [embedData, setEmbedData] = useState<{
    type: 'repository' | 'issue' | 'pull' | 'gist' | 'file' | 'unknown';
    owner?: string;
    repo?: string;
    number?: string;
    gistId?: string;
  } | null>(null);

  useEffect(() => {
    const parseGitHubUrl = (githubUrl: string) => {
      // Repository: https://github.com/owner/repo
      let match = githubUrl.match(/github\.com\/([^/]+)\/([^/]+)(?:\/)?$/);
      if (match) {
        return {
          type: 'repository' as const,
          owner: match[1],
          repo: match[2],
        };
      }

      // File: https://github.com/owner/repo/blob/branch/path/to/file
      match = githubUrl.match(
        /github\.com\/([^/]+)\/([^/]+)\/blob\/([^/]+)\/(.+)/
      );
      if (match) {
        return {
          type: 'file' as const,
          owner: match[1],
          repo: match[2],
        };
      }

      // Issue: https://github.com/owner/repo/issues/123
      match = githubUrl.match(/github\.com\/([^/]+)\/([^/]+)\/issues\/(\d+)/);
      if (match) {
        return {
          type: 'issue' as const,
          owner: match[1],
          repo: match[2],
          number: match[3],
        };
      }

      // Pull Request: https://github.com/owner/repo/pull/123
      match = githubUrl.match(/github\.com\/([^/]+)\/([^/]+)\/pull\/(\d+)/);
      if (match) {
        return {
          type: 'pull' as const,
          owner: match[1],
          repo: match[2],
          number: match[3],
        };
      }

      // Gist: https://gist.github.com/username/gistId
      match = githubUrl.match(/gist\.github\.com\/([^/]+)\/([^/]+)/);
      if (match) {
        return {
          type: 'gist' as const,
          owner: match[1],
          gistId: match[2],
        };
      }

      // Any other GitHub URL (commits, releases, etc.)
      match = githubUrl.match(/github\.com\/([^/]+)\/([^/]+)/);
      if (match) {
        return {
          type: 'repository' as const,
          owner: match[1],
          repo: match[2],
        };
      }

      return { type: 'unknown' as const };
    };

    setEmbedData(parseGitHubUrl(url));
  }, [url]);

  if (!embedData || embedData.type === 'unknown') {
    return <div>Invalid GitHub URL.</div>;
  }

  const { type, owner, repo, number, gistId } = embedData;

  if (type === 'gist') {
    return (
      <div className="github-embed">
        <script src={`https://gist.github.com/${owner}/${gistId}.js`}></script>
      </div>
    );
  }

  if (type === 'file') {
    // Use emgithub.com script embedding for GitHub files
    const emgithubUrl = `https://emgithub.com/embed-v2.js?target=${encodeURIComponent(url)}&style=default&theme=default&type=code&showBorder=on&showLineNumbers=on&showFileMeta=on&showFullPath=on&showCopy=on&fetchFrom=jsDelivr&maxHeight=500`;

    return (
      <div className="github-embed border border-gray-300 rounded-lg p-4 bg-white shadow-sm">
        <div className="flex items-center justify-between mb-3">
          <div className="flex items-center space-x-2">
            <svg
              className="w-5 h-5 text-gray-700"
              fill="currentColor"
              viewBox="0 0 24 24"
            >
              <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
            </svg>
            <span className="text-sm font-medium text-gray-700">
              GitHub File
            </span>
          </div>
          <a
            href={url}
            target="_blank"
            rel="noopener noreferrer"
            className="text-sm text-blue-600 hover:text-blue-800"
          >
            View on GitHub
          </a>
        </div>
        <div id={`emgithub-${owner}-${repo}`}>
          <script src={emgithubUrl} async charSet="utf-8"></script>
        </div>
      </div>
    );
  }

  // For repositories, issues, and pull requests, create a custom card instead of iframe
  const getTitle = () => {
    switch (type) {
      case 'repository':
        return `${owner}/${repo}`;
      case 'issue':
        return `${owner}/${repo} #${number}`;
      case 'pull':
        return `${owner}/${repo} PR #${number}`;
      default:
        return 'GitHub';
    }
  };

  const getDescription = () => {
    switch (type) {
      case 'repository':
        return 'View repository on GitHub';
      case 'issue':
        return 'View issue on GitHub';
      case 'pull':
        return 'View pull request on GitHub';
      default:
        return 'View on GitHub';
    }
  };

  return (
    <div className="github-embed border border-gray-300 rounded-lg p-4 bg-white shadow-sm">
      <div className="flex items-center space-x-3">
        <div className="flex-shrink-0">
          <svg
            className="w-8 h-8 text-gray-700"
            fill="currentColor"
            viewBox="0 0 24 24"
          >
            <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
          </svg>
        </div>
        <div className="flex-1 min-w-0">
          <h3 className="text-lg font-semibold text-gray-900 truncate">
            {getTitle()}
          </h3>
          <p className="text-sm text-gray-600">{getDescription()}</p>
        </div>
        <div className="flex-shrink-0">
          <a
            href={url}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center px-3 py-2 border border-gray-300 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
          >
            View on GitHub
          </a>
        </div>
      </div>
    </div>
  );
};

const RedditEmbed: React.FC<RedditEmbedProps> = ({ url }) => {
  const [postId, setPostId] = useState<string | null>(null);

  useEffect(() => {
    const extractPostId = (redditUrl: string): string | null => {
      // Reddit post URL: https://www.reddit.com/r/subreddit/comments/postId/title/
      const match = redditUrl.match(/reddit\.com\/r\/[^/]+\/comments\/([^/]+)/);
      return match ? match[1] : null;
    };

    setPostId(extractPostId(url));
  }, [url]);

  if (!postId) {
    return <div>Invalid Reddit URL.</div>;
  }

  return (
    <div className="redditwrapper">
      <blockquote className="reddit-card" data-card-created={Date.now()}>
        <a href={url}>Reddit Post</a>
      </blockquote>
      <script
        src="https://embed.redditmedia.com/widgets/platform.js"
        async
      ></script>
      <style
        dangerouslySetInnerHTML={{
          __html: `
            .redditwrapper {
              background: #fff;
              position: relative;
            }
            .redditwrapper iframe {
              border: 0;
              position: relative;
              z-index: 2;
            }
            .redditwrapper a {
              color: rgba(0, 0, 0, 0);
              position: absolute;
              left: 0;
              top: 0;
              z-index: 0;
            }
          `,
        }}
      />
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
  if (url.includes('github.com') || url.includes('gist.github.com')) {
    return <GitHubEmbed url={url} />;
  }
  if (url.includes('reddit.com')) {
    return <RedditEmbed url={url} />;
  }

  return <p>The provided URL is not supported for embedding.</p>;
};

export default EmbedWidget;
