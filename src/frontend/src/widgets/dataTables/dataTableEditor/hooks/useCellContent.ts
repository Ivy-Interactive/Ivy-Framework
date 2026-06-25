import { useCallback, useMemo } from "react";
import { getDefaultTheme } from "@glideapps/glide-data-grid";
import { GridCell, GridCellKind, Item } from "@glideapps/glide-data-grid";
import { Densities } from "@/types/density";
import { getCellContent as getCellContentUtil } from "../../utils/cellContent";
import { getCellFont } from "../../utils/canvasText";
import { getVisibleColumnWidthAt, getOrderedVisibleDataColumns } from "../../utils/columnHelpers";
import { parseSizeGrow } from "../../dataTableContext/utils/columnSizing";
import { DENSITY_CONFIG } from "../constants";
import { DataColumn, DataRow } from "../../types/types";

interface UseCellContentProps {
  columns: DataColumn[];
  columnOrder: number[];
  columnWidths: Record<string, number>;
  density: Densities;
  editable: boolean;
  visibleRows: number;
  getRowData: (rowIndex: number) => DataRow | null;
}

/**
 * Hook for managing grid cell content
 */
export const useCellContent = ({
  columns,
  columnOrder,
  columnWidths,
  density,
  editable,
  visibleRows,
  getRowData,
}: UseCellContentProps) => {
  const headerFont = useMemo(() => {
    const t = getDefaultTheme();
    return `${t.headerFontStyle} ${t.fontFamily}`;
  }, []);

  const cellFont = useMemo(() => getCellFont(density), [density]);
  const cellHorizontalPadding = DENSITY_CONFIG[density].cellHorizontalPadding;

  const getCellContent = useCallback(
    (cell: Item): GridCell => {
      const [col, row] = cell;
      // If this is an empty filler row, return empty text cell
      if (row >= visibleRows) {
        return {
          kind: GridCellKind.Text,
          data: "",
          displayData: "",
          allowOverlay: false,
        };
      }

      const orderedColumns = getOrderedVisibleDataColumns(columns, columnOrder);
      const column = orderedColumns[col];

      let isGrowColumn = false;
      if (column) {
        const grow = parseSizeGrow(
          column.originalWidth ?? (typeof column.width === "string" ? column.width : undefined),
        );
        const isLastColumn = col === orderedColumns.length - 1;
        isGrowColumn = (grow !== undefined && grow > 0) || isLastColumn;
      }

      return getCellContentUtil(cell, columns, columnOrder, editable, getRowData, {
        columnWidth: getVisibleColumnWidthAt(col, columns, columnOrder, columnWidths, headerFont),
        cellHorizontalPadding,
        cellFont,
        isGrowColumn,
      });
    },
    [
      columns,
      columnOrder,
      columnWidths,
      headerFont,
      cellFont,
      cellHorizontalPadding,
      editable,
      visibleRows,
      getRowData,
    ],
  );

  return { getCellContent };
};
