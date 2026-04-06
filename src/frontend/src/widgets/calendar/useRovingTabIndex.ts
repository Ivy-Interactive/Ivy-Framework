import { useState, useRef, useCallback, useEffect } from "react";

export function useRovingTabIndex(itemCount: number) {
  const [focusedIndex, setFocusedIndex] = useState(0);
  const itemsRef = useRef<(HTMLElement | null)[]>([]);

  useEffect(() => {
    setFocusedIndex(0);
    itemsRef.current = itemsRef.current.slice(0, itemCount);
  }, [itemCount]);

  const setItemRef = useCallback(
    (index: number) => (el: HTMLElement | null) => {
      itemsRef.current[index] = el;
    },
    [],
  );

  const getTabIndex = useCallback(
    (index: number) => (index === focusedIndex ? 0 : -1),
    [focusedIndex],
  );

  const onKeyDown = useCallback(
    (e: React.KeyboardEvent, index: number) => {
      let nextIndex: number | null = null;
      switch (e.key) {
        case "ArrowDown":
        case "ArrowRight":
          nextIndex = (index + 1) % itemCount;
          break;
        case "ArrowUp":
        case "ArrowLeft":
          nextIndex = (index - 1 + itemCount) % itemCount;
          break;
        case "Home":
          nextIndex = 0;
          break;
        case "End":
          nextIndex = itemCount - 1;
          break;
        default:
          return;
      }
      e.preventDefault();
      setFocusedIndex(nextIndex);
      itemsRef.current[nextIndex]?.focus();
    },
    [itemCount],
  );

  return { setItemRef, getTabIndex, onKeyDown };
}
