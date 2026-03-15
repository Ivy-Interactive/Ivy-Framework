import React, {
  useState,
  useEffect,
  useCallback,
  useMemo,
  useRef,
  useReducer,
} from 'react';

import Icon from '@/components/Icon';
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '@/components/ui/collapsible';
import { ScrollArea } from '@/components/ui/scroll-area';
import { ChevronRight, PanelLeftClose, PanelLeftOpen } from 'lucide-react';
import { MenuItem, WidgetEventHandlerType } from '@/types/widgets';
import { useFocusable } from '@/hooks/use-focus-management';
import { sidebarMenuRef } from './sidebar-refs';
import { useEventHandler } from '@/components/event-handler';
import { cn, getAppId } from '@/lib/utils';
import { getWidth } from '@/lib/styles';
import { Separator } from '@/components/ui/separator';

interface SidebarLayoutWidgetProps {
  slots?: {
    SidebarHeader?: React.ReactNode[];
    SidebarContent?: React.ReactNode[];
    SidebarFooter?: React.ReactNode[];
    MainContent: React.ReactNode[];
  };
  showToggleButton?: boolean;
  autoCollapseThreshold?: number; // Width threshold for auto-collapse (default: 768px)
  mainAppSidebar?: boolean;
  mainContentPadding?: number; // Padding for main content area (default: 2)
  width?: string; // Width of the sidebar (Size format: "Type:Value,MinType:MinValue,MaxType:MaxValue")
  open?: boolean; // Whether the sidebar starts open (default: true)
  resizable?: boolean; // Enable drag-to-resize on sidebar border
}

// Helper to parse a Size string to pixels
const parseSizeToPixels = (
  sizeStr: string | undefined,
  defaultPx: number
): number => {
  if (!sizeStr) return defaultPx;
  const [sizeType, value] = sizeStr.split(':');
  const numValue = parseFloat(value);
  if (isNaN(numValue)) return defaultPx;

  switch (sizeType.toLowerCase()) {
    case 'px':
      return numValue;
    case 'rem':
      // Assume 16px base font size
      return numValue * 16;
    case 'units':
      // Units are 0.25rem = 4px
      return numValue * 4;
    default:
      return defaultPx;
  }
};

// Helper function to check if a slot has meaningful content
// Checks both props.children (legacy) and props.node (MemoizedWidget)
const hasContent = (slot?: React.ReactNode[]): boolean => {
  if (!slot || slot.length === 0) return false;

  return slot.some(node => {
    if (node === null || node === undefined) return false;
    if (typeof node === 'string') return node.trim().length > 0;
    if (typeof node === 'number') return true;
    if (React.isValidElement(node)) {
      const props = node.props as {
        children?: React.ReactNode;
        node?: { children?: unknown[] };
      };
      // Check for MemoizedWidget's node prop first
      if (props.node !== undefined) {
        return true; // MemoizedWidget with a node always has content
      }
      // Legacy check for direct children
      if (props.children === null || props.children === undefined) return false;
      if (typeof props.children === 'string')
        return props.children.trim().length > 0;
      if (Array.isArray(props.children)) return props.children.length > 0;
      return true;
    }
    return false;
  });
};

