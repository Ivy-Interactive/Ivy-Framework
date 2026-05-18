import React from "react";
import { useEventHandler } from "@/components/event-handler";
import { getTabProps } from "../utils/tabUtils";

/**
 * Hook to manage all tab-related state, refs, and synchronization
 * Combines state management and backend synchronization logic
 */
export function useTabManagement(
  tabWidgets: React.ReactElement[],
  selectedIndex: number,
  events: string[],
  id: string,
) {
  // ====================
  // State & Refs Setup
  // ====================
  const [dropdownOpen, setDropdownOpen] = React.useState(false);
  const [visibleTabs, setVisibleTabs] = React.useState<string[]>([]);
  const [hiddenTabs, setHiddenTabs] = React.useState<string[]>([]);

  const initialTabOrder = React.useMemo(
    () =>
      tabWidgets.reduce<string[]>((acc, tab) => {
        const id = getTabProps(tab)?.id;
        if (id !== undefined) acc.push(id);
        return acc;
      }, []),
    // oxlint-disable-next-line react-hooks/exhaustive-deps
    [], // Only run on mount
  );

  const [tabOrder, setTabOrder] = React.useState<string[]>(initialTabOrder);

  const [activeTabId, setActiveTabId] = React.useState<string | null>(
    () => initialTabOrder[selectedIndex] ?? initialTabOrder[0] ?? null,
  );

  const [loadedTabs, setLoadedTabs] = React.useState<Set<string>>(() => {
    const initialActiveTab = initialTabOrder[selectedIndex] ?? initialTabOrder[0] ?? null;
    return initialActiveTab ? new Set([initialActiveTab]) : new Set();
  });

  const [activeIndex, setActiveIndex] = React.useState(selectedIndex ?? 0);

  // Refs for stable references
  const activeTabIdRef = React.useRef<string | null>(activeTabId);
  const containerRef = React.useRef<HTMLDivElement>(null);
  const tabsListRef = React.useRef<HTMLDivElement>(null);
  const tabRefs = React.useRef<(HTMLButtonElement | null)[]>([]);
  const tabWidgetsRef = React.useRef(tabWidgets);
  const tabOrderRef = React.useRef(tabOrder);
  const isDraggingRef = React.useRef(false);
  const isUserInitiatedChangeRef = React.useRef(false);
  const tabMeasurementsRef = React.useRef<Map<string, number>>(new Map());

  // Event handler setup
  const eventHandler = useEventHandler();
  const eventHandlerRef = React.useRef(eventHandler);

  const safeEvent = React.useCallback(
    (
      name:
        | "OnSelect"
        | "OnClose"
        | "OnCloseOthers"
        | "OnRefresh"
        | "OnReorder"
        | "OnAddButtonClick",
      args: unknown[],
    ) => {
      if (Array.isArray(events) && events.includes(name)) {
        eventHandler(name, id, args);
      }
    },
    [events, eventHandler, id],
  );

  // ====================
  // Synchronization Logic
  // ====================

  // Update refs when they change
  React.useEffect(() => {
    activeTabIdRef.current = activeTabId;
  }, [activeTabId]);

  React.useEffect(() => {
    tabWidgetsRef.current = tabWidgets;
  }, [tabWidgets]);

  React.useEffect(() => {
    tabOrderRef.current = tabOrder;
  }, [tabOrder]);

  React.useEffect(() => {
    eventHandlerRef.current = eventHandler;
  }, [eventHandler]);

  const currentTabIds = React.useMemo(
    () =>
      tabWidgets.reduce<string[]>((acc, tab) => {
        const id = getTabProps(tab)?.id;
        if (id !== undefined) acc.push(id);
        return acc;
      }, []),
    [tabWidgets],
  );

  const added = currentTabIds.filter((id) => !tabOrder.includes(id));
  const removed = tabOrder.filter((id) => !currentTabIds.includes(id));

  if (added.length || removed.length) {
    if (activeTabId && removed.includes(activeTabId)) {
      const oldIndex = tabOrder.indexOf(activeTabId);
      const newIdAtSamePos = currentTabIds[oldIndex];
      if (newIdAtSamePos && added.includes(newIdAtSamePos)) {
        setActiveTabId(newIdAtSamePos);
        setLoadedTabs((prev) => {
          const newSet = new Set(prev);
          newSet.add(newIdAtSamePos);
          return newSet;
        });
      }
    }
    setTabOrder(currentTabIds);
  }

  const syncActiveTabId = React.useCallback(() => {
    if (selectedIndex != null && tabOrder[selectedIndex]) {
      const targetTabId = tabOrder[selectedIndex];
      // Only sync if it's not user-initiated OR if the current activeTabId is invalid
      if (!isUserInitiatedChangeRef.current || !activeTabId || !tabOrder.includes(activeTabId)) {
        if (targetTabId !== activeTabId) {
          setLoadedTabs((prev) => {
            if (prev.has(targetTabId)) return prev;
            const newSet = new Set(prev);
            newSet.add(targetTabId);
            return newSet;
          });
          setActiveTabId(targetTabId);
          // Update activeIndex for Content variant animation
          setActiveIndex(selectedIndex);
        }
      }
    }
  }, [selectedIndex, tabOrder, activeTabId, setActiveTabId, setActiveIndex]);

  // Sync activeTabId with selectedIndex prop from backend (only when not user-initiated)
  React.useEffect(() => {
    syncActiveTabId();
  }, [syncActiveTabId]);

  const resetUserInitiatedFlag = React.useCallback(() => {
    if (isUserInitiatedChangeRef.current) {
      isUserInitiatedChangeRef.current = false;
    }
  }, []);

  // Reset user-initiated flag when tabWidgets changes (backend response received)
  React.useEffect(() => {
    resetUserInitiatedFlag();
  }, [tabWidgets, resetUserInitiatedFlag]);

  const loadActiveTab = React.useCallback(() => {
    if (activeTabId) {
      setLoadedTabs((prev) => {
        if (prev.has(activeTabId)) return prev;
        const newSet = new Set(prev);
        newSet.add(activeTabId);
        return newSet;
      });
    }
  }, [activeTabId]);

  // Load active tab only when it becomes active
  React.useEffect(() => {
    loadActiveTab();
  }, [loadActiveTab]);

  const addToLoadedTabs = React.useCallback(
    (tabId: string) => {
      setLoadedTabs((prev) => {
        if (prev.has(tabId)) return prev;
        const newSet = new Set(prev);
        newSet.add(tabId);
        return newSet;
      });
    },
    [setLoadedTabs],
  );

  return {
    // State
    dropdownOpen,
    setDropdownOpen,
    visibleTabs,
    setVisibleTabs,
    hiddenTabs,
    setHiddenTabs,
    tabOrder,
    setTabOrder,
    activeTabId,
    setActiveTabId,
    loadedTabs,
    setLoadedTabs,
    activeIndex,
    setActiveIndex,
    // Refs
    activeTabIdRef,
    containerRef,
    tabsListRef,
    tabRefs,
    tabWidgetsRef,
    tabOrderRef,
    eventHandlerRef,
    isDraggingRef,
    isUserInitiatedChangeRef,
    tabMeasurementsRef,
    // Utilities
    safeEvent,
    addToLoadedTabs,
  };
}
