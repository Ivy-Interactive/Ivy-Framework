import React, {
  useEffect,
  useState,
  lazy,
  Suspense,
  Component,
  ErrorInfo,
  ReactNode,
} from 'react';
import Icon from '@/components/Icon';

declare global {
  interface Window {
    twttr?: {
      widgets: {
        load: () => void;
      };
    };
  }
}

// Lazy load embed components for better performance
const TwitterEmbed = lazy(() =>
  Promise.resolve({ default: TwitterEmbedComponent })
);
const FacebookEmbed = lazy(() =>
  Promise.resolve({ default: FacebookEmbedComponent })
);
const InstagramEmbed = lazy(() =>
  Promise.resolve({ default: InstagramEmbedComponent })
);
const TikTokEmbed = lazy(() =>
  Promise.resolve({ default: TikTokEmbedComponent })
);
const LinkedInEmbed = lazy(() =>
  Promise.resolve({ default: LinkedInEmbedComponent })
);
const PinterestEmbed = lazy(() =>
  Promise.resolve({ default: PinterestEmbedComponent })
);
const GitHubEmbed = lazy(() =>
  Promise.resolve({ default: GitHubEmbedComponent })
);
const RedditEmbed = lazy(() =>
  Promise.resolve({ default: RedditEmbedComponent })
);

// Error Boundary for embed components
interface ErrorBoundaryState {
  hasError: boolean;
  error?: Error;
}

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback: ReactNode;
}

class EmbedErrorBoundary extends Component<
  ErrorBoundaryProps,
  ErrorBoundaryState
> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    // Log error for debugging (in development)
    if (process.env.NODE_ENV === 'development') {
      console.error('Embed Error Boundary caught an error:', error, errorInfo);
    }
  }

  render() {
    if (this.state.hasError) {
      return this.props.fallback;
    }

    return this.props.children;
  }
}

// Error fallback component
interface EmbedErrorFallbackProps {
  url: string;
  platform?: string;
}

const EmbedErrorFallback: React.FC<EmbedErrorFallbackProps> = ({
  url,
  platform,
}) => {
  return (
    <div className="embed-error border rounded-lg p-4 bg-card shadow-sm">
      <div className="flex items-center space-x-3">
        <div className="flex-1 min-w-0">
          <h3 className="text-lg font-semibold text-card-foreground truncate">
            {platform === 'Unsupported'
              ? 'Unsupported URL'
              : platform
                ? `${platform} Embed Error`
                : 'Embed Error'}
          </h3>
          <p className="text-sm text-muted-foreground">
            {platform === 'Unsupported'
              ? 'This URL is not supported for embedding. Please visit the link directly.'
              : 'Failed to load embed. Please try again or visit the link directly.'}
          </p>
        </div>
        <div className="flex-shrink-0">
          <a
            href={url}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center px-3 py-2 border shadow-sm text-sm font-medium rounded-md text-card-foreground bg-card hover:bg-accent focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary"
          >
            View Original
          </a>
        </div>
      </div>
    </div>
  );
};

// Reusable embed card component
interface EmbedCardProps {
  platform: string;
  iconName: string;
  iconColor: string;
  title: string;
  description: string;
  url: string;
  linkText: string;
}

