import { useCallback, type Dispatch, type SetStateAction } from "react";
import {
  CompactSelection,
  GridSelection,
  HeaderClickedEventArgs,
} from "@glideapps/glide-data-grid";
import { getOrderedVisibleDataColumns } from "../../utils/columnHelpers";
import { DataColumn } from "../../types/types";

interface UseHeaderMenuProps {
  columns: DataColumn[];
  columnOrder: number[];
  allowSorting: boolean;
  handleSort: (columnName: string) => void;
  setGridSelection: Dispatch<SetStateAction<GridSelection>>;
}

/**
 * Hook for handling header menu interactions (e.g., sorting)
 */
export const useHeaderMenu = ({
  columns,
  columnOrder,
  allowSorting,
  handleSort,
  setGridSelection,
}: UseHeaderMenuProps) => {
  const handleHeaderMenuClick = useCallback(
    (col: number, event: HeaderClickedEventArgs) => {
      if (!allowSorting) return;
      // Modifier clicks are for column selection, not sorting.
      if (event.ctrlKey || event.metaKey || event.shiftKey) return;

      event.preventDefault();

      const visibleColumns = getOrderedVisibleDataColumns(columns, columnOrder);
      const column = visibleColumns[col];

      if (column && (column.sortable ?? true)) {
        handleSort(column.name);
        setGridSelection((prev) => ({
          ...prev,
          columns: CompactSelection.empty(),
        }));
      }
    },
    [columns, columnOrder, handleSort, allowSorting, setGridSelection],
  );

  return { handleHeaderMenuClick };
};
