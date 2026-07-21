import { useEventHandler } from "@/components/event-handler";
import React, { useRef, useState } from "react";
import { X } from "lucide-react";

import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuPortal,
  DropdownMenuSeparator,
  DropdownMenuShortcut,
  DropdownMenuSub,
  DropdownMenuSubContent,
  DropdownMenuSubTrigger,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { MenuItem } from "@/types/widgets";
import Icon from "@/components/Icon";
import { camelCase, cn } from "@/lib/utils";
import { getColor, getWidth } from "@/lib/styles";
import { ShortcutKeys } from "@/components/Kbd";
import { Densities } from "@/types/density";

const EMPTY_ARRAY: never[] = [];

interface DropDownMenuWidgetProps {
  id: string;
  items: MenuItem[];
  align?: "Start" | "Center" | "End";
  side?: "Top" | "Right" | "Bottom" | "Left";
  alignOffset?: number;
  stayOpen?: boolean;
  density?: Densities;
  width?: string;
  events?: string[];
  slots?: {
    Trigger?: React.ReactNode[];
    Header?: React.ReactNode[];
  };
}

interface DropDownMenuItemGroupProps {
  items: MenuItem[];
  onItemClick: (item: MenuItem) => void;
  iconSize: number;
  stayOpen?: boolean;
}

const DropDownMenuItemGroup = ({
  items,
  onItemClick,
  iconSize,
  stayOpen,
}: DropDownMenuItemGroupProps) => {
  return items.map((item, i) => {
    const colorStyle = item.color ? getColor(item.color, "color") : {};

    // Handle group variant
    if (item.variant === "Group" && item.children) {
      const groupKey = item.label || `group-${i}`;
      return (
        <React.Fragment key={groupKey}>
          {item.label && <DropdownMenuLabel>{item.label}</DropdownMenuLabel>}
          <DropdownMenuGroup>
            <DropDownMenuItemGroup
              items={item.children}
              onItemClick={onItemClick}
              iconSize={iconSize}
              stayOpen={stayOpen}
            />
          </DropdownMenuGroup>
        </React.Fragment>
      );
    }

    // Handle separator variant
    if (item.variant === "Separator") {
      const sepKey = `separator-${i}`;
      return <DropdownMenuSeparator key={sepKey} />;
    }

    if (item.variant === "Checkbox") {
      return (
        <DropdownMenuItem
          key={item.label}
          onClick={() => onItemClick(item)}
          onSelect={stayOpen ? (e) => e.preventDefault() : undefined}
          disabled={item.disabled}
          style={colorStyle}
          className={item.checked ? "bg-accent" : ""}
        >
          {item.icon && <Icon name={item.icon} size={iconSize} style={colorStyle} />}
          {item.label}
          {item.checked &&
            (stayOpen ? (
              <X className="ml-auto size-4 shrink-0 text-muted-foreground" />
            ) : (
              <span className="ml-auto">✓</span>
            ))}
          {item.shortcut && (
            <DropdownMenuShortcut>
              <ShortcutKeys shortcut={item.shortcut} />
            </DropdownMenuShortcut>
          )}
        </DropdownMenuItem>
      );
    }

    if (item.variant === "Radio") {
      return (
        <DropdownMenuItem
          key={item.label}
          onClick={() => onItemClick(item)}
          onSelect={stayOpen ? (e) => e.preventDefault() : undefined}
          disabled={item.disabled}
          style={colorStyle}
          className={item.checked ? "bg-accent" : ""}
        >
          {item.icon && <Icon name={item.icon} size={iconSize} style={colorStyle} />}
          {item.label}
          {item.checked && <span className="ml-auto">●</span>}
          {item.shortcut && (
            <DropdownMenuShortcut>
              <ShortcutKeys shortcut={item.shortcut} />
            </DropdownMenuShortcut>
          )}
        </DropdownMenuItem>
      );
    }

    // Handle submenu with children
    if (item.children && item.children.length > 0) {
      return (
        <DropdownMenuSub key={item.label}>
          <DropdownMenuSubTrigger disabled={item.disabled} style={colorStyle}>
            {item.icon && <Icon name={item.icon} size={iconSize} style={colorStyle} />}
            {item.label}
            {item.shortcut && (
              <DropdownMenuShortcut>
                <ShortcutKeys shortcut={item.shortcut} />
              </DropdownMenuShortcut>
            )}
          </DropdownMenuSubTrigger>
          <DropdownMenuPortal>
            <DropdownMenuSubContent className="m-2">
              <DropDownMenuItemGroup
                items={item.children}
                onItemClick={onItemClick}
                iconSize={iconSize}
                stayOpen={stayOpen}
              />
            </DropdownMenuSubContent>
          </DropdownMenuPortal>
        </DropdownMenuSub>
      );
    }

    // Default menu item
    return (
      <DropdownMenuItem
        key={item.label}
        onClick={() => onItemClick(item)}
        onSelect={stayOpen ? (e) => e.preventDefault() : undefined}
        disabled={item.disabled}
        style={colorStyle}
      >
        {item.icon && <Icon name={item.icon} size={iconSize} style={colorStyle} />}
        {item.label}
        {item.shortcut && (
          <DropdownMenuShortcut>
            <ShortcutKeys shortcut={item.shortcut} />
          </DropdownMenuShortcut>
        )}
      </DropdownMenuItem>
    );
  });
};

