import React from 'react';
import Icon from '@/components/Icon';
import { MenuItem } from '@/types/widgets';
import { ACTION_BUTTON_CLASSES } from './utils';

interface ActionButtonProps {
  action: MenuItem;
  actionId: string;
  onClick?: () => void;
}

/**
 * Action button component used in both dropdown triggers and regular buttons
 */
export const ActionButton: React.FC<ActionButtonProps> = ({
  action,
  actionId,
  onClick,
}) => {
  return (
    <button
      className={ACTION_BUTTON_CLASSES}
      onClick={onClick}
      aria-label={action.label || actionId}
      type="button"
    >
      {action.icon && (
        <Icon
          name={action.icon}
          size={16}
          className="text-(--color-foreground)"
        />
      )}
    </button>
  );
};
