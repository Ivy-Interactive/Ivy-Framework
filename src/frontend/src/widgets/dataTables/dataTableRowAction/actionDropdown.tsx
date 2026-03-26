import React from "react";
import Icon from "@/components/Icon";
import { MenuItem } from "@/types/widgets";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { getActionId, ACTION_BUTTON_CLASSES } from "./utils";

interface ActionDropdownProps {
  action: MenuItem;
  actionId: string;
  onActionClick: (action: MenuItem) => void;
}

interface TriggerButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  action: MenuItem;
  actionId: string;
}

/**
 * Custom trigger button that stops propagation but allows dropdown to work
 */
const TriggerButton = React.forwardRef<HTMLButtonElement, TriggerButtonProps>(
  ({ action, actionId, ...props }, ref) => {
    const handleMouseDown = (e: React.MouseEvent<HTMLButtonElement>) => {
      // Stop propagation to prevent grid interactions
      e.stopPropagation();
      props.onMouseDown?.(e);
    };

    return (
      <button
        ref={ref}
        className={ACTION_BUTTON_CLASSES}
        {...props}
        onMouseDown={handleMouseDown}
        aria-label={action.label || actionId}
        title={action.tooltip}
        type="button"
      >
        {action.icon && <Icon name={action.icon} size={16} className="text-(--color-foreground)" />}
      </button>
    );
  },
);
TriggerButton.displayName = "TriggerButton";

/**
 * Dropdown menu action with nested children
 */
export const ActionDropdown: React.FC<ActionDropdownProps> = ({
  action,
  actionId,
  onActionClick,
}) => {
  const validChildren = action.children?.filter((child) => child.variant !== "Separator") || [];

  return (
    <DropdownMenu key={actionId}>
      <DropdownMenuTrigger asChild>
        <TriggerButton action={action} actionId={actionId} />
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" onCloseAutoFocus={(e) => e.preventDefault()}>
        {validChildren.map((childAction) => {
          const childId = getActionId(childAction);
          const isDestructive = childAction.color === "Destructive";
          return (
            <DropdownMenuItem
              key={childId}
              className={isDestructive ? "text-destructive focus:text-destructive" : undefined}
              onClick={(e) => {
                e.stopPropagation();
                onActionClick(childAction);
              }}
            >
              {childAction.icon && (
                <Icon
                  name={childAction.icon}
                  size={16}
                  className={
                    isDestructive ? "mr-2 text-destructive" : "mr-2 text-(--color-foreground)"
                  }
                />
              )}
              {childAction.label || childId}
            </DropdownMenuItem>
          );
        })}
      </DropdownMenuContent>
    </DropdownMenu>
  );
};
