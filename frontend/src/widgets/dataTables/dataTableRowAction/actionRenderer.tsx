import React from 'react';
import { MenuItem } from '@/types/widgets';
import { ActionDropdown } from './actionDropdown';
import { ActionButtonItem } from './actionButtonItem';
import { getActionId } from './utils';

interface ActionRendererProps {
  action: MenuItem;
  onActionClick: (action: MenuItem) => void;
}

/**
 * Renders a single action (either dropdown or button)
 */
export const ActionRenderer: React.FC<ActionRendererProps> = ({
  action,
  onActionClick,
}) => {
  // Skip separator variants
  if (action.variant === 'Separator') {
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
      />
    );
  }

  // Otherwise, render as regular button
  return (
    <ActionButtonItem
      action={action}
      actionId={actionId}
      onActionClick={onActionClick}
    />
  );
};
