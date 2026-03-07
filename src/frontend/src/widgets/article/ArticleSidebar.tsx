import { TableOfContents } from '@/widgets/article/TableOfContents';
import { GitHubContributors } from '@/widgets/article/GitHubContributors';
import { DocumentTools } from '@/widgets/article/DocumentTools';
import React, { useState } from 'react';

interface ArticleSidebarProps {
  articleRef: React.RefObject<HTMLElement | null>;
  showToc?: boolean;
  documentSource?: string;
  title?: string;
  headings?: { id: string; text: string; level: number }[];
}

export const ArticleSidebar: React.FC<ArticleSidebarProps> = ({
  articleRef,
  showToc,
  documentSource,
  title,
  headings,
}) => {
  const [tocLoading, setTocLoading] = useState(true);
  const [contributorsLoading, setContributorsLoading] = useState(true);
  // Only show contributors when TOC is ready too
  const showContributors = !tocLoading && !contributorsLoading;
  // Only show sidebar if TOC should be displayed
  // If headings are provided, we don't need to block on loading state for TOC to check emptiness
  // But TableOfContents component handles loading state
  if (!showToc) return null;

  return (
    <div className="hidden lg:block w-64">
      <div className="sticky top-8 w-64 flex flex-col gap-4 max-h-[calc(100vh-4rem)]">
        <DocumentTools
          articleRef={articleRef}
          documentSource={documentSource}
          title={title}
        />
        <div className="flex-1 flex flex-col gap-4 min-h-0">
          <TableOfContents
            articleRef={articleRef}
            show={showToc}
            onLoadingChange={setTocLoading}
            headings={headings}
          />
          <GitHubContributors
            documentSource={documentSource}
            onLoadingChange={setContributorsLoading}
            show={showContributors}
          />
        </div>
      </div>
    </div>
  );
};
