import { useCallback } from 'react';
import { GridCell, GridCellKind, Item } from '@glideapps/glide-data-grid';
import { getCellContent as getCellContentUtil } from '../../utils/cellContent';
import { DataColumn, DataRow } from '../../types/types';

interface UseCellContentProps {
  columns: DataColumn[];
  columnOrder: number[];
  editable: boolean;
  visibleRows: number;
  getRowData: (rowIndex: number) => DataRow | null;
}

/**
 * Hook for generating cell content
 */
export const useCellContent = ({
  columns,
  columnOrder,
  editable,
  visibleRows,
  getRowData,
}: UseCellContentProps) => {
  const getCellContent = useCallback(
    (cell: Item): GridCell => {
      const [, row] = cell;
      // If this is an empty filler row, return empty text cell
      if (row >= visibleRows) {
        return {
          kind: GridCellKind.Text,
          data: '',
          displayData: '',
          allowOverlay: false,
        };
      }
      return getCellContentUtil(
        cell,
        columns,
        columnOrder,
        editable,
        getRowData
      );
    },
    [columns, columnOrder, editable, visibleRows, getRowData]
  );

  return { getCellContent };
};
