import React from 'react';
import type { TabWidgetProps } from '../types';

// ====================
// Tab Filtering & Ordering
// ====================

/**
 * Filters children to only include valid TabWidget elements
 * @param children - React children to filter
 * @returns Array of valid TabWidget elements
 */
export function filterTabWidgets(
  children: React.ReactNode[]
): React.ReactElement<TabWidgetProps>[] {
  return React.Children.toArray(children).filter(
    child =>
      React.isValidElement(child) &&
      (child.type as React.ComponentType<TabWidgetProps>)?.displayName ===
        'TabWidget'
  ) as React.ReactElement<TabWidgetProps>[];
}

/**
 * Orders tab widgets according to the specified tab order
 * @param tabOrder - Array of tab IDs in desired order
 * @param tabWidgets - Array of tab widget elements
 * @returns Ordered array of tab widgets (excluding any that don't match)
 */
export function orderTabWidgets(
  tabOrder: string[],
  tabWidgets: React.ReactElement<TabWidgetProps>[]
): React.ReactElement<TabWidgetProps>[] {
  return tabOrder
    .map(id =>
      tabWidgets.find(
        tab => (tab as React.ReactElement<TabWidgetProps>).props.id === id
      )
    )
    .filter(Boolean) as React.ReactElement<TabWidgetProps>[];
}

/**
 * Extracts tab IDs from an array of tab widgets
 * @param tabWidgets - Array of tab widget elements
 * @returns Array of tab IDs
 */
export function extractTabIds(
  tabWidgets: React.ReactElement<TabWidgetProps>[]
): string[] {
  return tabWidgets.map(
    tab => (tab as React.ReactElement<TabWidgetProps>).props.id
  );
}

/**
 * Swaps two tabs in the tab order array
 * @param tabOrder - Current tab order
 * @param tabId1 - First tab ID
 * @param tabId2 - Second tab ID
 * @returns New tab order with swapped tabs, or original if tabs not found
 */
export function swapTabsInOrder(
  tabOrder: string[],
  tabId1: string,
  tabId2: string
): string[] {
  const index1 = tabOrder.indexOf(tabId1);
  const index2 = tabOrder.indexOf(tabId2);

  // If either tab not found, return original order
  if (index1 === -1 || index2 === -1) {
    return tabOrder;
  }

  // Create new array and swap
  const newOrder = [...tabOrder];
  newOrder[index1] = tabId2;
  newOrder[index2] = tabId1;

  return newOrder;
}

/**
 * Creates reorder mapping for backend sync
 * @param newOrder - New tab order
 * @param originalOrder - Original tab order
 * @returns Array of indices mapping new positions to original positions
 */
export function createReorderMapping(
  newOrder: string[],
  originalOrder: string[]
): number[] {
  return newOrder.map(id => originalOrder.indexOf(id));
}

// ====================
// Tab Width Calculations
// ====================

/**
 * Estimates the width of a tab based on its properties
 * @param title - Tab title text
 * @param icon - Icon name (if present)
 * @param badge - Badge text (if present)
 * @param hasButtons - Whether the tab has action buttons (close/refresh)
 * @param variant - Tab variant ('Tabs' or 'Content')
 * @returns Estimated width in pixels
 */
export function estimateTabWidth(
  title: string,
  icon: string | undefined,
  badge: string | undefined,
  hasButtons: boolean,
  variant: 'Tabs' | 'Content'
): number {
  const avgCharWidth = 7.5; // Average character width in typical fonts
  let estimatedWidth = 0;

  if (variant === 'Tabs') {
    // Base padding (px-3 = 12px * 2)
    estimatedWidth += 24;

    // Icon width (16px) + gap (6px)
    if (icon) {
      estimatedWidth += 22;
    }

    // Title width
    estimatedWidth += Math.ceil(title.length * avgCharWidth);

    // Badge width
    if (badge) {
      const badgeWidth = Math.max(24, 16 + badge.length * avgCharWidth);
      estimatedWidth += 8 + badgeWidth; // ml-2 (8px) + badge
    }

    // Buttons width (refresh + close)
    if (hasButtons) {
      estimatedWidth += 8 + 24; // ml-2 (8px) + button (24px)
    }

    // Border and gap between tabs
    estimatedWidth += 3;
  } else {
    // Content variant
    // Base padding (px-1.5 = 6px * 2)
    estimatedWidth += 24;

    // Title width
    estimatedWidth += Math.ceil(title.length * avgCharWidth);

    // Gap between tabs
    estimatedWidth += 6;
  }

  return Math.ceil(estimatedWidth);
}

/**
 * Calculates button count for a tab based on events
 * @param events - Array of event names
 * @returns Number of buttons (0-2)
 */
export function calculateButtonCount(events: string[]): number {
  let count = 0;
  if (events.includes('OnClose')) count++;
  if (events.includes('OnRefresh')) count++;
  return count;
}

/**
 * Measures actual tab widths from DOM elements
 * @param tabsList - The tabs list DOM element
 * @returns Map of tab IDs to their measured widths
 */
export function measureTabsFromDOM(tabsList: HTMLElement): Map<string, number> {
  const measurements = new Map<string, number>();
  const allTabElements = tabsList.querySelectorAll('[role="tab"]');

  allTabElements.forEach(element => {
    const tabId = element.getAttribute('value');
    if (tabId && element instanceof HTMLElement) {
      const rect = element.getBoundingClientRect();
      const styles = window.getComputedStyle(element);
      const marginLeft = parseFloat(styles.marginLeft) || 0;
      const marginRight = parseFloat(styles.marginRight) || 0;
      const totalWidth = Math.ceil(rect.width + marginLeft + marginRight);
      measurements.set(tabId, totalWidth);
    }
  });

  return measurements;
}

/**
 * Estimates tab widths for tabs not yet rendered in the DOM
 * @param tabOrder - Array of tab IDs
 * @param tabWidgets - Array of tab widget elements
 * @param measurements - Existing measurements from DOM
 * @param events - Array of event names
 * @param variant - Tab variant
 * @returns Map of tab IDs to estimated widths
 */
export function estimateUnrenderedTabWidths(
  tabOrder: string[],
  tabWidgets: React.ReactElement<TabWidgetProps>[],
  measurements: Map<string, number>,
  events: string[],
  variant: 'Tabs' | 'Content'
): Map<string, number> {
  const estimatedWidths = new Map<string, number>();
  const hasButtons = events.includes('OnClose') || events.includes('OnRefresh');

  for (const tabId of tabOrder) {
    // Skip if we already have a measurement
    if (measurements.has(tabId)) continue;

    const tabWidget = tabWidgets.find(
      tab => (tab as React.ReactElement<TabWidgetProps>).props.id === tabId
    );
    if (!tabWidget || !React.isValidElement(tabWidget)) continue;

    const { title, icon, badge } = tabWidget.props as TabWidgetProps;
    const estimatedWidth = estimateTabWidth(
      title,
      icon,
      badge,
      hasButtons,
      variant
    );
    estimatedWidths.set(tabId, estimatedWidth);
  }

  return estimatedWidths;
}
