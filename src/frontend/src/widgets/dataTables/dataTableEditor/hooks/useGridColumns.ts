import { useMemo } from "react";
import { getDefaultTheme } from "@glideapps/glide-data-grid";
import { convertToGridColumns } from "../../utils/columnHelpers";
import { useColumnGroups } from "../../hooks/useColumnGroups";
import { DataColumn } from "../../types/types";
import type { SortOrder } from "@/services/grpcTableService";

interface UseGridColumnsProps {
  columns: DataColumn[];
  columnOrder: number[];
  columnWidths: Record<string, number>;
  showGroups?: boolean;
  showColumnTypeIcons?: boolean;
  activeSort?: SortOrder[] | null;
}

/**
 * Hook for managing grid columns configuration
 */
export const useGridColumns = ({
  columns,
  columnOrder,
  columnWidths,
  showGroups = false,
  showColumnTypeIcons = true,
  activeSort,
}: UseGridColumnsProps) => {
  const headerFont = useMemo(() => {
    const t = getDefaultTheme();
    return `${t.headerFontStyle} ${t.fontFamily}`;
  }, []);

  // Convert columns to grid format with proper widths
  // Memoize to prevent recalculation on every render
  const gridColumns = useMemo(
    () =>
      convertToGridColumns(
        columns,
        columnOrder,
        columnWidths,
        showGroups,
        showColumnTypeIcons,
        headerFont,
        activeSort,
      ),
    [columns, columnOrder, columnWidths, showGroups, showColumnTypeIcons, headerFont, activeSort],
  );

  // Use column groups hook when showGroups is enabled
  const columnGroupsHook = useColumnGroups(gridColumns);
  const shouldUseColumnGroups = showGroups;

  // Use grouped columns if showGroups is enabled, otherwise use regular columns
  const finalColumns = shouldUseColumnGroups ? columnGroupsHook.columns : gridColumns;

  return {
    columns: finalColumns,
    shouldUseColumnGroups,
    onGroupHeaderClicked: columnGroupsHook.onGroupHeaderClicked,
  };
};
