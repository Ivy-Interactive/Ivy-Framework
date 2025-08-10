import { useEventHandler } from '@/components/EventHandlerContext';
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
}

type TocItem = {
  id: string;
  text: string;
  level: number;
};

// Navigation link component
const NavigationLink: React.FC<{
  link: InternalLink;
  direction: 'previous' | 'next';
  onClick: (appId: string) => void;
}> = ({ link, direction, onClick }) => {
  const isNext = direction === 'next';
  const arrow = isNext ? '→' : '←';
  const label = isNext ? 'Next' : 'Previous';

  return (
    <a
      onClick={() => onClick(link.appId)}
      href={`app://${link.appId}`}
      className={cn(
        'group flex flex-col gap-2 hover:text-primary transition-colors',
        isNext && 'text-right'
      )}
    >
      <div className="text-body">
        {arrow} {label}
      </div>
      <div className="text-body text-muted-foreground">{link.title}</div>
    </a>
  );
};

// Footer component
const ArticleFooter: React.FC<{
  previous?: InternalLink;
  next?: InternalLink;
  documentSource?: string;
  onLinkClick: (appId: string) => void;
}> = ({ previous, next, documentSource, onLinkClick }) => (
  <footer className="border-t py-8 mt-20">
    <div className="flex flex-col gap-6">
      <div className="flex justify-between items-center">
        <div className="flex-1">
          {previous && (
            <NavigationLink
              link={previous}
              direction="previous"
              onClick={onLinkClick}
            />
          )}
        </div>
        <div className="flex-1 flex justify-end">
          {next && (
            <NavigationLink
              link={next}
              direction="next"
              onClick={onLinkClick}
            />
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
);

// Loading skeleton for table of contents
const TocSkeleton: React.FC = () => (
  <div className="sticky top-8 w-64 relative">
    <div className="text-body mb-4">Table of Contents</div>
    <ScrollArea>
      <div className="space-y-2">
        {Array.from({ length: 8 }).map((_, index) => (
          <div
            key={index}
            className={cn(
              'h-3 bg-muted rounded animate-pulse',
              index % 2 === 0 ? 'w-3/4' : index % 3 === 0 ? 'w-full' : 'w-5/6'
            )}
          />
        ))}
      </div>
    </ScrollArea>
  </div>
);

// Table of contents component
const TableOfContents: React.FC<{
  className?: string;
  articleRef: React.RefObject<HTMLElement | null>;
}> = ({ className, articleRef }) => {
  const [headings, setHeadings] = useState<TocItem[]>([]);
  const [activeId, setActiveId] = useState<string>('');

  useEffect(() => {
    if (!articleRef.current) return;

    let observer: IntersectionObserver | null = null;
    let mutationObserver: MutationObserver | null = null;
    let retryCount = 0;
    const maxRetries = 5;

    const updateHeadings = () => {
      if (!articleRef.current) return;

      const articleElement = articleRef.current;
      const elements = Array.from(
        articleElement.querySelectorAll('h1, h2, h3, h4, h5, h6')
      );

      if (elements.length === 0) {
        if (retryCount < maxRetries) {
          retryCount++;
          // Exponential backoff: 50ms, 100ms, 200ms, 400ms, 800ms
          const delay = Math.min(50 * Math.pow(2, retryCount - 1), 800);
          setTimeout(updateHeadings, delay);
        }
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

      setHeadings(items);

      // Set up intersection observer for active heading tracking
      observer = new IntersectionObserver(
        entries => {
          entries.forEach(entry => {
            if (entry.isIntersecting) {
              setActiveId(entry.target.id);
            }
          });
        },
        { rootMargin: '0px 0px -80% 0px' }
      );

      elements.forEach(element => observer?.observe(element));
    };

    // Set up mutation observer to detect DOM changes
    mutationObserver = new MutationObserver(mutations => {
      let shouldUpdate = false;

      for (const mutation of mutations) {
        if (mutation.type === 'childList') {
          // Check if headings were added/removed
          for (const node of mutation.addedNodes) {
            if (node.nodeType === Node.ELEMENT_NODE) {
              const element = node as Element;
              if (
                element.matches('h1, h2, h3, h4, h5, h6') ||
                element.querySelector('h1, h2, h3, h4, h5, h6')
              ) {
                shouldUpdate = true;
                break;
              }
            }
          }

          for (const node of mutation.removedNodes) {
            if (node.nodeType === Node.ELEMENT_NODE) {
              const element = node as Element;
              if (
                element.matches('h1, h2, h3, h4, h5, h6') ||
                element.querySelector('h1, h2, h3, h4, h5, h6')
              ) {
                shouldUpdate = true;
                break;
              }
            }
          }
        }
      }

      if (shouldUpdate) {
        // Reset retry count for new content
        retryCount = 0;
        updateHeadings();
      }
    });

    // Start observing DOM changes
    if (articleRef.current) {
      mutationObserver.observe(articleRef.current, {
        childList: true,
        subtree: true,
      });
    }

    // Initial attempt with immediate execution
    updateHeadings();

    // Fallback retry with shorter intervals
    const fallbackTimer = setTimeout(() => {
      if (headings.length === 0) {
        retryCount = 0;
        updateHeadings();
      }
    }, 300); // Reduced from 1000ms to 300ms

    return () => {
      if (observer) observer.disconnect();
      if (mutationObserver) mutationObserver.disconnect();
      clearTimeout(fallbackTimer);
    };
  }, [articleRef, headings.length]);

  const handleHeadingClick = (e: React.MouseEvent, headingId: string) => {
    e.preventDefault();
    document.getElementById(headingId)?.scrollIntoView({
      behavior: 'smooth',
    });
  };

  return (
    <div className={cn('w-64 relative', className)}>
      <div className="text-body mb-4">Table of Contents</div>
      <ScrollArea>
        <nav className="relative">
          {headings.map(heading => (
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
              onClick={e => handleHeadingClick(e, heading.id)}
            >
              {heading.text}
            </a>
          ))}
        </nav>
      </ScrollArea>
    </div>
  );
};

// Main article widget component
export const ArticleWidget: React.FC<ArticleWidgetProps> = ({
  id,
  children,
  previous,
  next,
  documentSource,
  showFooter,
  showToc,
}) => {
  const eventHandler = useEventHandler();
  const [contentLoaded, setContentLoaded] = useState(false);
  const articleRef = useRef<HTMLElement>(null);

  useEffect(() => {
    // Use a more intelligent content loading detection
    const checkContentReady = () => {
      if (articleRef.current) {
        const headings = articleRef.current.querySelectorAll(
          'h1, h2, h3, h4, h5, h6'
        );
        const hasContent =
          headings.length > 0 || articleRef.current.children.length > 0;

        if (hasContent) {
          setContentLoaded(true);
          return true;
        }
      }
      return false;
    };

    // Try immediate check first
    if (checkContentReady()) return;

    // Use requestAnimationFrame for next frame check
    let rafId: number;
    const checkNextFrame = () => {
      if (checkContentReady()) return;

      // Check again after a short delay
      rafId = requestAnimationFrame(() => {
        if (checkContentReady()) return;

        // Final fallback with minimal delay
        setTimeout(() => {
          setContentLoaded(true);
        }, 150); // Reduced from 1000ms to 150ms
      });
    };

    rafId = requestAnimationFrame(checkNextFrame);

    return () => {
      if (rafId) cancelAnimationFrame(rafId);
    };
  }, [children]); // Add children dependency to re-check when content changes

  const handleLinkClick = (appId: string) => {
    eventHandler('OnLinkClick', id, [`app://${appId}`]);
  };

  return (
    <div className="flex flex-col gap-2 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative mt-8">
      <div className="flex gap-8 flex-grow">
        <article ref={articleRef} className="w-[48rem]">
          <div className="flex flex-col gap-2 flex-grow min-h-[calc(100vh+8rem)]">
            {children}
          </div>
          {showFooter && (
            <ArticleFooter
              previous={previous}
              next={next}
              documentSource={documentSource}
              onLinkClick={handleLinkClick}
            />
          )}
        </article>
        {showToc && (
          <div className="hidden lg:block w-64">
            {contentLoaded ? (
              <TableOfContents
                className="sticky top-8"
                articleRef={articleRef}
              />
            ) : (
              <TocSkeleton />
            )}
          </div>
        )}
      </div>
    </div>
  );
};
