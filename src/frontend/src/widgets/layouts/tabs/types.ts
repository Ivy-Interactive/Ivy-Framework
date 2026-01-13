import React from 'react';

/**
 * Props for the TabWidget component
 */
export interface TabWidgetProps {
  children: React.ReactNode[];
  title: string;
  id: string;
  badge?: string;
  icon?: string;
}

/**
 * Props for the TabsLayoutWidget component
 */
export interface TabsLayoutWidgetProps {
  id: string;
  variant?: 'Tabs' | 'Content';
  removeParentPadding?: boolean;
  selectedIndex: number;
  children: React.ReactElement<TabWidgetProps>[];
  events: string[];
  padding?: string;
  width?: string;
  addButtonText?: string;
}

/**
 * Props for the SortableTabTrigger component
 */
export interface SortableTabTriggerProps {
  id: string;
  value: string;
  onClick: () => void;
  onMouseDown: (e: React.MouseEvent) => void;
  className?: string;
  onMouseEnter?: () => void;
  onMouseLeave?: () => void;
  children: React.ReactNode;
  useRadix?: boolean;
}

/**
 * Props for the SortableDropdownMenuItem component
 */
export interface SortableDropdownMenuItemProps {
  id: string;
  children: React.ReactNode;
  onClick: () => void;
  isActive?: boolean;
  showClose?: boolean;
}
