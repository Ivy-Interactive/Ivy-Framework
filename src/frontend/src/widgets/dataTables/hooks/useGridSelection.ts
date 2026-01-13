import { useCallback, useState } from 'react';
import {
  CompactSelection,
  GridCell,
  GridCellKind,
  GridSelection,
  Item,
} from '@glideapps/glide-data-grid';

interface UseGridSelectionProps {
  visibleRows: number;
  getCellContent: (cell: Item) => GridCell;
}

/**
 * Hook to manage grid selection state and changes
 */
export const useGridSelection = ({
  visibleRows,
  getCellContent,
}: UseGridSelectionProps) => {
  const [gridSelection, setGridSelection] = useState<GridSelection>({
    columns: CompactSelection.empty(),
    rows: CompactSelection.empty(),
  });

  const handleGridSelectionChange = useCallback(
    (newSelection: GridSelection) => {
      // Consolidate check for newSelection.current
      if (newSelection.current !== undefined) {
        const [col, row] = newSelection.current.cell;

        // Prevent selection of empty filler rows
        if (row >= visibleRows) {
          // Don't allow selection of empty filler rows
          return;
        }

        // Check if the new selection includes link cells and prevent fuzzy effect
        // by clearing the selection if it's a single link cell click
        const cellContent = getCellContent([col, row]);

        // If it's a link cell, don't allow it to be selected (prevents fuzzy effect)
        if (
          cellContent.kind === GridCellKind.Custom &&
          (cellContent.data as { kind?: string })?.kind === 'link-cell'
        ) {
          // Clear the selection for link cells
          setGridSelection({
            columns: CompactSelection.empty(),
            rows: CompactSelection.empty(),
          });
          return;
        }
      }

      setGridSelection(newSelection);
    },
    [getCellContent, visibleRows]
  );

  return {
    gridSelection,
    handleGridSelectionChange,
  };
};