export const SidebarLayoutWidget: React.FC<SidebarLayoutWidgetProps> = ({
  slots,
  showToggleButton = true,
  autoCollapseThreshold = 768,
  mainAppSidebar = false,
  mainContentPadding,
  width,
  open: openProp = true,
  resizable = false,
}) => {
  // Parse Size format: "Type:Value,MinType:MinValue,MaxType:MaxValue"
  const [wantedWidth, minWidthStr, maxWidthStr] = (width ?? '').split(',');

  // Get sidebar width from the width prop (default set in backend)
  const sidebarWidth = getWidth(width).width as string;
  const initialWidthPx = parseSizeToPixels(wantedWidth, 256);

  // Parse min/max constraints from Size API (defaults match Streamlit: 200-600px)
  const minWidthPx = parseSizeToPixels(minWidthStr, 200);
  const maxWidthPx = parseSizeToPixels(maxWidthStr, 600);

  // Initialize sidebar state based on current window width (only for main app sidebar)
  const getInitialSidebarState = () => {
    if (!mainAppSidebar) return true;
    if (!openProp) return false;

    // Check if we're in a browser environment
    if (typeof window !== 'undefined') {
      return window.innerWidth >= autoCollapseThreshold;
    }

    return true; // Default to open if we can't determine width
  };

  const [sidebarState, dispatchSidebar] = useReducer(
    (
      state: {
        isSidebarOpen: boolean;
        isManuallyToggled: boolean;
        currentWidth: number;
        isResizing: boolean;
        prevInitialWidthPx: number;
      },
      action:
        | Partial<typeof state>
        | ((prev: typeof state) => Partial<typeof state>)
    ) => {
      const updates = typeof action === 'function' ? action(state) : action;
      return { ...state, ...updates };
    },
    {
      isSidebarOpen: getInitialSidebarState(),
      isManuallyToggled: false,
      currentWidth: initialWidthPx,
      isResizing: false,
      prevInitialWidthPx: initialWidthPx,
    }
  );
  const {
    isSidebarOpen,
    isManuallyToggled,
    currentWidth,
    isResizing,
    prevInitialWidthPx,
  } = sidebarState;

  const containerRef = useRef<HTMLDivElement>(null);
  const sidebarRef = useRef<HTMLDivElement>(null);

  if (initialWidthPx !== prevInitialWidthPx) {
    dispatchSidebar({ prevInitialWidthPx: initialWidthPx });
    if (!isResizing) {
      dispatchSidebar({ currentWidth: initialWidthPx });
    }
  }

  // Handle resize drag
  const handleResizeStart = useCallback(
    (e: React.MouseEvent | React.TouchEvent) => {
      if (!resizable || !isSidebarOpen) return;

      e.preventDefault();
      dispatchSidebar({ isResizing: true });

      const startX = 'touches' in e ? e.touches[0].clientX : e.clientX;
      const startWidth = currentWidth;

      const handleMove = (moveEvent: MouseEvent | TouchEvent) => {
        const clientX =
          'touches' in moveEvent
            ? moveEvent.touches[0].clientX
            : moveEvent.clientX;
        const delta = clientX - startX;
        const newWidth = Math.min(
          maxWidthPx,
          Math.max(minWidthPx, startWidth + delta)
        );
        dispatchSidebar({ currentWidth: newWidth });
      };

      const handleEnd = () => {
        dispatchSidebar({ isResizing: false });
        document.removeEventListener('mousemove', handleMove);
        document.removeEventListener('mouseup', handleEnd);
        document.removeEventListener('touchmove', handleMove);
        document.removeEventListener('touchend', handleEnd);
      };

      document.addEventListener('mousemove', handleMove);
      document.addEventListener('mouseup', handleEnd);
      document.addEventListener('touchmove', handleMove, { passive: true });
      document.addEventListener('touchend', handleEnd, { passive: true });
    },
    [
      resizable,
      isSidebarOpen,
      currentWidth,
      minWidthPx,
      maxWidthPx,
      dispatchSidebar,
    ]
  );

  // Get the effective sidebar width (use currentWidth when resizable)
  const effectiveSidebarWidth = resizable ? `${currentWidth}px` : sidebarWidth;

  // Handle manual toggle
  const handleManualToggle = useCallback(() => {
    dispatchSidebar(prev => ({ isSidebarOpen: !prev.isSidebarOpen }));
    dispatchSidebar({ isManuallyToggled: true });
  }, [dispatchSidebar]);

  // Auto-collapse/expand based on width (only for main app sidebar)
  useEffect(() => {
    if (!mainAppSidebar) return;

    const mql = window.matchMedia(`(min-width: ${autoCollapseThreshold}px)`);

    const handleMediaChange = (e: MediaQueryListEvent | MediaQueryList) => {
      if (!isManuallyToggled) {
        dispatchSidebar({ isSidebarOpen: openProp && e.matches });
      }
    };

    handleMediaChange(mql);

    mql.addEventListener('change', handleMediaChange);
    return () => mql.removeEventListener('change', handleMediaChange);
  }, [
    autoCollapseThreshold,
    isManuallyToggled,
    mainAppSidebar,
    openProp,
    dispatchSidebar,
  ]);

  return (
    <div
      ref={containerRef}
      className="grid h-full w-full remove-parent-padding"
      style={{
        gridTemplateColumns: isSidebarOpen
          ? `${effectiveSidebarWidth} 1fr`
          : '0 1fr',
        transition: isResizing
          ? 'none'
          : 'grid-template-columns 300ms ease-in-out',
      }}
    >
      {/* Custom Sidebar with Slide Animation */}
      <div
        ref={sidebarRef}
        className={`flex h-full flex-col bg-background text-foreground border-r border-border relative overflow-hidden ${
          isResizing ? '' : 'transition-transform duration-300 ease-in-out'
        } ${isSidebarOpen ? 'translate-x-0' : '-translate-x-full'}`}
        style={{ width: effectiveSidebarWidth }}
      >
        {hasContent(slots?.SidebarHeader) && (
          <div className="flex flex-col shrink-0 p-2 space-y-4">
            {slots?.SidebarHeader}
          </div>
        )}
        {slots?.SidebarContent && (
          <div className="flex-1 min-h-0 min-w-0 overflow-hidden">
            <ScrollArea className="h-full w-full">
              <div className="p-2 space-y-2">{slots.SidebarContent}</div>
            </ScrollArea>
          </div>
        )}
        {hasContent(slots?.SidebarFooter) && (
          <div className="flex flex-col shrink-0">
            <div className="flex flex-col p-2 gap-4 min-h-0">
              {slots?.SidebarFooter}
            </div>
          </div>
        )}
        {/* Resize Handle */}
        {resizable && isSidebarOpen && (
          <div
            className={cn(
              'absolute top-0 right-0 w-1 h-full cursor-ew-resize group'
            )}
            onMouseDown={handleResizeStart}
            onTouchStart={handleResizeStart}
            role="separator"
            aria-orientation="vertical"
            aria-label="Resize sidebar"
            tabIndex={0}
            onKeyDown={e => {
              if (e.key === 'ArrowLeft') {
                dispatchSidebar(prev => ({
                  currentWidth: Math.max(minWidthPx, prev.currentWidth - 10),
                }));
              } else if (e.key === 'ArrowRight') {
                dispatchSidebar(prev => ({
                  currentWidth: Math.min(maxWidthPx, prev.currentWidth + 10),
                }));
              }
            }}
          >
            <div
              className={cn(
                'absolute top-1/2 -translate-y-1/2 right-0 w-1 h-8 rounded-full bg-border',
                'opacity-0 group-hover:opacity-100 transition-opacity',
                isResizing && 'opacity-100'
              )}
            />
          </div>
        )}
      </div>

      {/* Main Content - Always takes full remaining width */}
      <div
        className={cn(
          `relative h-full overflow-auto`,
          !mainAppSidebar ? `p-${mainContentPadding ?? 2}` : ''
        )}
      >
        {/* Toggle Button - Only show for main app sidebar */}
        {showToggleButton && mainAppSidebar && (
          <button
            onClick={handleManualToggle}
            className="absolute top-0 left-1 z-50 p-2 rounded-selector bg-background hover:bg-muted hover:text-accent-foreground cursor-pointer"
            style={{ marginTop: '3px' }}
            aria-label={isSidebarOpen ? 'Close sidebar' : 'Open sidebar'}
          >
            {isSidebarOpen ? (
              <PanelLeftClose className="h-4 w-4" />
            ) : (
              <PanelLeftOpen className="h-4 w-4" />
            )}
          </button>
        )}
        {slots?.MainContent}
      </div>
    </div>
  );
};

