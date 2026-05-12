import { useCallback, useEffect, useRef, useState } from "react";

interface ScrollState {
  isAtBottom: boolean;
  autoScrollEnabled: boolean;
}

interface UseAutoScrollOptions {
  offset?: number;
  smooth?: boolean;
  content?: unknown;
  enabled?: boolean;
}

export function useAutoScroll(options: UseAutoScrollOptions = {}) {
  const { offset = 20, smooth = false, content, enabled = true } = options;
  const scrollRef = useRef<HTMLDivElement>(null);
  const lastContentHeight = useRef(0);
  const userHasScrolled = useRef(false);
  const prevEnabledRef = useRef(enabled);

  const [scrollState, setScrollState] = useState<ScrollState>({
    isAtBottom: true,
    autoScrollEnabled: true,
  });

  const checkIsAtBottom = useCallback(
    (element: HTMLElement) => {
      const { scrollTop, scrollHeight, clientHeight } = element;
      const distanceToBottom = Math.abs(scrollHeight - scrollTop - clientHeight);
      return distanceToBottom <= offset;
    },
    [offset],
  );

  const scrollToBottom = useCallback(
    (instant?: boolean) => {
      if (!scrollRef.current) return;

      const targetScrollTop = scrollRef.current.scrollHeight - scrollRef.current.clientHeight;

      if (instant) {
        scrollRef.current.scrollTop = targetScrollTop;
      } else {
        scrollRef.current.scrollTo({
          top: targetScrollTop,
          behavior: smooth ? "smooth" : "auto",
        });
      }

      setScrollState({
        isAtBottom: true,
        autoScrollEnabled: true,
      });
      userHasScrolled.current = false;
    },
    [smooth],
  );

  const handleScroll = useCallback(() => {
    if (!scrollRef.current) return;

    const atBottom = checkIsAtBottom(scrollRef.current);

    setScrollState((prev) => ({
      isAtBottom: atBottom,
      autoScrollEnabled: atBottom ? true : prev.autoScrollEnabled,
    }));
  }, [checkIsAtBottom]);

  useEffect(() => {
    const element = scrollRef.current;
    if (!element) return;

    element.addEventListener("scroll", handleScroll, { passive: true });
    return () => element.removeEventListener("scroll", handleScroll);
  }, [handleScroll]);

  useEffect(() => {
    const scrollElement = scrollRef.current;
    if (!scrollElement) return;

    const currentHeight = scrollElement.scrollHeight;
    const hasNewContent = currentHeight !== lastContentHeight.current;

    if (hasNewContent) {
      if (enabled && scrollState.autoScrollEnabled) {
        requestAnimationFrame(() => {
          scrollToBottom(lastContentHeight.current === 0);
        });
      }
      lastContentHeight.current = currentHeight;
    }
  }, [content, scrollState.autoScrollEnabled, scrollToBottom, enabled]);

  useEffect(() => {
    const element = scrollRef.current;
    if (!element) return;

    const resizeObserver = new ResizeObserver(() => {
      if (enabled && scrollState.autoScrollEnabled) {
        scrollToBottom(true);
      }
    });

    resizeObserver.observe(element);

    // Observe every descendant whose size could push scrollHeight: any element
    // expanding inside the body (a tool card opening, an image loading, streaming
    // output) should trigger a re-pin to the bottom while the user is at the bottom.
    const observed = new WeakSet<Element>();
    const observeAll = (root: Element) => {
      for (const child of Array.from(root.children)) {
        if (!observed.has(child)) {
          resizeObserver.observe(child);
          observed.add(child);
        }
        observeAll(child);
      }
    };
    observeAll(element);

    const mutationObserver = new MutationObserver((mutations) => {
      for (const m of mutations) {
        m.addedNodes.forEach((n) => {
          if (n.nodeType === Node.ELEMENT_NODE) observeAll(n as Element);
        });
      }
    });
    mutationObserver.observe(element, { childList: true, subtree: true });

    return () => {
      resizeObserver.disconnect();
      mutationObserver.disconnect();
    };
  }, [scrollState.autoScrollEnabled, scrollToBottom, enabled]);

  useEffect(() => {
    if (enabled && !prevEnabledRef.current) {
      requestAnimationFrame(() => scrollToBottom(true));
    }
    prevEnabledRef.current = enabled;
  }, [enabled, scrollToBottom]);

  const disableAutoScroll = useCallback(() => {
    const atBottom = scrollRef.current ? checkIsAtBottom(scrollRef.current) : false;

    if (!atBottom) {
      userHasScrolled.current = true;
      setScrollState((prev) => ({
        ...prev,
        autoScrollEnabled: false,
      }));
    }
  }, [checkIsAtBottom]);

  return {
    scrollRef,
    isAtBottom: scrollState.isAtBottom,
    autoScrollEnabled: scrollState.autoScrollEnabled,
    scrollToBottom: () => scrollToBottom(false),
    disableAutoScroll,
  };
}
