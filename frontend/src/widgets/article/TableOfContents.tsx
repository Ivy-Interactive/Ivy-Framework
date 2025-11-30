import { cn } from '@/lib/utils';
import React, { useEffect, useState, useRef } from 'react';

export type TocItem = {
  id: string;
  text: string;
  level: number;
};

interface TableOfContentsProps {
  articleRef: React.RefObject<HTMLElement | null>;
  show?: boolean;
  onLoadingChange?: (isLoading: boolean) => void;
  /** Delay in milliseconds before attempting to extract headings (default: 50ms) */
  extractionDelay?: number;
  /** Maximum number of retry attempts for heading extraction (default: 15) */
  maxExtractionRetries?: number;
}

export const TableOfContents: React.FC<TableOfContentsProps> = ({
  articleRef,
  show = true,
  onLoadingChange,
  extractionDelay = 50,
  maxExtractionRetries = 15,
}) => {
  const [tocItems, setTocItems] = useState<TocItem[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [activeId, setActiveId] = useState<string>('');
  const [isUserNavigating, setIsUserNavigating] = useState(false);
  const timeoutRef = useRef<NodeJS.Timeout | null>(null);
  const navigationTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  // Extract headings and manage loading state
  useEffect(() => {
    // Clear any existing timeout
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }

    if (!show) {
      setTocItems([]);
      setIsLoading(false);
      onLoadingChange?.(false);
      return;
    }

    // Reset states when starting fresh
    setTocItems([]);
    setIsLoading(true);
    onLoadingChange?.(true);

    const startTime = Date.now();
    const minLoadingTime = 500; // 0.5 seconds minimum loading time for smooth transition
    let retryCount = 0;

    const extractHeadings = () => {
      const articleElement = articleRef.current;
      if (!articleElement) {
        // If article ref is not available yet, try again after a short delay
        if (retryCount < maxExtractionRetries) {
          retryCount++;
          timeoutRef.current = setTimeout(extractHeadings, 100);
        } else {
          // Stop loading if max retries reached, respecting minimum loading time
          console.warn(
            'TableOfContents: Failed to find article element after maximum retries'
          );
          const elapsedTime = Date.now() - startTime;
          const remainingTime = Math.max(0, minLoadingTime - elapsedTime);
          timeoutRef.current = setTimeout(() => {
            setIsLoading(false);
            onLoadingChange?.(false);
          }, remainingTime);
        }
        return;
      }

      // Get all headings, but exclude those inside example boxes
      const allHeadings = Array.from(
        articleElement.querySelectorAll('h1, h2, h3, h4, h5, h6')
      );

      // Filter out headings that are inside example boxes
      const elements = allHeadings.filter(heading => {
        // Check if the heading is inside a demo tab
        const isInsideDemoTab = (() => {
          const tabPanel = heading.closest('[role="tabpanel"]');
          if (!tabPanel) return false;

          // Find the TabsList within the same parent as the tabPanel
          const tabsList =
            tabPanel.parentElement?.querySelector('[role="tablist"]');
          if (!tabsList) return false;

          // Check if any tab button within this TabsList has the text "Demo"
          const demoTab = Array.from(
            tabsList.querySelectorAll('[role="tab"]')
          ).find(tab => tab.textContent?.trim() === 'Demo');

          return demoTab !== undefined;
        })();
        return !isInsideDemoTab;
      });

      // If no headings found but content might still be loading, retry
      if (elements.length === 0 && retryCount < maxExtractionRetries) {
        retryCount++;
        timeoutRef.current = setTimeout(extractHeadings, 100);
        return;
      }

      const items = elements.map(element => {
        // Don't override IDs that already exist (from rehype-slug)
        // Only generate ID if doesn't exist and use the same algorithm as rehype-slug
        if (!element.id) {
          element.id =
            element.textContent
              ?.toLowerCase()
              .trim()
              .replace(/\s+/g, '-')
              .replace(/[^\w\u00C0-\u024F\u1E00-\u1EFF-]/g, '') ?? '';
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
        onLoadingChange?.(false);
      }, remainingTime);
    };

    // Configurable delay to ensure content is rendered
    timeoutRef.current = setTimeout(extractHeadings, extractionDelay);

    // Cleanup function
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
      if (navigationTimeoutRef.current) {
        clearTimeout(navigationTimeoutRef.current);
      }
    };
  }, [
    show,
    articleRef,
    onLoadingChange,
    extractionDelay,
    maxExtractionRetries,
  ]);

  // Track navigation events to immediately set active state
  useEffect(() => {
    const handleClick = (event: MouseEvent) => {
      const target = event.target as HTMLElement;
      const link = target.closest('a');

      if (link && link.getAttribute('href')?.startsWith('#')) {
        const targetId = link.getAttribute('href')?.substring(1);
        const isTocLink = link.closest('[data-toc-container]');

        if (targetId && !isTocLink) {
          // This is a regular section link (not from TOC)
          // Verify target exists before setting active
          const targetElement = document.getElementById(targetId);
          if (targetElement) {
            // Clear any existing navigation timeout
            if (navigationTimeoutRef.current) {
              clearTimeout(navigationTimeoutRef.current);
            }

            // Set as active immediately and mark as user navigating
            setActiveId(targetId);
            setIsUserNavigating(true);

            // Reset navigation state after scroll completes
            navigationTimeoutRef.current = setTimeout(() => {
              setIsUserNavigating(false);
            }, 1000);

            // Let the browser handle the default scroll behavior for regular links
            // Don't prevent default - let the browser scroll naturally
          } else {
            console.warn(
              `TableOfContents: Target element with id "${targetId}" not found`
            );
          }
        }
      }
    };

    document.addEventListener('click', handleClick);
    return () => document.removeEventListener('click', handleClick);
  }, []);

  // Handle active heading highlighting
  useEffect(() => {
    if (!articleRef.current || tocItems.length === 0) return;

    const articleElement = articleRef.current;

    // Find all heading elements in the DOM, excluding those inside example boxes
    const allHeadings = Array.from(
      articleElement.querySelectorAll('h1, h2, h3, h4, h5, h6')
    );

    // Filter out headings that are inside example boxes
    const elements = allHeadings.filter(heading => {
      // Check if the heading is inside a demo tab
      const isInsideDemoTab = (() => {
        const tabPanel = heading.closest('[role="tabpanel"]');
        if (!tabPanel) return false;

        // Find the TabsList within the same parent as the tabPanel
        const tabsList =
          tabPanel.parentElement?.querySelector('[role="tablist"]');
        if (!tabsList) return false;

        // Check if any tab button within this TabsList has the text "Demo"
        const demoTab = Array.from(
          tabsList.querySelectorAll('[role="tab"]')
        ).find(tab => tab.textContent?.trim() === 'Demo');

        return demoTab !== undefined;
      })();
      return !isInsideDemoTab;
    });

    const observer = new IntersectionObserver(
      entries => {
        // Don't update active ID if user is currently navigating
        if (!isUserNavigating) {
          entries.forEach(entry => {
            if (entry.isIntersecting) {
              setActiveId(entry.target.id);
            }
          });
        }
      },
      { rootMargin: '0px 0px -80% 0px' }
    );

    elements.forEach(element => observer.observe(element));

    return () => observer.disconnect();
  }, [tocItems, articleRef, isUserNavigating]);

  // Smart TOC auto-scroll - scroll TOC to show active item
  useEffect(() => {
    if (!activeId) return;

    // Find the TOC link for the active heading
    const tocContainer = document.querySelector('[data-toc-container]');
    if (!tocContainer) {
      console.warn('TableOfContents: TOC container not found for auto-scroll');
      return;
    }

    const activeElement = tocContainer.querySelector(`a[href="#${activeId}"]`);
    if (!activeElement) {
      console.warn(
        `TableOfContents: TOC link for "${activeId}" not found for auto-scroll`
      );
      return;
    }

    try {
      // Check if the active element is already visible in the TOC
      const containerRect = tocContainer.getBoundingClientRect();
      const elementRect = activeElement.getBoundingClientRect();

      const isVisible =
        elementRect.top >= containerRect.top &&
        elementRect.bottom <= containerRect.bottom;

      // Only scroll if the active element is not visible
      if (!isVisible) {
        activeElement.scrollIntoView({
          behavior: 'smooth',
          block: 'nearest',
        });
      }
    } catch (error) {
      console.error('TableOfContents: Error during auto-scroll:', error);
    }
  }, [activeId]);

  if (!show) return null;

  if (isLoading) {
    return (
      <div className="flex-1 min-h-48 overflow-hidden">
        <div className="flex flex-col gap-4 pr-2">
          <div className="h-3 bg-muted rounded animate-pulse w-3/4"></div>
          <div className="h-3 bg-muted rounded animate-pulse w-full"></div>
          <div className="h-3 bg-muted rounded animate-pulse w-5/6"></div>
          <div className="h-3 bg-muted rounded animate-pulse w-2/3"></div>
          <div className="h-3 bg-muted rounded animate-pulse w-4/5"></div>
          <div className="h-3 bg-muted rounded animate-pulse w-1/2"></div>
        </div>
      </div>
    );
  }

  if (tocItems.length === 0) {
    return (
      <div className="text-sm text-muted-foreground">No headings found</div>
    );
  }

  return (
    <div
      className="flex-1 min-h-48 overflow-y-auto overflow-x-hidden scrollbar-thin scrollbar-thumb-border scrollbar-track-transparent"
      data-toc-container
    >
      <nav className="relative pr-2">
        {tocItems.map(heading => (
          <a
            key={heading.id}
            href={`#${heading.id}`}
            data-toc-link
            className={cn(
              'block text-sm py-1 hover:text-foreground transition-colors',
              heading.level === 1 ? 'pl-0' : `pl-${(heading.level - 1) * 4}`,
              activeId === heading.id
                ? 'text-foreground'
                : 'text-muted-foreground'
            )}
            onClick={e => {
              e.preventDefault();

              // Clear any existing navigation timeout
              if (navigationTimeoutRef.current) {
                clearTimeout(navigationTimeoutRef.current);
              }

              // Set as active immediately and mark as user navigating
              setActiveId(heading.id);
              setIsUserNavigating(true);

              // Reset navigation state after scroll completes
              navigationTimeoutRef.current = setTimeout(() => {
                setIsUserNavigating(false);
              }, 1000);

              // Scroll to the target element with error handling
              const targetElement = document.getElementById(heading.id);
              if (targetElement) {
                targetElement.scrollIntoView({
                  behavior: 'smooth',
                });
              } else {
                console.warn(
                  `TableOfContents: Target element with id "${heading.id}" not found for TOC navigation`
                );
              }
            }}
          >
            {heading.text}
          </a>
        ))}
      </nav>
    </div>
  );
};
