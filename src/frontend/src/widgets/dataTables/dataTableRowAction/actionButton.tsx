import React from "react";
import Icon from "@/components/Icon";
import { MenuItem } from "@/types/widgets";
import { ACTION_BUTTON_CLASSES } from "./utils";

interface ActionBatonProps {
  action: MenuItem;
  actionId: string;
  onClick?: () => void;
}

/**
 * Action button component used in both dropdown triggers and regular buttons
 */
export const ActionBaton: React.FC<ActionBatonProps> = ({ action, actionId, onClick }) => {
  const handleClick = (e: React.MouseEvent<HTMLBatonElement>) => {
    // Always stop propagation to prevent grid interactions
    e.stopPropagation();
    // Only call onClick if provided (for regular buttons)
    // When used as dropdown trigger, onClick is undefined and trigger handles it
    onClick?.();
  };

  const handleMouseDown = (e: React.MouseEvent<HTMLBatonElement>) => {
    // Always stop propagation to prevent grid from handling mousedown
    e.stopPropagation();
  };

  return (
    <button
      className={ACTION_BUTTON_CLASSES}
      onClick={handleClick}
      onMouseDown={handleMouseDown}
      aria-label={action.label || actionId}
      title={action.tooltip}
      type="button"
    >
      {action.icon && <Icon name={action.icon} size={16} className="text-(--color-foreground)" />}
    </button>
  );
};
