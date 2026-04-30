import React from "react";
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/components/ui/collapsible";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import { ChevronRight } from "lucide-react";
import Icon from "@/components/Icon";
import { cn } from "@/lib/utils";
import { MenuItem } from "@/types/widgets";
import { ActionRenderer } from "@/widgets/rowAction";
import { Densities } from "@/types/density";
import { densityTreeGap } from "@/components/ui/density-scale";

const TreeItemLabel: React.FC<{ label: string; tooltip?: string }> = ({ label, tooltip }) => {
  const spanRef = React.useRef<HTMLSpanElement>(null);
  const [isTruncated, setIsTruncated] = React.useState(false);
  const [showTooltip, setShowTooltip] = React.useState(false);

  React.useEffect(() => {
    const checkTruncation = () => {
      const el = spanRef.current;
      if (el) {
        setIsTruncated(el.scrollWidth > el.clientWidth);
      }
    };
    checkTruncation();
    window.addEventListener("resize", checkTruncation);
    return () => window.removeEventListener("resize", checkTruncation);
  }, [label]);

  return (
    <TooltipProvider>
      <Tooltip>
        <TooltipTrigger asChild>
          <span
            ref={spanRef}
            className="truncate flex-1"
            onMouseEnter={() => setShowTooltip(true)}
            onMouseLeave={() => setShowTooltip(false)}
          >
            {label}
          </span>
        </TooltipTrigger>
        {showTooltip && isTruncated && (
          <TooltipContent className="bg-popover text-popover-foreground shadow-md">
            {tooltip ?? label}
          </TooltipContent>
        )}
      </Tooltip>
    </TooltipProvider>
  );
};

interface TreeItemWidgetProps {
  item: MenuItem;
  density?: Densities;
  rowActions?: MenuItem[];
  hasSiblingWithChildren?: boolean;
  isNested?: boolean;
  onItemClick: (item: MenuItem) => void;
  onRowActionClick?: (item: MenuItem, action: MenuItem) => void;
}

