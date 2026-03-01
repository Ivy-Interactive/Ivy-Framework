import { useCallback, useState } from 'react';
import { DataEditorRef, GridMouseEventArgs } from '@glideapps/glide-data-grid';
import { useEventHandler } from '@/components/event-handler';
import { MenuItem } from '@/types/widgets';
import * as arrow from 'apache-arrow';

interface UseRowHoverProps {
  widgetId: string;
  visibleRows: number;
  enableRowHover: boolean | undefined;
  rowActions?: MenuItem[];
  gridRef: React.RefObject<DataEditorRef | null>;
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
  gridRef,
  containerRef,
  arrowTableRef,
}: UseRowHoverProps) => {
  const [hoverRow, setHoverRow] = useState<number | undefined>(undefined);
  const [actionButtonsTop, setActionButtonsTop] = useState<number>(0);
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
        if (field && field.name === '_hiddenKey') {
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
      if (value === null || value === undefined || value === '') {
        return null;
      }

      return String(value);
    },
    [arrowTableRef]
  );

  // Handle row hover
  const onItemHovered = useCallback(
    (args: GridMouseEventArgs) => {
      if (!(enableRowHover ?? false)) return;
      const [col, row] = args.location;
      // Don't allow hover on empty filler rows
      if (args.kind === 'cell' && row >= visibleRows) {
        setHoverRow(undefined);
        return;
      }
      const newHoverRow = args.kind !== 'cell' ? undefined : row;
      setHoverRow(newHoverRow);

      // Calculate action buttons position if row actions are configured
      if (
        rowActions &&
        rowActions.length > 0 &&
        newHoverRow !== undefined &&
        gridRef.current &&
        containerRef.current
      ) {
        // Use getBounds to get the actual cell position from the grid
        const bounds = gridRef.current.getBounds(col, newHoverRow);
        const containerRect = containerRef.current.getBoundingClientRect();

        if (bounds) {
          // Position button in the center of the row using the actual bounds
          // Subtract container offset to get position relative to container
          const buttonHeight = 24;
          const buttonTop =
            bounds.y -
            containerRect.top +
            bounds.height / 2 -
            buttonHeight / 2 -
            5;
          setActionButtonsTop(buttonTop);
        }
      }
    },
    [enableRowHover, rowActions, visibleRows, gridRef, containerRef]
  );

  // Handle row action button click
  const handleRowActionClick = useCallback(
    (action: MenuItem) => {
      if (hoverRow === undefined) return;

      // Extract _hiddenKey directly from Arrow table
      const rowId = getHiddenKeyValue(hoverRow);

      // Send event to backend's OnRowAction event with row ID and menu item tag
      eventHandler('OnRowAction', widgetId, [
        {
          id: rowId !== null ? rowId : hoverRow,
          tag: action.tag ?? null,
        },
      ]);
    },
    [hoverRow, eventHandler, widgetId, getHiddenKeyValue]
  );

  return {
    hoverRow,
    actionButtonsTop,
    onItemHovered,
    handleRowActionClick,
  };
};