interface SidebarMenuWidgetProps {
  id: string;
  items: MenuItem[];
  searchActive?: boolean;
}

const EMPTY_ITEMS: MenuItem[] = [];

const getFlatItemsInSearchRenderOrder = (items: MenuItem[]): MenuItem[] => {
  const result: MenuItem[] = [];
  for (const item of items) {
    if (item.children && item.children.length > 0) {
      const groupsMap = item.children.reduce<Record<string, MenuItem[]>>(
        (acc, child) => {
          const path = child.path ?? '';
          (acc[path] ??= []).push(child);
          return acc;
        },
        {}
      );
      const groupsOrdered = Object.entries(groupsMap).sort(
        ([pathA], [pathB]) => {
          if (!pathA) return 1;
          if (!pathB) return -1;
          return 0;
        }
      );
      for (const [, pathItems] of groupsOrdered) {
        result.push(...pathItems);
      }
    }
  }
  return result;
};

// Animation duration for collapsible sections (in milliseconds)
const COLLAPSIBLE_ANIMATION_DURATION = 300;

const CollapsibleMenuItem: React.FC<{
  item: MenuItem;
  eventHandler: WidgetEventHandlerType;
  widgetId: string;
  level: number;
  activeTag?: string | null;
  expandedSections: Set<string>;
  onExpandChange: (label: string, expanded: boolean) => void;
}> = ({
  item,
  eventHandler,
  widgetId,
  level,
  activeTag,
  expandedSections,
  onExpandChange,
}) => {
  // Derive the open state from expandedSections or item.expanded
  const shouldBeOpen =
    expandedSections.has(item.label) || (item.expanded ?? false);
  const [isOpen, setIsOpen] = useState(shouldBeOpen);
  const [prevShouldBeOpen, setPrevShouldBeOpen] = useState(shouldBeOpen);
  const itemRef = useRef<HTMLLIElement>(null);

  // Sync local state with derived state
  if (shouldBeOpen !== prevShouldBeOpen) {
    setPrevShouldBeOpen(shouldBeOpen);
    setIsOpen(shouldBeOpen);
  }

  const handleOpenChange = (open: boolean) => {
    setIsOpen(open);
    onExpandChange(item.label, open);
  };

  const onItemClick = (item: MenuItem) => {
    if (!item.tag) return;
    eventHandler('OnSelect', widgetId, [item.tag]);
  };

  const onCtrlRightMouseClick = (e: React.MouseEvent, item: MenuItem) => {
    if (e.ctrlKey && e.button === 2 && !!item.tag) {
      e.preventDefault();
      eventHandler('OnCtrlRightClickSelect', widgetId, [item.tag]);
    }
  };

  const isActive = item.tag === activeTag;

  if (item.children && item.children.length > 0) {
    return (
      <Collapsible open={isOpen} onOpenChange={handleOpenChange}>
        <li
          className="relative"
          ref={itemRef}
          data-menu-item={item.tag || item.label}
        >
          <CollapsibleTrigger asChild>
            <button
              className={cn(
                'group flex w-full items-center gap-2 rounded-selector p-2 text-large-label hover:bg-accent hover:text-accent-foreground cursor-pointer h-8 text-left',
                isActive && 'bg-accent text-accent-foreground'
              )}
              onClick={() => {
                // For items with children, toggle the collapsible state
                // Only try to navigate if the item has a tag
                if (item.tag) {
                  onItemClick(item);
                }
              }}
              onMouseDown={e => onCtrlRightMouseClick(e, item)}
            >
              <Icon name={item.icon} size={16} />
              <span className="text-sm">{item.label}</span>
              <ChevronRight className="ml-auto h-4 w-4 transition-transform group-data-[state=open]:rotate-90" />
            </button>
          </CollapsibleTrigger>
          <CollapsibleContent>
            <ul className="mt-1 space-y-1 px-3">
              {item.children &&
                renderMenuItems(
                  item.children!,
                  eventHandler,
                  widgetId,
                  level + 1,
                  activeTag,
                  expandedSections,
                  onExpandChange
                )}
            </ul>
          </CollapsibleContent>
        </li>
      </Collapsible>
    );
  } else {
    return (
      <li
        key={item.label}
        ref={itemRef}
        data-menu-item={item.tag || item.label}
      >
        <button
          className={cn(
            'flex w-full items-center gap-2 rounded-selector p-2 text-large-label hover:bg-accent hover:text-accent-foreground cursor-pointer h-8 text-left',
            isActive && 'bg-accent text-accent-foreground'
          )}
          onClick={() => onItemClick(item)}
          onMouseDown={e => onCtrlRightMouseClick(e, item)}
        >
          <Icon name={item.icon} size={16} />
          <span className="text-sm">{item.label}</span>
        </button>
      </li>
    );
  }
};

