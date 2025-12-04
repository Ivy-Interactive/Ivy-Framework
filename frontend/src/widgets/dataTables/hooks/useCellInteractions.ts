import { useCallback } from 'react';
import {
  GridCell,
  GridCellKind,
  GridMouseEventArgs,
  Item,
} from '@glideapps/glide-data-grid';
import { useEventHandler } from '@/components/event-handler';
import { validateLinkUrl, validateRedirectUrl } from '@/lib/utils';
import { DataColumn } from '../types/types';

interface UseCellInteractionsProps {
  widgetId: string;
  columns: DataColumn[];
  visibleRows: number;
  enableCellClickEvents: boolean | undefined;
  getCellContent: (cell: Item) => GridCell;
}

/**
 * Hook to handle cell click and activation events
 */
export const useCellInteractions = ({
  widgetId,
  columns,
  visibleRows,
  enableCellClickEvents,
  getCellContent,
}: UseCellInteractionsProps) => {
  const eventHandler = useEventHandler();

  // Handle cell single-clicks (for backend events and link navigation)
  const handleCellClicked = useCallback(
    (cell: Item, args: GridMouseEventArgs) => {
      const [, row] = cell;
      // Prevent interactions with empty filler rows
      if (row >= visibleRows) {
        return;
      }

      const cellContent = getCellContent(cell);

      // Handle Ctrl+Click or Cmd+Click on custom link cells
      if (
        (args.ctrlKey || args.metaKey) &&
        cellContent.kind === GridCellKind.Custom &&
        (cellContent.data as { kind?: string })?.kind === 'link-cell'
      ) {
        const url = (cellContent.data as { url?: string })?.url;

        // Validate URL to prevent open redirect vulnerabilities
        const validatedUrl = validateLinkUrl(url);
        if (validatedUrl === '#') {
          // Invalid URL, don't proceed
          return;
        }

        // External URLs (http/https) open in new tab
        if (
          validatedUrl.startsWith('http://') ||
          validatedUrl.startsWith('https://')
        ) {
          const newWindow = window.open(
            validatedUrl,
            '_blank',
            'noopener,noreferrer'
          );
          newWindow?.focus();
        } else {
          // Internal relative URLs navigate in same tab
          // Validate it's safe for redirect (relative path or same-origin)
          const redirectUrl = validateRedirectUrl(validatedUrl, false);
          if (redirectUrl) {
            window.location.href = redirectUrl;
          }
        }
        return; // Don't proceed with other click handling
      }

      if (enableCellClickEvents ?? false) {
        // Get actual cell value
        const visibleColumns = columns.filter(c => !c.hidden);
        const column = visibleColumns[cell[0]];

        // Extract the actual value from the cell based on its kind
        let cellValue: unknown = null;
        if (
          cellContent.kind === 'text' ||
          cellContent.kind === 'number' ||
          cellContent.kind === 'boolean'
        ) {
          cellValue = cellContent.data;
        } else if ('data' in cellContent) {
          // Cast to unknown first, then access the data property
          cellValue = (cellContent as unknown as { data: unknown }).data;
        }

        // Send event to backend as a single object matching CellClickEventArgs structure
        eventHandler('OnCellClick', widgetId, [
          {
            rowIndex: cell[1],
            columnIndex: cell[0],
            columnName: column?.name || '',
            cellValue: cellValue,
          },
        ]);
      }
      // Do NOT prevent default - let selection happen normally!
    },
    [
      enableCellClickEvents,
      eventHandler,
      widgetId,
      columns,
      getCellContent,
      visibleRows,
    ]
  );

  // Handle cell double-clicks/activation (for editing)
  const handleCellActivated = useCallback(
    (cell: Item) => {
      const [, row] = cell;
      // Prevent interactions with empty filler rows
      if (row >= visibleRows) {
        return;
      }

      if (enableCellClickEvents ?? false) {
        const cellContent = getCellContent(cell);
        const visibleColumns = columns.filter(c => !c.hidden);
        const column = visibleColumns[cell[0]];

        // Extract the actual value from the cell based on its kind
        let cellValue: unknown = null;
        if (
          cellContent.kind === 'text' ||
          cellContent.kind === 'number' ||
          cellContent.kind === 'boolean'
        ) {
          cellValue = cellContent.data;
        } else if ('data' in cellContent) {
          // Cast to unknown first, then access the data property
          cellValue = (cellContent as unknown as { data: unknown }).data;
        }

        // Send activation event to backend as a single object matching CellClickEventArgs structure
        eventHandler('OnCellActivated', widgetId, [
          {
            rowIndex: cell[1],
            columnIndex: cell[0],
            columnName: column?.name || '',
            cellValue: cellValue,
          },
        ]);
      }
    },
    [
      enableCellClickEvents,
      eventHandler,
      widgetId,
      columns,
      getCellContent,
      visibleRows,
    ]
  );

  return {
    handleCellClicked,
    handleCellActivated,
  };
};
