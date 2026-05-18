import { GridCell, GridCellKind } from "@glideapps/glide-data-grid";
import { getMaxTextWidth, truncateTextWithEllipsis } from "./canvasText";
import type { AnimatedStatusCellData, LinkCellData } from "./customRenderers";
import { formatNumberValue } from "./cellContent";

/** Full single-line label shown in a cell before ellipsis clipping. */
export function getCellDisplayLabel(cell: GridCell): string | null {
  if (cell.kind === GridCellKind.Text) {
    const value = cell.data;
    if (value == null || value === "") return null;
    return String(value);
  }

  if (cell.kind === GridCellKind.Number) {
    if (cell.data == null) return null;
    return formatNumberValue(cell.data);
  }

  if (cell.kind === GridCellKind.Custom) {
    const data = cell.data as { kind?: string } | undefined;
    if (data?.kind === "link-cell") {
      const link = data as LinkCellData;
      return link.text || link.url || null;
    }
    if (data?.kind === "animated-status-cell") {
      const status = data as AnimatedStatusCellData;
      if (!status.statusText) return null;
      return status.rightLabel ? `${status.statusText} (${status.rightLabel})` : status.statusText;
    }
  }

  return null;
}

/**
 * Returns tooltip text when the cell value would not fit at the given column width.
 */
export function getTruncatedCellTooltip(
  cell: GridCell,
  columnWidth: number | undefined,
  cellHorizontalPadding: number,
  cellFont: string,
  wrapText?: boolean,
): string | null {
  if (wrapText || columnWidth === undefined) return null;

  const full = getCellDisplayLabel(cell);
  if (!full) return null;

  const maxWidth = getMaxTextWidth(columnWidth, cellHorizontalPadding);
  if (maxWidth <= 0) return null;

  const truncated = truncateTextWithEllipsis(full, maxWidth, cellFont);
  if (truncated === full) return null;

  return full;
}
