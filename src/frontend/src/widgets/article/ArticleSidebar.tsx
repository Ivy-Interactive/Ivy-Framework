import { TableOfContents } from '@/widgets/article/TableOfContents';
import { GitHubContributors } from '@/widgets/article/GitHubContributors';
import { DocumentTools } from '@/widgets/article/DocumentTools';
import { mcpPanelStore } from '@/widgets/primitives/mcpPanelStore';
import React, { useState } from 'react';
import { useSyncExternalStore } from 'react';

interface ArticleSidebarProps {
  articleRef: React.RefObject<HTMLElement | null>;
  showToc?: boolean;
  documentSource?: string;
  title?: string;
  headings?: { id: string; text: string; level: number }[];
}

const VIEWPORT_THRESHOLD_HIDE_TOC = 1400;

export const ArticleSidebar: React.FC<ArticleSidebarProps> = ({
  articleRef,
  showToc,
  documentSource,
  title,
  headings,
}) => {
  const [tocLoading, setTocLoading] = useState(true);
  const [contributorsLoading, setContributorsLoading] = useState(true);
  const [viewportWidth, setViewportWidth] = useState(
    typeof window !== 'undefined' ? window.innerWidth : 1024
  );
  const mcpPanelOpen = useSyncExternalStore(
    mcpPanelStore.subscribe,
    mcpPanelStore.getState,
    mcpPanelStore.getState
  ).isOpen;

  React.useEffect(() => {
    const onResize = () => setViewportWidth(window.innerWidth);
    window.addEventListener('resize', onResize);
    return () => window.removeEventListener('resize', onResize);
  }, []);

  const hideTocForMcpPanel =
    mcpPanelOpen && viewportWidth < VIEWPORT_THRESHOLD_HIDE_TOC;
  const showSidebar = showToc && !hideTocForMcpPanel;
  const showContributors = !tocLoading && !contributorsLoading && showSidebar;

  if (!showSidebar) return null;

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
