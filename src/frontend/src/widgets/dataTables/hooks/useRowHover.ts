import { useCallback, useState } from "react";
import { GridMouseCellEventArgs, GridMouseEventArgs } from "@glideapps/glide-data-grid";
import { useEventHandler } from "@/components/event-handler";
import { MenuItem } from "@/types/widgets";
import * as arrow from "apache-arrow";

interface UseRowHoverProps {
  widgetId: string;
  visibleRows: number;
  enableRowHover: boolean | undefined;
  rowActions?: MenuItem[];
  containerRef: React.RefObject<HTMLDivElement | null>;
  arrowTableRef: React.RefObject<arrow.Table | null>;
}

/**
 * Hook to manage row hover state and action button positioning
 */
export const useRowHover = ({
  widgetId,
  visibleRows,
  enableRowHover,
  rowActions,
  containerRef,
  arrowTableRef,
}: UseRowHoverProps) => {
  const [hoverRow, setHoverRow] = useState<number | undefined>(undefined);
  const [actionButtonsTop, setActionButtonsTop] = useState<number>(0);
  const [actionButtonsHeight, setActionButtonsHeight] = useState<number>(0);
  const eventHandler = useEventHandler();

  // Extract _hiddenKey value directly from Arrow table
  const getHiddenKeyValue = useCallback(
    (rowIndex: number): string | number | null => {
      const table = arrowTableRef.current;
      if (!table) return null;

      // Access schema fields directly to find _hiddenKey
      const schema = table.schema;
      if (!schema || !schema.fields) return null;

      // Find the _hiddenKey column index
      let hiddenKeyIndex = -1;
      for (let i = 0; i < schema.fields.length; i++) {
        const field = schema.fields[i];
        if (field && field.name === "_hiddenKey") {
          hiddenKeyIndex = i;
          break;
        }
      }

      if (hiddenKeyIndex === -1) return null;

      // Get the column directly from the Arrow table
      const column = table.getChildAt(hiddenKeyIndex);
      if (!column) return null;

      // Get the value for this row
      const value = column.get(rowIndex);
      if (value === null || value === undefined || value === "") {
        return null;
      }

      return String(value);
    },
    [arrowTableRef],
  );

  // Handle row hover
  const onItemHovered = useCallback(
    (args: GridMouseEventArgs) => {
      if (!(enableRowHover ?? false)) return;
      const [, row] = args.location;
      // Don't allow hover on empty filler rows
      if (args.kind === "cell" && row >= visibleRows) {
        setHoverRow(undefined);
        return;
      }
      const newHoverRow = args.kind !== "cell" ? undefined : row;
      setHoverRow(newHoverRow);

      if (rowActions?.length && newHoverRow !== undefined && containerRef.current) {
        const { bounds } = args as GridMouseCellEventArgs;
        const container = containerRef.current;
        const containerRect = container.getBoundingClientRect();

        // Get precision border width (clientTop is always an integer, which is inaccurate at zoom)
        const style = window.getComputedStyle(container);
        const borderTop = parseFloat(style.borderTopWidth) || 0;

        // Convert grid viewport coords -> overlay container padding-box coords.
        const overlayTop = bounds.y - containerRect.top - borderTop;
        const overlayHeight = bounds.height;

        // Pixel-perfect snapping using devicePixelRatio.
        // This ensures the overlay aligns perfectly with physical pixels even at non-standard zoom levels.
        const dpr = window.devicePixelRatio;
        const snap = (val: number) => Math.round(val * dpr) / dpr;

        setActionButtonsTop(snap(overlayTop));
        setActionButtonsHeight(snap(overlayHeight));
      }
    },
    [enableRowHover, rowActions, visibleRows, containerRef],
  );

  // Handle row action button click
  const handleRowActionClick = useCallback(
    (action: MenuItem) => {
      if (hoverRow === undefined) return;

      // Extract _hiddenKey directly from Arrow table
      const rowId = getHiddenKeyValue(hoverRow);

      // Send event to backend's OnRowAction event with row ID and menu item tag
      eventHandler("OnRowAction", widgetId, [
        {
          id: rowId !== null ? rowId : hoverRow,
          tag: action.tag ?? null,
        },
      ]);
    },
    [hoverRow, eventHandler, widgetId, getHiddenKeyValue],
  );

  return {
    hoverRow,
    actionButtonsTop,
    actionButtonsHeight,
    onItemHovered,
    handleRowActionClick,
  };
};
