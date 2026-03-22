import { useEventHandler } from "@/components/event-handler";
import { ArticleFooter } from "@/widgets/article/ArticleFooter";
import { ArticleSidebar } from "@/widgets/article/ArticleSidebar";
import { InternalLink } from "@/types/widgets";
import React, { useRef } from "react";

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

import { TypographyContext } from "@/contexts/TypographyContext";
import { articleTypography } from "@/lib/styles";

const EMPTY_ARRAY: never[] = [];

export const ArticleWidget: React.FC<ArticleWidgetProps> = ({
  id,
  children,
  previous,
  next,
  documentSource,
  showFooter = true,
  showToc = true,
  title,
  headings = EMPTY_ARRAY,
  gap = 4,
}) => {
  const eventHandler = useEventHandler();
  const articleRef = useRef<HTMLElement>(null);

  return (
    <div className="flex flex-col gap-2 max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 relative mt-8">
      <div className="flex flex-grow gap-8">
        <article ref={articleRef} className="w-full max-w-[48rem]">
          <TypographyContext.Provider value={articleTypography}>
            <div
              className="flex flex-col flex-grow min-h-[calc(100vh+8rem)]"
              style={{ gap: `${gap * 0.25}rem` }}
            >
              {children}
            </div>
          </TypographyContext.Provider>
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
