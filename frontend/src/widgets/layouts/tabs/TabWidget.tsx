import React from 'react';
import type { TabWidgetProps } from './types';

/**
 * TabWidget component - simple wrapper for tab content
 * Ensures tab content takes full height of the container
 */
export const TabWidget: React.FC<TabWidgetProps> = ({ children }) => {
  return <div className="h-full">{children}</div>;
};

TabWidget.displayName = 'TabWidget';
