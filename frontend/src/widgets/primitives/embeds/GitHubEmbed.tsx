import React, { useEffect, useState } from 'react';
import { sanitizeUrl, sanitizeId } from './shared';
import EmbedCard from './EmbedCard';
import EmbedErrorFallback from './EmbedErrorFallback';

interface GitHubEmbedProps {
  url: string;
}

const GitHubEmbed: React.FC<GitHubEmbedProps> = ({ url }) => {
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

  const sanitizedUrl = sanitizeUrl(url);
  if (!repoInfo || !sanitizedUrl) {
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

export default GitHubEmbed;
