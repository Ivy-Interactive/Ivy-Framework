import React, { useState, useEffect, useCallback, useRef } from 'react';

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
  width?: string; // Width of the sidebar (default: 256px)
}

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
}) => {
  // Get sidebar width from the width prop (default set in backend)
  const sidebarWidth = getWidth(width).width as string;
  // Initialize sidebar state based on current window width (only for main app sidebar)
  const getInitialSidebarState = () => {
    if (!mainAppSidebar) return true;

    // Check if we're in a browser environment
    if (typeof window !== 'undefined') {
      return window.innerWidth >= autoCollapseThreshold;
    }

    return true; // Default to open if we can't determine width
  };

  const [isSidebarOpen, setIsSidebarOpen] = useState(getInitialSidebarState);
  const [isManuallyToggled, setIsManuallyToggled] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  // Handle manual toggle
  const handleManualToggle = useCallback(() => {
    setIsSidebarOpen(prev => !prev);
    setIsManuallyToggled(true);
  }, []);

  // Auto-collapse/expand based on width (only for main app sidebar)
  useEffect(() => {
    if (!mainAppSidebar) return;

    const mql = window.matchMedia(`(min-width: ${autoCollapseThreshold}px)`);

    const handleMediaChange = (e: MediaQueryListEvent | MediaQueryList) => {
      if (!isManuallyToggled) {
        setIsSidebarOpen(e.matches);
      }
    };

    handleMediaChange(mql);

    mql.addEventListener('change', handleMediaChange);
    return () => mql.removeEventListener('change', handleMediaChange);
  }, [autoCollapseThreshold, isManuallyToggled, mainAppSidebar]);

  return (
    <div
      ref={containerRef}
      className="grid h-full w-full remove-parent-padding"
      style={{
        gridTemplateColumns: isSidebarOpen ? `${sidebarWidth} 1fr` : '0 1fr',
        transition: 'grid-template-columns 300ms ease-in-out',
      }}
    >
      {/* Custom Sidebar with Slide Animation */}
      <div
        className={`flex h-full flex-col bg-background text-foreground border-r border-border transition-transform duration-300 ease-in-out relative overflow-hidden ${
          isSidebarOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
        style={{ width: sidebarWidth }}
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
}

function findPathToTag(
  items: MenuItem[],
  targetTag: string,
  path: string[] = []
): string[] | null {
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
}

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
  const itemRef = useRef<HTMLLIElement>(null);

  // Sync local state with derived state when expandedSections changes
  // Using useEffect to avoid setState during render
  useEffect(() => {
    setIsOpen(shouldBeOpen);
  }, [shouldBeOpen]);

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
  items = [],
}) => {
  const eventHandler = useEventHandler();
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

  // Expand sections and scroll to active item when activeTag changes
  useEffect(() => {
    if (!activeTag) return;

    // Find the path to the active item
    const path = findPathToTag(items, activeTag);

    if (path && path.length > 0) {
      // Always expand parent sections when activeTag changes (e.g. URL navigation)
      // eslint-disable-next-line react-hooks/set-state-in-effect -- intentional sync of URL to expanded state
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
  }, [activeTag, items]);

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

  return (
    <div
      ref={el => {
        focusRef(el);
        (
          sidebarMenuRef as React.MutableRefObject<HTMLDivElement | null>
        ).current = el;
        containerRef.current = el;
      }}
      tabIndex={0}
      style={{ outline: 'none' }}
      data-sidebar-menu-widget
    >
      {renderMenuItems(
        items,
        eventHandler,
        id,
        0,
        activeTag,
        expandedSections,
        handleExpandChange
      )}
    </div>
  );
};
