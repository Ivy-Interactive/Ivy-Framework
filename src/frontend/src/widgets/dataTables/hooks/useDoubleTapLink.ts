import { useEffect, useRef, type RefObject } from "react";
import {
  DataEditorRef,
  GridCell,
  GridCellKind,
  GridColumn,
  Item,
} from "@glideapps/glide-data-grid";
import { openLinkUrl } from "./useCellInteractions";

/** Two taps on the same link cell within this window open the URL */
const DOUBLE_TAP_MS = 350;

interface UseDoubleTapLinkProps {
  containerRef: RefObject<HTMLDivElement | null>;
  gridRef: RefObject<DataEditorRef | null>;
  getCellContent: (cell: Item) => GridCell;
  columns: GridColumn[];
  headerHeight: number;
  rowHeight: number;
  visibleRows: number;
}

function resolveCellFromPointer(
  container: HTMLDivElement,
  grid: DataEditorRef | null,
  columns: GridColumn[],
  headerHeight: number,
  rowHeight: number,
  visibleRows: number,
  clientX: number,
  clientY: number,
): Item | null {
  const containerRect = container.getBoundingClientRect();
  const localY = clientY - containerRect.top;
  const row = Math.floor((localY - headerHeight) / rowHeight);
  if (row < 0 || row >= visibleRows) return null;

  let col = -1;
  if (grid) {
    for (let c = 0; c < columns.length; c++) {
      const bounds = grid.getBounds(c, 0);
      if (!bounds) break;
      if (clientX >= bounds.x && clientX < bounds.x + bounds.width) {
        col = c;
        break;
      }
    }
  }
  if (col === -1) return null;
  return [col, row];
}

export function useDoubleTapLink({
  containerRef,
  gridRef,
  getCellContent,
  columns,
  headerHeight,
  rowHeight,
  visibleRows,
}: UseDoubleTapLinkProps) {
  const propsRef = useRef({ getCellContent, columns, headerHeight, rowHeight, visibleRows });
  propsRef.current = { getCellContent, columns, headerHeight, rowHeight, visibleRows };

  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    let lastTap: { key: string; time: number } | null = null;

    const onPointerUp = (e: PointerEvent) => {
      if (e.pointerType === "mouse") return;

      const grid = gridRef.current;
      const { getCellContent, columns, headerHeight, rowHeight, visibleRows } = propsRef.current;

      const cell = resolveCellFromPointer(
        container,
        grid,
        columns,
        headerHeight,
        rowHeight,
        visibleRows,
        e.clientX,
        e.clientY,
      );
      if (!cell) return;

      const cellContent = getCellContent(cell);
      if (
        cellContent.kind !== GridCellKind.Custom ||
        (cellContent.data as { kind?: string })?.kind !== "link-cell"
      ) {
        lastTap = null;
        return;
      }

      const url = (cellContent.data as { url?: string })?.url;
      const key = `${cell[0]},${cell[1]}`;
      const now = Date.now();

      if (lastTap?.key === key && now - lastTap.time <= DOUBLE_TAP_MS) {
        openLinkUrl(url);
        lastTap = null;
      } else {
        lastTap = { key, time: now };
      }
    };

    container.addEventListener("pointerup", onPointerUp);

    return () => {
      container.removeEventListener("pointerup", onPointerUp);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [containerRef, gridRef]);
}