const renderMenuItems = (
  items: MenuItem[],
  eventHandler: WidgetEventHandlerType,
  widgetId: string,
  level: number,
  activeTag?: string | null,
  expandedSections: Set<string> = new Set(),
  onExpandChange: (label: string, expanded: boolean) => void = () => {}
) => {
  const onItemClick = (item: MenuItem) => {
    if (!item.tag) return;
    eventHandler('OnSelect', widgetId, [item.tag]);
  };

  const onCtrlRightMouseClick = (e: React.MouseEvent, item: MenuItem) => {
    if (e.ctrlKey && e.button === 2 && !!item.tag) {
      e.preventDefault();
      eventHandler('OnCtrlRightClickSelect', widgetId, [item.tag]);
    }
  };

  return items.map(item => {
    if ('children' in item) {
      if (level === 0) {
        return (
          <div key={item.label} className="space-y-1 mt-6 first:mt-0">
            <h4 className="sticky top-0 z-10 bg-background px-2 py-2 text-small-label text-muted-foreground mb-0">
              {item.label}
            </h4>
            <ul className="space-y-1">
              {item.children &&
                renderMenuItems(
                  item.children!,
                  eventHandler,
                  widgetId,
                  1,
                  activeTag,
                  expandedSections,
                  onExpandChange
                )}
            </ul>
          </div>
        );
      } else {
        return (
          <CollapsibleMenuItem
            key={item.label}
            item={item}
            eventHandler={eventHandler}
            widgetId={widgetId}
            level={level}
            activeTag={activeTag}
            expandedSections={expandedSections}
            onExpandChange={onExpandChange}
          />
        );
      }
    } else {
      if (level === 0) {
        return <></>;
      }
      const isActive = item.tag === activeTag;
      if (level === 1) {
        return (
          <li key={item.tag} data-menu-item={item.tag}>
            <button
              className={cn(
                'flex w-full items-center gap-2 rounded-selector p-2 text-body hover:bg-accent hover:text-accent-foreground cursor-pointer h-8 text-left',
                isActive && 'bg-accent text-accent-foreground'
              )}
              onClick={() => onItemClick(item)}
              onMouseDown={e => onCtrlRightMouseClick(e, item)}
            >
              <Icon name={item.icon} size={16} />
              <span className="text-sm">{item.label}</span>
            </button>
          </li>
        );
      } else {
        return (
          <li key={item.tag} data-menu-item={item.tag}>
            <button
              className={cn(
                'flex w-full items-center gap-2 rounded-selector p-2 text-body hover:bg-accent hover:text-accent-foreground cursor-pointer h-8 text-left',
                isActive && 'bg-accent text-accent-foreground'
              )}
              onClick={() => onItemClick(item)}
              onMouseDown={e => onCtrlRightMouseClick(e, item)}
            >
              <Icon name={item.icon} size={16} />
              <span className="text-sm">{item.label}</span>
            </button>
          </li>
        );
      }
    }
  });
};

