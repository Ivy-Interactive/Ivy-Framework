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
}

export const TableOfContents: React.FC<TableOfContentsProps> = ({
  articleRef,
  show = true,
  onLoadingChange,
}) => {
  const [tocItems, setTocItems] = useState<TocItem[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [activeId, setActiveId] = useState<string>('');
  const [isManualScrolling, setIsManualScrolling] = useState(false);
  const timeoutRef = useRef<NodeJS.Timeout | null>(null);
  const manualScrollTimeoutRef = useRef<NodeJS.Timeout | null>(null);

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
            onLoadingChange?.(false);
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
        onLoadingChange?.(false);
      }, remainingTime);
    };

    // Small delay to ensure content is rendered
    timeoutRef.current = setTimeout(extractHeadings, 50);

    // Cleanup function
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
      if (manualScrollTimeoutRef.current) {
        clearTimeout(manualScrollTimeoutRef.current);
      }
    };
  }, [show, articleRef, onLoadingChange]);

  // Handle active heading highlighting and auto-scroll
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
  }, [tocItems, articleRef]);

  // Auto-scroll TOC to show active heading (but not during manual scrolling)
  useEffect(() => {
    if (!activeId || isManualScrolling) return;

    const activeElement = document.querySelector(`a[href="#${activeId}"]`);
    if (activeElement) {
      activeElement.scrollIntoView({
        behavior: 'smooth',
        block: 'nearest',
        // Removed inline: 'nearest' to prevent horizontal scrolling
      });
    }
  }, [activeId, isManualScrolling]);

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
    <div className="flex-1 min-h-48 overflow-y-auto overflow-x-hidden scrollbar-thin scrollbar-thumb-border scrollbar-track-transparent">
      <nav className="relative pr-2">
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

              // Set manual scrolling mode to prevent TOC auto-scroll interference
              setIsManualScrolling(true);

              // Clear any existing timeout
              if (manualScrollTimeoutRef.current) {
                clearTimeout(manualScrollTimeoutRef.current);
              }

              // Scroll to the target element
              document.getElementById(heading.id)?.scrollIntoView({
                behavior: 'smooth',
              });

              // Re-enable auto-scroll after scrolling is likely complete
              manualScrollTimeoutRef.current = setTimeout(() => {
                setIsManualScrolling(false);
              }, 1000);
            }}
          >
            {heading.text}
          </a>
        ))}
      </nav>
    </div>
  );
};
