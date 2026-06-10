import { useCallback, useMemo, useState, type CSSProperties } from "react";
import { GridCellKind, GridMouseEventArgs, Item } from "@glideapps/glide-data-grid";
import { GridCell } from "@glideapps/glide-data-grid";
import { Densities } from "@/types/density";
import { getCellFont } from "../utils/canvasText";
import { getTruncatedCellTooltip } from "../utils/cellTooltip";
import { DENSITY_CONFIG } from "../dataTableEditor/constants";
import { DataColumn } from "../types/types";

interface UseCellHoverTooltipProps {
  columns: DataColumn[];
  columnOrder: number[];
  density: Densities;
  getCellContent: (cell: Item) => GridCell;
  visibleRows: number;
}

export interface CellHoverTooltipState {
  x: number;
  y: number;
  cellHeight: number;
  content: string;
  /** Secondary line (e.g. link open hint) */
  hint?: string;
}

type CellTooltipPlacement = "above" | "below";

interface CellTooltipLayout {
  cellGap: number;
  viewportInset: number;
}

const defaultTooltipLayout: CellTooltipLayout = {
  cellGap: 8,
  viewportInset: 8,
};

/** Positions tooltip above or below the cell based on available viewport space. */
export function getCellTooltipPlacementStyle(
  tooltip: CellHoverTooltipState,
  viewportHeight: number,
  layout: CellTooltipLayout = defaultTooltipLayout,
): CSSProperties {
  const { cellGap, viewportInset } = layout;
  const cellBottom = tooltip.y + tooltip.cellHeight;
  const availableAbove = tooltip.y - cellGap - viewportInset;
  const availableBelow = viewportHeight - cellBottom - cellGap - viewportInset;

  const placement: CellTooltipPlacement = availableAbove >= availableBelow ? "above" : "below";

  if (placement === "above") {
    return {
      left: tooltip.x,
      top: tooltip.y - cellGap,
      transform: "translate(-50%, -100%)",
      maxHeight: Math.max(0, availableAbove),
    };
  }

  return {
    left: tooltip.x,
    top: cellBottom + cellGap,
    transform: "translate(-50%, 0)",
    maxHeight: Math.max(0, availableBelow),
  };
}

/** Tooltip hover relies on real hover; skip on touch / coarse pointers where it mispositions */
function canShowCellHoverTooltip(): boolean {
  if (typeof window === "undefined") return false;
  return (
    window.matchMedia("(hover: hover)").matches && window.matchMedia("(pointer: fine)").matches
  );
}

export const useCellHoverTooltip = ({
  columns,
  columnOrder,
  density,
  getCellContent,
  visibleRows,
}: UseCellHoverTooltipProps) => {
  const tooltipSupported = useMemo(() => canShowCellHoverTooltip(), []);
  const cellFont = useMemo(() => getCellFont(density), [density]);
  const cellHorizontalPadding = DENSITY_CONFIG[density].cellHorizontalPadding;

  const [tooltip, setTooltip] = useState<CellHoverTooltipState | null>(null);

  const cellTooltipStyle = useMemo(() => {
    if (!tooltip) return null;
    const viewportHeight = typeof window !== "undefined" ? window.innerHeight : 0;
    return getCellTooltipPlacementStyle(tooltip, viewportHeight);
  }, [tooltip]);

  const onItemHovered = useCallback(
    (args: GridMouseEventArgs) => {
      if (!tooltipSupported) {
        setTooltip(null);
        return;
      }
      if (args.kind !== "cell") {
        setTooltip(null);
        return;
      }

      const [col, row] = args.location;
      if (row >= visibleRows) {
        setTooltip(null);
        return;
      }

      const orderedColumns =
        columnOrder.length === columns.length
          ? columnOrder.map((idx) => columns[idx]).filter((c) => c && !c.hidden)
          : columns.filter((c) => !c.hidden);
      const column = orderedColumns[col];
      if (!column) {
        setTooltip(null);
        return;
      }

      const cell = getCellContent(args.location);
      const columnWidth = args.bounds.width;

      const truncatedText = getTruncatedCellTooltip(
        cell,
        columnWidth,
        cellHorizontalPadding,
        cellFont,
        column.wrapText,
      );

      const cellData =
        cell.kind === GridCellKind.Custom
          ? (cell.data as { kind?: string; url?: string })
          : undefined;
      const isLinkCell = cellData?.kind === "link-cell" && !!cellData?.url;

      if (!truncatedText && !isLinkCell) {
        setTooltip(null);
        return;
      }

      const isMac = typeof navigator !== "undefined" && /Mac|iPhone|iPad/.test(navigator.platform);
      const linkHint = isMac ? "\u2318+click to open link" : "Ctrl+click to open link";

      setTooltip({
        x: args.bounds.x + args.bounds.width / 2,
        y: args.bounds.y,
        cellHeight: args.bounds.height,
        content: truncatedText ?? (isLinkCell ? linkHint : ""),
        hint: truncatedText && isLinkCell ? linkHint : undefined,
      });
    },
    [
      tooltipSupported,
      columns,
      columnOrder,
      cellFont,
      cellHorizontalPadding,
      getCellContent,
      visibleRows,
    ],
  );

  const clearCellHoverTooltip = useCallback(() => {
    setTooltip(null);
  }, []);

  return {
    cellTooltip: tooltip,
    cellTooltipStyle,
    supportsHoverTooltip: tooltipSupported,
    onItemHovered,
    clearCellHoverTooltip,
  };
};
