import React from 'react';
import {
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
} from '@dnd-kit/core';
import { arrayMove, sortableKeyboardCoordinates } from '@dnd-kit/sortable';
import type { TabWidgetProps } from '../types';

/**
 * Custom hook to manage drag-and-drop functionality for tabs.
 * Handles drag events, tab reordering, and backend synchronization.
 *
 * @param tabOrder - Current order of tab IDs
 * @param activeTabId - Currently active tab ID
 * @param tabWidgets - Array of tab widget elements
 * @param events - Array of enabled event types
 * @param id - Component ID for event handling
 * @param tabOrderRef - Ref to track tab order
 * @param eventHandlerRef - Ref to event handler function
 * @param isDraggingRef - Ref to track dragging state
 * @param isUserInitiatedChangeRef - Ref to track user-initiated changes
 * @param setTabOrder - Setter for tab order state
 * @param setActiveIndex - Setter for active index state
 * @param safeEvent - Function to safely trigger events
 */
export function useDrag(
  tabOrder: string[],
  activeTabId: string | null,
  tabWidgets: React.ReactElement[],
  events: string[],
  id: string,
  tabOrderRef: React.MutableRefObject<string[]>,
  eventHandlerRef: React.MutableRefObject<
    (name: string, id: string, args: unknown[]) => void
  >,
  isDraggingRef: React.MutableRefObject<boolean>,
  isUserInitiatedChangeRef: React.MutableRefObject<boolean>,
  setTabOrder: React.Dispatch<React.SetStateAction<string[]>>,
  setActiveIndex: React.Dispatch<React.SetStateAction<number>>,
  safeEvent: (
    name:
      | 'OnSelect'
      | 'OnClose'
      | 'OnRefresh'
      | 'OnReorder'
      | 'OnAddButtonClick',
    args: unknown[]
  ) => void
) {
  // Configure drag-and-drop sensors
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 3 } }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  // Handle drag start
  const handleDragStart = React.useCallback(() => {
    isDraggingRef.current = true;
  }, [isDraggingRef]);

  // Handle drag end and tab reordering
  const handleDragEnd = React.useCallback(
    (event: {
      active: { id: string | number };
      over: { id: string | number } | null;
    }) => {
      const { active, over } = event;
      if (active && over && active.id !== over.id) {
        const oldIndex = tabOrder.indexOf(String(active.id));
        const newIndex = tabOrder.indexOf(String(over.id));

        // Track active tab's position before the move
        const oldActiveIndex = activeTabId ? tabOrder.indexOf(activeTabId) : -1;

        const newOrder = arrayMove(tabOrder, oldIndex, newIndex);
        setTabOrder(newOrder);

        // Send reorder event to backend with mapping from new order to original backend indices
        const originalTabOrder = tabWidgets.map(
          tab => (tab as React.ReactElement<TabWidgetProps>).props.id
        );
        const reorderMapping = newOrder.map(tabId => {
          const index = originalTabOrder.indexOf(tabId);
          if (index === -1) {
            console.error(
              'Tab reorder error: Tab ID not found in original order',
              {
                tabId,
                newOrder,
                originalTabOrder,
              }
            );
          }
          return index;
        });

        // Safety check: Only send reorder if all indices are valid
        if (
          !reorderMapping.includes(-1) &&
          reorderMapping.length === originalTabOrder.length
        ) {
          // Mark as user-initiated to prevent backend sync from interfering
          isUserInitiatedChangeRef.current = true;
          safeEvent('OnReorder', [reorderMapping]);
        } else {
          console.error('Tab reorder aborted: Invalid mapping', {
            reorderMapping,
            newOrder,
            originalTabOrder,
          });
        }

        // Check if the active tab's position changed after ANY drag operation
        if (activeTabId && oldActiveIndex !== -1) {
          const newActiveIndex = newOrder.indexOf(activeTabId);
          if (newActiveIndex !== oldActiveIndex) {
            // Active tab's index changed due to other tabs moving around it
            setActiveIndex(newActiveIndex);
            // The OnReorder event will handle updating the backend about the new active position
          }
        }
      }
      isDraggingRef.current = false;
    },
    [
      tabOrder,
      activeTabId,
      safeEvent,
      tabWidgets,
      isDraggingRef,
      isUserInitiatedChangeRef,
      setTabOrder,
      setActiveIndex,
    ]
  );

  // Setup event listeners for tab close and refresh events
  React.useEffect(() => {
    const handleTabEvent = (eventType: string) => (e: Event) => {
      const customEvent = e as CustomEvent<{ id: string }>;
      if (!customEvent.detail?.id) return;
      const idx = tabOrderRef.current.indexOf(customEvent.detail.id);
      if (idx !== -1 && events?.includes(eventType))
        eventHandlerRef.current(eventType, id, [idx]);
    };

    const closeHandler = handleTabEvent('OnClose');
    const refreshHandler = handleTabEvent('OnRefresh');

    window.addEventListener('tab-close', closeHandler);
    window.addEventListener('tab-refresh', refreshHandler);

    return () => {
      window.removeEventListener('tab-close', closeHandler);
      window.removeEventListener('tab-refresh', refreshHandler);
    };
  }, [id, events, tabOrderRef, eventHandlerRef]);

  return {
    sensors,
    handleDragStart,
    handleDragEnd,
  };
}
