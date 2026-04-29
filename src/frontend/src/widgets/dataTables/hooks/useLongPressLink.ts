import { useEffect, useRef } from "react";
import {
  DataEditorRef,
  GridCell,
  GridCellKind,
  GridColumn,
  Item,
} from "@glideapps/glide-data-grid";
import { openLinkUrl } from "./useCellInteractions";

const LONG_PRESS_DELAY = 500;
const MOVE_THRESHOLD = 10;

interface UseLongPressLinkProps {
  containerRef: React.RefObject<HTMLDivElement | null>;
  gridRef: React.RefObject<DataEditorRef | null>;
  getCellContent: (cell: Item) => GridCell;
  columns: GridColumn[];
  headerHeight: number;
  rowHeight: number;
  visibleRows: number;
}

export function useLongPressLink({
  containerRef,
  gridRef,
  getCellContent,
  columns,
  headerHeight,
  rowHeight,
  visibleRows,
}: UseLongPressLinkProps) {
  const propsRef = useRef({ getCellContent, columns, headerHeight, rowHeight, visibleRows });
  propsRef.current = { getCellContent, columns, headerHeight, rowHeight, visibleRows };

  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    let timer: ReturnType<typeof setTimeout> | null = null;
    let startX = 0;
    let startY = 0;

    const clearTimer = () => {
      if (timer !== null) {
        clearTimeout(timer);
        timer = null;
      }
    };

    const onPointerDown = (e: PointerEvent) => {
      if (e.pointerType === "mouse") return;

      clearTimer();
      startX = e.clientX;
      startY = e.clientY;

      timer = setTimeout(() => {
        timer = null;
        const grid = gridRef.current;
        const { getCellContent, columns, headerHeight, rowHeight, visibleRows } = propsRef.current;

        const containerRect = container.getBoundingClientRect();
        const localY = startY - containerRect.top;
        const row = Math.floor((localY - headerHeight) / rowHeight);
        if (row < 0 || row >= visibleRows) return;

        // getBounds returns viewport-relative coords — compare directly with clientX
        let col = -1;
        if (grid) {
          for (let c = 0; c < columns.length; c++) {
            const bounds = grid.getBounds(c, 0);
            if (!bounds) break;
            if (startX >= bounds.x && startX < bounds.x + bounds.width) {
              col = c;
              break;
            }
          }
        }
        if (col === -1) return;

        const cell: Item = [col, row];
        const cellContent = getCellContent(cell);
        if (
          cellContent.kind === GridCellKind.Custom &&
          (cellContent.data as { kind?: string })?.kind === "link-cell"
        ) {
          const url = (cellContent.data as { url?: string })?.url;
          openLinkUrl(url);
        }
      }, LONG_PRESS_DELAY);
    };

    const onPointerMove = (e: PointerEvent) => {
      if (e.pointerType === "mouse") return;
      const dx = e.clientX - startX;
      const dy = e.clientY - startY;
      if (Math.sqrt(dx * dx + dy * dy) > MOVE_THRESHOLD) {
        clearTimer();
      }
    };

    const onPointerUp = () => clearTimer();
    const onPointerCancel = () => clearTimer();

    container.addEventListener("pointerdown", onPointerDown);
    container.addEventListener("pointermove", onPointerMove);
    container.addEventListener("pointerup", onPointerUp);
    container.addEventListener("pointercancel", onPointerCancel);

    return () => {
      clearTimer();
      container.removeEventListener("pointerdown", onPointerDown);
      container.removeEventListener("pointermove", onPointerMove);
      container.removeEventListener("pointerup", onPointerUp);
      container.removeEventListener("pointercancel", onPointerCancel);
    };
    // containerRef and gridRef are stable refs — no need to include in deps
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [containerRef, gridRef]);
}