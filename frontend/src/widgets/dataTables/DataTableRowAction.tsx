import React from 'react';
import Icon from '@/components/Icon';
import { RowAction } from './types/types';

interface RowActionButtonsProps {
  /**
   * Array of action configurations
   */
  actions: RowAction[];
  /**
   * Y position of the button group (should center within row)
   */
  top: number;
  /**
   * Whether buttons are visible
   */
  visible: boolean;
  /**
   * Click handler for action buttons
   */
  onActionClick: (action: RowAction) => void;
}

/**
 * Row action buttons that appear on hover at the right edge of the data table
 */
export const RowActionButtons: React.FC<RowActionButtonsProps> = ({
  actions,
  top,
  visible,
  onActionClick,
}) => {
  if (!visible || actions.length === 0) return null;

  return (
    <div
      className="absolute z-50 flex flex-row gap-1"
      style={{
        top: `${top}px`,
        right: '8px',
        opacity: visible ? 1 : 0,
        pointerEvents: visible ? 'auto' : 'none',
      }}
    >
      {actions.map(action => (
        <button
          key={action.id}
          className="flex items-center justify-center p-1 rounded transition-colors cursor-pointer"
          style={{
            backgroundColor: 'var(--accent)',
            color: 'var(--muted-foreground)',
          }}
          onMouseEnter={e => {
            e.currentTarget.style.backgroundColor = 'var(--muted-foreground)';
            e.currentTarget.style.color = 'var(--accent)';
          }}
          onMouseLeave={e => {
            e.currentTarget.style.backgroundColor = 'var(--accent)';
          }}
          onClick={() => onActionClick(action)}
          aria-label={action.eventName}
          type="button"
        >
          <Icon name={action.icon} size={16} />
        </button>
      ))}
    </div>
  );
};
