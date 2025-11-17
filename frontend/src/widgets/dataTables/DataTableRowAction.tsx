import React from 'react';
import Icon from '@/components/Icon';
import { MenuItem } from '@/types/widgets';
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
  actions: MenuItem[];
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
  onActionClick: (action: MenuItem) => void;
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

  const renderAction = (action: MenuItem) => {
    // Skip separator variants
    if (action.variant === 'Separator') {
      return null;
    }

    // Get action identifier (tag or label)
    const actionId = action.tag?.toString() || action.label || '';

    // If action has children, render as dropdown menu
    if (action.children && action.children.length > 0) {
      return (
        <DropdownMenu key={actionId}>
          <DropdownMenuTrigger asChild>
            <button
              className="flex items-center justify-center p-1.5 rounded bg-background hover:bg-[var(--color-muted)] transition-colors cursor-pointer border border-[var(--color-border)]"
              aria-label={action.label || actionId}
              type="button"
            >
              {action.icon && (
                <Icon
                  name={action.icon}
                  size={16}
                  className="text-[var(--color-foreground)]"
                />
              )}
            </button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            {action.children
              .filter(child => child.variant !== 'Separator')
              .map(childAction => {
                const childId =
                  childAction.tag?.toString() || childAction.label || '';
                return (
                  <DropdownMenuItem
                    key={childId}
                    onClick={() => onActionClick(childAction)}
                  >
                    {childAction.icon && (
                      <Icon
                        name={childAction.icon}
                        size={16}
                        className="mr-2 text-[var(--color-foreground)]"
                      />
                    )}
                    {childAction.label || childId}
                  </DropdownMenuItem>
                );
              })}
          </DropdownMenuContent>
        </DropdownMenu>
      );
    }

    // Otherwise, render as regular button
    return (
      <button
        key={actionId}
        className="flex items-center justify-center p-1.5 rounded bg-background hover:bg-[var(--color-muted)] transition-colors cursor-pointer border border-[var(--color-border)]"
        onClick={() => onActionClick(action)}
        aria-label={action.label || actionId}
        type="button"
      >
        {action.icon && (
          <Icon
            name={action.icon}
            size={16}
            className="text-[var(--color-foreground)]"
          />
        )}
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
