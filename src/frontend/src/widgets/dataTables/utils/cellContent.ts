import { GridCell, GridCellKind, Item, Theme } from "@glideapps/glide-data-grid";
import { Align, DataColumn, DataRow } from "../types/types";
import { getCSSVariable } from "@/lib/theme";
import type { LabelsBadgesCellData } from "./customRenderers";

/**
 * Converts Align enum to contentAlign value for GridCell
 */
export function getContentAlign(align?: Align): "left" | "center" | "right" {
  if (!align) return "left";

  switch (align) {
    case "Left":
      return "left";
    case "Center":
      return "center";
    case "Right":
      return "right";
    default:
      return "left";
  }
}

/**
 * Creates an empty/fallback cell for out-of-bounds requests
 */
export function createEmptyCell(): GridCell {
  return {
    kind: GridCellKind.Text,
    data: "",
    displayData: "",
    allowOverlay: false,
    readonly: true,
  };
}

/**
 * Creates a cell for null or undefined values
 */
export function createNullCell(editable: boolean): GridCell {
  return {
    kind: GridCellKind.Text,
    data: "",
    displayData: "", // Show empty instead of "null" text
    allowOverlay: editable,
    readonly: !editable,
    style: "faded",
  };
}

/**
 * Checks if a string value looks like a Lucide icon name (PascalCase)
 * @deprecated Use column type ColType.Icon instead of heuristics
 */
export function isProbablyIconValue(value: unknown): boolean {
  return (
    typeof value === "string" &&
    /^[A-Z][a-zA-Z0-9]*$/.test(value) &&
    value.length > 2 &&
    !value.includes(" ")
  );
}

/**
 * Creates an icon cell
 */
export function createIconCell(iconName: string, align?: Align): GridCell {
  return {
    kind: GridCellKind.Custom,
    allowOverlay: false,
    readonly: true,
    copyData: iconName,
    data: {
      kind: "icon-cell",
      iconName,
      align: align ? getContentAlign(align) : undefined,
    },
  };
}

/**
 * Checks if a column type represents a date/timestamp
 */
export function isDateColumnType(columnType: string): boolean {
  return columnType.includes("date") || columnType.includes("timestamp");
}

/**
 * Checks if a column type represents a numeric type
 */
export function isNumericColumnType(columnType: string): boolean {
  return (
    columnType.includes("int") ||
    columnType.includes("float") ||
    columnType.includes("double") ||
    columnType.includes("decimal") ||
    columnType.includes("number")
  );
}

/**
 * Formats a date value for display
 */
export function formatDateValue(dateValue: Date, columnType: string): string {
  const hasTime =
    columnType.includes("datetime") ||
    columnType.includes("timestamp") ||
    dateValue.getHours() !== 0 ||
    dateValue.getMinutes() !== 0 ||
    dateValue.getSeconds() !== 0;

  return hasTime ? dateValue.toLocaleString() : dateValue.toLocaleDateString();
}

/**
 * Parses a date from various input formats
 */
export function parseDateValue(cellValue: unknown): Date | null {
  // Handle Date objects directly (from Arrow Timestamp vectors)
  if (cellValue instanceof Date) {
    return !isNaN(cellValue.getTime()) ? cellValue : null;
  }

  if (typeof cellValue === "number") {
    const date = new Date(cellValue);
    return !isNaN(date.getTime()) ? date : null;
  }

  if (typeof cellValue === "string") {
    const date = new Date(cellValue);
    return !isNaN(date.getTime()) ? date : null;
  }

  return null;
}

/**
 * Creates a date/timestamp cell
 */
export function createDateCell(
  cellValue: unknown,
  columnType: string,
  editable: boolean,
  align?: Align,
): GridCell | null {
  const dateValue = parseDateValue(cellValue);

  if (!dateValue) {
    return null;
  }

  const displayData = formatDateValue(dateValue, columnType);

  return {
    kind: GridCellKind.Text,
    data: displayData,
    displayData,
    allowOverlay: editable,
    readonly: !editable,
    contentAlign: align ? getContentAlign(align) : undefined,
  };
}

/**
 * Formats a number for display
 */
export function formatNumberValue(value: number): string {
  return Number.isInteger(value) ? value.toString() : value.toFixed(2);
}

/**
 * Creates a numeric cell
 */
export function createNumberCell(cellValue: number, editable: boolean, align?: Align): GridCell {
  const displayData = formatNumberValue(cellValue);

  return {
    kind: GridCellKind.Number,
    data: cellValue,
    displayData,
    allowOverlay: editable,
    readonly: !editable,
    contentAlign: align ? getContentAlign(align) : undefined,
  };
}

