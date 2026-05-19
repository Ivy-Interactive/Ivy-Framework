import { useCallback, useEffect, useLayoutEffect, useRef, useState, type RefObject } from "react";
import { DataEditorRef, GridColumn, GridSelection } from "@glideapps/glide-data-grid";

const MIN_COLUMN_WIDTH = 50;
const MAX_COLUMN_WIDTH = 2000;
const HANDLE_HIT_WIDTH = 28;

export interface ColumnResizeHandle {
  colIndex: number;
  column: GridColumn;
  x: number;
  y: number;
  width: number;
  height: number;
}

function isCoarsePointer(): boolean {
  if (typeof window === "undefined") return false;
  return window.matchMedia("(pointer: coarse)").matches;
}

function clampWidth(width: number): number {
  return Math.max(MIN_COLUMN_WIDTH, Math.min(MAX_COLUMN_WIDTH, Math.round(width)));
}

/** Column that should show the mobile resize grip (selected cell or selected column). */
export function getActiveColumnIndex(selection: GridSelection): number | null {
  const cellCol = selection.current?.cell[0];
  if (cellCol !== undefined) return cellCol;
  return selection.columns.first() ?? null;
}

interface UseMobileColumnResizeProps {
  enabled: boolean;
  gridRef: RefObject<DataEditorRef | null>;
  containerRef: RefObject<HTMLDivElement | null>;
  columns: GridColumn[];
  groupHeaderHeight: number;
  selectedColIndex: number | null;
  onColumnResize: (column: GridColumn, newSize: number) => void;
  layoutKey: string;
}

/**
 * Touch-friendly column resize: visible grip on header right edge, drag resizes the column
 * without horizontal table scroll (Glide edge-drag is disabled on coarse pointers).
 */
export function useMobileColumnResize({
  enabled,
  gridRef,
  containerRef,
  columns,
  groupHeaderHeight,
  selectedColIndex,
  onColumnResize,
  layoutKey,
}: UseMobileColumnResizeProps) {
  const [coarsePointer, setCoarsePointer] = useState(isCoarsePointer);
  const [handles, setHandles] = useState<ColumnResizeHandle[]>([]);
  const [resizingColIndex, setResizingColIndex] = useState<number | null>(null);
  const dragRef = useRef<{ colIndex: number; startX: number; startWidth: number } | null>(null);
  const onColumnResizeRef = useRef(onColumnResize);
  onColumnResizeRef.current = onColumnResize;

  useEffect(() => {
    const mql = window.matchMedia("(pointer: coarse)");
    const onChange = () => setCoarsePointer(mql.matches);
    mql.addEventListener("change", onChange);
    onChange();
    return () => mql.removeEventListener("change", onChange);
  }, []);

  const useMobileHandles = enabled && coarsePointer;

  const syncHandles = useCallback(() => {
    const grid = gridRef.current;
    const container = containerRef.current;
    if (!useMobileHandles || !grid || !container) {
      setHandles([]);
      return;
    }

    const targetColIndex = resizingColIndex ?? selectedColIndex;
    if (targetColIndex === null) {
      setHandles([]);
      return;
    }

    const column = columns[targetColIndex];
    if (!column) {
      setHandles([]);
      return;
    }

    const headerBounds = grid.getBounds(targetColIndex, -1);
    if (!headerBounds || headerBounds.width < 1) {
      setHandles([]);
      return;
    }

    const containerRect = container.getBoundingClientRect();
    const right = headerBounds.x + headerBounds.width;
    if (right < containerRect.left || headerBounds.x > containerRect.right) {
      setHandles([]);
      return;
    }

    const top = groupHeaderHeight > 0 ? headerBounds.y - groupHeaderHeight : headerBounds.y;
    const height = headerBounds.height + groupHeaderHeight;

    setHandles([
      {
        colIndex: targetColIndex,
        column,
        x: right - HANDLE_HIT_WIDTH / 2,
        y: top,
        width: HANDLE_HIT_WIDTH,
        height,
      },
    ]);
  }, [
    columns,
    containerRef,
    gridRef,
    groupHeaderHeight,
    resizingColIndex,
    selectedColIndex,
    useMobileHandles,
  ]);

  useLayoutEffect(() => {
    syncHandles();
  }, [syncHandles, layoutKey]);

  useEffect(() => {
    if (!useMobileHandles) return;

    const container = containerRef.current;
    if (!container) return;

    const scroller = container.querySelector(".dvn-scroller");
    const onScrollOrResize = () => requestAnimationFrame(syncHandles);

    scroller?.addEventListener("scroll", onScrollOrResize, { passive: true });
    window.addEventListener("resize", onScrollOrResize);

    return () => {
      scroller?.removeEventListener("scroll", onScrollOrResize);
      window.removeEventListener("resize", onScrollOrResize);
    };
  }, [containerRef, syncHandles, useMobileHandles]);

  const endDrag = useCallback(() => {
    dragRef.current = null;
    setResizingColIndex(null);
    document.body.classList.remove("ivy-datatable-column-resize-active");
  }, []);

  useEffect(() => {
    if (!useMobileHandles) return;

    const onPointerMove = (e: PointerEvent) => {
      const drag = dragRef.current;
      if (!drag) return;

      e.preventDefault();
      const delta = e.clientX - drag.startX;
      const newSize = clampWidth(drag.startWidth + delta);
      const column = columns[drag.colIndex];
      if (!column) return;
      onColumnResizeRef.current(column, newSize);
      requestAnimationFrame(syncHandles);
    };

    const onPointerUp = () => endDrag();

    const onPointerCancel = () => endDrag();

    window.addEventListener("pointermove", onPointerMove, { passive: false });
    window.addEventListener("pointerup", onPointerUp);
    window.addEventListener("pointercancel", onPointerCancel);

    return () => {
      window.removeEventListener("pointermove", onPointerMove);
      window.removeEventListener("pointerup", onPointerUp);
      window.removeEventListener("pointercancel", onPointerCancel);
    };
  }, [columns, endDrag, syncHandles, useMobileHandles]);

  const onHandlePointerDown = useCallback(
    (colIndex: number, e: React.PointerEvent<HTMLDivElement>) => {
      if (!useMobileHandles) return;

      const column = columns[colIndex];
      const grid = gridRef.current;
      if (!column || !grid) return;

      const bounds = grid.getBounds(colIndex, -1);
      if (!bounds) return;

      e.preventDefault();
      e.stopPropagation();
      e.currentTarget.setPointerCapture(e.pointerId);

      dragRef.current = {
        colIndex,
        startX: e.clientX,
        startWidth: bounds.width,
      };
      setResizingColIndex(colIndex);
      document.body.classList.add("ivy-datatable-column-resize-active");
    },
    [columns, gridRef, useMobileHandles],
  );

  return {
    useMobileHandles,
    handles,
    onHandlePointerDown,
  };
}
