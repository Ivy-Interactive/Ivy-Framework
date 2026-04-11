import React, { useCallback } from "react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import Icon from "@/components/Icon";
import withTooltip from "@/hoc/withTooltip";
import { useEventHandler } from "@/components/event-handler";
import { MenuItem } from "@/types/widgets";
import { Densities } from "@/types/density";

const ButtonWithTooltip = withTooltip(Button);
const EMPTY_ITEMS: MenuItem[] = [];

interface ToolbarWidgetProps {
  id: string;
  items: MenuItem[];
  disabled?: boolean;
  density?: Densities;
}

interface ToolbarItemGroupProps {
  items: MenuItem[];
  onItemClick: (item: MenuItem) => void;
  disabled?: boolean;
  density?: Densities;
}

const ToolbarItemGroup: React.FC<ToolbarItemGroupProps> = ({
  items,
  onItemClick,
  disabled = false,
  density = Densities.Medium,
}) => {
  const separatorClass =
    density === Densities.Small
      ? "h-4 mx-0.5"
      : density === Densities.Large
        ? "h-8 mx-1.5"
        : "h-6 mx-1";
  const buttonSize =
    density === Densities.Small ? "sm" : density === Densities.Large ? "default" : "sm";
  const iconButtonSize =
    density === Densities.Small ? "icon-sm" : density === Densities.Large ? "icon" : "icon-sm";
  const iconDimension =
    density === Densities.Small ? "0.75rem" : density === Densities.Large ? "1.25rem" : "1rem";

  return items.map((item, i) => {
    // Handle group variant
    if (item.variant === "Group" && item.children) {
      const groupKey = item.label || `group-${i}`;
      return (
        <div
          key={groupKey}
          className="flex items-center gap-1"
          role="group"
          aria-label={item.label || "Toolbar group"}
        >
          <ToolbarItemGroup
            items={item.children}
            onItemClick={onItemClick}
            disabled={disabled || item.disabled}
            density={density}
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
          className={cn("w-px bg-border", separatorClass)}
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
        size={isIconOnly ? iconButtonSize : buttonSize}
        variant="ghost"
        onClick={() => !isDisabled && onItemClick(item)}
        disabled={isDisabled}
        className={cn(item.checked && "bg-accent")}
        tooltipText={item.tooltip}
      >
        {item.icon && (
          <Icon name={item.icon} style={{ width: iconDimension, height: iconDimension }} />
        )}
        {item.label && <span>{item.label}</span>}
      </ButtonWithTooltip>
    );
  });
};

export const ToolbarWidget: React.FC<ToolbarWidgetProps> = ({
  id,
  items = EMPTY_ITEMS,
  disabled = false,
  density = Densities.Medium,
}) => {
  const eventHandler = useEventHandler();

  const containerGapClass =
    density === Densities.Small
      ? "gap-1 p-1"
      : density === Densities.Large
        ? "gap-3 p-3"
        : "gap-2 p-2";

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
      aria-label="Toolbar"
      aria-disabled={disabled}
      className={cn(
        "flex items-center bg-background border rounded-md",
        containerGapClass,
        disabled && "opacity-50 pointer-events-none",
      )}
    >
      <ToolbarItemGroup
        items={items}
        onItemClick={onItemClick}
        disabled={disabled}
        density={density}
      />
    </div>
  );
};
