import React, { useState, useEffect, useCallback, useMemo, useRef, useReducer } from "react";

import Icon from "@/components/Icon";
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/components/ui/collapsible";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import { ChevronRight, PanelLeftClose, PanelLeftOpen } from "lucide-react";
import { MenuItem, WidgetEventHandlerType } from "@/types/widgets";
import { useFocusable } from "@/hooks/use-focus-management";
import { useShortcut } from "@/lib/useShortcut";
import { sidebarMenuRef } from "./sidebar-refs";
import { useEventHandler } from "@/components/event-handler";
import { cn, getAppId } from "@/lib/utils";
import { getWidth } from "@/lib/styles";
import { Separator } from "@/components/ui/separator";
import { SidebarMenuBadge } from "@/components/ui/sidebar";
import { SidebarLayoutContext, useSidebarLayout } from "./sidebar-layout-context";

// Wide enough for a menu item's full icon box (container padding + button padding + 16px icon)
// so icons keep their x-position while the sidebar width animates.
const SIDEBAR_RAIL_WIDTH_PX = 54;

interface SidebarLayoutWidgetProps {
  slots?: {
    SidebarHeader?: React.ReactNode[];
    SidebarContent?: React.ReactNode[];
    SidebarFooter?: React.ReactNode[];
    SidebarHeaderCollapsed?: React.ReactNode[];
    SidebarFooterCollapsed?: React.ReactNode[];
    MainContent: React.ReactNode[];
  };
  showToggleButton?: boolean;
  autoCollapseThreshold?: number; // Width threshold for auto-collapse (default: 768px)
  mainAppSidebar?: boolean;
  mainContentPadding?: number; // Padding for main content area (default: 2)
  width?: string; // Width of the sidebar (Size format: "Type:Value,MinType:MinValue,MaxType:MaxValue")
  open?: boolean; // Whether the sidebar starts open (default: true)
  resizable?: boolean; // Enable drag-to-resize on sidebar border
  sidebarContentScroll?: "None" | "Auto"; // Controls whether sidebar content is wrapped in ScrollArea (default: Auto)
}

// Helper to parse a Size string to pixels
const parseSizeToPixels = (sizeStr: string | undefined, defaultPx: number): number => {
  if (!sizeStr) return defaultPx;
  const [sizeType, value] = sizeStr.split(":");
  const numValue = parseFloat(value);
  if (isNaN(numValue)) return defaultPx;

  switch (sizeType.toLowerCase()) {
    case "px":
      return numValue;
    case "rem":
      // Assume 16px base font size
      return numValue * 16;
    case "units":
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

  return slot.some((node) => {
    if (node === null || node === undefined) return false;
    if (typeof node === "string") return node.trim().length > 0;
    if (typeof node === "number") return true;
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
      if (typeof props.children === "string") return props.children.trim().length > 0;
      if (Array.isArray(props.children)) return props.children.length > 0;
      return true;
    }
    return false;
  });
};

const SidebarToggleButton: React.FC<{ isSidebarOpen: boolean; onToggle: () => void }> = ({
  isSidebarOpen,
  onToggle,
}) => (
  <TooltipProvider delayDuration={300}>
    <Tooltip>
      <TooltipTrigger asChild>
        <button
          onClick={onToggle}
          className="p-2 rounded-selector text-muted-foreground hover:bg-accent hover:text-accent-foreground cursor-pointer"
          aria-label={isSidebarOpen ? "Close sidebar" : "Open sidebar"}
        >
          {isSidebarOpen ? (
            <PanelLeftClose className="size-4" />
          ) : (
            <PanelLeftOpen className="size-4" />
          )}
        </button>
      </TooltipTrigger>
      <TooltipContent side="right">Toggle sidebar (Ctrl+B)</TooltipContent>
    </Tooltip>
  </TooltipProvider>
);

