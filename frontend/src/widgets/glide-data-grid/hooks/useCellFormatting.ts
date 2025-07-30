import { GridCell, GridCellKind, Item } from '@glideapps/glide-data-grid';
import { useCallback } from 'react';
import { DataTableData, UseCellFormattingReturn } from '../types';
import {
  formatCellValue,
  getGridCellKind,
  toBoolean,
  toNumber,
} from '../utils/cellTypeMapping';

export function useCellFormatting(
  data: DataTableData | null
): UseCellFormattingReturn {
  const getCellContent = useCallback(
    (cell: Item): GridCell => {
      const [colIndex, rowIndex] = cell;

      if (!data?.rows[rowIndex] || !data?.columns[colIndex]) {
        return {
          kind: GridCellKind.Text,
          data: '',
          displayData: '',
          allowOverlay: false,
        };
      }

      const value = data.rows[rowIndex].values[colIndex];
      const columnType = data.columns[colIndex].type;
      const cellKind = getGridCellKind(value, columnType);

      // Handle different cell types
      switch (cellKind) {
        case GridCellKind.Number:
          return {
            kind: GridCellKind.Number,
            data: toNumber(value),
            displayData: formatCellValue(value, columnType),
            allowOverlay: true,
          };

        case GridCellKind.Boolean:
          return {
            kind: GridCellKind.Boolean,
            data: toBoolean(value),
            allowOverlay: false,
          };

        default:
          return {
            kind: GridCellKind.Text,
            data: formatCellValue(value, columnType),
            displayData: formatCellValue(value, columnType),
            allowOverlay: true,
          };
      }
    },
    [data]
  );

  return { getCellContent };
}
