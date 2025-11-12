import React from 'react';
import type { TabsLayoutWidgetProps } from './types';
import {
  filterTabWidgets,
  orderTabWidgets,
  extractTabIds,
  swapTabsInOrder,
  createReorderMapping,
} from './utils/tabUtils';
import { TabContentRenderer } from './components/TabContent';
import { TabsDropdownMenu } from './components/DropdownMenu';
import { ContentVariant, TabsVariant } from './components/Variants';
import { useAnimation } from './hooks/useAnimation';
import { useTabManagement } from './hooks/useTabManagement';
import { useTabCalculation } from './hooks/useTabCalculation';
import { useDrag } from './hooks/useDrag';

export const TabsLayoutWidget = ({
  id,
  children,
  events,
  selectedIndex,
  removeParentPadding,
  variant = 'Tabs',
  padding,
  width,
  addButtonText,
}: TabsLayoutWidgetProps) => {
  // Store stable children and only update when the actual tab IDs change
  const [stableChildren, setStableChildren] = React.useState(children);
  const [prevHash, setPrevHash] = React.useState('');

  // Calculate hash of children IDs (sorted to ignore order changes)
  const childrenHash = React.useMemo(() => {
    const ids = React.Children.map(children, child => {
      if (React.isValidElement(child)) {
        return (child.props as { id?: string }).id || '';
      }
      return '';
    });
    // Sort IDs so that reordering doesn't change the hash
    const sortedIds = ids ? [...ids].sort() : [];
    return sortedIds.join(',');
  }, [children]);

  // Only update stableChildren when hash changes
  React.useEffect(() => {
    if (childrenHash !== prevHash) {
      setStableChildren(children);
      setPrevHash(childrenHash);
    }
  }, [children, childrenHash, prevHash]);

  const tabWidgets = React.useMemo(
    () => filterTabWidgets(stableChildren),
    [stableChildren]
  );

  // Centralized state management (includes synchronization logic)
  const {
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
    activeIndex,
    setActiveIndex,
    containerRef,
    tabsListRef,
    tabRefs,
    tabWidgetsRef,
    tabOrderRef,
    eventHandlerRef,
    isDraggingRef,
    isUserInitiatedChangeRef,
    tabMeasurementsRef,
    safeEvent,
    addToLoadedTabs,
  } = useTabManagement(tabWidgets, selectedIndex, events, id);

  // Restore animated underline logic for 'Content' variant
  const { activeStyle, isInitialRender } = useAnimation(
    variant,
    activeIndex,
    tabOrder,
    visibleTabs,
    tabRefs
  );

  // Calculate which tabs fit and which don't - handles responsive overflow
  useTabCalculation(
    tabOrder,
    dropdownOpen,
    visibleTabs,
    hiddenTabs,
    events,
    variant,
    containerRef,
    tabsListRef,
    tabWidgetsRef,
    tabMeasurementsRef,
    setVisibleTabs,
    setHiddenTabs,
    tabWidgets
  );

  // Event handlers
  const handleTabSelect = (tabId: string) => {
    addToLoadedTabs(tabId);
    setActiveTabId(tabId);
    setDropdownOpen(false);

    // If selecting from dropdown, swap with last visible tab
    let effectiveTabOrder = tabOrder;
    if (hiddenTabs.includes(tabId) && visibleTabs.length > 0) {
      const lastVisibleTabId = visibleTabs[visibleTabs.length - 1];
      const newOrder = swapTabsInOrder(tabOrder, tabId, lastVisibleTabId);

      // Update tab order
      setTabOrder(newOrder);
      effectiveTabOrder = newOrder;

      // Send reorder event to backend
      const originalTabOrder = extractTabIds(tabWidgets);
      const reorderMapping = createReorderMapping(newOrder, originalTabOrder);
      isUserInitiatedChangeRef.current = true;
      safeEvent('OnReorder', [reorderMapping]);
    }

    // Update activeIndex for Content variant animation (use new order if swapped)
    const newIndex = effectiveTabOrder.indexOf(tabId);
    setActiveIndex(newIndex);
    if (events?.includes('OnSelect')) safeEvent('OnSelect', [newIndex]);
  };

  const handleMouseDown = (e: React.MouseEvent, index: number) => {
    if (e.button === 1) {
      e.preventDefault();
      safeEvent('OnClose', [index]);
    }
  };

  // Drag-and-drop functionality
  const { sensors, handleDragStart, handleDragEnd } = useDrag(
    tabOrder,
    activeTabId,
    tabWidgets,
    events,
    id,
    tabOrderRef,
    eventHandlerRef,
    isDraggingRef,
    isUserInitiatedChangeRef,
    setTabOrder,
    setActiveIndex,
    safeEvent
  );
  const showClose = events.includes('OnClose');
  const showRefresh = events.includes('OnRefresh');
  const orderedTabWidgets = React.useMemo(
    () => orderTabWidgets(tabOrder, tabWidgets),
    [tabOrder, tabWidgets]
  );

  if (tabWidgets.length === 0)
    return <div className="remove-parent-padding"></div>;

  const renderTabContent = (tabWidget: React.ReactElement) => (
    <TabContentRenderer
      tabWidget={tabWidget}
      activeTabId={activeTabId}
      showClose={showClose}
      showRefresh={showRefresh}
      tabOrder={tabOrder}
      isUserInitiatedChangeRef={isUserInitiatedChangeRef}
      safeEvent={safeEvent}
    />
  );

  // Always render dropdown button but only show when needed
  const dropdownMenu = (
    <TabsDropdownMenu
      dropdownOpen={dropdownOpen}
      setDropdownOpen={setDropdownOpen}
      hiddenTabs={hiddenTabs}
      tabOrder={tabOrder}
      orderedTabWidgets={orderedTabWidgets}
      activeTabId={activeTabId}
      showClose={showClose}
      sensors={sensors}
      handleDragEnd={handleDragEnd}
      handleTabSelect={handleTabSelect}
      isUserInitiatedChangeRef={isUserInitiatedChangeRef}
    />
  );

  // Custom tab bar for 'Content' variant
  if (variant === 'Content') {
    return (
      <ContentVariant
        removeParentPadding={removeParentPadding}
        width={width}
        padding={padding}
        containerRef={containerRef}
        tabsListRef={tabsListRef}
        tabRefs={tabRefs}
        activeIndex={activeIndex}
        isInitialRender={isInitialRender}
        activeStyle={activeStyle}
        activeTabId={activeTabId}
        visibleTabs={visibleTabs}
        orderedTabWidgets={orderedTabWidgets}
        tabOrder={tabOrder}
        loadedTabs={loadedTabs}
        addButtonText={addButtonText}
        isUserInitiatedChangeRef={isUserInitiatedChangeRef}
        addToLoadedTabs={addToLoadedTabs}
        setActiveIndex={setActiveIndex}
        setActiveTabId={setActiveTabId}
        safeEvent={safeEvent}
        dropdownMenu={dropdownMenu}
      />
    );
  }

  return (
    <TabsVariant
      removeParentPadding={removeParentPadding}
      width={width}
      padding={padding}
      variant={variant}
      activeTabId={activeTabId}
      tabOrder={tabOrder}
      events={events}
      addButtonText={addButtonText}
      containerRef={containerRef}
      tabsListRef={tabsListRef}
      visibleTabs={visibleTabs}
      orderedTabWidgets={orderedTabWidgets}
      tabWidgets={tabWidgets}
      loadedTabs={loadedTabs}
      sensors={sensors}
      handleDragStart={handleDragStart}
      handleDragEnd={handleDragEnd}
      handleTabSelect={handleTabSelect}
      handleMouseDown={handleMouseDown}
      isUserInitiatedChangeRef={isUserInitiatedChangeRef}
      setActiveIndex={setActiveIndex}
      safeEvent={safeEvent}
      renderTabContent={renderTabContent}
      dropdownMenu={dropdownMenu}
    />
  );
};
