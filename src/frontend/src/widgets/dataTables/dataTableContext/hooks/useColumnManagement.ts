import { useCallback, useEffect, useRef, useState } from "react";
import { GridColumn } from "@glideapps/glide-data-grid";
import { DataColumn } from "../../types/types";
import {
  parseSize,
  parseSizeGrow,
  estimateHeaderWidth,
  estimateContentWidth,
  getSizeMode,
} from "../utils/columnSizing";
import type * as arrow from "apache-arrow";
import { getCSSVariable } from "@/lib/theme";

interface UseColumnManagementProps {
  columnsProp: DataColumn[];
  allowColumnResizing: boolean;
}

/**
 * Hook for managing column widths, order, resize, and reorder
 */
export const useColumnManagement = ({
  columnsProp,
  allowColumnResizing,
}: UseColumnManagementProps) => {
  const [columns, setColumns] = useState<DataColumn[]>(columnsProp);
  const [columnWidths, setColumnWidths] = useState<Record<string, number>>({});
  const [columnOrder, setColumnOrder] = useState<number[]>([]);
  const isReorderingRef = useRef(false);

  const syncColumns = useCallback(() => {
    // Don't update columns during reordering
    if (isReorderingRef.current) return;

    // Check if structure changed before updating
    const structureChanged =
      columns.length !== columnsProp.length ||
      columns.some((col, idx) => col.name !== columnsProp[idx].name);

    if (structureChanged) {
      // Structure changed, reset column order and update columns
      // Both state updates are batched by React, so this is safe
      setColumnOrder([]);
      setColumns(columnsProp);
    } else {
      // Same structure, just update metadata without resetting order
      setColumns(columnsProp);
    }
  }, [columnsProp, columns]);

  // Update columns when columnsProp changes
  useEffect(() => {
    syncColumns();
    // oxlint-disable-next-line react-hooks/exhaustive-deps
  }, [columnsProp, syncColumns]);

  // Reset column widths when connection changes
  const resetColumnWidths = useCallback(() => {
    setColumnWidths({});
  }, []);

  // Initialize column order when columns are first loaded
  const initializeColumnOrder = useCallback(
    (mergedColumns: DataColumn[]) => {
      if (columnOrder.length === 0) {
        setColumnOrder(mergedColumns.map((_, index) => index));
      }
    },
    [columnOrder.length],
  );

  // Initialize column widths only if not already set (first load)
  const initializeColumnWidths = useCallback(
    (mergedColumns: DataColumn[], arrowTable?: arrow.Table | null) => {
      setColumnWidths((prevWidths) => {
        // If we already have column widths, preserve them
        if (Object.keys(prevWidths).length > 0) {
          return prevWidths;
        }

        // First time loading, initialize with default widths
        const rawFontSans = getCSSVariable("--font-sans") || "Geist";
        const fontSans = rawFontSans.replace(/^["']|["']$/g, "") || "Geist";
        const fontFam = `${fontSans}, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif`;
        const headerFont = `600 13px ${fontFam}`;
        const contentFont = `13px ${fontFam}`;
        const widths: Record<string, number> = {};

        const growColumnIndices: number[] = [];
        let maxGrowBaseWidth = 0;

        mergedColumns.forEach((col, index) => {
          const explicitWidth = parseSize(col.width);
          const headerMinWidth = estimateHeaderWidth(col.header || col.name, headerFont);

          // For content-based size types (Fit, MinContent, MaxContent),
          // sample actual data if available
          const sizeMode = getSizeMode(col.originalWidth ?? col.width);
          if (sizeMode && arrowTable) {
            const contentWidth = estimateContentWidth(arrowTable, index, sizeMode, contentFont);
            if (contentWidth !== undefined) {
              widths[index.toString()] = Math.max(headerMinWidth, contentWidth);
              return;
            }
          }

          // Check if this column uses grow/fraction sizing
          const sizeStr =
            col.originalWidth ?? (typeof col.width === "string" ? col.width : undefined);
          const growFactor = parseSizeGrow(sizeStr);
          if (growFactor !== undefined) {
            const baseWidth = Math.max(explicitWidth, headerMinWidth);
            growColumnIndices.push(index);
            maxGrowBaseWidth = Math.max(maxGrowBaseWidth, baseWidth);
            widths[index.toString()] = baseWidth;
            return;
          }

          // For fixed-width types, use explicit width or header width as minimum
          widths[index.toString()] = Math.max(explicitWidth, headerMinWidth);
        });

        for (const idx of growColumnIndices) {
          widths[idx.toString()] = maxGrowBaseWidth;
        }
        return widths;
      });
    },
    [],
  );

  // Handle column resize
  const handleColumnResize = useCallback(
    (column: GridColumn, newSize: number) => {
      // Check if column resizing is allowed
      if (!allowColumnResizing) return;

      // Find the column by matching title (which is col.header || col.name)
      const columnIndex = columns.findIndex((col) => (col.header || col.name) === column.title);

      if (columnIndex !== -1) {
        setColumnWidths((prev) => ({
          ...prev,
          [columnIndex.toString()]: newSize,
        }));
      }
    },
    [columns, allowColumnResizing],
  );

  // Handle column reorder
  const handleColumnReorder = useCallback(
    (startIndex: number, endIndex: number) => {
      // Set flag to prevent column updates during reordering
      isReorderingRef.current = true;

      setColumnOrder((prevOrder) => {
        // prevOrder contains indices into the full columns array
        // startIndex and endIndex are positions in the VISIBLE columns

        // Get the visible column indices (filtering out hidden ones)
        const visibleIndices = prevOrder.filter((idx) => !columns[idx]?.hidden);

        // Reorder just the visible indices
        const newVisibleIndices = [...visibleIndices];
        const [movedIndex] = newVisibleIndices.splice(startIndex, 1);
        newVisibleIndices.splice(endIndex, 0, movedIndex);

        // Reconstruct the full order array, preserving hidden column positions
        const newOrder = [...prevOrder];
        let visiblePosition = 0;

        for (let i = 0; i < newOrder.length; i++) {
          // Check if the column at this position in the ORIGINAL order is hidden
          if (!columns[prevOrder[i]]?.hidden) {
            newOrder[i] = newVisibleIndices[visiblePosition++];
          }
        }

        // Reset flag after a short delay
        setTimeout(() => {
          isReorderingRef.current = false;
        }, 100);

        return newOrder;
      });
    },
    [columns],
  );

  return {
    columns,
    setColumns,
    columnWidths,
    columnOrder,
    resetColumnWidths,
    initializeColumnOrder,
    initializeColumnWidths,
    handleColumnResize,
    handleColumnReorder,
  };
};
