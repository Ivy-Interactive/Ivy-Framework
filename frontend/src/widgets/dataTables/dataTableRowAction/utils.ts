import { MenuItem } from '@/types/widgets';

/**
 * Gets a unique identifier for an action (tag or label)
 */
export const getActionId = (action: MenuItem): string => {
  return action.tag?.toString() || action.label || '';
};

/**
 * Shared button styles for action buttons
 */
export const ACTION_BUTTON_CLASSES =
  'flex items-center justify-center p-1.5 rounded bg-background hover:bg-(--color-muted) transition-colors cursor-pointer border border-[var(--color-border)]';
