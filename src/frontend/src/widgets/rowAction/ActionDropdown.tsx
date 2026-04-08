import React from "react";
import Icon from "@/components/Icon";
import { MenuItem } from "@/types/widgets";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { getActionId, getActionButtonClasses, ActionRendererVariant } from "./utils";
import { getColor } from "@/lib/styles";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";

interface ActionDropdownProps {
  action: MenuItem;
  actionId: string;
  onActionClick: (action: MenuItem) => void;
  variant?: ActionRendererVariant;
}

interface TriggerButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  action: MenuItem;
  actionId: string;
  variant?: ActionRendererVariant;
}

/**
 * Custom trigger button that stops propagation but allows dropdown to work
 */
const TriggerButton = React.forwardRef<HTMLButtonElement, TriggerButtonProps>(
  ({ action, actionId, variant = "default", ...props }, ref) => {
    const handleMouseDown = (e: React.MouseEvent<HTMLButtonElement>) => {
      e.stopPropagation();
      props.onMouseDown?.(e);
    };

    const colorStyle = action.color ? getColor(action.color, "color") : {};

    return (
      <button
        ref={ref}
        className={getActionButtonClasses(variant)}
        {...props}
        onMouseDown={handleMouseDown}
        aria-label={action.label || actionId}
        type="button"
      >
        {action.icon && <Icon name={action.icon} size={16} style={colorStyle} />}
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
  variant = "default",
}) => {
  const validChildren = action.children?.filter((child) => child.variant !== "Separator") || [];

  return (
    <TooltipProvider>
      <Tooltip>
        <DropdownMenu key={actionId}>
          <TooltipTrigger asChild>
            <DropdownMenuTrigger asChild>
              <TriggerButton action={action} actionId={actionId} variant={variant} />
            </DropdownMenuTrigger>
          </TooltipTrigger>
          {action.tooltip && (
            <TooltipContent className="bg-popover text-popover-foreground shadow-md">
              {action.tooltip}
            </TooltipContent>
          )}
          <DropdownMenuContent align="end" onCloseAutoFocus={(e) => e.preventDefault()}>
            {validChildren.map((childAction) => {
              const childId = getActionId(childAction);
              const colorStyle = childAction.color ? getColor(childAction.color, "color") : {};

              return (
                <DropdownMenuItem
                  key={childId}
                  style={colorStyle}
                  onClick={(e) => {
                    e.stopPropagation();
                    onActionClick(childAction);
                  }}
                >
                  {childAction.icon && (
                    <Icon name={childAction.icon} size={16} className="mr-2" style={colorStyle} />
                  )}
                  {childAction.label || childId}
                </DropdownMenuItem>
              );
            })}
          </DropdownMenuContent>
        </DropdownMenu>
      </Tooltip>
    </TooltipProvider>
  );
};
