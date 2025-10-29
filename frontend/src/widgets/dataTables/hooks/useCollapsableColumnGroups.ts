import { useState, useCallback, useMemo } from 'react';
import {
  GridColumn,
  GroupHeaderClickedEventArgs,
} from '@glideapps/glide-data-grid';

/**
 * Hook to enable collapsible column groups in DataTable
 *
 * @param columns - The grid columns array
 * @returns Object containing props to spread on DataEditor and helper functions
 */
export function useCollapsableColumnGroups(columns: GridColumn[]) {
  // Track which groups are collapsed
  const [collapsedGroups, setCollapsedGroups] = useState<Set<string>>(
    new Set()
  );

  // Get unique groups from columns
  const groups = useMemo(() => {
    const uniqueGroups = new Set<string>();
    columns.forEach(col => {
      if (col.group) {
        uniqueGroups.add(col.group);
      }
    });
    return Array.from(uniqueGroups);
  }, [columns]);

  // Filter columns based on collapsed groups
  const visibleColumns = useMemo(() => {
    return columns.filter(col => {
      // If column has no group, always show it
      if (!col.group) return true;

      // If column's group is collapsed, hide it
      return !collapsedGroups.has(col.group);
    });
  }, [columns, collapsedGroups]);

  // Handle group header click
  const onGroupHeaderClicked = useCallback(
    (_colIndex: number, event: GroupHeaderClickedEventArgs) => {
      // Prevent default behavior
      event.preventDefault();

      // Get the group name from the event
      const groupName = event.group;
      if (!groupName) return;

      // Toggle the collapsed state
      setCollapsedGroups(prev => {
        const newSet = new Set(prev);
        if (newSet.has(groupName)) {
          newSet.delete(groupName);
        } else {
          newSet.add(groupName);
        }
        return newSet;
      });
    },
    []
  );

  // Helper function to toggle a specific group
  const toggleGroup = useCallback((groupName: string) => {
    setCollapsedGroups(prev => {
      const newSet = new Set(prev);
      if (newSet.has(groupName)) {
        newSet.delete(groupName);
      } else {
        newSet.add(groupName);
      }
      return newSet;
    });
  }, []);

  // Helper function to expand all groups
  const expandAllGroups = useCallback(() => {
    setCollapsedGroups(new Set());
  }, []);

  // Helper function to collapse all groups
  const collapseAllGroups = useCallback(() => {
    setCollapsedGroups(new Set(groups));
  }, [groups]);

  // Check if a group is collapsed
  const isGroupCollapsed = useCallback(
    (groupName: string) => {
      return collapsedGroups.has(groupName);
    },
    [collapsedGroups]
  );

  return {
    // Props to spread on DataEditor
    columns: visibleColumns,
    onGroupHeaderClicked,

    // Helper functions
    toggleGroup,
    expandAllGroups,
    collapseAllGroups,
    isGroupCollapsed,
    collapsedGroups: Array.from(collapsedGroups),

    // Metadata
    totalColumns: columns.length,
    visibleColumnCount: visibleColumns.length,
    hiddenColumnCount: columns.length - visibleColumns.length,
  };
}

export default useCollapsableColumnGroups;
