import * as React from 'react';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs';
import Icon from '@/components/Icon';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { cn } from '@/lib/utils';
import { getPadding } from '@/lib/styles';
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
} from '@dnd-kit/core';
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { ChevronDown, X } from 'lucide-react';
import { useEventHandler } from '@/components/EventHandlerContext';
import { Badge } from '@/components/ui/badge';
import { RotateCw } from 'lucide-react';
import { useDebounce } from '@/hooks/use-debounce';

interface TabWidgetProps {
  children: React.ReactNode[];
  title: string;
  id: string;
  badge?: string;
  icon?: string;
}

export const TabWidget: React.FC<TabWidgetProps> = ({ children }) => {
  return <div className="h-full">{children}</div>;
};

TabWidget.displayName = 'TabWidget';

interface TabsLayoutWidgetProps {
  id: string;
  variant?: 'Tabs' | 'Content';
  removeParentPadding?: boolean;
  selectedIndex: number;
  children: React.ReactElement<TabWidgetProps>[];
  events: string[];
  padding?: string;
}

function SortableTabTrigger({
  id,
  value,
  onClick,
  onMouseDown,
  className,
  children,
  useRadix = false,
  ...props
}: {
  id: string;
  value: string;
  onClick: () => void;
  onMouseDown: (e: React.MouseEvent) => void;
  className?: string;
  onMouseEnter?: () => void;
  onMouseLeave?: () => void;
  children: React.ReactNode;
  useRadix?: boolean;
}) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id });

  return (
    <TabsTrigger
      ref={setNodeRef}
      style={{
        transform: transform
          ? `translate3d(${transform.x}px, 0, 0)`
          : undefined,
        transition,
        opacity: isDragging ? 0.5 : 1,
        zIndex: isDragging ? 100 : undefined,
      }}
      value={value}
      onClick={onClick}
      onMouseDown={onMouseDown}
      className={className}
      useRadix={useRadix}
      {...attributes}
      {...listeners}
      {...props}
    >
      {children}
    </TabsTrigger>
  );
}

function SortableDropdownMenuItem({
  id,
  children,
  onClick,
  isActive,
}: {
  id: string;
  children: React.ReactNode;
  onClick: () => void;
  isActive?: boolean;
}) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id });

  return (
    <div
      ref={setNodeRef}
      style={{
        transform: CSS.Transform.toString(transform),
        transition,
        opacity: isDragging ? 0.5 : 1,
        zIndex: isDragging ? 100 : undefined,
      }}
      {...attributes}
      {...listeners}
      onClick={onClick}
      className={cn(
        'group w-full flex items-center p-1 text-sm cursor-pointer select-none rounded-sm transition-colors hover:bg-accent',
        isActive && 'bg-accent text-accent-foreground'
      )}
    >
      <span className="truncate text-left">{children}</span>
      <button
        type="button"
        className="ml-auto opacity-60 p-1 hover:opacity-100 invisible group-hover:visible cursor-pointer"
        onClick={e => {
          e.stopPropagation();
          window.dispatchEvent(
            new CustomEvent('tab-close', { detail: { id } })
          );
        }}
      >
        <X className="w-3 h-3" />
      </button>
    </div>
  );
}

