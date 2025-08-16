import { useEventHandler } from '@/components/event-handler';
import { ArticleFooter } from '@/widgets/article/ArticleFooter';
import { ArticleSidebar } from '@/widgets/article/ArticleSidebar';
import { InternalLink } from '@/types/widgets';
import React, { useRef } from 'react';

interface ArticleWidgetProps {
  id: string;
  children: React.ReactNode[];
  showToc?: boolean;
  showFooter?: boolean;
  previous: InternalLink;
  next: InternalLink;
  documentSource?: string;
  title?: string;
}

export const ArticleWidget: React.FC<ArticleWidgetProps> = ({
  id,
  children,
  previous,
  next,
  documentSource,
  showFooter,
  showToc,
  title,
}) => {
  const eventHandler = useEventHandler();
  const articleRef = useRef<HTMLElement>(null);

  return (
    <div className="flex flex-col gap-2 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative mt-8">
      <div className="flex gap-8 flex-grow">
        <article ref={articleRef} className="w-[48rem]">
          <div className="flex flex-col gap-4 flex-grow min-h-[calc(100vh+8rem)]">
            {children}
          </div>
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
        />
      </div>
    </div>
  );
};
