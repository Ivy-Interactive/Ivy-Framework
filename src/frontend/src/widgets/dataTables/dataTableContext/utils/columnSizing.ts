/**
 * Parses a Size string (e.g., "Px:200") to a numeric pixel value.
 * If input is already a number, returns it as-is.
 * For grow/proportional types (Fraction, Auto, Grow, Full, Screen), returns 0
 * so that downstream code can use header-based minimum widths.
 */
export function parseSize(size: number | string | undefined): number {
  if (typeof size === "number") return size;
  if (!size) return 150; // default width

  // Size strings may have min/max: "Fraction:0.5,Px:100,Px:500" — use only the first part
  const primary = size.split(",")[0];
  const [sizeType, rawValue] = primary.split(":");
  const value = rawValue ? parseFloat(rawValue) : undefined;

  switch (sizeType) {
    case "Px":
      return value ?? 150;
    case "Rem":
      return (value ?? 1) * 16;
    case "Units":
      return (value ?? 1) * 4; // 1 unit = 0.25rem = 4px (matches styles.ts)
    // All grow/proportional types return 0 so header minimum is used downstream
    case "Fraction":
    case "Grow":
    case "Full":
    case "Auto":
    case "Screen":
      return 0;
    // Content-fitting types: no fixed width, rely on header estimate downstream
    case "Fit":
    case "MinContent":
    case "MaxContent":
      return 0;
    // Shrink: not meaningful for grid columns, use 0 for header estimate
    case "Shrink":
      return 0;
    default:
      return 150; // fallback to default
  }
}

/**
 * Extracts the grow factor for Glide Data Grid's `grow` property from a Size string.
 * Returns undefined for fixed/content types that don't grow.
 */
export function parseSizeGrow(size: number | string | undefined): number | undefined {
  if (typeof size === "number" || !size) return undefined;

  const primary = size.split(",")[0];
  const [sizeType, rawValue] = primary.split(":");
  const value = rawValue ? parseFloat(rawValue) : undefined;

  switch (sizeType) {
    case "Fraction":
      return value ?? 1;
    case "Grow":
      return value ?? 1;
    case "Full":
      return 1;
    case "Auto":
      return 1;
    case "Screen":
      return 1; // 100vw not meaningful for columns, treat as fill
    // Fixed and content types don't grow
    case "Px":
    case "Rem":
    case "Units":
    case "Fit":
    case "MinContent":
    case "MaxContent":
    case "Shrink":
    default:
      return undefined;
  }
}

/**
 * Extracts the minimum width constraint from the min/max portion of a Size string.
 * Size format: "Primary,Min,Max" — e.g., "Fraction:0.5,Px:100,Px:500"
 */
export function parseSizeMin(size: number | string | undefined): number | undefined {
  if (typeof size === "number" || !size) return undefined;

  const parts = size.split(",");
  if (parts.length < 2 || !parts[1]) return undefined;

  // The second part is the min constraint (e.g., "Px:100")
  return parseSize(parts[1]) || undefined;
}

let _measureCanvas: HTMLCanvasElement | null = null;

function getMeasureContext(): CanvasRenderingContext2D | null {
  if (!_measureCanvas) {
    _measureCanvas = document.createElement("canvas");
  }
  return _measureCanvas.getContext("2d");
}

/**
 * Estimates minimum column width needed to display header text without truncation.
 * When a font string is provided, uses canvas.measureText() for accurate width;
 * otherwise falls back to approximate character width (8px per char).
 */
export function estimateHeaderWidth(headerText: string, font?: string): number {
  const HEADER_PADDING = 40; // padding for icon, sort arrow, cell padding
  const MIN_WIDTH = 60; // absolute minimum
  const MAX_AUTO_WIDTH = 300; // cap auto-width to prevent excessively wide columns

  let textWidth: number;
  const ctx = font ? getMeasureContext() : null;
  if (ctx && font) {
    ctx.font = font;
    textWidth = ctx.measureText(headerText).width;
  } else {
    textWidth = headerText.length * 8; // fallback
  }

  const estimated = textWidth + HEADER_PADDING;
  return Math.max(MIN_WIDTH, Math.min(estimated, MAX_AUTO_WIDTH));
}

import type { Table } from "apache-arrow";

const CONTENT_PADDING = 24;
const MIN_CONTENT_WIDTH = 60;
const MAX_CONTENT_WIDTH = 400;
const SAMPLE_SIZE = 20;

/**
 * Estimates column width by sampling the first N rows of actual cell data.
 * When a font string is provided, uses canvas.measureText() for accurate width;
 * otherwise falls back to approximate character width (8px per char).
 * Returns undefined if no data is available or column not found.
 */
export function estimateContentWidth(
  arrowTable: Table,
  columnIndex: number,
  mode: "fit" | "min" | "max",
  font?: string,
): number | undefined {
  if (!arrowTable || columnIndex < 0 || columnIndex >= arrowTable.numCols) {
    return undefined;
  }

  const column = arrowTable.getChildAt(columnIndex);
  if (!column) return undefined;

  const rowCount = Math.min(arrowTable.numRows, SAMPLE_SIZE);
  if (rowCount === 0) return undefined;

  const ctx = font ? getMeasureContext() : null;
  if (ctx && font) {
    ctx.font = font;
  }

  const widths: number[] = [];
  for (let i = 0; i < rowCount; i++) {
    const value = column.get(i);
    const text = value == null ? "" : String(value);
    const textWidth = ctx ? ctx.measureText(text).width : text.length * 8;
    const width = textWidth + CONTENT_PADDING;
    widths.push(Math.max(MIN_CONTENT_WIDTH, Math.min(width, MAX_CONTENT_WIDTH)));
  }

  switch (mode) {
    case "min":
      return Math.min(...widths);
    case "max":
      return Math.max(...widths);
    case "fit":
      return Math.max(...widths);
  }
}

/**
 * Returns the content-sizing mode for a Size string, or undefined if not content-based.
 */
export function getSizeMode(size: string | number | undefined): "fit" | "min" | "max" | undefined {
  if (typeof size !== "string") return undefined;
  const sizeType = size.split(",")[0].split(":")[0];
  switch (sizeType) {
    case "Fit":
      return "fit";
    case "MinContent":
      return "min";
    case "MaxContent":
      return "max";
    default:
      return undefined;
  }
}
