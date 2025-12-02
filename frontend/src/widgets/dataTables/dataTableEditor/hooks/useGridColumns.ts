import { useMemo } from 'react';
import { convertToGridColumns } from '../../utils/columnHelpers';
import { useColumnGroups } from '../../hooks/useColumnGroups';
import { DataColumn } from '../../types/types';

interface UseGridColumnsProps {
  columns: DataColumn[];
  columnOrder: number[];
  columnWidths: Record<string, number>;
  containerWidth: number;
  showGroups?: boolean;
  showColumnTypeIcons?: boolean;
}

/**
 * Hook for managing grid columns configuration
 */
export const useGridColumns = ({
  columns,
  columnOrder,
  columnWidths,
  containerWidth,
  showGroups = false,
  showColumnTypeIcons = true,
}: UseGridColumnsProps) => {
  // Convert columns to grid format with proper widths
  // Memoize to prevent recalculation on every render
  const gridColumns = useMemo(
    () =>
      convertToGridColumns(
        columns,
        columnOrder,
        columnWidths,
        containerWidth,
        showGroups,
        showColumnTypeIcons
      ),
    [
      columns,
      columnOrder,
      columnWidths,
      containerWidth,
      showGroups,
      showColumnTypeIcons,
    ]
  );

  // Use column groups hook when showGroups is enabled
  const columnGroupsHook = useColumnGroups(gridColumns);
  const shouldUseColumnGroups = showGroups;

  // Use grouped columns if showGroups is enabled, otherwise use regular columns
  const finalColumns = shouldUseColumnGroups
    ? columnGroupsHook.columns
    : gridColumns;

  return {
    columns: finalColumns,
    shouldUseColumnGroups,
    onGroupHeaderClicked: columnGroupsHook.onGroupHeaderClicked,
  };
};
