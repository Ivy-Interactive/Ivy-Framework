import { useEventHandler } from '@/components/event-handler';
import { ArticleFooter } from '@/widgets/article/ArticleFooter';
import { ArticleSidebar } from '@/widgets/article/ArticleSidebar';
import { InternalLink } from '@/types/widgets';
import React, { useRef } from 'react';
import { useSyncExternalStore } from 'react';
import { mcpPanelStore } from '@/widgets/primitives/mcpPanelStore';

interface ArticleWidgetProps {
  id: string;
  children: React.ReactNode[];
  showToc?: boolean;
  showFooter?: boolean;
  previous: InternalLink;
  next: InternalLink;
  documentSource?: string;
  title?: string;
  headings?: { id: string; text: string; level: number }[];
  gap?: number;
}

import { TypographyProvider } from '@/contexts/TypographyContext';
import { articleTypography } from '@/lib/styles';

export const ArticleWidget: React.FC<ArticleWidgetProps> = ({
  id,
  children,
  previous,
  next,
  documentSource,
  showFooter = true,
  showToc = true,
  title,
  headings = [],
  gap = 4,
}) => {
  const eventHandler = useEventHandler();
  const articleRef = useRef<HTMLElement>(null);
  const { isOpen, panelWidthFraction } = useSyncExternalStore(
    mcpPanelStore.subscribe,
    mcpPanelStore.getState,
    mcpPanelStore.getState
  );

  return (
    <div
      className="flex flex-col gap-2 max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 relative"
      data-docs-content-boundary
      style={
        isOpen && panelWidthFraction > 0
          ? {
              width: '100%',
              maxWidth: `calc(100vw * (1 - ${panelWidthFraction}))`,
            }
          : undefined
      }
    >
      <div className="flex flex-grow gap-8">
        <article ref={articleRef} className="w-full max-w-[48rem]">
          <TypographyProvider value={articleTypography}>
            <div
              className="flex flex-col flex-grow min-h-[calc(100vh+8rem)]"
              style={{ gap: `${gap * 0.25}rem` }}
            >
              {children}
            </div>
          </TypographyProvider>
          {showFooter && (
            <ArticleFooter
              id={id}
              previous={previous}
              next={next}
              documentSource={documentSource}
              onLinkClick={eventHandler}
            />
          )}
        </article>
        <ArticleSidebar
          articleRef={articleRef}
          showToc={showToc}
          documentSource={documentSource}
          title={title}
          headings={headings}
        />
      </div>
    </div>
  );
};
