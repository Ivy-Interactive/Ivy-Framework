import * as React from 'react';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
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
        'group w-full flex items-center px-2 py-1.5 text-large-body cursor-pointer select-none rounded-sm transition-colors hover:bg-accent',
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
  const [isOverflowing, setIsOverflowing] = React.useState(false);
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

  // Restore animated underline logic for 'Content' variant
  const [activeIndex, setActiveIndex] = React.useState(selectedIndex ?? 0);
  const [activeStyle, setActiveStyle] = React.useState({
    left: '0px',
    width: '0px',
  });
  const tabRefs = React.useRef<(HTMLDivElement | null)[]>([]);

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
  }, [activeIndex, tabOrder, tabWidgets, variant]);

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

  const checkOverflow = React.useCallback(() => {
    const tabsList = tabsListRef.current;
    if (tabsList) {
      setIsOverflowing(tabsList.scrollWidth > tabsList.clientWidth + 1); // +1 for rounding
    }
  }, []);

  const debouncedCheckOverflow = useDebounce(checkOverflow, 100);

  React.useEffect(() => {
    checkOverflow();
    window.addEventListener('resize', debouncedCheckOverflow);
    return () => {
      window.removeEventListener('resize', debouncedCheckOverflow);
    };
  }, [checkOverflow, debouncedCheckOverflow]);

  React.useEffect(() => {
    checkOverflow();
  }, [tabOrder.length, checkOverflow]);

  // Keep ref in sync with state
  React.useEffect(() => {
    activeTabIdRef.current = activeTabId;
  }, [activeTabId]);

  // Sync tab order on add/remove
  React.useEffect(() => {
    const prev = tabOrder;
    const currentTabIds = tabWidgets.map(
      tab => (tab as React.ReactElement<TabWidgetProps>).props.id
    );
    const added = currentTabIds.filter(id => !prev.includes(id));
    const removed = prev.filter(id => !currentTabIds.includes(id));

    if (added.length || removed.length) {
      setTabOrder(currentTabIds);
    }
  }, [tabWidgets]);

  // Load active tab
  React.useEffect(() => {
    if (activeTabId) setLoadedTabs(prev => new Set(prev).add(activeTabId));
  }, [activeTabId]);

  // Sync with selectedIndex prop
  React.useEffect(() => {
    // Only update active tab if selectedIndex is explicitly provided and valid
    if (
      selectedIndex != null &&
      selectedIndex >= 0 &&
      selectedIndex < tabOrder.length
    ) {
      const newTargetTabId = tabOrder[selectedIndex];
      // Only update state if the target tab ID is actually different from the current active one.
      if (newTargetTabId !== activeTabIdRef.current) {
        // Small delay to handle race condition between tabOrder and selectedIndex updates
        const timeoutId = setTimeout(() => {
          setActiveTabId(newTargetTabId);
        }, 10);

        return () => clearTimeout(timeoutId);
      }
    }
    // If selectedIndex is null or out of bounds, but we have a valid activeTabId that still exists,
    // keep the current active tab instead of clearing it
    else if (
      selectedIndex === null &&
      activeTabIdRef.current &&
      tabOrder.includes(activeTabIdRef.current)
    ) {
      // Keep current active tab
    }
    // If selectedIndex is null and we don't have a valid active tab, let it be null
    else if (
      selectedIndex === null &&
      (!activeTabIdRef.current || !tabOrder.includes(activeTabIdRef.current))
    ) {
      setActiveTabId(null);
    }
  }, [selectedIndex, tabOrder]);

  // Event handlers
  const handleTabSelect = (tabId: string) => {
    setLoadedTabs(prev => new Set(prev).add(tabId));
    setActiveTabId(tabId);
    setDropdownOpen(false);
    eventHandler('OnSelect', id, [tabOrder.indexOf(tabId)]);
  };

  const handleMouseDown = (e: React.MouseEvent, index: number) => {
    if (e.button === 1) {
      e.preventDefault();
      eventHandler('OnClose', id, [index]);
    }
  };

  const handleDragEnd = React.useCallback(
    (event: {
      active: { id: string | number };
      over: { id: string | number } | null;
    }) => {
      const { active, over } = event;
      if (active && over && active.id !== over.id) {
        setTabOrder(items =>
          arrayMove(
            items,
            items.indexOf(String(active.id)),
            items.indexOf(String(over.id))
          )
        );
      }
    },
    []
  );

  React.useEffect(() => {
    const handleTabEvent = (eventType: string) => (e: Event) => {
      const customEvent = e as CustomEvent<{ id: string }>;
      if (!customEvent.detail?.id) return;
      const idx = tabOrder.indexOf(customEvent.detail.id);
      if (idx !== -1) eventHandler(eventType, id, [idx]);
    };

    const closeHandler = handleTabEvent('OnClose');
    const refreshHandler = handleTabEvent('OnRefresh');

    window.addEventListener('tab-close', closeHandler);
    window.addEventListener('tab-refresh', refreshHandler);

    return () => {
      window.removeEventListener('tab-close', closeHandler);
      window.removeEventListener('tab-refresh', refreshHandler);
    };
  }, [tabOrder, eventHandler, id]);

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 8 } }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );
  const showClose = events.includes('OnClose');
  const showRefresh = events.includes('OnRefresh');
  const orderedTabWidgets = tabOrder
    .map(id =>
      tabWidgets.find(
        tab => (tab as React.ReactElement<TabWidgetProps>).props.id === id
      )
    )
    .filter(Boolean);

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
          <Badge variant="default" className="ml-2 w-min whitespace-nowrap">
            {badge}
          </Badge>
        )}
        <div className="ml-2 items-center flex gap-0 relative">
          {activeTabId === tabId && showRefresh && (
            <a
              onClick={e => {
                e.stopPropagation();
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

  return (
    <Tabs
      value={activeTabId ?? undefined}
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
            onDragEnd={handleDragEnd}
            sensors={sensors}
          >
            <SortableContext items={tabOrder}>
              <TabsList
                ref={tabsListRef}
                className="relative h-auto w-full gap-0.5 mt-3 bg-transparent p-0 flex justify-start flex-nowrap overflow-x-auto scrollbar-hide"
                style={{
                  msOverflowStyle: 'none',
                  scrollbarWidth: 'none',
                }}
              >
                {orderedTabWidgets.map(tabWidget => {
                  if (!React.isValidElement(tabWidget)) return null;
                  const props = tabWidget.props as Partial<TabWidgetProps>;
                  if (!props.id) return null;
                  const { id } = props;

                  return (
                    <SortableTabTrigger
                      key={id}
                      id={id}
                      value={id}
                      onClick={() => handleTabSelect(id)}
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

          {/* Only render dropdown if overflowing */}
          {isOverflowing && (
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
                    <div className="flex flex-col w-48">
                      {orderedTabWidgets.map(tabWidget => {
                        if (!React.isValidElement(tabWidget)) return null;
                        const props =
                          tabWidget.props as Partial<TabWidgetProps> & {
                            key?: string;
                          };
                        if (!props.id) return null;
                        const { title, id, key } = props;

                        return (
                          <SortableDropdownMenuItem
                            key={key ?? id}
                            id={id}
                            onClick={() => handleTabSelect(id)}
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

          return (
            <div
              key={id}
              className={cn(
                'h-full overflow-auto',
                activeTabId === id ? 'block' : 'hidden',
                (variant as string) === 'Content' && 'border-none'
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
