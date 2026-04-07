import { cn } from "@/lib/utils";
import React, { useEffect, useRef } from "react";

type HeadingNode = { id: string; text: string; level: number; offset?: number };

interface TableOfContentsProps {
  articleRef: React.RefObject<HTMLElement | null>;
  show?: boolean;
  onLoadingChange?: (isLoading: boolean) => void;
  headings?: HeadingNode[];
}

const EMPTY_HEADINGS: HeadingNode[] = [];

/** Find the element that actually scrolls (has overflow and scrollable content). */
function getScrollParent(el: HTMLElement | null): HTMLElement | Window {
  if (!el) return window;
  let parent: HTMLElement | null = el.parentElement;
  while (parent) {
    const { overflowY } = getComputedStyle(parent);
    if (
      (overflowY === "auto" || overflowY === "scroll" || overflowY === "overlay") &&
      parent.scrollHeight > parent.clientHeight
    ) {
      return parent;
    }
    parent = parent.parentElement;
  }
  return window;
}

export const TableOfContents: React.FC<TableOfContentsProps> = ({
  articleRef,
  show = true,
  onLoadingChange,
  headings = EMPTY_HEADINGS,
}) => {
  const [navState, dispatchNav] = React.useReducer(
    (
      state: { activeId: string; isUserNavigating: boolean },
      action: Partial<{ activeId: string; isUserNavigating: boolean }>,
    ) => ({ ...state, ...action }),
    { activeId: "", isUserNavigating: false },
  );
  const { activeId, isUserNavigating } = navState;
  const navigationTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const scrollUpdateTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const isUserNavigatingRef = useRef(isUserNavigating);
  const computeActiveIdRef = useRef<() => string>(() => "");

  // Notify parent about loading state (always loaded since we use props)
  useEffect(() => {
    onLoadingChange?.(false);
  }, [onLoadingChange]);

  // Track navigation events to immediately set active state
  useEffect(() => {
    const handleClick = (event: MouseEvent) => {
      const target = event.target as HTMLElement;
      const link = target.closest("a");

      if (link && link.getAttribute("href")?.startsWith("#")) {
        const targetId = link.getAttribute("href")?.substring(1);
        const isTocLink = link.closest("[data-toc-container]");

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
            dispatchNav({ activeId: targetId, isUserNavigating: true });

            // Reset navigation state after scroll completes
            navigationTimeoutRef.current = setTimeout(() => {
              dispatchNav({ isUserNavigating: false });
            }, 1000);

            // Let the browser handle the default scroll behavior for regular links
            // Don't prevent default - let the browser scroll naturally
          }
        }
      }
    };

    document.addEventListener("click", handleClick);
    return () => document.removeEventListener("click", handleClick);
  }, []);

  // Compute active heading from current scroll position (used after debounce)
  const computeActiveId = React.useCallback(() => {
    for (let i = headings.length - 1; i >= 0; i--) {
      const el = document.getElementById(headings[i].id);
      if (el) {
        const top = el.getBoundingClientRect().top;
        if (top <= 100) return headings[i].id;
      }
    }
    return headings[0]?.id ?? "";
  }, [headings]);

  useEffect(() => {
    isUserNavigatingRef.current = isUserNavigating;
  }, [isUserNavigating]);

  useEffect(() => {
    computeActiveIdRef.current = computeActiveId;
  }, [computeActiveId]);

  // Handle active heading highlighting with debounce to avoid jank during fast scroll
  useEffect(() => {
    if (!articleRef.current || headings.length === 0) return;

    const scheduleScrollUpdate = () => {
      if (scrollUpdateTimeoutRef.current) {
        clearTimeout(scrollUpdateTimeoutRef.current);
      }
      scrollUpdateTimeoutRef.current = setTimeout(() => {
        scrollUpdateTimeoutRef.current = null;
        if (!isUserNavigatingRef.current) {
          dispatchNav({ activeId: computeActiveIdRef.current() });
        }
      }, 120);
    };

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((e) => e.isIntersecting)) scheduleScrollUpdate();
      },
      { rootMargin: "0px 0px -80% 0px" },
    );

    headings.forEach((heading) => {
      const element = document.getElementById(heading.id);
      if (element) observer.observe(element);
    });

    // Listen on the actual scroll container (main content div), not window
    const scrollTarget = getScrollParent(articleRef.current);
    scrollTarget.addEventListener("scroll", scheduleScrollUpdate, {
      passive: true,
    });

    // Set initial active section on mount/headings change
    scheduleScrollUpdate();

    return () => {
      observer.disconnect();
      scrollTarget.removeEventListener("scroll", scheduleScrollUpdate);
      if (scrollUpdateTimeoutRef.current) {
        clearTimeout(scrollUpdateTimeoutRef.current);
        scrollUpdateTimeoutRef.current = null;
      }
    };
  }, [headings, articleRef]);

  // Auto-scroll TOC so the active heading is visible when the TOC is scrollable
  useEffect(() => {
    if (!activeId) return;

    const tocContainer = document.querySelector("[data-toc-container]");
    if (!tocContainer) return;

    const activeButton = tocContainer.querySelector(
      `[data-toc-link][data-heading-id="${activeId}"]`,
    ) as HTMLElement | null;
    if (!activeButton) return;

    try {
      const containerRect = tocContainer.getBoundingClientRect();
      const elementRect = activeButton.getBoundingClientRect();
      const isVisible =
        elementRect.top >= containerRect.top && elementRect.bottom <= containerRect.bottom;

      if (!isVisible) {
        activeButton.scrollIntoView({
          behavior: "smooth",
          block: "nearest",
        });
      }
    } catch (error) {
      console.error("TableOfContents: Error during TOC auto-scroll:", error);
    }
  }, [activeId]);

  if (!show) return null;

  if (headings.length === 0) {
    return <div className="text-sm text-muted-foreground">No headings found</div>;
  }

  return (
    <div
      className="flex-1 min-h-48 overflow-y-auto overflow-x-hidden slim-scrollbar"
      data-toc-container
    >
      <nav className="relative pr-2">
        {headings.map((heading) => (
          <button
            key={heading.id}
            type="button"
            data-toc-link
            data-heading-id={heading.id}
            className={cn(
              "block text-sm py-1 hover:text-foreground transition-colors w-full text-left",
              heading.level === 1
                ? "pl-0"
                : heading.level === 2
                  ? "pl-2"
                  : heading.level === 3
                    ? "pl-4"
                    : heading.level === 4
                      ? "pl-6"
                      : heading.level === 5
                        ? "pl-8"
                        : "pl-10",
              activeId === heading.id ? "text-foreground" : "text-muted-foreground",
            )}
            onClick={() => {
              // Clear any existing navigation timeout
              if (navigationTimeoutRef.current) {
                clearTimeout(navigationTimeoutRef.current);
              }

              // Set as active immediately and mark as user navigating
              dispatchNav({ activeId: heading.id, isUserNavigating: true });

              // Reset navigation state after scroll completes
              navigationTimeoutRef.current = setTimeout(() => {
                dispatchNav({ isUserNavigating: false });
              }, 1000);

              // Scroll to the target element with error handling
              const targetElement = document.getElementById(heading.id);
              if (targetElement) {
                targetElement.scrollIntoView({
                  behavior: "smooth",
                });
              } else {
                console.warn(
                  `TableOfContents: Target element with id "${heading.id}" not found for TOC navigation`,
                );
              }
            }}
          >
            {heading.text}
          </button>
        ))}
      </nav>
    </div>
  );
};
