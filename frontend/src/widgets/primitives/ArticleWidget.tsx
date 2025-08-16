import { useEventHandler } from '@/components/event-handler';
import { ScrollArea } from '@/components/ui/scroll-area';
import { cn } from '@/lib/utils';
import { InternalLink } from '@/types/widgets';
import { Github } from 'lucide-react';
import React, { useEffect, useState, useRef } from 'react';

interface ArticleWidgetProps {
  id: string;
  children: React.ReactNode[];
  showToc?: boolean;
  showFooter?: boolean;
  previous: InternalLink;
  next: InternalLink;
  documentSource?: string;
  showContributors?: boolean;
}

export const ArticleWidget: React.FC<ArticleWidgetProps> = ({
  id,
  children,
  previous,
  next,
  documentSource,
  showFooter,
  showToc,
  showContributors = true,
}) => {
  const eventHandler = useEventHandler();
  const articleRef = useRef<HTMLElement>(null);
  const [tocItems, setTocItems] = useState<TocItem[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const timeoutRef = useRef<NodeJS.Timeout | null>(null);

  // Extract Contributors widget from children
  const contributorsWidget = React.Children.toArray(children).find(
    (child: React.ReactNode) =>
      React.isValidElement(child) &&
      ((child.type as { name?: string })?.name === 'ContributorsWidget' ||
        (child.props as { type?: string })?.type === 'Ivy.Contributors')
  );

  // Filter out Contributors widget from main content
  const mainContent = React.Children.toArray(children).filter(
    (child: React.ReactNode) =>
      !(
        React.isValidElement(child) &&
        ((child.type as { name?: string })?.name === 'ContributorsWidget' ||
          (child.props as { type?: string })?.type === 'Ivy.Contributors')
      )
  );

  // Extract headings and manage loading state
  useEffect(() => {
    // Clear any existing timeout
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }

    if (!showToc) {
      setTocItems([]);
      setIsLoading(false);
      return;
    }

    // Reset states when starting fresh
    setTocItems([]);
    setIsLoading(true);

    const startTime = Date.now();
    const minLoadingTime = 500; // 0.5 seconds minimum loading time for smooth transition
    let retryCount = 0;
    const maxRetries = 15; // Try for up to 1.5 seconds (15 * 100ms)

    const extractHeadings = () => {
      const articleElement = articleRef.current;
      if (!articleElement) {
        // If article ref is not available yet, try again after a short delay
        if (retryCount < maxRetries) {
          retryCount++;
          timeoutRef.current = setTimeout(extractHeadings, 100);
        } else {
          // Stop loading if max retries reached, respecting minimum loading time
          const elapsedTime = Date.now() - startTime;
          const remainingTime = Math.max(0, minLoadingTime - elapsedTime);
          timeoutRef.current = setTimeout(() => {
            setIsLoading(false);
          }, remainingTime);
        }
        return;
      }

      const elements = Array.from(
        articleElement.querySelectorAll('h1, h2, h3, h4, h5, h6')
      );

      // If no headings found but content might still be loading, retry
      if (elements.length === 0 && retryCount < maxRetries) {
        retryCount++;
        timeoutRef.current = setTimeout(extractHeadings, 100);
        return;
      }

      const items = elements.map(element => {
        // Generate ID if doesn't exist
        if (!element.id) {
          element.id =
            element.textContent
              ?.toLowerCase()
              .replace(/\s+/g, '-')
              .replace(/[^\w-]/g, '') ?? '';
        }

        return {
          id: element.id,
          text: element.textContent ?? '',
          level: parseInt(element.tagName[1]),
        };
      });

      setTocItems(items);

      // Calculate remaining time to show loading for minimum duration
      const elapsedTime = Date.now() - startTime;
      const remainingTime = Math.max(0, minLoadingTime - elapsedTime);

      // Show TOC content after minimum loading time has passed
      timeoutRef.current = setTimeout(() => {
        setIsLoading(false);
      }, remainingTime);
    };

    // Small delay to ensure content is rendered
    timeoutRef.current = setTimeout(extractHeadings, 50);

    // Cleanup function
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, [showToc]);

  return (
    <div className="flex flex-col gap-2 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative mt-8 ">
      <div className="flex gap-8 flex-grow">
        <article ref={articleRef} className="w-[48rem]">
          <div className="flex flex-col gap-4 flex-grow min-h-[calc(100vh+8rem)]">
            {mainContent}
          </div>
          {showFooter && (
            <footer className="border-t py-8 mt-20">
              <div className="flex flex-col gap-6">
                <div className="flex justify-between items-center">
                  <div className="flex-1">
                    {previous && (
                      <a
                        onClick={() =>
                          eventHandler('OnLinkClick', id, [
                            'app://' + previous.appId,
                          ])
                        }
                        href={'app://' + previous.appId}
                        className="group flex flex-col gap-2 hover:text-primary transition-colors"
                      >
                        <div className="text-body">← Previous</div>
                        <div className="text-body text-muted-foreground">
                          {previous.title}
                        </div>
                      </a>
                    )}
                  </div>
                  <div className="flex-1 flex justify-end">
                    {next && (
                      <a
                        onClick={() =>
                          eventHandler('OnLinkClick', id, [
                            'app://' + next.appId,
                          ])
                        }
                        href={'app://' + next.appId}
                        className="group flex flex-col text-right gap-2 hover:text-primary transition-colors"
                      >
                        <div className="text-body">Next →</div>
                        <div className="text-body text-muted-foreground">
                          {next.title}
                        </div>
                      </a>
                    )}
                  </div>
                </div>
                {documentSource && (
                  <div className="flex justify-center">
                    <a
                      href={documentSource}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="group flex items-center gap-2 text-body text-muted-foreground hover:text-primary transition-colors"
                    >
                      <Github className="w-4 h-4" />
                      Edit this document
                    </a>
                  </div>
                )}
              </div>
            </footer>
          )}
        </article>
        {showToc && (
          <div className="hidden lg:block w-64">
            <div className="sticky top-8">
              {isLoading ? (
                <div className="w-64">
                  <div className="text-body mb-4">Table of Contents</div>
                  <ScrollArea>
                    <div className="flex flex-col gap-4">
                      <div className="h-3 bg-muted rounded animate-pulse w-3/4"></div>
                      <div className="h-3 bg-muted rounded animate-pulse w-full"></div>
                      <div className="h-3 bg-muted rounded animate-pulse w-5/6"></div>
                      <div className="h-3 bg-muted rounded animate-pulse w-2/3"></div>
                      <div className="h-3 bg-muted rounded animate-pulse w-4/5"></div>
                      <div className="h-3 bg-muted rounded animate-pulse w-1/2"></div>
                    </div>
                  </ScrollArea>
                </div>
              ) : tocItems.length > 0 ? (
                <div>
                  <TableOfContents
                    className=""
                    articleRef={articleRef}
                    tocItems={tocItems}
                  />
                  {/* Show contributors after TOC */}
                  {showContributors && contributorsWidget && contributorsWidget}
                </div>
              ) : (
                <div className="w-64">
                  <div className="text-body mb-4">Table of Contents</div>
                  <div className="text-sm text-muted-foreground">
                    No headings found
                  </div>
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

type TocItem = {
  id: string;
  text: string;
  level: number;
};
const TableOfContents = ({
  className,
  articleRef,
  tocItems,
}: {
  className?: string;
  articleRef: React.RefObject<HTMLElement | null>;
  tocItems: TocItem[];
}) => {
  const [activeId, setActiveId] = useState<string>('');

  useEffect(() => {
    if (!articleRef.current || tocItems.length === 0) return;

    const articleElement = articleRef.current;

    // Find all heading elements in the DOM
    const elements = Array.from(
      articleElement.querySelectorAll('h1, h2, h3, h4, h5, h6')
    );

    const observer = new IntersectionObserver(
      entries => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            setActiveId(entry.target.id);
          }
        });
      },
      { rootMargin: '0px 0px -80% 0px' }
    );

    elements.forEach(element => observer.observe(element));

    return () => observer.disconnect();
  }, [articleRef, tocItems]);

  return (
    <div className={cn('w-64 relative', className)}>
      <div className="text-body mb-4">Table of Contents</div>
      <ScrollArea>
        <nav className="relative">
          {tocItems.map(heading => (
            <a
              key={heading.id}
              href={`#${heading.id}`}
              className={cn(
                'block text-sm py-1 hover:text-foreground transition-colors',
                heading.level === 1 ? 'pl-0' : `pl-${(heading.level - 1) * 4}`,
                activeId === heading.id
                  ? 'text-foreground'
                  : 'text-muted-foreground'
              )}
              onClick={e => {
                e.preventDefault();
                document.getElementById(heading.id)?.scrollIntoView({
                  behavior: 'smooth',
                });
              }}
            >
              {heading.text}
            </a>
          ))}
        </nav>
      </ScrollArea>
    </div>
  );
};
