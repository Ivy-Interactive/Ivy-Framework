import { useCallback, useState } from 'react';
import { DataEditorRef, GridMouseEventArgs } from '@glideapps/glide-data-grid';
import { useEventHandler } from '@/components/event-handler';
import { MenuItem } from '@/types/widgets';

interface UseRowHoverProps {
  widgetId: string;
  visibleRows: number;
  enableRowHover: boolean | undefined;
  rowActions?: MenuItem[];
  gridRef: React.RefObject<DataEditorRef | null>;
  containerRef: React.RefObject<HTMLDivElement | null>;
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
}: UseRowHoverProps) => {
  const [hoverRow, setHoverRow] = useState<number | undefined>(undefined);
  const [actionButtonsTop, setActionButtonsTop] = useState<number>(0);
  const eventHandler = useEventHandler();

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

      // Get action identifier from tag or label
      const actionId = action.tag?.toString() || action.label || '';

      // Send event to backend's OnRowAction event
      // Backend will look up the row ID using the idSelector
      eventHandler('OnRowAction', widgetId, [
        {
          actionId: actionId,
          rowIndex: hoverRow,
        },
      ]);
    },
    [hoverRow, eventHandler, widgetId]
  );

  return {
    hoverRow,
    actionButtonsTop,
    onItemHovered,
    handleRowActionClick,
  };
};