export const SidebarMenuWidget: React.FC<SidebarMenuWidgetProps> = ({
  id,
  items = EMPTY_ITEMS,
  searchActive = false,
}) => {
  const eventHandler = useEventHandler();
  const [selectedIndex, setSelectedIndex] = useState(0);
  const prevSearchActiveRef = React.useRef(searchActive);

  const [expandedSections, setExpandedSections] = useState<Set<string>>(
    new Set()
  );
  const containerRef = useRef<HTMLDivElement>(null);
  const isInitialMount = useRef(true);

  // Get active tag from URL instead of props
  const activeTag = getAppId();
  const prevActiveTagRef = useRef(activeTag);

  // Register only the sidebar menu container with useFocusable
  const { ref: focusRef } = useFocusable('sidebar-navigation', 1);

  const flatItems: MenuItem[] = useMemo(() => {
    return searchActive ? getFlatItemsInSearchRenderOrder(items) : [];
  }, [searchActive, items]);

  useEffect(() => {
    // Only reset when search becomes active (false -> true transition)
    if (searchActive && !prevSearchActiveRef.current) {
      queueMicrotask(() => setSelectedIndex(0));
    }
    prevSearchActiveRef.current = searchActive;
  }, [searchActive]);

  useEffect(() => {
    if (!searchActive || flatItems.length === 0) return;
    const el = containerRef.current?.querySelector<HTMLElement>(
      `[data-sidebar-result-index="${selectedIndex}"]`
    );
    if (!el) return;

    // Smooth scrolling logic for search results
    let p: HTMLElement | null = el.parentElement;
    while (p) {
      const { overflowY } = getComputedStyle(p);
      if (
        (overflowY === 'auto' ||
          overflowY === 'scroll' ||
          overflowY === 'overlay') &&
        p.scrollHeight > p.clientHeight
      ) {
        if (selectedIndex === 0) {
          p.scrollTo({ top: 0, behavior: 'smooth' });
          return;
        }
        const elRect = el.getBoundingClientRect();
        const portRect = p.getBoundingClientRect();
        const isAbove = elRect.top < portRect.top;
        const isBelow = elRect.bottom > portRect.bottom;
        if (isAbove || isBelow) {
          const delta = isAbove
            ? elRect.top - portRect.top
            : elRect.bottom - portRect.bottom;
          p.scrollTo({
            top: Math.max(0, p.scrollTop + delta),
            behavior: 'smooth',
          });
        }
        return;
      }
      p = p.parentElement;
    }
  }, [searchActive, flatItems.length, selectedIndex]);

  // Helper function to find the path to an item with a specific tag
  const findPathToTag = useCallback(
    (
      items: MenuItem[],
      targetTag: string,
      path: string[] = []
    ): string[] | null => {
      for (const item of items) {
        if (item.tag === targetTag) {
          return path;
        }
        if (item.children && item.children.length > 0) {
          const result = findPathToTag(item.children, targetTag, [
            ...path,
            item.label,
          ]);
          if (result) {
            return result;
          }
        }
      }
      return null;
    },
    []
  );

  // Expand sections and scroll to active item when activeTag changes
  useEffect(() => {
    if (!activeTag || searchActive) return;

    // Find the path to the active item
    const path = findPathToTag(items, activeTag);

    if (path && path.length > 0) {
      // Always expand parent sections
      setExpandedSections(new Set(path));

      // Only scroll to center on initial mount or when URL changes externally
      // (not when user clicks menu items)
      if (isInitialMount.current) {
        // Wait for the DOM to update, then scroll to the active item
        // Use a longer timeout to ensure collapsibles have fully expanded
        setTimeout(() => {
          try {
            const activeElement = containerRef.current?.querySelector(
              `[data-menu-item="${activeTag}"]`
            );
            if (activeElement) {
              activeElement.scrollIntoView({
                behavior: 'smooth',
                block: 'center',
                inline: 'nearest',
              });
            }
          } catch (error) {
            console.warn('Failed to scroll to active menu item:', error);
          }
        }, COLLAPSIBLE_ANIMATION_DURATION);

        isInitialMount.current = false;
      }
    }

    prevActiveTagRef.current = activeTag;
  }, [activeTag, items, searchActive, findPathToTag]);

  const handleExpandChange = useCallback((label: string, expanded: boolean) => {
    setExpandedSections(prev => {
      const next = new Set(prev);
      if (expanded) {
        next.add(label);
      } else {
        next.delete(label);
      }
      return next;
    });
  }, []);

  const handleMenuKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (!searchActive || flatItems.length === 0) return;
      if (e.key === 'ArrowDown') {
        setSelectedIndex(idx => Math.min(idx + 1, flatItems.length - 1));
        e.preventDefault();
      } else if (e.key === 'ArrowUp') {
        setSelectedIndex(idx => Math.max(idx - 1, 0));
        e.preventDefault();
      } else if (e.key === 'Enter') {
        const item = flatItems[selectedIndex];
        if (item && item.tag) {
          eventHandler('OnSelect', id, [item.tag]);
        }
        e.preventDefault();
      }
    },
    [searchActive, flatItems, selectedIndex, eventHandler, id]
  );

  const renderMenuItemsWithHighlight = (items: MenuItem[]) => {
    const onCtrlRightMouseClick = (e: React.MouseEvent, item: MenuItem) => {
      if (e.ctrlKey && e.button === 2 && !!item.tag) {
        e.preventDefault();
        eventHandler('OnCtrlRightClickSelect', id, [item.tag]);
      }
    };

    const renderResultItem = (item: MenuItem, showPath: boolean) => {
      const flatIdx = flatItems.findIndex(
        flatItem => flatItem.tag === item.tag
      );
      const isHovered = searchActive && flatIdx === selectedIndex;
      const isActivePage = item.tag === activeTag;
      return (
        <li key={item.tag}>
          <button
            {...(flatIdx >= 0 && { 'data-sidebar-result-index': flatIdx })}
            className={cn(
              'flex w-full rounded-selector p-2 text-sm hover:bg-accent/50 cursor-pointer min-h-8 text-left',
              showPath && item.path
                ? 'flex-col items-start gap-1'
                : 'items-center gap-2',
              isHovered && !isActivePage && 'bg-accent/30',
              isActivePage && 'bg-accent text-accent-foreground hover:bg-accent'
            )}
            tabIndex={-1}
            onClick={() => {
              if (item.tag) {
                if (searchActive && flatIdx !== -1) {
                  setSelectedIndex(flatIdx);
                }
                eventHandler('OnSelect', id, [item.tag]);
              }
            }}
            onMouseDown={e => onCtrlRightMouseClick(e, item)}
            onMouseEnter={() => {
              if (searchActive) {
                setSelectedIndex(flatIdx);
              }
            }}
          >
            {showPath && item.path && (
              <span className="text-xs text-muted-foreground truncate w-full">
                {item.path}
              </span>
            )}
            <div className="flex w-full items-center gap-2 min-w-0">
              <Icon name={item.icon} size={16} className="shrink-0" />
              <span className="text-sm truncate font-medium">{item.label}</span>
            </div>
          </button>
        </li>
      );
    };

    return items.map(item => {
      if (item.children && item.children.length > 0) {
        const children = item.children;
        const groupsMap = children.reduce<Record<string, MenuItem[]>>(
          (acc, child) => {
            const path = child.path ?? '';
            (acc[path] ??= []).push(child);
            return acc;
          },
          {}
        );
        const groups = Object.entries(groupsMap);
        const groupsOrdered = groups.sort(([pathA], [pathB]) => {
          if (!pathA) return 1;
          if (!pathB) return -1;
          return 0;
        });

        return (
          <div key={item.label} className="space-y-1 mt-6 first:mt-0">
            <h4 className="sticky top-0 z-10 bg-background px-2 py-2 text-small-label text-muted-foreground mb-0">
              {item.label}
            </h4>
            <ul className="space-y-1">
              {groupsOrdered.map(([path, pathItems], index) => (
                <React.Fragment key={path || '__none__'}>
                  {index > 0 && (
                    <li className="list-none py-2" aria-hidden>
                      <Separator orientation="horizontal" />
                    </li>
                  )}
                  <li className="list-none">
                    {path && (
                      <div className="px-2 pt-2 pb-1 text-xs text-muted-foreground truncate">
                        {path}
                      </div>
                    )}
                    <ul className="space-y-1">
                      {pathItems.map(child => renderResultItem(child, false))}
                    </ul>
                  </li>
                </React.Fragment>
              ))}
            </ul>
          </div>
        );
      } else {
        return renderResultItem(item, true);
      }
    });
  };

  return (
    <div
      ref={el => {
        focusRef(el);
        (
          sidebarMenuRef as React.MutableRefObject<HTMLDivElement | null>
        ).current = el;
        containerRef.current = el;
      }}
      role="menu"
      tabIndex={0}
      onFocus={() => {
        if (searchActive && flatItems.length > 0) setSelectedIndex(0);
      }}
      onKeyDown={handleMenuKeyDown}
      style={{ outline: 'none' }}
      data-sidebar-menu-widget
    >
      {searchActive ? (
        flatItems.length > 0 ? (
          renderMenuItemsWithHighlight(items)
        ) : (
          <div className="flex items-center justify-center p-4 text-descriptive text-muted-foreground">
            No results found
          </div>
        )
      ) : (
        renderMenuItems(
          items,
          eventHandler,
          id,
          0,
          activeTag,
          expandedSections,
          handleExpandChange
        )
      )}
    </div>
  );
};
