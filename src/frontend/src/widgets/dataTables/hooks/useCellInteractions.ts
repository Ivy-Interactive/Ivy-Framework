import { useCallback, useEffect, useRef } from "react";
import { GridCell, GridCellKind, GridMouseEventArgs, Item } from "@glideapps/glide-data-grid";
import { useEventHandler } from "@/components/event-handler";
import { validateLinkUrl, validateRedirectUrl } from "@/lib/url";
import { DataColumn } from "../types/types";
import { getHiddenKeyValue } from "../utils/arrowUtils";
import * as arrow from "apache-arrow";

/** Delay before emitting OnCellClick so a quick second click on the same cell can cancel it (double-click → OnCellActivated only). */
const SINGLE_CLICK_EMIT_DELAY_MS = 300;

export function openLinkUrl(url: string | undefined) {
  const validatedUrl = validateLinkUrl(url);
  if (validatedUrl === "#") return;
  if (validatedUrl.startsWith("http://") || validatedUrl.startsWith("https://")) {
    window.open(validatedUrl, "_blank", "noopener,noreferrer")?.focus();
  } else {
    const redirectUrl = validateRedirectUrl(validatedUrl, false);
    if (redirectUrl) window.location.href = redirectUrl;
  }
}

interface UseCellInteractionsProps {
  widgetId: string;
  events: string[];
  columns: DataColumn[];
  visibleRows: number;
  enableCellClickEvents: boolean | undefined;
  getCellContent: (cell: Item) => GridCell;
  arrowTableRef: React.RefObject<arrow.Table | null>;
}

/**
 * Hook to handle cell click and activation events
 */
export const useCellInteractions = ({
  widgetId,
  events,
  columns,
  visibleRows,
  enableCellClickEvents,
  getCellContent,
  arrowTableRef,
}: UseCellInteractionsProps) => {
  const eventHandler = useEventHandler();

  const pendingSingleClickTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const lastClickedCellKeyRef = useRef<string | null>(null);

  useEffect(
    () => () => {
      if (pendingSingleClickTimerRef.current !== null) {
        clearTimeout(pendingSingleClickTimerRef.current);
        pendingSingleClickTimerRef.current = null;
      }
    },
    [],
  );

  const emitOnCellClick = useCallback(
    (cell: Item, cellContent: GridCell) => {
      const visibleColumns = columns.filter((c) => !c.hidden);
      const column = visibleColumns[cell[0]];

      const getCellValue = (content: GridCell) => {
        if (content.kind === "text" || content.kind === "number" || content.kind === "boolean") {
          return content.data;
        } else if ("data" in content) {
          const cellData = (content as unknown as { data: unknown }).data;

          if (
            cellData &&
            typeof cellData === "object" &&
            "kind" in cellData &&
            (cellData as { kind: string }).kind === "link-cell" &&
            "url" in cellData
          ) {
            return (cellData as unknown as { url: string }).url;
          } else {
            return cellData;
          }
        }
        return null;
      };

      const cellValue = getCellValue(cellContent);
      const rowId = getHiddenKeyValue(arrowTableRef.current, cell[1]);

      if (events.includes("OnCellClick"))
        eventHandler("OnCellClick", widgetId, [
          {
            rowIndex: cell[1],
            columnIndex: cell[0],
            columnName: column?.name || "",
            cellValue: cellValue,
            rowId: rowId,
          },
        ]);
    },
    [columns, events, eventHandler, widgetId, arrowTableRef],
  );

  // Handle cell single-clicks (for backend events and link navigation)
  const handleCellClicked = useCallback(
    (cell: Item, args: GridMouseEventArgs) => {
      const [, row] = cell;
      // Prevent interactions with empty filler rows
      if (row >= visibleRows) {
        return;
      }

      const cellContent = getCellContent(cell);

      // Handle click on custom link cells (requires cmd/ctrl+click) — always immediate, even when deferring OnCellClick
      if (
        cellContent.kind === GridCellKind.Custom &&
        (cellContent.data as { kind?: string })?.kind === "link-cell" &&
        (args.metaKey || args.ctrlKey)
      ) {
        const url = (cellContent.data as { url?: string })?.url;
        openLinkUrl(url);
      }

      if (enableCellClickEvents ?? false) {
        const cellKey = `${cell[0]},${cell[1]}`;

        // Second click on the same cell before the deferred OnCellClick fires — likely double-click; suppress OnCellClick (OnCellActivated follows).
        if (
          pendingSingleClickTimerRef.current !== null &&
          lastClickedCellKeyRef.current === cellKey
        ) {
          clearTimeout(pendingSingleClickTimerRef.current);
          pendingSingleClickTimerRef.current = null;
          lastClickedCellKeyRef.current = cellKey;
          return;
        }

        if (pendingSingleClickTimerRef.current !== null) {
          clearTimeout(pendingSingleClickTimerRef.current);
          pendingSingleClickTimerRef.current = null;
        }

        lastClickedCellKeyRef.current = cellKey;

        pendingSingleClickTimerRef.current = setTimeout(() => {
          pendingSingleClickTimerRef.current = null;
          emitOnCellClick(cell, cellContent);
        }, SINGLE_CLICK_EMIT_DELAY_MS);
      }

      // Do NOT prevent default - let selection happen normally!
    },
    [enableCellClickEvents, getCellContent, visibleRows, emitOnCellClick],
  );

  // Handle cell double-clicks/activation (for editing)
  const handleCellActivated = useCallback(
    (cell: Item) => {
      const [, row] = cell;
      // Prevent interactions with empty filler rows
      if (row >= visibleRows) {
        return;
      }

      // Double-click cancels deferred single-click so OnCellClick does not fire together with OnCellActivated.
      if (pendingSingleClickTimerRef.current !== null) {
        clearTimeout(pendingSingleClickTimerRef.current);
        pendingSingleClickTimerRef.current = null;
      }

      if (enableCellClickEvents ?? false) {
        const cellContent = getCellContent(cell);
        const visibleColumns = columns.filter((c) => !c.hidden);
        const column = visibleColumns[cell[0]];

        // Extract the actual value from the cell based on its kind
        let cellValue: unknown = null;
        if (
          cellContent.kind === "text" ||
          cellContent.kind === "number" ||
          cellContent.kind === "boolean"
        ) {
          cellValue = cellContent.data;
        } else if ("data" in cellContent) {
          // Cast to unknown first, then access the data property
          cellValue = (cellContent as unknown as { data: unknown }).data;
        }

        const rowId = getHiddenKeyValue(arrowTableRef.current, cell[1]);

        // Send activation event to backend as a single object matching CellClickEventArgs structure
        if (events.includes("OnCellActivated"))
          eventHandler("OnCellActivated", widgetId, [
            {
              rowIndex: cell[1],
              columnIndex: cell[0],
              columnName: column?.name || "",
              cellValue: cellValue,
              rowId: rowId,
            },
          ]);
      }
    },
    [
      enableCellClickEvents,
      events,
      eventHandler,
      widgetId,
      columns,
      getCellContent,
      visibleRows,
      arrowTableRef,
    ],
  );

  return {
    handleCellClicked,
    handleCellActivated,
  };
};
