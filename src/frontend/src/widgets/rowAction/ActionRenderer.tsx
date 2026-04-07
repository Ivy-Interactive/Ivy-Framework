import React from "react";
import { MenuItem } from "@/types/widgets";
import { ActionDropdown } from "./ActionDropdown";
import { ActionButtonItem } from "./ActionButtonItem";
import { getActionId, ActionRendererVariant } from "./utils";

interface ActionRendererProps {
  action: MenuItem;
  onActionClick: (action: MenuItem) => void;
  variant?: ActionRendererVariant;
}

/**
 * Renders a single action (either dropdown or button)
 */
export const ActionRenderer: React.FC<ActionRendererProps> = ({
  action,
  onActionClick,
  variant = "default",
}) => {
  // Skip separator variants
  if (action.variant === "Separator") {
    return null;
  }

  const actionId = getActionId(action);

  // Render as dropdown if action has children
  if (action.children && action.children.length > 0) {
    return (
      <ActionDropdown
        action={action}
        actionId={actionId}
        onActionClick={onActionClick}
        variant={variant}
      />
    );
  }

  // Otherwise, render as regular button
  return (
    <ActionButtonItem
      action={action}
      actionId={actionId}
      onActionClick={onActionClick}
      variant={variant}
    />
  );
};
