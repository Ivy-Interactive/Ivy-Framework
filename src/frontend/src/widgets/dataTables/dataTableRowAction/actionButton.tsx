import React, { forwardRef } from "react";
import Icon from "@/components/Icon";
import { MenuItem } from "@/types/widgets";
import { ACTION_BUTTON_CLASSES } from "./utils";
import withTooltip from "@/hoc/withTooltip";
import { getColor } from "@/lib/styles";

const NativeButton = forwardRef<HTMLButtonElement, React.ButtonHTMLAttributes<HTMLButtonElement>>(
  (props, ref) => <button ref={ref} {...props} />,
);
NativeButton.displayName = "NativeButton";

const ButtonWithTooltip = withTooltip(NativeButton);

interface ActionButtonProps {
  action: MenuItem;
  actionId: string;
  onClick?: () => void;
}

/**
 * Action button component used in both dropdown triggers and regular buttons
 */
export const ActionButton: React.FC<ActionButtonProps> = ({ action, actionId, onClick }) => {
  const handleClick = (e: React.MouseEvent<HTMLButtonElement>) => {
    // Always stop propagation to prevent grid interactions
    e.stopPropagation();
    // Only call onClick if provided (for regular buttons)
    // When used as dropdown trigger, onClick is undefined and trigger handles it
    onClick?.();
  };

  const handleMouseDown = (e: React.MouseEvent<HTMLButtonElement>) => {
    // Always stop propagation to prevent grid from handling mousedown
    e.stopPropagation();
  };

  const getColorStyle = (color: MenuItem["color"]) => (color ? getColor(color, "color") : {});

  return (
    <ButtonWithTooltip
      className={ACTION_BUTTON_CLASSES}
      onClick={handleClick}
      onMouseDown={handleMouseDown}
      aria-label={action.label || actionId}
      tooltipText={action.tooltip}
      type="button"
    >
      {action.icon && <Icon name={action.icon} size={16} style={getColorStyle(action.color)} />}
    </ButtonWithTooltip>
  );
};
