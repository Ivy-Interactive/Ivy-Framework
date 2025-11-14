import React from 'react';
import Icon from '@/components/Icon';
import { RowAction } from './types/types';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

interface RowActionButtonsProps {
  /**
   * Array of action configurations
   */
  actions: RowAction[];
  /**
   * Y position of the button group (should center within row)
   */
  top: number;
  /**
   * Whether buttons are visible
   */
  visible: boolean;
  /**
   * Click handler for action buttons
   */
  onActionClick: (action: RowAction) => void;
  /**
   * Mouse enter handler to prevent losing hover state
   */
  onMouseEnter?: () => void;
  /**
   * Mouse leave handler
   */
  onMouseLeave?: () => void;
}

/**
 * Row action buttons that appear on hover at the right edge of the data table
 */
export const RowActionButtons: React.FC<RowActionButtonsProps> = ({
  actions,
  top,
  visible,
  onActionClick,
  onMouseEnter,
  onMouseLeave,
}) => {
  if (!visible || actions.length === 0) return null;

  const renderAction = (action: RowAction) => {
    // If action has children, render as dropdown menu
    if (action.children && action.children.length > 0) {
      return (
        <DropdownMenu key={action.id}>
          <DropdownMenuTrigger asChild>
            <button
              className="flex items-center justify-center p-1.5 rounded bg-background hover:bg-[var(--color-muted)] transition-colors cursor-pointer border border-[var(--color-border)]"
              aria-label={action.tooltip || action.eventName}
              type="button"
            >
              <Icon
                name={action.icon}
                size={16}
                className="text-[var(--color-foreground)]"
              />
            </button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            {action.children.map(childAction => (
              <DropdownMenuItem
                key={childAction.id}
                onClick={() => onActionClick(childAction)}
              >
                {childAction.icon && (
                  <Icon
                    name={childAction.icon}
                    size={16}
                    className="mr-2 text-[var(--color-foreground)] "
                  />
                )}
                {childAction.label || childAction.id}
              </DropdownMenuItem>
            ))}
          </DropdownMenuContent>
        </DropdownMenu>
      );
    }

    // Otherwise, render as regular button
    return (
      <button
        key={action.id}
        className="flex items-center  justify-center p-1.5 rounded bg-background hover:bg-[var(--color-muted)] transition-colors cursor-pointer border border-[var(--color-border)]"
        onClick={() => onActionClick(action)}
        aria-label={action.tooltip || action.eventName}
        type="button"
      >
        <Icon
          name={action.icon}
          size={16}
          className="text-[var(--color-foreground)]"
        />
      </button>
    );
  };

  return (
    <div
      className="absolute z-50 flex flex-row gap-1"
      style={{
        top: `${top}px`,
        right: '8px',
        opacity: visible ? 1 : 0,
        pointerEvents: visible ? 'auto' : 'none',
      }}
      onMouseEnter={onMouseEnter}
      onMouseLeave={onMouseLeave}
    >
      {actions.map(renderAction)}
    </div>
  );
};
