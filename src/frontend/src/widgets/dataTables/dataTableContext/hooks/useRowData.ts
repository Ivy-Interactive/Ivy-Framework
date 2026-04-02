import { useCallback, useMemo } from "react";
import * as arrow from "apache-arrow";
import { DataRow } from "../../types/types";
import { buildDecimalScales, convertDecimalValue } from "../../utils/tableDataMapper";

/**
 * Hook for accessing row data from Arrow table
 */
export const useRowData = (arrowTableRef: React.RefObject<arrow.Table | null>) => {
  const decimalScalesRef = useMemo(() => {
    return { current: new Map<number, number>() };
  }, []);

  const getRowData = useCallback(
    (rowIndex: number): DataRow | null => {
      const table = arrowTableRef.current;
      if (!table || rowIndex < 0 || rowIndex >= table.numRows) {
        return null;
      }

      // Rebuild decimal scales when schema changes (cheap for small field counts)
      if (decimalScalesRef.current.size === 0 && table.schema.fields.length > 0) {
        decimalScalesRef.current = buildDecimalScales(table.schema);
      }

      const values: (string | number | boolean | Date | string[] | null)[] = [];
      for (let j = 0; j < table.numCols; j++) {
        const column = table.getChildAt(j);
        if (column) {
          let value = column.get(rowIndex);
          const scale = decimalScalesRef.current.get(j);
          if (scale !== undefined && value != null && typeof value === "object") {
            value = convertDecimalValue(value, scale);
          }
          values.push(value);
        }
      }
      return { values };
    },
    [arrowTableRef, decimalScalesRef],
  );

  return { getRowData };
};
