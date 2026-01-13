import React from 'react';
import { useDebounce } from '@/hooks/use-debounce';
import {
  measureTabsFromDOM,
  estimateUnrenderedTabWidths,
} from '../utils/tabUtils';
import type { TabWidgetProps } from '../types';

/**
 * Hook to calculate which tabs are visible vs hidden in dropdown
 * Handles responsive overflow calculation with resize/mutation observers
 * This is the most complex hook - manages tab visibility based on available space
 */
export function useTabCalculation(
  tabOrder: string[],
  dropdownOpen: boolean,
  visibleTabs: string[],
  hiddenTabs: string[],
  events: string[],
  variant: 'Tabs' | 'Content',
  containerRef: React.MutableRefObject<HTMLDivElement | null>,
  tabsListRef: React.MutableRefObject<HTMLDivElement | null>,
  tabWidgetsRef: React.MutableRefObject<React.ReactElement[]>,
  tabMeasurementsRef: React.MutableRefObject<Map<string, number>>,
  setVisibleTabs: React.Dispatch<React.SetStateAction<string[]>>,
  setHiddenTabs: React.Dispatch<React.SetStateAction<string[]>>,
  tabWidgets: React.ReactElement[]
) {
  // Main calculation function
  const calculateVisibleTabs = React.useCallback(() => {
    const container = containerRef.current;
    const tabsList = tabsListRef.current;
    if (!container || !tabsList) return;

    // Don't recalculate when dropdown is open to prevent infinite loops
    if (dropdownOpen) return;

    // Get the actual available width for tabs
    const containerComputedStyle = getComputedStyle(container);
    const containerPadding =
      parseFloat(containerComputedStyle.paddingLeft) +
        parseFloat(containerComputedStyle.paddingRight) || 0;
    const dropdownButtonWidth = 40; // Space for dropdown button only when needed
    let availableWidth = container.clientWidth - containerPadding;

    const newVisibleTabs: string[] = [];
    const newHiddenTabs: string[] = [];
    let currentWidth = 0;

    // Get actual tab measurements from the DOM
    const measurements = measureTabsFromDOM(tabsList);

    // Store measurements for future use
    tabMeasurementsRef.current = measurements;

    // Try to fit all tabs first
    let totalRequiredWidth = 0;

    // Estimate widths for tabs that haven't been measured yet
    const estimatedWidths = estimateUnrenderedTabWidths(
      tabOrder,
      tabWidgetsRef.current as React.ReactElement<TabWidgetProps>[],
      measurements,
      events,
      variant
    );

    // Calculate total required width
    for (const tabId of tabOrder) {
      const tabWidth =
        measurements.get(tabId) || estimatedWidths.get(tabId) || 0;
      totalRequiredWidth += tabWidth;
    }

    // Check if we need the dropdown button
    const needsDropdown = totalRequiredWidth > availableWidth;
    if (needsDropdown) {
      availableWidth -= dropdownButtonWidth;
    }

    // Second pass: determine which tabs fit
    for (const tabId of tabOrder) {
      const tabWidth =
        measurements.get(tabId) || estimatedWidths.get(tabId) || 0;

      // Check if this tab fits in the remaining space
      if (currentWidth + tabWidth <= availableWidth) {
        newVisibleTabs.push(tabId);
        currentWidth += tabWidth;
      } else {
        // This tab doesn't fit, so it and all remaining tabs go to dropdown
        newHiddenTabs.push(tabId);
        // Add all remaining tabs to hidden list
        const remainingIndex = tabOrder.indexOf(tabId) + 1;
        if (remainingIndex < tabOrder.length) {
          newHiddenTabs.push(...tabOrder.slice(remainingIndex));
        }
        break; // Stop processing - order is preserved
      }
    }

    // Only update state if there's an actual change
    const visibleChanged =
      newVisibleTabs.length !== visibleTabs.length ||
      !newVisibleTabs.every((id, index) => id === visibleTabs[index]);
    const hiddenChanged =
      newHiddenTabs.length !== hiddenTabs.length ||
      !newHiddenTabs.every((id, index) => id === hiddenTabs[index]);

    if (visibleChanged || hiddenChanged) {
      setVisibleTabs(newVisibleTabs);
      setHiddenTabs(newHiddenTabs);
    }
  }, [
    tabOrder,
    dropdownOpen,
    visibleTabs,
    hiddenTabs,
    events,
    variant,
    containerRef,
    tabsListRef,
    tabWidgetsRef,
    tabMeasurementsRef,
    setVisibleTabs,
    setHiddenTabs,
  ]);

  // Debounce the calculation
  const debouncedCalculateVisibleTabs = useDebounce(calculateVisibleTabs, 100);
  const debouncedCalculateVisibleTabsRef = React.useRef(
    debouncedCalculateVisibleTabs
  );
  const calculateVisibleTabsRef = React.useRef(calculateVisibleTabs);

  // Update refs
  React.useEffect(() => {
    debouncedCalculateVisibleTabsRef.current = debouncedCalculateVisibleTabs;
  }, [debouncedCalculateVisibleTabs]);

  React.useEffect(() => {
    calculateVisibleTabsRef.current = calculateVisibleTabs;
  }, [calculateVisibleTabs]);

  // Initial calculation + window resize listener
  React.useEffect(() => {
    calculateVisibleTabsRef.current();
    const handleResize = () => debouncedCalculateVisibleTabsRef.current();
    window.addEventListener('resize', handleResize);
    return () => {
      window.removeEventListener('resize', handleResize);
    };
  }, []);

  // ResizeObserver for container size tracking
  React.useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    const resizeObserver = new ResizeObserver(() => {
      debouncedCalculateVisibleTabsRef.current();
    });

    resizeObserver.observe(container);

    return () => {
      resizeObserver.disconnect();
    };
  }, [containerRef]);

  // Recalculate when tab order changes
  React.useEffect(() => {
    calculateVisibleTabsRef.current();
  }, [tabOrder]);

  // Recalculate when tabs are rendered
  React.useEffect(() => {
    const timer = setTimeout(() => {
      calculateVisibleTabsRef.current();
    }, 50);

    return () => clearTimeout(timer);
  }, [tabWidgets]);

  // Force initial calculation
  React.useEffect(() => {
    // Only set initial state if we haven't calculated yet
    if (
      visibleTabs.length === 0 &&
      hiddenTabs.length === 0 &&
      tabOrder.length > 0
    ) {
      setVisibleTabs(tabOrder);
      setHiddenTabs([]);
    }

    // Then calculate actual visibility after a brief delay
    const timer = setTimeout(() => {
      calculateVisibleTabsRef.current();
    }, 100);

    return () => clearTimeout(timer);
  }, [
    hiddenTabs.length,
    tabOrder,
    visibleTabs.length,
    setVisibleTabs,
    setHiddenTabs,
  ]);

  // MutationObserver for dynamic content changes
  React.useEffect(() => {
    if (!tabsListRef.current) return;

    const observer = new MutationObserver(mutations => {
      // Check if any mutation affects tab content
      const shouldRecalculate = mutations.some(mutation => {
        return (
          mutation.type === 'childList' ||
          mutation.type === 'characterData' ||
          (mutation.type === 'attributes' && mutation.attributeName === 'class')
        );
      });

      if (shouldRecalculate) {
        debouncedCalculateVisibleTabsRef.current();
      }
    });

    observer.observe(tabsListRef.current, {
      childList: true,
      subtree: true,
      characterData: true,
      attributes: true,
      attributeFilter: ['class'],
    });

    return () => observer.disconnect();
  }, [tabsListRef]);
}
