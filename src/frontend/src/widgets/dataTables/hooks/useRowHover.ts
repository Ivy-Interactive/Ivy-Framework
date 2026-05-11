import { useCallback, useMemo, useState, useSyncExternalStore } from "react";
import { GridMouseCellEventArgs, GridMouseEventArgs } from "@glideapps/glide-data-grid";
import { useEventHandler } from "@/components/event-handler";
import { MenuItem } from "@/types/widgets";
import { getHiddenKeyValue } from "../utils/arrowUtils";
import * as arrow from "apache-arrow";

function subscribeHoverNone(onStoreChange: () => void) {
  const mq = window.matchMedia("(hover: none)");
  mq.addEventListener("change", onStoreChange);
  return () => mq.removeEventListener("change", onStoreChange);
}

function getHoverNoneSnapshot() {
  return window.matchMedia("(hover: none)").matches;
}

function getServerHoverNoneSnapshot() {
  return false;
}

interface UseRowHoverProps {
  widgetId: string;
  events: string[];
  visibleRows: number;
  enableRowHover: boolean | undefined;
  rowActions?: MenuItem[];
  perRowActions?: Record<string, MenuItem[]>;
  containerRef: React.RefObject<HTMLDivElement | null>;
  arrowTableRef: React.RefObject<arrow.Table | null>;
}

/**
 * Row hover + RowActions overlay. On touch / `(hover: none)` UIs, pointer "hover" is
 * sticky and must not drive the bar — use `syncRowChromeFromCellArgs` from `onCellClicked`.
 */
export const useRowHover = ({
  widgetId,
  events,
  visibleRows,
  enableRowHover,
  rowActions,
  perRowActions,
  containerRef,
  arrowTableRef,
}: UseRowHoverProps) => {
  const [hoverRow, setHoverRow] = useState<number | undefined>(undefined);
  const [actionButtonsTop, setActionButtonsTop] = useState<number>(0);
  const [actionButtonsHeight, setActionButtonsHeight] = useState<number>(0);
  const eventHandler = useEventHandler();

  const primaryInputCannotHover = useSyncExternalStore(
    subscribeHoverNone,
    getHoverNoneSnapshot,
    getServerHoverNoneSnapshot,
  );

  const layoutActionButtonsFromCellArgs = useCallback(
    (args: GridMouseCellEventArgs) => {
      if (!(rowActions?.length || perRowActions) || !containerRef.current) {
        setActionButtonsTop(0);
        setActionButtonsHeight(0);
        return;
      }
      const { bounds } = args;
      const container = containerRef.current;
      const containerRect = container.getBoundingClientRect();
      const style = window.getComputedStyle(container);
      const borderTop = parseFloat(style.borderTopWidth) || 0;
      const overlayTop = bounds.y - containerRect.top - borderTop;
      const overlayHeight = bounds.height;
      const dpr = window.devicePixelRatio;
      const snap = (val: number) => Math.round(val * dpr) / dpr;
      setActionButtonsTop(snap(overlayTop));
      setActionButtonsHeight(snap(overlayHeight));
    },
    [rowActions, perRowActions, containerRef],
  );

  const syncRowChromeFromCellArgs = useCallback(
    (args: GridMouseCellEventArgs) => {
      if (!(enableRowHover ?? false)) return;
      const [, row] = args.location;
      if (row >= visibleRows) {
        setHoverRow(undefined);
        setActionButtonsTop(0);
        setActionButtonsHeight(0);
        return;
      }
      setHoverRow(row);
      layoutActionButtonsFromCellArgs(args);
    },
    [enableRowHover, visibleRows, layoutActionButtonsFromCellArgs],
  );

  const clearRowHover = useCallback(() => {
    setHoverRow(undefined);
    setActionButtonsTop(0);
    setActionButtonsHeight(0);
  }, []);

  const onItemHovered = useCallback(
    (args: GridMouseEventArgs) => {
      if (!(enableRowHover ?? false)) return;

      const [, row] = args.location;

      if (args.kind === "cell" && row >= visibleRows) {
        setHoverRow(undefined);
        setActionButtonsTop(0);
        setActionButtonsHeight(0);
        return;
      }

      if (args.kind !== "cell") {
        setHoverRow(undefined);
        setActionButtonsTop(0);
        setActionButtonsHeight(0);
        return;
      }

      if (args.isTouch || primaryInputCannotHover) {
        return;
      }

      setHoverRow(row);
      layoutActionButtonsFromCellArgs(args as GridMouseCellEventArgs);
    },
    [enableRowHover, primaryInputCannotHover, layoutActionButtonsFromCellArgs, visibleRows],
  );

  const handleRowActionClick = useCallback(
    (action: MenuItem) => {
      if (hoverRow === undefined) return;

      const rowId = getHiddenKeyValue(arrowTableRef.current, hoverRow);

      if (events.includes("OnRowAction"))
        eventHandler("OnRowAction", widgetId, [
          {
            id: rowId !== null ? rowId : hoverRow,
            tag: action.tag ?? null,
          },
        ]);
    },
    [hoverRow, events, eventHandler, widgetId, arrowTableRef],
  );

  const resolvedRowActions = useMemo(() => {
    if (!perRowActions || hoverRow === undefined) return rowActions;
    const rowId = getHiddenKeyValue(arrowTableRef.current, hoverRow);
    if (rowId !== null && String(rowId) in perRowActions) {
      return perRowActions[String(rowId)];
    }
    return rowActions;
  }, [perRowActions, rowActions, hoverRow, arrowTableRef]);

  return {
    hoverRow,
    actionButtonsTop,
    actionButtonsHeight,
    onItemHovered,
    handleRowActionClick,
    resolvedRowActions,
    clearRowHover,
    syncRowChromeFromCellArgs,
  };
};
