import React from "react";
import { MenuItem } from "@/types/widgets";
import { ActionButton } from "./ActionButton";
import { ActionRendererVariant } from "./utils";

interface ActionButtonItemProps {
  action: MenuItem;
  actionId: string;
  onActionClick: (action: MenuItem) => void;
  variant?: ActionRendererVariant;
}

/**
 * Regular button action (no children)
 */
export const ActionButtonItem: React.FC<ActionButtonItemProps> = ({
  action,
  actionId,
  onActionClick,
  variant = "default",
}) => {
  return (
    <ActionButton
      key={actionId}
      action={action}
      actionId={actionId}
      onClick={() => onActionClick(action)}
      variant={variant}
    />
  );
};
