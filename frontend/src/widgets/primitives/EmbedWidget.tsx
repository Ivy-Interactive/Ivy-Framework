import React, { lazy, Suspense } from 'react';
import { isValidUrl } from './embeds/shared';
import EmbedErrorFallback from './embeds/EmbedErrorFallback';
import EmbedLoadingFallback from './embeds/EmbedLoadingFallback';
import EmbedErrorBoundary from './embeds/EmbedErrorBoundary';
import YouTubeEmbed from './embeds/YouTubeEmbed';

const TwitterEmbed = lazy(() => import('./embeds/TwitterEmbed'));
const FacebookEmbed = lazy(() => import('./embeds/FacebookEmbed'));
const InstagramEmbed = lazy(() => import('./embeds/InstagramEmbed'));
const TikTokEmbed = lazy(() => import('./embeds/TikTokEmbed'));
const LinkedInEmbed = lazy(() => import('./embeds/LinkedInEmbed'));
const PinterestEmbed = lazy(() => import('./embeds/PinterestEmbed'));
const GitHubEmbed = lazy(() => import('./embeds/GitHubEmbed'));
const RedditEmbed = lazy(() => import('./embeds/RedditEmbed'));

// Main EmbedWidget component
interface EmbedWidgetProps {
  url: string;
}

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
