import { useState, useCallback, useMemo } from 'react';
import {
  GridColumn,
  GroupHeaderClickedEventArgs,
} from '@glideapps/glide-data-grid';

/**
 * Hook to enable column groups in DataTable
 *
 * @param columns - The grid columns array
 * @returns Object containing props to spread on DataEditor and helper functions
 */
export function useColumnGroups(columns: GridColumn[]) {
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

  // Adjust column widths based on collapsed groups
  const adjustedColumns = useMemo(() => {
    // Group columns by their group name
    const columnsByGroup = new Map<string, GridColumn[]>();
    columns.forEach(col => {
      if (col.group) {
        const group = columnsByGroup.get(col.group) || [];
        group.push(col);
        columnsByGroup.set(col.group, group);
      }
    });

    return columns.map(col => {
      // If column has no group, return as-is
      if (!col.group) return col;

      // If column's group is collapsed
      if (collapsedGroups.has(col.group)) {
        const groupColumns = columnsByGroup.get(col.group) || [];
        const isFirstInGroup = groupColumns[0] === col;

        if (isFirstInGroup) {
          // First column in collapsed group shows group indicator
          return {
            ...col,
            width: 60, // Slightly wider for the first column
            title: '▶', // Show collapse indicator
          };
        } else {
          // Other columns in collapsed group are hidden (width 0)
          return {
            ...col,
            width: 0,
            title: '',
          };
        }
      }

      // Otherwise return the column unchanged
      return col;
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
    columns: adjustedColumns,
    onGroupHeaderClicked,

    // Helper functions
    toggleGroup,
    expandAllGroups,
    collapseAllGroups,
    isGroupCollapsed,
    collapsedGroups: Array.from(collapsedGroups),

    // Metadata
    totalColumns: columns.length,
    visibleColumnCount: adjustedColumns.length,
    collapsedColumnCount: adjustedColumns.filter(
      col => col.group && collapsedGroups.has(col.group)
    ).length,
  };
}

export default useColumnGroups;