export const SidebarLayoutWidget: React.FC<SidebarLayoutWidgetProps> = ({
  slots,
  showToggleButton = true,
  autoCollapseThreshold = 768,
  mainAppSidebar = false,
  mainContentPadding,
  width,
  open: openProp = true,
  resizable = false,
  sidebarContentScroll = "Auto",
}) => {
  const collapseEnabled = true;
  // Parse Size format: "Type:Value,MinType:MinValue,MaxType:MaxValue"
  const [wantedWidth, minWidthStr, maxWidthStr] = (width ?? "").split(",");

  // Get sidebar width from the width prop (default set in backend)
  const sidebarWidth = getWidth(width).width as string;
  const initialWidthPx = parseSizeToPixels(wantedWidth, 256);

  // Parse min/max constraints from Size API (defaults match Streamlit: 200-600px)
  const minWidthPx = parseSizeToPixels(minWidthStr, 200);
  const maxWidthPx = parseSizeToPixels(maxWidthStr, 600);

  // Initialize sidebar state based on current window width (when collapse is enabled)
  const getInitialSidebarState = () => {
    if (!collapseEnabled) return true;
    if (!openProp) return false;

    // Check if we're in a browser environment
    if (typeof window !== "undefined") {
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
      action: Partial<typeof state> | ((prev: typeof state) => Partial<typeof state>),
    ) => {
      const updates = typeof action === "function" ? action(state) : action;
      return { ...state, ...updates };
    },
    {
      isSidebarOpen: getInitialSidebarState(),
      isManuallyToggled: false,
      currentWidth: initialWidthPx,
      isResizing: false,
      prevInitialWidthPx: initialWidthPx,
    },
  );
  const { isSidebarOpen, isManuallyToggled, currentWidth, isResizing, prevInitialWidthPx } =
    sidebarState;

  const [isMobileViewport, setIsMobileViewport] = useState(() =>
    typeof window !== "undefined" ? window.innerWidth < autoCollapseThreshold : false,
  );

  const MOBILE_DRAWER_CONTENT_PEEK_PX = 48;

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

      const startX = "touches" in e ? e.touches[0].clientX : e.clientX;
      const startWidth = currentWidth;
      let animationFrameId: number | null = null;
      let latestWidth = startWidth;

      const handleMove = (moveEvent: MouseEvent | TouchEvent) => {
        const clientX = "touches" in moveEvent ? moveEvent.touches[0].clientX : moveEvent.clientX;
        const delta = clientX - startX;
        latestWidth = Math.min(maxWidthPx, Math.max(minWidthPx, startWidth + delta));

        if (containerRef.current) {
          containerRef.current.style.gridTemplateColumns = `${latestWidth}px 1fr`;
        }
        if (sidebarRef.current) {
          sidebarRef.current.style.width = `${latestWidth}px`;
        }

        if (animationFrameId === null) {
          animationFrameId = requestAnimationFrame(() => {
            animationFrameId = null;
            dispatchSidebar({ currentWidth: latestWidth });
          });
        }
      };

      const handleEnd = () => {
        if (animationFrameId !== null) {
          cancelAnimationFrame(animationFrameId);
          animationFrameId = null;
        }
        dispatchSidebar({ isResizing: false, currentWidth: latestWidth });
        document.removeEventListener("mousemove", handleMove);
        document.removeEventListener("mouseup", handleEnd);
        document.removeEventListener("touchmove", handleMove);
        document.removeEventListener("touchend", handleEnd);
      };

      document.addEventListener("mousemove", handleMove);
      document.addEventListener("mouseup", handleEnd);
      document.addEventListener("touchmove", handleMove, { passive: true });
      document.addEventListener("touchend", handleEnd, { passive: true });
    },
    [resizable, isSidebarOpen, currentWidth, minWidthPx, maxWidthPx, dispatchSidebar],
  );

  // On mobile, the main app sidebar expands to a near-full-width drawer over the content.
  const effectiveSidebarWidth =
    mainAppSidebar && isMobileViewport
      ? `calc(100vw - ${MOBILE_DRAWER_CONTENT_PEEK_PX}px)`
      : resizable
        ? `${currentWidth}px`
        : sidebarWidth;

  // On desktop, the main app sidebar collapses to an icon rail instead of sliding offscreen.
  const isIconRail = mainAppSidebar && !isMobileViewport;
  const isCollapsedToRail = isIconRail && !isSidebarOpen;

  // Handle manual toggle
  const handleManualToggle = useCallback(() => {
    dispatchSidebar((prev) => ({ isSidebarOpen: !prev.isSidebarOpen }));
    dispatchSidebar({ isManuallyToggled: true });
  }, [dispatchSidebar]);

  const sidebarContextValue = useMemo(
    () => ({
      collapsed: isCollapsedToRail,
      expand: () => dispatchSidebar({ isSidebarOpen: true, isManuallyToggled: true }),
      toggle: handleManualToggle,
    }),
    [isCollapsedToRail, handleManualToggle, dispatchSidebar],
  );

  // Register keyboard shortcut for toggling sidebar (Ctrl+B)
  useShortcut("sidebar-layout-toggle", "CTRL+B", handleManualToggle, {
    skipInInputs: true,
    disabled: !mainAppSidebar,
  });

  // Auto-collapse/expand based on width (when collapse is enabled)
  useEffect(() => {
    if (!collapseEnabled) return;

    const mql = window.matchMedia(`(min-width: ${autoCollapseThreshold}px)`);

    const handleMediaChange = (e: MediaQueryListEvent | MediaQueryList) => {
      setIsMobileViewport(!e.matches);
      if (!isManuallyToggled) {
        dispatchSidebar({ isSidebarOpen: openProp && e.matches });
      }
    };

    handleMediaChange(mql);

    mql.addEventListener("change", handleMediaChange);
    return () => mql.removeEventListener("change", handleMediaChange);
  }, [autoCollapseThreshold, isManuallyToggled, collapseEnabled, openProp, dispatchSidebar]);

  // On mobile, collapse the main app sidebar after a leaf menu item is selected so the chosen
  // page is revealed instead of staying behind the open drawer.
  useEffect(() => {
    if (!mainAppSidebar) return;
    const sidebarEl = sidebarRef.current;
    if (!sidebarEl) return;

    const handleClick = (e: MouseEvent) => {
      if (!isMobileViewport) return;
      const target = e.target as HTMLElement | null;
      const menuItem = target?.closest<HTMLElement>("[data-menu-item]");
      if (!menuItem) return;
      if (menuItem.querySelector("ul")) return; // group header: only toggles its own submenu
      dispatchSidebar({ isSidebarOpen: false });
    };

    sidebarEl.addEventListener("click", handleClick);
    return () => sidebarEl.removeEventListener("click", handleClick);
  }, [mainAppSidebar, isMobileViewport, dispatchSidebar]);

  return (
    <SidebarLayoutContext.Provider value={sidebarContextValue}>
      {isResizing && (
        <div className="fixed inset-0 z-50 cursor-ew-resize select-none pointer-events-auto" />
      )}
      <div
        ref={containerRef}
        className="grid h-full w-full remove-parent-padding"
        style={{
          gridTemplateColumns: isSidebarOpen
            ? `${effectiveSidebarWidth} 1fr`
            : isIconRail
              ? `${SIDEBAR_RAIL_WIDTH_PX}px 1fr`
              : "0 1fr",
          transition: isResizing ? "none" : "grid-template-columns 300ms ease-in-out",
        }}
      >
        {/* Custom Sidebar with Slide Animation */}
        <div
          ref={sidebarRef}
          className={`flex h-full flex-col bg-card text-foreground border-r border-border relative overflow-hidden ${
            isResizing ? "" : "transition-[width,transform] duration-300 ease-in-out"
          } ${isSidebarOpen || isCollapsedToRail ? "translate-x-0" : "-translate-x-full"}`}
          style={{
            width: isCollapsedToRail ? `${SIDEBAR_RAIL_WIDTH_PX}px` : effectiveSidebarWidth,
          }}
        >
          {/* In the rail the toggle is its own centered row at the top; when expanded it sits in
              the top-right corner, aligned with the top of the header title instead of pushing it
              down. The header slot lives in a separate DOM subtree, so we overlay rather than nest. */}
          {showToggleButton && isIconRail && isCollapsedToRail && (
            <div className="flex justify-center shrink-0 px-2 pt-2 pb-2">
              <SidebarToggleButton isSidebarOpen={isSidebarOpen} onToggle={handleManualToggle} />
            </div>
          )}
          <div className="relative shrink-0">
            {showToggleButton && isIconRail && !isCollapsedToRail && (
              <div className="absolute top-2 right-2 z-10">
                <SidebarToggleButton isSidebarOpen={isSidebarOpen} onToggle={handleManualToggle} />
              </div>
            )}
            {isCollapsedToRail
              ? hasContent(slots?.SidebarHeaderCollapsed) && (
                  <div className="flex flex-col items-center px-2 pb-2 gap-y-2 h-fit animate-in fade-in duration-300">
                    {slots?.SidebarHeaderCollapsed}
                  </div>
                )
              : hasContent(slots?.SidebarHeader) && (
                  <div className="flex flex-col px-2 pb-2 gap-y-4 h-fit">
                    {slots?.SidebarHeader}
                  </div>
                )}
          </div>
          {slots?.SidebarContent &&
            (sidebarContentScroll === "None" ? (
              <div className="flex-1 min-h-0 min-w-0 overflow-hidden">{slots.SidebarContent}</div>
            ) : (
              <div className="flex-1 min-h-0 min-w-0 overflow-hidden">
                <ScrollArea className="h-full w-full">
                  <div className="p-2 w-full">{slots.SidebarContent}</div>
                </ScrollArea>
              </div>
            ))}
          {isCollapsedToRail
            ? hasContent(slots?.SidebarFooterCollapsed) && (
                <div className="flex flex-col items-center shrink-0 p-2 gap-y-2 animate-in fade-in duration-300">
                  {slots?.SidebarFooterCollapsed}
                </div>
              )
            : hasContent(slots?.SidebarFooter) && (
                <div className="flex flex-col shrink-0">
                  <div className="flex flex-col p-2 gap-4 min-h-0">{slots?.SidebarFooter}</div>
                </div>
              )}
          {/* Resize Handle */}
          {resizable && isSidebarOpen && (
            <div
              className={cn("absolute top-0 right-0 w-1 h-full cursor-ew-resize group")}
              onMouseDown={handleResizeStart}
              onTouchStart={handleResizeStart}
              role="separator"
              aria-orientation="vertical"
              aria-label="Resize sidebar"
              tabIndex={0}
              onKeyDown={(e) => {
                if (e.key === "ArrowLeft") {
                  dispatchSidebar((prev) => ({
                    currentWidth: Math.max(minWidthPx, prev.currentWidth - 10),
                  }));
                } else if (e.key === "ArrowRight") {
                  dispatchSidebar((prev) => ({
                    currentWidth: Math.min(maxWidthPx, prev.currentWidth + 10),
                  }));
                }
              }}
            >
              <div
                className={cn(
                  "absolute top-1/2 -translate-y-1/2 right-0 w-1 h-8 rounded-full bg-border",
                  "opacity-0 group-hover:opacity-100 transition-opacity",
                  isResizing && "opacity-100",
                )}
              />
            </div>
          )}
        </div>

        {/* min-w-0 keeps the 1fr grid track from overflowing the viewport on narrow screens. */}
        <div
          className={cn(
            `relative h-full overflow-auto min-w-0`,
            !mainAppSidebar ? `p-${mainContentPadding ?? 2}` : "",
          )}
        >
          {showToggleButton && mainAppSidebar && isMobileViewport && (
            <TooltipProvider delayDuration={300}>
              <Tooltip>
                <TooltipTrigger asChild>
                  <button
                    onClick={handleManualToggle}
                    className="absolute top-0 left-1 z-50 p-2 rounded-selector bg-background hover:bg-muted hover:text-accent-foreground cursor-pointer"
                    style={{ marginTop: "3px" }}
                    aria-label={isSidebarOpen ? "Close sidebar" : "Open sidebar"}
                  >
                    {isSidebarOpen ? (
                      <PanelLeftClose className="size-4" />
                    ) : (
                      <PanelLeftOpen className="size-4" />
                    )}
                  </button>
                </TooltipTrigger>
                <TooltipContent side="right">Toggle sidebar (Ctrl+B)</TooltipContent>
              </Tooltip>
            </TooltipProvider>
          )}
          {/* On mobile, the content sliver beside the open drawer closes it instead of being interactive. */}
          {mainAppSidebar && isMobileViewport && isSidebarOpen && (
            <div
              className="absolute inset-0 z-40 cursor-pointer"
              onClick={() => dispatchSidebar({ isSidebarOpen: false })}
              aria-label="Close sidebar"
            />
          )}
          {slots?.MainContent}
        </div>
      </div>
    </SidebarLayoutContext.Provider>
  );
};