const EMPTY_EVENTS: string[] = [];

export const DropDownMenuWidget: React.FC<DropDownMenuWidgetProps> = ({
  slots,
  id,
  items = EMPTY_ARRAY,
  align = "Start",
  side = "Bottom",
  alignOffset = 0,
  stayOpen = false,
  density = Densities.Medium,
  width,
  events = EMPTY_EVENTS,
}) => {
  const eventHandler = useEventHandler();
  const [open, setOpen] = useState(false);
  const triggerRef = useRef<HTMLButtonElement>(null);

  const iconSize = density === Densities.Small ? 12 : density === Densities.Large ? 16 : 14;
  const contentMarginClass =
    density === Densities.Small ? "m-1" : density === Densities.Large ? "m-3" : "m-2";
  const widthStyles = getWidth(width);

  if (!slots?.Trigger) {
    return <div className="text-red-500">Error: DropDownMenu requires Trigger slot.</div>;
  }

  const onItemClick = (item: MenuItem) => {
    if (!item.tag) return;
    if (events.includes("OnSelect")) eventHandler("OnSelect", id, [item.tag]);
  };

  const handleOpenChange = (isOpen: boolean) => {
    setOpen(isOpen);

    // When dropdown closes, blur the trigger after a short delay
    // to remove the focus ring
    if (!isOpen) {
      setTimeout(() => {
        triggerRef.current?.blur();
      }, 10);
    }
  };

  return (
    <div style={widthStyles} className={cn(width && "min-w-0")}>
      <DropdownMenu open={open} onOpenChange={handleOpenChange}>
        <DropdownMenuTrigger ref={triggerRef} asChild>
          <div style={{ maxWidth: "100%", minWidth: 0, width: width ? "100%" : undefined }}>
            {slots.Trigger}
          </div>
        </DropdownMenuTrigger>
        <DropdownMenuContent
          onClick={(e) => e.stopPropagation()}
          align={camelCase(align) as "center" | "end" | "start" | undefined}
          side={camelCase(side) as "top" | "right" | "bottom" | "left" | undefined}
          className={contentMarginClass}
          alignOffset={alignOffset}
        >
          {slots.Header && <DropdownMenuLabel>{slots.Header}</DropdownMenuLabel>}
          <DropDownMenuItemGroup
            items={items}
            onItemClick={onItemClick}
            iconSize={iconSize}
            stayOpen={stayOpen}
          />
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
};
