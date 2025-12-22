import { cn } from '@/lib/utils';
import React, { useEffect, useState, useRef } from 'react';

interface TableOfContentsProps {
  articleRef: React.RefObject<HTMLElement | null>;
  show?: boolean;
  onLoadingChange?: (isLoading: boolean) => void;
  headings?: { id: string; text: string; level: number }[];
}

export const TableOfContents: React.FC<TableOfContentsProps> = ({
  articleRef,
  show = true,
  onLoadingChange,
  headings = [],
}) => {
  const [activeId, setActiveId] = useState<string>('');
  const [isUserNavigating, setIsUserNavigating] = useState(false);
  const navigationTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  // Notify parent about loading state (always loaded since we use props)
  useEffect(() => {
    onLoadingChange?.(false);
  }, [onLoadingChange]);

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
          }
        }
      }
    };

    document.addEventListener('click', handleClick);
    return () => document.removeEventListener('click', handleClick);
  }, []);

  // Handle active heading highlighting
  useEffect(() => {
    if (!articleRef.current || headings.length === 0) return;

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

    // Observe elements corresponding to the headings
    headings.forEach(heading => {
      const element = document.getElementById(heading.id);
      if (element) {
        observer.observe(element);
      }
    });

    return () => observer.disconnect();
  }, [headings, articleRef, isUserNavigating]);

  // Smart TOC auto-scroll - scroll TOC to show active item
  useEffect(() => {
    if (!activeId) return;

    // Find the TOC link for the active heading
    const tocContainer = document.querySelector('[data-toc-container]');
    if (!tocContainer) {
      return;
    }

    const activeElement = tocContainer.querySelector(`a[href="#${activeId}"]`);
    if (!activeElement) {
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

  if (headings.length === 0) {
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
        {headings.map(heading => (
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
