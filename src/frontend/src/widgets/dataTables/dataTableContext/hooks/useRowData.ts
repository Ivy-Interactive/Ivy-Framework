import { useCallback } from 'react';
import * as arrow from 'apache-arrow';
import { DataRow } from '../../types/types';

/**
 * Hook for accessing row data from Arrow table
 */
export const useRowData = (
  arrowTableRef: React.RefObject<arrow.Table | null>
) => {
  const getRowData = useCallback(
    (rowIndex: number): DataRow | null => {
      const table = arrowTableRef.current;
      if (!table || rowIndex < 0 || rowIndex >= table.numRows) {
        return null;
      }

      const values: (string | number | boolean | Date | string[] | null)[] = [];
      for (let j = 0; j < table.numCols; j++) {
        const column = table.getChildAt(j);
        if (column) {
          const value = column.get(rowIndex);
          values.push(value);
        }
      }
      return { values };
    },
    [arrowTableRef]
  );

  return { getRowData };
};
