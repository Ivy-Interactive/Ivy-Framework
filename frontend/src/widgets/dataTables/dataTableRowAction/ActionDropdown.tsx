import React from 'react';
import Icon from '@/components/Icon';
import { MenuItem } from '@/types/widgets';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { ActionButton } from './actionButton';
import { getActionId } from './utils';

interface ActionDropdownProps {
  action: MenuItem;
  actionId: string;
  onActionClick: (action: MenuItem) => void;
}

/**
 * Dropdown menu action with nested children
 */
export const ActionDropdown: React.FC<ActionDropdownProps> = ({
  action,
  actionId,
  onActionClick,
}) => {
  const validChildren =
    action.children?.filter(child => child.variant !== 'Separator') || [];

  return (
    <DropdownMenu key={actionId}>
      <DropdownMenuTrigger asChild>
        <ActionButton action={action} actionId={actionId} />
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        {validChildren.map(childAction => {
          const childId = getActionId(childAction);
          return (
            <DropdownMenuItem
              key={childId}
              onClick={() => onActionClick(childAction)}
            >
              {childAction.icon && (
                <Icon
                  name={childAction.icon}
                  size={16}
                  className="mr-2 text-(--color-foreground)"
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
