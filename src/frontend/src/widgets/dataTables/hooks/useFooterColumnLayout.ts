import { useCallback, useLayoutEffect, useMemo, useRef } from "react";
import type { RefObject } from "react";
import {
  AllCellRenderers,
  blend,
  getDefaultTheme,
  GridCellKind,
  useColumnSizer,
  type CustomCell,
  type GridCell,
  type GridColumn,
  type InnerGridCell,
  type Item,
  type Theme,
} from "@glideapps/glide-data-grid";
import { iconCellRenderer, linkCellRenderer } from "../utils/customRenderers";

/**
 * Same behavior as Glide's mergeAndRealizeTheme (common/styles.js), using only
 * public exports — deep imports break production resolve (package exports field).
 */
function mergeAndRealizeTheme(
  theme: Theme,
  ...overlays: Partial<Theme | undefined>[]
): Parameters<typeof useColumnSizer>[6] {
  const merged = { ...theme } as Theme & {
    headerFontFull?: string;
    baseFontFull?: string;
    markerFontFull?: string;
  };
  for (const overlay of overlays) {
    if (overlay !== undefined) {
      for (const key in overlay) {
        if (Object.prototype.hasOwnProperty.call(overlay, key)) {
          if (key === "bgCell") {
            merged.bgCell = blend(
              overlay.bgCell as string,
              merged.bgCell as string,
            ) as Theme["bgCell"];
          } else {
            (merged as unknown as Record<string, unknown>)[key] = (
              overlay as Record<string, unknown>
            )[key];
          }
        }
      }
    }
  }
  if (
    merged.headerFontFull === undefined ||
    theme.fontFamily !== merged.fontFamily ||
    theme.headerFontStyle !== merged.headerFontStyle
  ) {
    merged.headerFontFull = `${merged.headerFontStyle} ${merged.fontFamily}`;
  }
  if (
    merged.baseFontFull === undefined ||
    theme.fontFamily !== merged.fontFamily ||
    theme.baseFontStyle !== merged.baseFontStyle
  ) {
    merged.baseFontFull = `${merged.baseFontStyle} ${merged.fontFamily}`;
  }
  if (
    merged.markerFontFull === undefined ||
    theme.fontFamily !== merged.fontFamily ||
    theme.markerFontStyle !== merged.markerFontStyle
  ) {
    merged.markerFontFull = `${merged.markerFontStyle} ${merged.fontFamily}`;
  }
  return merged as Parameters<typeof useColumnSizer>[6];
}

export interface FooterColumnLayout {
  markerWidth: number;
  columnWidths: number[];
}

/** Same rule as Glide DataEditor for row marker column width */
function rowMarkerWidthForRowCount(visibleRows: number): number {
  if (visibleRows > 10_000) return 48;
  if (visibleRows > 1000) return 44;
  if (visibleRows > 100) return 36;
  return 32;
}

/** Vertical scrollbar reduces usable width — matches Glide clientSize[2] usage */
function verticalScrollbarWidth(): number {
  if (typeof window === "undefined") return 0;
  return Math.max(0, window.innerWidth - document.documentElement.clientWidth);
}

/**
 * Footer widths use the same `useColumnSizer` pipeline as Glide DataEditor, so pixel widths,
 * grow distribution, and min/max bounds match the grid exactly.
 */
export function useFooterColumnLayout(
  containerRef: RefObject<HTMLDivElement | null>,
  gridColumns: readonly GridColumn[],
  getCellContent: (cell: Item) => GridCell,
  totalRows: number,
  containerWidth: number,
  tableTheme: Partial<Theme> | undefined,
  showIndexColumn: boolean,
  visibleRows: number,
  enabled: boolean,
): {
  layout: FooterColumnLayout;
  footerScrollRef: RefObject<HTMLDivElement | null>;
} {
  const footerScrollRef = useRef<HTMLDivElement | null>(null);
  const abortControllerRef = useRef(new AbortController());

  const mergedTheme = useMemo(
    () => mergeAndRealizeTheme(getDefaultTheme(), tableTheme ?? {}),
    [tableTheme],
  );

  const rowMarkerWidth = showIndexColumn ? rowMarkerWidthForRowCount(visibleRows) : 0;

  const clientWidthForDataColumns = useMemo(() => {
    if (containerWidth <= 0) return 0;
    return Math.max(
      0,
      containerWidth - (showIndexColumn ? rowMarkerWidth : 0) - verticalScrollbarWidth(),
    );
  }, [containerWidth, showIndexColumn, rowMarkerWidth]);

  const getCellsForSelectionDirect = useCallback(
    (rect: { x: number; y: number; width: number; height: number }) => {
      const result: GridCell[][] = [];
      for (let y = rect.y; y < rect.y + rect.height; y++) {
        const row: GridCell[] = [];
        for (let x = rect.x; x < rect.x + rect.width; x++) {
          if (x < 0 || y >= totalRows) {
            row.push({ kind: GridCellKind.Loading, allowOverlay: false });
          } else {
            row.push(getCellContent([x, y]));
          }
        }
        result.push(row);
      }
      return result;
    },
    [getCellContent, totalRows],
  );

  const rendererMap = useMemo(() => {
    const m = new Map<number, (typeof AllCellRenderers)[number]>();
    for (const r of AllCellRenderers) {
      m.set(r.kind as unknown as number, r);
    }
    return m;
  }, []);

  const getCellRenderer = useCallback(
    (cell: InnerGridCell) => {
      if (cell.kind !== GridCellKind.Custom) {
        return rendererMap.get(cell.kind as unknown as number);
      }
      const c = cell as CustomCell<Record<string, unknown>>;
      if (iconCellRenderer.isMatch(c)) return iconCellRenderer;
      if (linkCellRenderer.isMatch(c)) return linkCellRenderer;
      return undefined;
    },
    [rendererMap],
  );

  const columnsForSizer = enabled && gridColumns.length > 0 ? gridColumns : [];
  const rowsForSizer = enabled ? totalRows : 0;

  const { sizedColumns } = useColumnSizer(
    columnsForSizer,
    rowsForSizer,
    getCellsForSelectionDirect,
    clientWidthForDataColumns,
    50,
    500,
    mergedTheme,
    getCellRenderer as Parameters<typeof useColumnSizer>[7],
    abortControllerRef.current,
  );

  const layout = useMemo((): FooterColumnLayout => {
    if (!enabled || sizedColumns.length === 0) {
      return { markerWidth: 0, columnWidths: [] };
    }
    return {
      markerWidth: rowMarkerWidth,
      columnWidths: sizedColumns.map((c) => c.width),
    };
  }, [enabled, sizedColumns, rowMarkerWidth]);

  useLayoutEffect(() => {
    if (!enabled || columnsForSizer.length === 0) return;

    const sync = () => {
      const container = containerRef.current;
      const footerEl = footerScrollRef.current;
      if (!container || !footerEl) return;
      const scroller = container.querySelector(".dvn-scroller");
      if (!scroller) return;
      footerEl.scrollLeft = scroller.scrollLeft;
    };

    sync();
    const scroller = containerRef.current?.querySelector(".dvn-scroller");
    scroller?.addEventListener("scroll", sync, { passive: true });
    window.addEventListener("resize", sync);
    const raf = requestAnimationFrame(sync);

    return () => {
      cancelAnimationFrame(raf);
      containerRef.current?.querySelector(".dvn-scroller")?.removeEventListener("scroll", sync);
      window.removeEventListener("resize", sync);
    };
  }, [containerRef, enabled, layout, columnsForSizer.length]);

  return { layout, footerScrollRef };
}