export const TreeItem: React.FC<TreeItemWidgetProps> = ({
  item,
  density,
  rowActions,
  hasSiblingWithChildren,
  isNested,
  onItemClick,
  onRowActionClick,
}) => {
  const [isOpen, setIsOpen] = React.useState(item.expanded ?? false);
  const [hovered, setHovered] = React.useState(false);
  const hasChildren = item.children && item.children.length > 0;
  const gapClass = densityTreeGap[density ?? Densities.Medium];

  React.useEffect(() => {
    setIsOpen(item.expanded ?? false);
  }, [item.expanded]);

  const handleToggle = (e: React.MouseEvent) => {
    if (item.disabled) return;
    e.stopPropagation();
    setIsOpen((prev) => !prev);
  };

  const handleClick = (e: React.MouseEvent) => {
    if (item.disabled) return;
    e.stopPropagation();
    if (hasChildren) {
      setIsOpen((prev) => !prev);
    }
    onItemClick(item);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (item.disabled) return;
    if (e.key === "Enter" || e.key === " ") {
      e.preventDefault();
      if (hasChildren) {
        setIsOpen((prev) => !prev);
      } else {
        onItemClick(item);
      }
    }
    if (e.key === "ArrowRight" && hasChildren && !isOpen) {
      e.preventDefault();
      setIsOpen(true);
    }
    if (e.key === "ArrowLeft" && hasChildren && isOpen) {
      e.preventDefault();
      setIsOpen(false);
    }
  };

  if (hasChildren) {
    return (
      <Collapsible open={isOpen} onOpenChange={(val) => !item.disabled && setIsOpen(val)}>
        <div
          className={cn(
            "ivy-tree-item group flex items-center gap-1 flex-1 min-w-0 rounded-sm px-1 py-0 text-sm cursor-pointer select-none outline-none",
            "hover:bg-accent/50 transition-colors focus-visible:ring-1 focus-visible:ring-ring",
            item.disabled && "opacity-50 cursor-not-allowed",
          )}
          role="treeitem"
          aria-expanded={isOpen}
          aria-disabled={item.disabled}
          tabIndex={item.disabled ? -1 : 0}
          onKeyDown={handleKeyDown}
          onClick={handleClick}
          onMouseEnter={() => setHovered(true)}
          onMouseLeave={() => setHovered(false)}
        >
          {item.icon && item.icon !== "None" ? (
            <CollapsibleTrigger asChild>
              <button
                className="flex items-center justify-center h-5 w-5 shrink-0 rounded-sm hover:bg-accent transition-colors"
                onClick={handleToggle}
                tabIndex={-1}
                disabled={item.disabled}
              >
                <span style={{ display: hovered ? "inline-flex" : "none" }}>
                  <ChevronRight
                    className={cn(
                      "h-3.5 w-3.5 text-muted-foreground transition-transform duration-200",
                      isOpen && "rotate-90",
                    )}
                  />
                </span>
                <span style={{ display: hovered ? "none" : "inline-flex" }}>
                  <Icon className="h-4 w-4 shrink-0 text-muted-foreground" name={item.icon} />
                </span>
              </button>
            </CollapsibleTrigger>
          ) : (
            <CollapsibleTrigger asChild>
              <button
                className="flex items-center justify-center h-5 w-5 shrink-0 rounded-sm hover:bg-accent transition-colors"
                onClick={handleToggle}
                tabIndex={-1}
                disabled={item.disabled}
              >
                <ChevronRight
                  className={cn(
                    "h-3.5 w-3.5 text-muted-foreground transition-transform duration-200",
                    isOpen && "rotate-90",
                  )}
                />
              </button>
            </CollapsibleTrigger>
          )}
          <TreeItemLabel label={item.label} tooltip={item.tooltip} />

          {rowActions && rowActions.length > 0 && onRowActionClick && (
            <div
              className="opacity-0 group-hover:opacity-100 focus-within:opacity-100 has-[[data-state=open]]:opacity-100 flex items-center shrink-0"
              onClick={(e) => e.stopPropagation()}
              onKeyDown={(e) => e.stopPropagation()}
              onPointerDown={(e) => e.stopPropagation()}
              role="toolbar"
              aria-label="Row actions"
            >
              {rowActions.map((action) => (
                <ActionRenderer
                  key={action.tag || action.label}
                  action={action}
                  onActionClick={(clickedAction) => onRowActionClick(item, clickedAction)}
                  variant="ghost"
                />
              ))}
            </div>
          )}
        </div>
        <CollapsibleContent className="overflow-hidden transition-all data-[state=closed]:animate-accordion-up data-[state=open]:animate-accordion-down">
          <div
            className={cn(
              "ivy-tree-children flex flex-col pl-[1rem] ml-2 border-l border-border/50 mt-0.5",
              gapClass,
            )}
          >
            {item.children!.map((child) => (
              <TreeItem
                key={child.tag || child.label}
                item={child}
                density={density}
                onItemClick={onItemClick}
                rowActions={rowActions}
                hasSiblingWithChildren={item.children!.some(
                  (c) => c.children && c.children.length > 0,
                )}
                isNested
                onRowActionClick={onRowActionClick}
              />
            ))}
          </div>
        </CollapsibleContent>
      </Collapsible>
    );
  }

  return (
    <div
      className={cn(
        "ivy-tree-item group flex items-center flex-1 min-w-0 gap-1 rounded-sm px-1 py-0 text-sm cursor-pointer select-none outline-none",
        "hover:bg-accent/50 transition-colors focus-visible:ring-1 focus-visible:ring-ring",
        item.disabled && "opacity-50 cursor-not-allowed",
      )}
      role="treeitem"
      aria-disabled={item.disabled}
      tabIndex={item.disabled ? -1 : 0}
      onKeyDown={handleKeyDown}
      onClick={handleClick}
    >
      {hasSiblingWithChildren && !isNested && <span className="h-5 w-5 shrink-0" />}
      {item.icon && item.icon !== "None" && (
        <Icon className="h-4 w-4 shrink-0 text-muted-foreground" name={item.icon} />
      )}
      <TreeItemLabel label={item.label} tooltip={item.tooltip} />

      {rowActions && rowActions.length > 0 && onRowActionClick && (
        <div
          className="opacity-0 group-hover:opacity-100 focus-within:opacity-100 has-[[data-state=open]]:opacity-100 flex items-center shrink-0 pr-1"
          onClick={(e) => e.stopPropagation()}
          onKeyDown={(e) => e.stopPropagation()}
          onPointerDown={(e) => e.stopPropagation()}
          role="toolbar"
          aria-label="Row actions"
        >
          {rowActions.map((action) => (
            <ActionRenderer
              key={action.tag || action.label}
              action={action}
              onActionClick={(clickedAction) => onRowActionClick(item, clickedAction)}
              variant="ghost"
            />
          ))}
        </div>
      )}
    </div>
  );
};
