import { useEventHandler } from "@/components/event-handler";
import React, { useRef, useState } from "react";

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
import { camelCase } from "@/lib/utils";

const EMPTY_ARRAY: never[] = [];

interface DropDownMenuWidgetProps {
  id: string;
  items: MenuItem[];
  align?: "Start" | "Center" | "End";
  side?: "Top" | "Right" | "Bottom" | "Left";
  alignOffset?: number;
  slots?: {
    Trigger?: React.ReactNode[];
    Header?: React.ReactNode[];
  };
}

interface DropDownMenuItemGroupProps {
  items: MenuItem[];
  onItemClick: (item: MenuItem) => void;
}

const DropDownMenuItemGroup = ({ items, onItemClick }: DropDownMenuItemGroupProps) => {
  return items.map((item, i) => {
    // Handle group variant
    if (item.variant === "Group" && item.children) {
      const groupKey = item.label || `group-${i}`;
      return (
        <React.Fragment key={groupKey}>
          {item.label && <DropdownMenuLabel>{item.label}</DropdownMenuLabel>}
          <DropdownMenuGroup>
            <DropDownMenuItemGroup items={item.children} onItemClick={onItemClick} />
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
          disabled={item.disabled}
          variant={(item.color?.toLowerCase() as any) || "default"}
          className={item.checked ? "bg-accent" : ""}
        >
          {item.icon && <Icon name={item.icon} size={14} />}
          {item.label}
          {item.checked && <span className="ml-auto">✓</span>}
          {item.shortcut && <DropdownMenuShortcut>{item.shortcut}</DropdownMenuShortcut>}
        </DropdownMenuItem>
      );
    }

    if (item.variant === "Radio") {
      return (
        <DropdownMenuItem
          key={item.label}
          onClick={() => onItemClick(item)}
          disabled={item.disabled}
          variant={(item.color?.toLowerCase() as any) || "default"}
          className={item.checked ? "bg-accent" : ""}
        >
          {item.icon && <Icon name={item.icon} size={14} />}
          {item.label}
          {item.checked && <span className="ml-auto">●</span>}
          {item.shortcut && <DropdownMenuShortcut>{item.shortcut}</DropdownMenuShortcut>}
        </DropdownMenuItem>
      );
    }

    // Handle submenu with children
    if (item.children && item.children.length > 0) {
      return (
        <DropdownMenuSub key={item.label}>
          <DropdownMenuSubTrigger
            disabled={item.disabled}
            variant={(item.color?.toLowerCase() as any) || "default"}
          >
            {item.icon && <Icon name={item.icon} size={14} />}
            {item.label}
            {item.shortcut && <DropdownMenuShortcut>{item.shortcut}</DropdownMenuShortcut>}
          </DropdownMenuSubTrigger>
          <DropdownMenuPortal>
            <DropdownMenuSubContent className="m-2">
              <DropDownMenuItemGroup items={item.children} onItemClick={onItemClick} />
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
        disabled={item.disabled}
        variant={(item.color?.toLowerCase() as any) || "default"}
      >
        {item.icon && <Icon name={item.icon} size={14} />}
        {item.label}
        {item.shortcut && <DropdownMenuShortcut>{item.shortcut}</DropdownMenuShortcut>}
      </DropdownMenuItem>
    );
  });
};

export const DropDownMenuWidget: React.FC<DropDownMenuWidgetProps> = ({
  slots,
  id,
  items = EMPTY_ARRAY,
  align = "Start",
  side = "Bottom",
  alignOffset = 0,
}) => {
  const eventHandler = useEventHandler();
  const [open, setOpen] = useState(false);
  const triggerRef = useRef<HTMLButtonElement>(null);

  if (!slots?.Trigger) {
    return <div className="text-red-500">Error: DropDownMenu requires Trigger slot.</div>;
  }

  const onItemClick = (item: MenuItem) => {
    if (!item.tag) return;
    eventHandler("OnSelect", id, [item.tag]);
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
    <DropdownMenu open={open} onOpenChange={handleOpenChange}>
      <DropdownMenuTrigger ref={triggerRef} asChild>
        <div>{slots.Trigger}</div>
      </DropdownMenuTrigger>
      <DropdownMenuContent
        onClick={(e) => e.stopPropagation()}
        align={camelCase(align) as "center" | "end" | "start" | undefined}
        side={camelCase(side) as "top" | "right" | "bottom" | "left" | undefined}
        className="m-2"
        alignOffset={alignOffset}
      >
        {slots.Header && <DropdownMenuLabel>{slots.Header}</DropdownMenuLabel>}
        <DropDownMenuItemGroup items={items} onItemClick={onItemClick} />
      </DropdownMenuContent>
    </DropdownMenu>
  );
};
