import React from "react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { cn } from "@/lib/utils";
import { ChevronDown } from "lucide-react";
import { DndContext, closestCenter } from "@dnd-kit/core";
import { SortableContext } from "@dnd-kit/sortable";
import { Badge } from "@/components/ui/badge";
import { SortableDropdownMenuItem } from "./Sortable";
import { getTabProps } from "../utils/tabUtils";

interface TabsDropdownMenuProps {
  dropdownOpen: boolean;
  setDropdownOpen: React.Dispatch<React.SetStateAction<boolean>>;
  hiddenTabs: string[];
  tabOrder: string[];
  orderedTabWidgets: React.ReactElement[];
  activeTabId: string | null;
  showClose: boolean;
  sensors: ReturnType<typeof import("@dnd-kit/core").useSensors>;
  handleDragEnd: (event: {
    active: { id: string | number };
    over: { id: string | number } | null;
  }) => void;
  handleTabSelect: (tabId: string) => void;
  isUserInitiatedChangeRef: React.MutableRefObject<boolean>;
}

/**
 * Dropdown menu component for displaying hidden tabs when they overflow.
 * Supports drag-and-drop reordering within the dropdown.
 *
 * Returns an object with trigger button and menu content to allow flexible placement.
 */
export const TabsDropdownMenu: React.FC<TabsDropdownMenuProps> = ({
  dropdownOpen,
  setDropdownOpen,
  hiddenTabs,
  tabOrder,
  orderedTabWidgets,
  activeTabId,
  showClose,
  sensors,
  handleDragEnd,
  handleTabSelect,
  isUserInitiatedChangeRef,
}) => {
  const menuContent =
    hiddenTabs.length > 0 ? (
      <DropdownMenuContent align="end">
        <DndContext collisionDetection={closestCenter} onDragEnd={handleDragEnd} sensors={sensors}>
          <SortableContext items={tabOrder}>
            <div className="flex flex-col gap-1 w-48">
              {orderedTabWidgets.map((tabWidget) => {
                if (!React.isValidElement(tabWidget)) return null;
                const props = getTabProps(tabWidget);
                if (!props?.id) return null;
                const { title, id, badge } = props;

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
                    showClose={showClose}
                  >
                    <span className="flex items-center">
                      {title}
                      {badge && (
                        <Badge variant="primary" className="ml-2 w-min whitespace-nowrap">
                          {badge}
                        </Badge>
                      )}
                    </span>
                  </SortableDropdownMenuItem>
                );
              })}
            </div>
          </SortableContext>
        </DndContext>
      </DropdownMenuContent>
    ) : null;

  return (
    <DropdownMenu open={dropdownOpen} onOpenChange={setDropdownOpen}>
      <DropdownMenuTrigger asChild>
        <Button
          variant="ghost"
          size="icon"
          className={cn(
            "h-7 w-7 bg-transparent transition-opacity flex-shrink-0 flex items-center justify-center ml-2",
            hiddenTabs.length > 0 ? "opacity-100" : "opacity-0 pointer-events-none",
          )}
          aria-label="Show more tabs"
        >
          <ChevronDown className="w-5 h-5" />
        </Button>
      </DropdownMenuTrigger>
      {menuContent}
    </DropdownMenu>
  );
};
