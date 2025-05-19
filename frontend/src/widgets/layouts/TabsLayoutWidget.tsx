import * as React from 'react';
import { ScrollArea, ScrollBar } from "@/components/ui/scroll-area";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import Icon from '@/components/Icon';
import { RotateCw, X, ChevronDown } from 'lucide-react';
import { useEventHandler } from '@/components/EventHandlerContext';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';
import { getHeight, getPadding, getWidth } from '@/lib/styles';
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuItem
} from '@/components/ui/dropdown-menu';
import { DndContext, closestCenter, DragOverlay, PointerSensor, useSensor, useSensors } from '@dnd-kit/core';
import { SortableContext, useSortable, arrayMove, verticalListSortingStrategy } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';

interface TabWidgetProps {
  children: React.ReactNode[];
  title: string;
  id: string;
  badge?:string;
  icon?:string;
}

export const TabWidget: React.FC<TabWidgetProps> = ({
  children
}) => {
  return (
    <div className='h-full'> 
      {children} 
    </div>
  );
};

TabWidget.displayName = 'TabWidget';

interface TabsLayoutWidgetProps {
  id: string;
  variant?: "Tabs" | "Content";
  removeParentPadding?: boolean;
  selectedIndex?: number;
  children: React.ReactElement<TabWidgetProps>[];
  events: string[];
  width?: string;
  height?: string;
  padding?: string;
}

