import { useEventHandler } from "@/components/EventHandlerContext";
import Icon from "@/components/Icon";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
} from "@/components/ui/dropdown-menu";
import { ScrollArea, ScrollBar } from "@/components/ui/scroll-area";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { getHeight, getPadding, getWidth } from "@/lib/styles";
import { cn } from "@/lib/utils";
import {
  DndContext,
  closestCenter,
  DragOverlay,
  PointerSensor,
  useSensor,
  useSensors,
} from "@dnd-kit/core";
import {
  SortableContext,
  useSortable,
  arrayMove,
  verticalListSortingStrategy,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { RotateCw, X, ChevronDown } from "lucide-react";
import * as React from "react";

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

TabWidget.displayName = "TabWidget";

interface TabsLayoutWidgetProps {
  id: string;
  variant?: "Tabs" | "Content";
  removeParentPadding?: boolean;
  selectedIndex: number;
  children: React.ReactElement<TabWidgetProps>[];
  events: string[];
  width?: string;
  height?: string;
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
        "group w-full flex items-center px-2 py-1.5 text-sm cursor-pointer select-none rounded-sm transition-colors hover:bg-accent",
        isActive && "bg-accent text-accent-foreground"
      )}
    >
      <span className="truncate text-left">{children}</span>
      <button
        type="button"
        className="ml-auto opacity-60 p-1 hover:opacity-100 invisible group-hover:visible"
        onClick={(e) => {
          e.stopPropagation();
          window.dispatchEvent(
            new CustomEvent("tab-close", { detail: { id } })
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
  width,
  height,
  padding,
  variant,
}: TabsLayoutWidgetProps) => {
  const tabWidgets = React.Children.toArray(children).filter(
    (child) =>
      React.isValidElement(child) &&
      (child.type as React.ComponentType<TabWidgetProps>)?.displayName ===
        "TabWidget"
  );

  const eventHandler = useEventHandler();
  const containerRef = React.useRef<HTMLDivElement>(null);
  const tabsListRef = React.useRef<HTMLDivElement>(null);
  const [tabsOverflowing, setTabsOverflowing] = React.useState(false);
  const [hoveredTabId, setHoveredTabId] = React.useState<string | null>(null);
  const [dropdownOpen, setDropdownOpen] = React.useState(false);

  // Tab management
  const tabIds = React.useMemo(
    () =>
      tabWidgets.map(
        (tab) => (tab as React.ReactElement<TabWidgetProps>).props.id
      ),
    [tabWidgets]
  );
  const prevTabIdsRef = React.useRef<string[]>(tabIds);
  const [tabOrder, setTabOrder] = React.useState<string[]>(() => tabIds);
  const [activeTabId, setActiveTabId] = React.useState<string | null>(
    () => tabOrder[selectedIndex] ?? tabOrder[0] ?? null
  );
  const [loadedTabs, setLoadedTabs] = React.useState<Set<string>>(
    () => new Set()
  );
  const activeTabIdRef = React.useRef<string | null>(activeTabId);

  // Keep ref in sync with state
  React.useEffect(() => {
    activeTabIdRef.current = activeTabId;
  }, [activeTabId]);

  // Sync tab order on add/remove
  React.useEffect(() => {
    const prev = prevTabIdsRef.current;
    const added = tabIds.filter((id) => !prev.includes(id));
    const removed = prev.filter((id) => !tabIds.includes(id));

    if (added.length || removed.length) {
      setTabOrder(tabIds);
      prevTabIdsRef.current = tabIds;
    }
  }, [tabIds]);

  // Handle overflow detection
  React.useEffect(() => {
    if (hoveredTabId === null && containerRef.current && tabsListRef.current) {
      const containerWidth = containerRef.current.getBoundingClientRect().width;
      const tabsListWidth = tabsListRef.current.getBoundingClientRect().width;
      setTabsOverflowing(tabsListWidth + 96 > containerWidth);
    }
  }, [hoveredTabId, tabOrder]);

  // Recalculate overflow on window resize and sidemenu toggle
  React.useEffect(() => {
    const recalculateOverflow = () => {
      if (containerRef.current && tabsListRef.current) {
        // Use setTimeout to ensure DOM has updated after layout changes
        setTimeout(() => {
          if (containerRef.current && tabsListRef.current) {
            const containerWidth =
              containerRef.current.getBoundingClientRect().width;
            const tabsListWidth =
              tabsListRef.current.getBoundingClientRect().width;
            setTabsOverflowing(tabsListWidth + 96 > containerWidth);
          }
        }, 100);
      }
    };

    // Listen for window resize
    window.addEventListener("resize", recalculateOverflow);

    // Listen for sidemenu toggle events (common event names)
    window.addEventListener("sidemenu-toggle", recalculateOverflow);
    window.addEventListener("sidebar-toggle", recalculateOverflow);
    window.addEventListener("navigation-toggle", recalculateOverflow);

    return () => {
      window.removeEventListener("resize", recalculateOverflow);
      window.removeEventListener("sidemenu-toggle", recalculateOverflow);
      window.removeEventListener("sidebar-toggle", recalculateOverflow);
      window.removeEventListener("navigation-toggle", recalculateOverflow);
    };
  }, []);

  // Load active tab
  React.useEffect(() => {
    if (activeTabId) setLoadedTabs((prev) => new Set(prev).add(activeTabId));
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
        setActiveTabId(newTargetTabId);
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
    setLoadedTabs((prev) => new Set(prev).add(tabId));
    setActiveTabId(tabId);
    setDropdownOpen(false);
    eventHandler("OnSelect", id, [tabOrder.indexOf(tabId)]);
  };

  const handleMouseDown = (e: React.MouseEvent, index: number) => {
    if (e.button === 1) {
      e.preventDefault();
      eventHandler("OnClose", id, [index]);
    }
  };

  const handleDragEnd = React.useCallback(
    (event: {
      active: { id: string | number };
      over: { id: string | number } | null;
    }) => {
      const { active, over } = event;
      if (active && over && active.id !== over.id) {
        setTabOrder((items) =>
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

    const closeHandler = handleTabEvent("OnClose");
    const refreshHandler = handleTabEvent("OnRefresh");

    window.addEventListener("tab-close", closeHandler);
    window.addEventListener("tab-refresh", refreshHandler);

    return () => {
      window.removeEventListener("tab-close", closeHandler);
      window.removeEventListener("tab-refresh", refreshHandler);
    };
  }, [tabOrder, eventHandler, id]);

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 8 } })
  );
  const showClose = events.includes("OnClose");
  const showRefresh = events.includes("OnRefresh");
  const orderedTabWidgets = tabOrder
    .map((id) =>
      tabWidgets.find(
        (tab) => (tab as React.ReactElement<TabWidgetProps>).props.id === id
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
              onClick={(e) => {
                e.stopPropagation();
                eventHandler("OnRefresh", id, [tabOrder.indexOf(tabId)]);
              }}
              className="opacity-60 p-1 rounded-full hover:bg-gray-200 hover:opacity-100 transition-colors"
            >
              <RotateCw className="w-3 h-3" />
            </a>
          )}
          {showClose && (
            <a
              onClick={(e) => {
                e.stopPropagation();
                eventHandler("OnClose", id, [tabOrder.indexOf(tabId)]);
              }}
              className="opacity-60 p-1 rounded-full hover:bg-gray-200 hover:opacity-100 transition-colors"
            >
              <X className="w-3 h-3" />
            </a>
          )}
        </div>
      </>
    );
  };

  return (
    <Tabs
      value={activeTabId ?? undefined}
      style={{ ...getWidth(width), ...getHeight(height) }}
      className={cn(
        removeParentPadding && "remove-parent-padding",
        "flex flex-col h-full"
      )}
    >
      <div className="flex-shrink-0">
        <div
          className="relative pl-12 pr-12 before:absolute before:inset-x-0 before:bottom-0 before:h-px before:bg-border before:z-0"
          ref={containerRef}
        >
          <ScrollArea className="w-full">
            <DndContext
              collisionDetection={closestCenter}
              onDragEnd={handleDragEnd}
              sensors={sensors}
            >
              <SortableContext items={tabOrder}>
                <TabsList
                  ref={tabsListRef}
                  className="relative h-auto w-max min-w-full gap-0.5 mt-3 bg-transparent p-0 flex justify-start"
                >
                  {orderedTabWidgets.map((tabWidget) => {
                    if (!React.isValidElement(tabWidget)) return null;
                    const { id } = tabWidget.props as TabWidgetProps;

                    return (
                      <SortableTabTrigger
                        key={id}
                        id={id}
                        value={id}
                        onMouseEnter={() => setHoveredTabId(id)}
                        onMouseLeave={() =>
                          setHoveredTabId((prev) => (prev === id ? null : prev))
                        }
                        onClick={() => handleTabSelect(id)}
                        onMouseDown={(e: React.MouseEvent) =>
                          handleMouseDown(e, tabOrder.indexOf(id))
                        }
                        className={cn(
                          "group overflow-hidden rounded-b-none py-2 data-[state=active]:z-10 data-[state=active]:shadow-none border-x border-t border-border",
                          variant === "Tabs" &&
                            "data-[state=active]:bg-background",
                          variant === "Content" &&
                            "border-b-2 border-b-transparent data-[state=active]:border-b-primary data-[state=active]:bg-background/50"
                        )}
                      >
                        {renderTabContent(tabWidget)}
                      </SortableTabTrigger>
                    );
                  })}
                </TabsList>
              </SortableContext>
            </DndContext>
            <ScrollBar
              orientation="horizontal"
              className="invisible-scrollbar"
            />
          </ScrollArea>

          {tabsOverflowing && (
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
                  <SortableContext
                    items={tabOrder}
                    strategy={verticalListSortingStrategy}
                  >
                    <div className="flex flex-col w-48">
                      {orderedTabWidgets.map((tabWidget) => {
                        if (!React.isValidElement(tabWidget)) return null;
                        const { title, id } = tabWidget.props as TabWidgetProps;

                        return (
                          <SortableDropdownMenuItem
                            key={id}
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
                  <DragOverlay>
                    {(() => {
                      const active = orderedTabWidgets.find(
                        (tab) =>
                          (tab as React.ReactElement<TabWidgetProps>)?.props
                            .id === activeTabId
                      );
                      if (active && React.isValidElement(active)) {
                        const { title } = active.props as TabWidgetProps;
                        return (
                          <div className="px-3 py-2 bg-muted rounded shadow text-left cursor-pointer select-none">
                            {title}
                          </div>
                        );
                      }
                      return null;
                    })()}
                  </DragOverlay>
                </DndContext>
              </DropdownMenuContent>
            </DropdownMenu>
          )}
        </div>
      </div>

      <div className="flex-1 overflow-hidden">
        {orderedTabWidgets.map((tabWidget) => {
          if (!React.isValidElement(tabWidget)) return null;
          const { id } = tabWidget.props as TabWidgetProps;

          if (!loadedTabs.has(id)) return null;

          return (
            <div
              key={id}
              className={cn(
                "h-full overflow-auto",
                activeTabId === id ? "block" : "hidden"
              )}
              style={getPadding(padding)}
            >
              {tabWidget}
            </div>
          );
        })}
      </div>
    </Tabs>
  );
};
