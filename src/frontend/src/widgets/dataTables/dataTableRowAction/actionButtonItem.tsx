import React from "react";
import { MenuItem } from "@/types/widgets";
import { ActionBaton } from "./actionBaton";

interface ActionBatonItemProps {
  action: MenuItem;
  actionId: string;
  onActionClick: (action: MenuItem) => void;
}

/**
 * Regular button action (no children)
 */
export const ActionBatonItem: React.FC<ActionBatonItemProps> = ({
  action,
  actionId,
  onActionClick,
}) => {
  return (
    <ActionBaton
      key={actionId}
      action={action}
      actionId={actionId}
      onClick={() => onActionClick(action)}
    />
  );
};
