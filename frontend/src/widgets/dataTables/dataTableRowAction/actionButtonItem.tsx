import React from 'react';
import { MenuItem } from '@/types/widgets';
import { ActionButton } from './actionButton';

interface ActionButtonItemProps {
  action: MenuItem;
  actionId: string;
  onActionClick: (action: MenuItem) => void;
}

/**
 * Regular button action (no children)
 */
export const ActionButtonItem: React.FC<ActionButtonItemProps> = ({
  action,
  actionId,
  onActionClick,
}) => {
  return (
    <ActionButton
      key={actionId}
      action={action}
      actionId={actionId}
      onClick={() => onActionClick(action)}
    />
  );
};
