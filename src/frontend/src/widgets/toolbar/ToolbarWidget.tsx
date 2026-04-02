import React, { useCallback } from "react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import Icon from "@/components/Icon";
import withTooltip from "@/hoc/withTooltip";
import { useEventHandler } from "@/components/event-handler";
import { MenuItem } from "@/types/widgets";

const ButtonWithTooltip = withTooltip(Button);

interface ToolbarWidgetProps {
  id: string;
  items: MenuItem[];
  disabled?: boolean;
}

interface ToolbarItemGroupProps {
  items: MenuItem[];
  onItemClick: (item: MenuItem) => void;
  disabled?: boolean;
}

const ToolbarItemGroup: React.FC<ToolbarItemGroupProps> = ({
  items,
  onItemClick,
  disabled = false,
}) => {
  return items.map((item, i) => {
    // Handle group variant
    if (item.variant === "Group" && item.children) {
      const groupKey = item.label || `group-${i}`;
      return (
        <div key={groupKey} className="flex items-center gap-1" role="group">
          <ToolbarItemGroup
            items={item.children}
            onItemClick={onItemClick}
            disabled={disabled || item.disabled}
          />
        </div>
      );
    }

    // Handle separator variant
    if (item.variant === "Separator") {
      const sepKey = `separator-${i}`;
      return (
        <div
          key={sepKey}
          role="separator"
          aria-orientation="vertical"
          className="h-6 w-px bg-border mx-1"
        />
      );
    }

    // Handle default toolbar button
    const isDisabled = disabled || item.disabled;
    const isIconOnly = item.icon && !item.label;
    const itemKey = item.label || item.icon || `item-${i}`;

    return (
      <ButtonWithTooltip
        key={itemKey}
        size={isIconOnly ? "icon-sm" : "sm"}
        variant="ghost"
        onClick={() => !isDisabled && onItemClick(item)}
        disabled={isDisabled}
        className={cn(item.checked && "bg-accent")}
        tooltipText={item.tooltip}
      >
        {item.icon && <Icon name={item.icon} style={{ width: "1rem", height: "1rem" }} />}
        {item.label && <span>{item.label}</span>}
      </ButtonWithTooltip>
    );
  });
};

export const ToolbarWidget: React.FC<ToolbarWidgetProps> = ({
  id,
  items = [],
  disabled = false,
}) => {
  const eventHandler = useEventHandler();

  const onItemClick = useCallback(
    (item: MenuItem) => {
      if (!item.tag) return;

      // First fire the widget's event
      eventHandler("OnSelect", id, [item.tag]);

      // Then invoke the item's own OnSelect handler if present
      if (item.onSelect) {
        item.onSelect(item);
      }
    },
    [id, eventHandler],
  );

  return (
    <div
      role="toolbar"
      aria-disabled={disabled}
      className={cn(
        "flex items-center gap-2 p-2 bg-background border rounded-md",
        disabled && "opacity-50 pointer-events-none",
      )}
    >
      <ToolbarItemGroup items={items} onItemClick={onItemClick} disabled={disabled} />
    </div>
  );
};
