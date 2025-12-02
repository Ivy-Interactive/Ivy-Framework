import { useCallback, useState } from 'react';
import {
  DataEditorRef,
  GridCell,
  GridMouseEventArgs,
  Item,
} from '@glideapps/glide-data-grid';
import { useEventHandler } from '@/components/event-handler';
import { MenuItem } from '@/types/widgets';
import { DataColumn } from '../types/types';

interface UseRowHoverProps {
  widgetId: string;
  columns: DataColumn[];
  visibleRows: number;
  enableRowHover: boolean | undefined;
  rowActions?: MenuItem[];
  gridRef: React.RefObject<DataEditorRef | null>;
  containerRef: React.RefObject<HTMLDivElement | null>;
  getCellContent: (cell: Item) => GridCell;
}

/**
 * Hook to manage row hover state and action button positioning
 */
export const useRowHover = ({
  widgetId,
  columns,
  visibleRows,
  enableRowHover,
  rowActions,
  gridRef,
  containerRef,
  getCellContent,
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

  // Get row data as a record of column name -> value
  const getRowData = useCallback(
    (rowIndex: number): Record<string, unknown> => {
      const rowData: Record<string, unknown> = {};
      const visibleColumns = columns.filter(c => !c.hidden);

      visibleColumns.forEach((column, colIndex) => {
        const cell = getCellContent([colIndex, rowIndex]);
        let cellValue: unknown = null;

        if (
          cell.kind === 'text' ||
          cell.kind === 'number' ||
          cell.kind === 'boolean'
        ) {
          cellValue = cell.data;
        } else if ('data' in cell) {
          cellValue = (cell as unknown as { data: unknown }).data;
        }

        rowData[column.name] = cellValue;
      });

      return rowData;
    },
    [columns, getCellContent]
  );

  // Handle row action button click
  const handleRowActionClick = useCallback(
    (action: MenuItem) => {
      if (hoverRow === undefined) return;

      const rowData = getRowData(hoverRow);

      // Get action identifier from tag or label
      const actionId = action.tag?.toString() || action.label || '';

      // Send event to backend's OnRowAction event
      eventHandler('OnRowAction', widgetId, [
        {
          actionId: actionId,
          eventName: actionId,
          rowIndex: hoverRow,
          rowData: rowData,
        },
      ]);
    },
    [hoverRow, getRowData, eventHandler, widgetId]
  );

  return {
    hoverRow,
    actionButtonsTop,
    onItemHovered,
    handleRowActionClick,
  };
};
