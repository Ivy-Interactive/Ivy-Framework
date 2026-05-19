import { cn } from "@/lib/utils";
import React, { useEffect, useLayoutEffect, useRef } from "react";

type HeadingNode = { id: string; text: string; level: number; offset?: number };

interface TableOfContentsProps {
  articleRef: React.RefObject<HTMLElement | null>;
  show?: boolean;
  onLoadingChange?: (isLoading: boolean) => void;
  headings?: HeadingNode[];
}

const EMPTY_HEADINGS: HeadingNode[] = [];

function getLocationHashId(): string | null {
  const raw = window.location.hash;
  if (!raw || raw === "#") return null;
  const withoutHash = raw.slice(1);
  try {
    return decodeURIComponent(withoutHash.replace(/\+/g, " "));
  } catch {
    return withoutHash;
  }
}

/** Don’t sync URL to the first section while a real #fragment is still valid in the DOM. */
function replaceUrlHashForActiveSection(
  headingId: string,
  firstHeadingId: string | undefined,
  force?: boolean,
) {
  if (getLocationHashId() === headingId) return;
  if (!force && firstHeadingId && headingId === firstHeadingId) {
    const frag = getLocationHashId();
    if (frag && frag !== firstHeadingId && document.getElementById(frag)) return;
  }
  const u = new URL(window.location.href);
  u.hash = headingId;
  window.history.replaceState(null, "", `${u.pathname}${u.search}${u.hash}`);
}

function getScrollParent(el: HTMLElement | null): HTMLElement | Window {
  if (!el) return window;
  let parent: HTMLElement | null = el.parentElement;
  while (parent) {
    const { overflowY } = getComputedStyle(parent);
    if (overflowY === "auto" || overflowY === "scroll" || overflowY === "overlay") return parent;
    parent = parent.parentElement;
  }
  return window;
}

function scrollHeadingIntoView(
  el: HTMLElement,
  articleRoot: HTMLElement | null,
  behavior: ScrollBehavior,
): void {
  const scrollParent = getScrollParent(articleRoot ?? el);
  if (scrollParent === window) {
    el.scrollIntoView({ behavior, block: "start" });
    return;
  }
  const parent = scrollParent as HTMLElement;
  const delta = el.getBoundingClientRect().top - parent.getBoundingClientRect().top;
  const margin = parseFloat(getComputedStyle(el).scrollMarginTop) || 0;
  parent.scrollTo({ top: Math.max(0, parent.scrollTop + delta - margin), behavior });
}

export const TableOfContents: React.FC<TableOfContentsProps> = ({
  articleRef,
  show = true,
  onLoadingChange,
  headings = EMPTY_HEADINGS,
}) => {
  const [navState, dispatchNav] = React.useReducer(
    (
      state: { activeId: string; isUserNavigating: boolean; hashSyncUnlocked: boolean },
      action: Partial<{ activeId: string; isUserNavigating: boolean; hashSyncUnlocked: boolean }>,
    ) => ({ ...state, ...action }),
    { activeId: "", isUserNavigating: false, hashSyncUnlocked: !getLocationHashId() },
  );
  const { activeId, isUserNavigating, hashSyncUnlocked } = navState;
  const hashSyncUnlockedRef = useRef(hashSyncUnlocked);
  const navigationTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const scrollUpdateTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const isUserNavigatingRef = useRef(isUserNavigating);
  const computeActiveIdRef = useRef<() => string>(() => "");

  const headingIdsKey = headings.map((h) => h.id).join("\0");
  const firstHeadingId = headings[0]?.id;

  useEffect(() => {
    hashSyncUnlockedRef.current = hashSyncUnlocked;
  }, [hashSyncUnlocked]);

  const handleHashSync = React.useCallback(() => {
    if (!getLocationHashId()) {
      dispatchNav({ hashSyncUnlocked: true });
      return;
    }
    dispatchNav({ hashSyncUnlocked: false });
    const t = window.setTimeout(() => dispatchNav({ hashSyncUnlocked: true }), 500);
    return () => clearTimeout(t);
  }, [headingIdsKey]);

  useLayoutEffect(() => {
    return handleHashSync();
  }, [handleHashSync]);

  useEffect(() => {
    onLoadingChange?.(false);
  }, [onLoadingChange]);

  useEffect(() => {
    const handleClick = (event: MouseEvent) => {
      const link = (event.target as HTMLElement).closest("a");
      if (!link?.getAttribute("href")?.startsWith("#")) return;
      const targetId = link.getAttribute("href")?.substring(1);
      if (!targetId || link.closest("[data-toc-container]")) return;
      const targetElement = document.getElementById(targetId);
      if (!targetElement) return;
      if (navigationTimeoutRef.current) clearTimeout(navigationTimeoutRef.current);
      dispatchNav({ activeId: targetId, isUserNavigating: true });
      navigationTimeoutRef.current = setTimeout(
        () => dispatchNav({ isUserNavigating: false }),
        1000,
      );
    };
    document.addEventListener("click", handleClick);
    return () => document.removeEventListener("click", handleClick);
  }, []);

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

  const handleActiveIdSync = React.useCallback(() => {
    if (!hashSyncUnlocked || activeId) return;
    const frag = getLocationHashId();
    if (frag && document.getElementById(frag)) {
      dispatchNav({ activeId: frag });
      return;
    }
    const id = computeActiveIdRef.current();
    if (id) dispatchNav({ activeId: id });
  }, [hashSyncUnlocked, activeId]);

  useEffect(() => {
    handleActiveIdSync();
  }, [handleActiveIdSync]);

  useEffect(() => {
    if (!activeId || isUserNavigating || !hashSyncUnlocked) return;
    replaceUrlHashForActiveSection(activeId, firstHeadingId);
  }, [activeId, isUserNavigating, hashSyncUnlocked, firstHeadingId]);

  useEffect(() => {
    if (!articleRef.current || headings.length === 0) return;

    const scheduleScrollUpdate = () => {
      if (scrollUpdateTimeoutRef.current) clearTimeout(scrollUpdateTimeoutRef.current);
      scrollUpdateTimeoutRef.current = setTimeout(() => {
        scrollUpdateTimeoutRef.current = null;
        if (!isUserNavigatingRef.current && hashSyncUnlockedRef.current) {
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

    const scrollTarget = getScrollParent(articleRef.current);
    scrollTarget.addEventListener("scroll", scheduleScrollUpdate, { passive: true });
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
      if (elementRect.top < containerRect.top || elementRect.bottom > containerRect.bottom) {
        activeButton.scrollIntoView({ behavior: "smooth", block: "nearest" });
      }
    } catch (e) {
      console.error("TableOfContents: TOC auto-scroll:", e);
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
              if (navigationTimeoutRef.current) clearTimeout(navigationTimeoutRef.current);
              dispatchNav({ activeId: heading.id, isUserNavigating: true });
              navigationTimeoutRef.current = setTimeout(
                () => dispatchNav({ isUserNavigating: false }),
                1000,
              );
              const targetElement = document.getElementById(heading.id);
              if (targetElement) {
                replaceUrlHashForActiveSection(heading.id, firstHeadingId, true);
                scrollHeadingIntoView(targetElement, articleRef.current, "smooth");
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