interface SidebarMenuWidgetProps {
  id: string;
  events?: string[];
  items: MenuItem[];
  searchActive?: boolean;
}

const EMPTY_ITEMS: MenuItem[] = [];

const getFlatItemsInSearchRenderOrder = (items: MenuItem[]): MenuItem[] => {
  const result: MenuItem[] = [];
  for (const item of items) {
    if (item.children && item.children.length > 0) {
      const groupsMap = item.children.reduce<Record<string, MenuItem[]>>((acc, child) => {
        const path = child.path ?? "";
        (acc[path] ??= []).push(child);
        return acc;
      }, {});
      const groupsOrdered = Object.entries(groupsMap).sort(([pathA], [pathB]) => {
        if (!pathA) return 1;
        if (!pathB) return -1;
        return 0;
      });
      for (const [, pathItems] of groupsOrdered) {
        result.push(...pathItems);
      }
    }
  }
  return result;
};

// Animation duration for collapsible sections (in milliseconds)
const COLLAPSIBLE_ANIMATION_DURATION = 300;

const CollapsedRailTooltip: React.FC<{
  label: string;
  collapsed: boolean;
  children: React.ReactElement;
}> = ({ label, collapsed, children }) =>
  collapsed ? (
    <TooltipProvider delayDuration={300}>
      <Tooltip>
        <TooltipTrigger asChild>{children}</TooltipTrigger>
        <TooltipContent side="right">{label}</TooltipContent>
      </Tooltip>
    </TooltipProvider>
  ) : (
    children
  );

