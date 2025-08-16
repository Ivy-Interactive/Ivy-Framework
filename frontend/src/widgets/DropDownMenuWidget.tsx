import { useEventHandler } from '@/components/event-handler';
import React, { useRef, useState } from 'react';

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
} from '@/components/ui/dropdown-menu';
import { MenuItem } from '@/types/widgets';
import Icon from '@/components/Icon';
import { camelCase } from '@/lib/utils';

interface DropDownMenuWidgetProps {
  id: string;
  items: MenuItem[];
  align?: 'Start' | 'Center' | 'End';
  side?: 'Top' | 'Right' | 'Bottom' | 'Left';
  sideOffset?: number;
  alignOffset?: number;
  slots?: {
    Trigger?: React.ReactNode[];
    Header?: React.ReactNode[];
  };
}

export const DropDownMenuWidget: React.FC<DropDownMenuWidgetProps> = ({
  slots,
  id,
  items,
  align = 'Start',
  side = 'Bottom',
  sideOffset = 8,
  alignOffset = 0,
}) => {
  const eventHandler = useEventHandler();
  const [open, setOpen] = useState(false);
  const triggerRef = useRef<HTMLButtonElement>(null);

  if (!slots?.Trigger) {
    return (
      <div className="text-red-500">
        Error: DropDownMenu requires Trigger slot.
      </div>
    );
  }

  const onItemClick = (item: MenuItem) => {
    if (!item.tag) return;
    eventHandler('OnSelect', id, [item.tag]);
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

  const renderMenuItems = (items: MenuItem[]) => {
    return items.map((item, index) => {
      // Handle group variant
      if (item.variant === 'Group' && item.children) {
        return (
          <React.Fragment key={`group-${index}`}>
            {item.label && <DropdownMenuLabel>{item.label}</DropdownMenuLabel>}
            <DropdownMenuGroup>
              {renderMenuItems(item.children)}
            </DropdownMenuGroup>
          </React.Fragment>
        );
      }

      // Handle separator variant
      if (item.variant === 'Separator') {
        return <DropdownMenuSeparator key={`separator-${index}`} />;
      }

      // Handle checkbox variant
      if (item.variant === 'Checkbox') {
        return (
          <DropdownMenuItem
            key={item.label}
            onClick={() => onItemClick(item)}
            disabled={item.disabled}
            className={item.checked ? 'bg-accent' : ''}
          >
            {item.icon && <Icon name={item.icon} size={14} />}
            {item.label}
            {item.checked && <span className="ml-auto">✓</span>}
            {item.shortcut && (
              <DropdownMenuShortcut>{item.shortcut}</DropdownMenuShortcut>
            )}
          </DropdownMenuItem>
        );
      }

      // Handle radio variant
      if (item.variant === 'Radio') {
        return (
          <DropdownMenuItem
            key={item.label}
            onClick={() => onItemClick(item)}
            disabled={item.disabled}
            className={item.checked ? 'bg-accent' : ''}
          >
            {item.icon && <Icon name={item.icon} size={14} />}
            {item.label}
            {item.checked && <span className="ml-auto">●</span>}
            {item.shortcut && (
              <DropdownMenuShortcut>{item.shortcut}</DropdownMenuShortcut>
            )}
          </DropdownMenuItem>
        );
      }

      // Handle submenu with children
      if (item.children && item.children.length > 0) {
        return (
          <DropdownMenuSub key={item.label}>
            <DropdownMenuSubTrigger disabled={item.disabled}>
              {item.icon && <Icon name={item.icon} size={14} />}
              {item.label}
              {item.shortcut && (
                <DropdownMenuShortcut>{item.shortcut}</DropdownMenuShortcut>
              )}
            </DropdownMenuSubTrigger>
            <DropdownMenuPortal>
              <DropdownMenuSubContent>
                {renderMenuItems(item.children)}
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
        >
          {item.icon && <Icon name={item.icon} size={14} />}
          {item.label}
          {item.shortcut && (
            <DropdownMenuShortcut>{item.shortcut}</DropdownMenuShortcut>
          )}
        </DropdownMenuItem>
      );
    });
  };

  return (
    <DropdownMenu open={open} onOpenChange={handleOpenChange}>
      <DropdownMenuTrigger ref={triggerRef} asChild>
        <div>{slots.Trigger}</div>
      </DropdownMenuTrigger>
      <DropdownMenuContent
        align={camelCase(align) as 'center' | 'end' | 'start' | undefined}
        side={
          camelCase(side) as 'top' | 'right' | 'bottom' | 'left' | undefined
        }
        sideOffset={sideOffset}
        alignOffset={alignOffset}
      >
        {slots.Header && <DropdownMenuLabel>{slots.Header}</DropdownMenuLabel>}
        {renderMenuItems(items)}
      </DropdownMenuContent>
    </DropdownMenu>
  );
};
