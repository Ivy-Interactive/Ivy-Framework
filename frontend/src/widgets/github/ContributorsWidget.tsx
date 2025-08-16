import React from 'react';
import { cn } from '@/lib/utils';

interface Contributor {
  login: string;
  avatarUrl: string;
  htmlUrl: string;
  contributions: number;
  lastCommitDate: string;
}

interface ContributorsWidgetProps {
  contributorsData: Contributor[];
  isLoading: boolean;
  hasError: boolean;
  showOnMobile: boolean;
}

export const ContributorsWidget: React.FC<ContributorsWidgetProps> = ({
  contributorsData,
  isLoading,
  hasError,
  showOnMobile,
}) => {
  // Don't render anything if there are no contributors and not loading
  if (!isLoading && contributorsData.length === 0 && !hasError) {
    return null;
  }

  const containerClasses = cn('w-64 mt-6', !showOnMobile && 'hidden lg:block');

  if (hasError) {
    return (
      <div className={containerClasses}>
        <div className="text-body mb-4">Contributors</div>
        <div className="text-sm text-muted-foreground">
          Unable to load contributors
        </div>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className={containerClasses}>
        <div className="text-body mb-4">Contributors</div>
        <div className="flex flex-col gap-3">
          {[...Array(3)].map((_, i) => (
            <div key={i} className="flex items-center gap-3">
              <div className="w-8 h-8 bg-muted rounded-full animate-pulse" />
              <div className="flex flex-col gap-1 flex-1">
                <div className="h-3 bg-muted rounded animate-pulse w-2/3" />
                <div className="h-2 bg-muted rounded animate-pulse w-1/3" />
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  const formatDate = (dateString: string) => {
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
      });
    } catch {
      return dateString;
    }
  };

  const formatContributions = (count: number) => {
    if (count === 1) return '1 commit';
    return `${count} commits`;
  };

  return (
    <div className={containerClasses}>
      <div className="text-body mb-4">Contributors</div>
      <div className="flex flex-col gap-3">
        {contributorsData.map(contributor => (
          <a
            key={contributor.login}
            href={contributor.htmlUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="flex items-center gap-3 p-2 rounded-lg hover:bg-muted/50 transition-colors group"
          >
            <img
              src={contributor.avatarUrl}
              alt={`${contributor.login}'s avatar`}
              className="w-8 h-8 rounded-full"
              loading="lazy"
            />
            <div className="flex flex-col min-w-0 flex-1">
              <div className="text-sm font-medium text-foreground group-hover:text-primary transition-colors truncate">
                {contributor.login}
              </div>
              <div className="text-xs text-muted-foreground">
                {formatContributions(contributor.contributions)} •{' '}
                {formatDate(contributor.lastCommitDate)}
              </div>
            </div>
          </a>
        ))}
      </div>
      {contributorsData.length > 0 && (
        <div className="mt-3 pt-3 border-t border-border">
          <div className="text-xs text-muted-foreground text-center">
            Based on recent commits
          </div>
        </div>
      )}
    </div>
  );
};
