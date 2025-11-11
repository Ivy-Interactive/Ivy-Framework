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
  /**
   * Mouse enter handler to prevent losing hover state
   */
  onMouseEnter?: () => void;
  /**
   * Mouse leave handler
   */
  onMouseLeave?: () => void;
}

/**
 * Row action buttons that appear on hover at the right edge of the data table
 */
export const RowActionButtons: React.FC<RowActionButtonsProps> = ({
  actions,
  top,
  visible,
  onActionClick,
  onMouseEnter,
  onMouseLeave,
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
      onMouseEnter={onMouseEnter}
      onMouseLeave={onMouseLeave}
    >
      {actions.map(action => (
        <button
          key={action.id}
          className="flex items-center justify-center p-1 rounded bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 transition-colors cursor-pointer"
          onClick={() => onActionClick(action)}
          aria-label={action.eventName}
          type="button"
        >
          <Icon
            name={action.icon}
            size={16}
            className="text-[#606664] dark:text-gray-300"
          />
        </button>
      ))}
    </div>
  );
};
