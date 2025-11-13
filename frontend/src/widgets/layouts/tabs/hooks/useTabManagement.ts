import React from 'react';
import { useEventHandler } from '@/components/event-handler';
import type { TabWidgetProps } from '../types';

/**
 * Hook to manage all tab-related state, refs, and synchronization
 * Combines state management and backend synchronization logic
 */
export function useTabManagement(
  tabWidgets: React.ReactElement[],
  selectedIndex: number,
  events: string[],
  id: string
) {
  // ====================
  // State & Refs Setup
  // ====================
  const [dropdownOpen, setDropdownOpen] = React.useState(false);
  const [visibleTabs, setVisibleTabs] = React.useState<string[]>([]);
  const [hiddenTabs, setHiddenTabs] = React.useState<string[]>([]);

  const initialTabOrder = React.useMemo(
    () =>
      tabWidgets.map(
        tab => (tab as React.ReactElement<{ id: string }>).props.id
      ),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [] // Only run on mount
  );

  const [tabOrder, setTabOrder] = React.useState<string[]>(initialTabOrder);

  const [activeTabId, setActiveTabId] = React.useState<string | null>(
    () => initialTabOrder[selectedIndex] ?? initialTabOrder[0] ?? null
  );

  const [loadedTabs, setLoadedTabs] = React.useState<Set<string>>(() => {
    const initialActiveTab =
      initialTabOrder[selectedIndex] ?? initialTabOrder[0] ?? null;
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
        | 'OnSelect'
        | 'OnClose'
        | 'OnRefresh'
        | 'OnReorder'
        | 'OnAddButtonClick',
      args: unknown[]
    ) => {
      if (Array.isArray(events) && events.includes(name)) {
        eventHandler(name, id, args);
      }
    },
    [events, eventHandler, id]
  );

  // ====================
  // Synchronization Logic
  // ====================

  // Helper function to efficiently add tab to loaded tabs
  const addToLoadedTabs = React.useCallback(
    (tabId: string) => {
      setLoadedTabs(prev => {
        if (prev.has(tabId)) {
          return prev; // Return the same Set if tab is already loaded
        }
        const newSet = new Set(prev);
        newSet.add(tabId);
        return newSet;
      });
    },
    [setLoadedTabs]
  );

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

  // Sync tab order on add/remove
  React.useEffect(() => {
    const prev = tabOrderRef.current;
    const currentTabIds = tabWidgets.map(
      tab => (tab as React.ReactElement<TabWidgetProps>).props.id
    );
    const added = currentTabIds.filter(id => !prev.includes(id));
    const removed = prev.filter(id => !currentTabIds.includes(id));

    if (added.length || removed.length) {
      setTabOrder(currentTabIds);
    }
  }, [tabWidgets, tabOrderRef, setTabOrder]);

  // Sync activeTabId with selectedIndex prop from backend (only when not user-initiated)
  React.useEffect(() => {
    if (selectedIndex != null && tabOrder[selectedIndex]) {
      const targetTabId = tabOrder[selectedIndex];
      // Only sync if it's not user-initiated OR if the current activeTabId is invalid
      if (
        !isUserInitiatedChangeRef.current ||
        !activeTabId ||
        !tabOrder.includes(activeTabId)
      ) {
        if (targetTabId !== activeTabId) {
          addToLoadedTabs(targetTabId);
          setActiveTabId(targetTabId);
          // Update activeIndex for Content variant animation
          setActiveIndex(selectedIndex);
        }
      }
    }
  }, [
    selectedIndex,
    tabOrder,
    activeTabId,
    addToLoadedTabs,
    isUserInitiatedChangeRef,
    setActiveTabId,
    setActiveIndex,
  ]);

  // Reset user-initiated flag when tabWidgets changes (backend response received)
  React.useEffect(() => {
    if (isUserInitiatedChangeRef.current) {
      isUserInitiatedChangeRef.current = false;
    }
  }, [tabWidgets, isUserInitiatedChangeRef]);

  // Load active tab only when it becomes active
  React.useEffect(() => {
    if (activeTabId) {
      addToLoadedTabs(activeTabId);
    }
  }, [activeTabId, addToLoadedTabs]);

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