/**
 * Creates a boolean cell
 */
export function createBooleanCell(cellValue: boolean, editable: boolean, align?: Align): GridCell {
  return {
    kind: GridCellKind.Boolean,
    data: cellValue,
    allowOverlay: false,
    readonly: !editable,
    contentAlign: align ? getContentAlign(align) : undefined,
  };
}

/**
 * Creates a text cell
 */
export function createTextCell(cellValue: unknown, editable: boolean, align?: Align): GridCell {
  const stringValue = String(cellValue);

  return {
    kind: GridCellKind.Text,
    data: stringValue,
    displayData: stringValue,
    allowOverlay: editable,
    readonly: !editable,
    contentAlign: align ? getContentAlign(align) : undefined,
  };
}

export function lookupBadgeColorMapping(
  mapping: Record<string, string> | null | undefined,
  label: string,
): string | undefined {
  if (!mapping) return undefined;
  if (mapping[label]) return mapping[label];
  const lower = label.toLowerCase();
  for (const [key, value] of Object.entries(mapping)) {
    if (key.toLowerCase() === lower) return value;
  }
  return undefined;
}

/**
 * Creates a labels/bubble cell for displaying multiple labels as chips
 */
export function createLabelsCell(
  cellValue: unknown,
  align?: Align,
  color?: string | null,
  customColor?: string | null,
  badgeColorMapping?: Record<string, string> | null,
): GridCell {
  // Handle different input formats
  let labels: readonly string[];

  if (Array.isArray(cellValue)) {
    labels = cellValue.filter((item) => item != null).map(String);
  } else if (typeof cellValue === "string") {
    // Try to parse as JSON first (from backend serialization)
    try {
      const parsed = JSON.parse(cellValue);
      if (Array.isArray(parsed)) {
        labels = parsed.filter((item) => item != null).map(String);
      } else {
        // Fallback to comma-separated if JSON parsing doesn't yield an array
        labels = cellValue
          .split(",")
          .map((s) => s.trim())
          .filter((s) => s.length > 0);
      }
    } catch {
      // Not JSON, treat as comma-separated string
      labels = cellValue
        .split(",")
        .map((s) => s.trim())
        .filter((s) => s.length > 0);
    }
  } else if (cellValue != null) {
    labels = [String(cellValue)];
  } else {
    labels = [];
  }

  const contentAlign = align === "Left" ? "left" : align === "Right" ? "right" : "center";

  // Per-label colors require a custom renderer: Bubble cells share one theme for all bubbles.
  if (badgeColorMapping && labels.length > 1) {
    const items = labels.map((label) => {
      const raw = lookupBadgeColorMapping(badgeColorMapping, label) ?? customColor ?? color ?? null;
      const { bg, text } = resolveBadgeColor(raw);
      return { text: label, bg, fg: text };
    });
    const data: LabelsBadgesCellData = {
      kind: "labels-badges-cell",
      items,
      align: contentAlign,
    };
    return {
      kind: GridCellKind.Custom,
      data,
      allowOverlay: false,
      readonly: true,
      copyData: labels.join(", "),
      contentAlign,
    };
  }

  // Resolve effective color from mapping if possible
  let effectiveColor = customColor || color;
  if (!effectiveColor && badgeColorMapping && labels.length > 0) {
    for (const label of labels) {
      const mapped = lookupBadgeColorMapping(badgeColorMapping, label);
      if (mapped) {
        effectiveColor = mapped;
        break;
      }
    }
  }

  const themeOverride: Partial<Theme> = {};
  if (effectiveColor) {
    const { bg, text } = resolveBadgeColor(effectiveColor);
    if (bg) themeOverride.bgBubble = bg;
    if (text) themeOverride.textBubble = text;
  }

  return {
    kind: GridCellKind.Bubble,
    data: labels as string[],
    allowOverlay: false,
    themeOverride,
    contentAlign,
  };
}

/**
 * Creates a link cell with custom renderer (blue text + underline)
 */
export function createLinkCell(
  url: string,
  _editable: boolean, // Intentionally unused - links are always readonly
  align?: Align,
): GridCell {
  return {
    kind: GridCellKind.Custom,
    data: {
      kind: "link-cell",
      url: url,
      align: align?.toLowerCase() as "left" | "center" | "right" | undefined,
    },
    copyData: url,
    allowOverlay: false,
    readonly: true,
    cursor: "pointer",
  };
}

