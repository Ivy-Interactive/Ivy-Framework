import { GridColumn, GridColumnIcon } from "@glideapps/glide-data-grid";
import type { DataColumn } from "../types/types";
import type { SortOrder } from "@/services/grpcTableService";
import {
  estimateHeaderWidth,
  parseSizeGrow,
  parseSizeMin,
} from "../dataTableContext/utils/columnSizing";

/**
 * Maps column icon names or types to appropriate icons.
 * Columns with an explicit `.Icon()` return the icon name as-is
 * (looked up in the SpriteMap). Type-based icons use built-in GridColumnIcon values.
 */
export function mapColumnIcon(col: DataColumn): GridColumnIcon | string | undefined {
  if (col.icon) {
    return col.icon;
  }
  const normalizedType = (col.type ?? "text").toLowerCase();

  if (normalizedType === "number") {
    return GridColumnIcon.HeaderNumber;
  }
  if (normalizedType === "text") {
    return GridColumnIcon.HeaderString;
  }
  if (normalizedType === "date" || normalizedType === "datetime") {
    return GridColumnIcon.HeaderDate;
  }
  if (normalizedType === "boolean") {
    return GridColumnIcon.HeaderBoolean;
  }
  if (normalizedType === "icon") {
    return GridColumnIcon.HeaderImage;
  }

  return GridColumnIcon.HeaderString;
}

/**
 * Reorders columns array by moving a column from startIndex to endIndex
 * Returns a new array without modifying the original
 */
export function reorderColumns(
  columns: DataColumn[],
  startIndex: number,
  endIndex: number,
): DataColumn[] {
  const result = [...columns];
  const [removed] = result.splice(startIndex, 1);
  result.splice(endIndex, 0, removed);
  return result;
}

export function resolveColumnWidth(
  col: DataColumn,
  originalIndex: number,
  columnWidths: Record<string, number>,
  headerFont?: string,
): number {
  const baseWidth = columnWidths[originalIndex.toString()] || col.width;
  let numericBaseWidth = typeof baseWidth === "string" ? parseFloat(baseWidth) : baseWidth;

  if (isNaN(numericBaseWidth) || !numericBaseWidth) {
    numericBaseWidth = estimateHeaderWidth(col.header || col.name, headerFont);
  }

  const minWidth = parseSizeMin(col.originalWidth);
  if (minWidth && minWidth > numericBaseWidth) {
    numericBaseWidth = minWidth;
  }

  return numericBaseWidth;
}

export function getVisibleColumnWidthAt(
  colIndex: number,
  columns: DataColumn[],
  columnOrder: number[],
  columnWidths: Record<string, number>,
  headerFont?: string,
): number | undefined {
  const orderedColumns = getOrderedVisibleDataColumns(columns, columnOrder);
  const col = orderedColumns[colIndex];
  if (!col) return undefined;
  const originalIndex = columns.indexOf(col);
  return resolveColumnWidth(col, originalIndex, columnWidths, headerFont);
}

export function getOrderedVisibleDataColumns(
  columns: DataColumn[],
  columnOrder: number[],
): DataColumn[] {
  const visibleColumns = columns.filter((col) => !col.hidden);
  let orderedColumns = visibleColumns;

  if (columnOrder.length === columns.length) {
    orderedColumns = columnOrder.reduce<DataColumn[]>((acc, idx) => {
      const col = columns[idx];
      if (col && !col.hidden) acc.push(col);
      return acc;
    }, []);
  } else {
    const hasOrderProperty = visibleColumns.some((col) => col.order !== undefined);
    if (hasOrderProperty) {
      orderedColumns = [...visibleColumns].sort((a, b) => {
        const orderA = a.order ?? Number.MAX_SAFE_INTEGER;
        const orderB = b.order ?? Number.MAX_SAFE_INTEGER;
        return orderA - orderB;
      });
    }
  }
  return orderedColumns;
}

export function convertToGridColumns(
  columns: DataColumn[],
  columnOrder: number[],
  columnWidths: Record<string, number>,
  showGroups: boolean,
  showColumnTypeIcons: boolean = true,
  headerFont?: string,
  activeSort?: SortOrder[] | null,
): GridColumn[] {
  const orderedColumns = getOrderedVisibleDataColumns(columns, columnOrder);

  return orderedColumns.map((col, index) => {
    const originalIndex = columns.indexOf(col);
    const numericBaseWidth = resolveColumnWidth(col, originalIndex, columnWidths, headerFont);

    const grow = parseSizeGrow(
      col.originalWidth ?? (typeof col.width === "string" ? col.width : undefined),
    );
    const isLastColumn = index === orderedColumns.length - 1;
    const effectiveGrow = grow !== undefined ? grow : isLastColumn ? 1 : undefined;

    const shouldShowIcon = Boolean(col.icon) || showColumnTypeIcons;
    const columnIcon = shouldShowIcon ? mapColumnIcon(col) : undefined;
    const sortForColumn = activeSort?.find((sort) => sort.column === col.name);
    const indicatorIcon =
      sortForColumn !== undefined
        ? sortForColumn.direction === "ASC"
          ? "ArrowUp"
          : "ArrowDown"
        : undefined;

    return {
      title: col.header || col.name,
      width: numericBaseWidth,
      ...(effectiveGrow !== undefined && { grow: effectiveGrow }),
      group: showGroups ? col.group : undefined,
      icon: columnIcon,
      ...(indicatorIcon !== undefined && { indicatorIcon }),
    };
  });
}