// Helper component for sortable tab triggers
interface SortableTabTriggerProps {
  id: string;
  value: string;
  onClick: () => void;
  onMouseDown: (e: React.MouseEvent) => void;
  className?: string;
  children: React.ReactNode;
}
function SortableTabTrigger({ id, value, onClick, onMouseDown, className, children, ...props }: SortableTabTriggerProps) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id });
  const style = {
    transform: transform ? `translate3d(${transform.x}px, 0, 0)` : undefined,
    transition,
    opacity: isDragging ? 0.5 : 1,
    zIndex: isDragging ? 100 : undefined,
  };
  return (
    <TabsTrigger
      ref={setNodeRef}
      style={style}
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

// Helper component for sortable dropdown menu items
interface SortableDropdownMenuItemProps {
  id: string;
  children: React.ReactNode;
  onClick: () => void;
  isActive?: boolean;
}
function SortableDropdownMenuItem({ id, children, onClick, isActive }: SortableDropdownMenuItemProps) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id });
  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
    zIndex: isDragging ? 100 : undefined,
    cursor: 'grab',
    background: isActive ? 'var(--muted)' : undefined,
    width: '100%',
    display: 'block',
  };
  return (
    <div
      ref={setNodeRef}
      style={style}
      {...attributes}
      {...listeners}
      onClick={onClick}
      className="px-3 py-2 hover:bg-muted text-left cursor-pointer select-none"
      tabIndex={0}
    >
      {children}
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
  variant
}: TabsLayoutWidgetProps) => {
  // All hooks must be called before any early return
  const eventHandler = useEventHandler();
  const tabsLayoutId = id;
  const containerRef = React.useRef<HTMLDivElement>(null);
  const tabsListRef = React.useRef<HTMLDivElement>(null);
  const [tabsOverflowing, setTabsOverflowing] = React.useState(false);
  // Tab order state
  const tabWidgets = React.Children.toArray(children).filter((child) =>
    React.isValidElement(child) &&
    (child.type as any)?.displayName === 'TabWidget'
  );
  const [tabOrder, setTabOrder] = React.useState(tabWidgets.map(tab => (tab as any).props.id));
  // Keep tabOrder in sync with children
  React.useEffect(() => {
    setTabOrder(tabWidgets.map(tab => (tab as any).props.id));
  }, [children]);
  React.useEffect(() => {
    function checkOverflow() {
      if (containerRef.current && tabsListRef.current) {
        setTabsOverflowing(
          tabsListRef.current.scrollWidth > containerRef.current.offsetWidth
        );
      }
    }
    checkOverflow();
    window.addEventListener('resize', checkOverflow);
    return () => window.removeEventListener('resize', checkOverflow);
  }, [React.Children.count(children)]);
  // DnD sensors with activation constraint
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8, // require 8px movement before drag starts
      },
    })
  );
  // Track the currently dragged item for DragOverlay
  const [activeDragId, setActiveDragId] = React.useState<string | null>(null);
  if(tabWidgets.length === 0) return <div className='remove-parent-padding'></div>;

  // Map tabOrder to tabWidgets
  const orderedTabWidgets = tabOrder.map(id => tabWidgets.find(tab => (tab as any).props.id === id)).filter(Boolean);

  const activeTab = selectedIndex != null && React.isValidElement(tabWidgets[selectedIndex]) ? tabWidgets[selectedIndex].props.id : null;

  const showClose = events.includes("OnClose");
  const showRefresh = events.includes("OnRefresh");

  const handleMouseDown = (e: React.MouseEvent, index: number) => {
    if (e.button === 1) {
      e.preventDefault(); 
      eventHandler("OnClose", tabsLayoutId, [index])
    }
  };

  const styles:React.CSSProperties = { 
    ...getWidth(width),
    ...getHeight(height),
  };

  // DnD handler
  const handleDragEnd = (event: any) => {
    const { active, over } = event;
    if (active && over && active.id !== over.id) {
      setTabOrder((items) => {
        const oldIndex = items.indexOf(active.id);
        const newIndex = items.indexOf(over.id);
        return arrayMove(items, oldIndex, newIndex);
      });
    }
  };

  return (
    <Tabs value={activeTab} style={styles} className={
      cn(
        removeParentPadding && 'remove-parent-padding',
        'flex flex-col h-full'
      )}>
      <div className="flex-shrink-0">
        <div className="relative pl-12" ref={containerRef}>
          <ScrollArea className="w-full pr-12" scrollbarPosition="top" horizontalScroll>
            <DndContext collisionDetection={closestCenter} onDragEnd={handleDragEnd} sensors={sensors}>
              <SortableContext items={tabOrder}>
                <TabsList
                  ref={tabsListRef}
                  className={cn(
                    "relative h-auto w-max min-w-full gap-0.5 mt-3 bg-transparent p-0 before:absolute before:inset-x-0 before:bottom-0 before:h-px flex justify-start",
                    variant === "Tabs" && "before:bg-border",
                    variant === "Content" && ""
                  )}>
                  {orderedTabWidgets.map((tabWidget, index) => {
                    if (React.isValidElement(tabWidget)) {
                      const { title, id, icon, badge  } = tabWidget.props as TabWidgetProps;
                      return (
                        <SortableTabTrigger
                          key={id}
                          id={id}
                          value={id}
                          onClick={() => eventHandler("OnSelect", tabsLayoutId, [tabOrder.indexOf(id)])}
                          onMouseDown={(e: React.MouseEvent) => handleMouseDown(e, tabOrder.indexOf(id))}
                          className={cn(
                            "group overflow-hidden rounded-b-none py-2 data-[state=active]:z-10 data-[state=active]:shadow-none",
                            (variant === "Tabs" && index === 0) && "ml-12",
                            variant === "Tabs" && "border-x border-t border-border bg-muted",
                            variant === "Content" && "border-b-2 data-[state=active]:border-b-primary"
                          )}
                        >
                          {icon && <Icon
                            name={icon}
                            className="-ms-0.5 me-1.5 opacity-60"
                            size={16}
                            aria-hidden="true"
                          />}
                          <span>{title}</span>
                          {badge && <Badge
                            variant="default"
                            className={cn(
                              "ml-2",
                              "w-min",
                              "whitespace-nowrap",
                              "visible",
                              (showClose || showRefresh) && "group-hover:hidden"
                            )}
                          >{badge}</Badge>}
                          {(showClose || showRefresh) && <div className="absolute ml-2 items-center flex gap-0 invisible group-hover:visible group-hover:relative">
                            {showRefresh && <a
                              onClick={(e) => {
                                e.stopPropagation();
                                eventHandler("OnRefresh", tabsLayoutId, [tabOrder.indexOf(id)])
                              }}
                              className="opacity-60 p-1 rounded-full hover:bg-gray-200 hover:opacity-100 transition-colors"
                            >
                              <RotateCw className="w-3 h-3" />
                            </a>}
                            {showClose && <a
                              onClick={(e) => {
                                e.stopPropagation();
                                eventHandler("OnClose", tabsLayoutId, [tabOrder.indexOf(id)])
                              }}
                              className="opacity-60 p-1 rounded-full  hover:bg-gray-200 hover:opacity-100 transition-colors"
                            >
                              <X className="w-3 h-3" />
                            </a>}
                          </div>}
                        </SortableTabTrigger>
                      );
                    }
                    return null;
                  })}
                </TabsList>
              </SortableContext>
            </DndContext>
            <ScrollBar orientation="horizontal" className="invisible-scrollbar" />
            <ScrollBar orientation="horizontal" scrollbarPosition="top" className="invisible-scrollbar" />
          </ScrollArea>
          {tabsOverflowing && (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <button
                  className="absolute right-0 top-1/2 -translate-y-1/2 rounded p-2 hover:bg-muted transition"
                  aria-label="Show more tabs"
                  type="button"
                >
                  <ChevronDown className="w-5 h-5" />
                </button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DndContext collisionDetection={closestCenter} onDragEnd={handleDragEnd} sensors={sensors}>
                  <SortableContext items={tabOrder} strategy={verticalListSortingStrategy}>
                    <div className="flex flex-col w-48">
                      {orderedTabWidgets.map((tabWidget, index) => {
                        if (React.isValidElement(tabWidget)) {
                          const { title, id } = tabWidget.props as TabWidgetProps;
                          return (
                            <SortableDropdownMenuItem
                              key={id}
                              id={id}
                              onClick={() => eventHandler("OnSelect", tabsLayoutId, [tabOrder.indexOf(id)])}
                              isActive={activeTab === id}
                            >
                              {title}
                            </SortableDropdownMenuItem>
                          );
                        }
                        return null;
                      })}
                    </div>
                  </SortableContext>
                  <DragOverlay>
                    {/** Show a visual clone of the dragged item */}
                    {(() => {
                      const active = orderedTabWidgets.find(tab => (tab as any)?.props.id === activeTab);
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
        {orderedTabWidgets.map((tabWidget, _) => {
          if (React.isValidElement(tabWidget)) {
            const { id } = tabWidget.props as TabWidgetProps;
            return (
              <div
                key={id}
                className={cn(
                  'h-full overflow-auto',
                  activeTab === id ? 'block' : 'hidden'
                )}
                style={{
                  ...getPadding(padding)
                }}
              >
                {tabWidget}
              </div>
            );
          }
          return null;
        })}
      </div>
    </Tabs>
  );
};