/**
 * Gets the ordered columns based on columnOrder array
 */
export function getOrderedColumns(columns: DataColumn[], columnOrder: number[]): DataColumn[] {
  return columnOrder.length === columns.length ? columnOrder.map((idx) => columns[idx]) : columns;
}

/**
 * Main function to get cell content for a grid cell
 * Filters out hidden columns and applies column ordering
 * Uses Arrow table via getRowData for efficient access to gRPC data
 */
export function getCellContent(
  cell: Item,
  columns: DataColumn[],
  columnOrder: number[],
  editable: boolean,
  getRowData: (rowIndex: number) => DataRow | null,
): GridCell {
  const [col, row] = cell;

  // Apply column order first, then filter out hidden columns
  let orderedCols: DataColumn[];
  if (columnOrder.length === columns.length) {
    // Map using columnOrder indices, then filter hidden
    orderedCols = columnOrder.map((idx) => columns[idx]).filter((col) => !col.hidden);
  } else {
    // No reordering, just filter hidden columns
    orderedCols = columns.filter((col) => !col.hidden);
  }

  // Get row data from Arrow table via getRowData
  const rowData = getRowData(row);

  // Safety check
  if (!rowData || col >= orderedCols.length) {
    return createEmptyCell();
  }
  const column = orderedCols[col];
  const originalColumnIndex = columns.indexOf(column);
  const cellValue = rowData.values[originalColumnIndex];
  const columnType = column.type?.toLowerCase() || "text";
  const align = column.alignContent;

  // Handle null/undefined values
  if (cellValue === null || cellValue === undefined) {
    return createNullCell(editable);
  }

  // Handle explicit icon type from backend metadata
  if (column.type === "Icon" && typeof cellValue === "string") {
    return createIconCell(cellValue, align);
  }

  // Handle Labels type - supports arrays or comma-separated strings
  if (column.type === "Labels") {
    return createLabelsCell(
      cellValue,
      align,
      column.color,
      column.customColor,
      column.badgeColorMapping,
    );
  }

  // Handle explicit link type from backend metadata
  if (column.type === "Link" && typeof cellValue === "string") {
    return createLinkCell(cellValue, editable, align);
  }

  // Handle Date and DateTime types
  if (isDateColumnType(columnType)) {
    const dateCell = createDateCell(cellValue, columnType, editable, align);
    if (dateCell) {
      return dateCell;
    }
  }

  // Handle numeric types
  if (typeof cellValue === "number" && isNumericColumnType(columnType)) {
    return createNumberCell(cellValue, editable, align);
  }

  // Handle boolean types
  if (typeof cellValue === "boolean") {
    return createBooleanCell(cellValue, editable, align);
  }

  // REMOVED: Heuristic icon detection (now that we have proper type metadata from backend)
  // The heuristic was causing false positives for PascalCase text like "Active", "EMP0001", etc.
  // Now that column.type is properly preserved from backend (#1273), we don't need this fallback

  // Default to text
  return createTextCell(cellValue, editable, align);
}

/**
 * Resolves a color name or custom color string to background and text colors
 */
export function resolveBadgeColor(colorValue: string | null | undefined): {
  bg: string | undefined;
  text: string | undefined;
} {
  if (!colorValue) return { bg: undefined, text: undefined };

  const isDirectColor =
    colorValue.startsWith("#") ||
    colorValue.startsWith("rgb") ||
    colorValue.startsWith("hsl") ||
    colorValue.includes("(");

  if (isDirectColor) {
    return { bg: colorValue, text: undefined };
  }

  const lowerColor = colorValue.toLowerCase().replace(/\s+/g, "-");
  const bgVar = `--${lowerColor}`;
  const fgVar = `--${lowerColor}-foreground`;

  let bgColor = getCSSVariable(bgVar) || getCSSVariable(`--color-${lowerColor}`);
  let fgColor = getCSSVariable(fgVar) || getCSSVariable(`--color-${lowerColor}-foreground`);

  // Shadcn/Tailwind often use raw HSL components in variables
  const wrapInHsl = (val: string) => {
    if (!val) return val;
    if (val.startsWith("#") || val.startsWith("rgb") || val.startsWith("hsl") || val.includes("("))
      return val;
    if (val.split(/[\s,]+/).filter(Boolean).length >= 3) return `hsl(${val})`;
    return val;
  };

  return {
    bg: wrapInHsl(bgColor) || undefined,
    text: wrapInHsl(fgColor) || undefined,
  };
}