const EmbedCard: React.FC<EmbedCardProps> = ({
  platform,
  iconName,
  iconColor,
  title,
  description,
  url,
  linkText,
}) => {
  const sanitizedUrl = sanitizeUrl(url);

  if (!sanitizedUrl) {
    return <div>Invalid {platform} URL.</div>;
  }

  return (
    <div
      className={`${platform.toLowerCase()}-embed border rounded-lg p-4 bg-card shadow-sm`}
    >
      <div className="flex items-center space-x-3">
        <div className="flex-shrink-0">
          <Icon name={iconName} size={32} className={iconColor} />
        </div>
        <div className="flex-1 min-w-0">
          <h3 className="text-lg font-semibold text-card-foreground truncate">
            {title}
          </h3>
          <p className="text-sm text-muted-foreground">{description}</p>
        </div>
        <div className="flex-shrink-0">
          <a
            href={sanitizedUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center px-3 py-2 border shadow-sm text-sm font-medium rounded-md text-card-foreground bg-card hover:bg-accent focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary"
          >
            {linkText}
          </a>
        </div>
      </div>
    </div>
  );
};

// Loading fallback component
const EmbedLoadingFallback: React.FC = () => (
  <div className="embed-loading border rounded-lg p-4 bg-card shadow-sm">
    <div className="flex items-center space-x-3">
      <div className="flex-shrink-0">
        <div className="w-8 h-8 bg-muted animate-pulse rounded"></div>
      </div>
      <div className="flex-1 min-w-0">
        <div className="h-4 bg-muted animate-pulse rounded w-1/3 mb-2"></div>
        <div className="h-3 bg-muted animate-pulse rounded w-1/2"></div>
      </div>
    </div>
  </div>
);

// Script loading utilities
const loadedScripts = new Set<string>();

const loadScript = (src: string): Promise<void> => {
  return new Promise((resolve, reject) => {
    // Check if script is already loaded
    if (loadedScripts.has(src)) {
      resolve();
      return;
    }

    // Check if script already exists in DOM
    const existingScript = document.querySelector(`script[src="${src}"]`);
    if (existingScript) {
      loadedScripts.add(src);
      resolve();
      return;
    }

    const script = document.createElement('script');
    script.src = src;
    script.async = true;
    script.onload = () => {
      loadedScripts.add(src);
      resolve();
    };
    script.onerror = () => {
      reject(new Error(`Failed to load script: ${src}`));
    };
    document.head.appendChild(script);
  });
};

// URL validation and sanitization utilities
const isValidUrl = (url: string): boolean => {
  try {
    const urlObj = new URL(url);
    // Only allow http and https protocols
    return ['http:', 'https:'].includes(urlObj.protocol);
  } catch {
    return false;
  }
};

const sanitizeUrl = (url: string): string | null => {
  if (!isValidUrl(url)) {
    return null;
  }

  try {
    const urlObj = new URL(url);
    // Remove potentially dangerous URL components
    urlObj.search = '';
    urlObj.hash = '';
    return urlObj.toString();
  } catch {
    return null;
  }
};

const sanitizeId = (id: string): string => {
  // Remove any non-alphanumeric characters except hyphens and underscores
  return id.replace(/[^a-zA-Z0-9\-_]/g, '');
};

const sanitizeText = (text: string): string => {
  // Remove potentially dangerous characters and limit length
  return text
    .replace(/[<>"'&]/g, '')
    .substring(0, 1000)
    .trim();
};

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
      if (match) return sanitizeId(match[1]);
      match = ytUrl.match(/youtu\.be\/([^?]+)/);
      if (match) return sanitizeId(match[1]);
      match = ytUrl.match(/youtube\.com\/embed\/([^?]+)/);
      if (match) return sanitizeId(match[1]);

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

const TwitterEmbedComponent: React.FC<TwitterEmbedProps> = ({ url }) => {
  const [tweetId, setTweetId] = useState<string | null>(null);

  useEffect(() => {
    const extractTweetId = (twitterUrl: string): string | null => {
      // Support both twitter.com and x.com URLs, with or without @ prefix
      const match = twitterUrl.match(
        /(?:twitter\.com|x\.com)\/\w+\/status\/(\d+)/
      );
      return match ? sanitizeId(match[1]) : null;
    };

    setTweetId(extractTweetId(url));
  }, [url]);

  if (!tweetId) {
    return <EmbedErrorFallback url={url} platform="Twitter" />;
  }

  return (
    <EmbedCard
      platform="Twitter"
      iconName="Twitter"
      iconColor="text-info"
      title="Twitter Tweet"
      description="View tweet on Twitter"
      url={url}
      linkText="View on Twitter"
    />
  );
};

const FacebookEmbedComponent: React.FC<FacebookEmbedProps> = ({ url }) => {
  const extractPageName = (facebookUrl: string): string => {
    // Extract page name from various Facebook URL formats
    const match = facebookUrl.match(/facebook\.com\/([^/?]+)/);
    if (match) {
      return sanitizeText(match[1]);
    }
    return 'Facebook';
  };

  const pageName = extractPageName(url);

  return (
    <EmbedCard
      platform="Facebook"
      iconName="Facebook"
      iconColor="text-info"
      title={pageName}
      description="View post on Facebook"
      url={url}
      linkText="View on Facebook"
    />
  );
};

const InstagramEmbedComponent: React.FC<InstagramEmbedProps> = ({ url }) => {
  const [postId, setPostId] = useState<string | null>(null);
  const [scriptLoaded, setScriptLoaded] = useState(false);

  useEffect(() => {
    const extractPostId = (instagramUrl: string): string | null => {
      const match = instagramUrl.match(/instagram\.com\/p\/([^/]+)/);
      return match ? sanitizeId(match[1]) : null;
    };

    setPostId(extractPostId(url));
  }, [url]);

  useEffect(() => {
    if (postId) {
      loadScript('https://www.instagram.com/embed.js')
        .then(() => setScriptLoaded(true))
        .catch(error =>
          console.error('Failed to load Instagram script:', error)
        );
    }
  }, [postId]);

  if (!postId) {
    return <EmbedErrorFallback url={url} platform="Instagram" />;
  }

  const sanitizedUrl = sanitizeUrl(url);
  if (!sanitizedUrl) {
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
          {scriptLoaded ? 'Loading Instagram post...' : 'Loading script...'}
        </a>
      </blockquote>
    </div>
  );
};

const TikTokEmbedComponent: React.FC<TikTokEmbedProps> = ({ url }) => {
  const [videoId, setVideoId] = useState<string | null>(null);
  const [scriptLoaded, setScriptLoaded] = useState(false);

  useEffect(() => {
    const extractVideoId = (tiktokUrl: string): string | null => {
      const match = tiktokUrl.match(/tiktok\.com\/@[\w.-]+\/video\/(\d+)/);
      return match ? sanitizeId(match[1]) : null;
    };

    setVideoId(extractVideoId(url));
  }, [url]);

  useEffect(() => {
    if (videoId) {
      loadScript('https://www.tiktok.com/embed.js')
        .then(() => setScriptLoaded(true))
        .catch(error => console.error('Failed to load TikTok script:', error));
    }
  }, [videoId]);

  if (!videoId) {
    return <EmbedErrorFallback url={url} platform="TikTok" />;
  }

  const sanitizedUrl = sanitizeUrl(url);
  if (!sanitizedUrl) {
    return <EmbedErrorFallback url={url} platform="TikTok" />;
  }

  return (
    <div className="tiktok-embed">
      <blockquote
        className="tiktok-embed"
        cite={sanitizedUrl}
        data-video-id={videoId}
      >
        <section>
          {scriptLoaded ? 'Loading TikTok video...' : 'Loading script...'}
        </section>
      </blockquote>
    </div>
  );
};

const LinkedInEmbedComponent: React.FC<LinkedInEmbedProps> = ({ url }) => {
  const [postId, setPostId] = useState<string | null>(null);

  useEffect(() => {
    const extractPostId = (linkedinUrl: string): string | null => {
      // LinkedIn post URL: https://www.linkedin.com/posts/activity-1234567890-abcdefgh/
      let match = linkedinUrl.match(/linkedin\.com\/posts\/activity-(\d+)/);
      if (match) return sanitizeId(match[1]);

      // LinkedIn post URL: https://www.linkedin.com/feed/update/urn:li:activity:1234567890/
      match = linkedinUrl.match(
        /linkedin\.com\/feed\/update\/urn:li:activity:(\d+)/
      );
      if (match) return sanitizeId(match[1]);

      // LinkedIn post URL: https://www.linkedin.com/posts/username-activity-1234567890/
      match = linkedinUrl.match(/linkedin\.com\/posts\/[^/]+-activity-(\d+)/);
      if (match) return sanitizeId(match[1]);

      // Any LinkedIn URL that contains "posts" or "feed"
      match = linkedinUrl.match(/linkedin\.com\/(posts|feed)/);
      if (match) return 'linkedin-post';

      return null;
    };

    setPostId(extractPostId(url));
  }, [url]);

  useEffect(() => {
    if (postId) {
      loadScript('https://platform.linkedin.com/in.js')
        .then(() => {
          // LinkedIn script loaded successfully
        })
        .catch(error =>
          console.error('Failed to load LinkedIn script:', error)
        );
    }
  }, [postId]);

  if (!postId) {
    return <EmbedErrorFallback url={url} platform="LinkedIn" />;
  }

  const sanitizedUrl = sanitizeUrl(url);
  if (!sanitizedUrl) {
    return <EmbedErrorFallback url={url} platform="LinkedIn" />;
  }

  return (
    <div className="linkedin-embed w-full">
      <div className="linkedin-embed-container w-full">
        {/* LinkedIn Official Embed - Using responsive iframe */}
        <iframe
          src={`https://www.linkedin.com/embed/feed/update/urn:li:activity:${postId}`}
          className="w-full h-96 sm:h-[32rem] md:h-[40rem] border-0 rounded-lg shadow-md"
          allowFullScreen
          title="Embedded LinkedIn post"
        />
      </div>
    </div>
  );
};

const PinterestEmbedComponent: React.FC<PinterestEmbedProps> = ({ url }) => {
  return (
    <EmbedCard
      platform="Pinterest"
      iconName="Pinterest"
      iconColor="text-destructive"
      title="Pinterest Pin"
      description="View pin on Pinterest"
      url={url}
      linkText="View on Pinterest"
    />
  );
};

const GitHubEmbedComponent: React.FC<GitHubEmbedProps> = ({ url }) => {
  const [repoInfo, setRepoInfo] = useState<{
    owner?: string;
    repo?: string;
    type?: string;
    number?: string;
  } | null>(null);

  useEffect(() => {
    const parseGitHubUrl = (githubUrl: string) => {
      // Issue: https://github.com/owner/repo/issues/123
      let match = githubUrl.match(
        /github\.com\/([^/]+)\/([^/]+)\/issues\/(\d+)/
      );
      if (match) {
        return {
          owner: sanitizeId(match[1]),
          repo: sanitizeId(match[2]),
          type: 'issue',
          number: match[3],
        };
      }

      // Pull Request: https://github.com/owner/repo/pull/123
      match = githubUrl.match(/github\.com\/([^/]+)\/([^/]+)\/pull\/(\d+)/);
      if (match) {
        return {
          owner: sanitizeId(match[1]),
          repo: sanitizeId(match[2]),
          type: 'pull',
          number: match[3],
        };
      }

      // Repository: https://github.com/owner/repo
      match = githubUrl.match(/github\.com\/([^/]+)\/([^/]+)$/);
      if (match) {
        return {
          owner: sanitizeId(match[1]),
          repo: sanitizeId(match[2]),
          type: 'repository',
        };
      }

      // Gist: https://gist.github.com/username/gistId
      match = githubUrl.match(/gist\.github\.com\/([^/]+)\/([^/]+)/);
      if (match) {
        return {
          owner: sanitizeId(match[1]),
          repo: 'gist',
          type: 'gist',
        };
      }

      return null;
    };

    setRepoInfo(parseGitHubUrl(url));
  }, [url]);

  if (!repoInfo) {
    return <EmbedErrorFallback url={url} platform="GitHub" />;
  }

  const sanitizedUrl = sanitizeUrl(url);
  if (!sanitizedUrl) {
    return <EmbedErrorFallback url={url} platform="GitHub" />;
  }

  const getTitle = () => {
    if (repoInfo.type === 'gist') {
      return `${repoInfo.owner}/gist`;
    }
    if (repoInfo.type === 'issue') {
      return `${repoInfo.owner}/${repoInfo.repo} #${repoInfo.number}`;
    }
    if (repoInfo.type === 'pull') {
      return `${repoInfo.owner}/${repoInfo.repo} #${repoInfo.number}`;
    }
    return `${repoInfo.owner}/${repoInfo.repo}`;
  };

  const getDescription = () => {
    if (repoInfo.type === 'gist') {
      return 'View gist on GitHub';
    }
    if (repoInfo.type === 'issue') {
      return 'View issue on GitHub';
    }
    if (repoInfo.type === 'pull') {
      return 'View pull request on GitHub';
    }
    return 'View repository on GitHub';
  };

  const getLinkText = () => {
    if (repoInfo.type === 'issue') {
      return 'View Issue';
    }
    if (repoInfo.type === 'pull') {
      return 'View Pull Request';
    }
    if (repoInfo.type === 'gist') {
      return 'View Gist';
    }
    return 'View Repository';
  };

  return (
    <EmbedCard
      platform="GitHub"
      iconName="Github"
      iconColor="text-card-foreground"
      title={getTitle()}
      description={getDescription()}
      url={url}
      linkText={getLinkText()}
    />
  );
};

const RedditEmbedComponent: React.FC<RedditEmbedProps> = ({ url }) => {
  const [postId, setPostId] = useState<string | null>(null);
  const [scriptLoaded, setScriptLoaded] = useState(false);

  useEffect(() => {
    const extractPostId = (redditUrl: string): string | null => {
      // Reddit post URL: https://www.reddit.com/r/subreddit/comments/postId/title/
      const match = redditUrl.match(/reddit\.com\/r\/[^/]+\/comments\/([^/]+)/);
      return match ? sanitizeId(match[1]) : null;
    };

    setPostId(extractPostId(url));
  }, [url]);

  useEffect(() => {
    if (postId) {
      loadScript('https://embed.redditmedia.com/widgets/platform.js')
        .then(() => setScriptLoaded(true))
        .catch(error => console.error('Failed to load Reddit script:', error));
    }
  }, [postId]);

  if (!postId) {
    return <EmbedErrorFallback url={url} platform="Reddit" />;
  }

  const sanitizedUrl = sanitizeUrl(url);
  if (!sanitizedUrl) {
    return <EmbedErrorFallback url={url} platform="Reddit" />;
  }

  return (
    <div className="redditwrapper">
      <blockquote className="reddit-card">
        <a href={sanitizedUrl}>
          {scriptLoaded ? 'Loading Reddit post...' : 'Loading script...'}
        </a>
      </blockquote>
    </div>
  );
};

const EmbedWidget: React.FC<EmbedWidgetProps> = ({ url }) => {
  // Validate URL at the entry point
  if (!isValidUrl(url)) {
    return <EmbedErrorFallback url={url} platform="Unsupported" />;
  }

  // YouTube embed doesn't need lazy loading as it's lightweight
  if (url.includes('youtube.com') || url.includes('youtu.be')) {
    return (
      <EmbedErrorBoundary
        fallback={<EmbedErrorFallback url={url} platform="YouTube" />}
      >
        <div className="relative w-full pt-[56.25%]">
          <div className="absolute top-0 left-0 w-full h-full">
            <YouTubeEmbed url={url} width="100%" height="100%" />
          </div>
        </div>
      </EmbedErrorBoundary>
    );
  }

  // Lazy load other embed components with error boundaries
  if (url.includes('facebook.com')) {
    return (
      <EmbedErrorBoundary
        fallback={<EmbedErrorFallback url={url} platform="Facebook" />}
      >
        <Suspense fallback={<EmbedLoadingFallback />}>
          <FacebookEmbed url={url} />
        </Suspense>
      </EmbedErrorBoundary>
    );
  }
  if (url.includes('instagram.com')) {
    return (
      <EmbedErrorBoundary
        fallback={<EmbedErrorFallback url={url} platform="Instagram" />}
      >
        <Suspense fallback={<EmbedLoadingFallback />}>
          <InstagramEmbed url={url} />
        </Suspense>
      </EmbedErrorBoundary>
    );
  }
  if (url.includes('tiktok.com')) {
    return (
      <EmbedErrorBoundary
        fallback={<EmbedErrorFallback url={url} platform="TikTok" />}
      >
        <Suspense fallback={<EmbedLoadingFallback />}>
          <TikTokEmbed url={url} />
        </Suspense>
      </EmbedErrorBoundary>
    );
  }
  if (url.includes('twitter.com') || url.includes('x.com')) {
    return (
      <EmbedErrorBoundary
        fallback={<EmbedErrorFallback url={url} platform="Twitter" />}
      >
        <Suspense fallback={<EmbedLoadingFallback />}>
          <TwitterEmbed url={url} />
        </Suspense>
      </EmbedErrorBoundary>
    );
  }
  if (url.includes('linkedin.com')) {
    return (
      <EmbedErrorBoundary
        fallback={<EmbedErrorFallback url={url} platform="LinkedIn" />}
      >
        <Suspense fallback={<EmbedLoadingFallback />}>
          <LinkedInEmbed url={url} />
        </Suspense>
      </EmbedErrorBoundary>
    );
  }
  if (url.includes('pinterest.com') || url.includes('pin.it')) {
    return (
      <EmbedErrorBoundary
        fallback={<EmbedErrorFallback url={url} platform="Pinterest" />}
      >
        <Suspense fallback={<EmbedLoadingFallback />}>
          <PinterestEmbed url={url} />
        </Suspense>
      </EmbedErrorBoundary>
    );
  }
  if (url.includes('github.com') || url.includes('gist.github.com')) {
    return (
      <EmbedErrorBoundary
        fallback={<EmbedErrorFallback url={url} platform="GitHub" />}
      >
        <Suspense fallback={<EmbedLoadingFallback />}>
          <GitHubEmbed url={url} />
        </Suspense>
      </EmbedErrorBoundary>
    );
  }
  if (url.includes('reddit.com')) {
    return (
      <EmbedErrorBoundary
        fallback={<EmbedErrorFallback url={url} platform="Reddit" />}
      >
        <Suspense fallback={<EmbedLoadingFallback />}>
          <RedditEmbed url={url} />
        </Suspense>
      </EmbedErrorBoundary>
    );
  }

  return <EmbedErrorFallback url={url} platform="Unsupported" />;
};

export default EmbedWidget;
