import { SelectionModes } from '../types/types';

interface SelectionProps {
  rowSelect: 'none' | 'single' | 'multi';
  columnSelect: 'none' | 'single' | 'multi';
  rangeSelect: 'none' | 'cell' | 'rect' | 'multi-cell' | 'multi-rect';
}

/**
 * Maps SelectionModes enum to Glide Data Grid selection props
 */
export function getSelectionProps(
  selectionMode?: SelectionModes
): SelectionProps {
  switch (selectionMode) {
    case SelectionModes.Rows:
      return {
        rowSelect: 'multi',
        columnSelect: 'none',
        rangeSelect: 'none',
      };

    case SelectionModes.Columns:
      return {
        rowSelect: 'none',
        columnSelect: 'multi',
        rangeSelect: 'none',
      };

    case SelectionModes.Cells:
    default:
      return {
        rowSelect: 'none',
        columnSelect: 'none',
        rangeSelect: 'rect',
      };
  }
}
