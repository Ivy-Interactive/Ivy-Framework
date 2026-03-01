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

/** When panel uses more than this fraction of viewport, TOC is considered overlayed and hidden. */
const TOC_OVERLAY_FRACTION = 0.5;

export const ArticleSidebar: React.FC<ArticleSidebarProps> = ({
  articleRef,
  showToc,
  documentSource,
  title,
  headings,
}) => {
  const [tocLoading, setTocLoading] = useState(true);
  const [contributorsLoading, setContributorsLoading] = useState(true);

  const { isOpen: mcpPanelOpen, panelWidthFraction } = useSyncExternalStore(
    mcpPanelStore.subscribe,
    mcpPanelStore.getState,
    mcpPanelStore.getState
  );

  const tocOverlayedByPanel =
    mcpPanelOpen && panelWidthFraction > TOC_OVERLAY_FRACTION;
  const showSidebar = showToc && (!mcpPanelOpen || !tocOverlayedByPanel);
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