export const TabsLayoutWidget = ({
  id,
  children,
  events,
  selectedIndex,
  removeParentPadding,
  variant = 'Tabs',
  padding,
}: TabsLayoutWidgetProps) => {
  const tabWidgets = React.Children.toArray(children).filter(
    child =>
      React.isValidElement(child) &&
      (child.type as React.ComponentType<TabWidgetProps>)?.displayName ===
        'TabWidget'
  );

  // Shared state and logic
  const [dropdownOpen, setDropdownOpen] = React.useState(false);
  const [visibleTabs, setVisibleTabs] = React.useState<string[]>([]);
  const [hiddenTabs, setHiddenTabs] = React.useState<string[]>([]);
  const [tabOrder, setTabOrder] = React.useState<string[]>(() =>
    tabWidgets.map(tab => (tab as React.ReactElement<TabWidgetProps>).props.id)
  );
  const [activeTabId, setActiveTabId] = React.useState<string | null>(
    () => tabOrder[selectedIndex] ?? tabOrder[0] ?? null
  );
  const [loadedTabs, setLoadedTabs] = React.useState<Set<string>>(
    () => new Set()
  );
  const activeTabIdRef = React.useRef<string | null>(activeTabId);
  const eventHandler = useEventHandler();
  const containerRef = React.useRef<HTMLDivElement>(null);
  const tabsListRef = React.useRef<HTMLDivElement>(null);
  const tabRefs = React.useRef<(HTMLDivElement | null)[]>([]);
  const tabWidgetsRef = React.useRef(tabWidgets);
  const tabOrderRef = React.useRef(tabOrder);
  const eventHandlerRef = React.useRef(eventHandler);
  const isDraggingRef = React.useRef(false);
  const isUserInitiatedChangeRef = React.useRef(false);

  // Restore animated underline logic for 'Content' variant
  const [activeIndex, setActiveIndex] = React.useState(selectedIndex ?? 0);
  const [activeStyle, setActiveStyle] = React.useState({
    left: '0px',
    width: '0px',
  });

  // Update refs when they change
  React.useEffect(() => {
    tabWidgetsRef.current = tabWidgets;
  }, [tabWidgets]);

  React.useEffect(() => {
    tabOrderRef.current = tabOrder;
  }, [tabOrder]);

  React.useEffect(() => {
    eventHandlerRef.current = eventHandler;
  }, [eventHandler]);

  React.useEffect(() => {
    if (variant !== 'Content') return;
    const activeElement = tabRefs.current[activeIndex];
    if (activeElement) {
      const { offsetLeft, offsetWidth } = activeElement;
      setActiveStyle({
        left: `${offsetLeft}px`,
        width: `${offsetWidth}px`,
      });
    }
  }, [activeIndex, tabOrder, variant]);

  React.useEffect(() => {
    if (variant !== 'Content') return;
    requestAnimationFrame(() => {
      const firstElement = tabRefs.current[0];
      if (firstElement) {
        const { offsetLeft, offsetWidth } = firstElement;
        setActiveStyle({
          left: `${offsetLeft}px`,
          width: `${offsetWidth}px`,
        });
      }
    });
  }, [variant]);

  // Calculate which tabs fit and which don't - preserving order
  const calculateVisibleTabs = React.useCallback(() => {
    const container = containerRef.current;
    if (!container) return;

    // Don't recalculate when dropdown is open to prevent infinite loops
    if (dropdownOpen) return;

    const containerWidth = container.clientWidth - 200; // Account for dropdown button space
    const newVisibleTabs: string[] = [];
    const newHiddenTabs: string[] = [];
    let currentWidth = 0;

    // Create temporary elements to measure tab widths
    const tempContainer = document.createElement('div');
    tempContainer.style.position = 'absolute';
    tempContainer.style.visibility = 'hidden';
    tempContainer.style.whiteSpace = 'nowrap';
    tempContainer.style.fontSize = '14px'; // Match the tab font size
    tempContainer.style.fontWeight = '500'; // Match the tab font weight
    tempContainer.style.padding = '8px 12px'; // Match the tab padding
    tempContainer.style.border = '1px solid transparent'; // Match the tab border
    tempContainer.style.marginRight = '2px'; // Match the tab gap
    document.body.appendChild(tempContainer);

    try {
      // Process tabs in their original order - first come, first served
      for (const tabId of tabOrder) {
        const tabWidget = tabWidgetsRef.current.find(
          tab => (tab as React.ReactElement<TabWidgetProps>).props.id === tabId
        );
        if (!tabWidget || !React.isValidElement(tabWidget)) continue;

        const { title, icon, badge } = tabWidget.props as TabWidgetProps;

        // Create a temporary element to measure the tab content
        const tempTab = document.createElement('div');
        tempTab.style.display = 'inline-flex';
        tempTab.style.alignItems = 'center';
        tempTab.style.gap = '6px';

        // Add icon if present
        if (icon) {
          const iconSpan = document.createElement('span');
          iconSpan.textContent = '🔧'; // Placeholder for icon
          iconSpan.style.fontSize = '16px';
          iconSpan.style.opacity = '0.6';
          tempTab.appendChild(iconSpan);
        }

        // Add title
        const titleSpan = document.createElement('span');
        titleSpan.textContent = title;
        tempTab.appendChild(titleSpan);

        // Add badge if present
        if (badge) {
          const badgeSpan = document.createElement('span');
          badgeSpan.textContent = badge;
          badgeSpan.style.marginLeft = '8px';
          badgeSpan.style.padding = '2px 6px';
          badgeSpan.style.fontSize = '12px';
          badgeSpan.style.borderRadius = '4px';
          badgeSpan.style.backgroundColor = 'var(--accent)';
          tempTab.appendChild(badgeSpan);
        }

        // Add close/refresh buttons space
        const buttonsSpan = document.createElement('span');
        buttonsSpan.style.marginLeft = '8px';
        buttonsSpan.style.width = '24px'; // Approximate space for buttons
        tempTab.appendChild(buttonsSpan);

        tempContainer.appendChild(tempTab);

        const tabWidth = tempTab.offsetWidth;
        tempContainer.removeChild(tempTab);

        // Check if this tab fits in the remaining space
        if (currentWidth + tabWidth <= containerWidth) {
          newVisibleTabs.push(tabId);
          currentWidth += tabWidth;
        } else {
          // This tab doesn't fit, so it and all remaining tabs go to dropdown
          newHiddenTabs.push(tabId);
          // Add all remaining tabs to hidden list
          const remainingTabIds = tabOrder.slice(tabOrder.indexOf(tabId) + 1);
          newHiddenTabs.push(...remainingTabIds);
          break; // Stop processing - order is preserved
        }
      }
    } finally {
      document.body.removeChild(tempContainer);
    }

    setVisibleTabs(newVisibleTabs);
    setHiddenTabs(newHiddenTabs);
  }, [tabOrder, dropdownOpen]);

  const debouncedCalculateVisibleTabs = useDebounce(calculateVisibleTabs, 100);
  const debouncedCalculateVisibleTabsRef = React.useRef(
    debouncedCalculateVisibleTabs
  );
  const calculateVisibleTabsRef = React.useRef(calculateVisibleTabs);

  React.useEffect(() => {
    debouncedCalculateVisibleTabsRef.current = debouncedCalculateVisibleTabs;
  }, [debouncedCalculateVisibleTabs]);

  React.useEffect(() => {
    calculateVisibleTabsRef.current = calculateVisibleTabs;
  }, [calculateVisibleTabs]);

  React.useEffect(() => {
    calculateVisibleTabsRef.current();
    const handleResize = () => debouncedCalculateVisibleTabsRef.current();
    window.addEventListener('resize', handleResize);
    return () => {
      window.removeEventListener('resize', handleResize);
    };
  }, []);

  React.useEffect(() => {
    calculateVisibleTabsRef.current();
  }, [tabOrder]);

  // Keep ref in sync with state
  React.useEffect(() => {
    activeTabIdRef.current = activeTabId;
  }, [activeTabId]);

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
  }, [tabWidgets]);

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
          setActiveTabId(targetTabId);
          setLoadedTabs(prev => new Set(prev).add(targetTabId));
          // Update activeIndex for Content variant animation
          setActiveIndex(selectedIndex);
        }
      }
    }
    // Reset the flag after processing
    isUserInitiatedChangeRef.current = false;
    // eslint-disable-next-line react-hooks/exhaustive-deps -- activeTabId intentionally excluded to prevent race conditions
  }, [selectedIndex, tabOrder]);

  // Load active tab
  React.useEffect(() => {
    if (activeTabId) setLoadedTabs(prev => new Set(prev).add(activeTabId));
  }, [activeTabId]);

  // Event handlers
  const handleTabSelect = (tabId: string) => {
    setLoadedTabs(prev => new Set(prev).add(tabId));
    setActiveTabId(tabId);
    setDropdownOpen(false);
    // Update activeIndex for Content variant animation
    const newIndex = tabOrder.indexOf(tabId);
    setActiveIndex(newIndex);
    eventHandler('OnSelect', id, [newIndex]);
  };

  const handleMouseDown = (e: React.MouseEvent, index: number) => {
    if (e.button === 1) {
      e.preventDefault();
      eventHandler('OnClose', id, [index]);
    }
  };

  const handleDragStart = React.useCallback(() => {
    isDraggingRef.current = true;
  }, []);

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
          eventHandler('OnReorder', id, [reorderMapping]);
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
    [tabOrder, activeTabId, eventHandler, id, tabWidgets]
  );

  React.useEffect(() => {
    const handleTabEvent = (eventType: string) => (e: Event) => {
      const customEvent = e as CustomEvent<{ id: string }>;
      if (!customEvent.detail?.id) return;
      const idx = tabOrderRef.current.indexOf(customEvent.detail.id);
      if (idx !== -1) eventHandlerRef.current(eventType, id, [idx]);
    };

    const closeHandler = handleTabEvent('OnClose');
    const refreshHandler = handleTabEvent('OnRefresh');

    window.addEventListener('tab-close', closeHandler);
    window.addEventListener('tab-refresh', refreshHandler);

    return () => {
      window.removeEventListener('tab-close', closeHandler);
      window.removeEventListener('tab-refresh', refreshHandler);
    };
  }, [id]);

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 8 } }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );
  const showClose = events.includes('OnClose');
  const showRefresh = events.includes('OnRefresh');
  const orderedTabWidgets = React.useMemo(
    () =>
      tabOrder
        .map(id =>
          tabWidgets.find(
            tab => (tab as React.ReactElement<TabWidgetProps>).props.id === id
          )
        )
        .filter(Boolean),
    [tabOrder, tabWidgets]
  );

  if (tabWidgets.length === 0)
    return <div className="remove-parent-padding"></div>;

  const renderTabContent = (tabWidget: React.ReactElement) => {
    if (!React.isValidElement(tabWidget)) return null;
    const { title, id: tabId, icon, badge } = tabWidget.props as TabWidgetProps;

    return (
      <>
        {icon && (
          <Icon name={icon} className="-ms-0.5 me-1.5 opacity-60" size={16} />
        )}
        <span>{title}</span>
        {badge && (
          <Badge variant="primary" className="ml-2 w-min whitespace-nowrap">
            {badge}
          </Badge>
        )}
        <div className="ml-2 items-center flex gap-0 relative">
          {activeTabId === tabId && showRefresh && (
            <a
              onClick={e => {
                e.stopPropagation();
                // Mark as user-initiated to prevent sync issues
                isUserInitiatedChangeRef.current = true;
                eventHandler('OnRefresh', id, [tabOrder.indexOf(tabId)]);
              }}
              className="opacity-60 p-1 rounded-full hover:bg-accent hover:opacity-100 transition-colors cursor-pointer"
            >
              <RotateCw className="w-3 h-3" />
            </a>
          )}
          {showClose && (
            <a
              onClick={e => {
                e.stopPropagation();
                // Mark as user-initiated since close affects selection
                isUserInitiatedChangeRef.current = true;
                eventHandler('OnClose', id, [tabOrder.indexOf(tabId)]);
              }}
              className="opacity-60 p-1 rounded-full hover:bg-accent hover:opacity-100 transition-colors cursor-pointer"
            >
              <X className="w-3 h-3" />
            </a>
          )}
        </div>
      </>
    );
  };

  // Custom tab bar for 'Content' variant
  if (variant === 'Content') {
    // Modern, animated, borderless tab bar implementation
    return (
      <div
        className={cn(
          'flex flex-col h-full',
          removeParentPadding && 'remove-parent-padding'
        )}
      >
        <div className="relative">
          {/* Hover Highlight */}
          <div
            className="absolute h-[26px] transition-all duration-300 ease-out bg-accent/20 rounded-[6px] flex items-center"
            style={{
              opacity: activeIndex !== null ? 1 : 0,
              pointerEvents: 'none',
            }}
          />
          {/* Active Indicator */}
          <div
            className="absolute bottom-[-6px] h-[2px] bg-foreground transition-all duration-300 ease-out"
            style={activeStyle}
          />
          {/* Tabs */}
          <div className="relative flex space-x-[6px] items-center">
            {orderedTabWidgets.map((tabWidget, index) => {
              if (!React.isValidElement(tabWidget)) return null;
              const props = tabWidget.props as Partial<TabWidgetProps>;
              if (!props.id) return null;
              const { title, id } = props;
              return (
                <div
                  key={id}
                  ref={el => {
                    tabRefs.current[index] = el;
                  }}
                  className={cn(
                    'px-3 py-1.5 cursor-pointer transition-colors duration-300 h-[26px]',
                    index === activeIndex
                      ? 'text-foreground'
                      : 'text-muted-foreground'
                  )}
                  onClick={() => {
                    // Mark as user-initiated for Content variant
                    isUserInitiatedChangeRef.current = true;
                    setActiveIndex(index);
                    setActiveTabId(tabOrder[index]);
                    eventHandler('OnSelect', id, [index]);
                  }}
                >
                  <div className="text-sm font-medium leading-4 whitespace-nowrap flex items-center justify-center h-full">
                    {title}
                  </div>
                </div>
              );
            })}
          </div>
        </div>
        <div className="flex-1 overflow-hidden">
          {orderedTabWidgets.map(tabWidget => {
            if (!React.isValidElement(tabWidget)) return null;
            const props = tabWidget.props as Partial<TabWidgetProps>;
            if (!props.id) return null;
            const { id } = props;
            if (!loadedTabs.has(id)) return null;
            const paddingStyle = getPadding(padding);
            return (
              <div
                key={id}
                className={cn(
                  'h-full overflow-auto',
                  activeTabId === id ? 'block' : 'hidden',
                  'border-none'
                )}
                style={paddingStyle}
              >
                {tabWidget}
              </div>
            );
          })}
        </div>
      </div>
    );
  }

  const useRadix = (variant as string) === 'Content'; // Use Radix only for Content variant, custom for Tabs variant

  return (
    <Tabs
      value={activeTabId ?? undefined}
      onValueChange={value => {
        if (!useRadix) {
          handleTabSelect(value);
        } else {
          // For Radix tabs, also mark as user-initiated to prevent flicker
          isUserInitiatedChangeRef.current = true;
          const newIndex = tabOrder.indexOf(value);
          setActiveIndex(newIndex);
          eventHandler('OnSelect', id, [newIndex]);
        }
      }}
      useRadix={useRadix}
      className={cn(
        removeParentPadding && 'remove-parent-padding',
        'flex flex-col h-full'
      )}
    >
      <div className="flex-shrink-0">
        <div
          className="relative pl-12 pr-12 before:absolute before:inset-x-0 before:bottom-0 before:h-px before:bg-border before:z-0 overflow-hidden"
          ref={containerRef}
        >
          <DndContext
            collisionDetection={closestCenter}
            onDragStart={handleDragStart}
            onDragEnd={handleDragEnd}
            sensors={sensors}
          >
            <SortableContext items={tabOrder}>
              <TabsList
                ref={tabsListRef}
                useRadix={useRadix}
                className="relative h-auto w-full gap-0.5 mt-3 bg-transparent p-0 flex justify-start flex-nowrap"
              >
                {orderedTabWidgets.map(tabWidget => {
                  if (!React.isValidElement(tabWidget)) return null;
                  const props = tabWidget.props as Partial<TabWidgetProps>;
                  if (!props.id) return null;
                  const { id } = props;

                  // Only render tabs that are visible
                  if (!visibleTabs.includes(id)) return null;

                  return (
                    <SortableTabTrigger
                      key={id}
                      id={id}
                      value={id}
                      useRadix={useRadix}
                      onClick={() => {
                        // Mark as user-initiated to prevent flicker
                        isUserInitiatedChangeRef.current = true;
                        handleTabSelect(id);
                      }}
                      onMouseDown={(e: React.MouseEvent) =>
                        handleMouseDown(e, tabOrder.indexOf(id))
                      }
                      className={cn(
                        'group overflow-hidden rounded-b-none py-2 data-[state=active]:z-10 data-[state=active]:shadow-none border-x border-t border-border flex-shrink-0',
                        variant === 'Tabs' &&
                          'data-[state=active]:bg-background',
                        (variant as string) === 'Content' &&
                          'border-b-2 border-b-transparent data-[state=active]:border-b-primary data-[state=active]:bg-background/50'
                      )}
                    >
                      {renderTabContent(tabWidget)}
                    </SortableTabTrigger>
                  );
                })}
              </TabsList>
            </SortableContext>
          </DndContext>

          {/* Only render dropdown if there are hidden tabs */}
          {hiddenTabs.length > 0 && (
            <DropdownMenu open={dropdownOpen} onOpenChange={setDropdownOpen}>
              <DropdownMenuTrigger asChild>
                <Button
                  variant="ghost"
                  size="icon"
                  className="absolute right-0 top-1/2 -translate-y-1/2 h-7 w-7 bg-transparent z-10 mr-3"
                  aria-label="Show more tabs"
                >
                  <ChevronDown className="w-5 h-5" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DndContext
                  collisionDetection={closestCenter}
                  onDragEnd={handleDragEnd}
                  sensors={sensors}
                >
                  <SortableContext items={tabOrder}>
                    <div className="flex flex-col gap-1 w-48">
                      {orderedTabWidgets.map(tabWidget => {
                        if (!React.isValidElement(tabWidget)) return null;
                        const props =
                          tabWidget.props as Partial<TabWidgetProps>;
                        if (!props.id) return null;
                        const { title, id } = props;

                        // Only render tabs that are hidden
                        if (!hiddenTabs.includes(id)) return null;

                        return (
                          <SortableDropdownMenuItem
                            key={id}
                            id={id}
                            onClick={() => {
                              // Mark as user-initiated to prevent flicker
                              isUserInitiatedChangeRef.current = true;
                              handleTabSelect(id);
                            }}
                            isActive={activeTabId === id}
                          >
                            {title}
                          </SortableDropdownMenuItem>
                        );
                      })}
                    </div>
                  </SortableContext>
                </DndContext>
              </DropdownMenuContent>
            </DropdownMenu>
          )}
        </div>
      </div>

      <div className="flex-1 overflow-hidden">
        {orderedTabWidgets.map(tabWidget => {
          if (!React.isValidElement(tabWidget)) return null;
          const props = tabWidget.props as Partial<TabWidgetProps>;
          if (!props.id) return null;
          const { id } = props;

          if (!loadedTabs.has(id)) return null;

          const paddingStyle = getPadding(padding);

          if (useRadix) {
            // Use TabsContent for Radix version (Content variant)
            return (
              <TabsContent
                key={id}
                value={id}
                useRadix={useRadix}
                className={cn('h-full overflow-auto border-none mt-0')}
                style={paddingStyle}
              >
                {tabWidget}
              </TabsContent>
            );
          }

          // Use custom div-based content for non-Radix version (Tabs variant)
          return (
            <div
              key={id}
              className={cn(
                'h-full overflow-auto',
                activeTabId === id ? 'block' : 'hidden'
              )}
              style={paddingStyle}
            >
              {tabWidget}
            </div>
          );
        })}
      </div>
    </Tabs>
  );
};