// Collapse the label to zero width in the rail (not just opacity) so the button shrinks to icon
// width; otherwise the still-laid-out labels keep every button label-wide and centering scatters.
const menuLabelClasses = (collapsed: boolean) =>
  cn(
    "text-sm whitespace-nowrap overflow-hidden transition-[max-width,opacity] duration-200",
    collapsed ? "max-w-0 opacity-0" : "max-w-[12rem] opacity-100",
  );

// In the rail, center the icon (matching the centered header/footer/toggle icons) and drop the
// horizontal padding and label gap so it lines up with them instead of sitting a hair to the right.
const menuButtonRailClasses = (collapsed: boolean) =>
  collapsed ? "justify-center gap-0 px-0" : "";

// Icon-less items would render as blank rows in the rail, so fall back to the label's initial.
const MenuItemIcon: React.FC<{ item: MenuItem; collapsed: boolean }> = ({ item, collapsed }) =>
  !item.icon && collapsed ? (
    <span className="size-4 shrink-0 flex items-center justify-center text-xs font-medium">
      {(item.label ?? "").charAt(0).toUpperCase()}
    </span>
  ) : (
    <Icon name={item.icon} size={16} className="shrink-0" />
  );

const CollapsibleMenuItem: React.FC<{
  item: MenuItem;
  events: string[];
  eventHandler: WidgetEventHandlerType;
  widgetId: string;
  level: number;
  activeTag?: string | null;
  expandedSections: Set<string>;
  onExpandChange: (pathKey: string, expanded: boolean) => void;
  pathKey: string;
}> = ({
  item,
  events,
  eventHandler,
  widgetId,
  level,
  activeTag,
  expandedSections,
  onExpandChange,
  pathKey,
}) => {
  // Derive the open state from expandedSections or item.expanded
  const shouldBeOpen = expandedSections.has(pathKey) || (item.expanded ?? false);
  const [isOpen, setIsOpen] = useState(shouldBeOpen);
  const prevShouldBeOpenRef = useRef(shouldBeOpen);
  const [isPenHovered, setIsPenHovered] = useState(false);
  const itemRef = useRef<HTMLLIElement>(null);
  const { collapsed, expand } = useSidebarLayout();

  // Sync local state with derived state
  if (shouldBeOpen !== prevShouldBeOpenRef.current) {
    prevShouldBeOpenRef.current = shouldBeOpen;
    setIsOpen(shouldBeOpen);
  }

  const handleOpenChange = (open: boolean) => {
    setIsOpen(open);
    onExpandChange(pathKey, open);
  };

  const onItemClick = (item: MenuItem) => {
    if (!item.tag) return;
    if (events.includes("OnSelect")) eventHandler("OnSelect", widgetId, [item.tag]);
  };

  const onCtrlRightMouseClick = (e: React.MouseEvent, item: MenuItem) => {
    if (e.ctrlKey && e.button === 2 && !!item.tag) {
      e.preventDefault();
      if (events.includes("OnCtrlRightClickSelect"))
        eventHandler("OnCtrlRightClickSelect", widgetId, [item.tag]);
    }
  };

  const isActive = item.tag === activeTag;

  if (item.children && item.children.length > 0) {
    const triggerClasses = cn(
      "group flex w-full items-center gap-2 rounded-selector p-2 text-large-label cursor-pointer h-8 text-left overflow-hidden",
      isActive
        ? "bg-secondary text-accent-foreground"
        : "hover:bg-accent hover:text-accent-foreground",
      isPenHovered && "bg-accent text-accent-foreground",
      menuButtonRailClasses(collapsed),
    );

    // In the collapsed rail, a section icon expands the sidebar with the section opened
    // instead of toggling a hidden submenu.
    if (collapsed) {
      return (
        <li className="relative" ref={itemRef} data-menu-item={item.tag || item.label}>
          <CollapsedRailTooltip label={item.label} collapsed>
            <button
              className={triggerClasses}
              onClick={() => {
                expand();
                handleOpenChange(true);
              }}
            >
              <MenuItemIcon item={item} collapsed={collapsed} />
              <span className={menuLabelClasses(collapsed)}>{item.label}</span>
            </button>
          </CollapsedRailTooltip>
        </li>
      );
    }

    return (
      <Collapsible open={isOpen} onOpenChange={handleOpenChange}>
        <li className="relative" ref={itemRef} data-menu-item={item.tag || item.label}>
          <CollapsibleTrigger asChild>
            <button
              className={triggerClasses}
              onClick={() => {
                // For items with children, toggle the collapsible state
                // Only try to navigate if the item has a tag
                if (item.tag) {
                  onItemClick(item);
                }
              }}
              onMouseDown={(e) => onCtrlRightMouseClick(e, item)}
              onPointerEnter={(e) => {
                if (e.pointerType === "pen") {
                  setIsPenHovered(true);
                }
              }}
              onPointerLeave={(e) => {
                if (e.pointerType === "pen") {
                  setIsPenHovered(false);
                }
              }}
            >
              <MenuItemIcon item={item} collapsed={collapsed} />
              <span className={menuLabelClasses(collapsed)}>{item.label}</span>
              <ChevronRight className="ml-auto size-4 shrink-0 transition-transform group-data-[state=open]:rotate-90" />
            </button>
          </CollapsibleTrigger>
          <CollapsibleContent>
            <ul className="flex flex-col mt-1 gap-y-0.5 px-3">
              {item.children &&
                renderMenuItems(
                  item.children!,
                  events,
                  eventHandler,
                  widgetId,
                  level + 1,
                  activeTag,
                  expandedSections,
                  onExpandChange,
                  pathKey,
                  collapsed,
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
        className="relative"
      >
        <CollapsedRailTooltip
          label={item.badge ? `${item.label} (${item.badge})` : item.label}
          collapsed={collapsed}
        >
          <button
            className={cn(
              "flex w-full items-center gap-2 rounded-selector p-2 text-large-label cursor-pointer h-8 text-left relative",
              collapsed ? "overflow-visible" : "overflow-hidden",
              isActive
                ? "bg-secondary text-accent-foreground"
                : "hover:bg-accent hover:text-accent-foreground",
              isPenHovered && "bg-accent text-accent-foreground",
              menuButtonRailClasses(collapsed),
            )}
            onClick={() => onItemClick(item)}
            onMouseDown={(e) => onCtrlRightMouseClick(e, item)}
            onPointerEnter={(e) => {
              if (e.pointerType === "pen") {
                setIsPenHovered(true);
              }
            }}
            onPointerLeave={(e) => {
              if (e.pointerType === "pen") {
                setIsPenHovered(false);
              }
            }}
          >
            <MenuItemIcon item={item} collapsed={collapsed} />
            <span className={menuLabelClasses(collapsed)}>{item.label}</span>
            {item.badge &&
              (collapsed ? (
                <span className="absolute -top-1 -right-1 z-10 pointer-events-none inline-flex h-4 min-w-4 px-1 items-center justify-center rounded-full bg-primary text-[10px] font-bold leading-none text-primary-foreground select-none">
                  {item.badge}
                </span>
              ) : (
                <SidebarMenuBadge className="transition-all duration-200">
                  {item.badge}
                </SidebarMenuBadge>
              ))}
          </button>
        </CollapsedRailTooltip>
      </li>
    );
  }
};

// Helper component to handle pen hover state for menu items
const MenuItemButton: React.FC<{
  item: MenuItem;
  isActive: boolean;
  level: number;
  onClick: () => void;
  onMouseDown: (e: React.MouseEvent) => void;
}> = ({ item, isActive, level, onClick, onMouseDown }) => {
  const [isPenHovered, setIsPenHovered] = useState(false);
  const { collapsed } = useSidebarLayout();

  return (
    <CollapsedRailTooltip
      label={item.badge ? `${item.label} (${item.badge})` : item.label}
      collapsed={collapsed}
    >
      <button
        className={cn(
          "flex w-full items-center gap-2 rounded-selector p-2 cursor-pointer h-8 text-left relative",
          collapsed ? "overflow-visible" : "overflow-hidden",
          level === 1 ? "text-body" : "text-body",
          isActive
            ? "bg-secondary text-accent-foreground"
            : "hover:bg-accent hover:text-accent-foreground",
          isPenHovered && "bg-accent text-accent-foreground",
          menuButtonRailClasses(collapsed),
        )}
        onClick={onClick}
        onMouseDown={onMouseDown}
        onPointerEnter={(e) => {
          if (e.pointerType === "pen") {
            setIsPenHovered(true);
          }
        }}
        onPointerLeave={(e) => {
          if (e.pointerType === "pen") {
            setIsPenHovered(false);
          }
        }}
      >
        <MenuItemIcon item={item} collapsed={collapsed} />
        <span className={menuLabelClasses(collapsed)}>{item.label}</span>
        {item.badge &&
          (collapsed ? (
            <span className="absolute -top-1 -right-1 z-10 pointer-events-none inline-flex h-4 min-w-4 px-1 items-center justify-center rounded-full bg-primary text-[10px] font-bold leading-none text-primary-foreground select-none">
              {item.badge}
            </span>
          ) : (
            <SidebarMenuBadge className="transition-all duration-200">
              {item.badge}
            </SidebarMenuBadge>
          ))}
      </button>
    </CollapsedRailTooltip>
  );
};

const renderMenuItems = (
  items: MenuItem[],
  events: string[],
  eventHandler: WidgetEventHandlerType,
  widgetId: string,
  level: number,
  activeTag?: string | null,
  expandedSections: Set<string> = new Set(),
  onExpandChange: (pathKey: string, expanded: boolean) => void = () => {},
  parentPathKey: string = "",
  collapsed: boolean = false,
) => {
  const onItemClick = (item: MenuItem) => {
    if (!item.tag) return;
    if (events.includes("OnSelect")) eventHandler("OnSelect", widgetId, [item.tag]);
  };

  const onCtrlRightMouseClick = (e: React.MouseEvent, item: MenuItem) => {
    if (e.ctrlKey && e.button === 2 && !!item.tag) {
      e.preventDefault();
      if (events.includes("OnCtrlRightClickSelect"))
        eventHandler("OnCtrlRightClickSelect", widgetId, [item.tag]);
    }
  };

  return items.map((item) => {
    const itemPathKey = parentPathKey ? `${parentPathKey}/${item.label}` : item.label;
    if ("children" in item) {
      if (level === 0) {
        return (
          <div
            key={itemPathKey}
            className={cn(
              "flex flex-col gap-y-1 first:mt-0 transition-all duration-300",
              collapsed ? "mt-2" : "mt-6",
            )}
          >
            <h4
              className={cn(
                "sticky top-0 z-10 bg-card text-small-label text-muted-foreground mb-0 overflow-hidden whitespace-nowrap transition-all duration-300",
                collapsed ? "h-0 w-0 p-0 opacity-0" : "h-8 p-2 opacity-100",
              )}
            >
              {item.label}
            </h4>
            <ul className="flex flex-col gap-y-0.5">
              {item.children &&
                renderMenuItems(
                  item.children!,
                  events,
                  eventHandler,
                  widgetId,
                  1,
                  activeTag,
                  expandedSections,
                  onExpandChange,
                  itemPathKey,
                  collapsed,
                )}
            </ul>
          </div>
        );
      } else {
        return (
          <CollapsibleMenuItem
            key={itemPathKey}
            item={item}
            events={events}
            eventHandler={eventHandler}
            widgetId={widgetId}
            level={level}
            activeTag={activeTag}
            expandedSections={expandedSections}
            onExpandChange={onExpandChange}
            pathKey={itemPathKey}
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
          <li key={item.tag} data-menu-item={item.tag} className="relative">
            <MenuItemButton
              item={item}
              isActive={isActive}
              level={level}
              onClick={() => onItemClick(item)}
              onMouseDown={(e) => onCtrlRightMouseClick(e, item)}
            />
          </li>
        );
      } else {
        return (
          <li key={item.tag} data-menu-item={item.tag} className="relative">
            <MenuItemButton
              item={item}
              isActive={isActive}
              level={level}
              onClick={() => onItemClick(item)}
              onMouseDown={(e) => onCtrlRightMouseClick(e, item)}
            />
          </li>
        );
      }
    }
  });
};

const EMPTY_EVENTS: string[] = [];

export const SidebarMenuWidget: React.FC<SidebarMenuWidgetProps> = ({
  id,
  events = EMPTY_EVENTS,
  items = EMPTY_ITEMS,
  searchActive = false,
}) => {
  const eventHandler = useEventHandler();
  const { collapsed } = useSidebarLayout();
  const [selectedIndex, setSelectedIndex] = useState(0);
  const prevSearchActiveRef = React.useRef(searchActive);

  const [expandedSections, setExpandedSections] = useState<Set<string>>(new Set());
  const containerRef = useRef<HTMLDivElement>(null);
  const isInitialMount = useRef(true);

  // Get active tag from URL instead of props
  const activeTag = getAppId();
  const prevActiveTagRef = useRef(activeTag);

  // Register only the sidebar menu container with useFocusable
  const { ref: focusRef } = useFocusable("sidebar-navigation", 1);

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
      `[data-sidebar-result-index="${selectedIndex}"]`,
    );
    if (!el) return;

    // Smooth scrolling logic for search results
    let p: HTMLElement | null = el.parentElement;
    while (p) {
      const { overflowY } = getComputedStyle(p);
      if (
        (overflowY === "auto" || overflowY === "scroll" || overflowY === "overlay") &&
        p.scrollHeight > p.clientHeight
      ) {
        if (selectedIndex === 0) {
          p.scrollTo({ top: 0, behavior: "smooth" });
          return;
        }
        const elRect = el.getBoundingClientRect();
        const portRect = p.getBoundingClientRect();
        const isAbove = elRect.top < portRect.top;
        const isBelow = elRect.bottom > portRect.bottom;
        if (isAbove || isBelow) {
          const delta = isAbove ? elRect.top - portRect.top : elRect.bottom - portRect.bottom;
          p.scrollTo({
            top: Math.max(0, p.scrollTop + delta),
            behavior: "smooth",
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
      currentPathKeys: string[] = [],
      parentPathKey: string = "",
    ): string[] | null => {
      for (const item of items) {
        if (item.tag === targetTag) {
          return currentPathKeys;
        }
        if (item.children && item.children.length > 0) {
          const itemPathKey = parentPathKey ? `${parentPathKey}/${item.label}` : item.label;
          const result = findPathToTag(
            item.children,
            targetTag,
            [...currentPathKeys, itemPathKey],
            itemPathKey,
          );
          if (result) {
            return result;
          }
        }
      }
      return null;
    },
    [],
  );

  // Expand sections and scroll to active item when activeTag changes
  useEffect(() => {
    if (!activeTag || searchActive) return;

    // Find the path to the active item
    const path = findPathToTag(items, activeTag);
    let timeoutId: ReturnType<typeof setTimeout> | undefined;

    if (path && path.length > 0) {
      // Always expand parent sections
      setExpandedSections(new Set(path));

      // Only scroll to center on initial mount or when URL changes externally
      // (not when user clicks menu items)
      if (isInitialMount.current) {
        // Wait for the DOM to update, then scroll to the active item
        // Use a longer timeout to ensure collapsibles have fully expanded
        timeoutId = setTimeout(() => {
          try {
            const activeElement = containerRef.current?.querySelector(
              `[data-menu-item="${activeTag}"]`,
            );
            if (activeElement) {
              activeElement.scrollIntoView({
                behavior: "smooth",
                block: "center",
                inline: "nearest",
              });
            }
          } catch (error) {
            console.warn("Failed to scroll to active menu item:", error);
          }
        }, COLLAPSIBLE_ANIMATION_DURATION);

        isInitialMount.current = false;
      }
    }

    prevActiveTagRef.current = activeTag;

    return () => {
      if (timeoutId) clearTimeout(timeoutId);
    };
  }, [activeTag, items, searchActive, findPathToTag]);

  const handleExpandChange = useCallback((pathKey: string, expanded: boolean) => {
    setExpandedSections((prev) => {
      const next = new Set(prev);
      if (expanded) {
        next.add(pathKey);
      } else {
        next.delete(pathKey);
      }
      return next;
    });
  }, []);

  const handleMenuKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (!searchActive || flatItems.length === 0) return;
      if (e.key === "ArrowDown") {
        setSelectedIndex((idx) => Math.min(idx + 1, flatItems.length - 1));
        e.preventDefault();
      } else if (e.key === "ArrowUp") {
        setSelectedIndex((idx) => Math.max(idx - 1, 0));
        e.preventDefault();
      } else if (e.key === "Enter") {
        const item = flatItems[selectedIndex];
        if (item && item.tag) {
          if (events.includes("OnSelect")) eventHandler("OnSelect", id, [item.tag]);
        }
        e.preventDefault();
      }
    },
    [searchActive, flatItems, selectedIndex, events, eventHandler, id],
  );

  const renderMenuItemsWithHighlight = (items: MenuItem[]) => {
    const onCtrlRightMouseClick = (e: React.MouseEvent, item: MenuItem) => {
      if (e.ctrlKey && e.button === 2 && !!item.tag) {
        e.preventDefault();
        if (events.includes("OnCtrlRightClickSelect"))
          eventHandler("OnCtrlRightClickSelect", id, [item.tag]);
      }
    };

    const renderResultItem = (item: MenuItem, showPath: boolean) => {
      const flatIdx = flatItems.findIndex((flatItem) => flatItem.tag === item.tag);
      const isHovered = searchActive && flatIdx === selectedIndex;
      const isActivePage = item.tag === activeTag;
      return (
        <li key={item.tag}>
          <button
            {...(flatIdx >= 0 && { "data-sidebar-result-index": flatIdx })}
            className={cn(
              "flex w-full rounded-selector p-2 text-sm cursor-pointer min-h-8 text-left",
              showPath && item.path ? "flex-col items-start gap-1" : "items-center gap-2",
              isActivePage
                ? "bg-secondary text-accent-foreground"
                : "hover:bg-accent hover:text-accent-foreground",
              isHovered && !isActivePage && "bg-accent",
            )}
            tabIndex={-1}
            onClick={() => {
              if (item.tag) {
                if (searchActive && flatIdx !== -1) {
                  setSelectedIndex(flatIdx);
                }
                if (events.includes("OnSelect")) eventHandler("OnSelect", id, [item.tag]);
              }
            }}
            onMouseDown={(e) => onCtrlRightMouseClick(e, item)}
            onMouseEnter={() => {
              if (searchActive) {
                setSelectedIndex(flatIdx);
              }
            }}
          >
            {showPath && item.path && (
              <span className="text-xs text-muted-foreground truncate w-full">{item.path}</span>
            )}
            <div className="flex w-full items-center gap-2 min-w-0">
              <Icon name={item.icon} size={16} className="shrink-0" />
              <span className="text-sm truncate font-medium">{item.label}</span>
            </div>
          </button>
        </li>
      );
    };

    return items.map((item) => {
      if (item.children && item.children.length > 0) {
        const children = item.children;
        const groupsMap = children.reduce<Record<string, MenuItem[]>>((acc, child) => {
          const path = child.path ?? "";
          (acc[path] ??= []).push(child);
          return acc;
        }, {});
        const groups = Object.entries(groupsMap);
        const groupsOrdered = groups.sort(([pathA], [pathB]) => {
          if (!pathA) return 1;
          if (!pathB) return -1;
          return 0;
        });

        return (
          <div key={item.label} className="flex flex-col gap-y-1 mt-6 first:mt-0">
            <h4 className="sticky top-0 z-10 bg-card p-2 text-small-label text-muted-foreground mb-0">
              {item.label}
            </h4>
            <ul className="flex flex-col gap-y-0.5">
              {groupsOrdered.map(([path, pathItems], index) => (
                <React.Fragment key={path || "__none__"}>
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
                    <ul className="flex flex-col gap-y-0.5">
                      {pathItems.map((child) => renderResultItem(child, false))}
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
      ref={(el) => {
        focusRef(el);
        (sidebarMenuRef as React.MutableRefObject<HTMLDivElement | null>).current = el;
        containerRef.current = el;
      }}
      role="menu"
      aria-label="Sidebar menu"
      tabIndex={0}
      onFocus={() => {
        if (searchActive && flatItems.length > 0) setSelectedIndex(0);
      }}
      onKeyDown={handleMenuKeyDown}
      style={{ outline: "none" }}
      data-sidebar-menu-widget
    >
      {searchActive && !collapsed ? (
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
          events,
          eventHandler,
          id,
          0,
          activeTag,
          expandedSections,
          handleExpandChange,
          "",
          collapsed,
        )
      )}
    </div>
  );
};
