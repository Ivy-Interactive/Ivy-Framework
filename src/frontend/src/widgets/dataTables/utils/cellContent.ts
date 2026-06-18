import { GridCell, GridCellKind, Item, Theme } from "@glideapps/glide-data-grid";
import { Align, DataColumn, DataRow } from "../types/types";
import { getCSSVariable, isDarkMode } from "@/lib/theme";
import type { AnimatedStatusCellData, LabelsBadgesCellData } from "./customRenderers";
import { getCellFont, getMaxTextWidth, truncateTextWithEllipsis } from "./canvasText";
import { DENSITY_CONFIG } from "../dataTableEditor/constants";
import { Densities } from "@/types/density";

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
    value.indexOf(" ") === -1
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
  return columnType.indexOf("date") !== -1 || columnType.indexOf("timestamp") !== -1;
}

/**
 * Checks if a column type represents a numeric type
 */
export function isNumericColumnType(columnType: string): boolean {
  return (
    columnType.indexOf("int") !== -1 ||
    columnType.indexOf("float") !== -1 ||
    columnType.indexOf("double") !== -1 ||
    columnType.indexOf("decimal") !== -1 ||
    columnType.indexOf("number") !== -1
  );
}

/**
 * Formats a date value for display
 */
export function formatDateValue(dateValue: Date, columnType: string): string {
  const hasTime =
    columnType.indexOf("datetime") !== -1 ||
    columnType.indexOf("timestamp") !== -1 ||
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

function truncateCellDisplayData(
  cell: GridCell,
  columnWidth: number | undefined,
  cellHorizontalPadding: number,
  cellFont: string,
  wrapText?: boolean,
): GridCell {
  if (wrapText || columnWidth === undefined) return cell;

  const maxWidth = getMaxTextWidth(columnWidth, cellHorizontalPadding);
  if (maxWidth <= 0) return cell;

  if (cell.kind === GridCellKind.Text || cell.kind === GridCellKind.Number) {
    const displayData = cell.displayData;
    const truncated = truncateTextWithEllipsis(displayData, maxWidth, cellFont);
    if (truncated === displayData) return cell;
    return { ...cell, displayData: truncated, copyData: displayData };
  }

  return cell;
}

/**
 * Creates a date/timestamp cell
 */
export function createDateCell(
  cellValue: unknown,
  columnType: string,
  editable: boolean,
  align?: Align,
  wrapText?: boolean,
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
    allowWrapping: wrapText ?? false,
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
export function createTextCell(
  cellValue: unknown,
  editable: boolean,
  align?: Align,
  wrapText?: boolean,
): GridCell {
  const stringValue = String(cellValue);

  return {
    kind: GridCellKind.Text,
    data: stringValue,
    displayData: stringValue,
    allowOverlay: editable,
    readonly: !editable,
    contentAlign: align ? getContentAlign(align) : undefined,
    allowWrapping: wrapText ?? false,
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
  badgeColorMapping?: Record<string, string> | null,
): GridCell {
  // Handle different input formats
  let labels: readonly string[];

  if (Array.isArray(cellValue)) {
    labels = cellValue.reduce<string[]>((acc, item) => {
      if (item != null) acc.push(String(item));
      return acc;
    }, []);
  } else if (typeof cellValue === "string") {
    // Try to parse as JSON first (from backend serialization)
    try {
      const parsed = JSON.parse(cellValue);
      if (Array.isArray(parsed)) {
        labels = parsed.reduce<string[]>((acc, item) => {
          if (item != null) acc.push(String(item));
          return acc;
        }, []);
      } else {
        // Fallback to comma-separated if JSON parsing doesn't yield an array
        labels = cellValue.split(",").reduce<string[]>((acc, s) => {
          const trimmed = s.trim();
          if (trimmed.length > 0) acc.push(trimmed);
          return acc;
        }, []);
      }
    } catch {
      // Not JSON, treat as comma-separated string
      labels = cellValue.split(",").reduce<string[]>((acc, s) => {
        const trimmed = s.trim();
        if (trimmed.length > 0) acc.push(trimmed);
        return acc;
      }, []);
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
      const raw = lookupBadgeColorMapping(badgeColorMapping, label) ?? color ?? null;
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
  let effectiveColor = color;
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
    if (bg) {
      themeOverride.bgBubble = bg;
      themeOverride.bgBubbleSelected = bg;
    }
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
 * Parses the encoded value emitted by AnimatedStatusValue (C#) into cell data.
 * Format: "<state>:<text>[\t<rightLabel>]" where state is "running" | "done" | "idle".
 * Plain unencoded strings are accepted too and treated as idle (e.g. for
 * SpinnerTimer cells where the C# side may emit raw timer text).
 */
export function createAnimatedStatusCell(
  cellValue: unknown,
  align?: Align,
  mode?: AnimatedStatusCellData["mode"],
  badgeColorMapping?: Record<string, string> | null,
): GridCell {
  const raw = cellValue == null ? "" : String(cellValue);
  const [body, rightLabel] = raw.split("\t", 2);
  const colonIdx = body.indexOf(":");
  let state: AnimatedStatusCellData["state"] = "idle";
  let statusText = body;
  if (colonIdx >= 0) {
    const head = body.slice(0, colonIdx);
    if (head === "running" || head === "done" || head === "idle") {
      state = head;
      statusText = body.slice(colonIdx + 1);
    }
  }
  let badgeBg: string | undefined;
  let badgeFg: string | undefined;
  if (mode === "badge" && badgeColorMapping) {
    const color = lookupBadgeColorMapping(badgeColorMapping, statusText);
    if (color) {
      const resolved = resolveBadgeColor(color);
      badgeBg = resolved.bg;
      badgeFg = resolved.text;
    }
  }
  return {
    kind: GridCellKind.Custom,
    allowOverlay: false,
    readonly: true,
    copyData: statusText,
    data: {
      kind: "animated-status-cell",
      mode: mode ?? "label",
      state,
      statusText,
      rightLabel: rightLabel || undefined,
      align: align ? getContentAlign(align) : undefined,
      badgeBg,
      badgeFg,
    },
  };
}

/**
 * Creates a link cell with custom renderer (blue text + underline)
 */
export function createLinkCell(
  value: string,
  _editable: boolean, // Intentionally unused - links are always readonly
  align?: Align,
  linkType?: string,
): GridCell {
  let url: string;
  let text: string;

  // Check if it's a markdown link [text](url)
  const markdownMatch = value.match(/^\[([^\]]+)\]\(([^)]+)\)$/);
  if (markdownMatch) {
    text = markdownMatch[1];
    url = markdownMatch[2];
  } else {
    url = value;
    text = value; // Backward compatible - show URL as text
  }

  // Auto-prepend mailto: or tel: for Email/Phone types
  if (linkType === "email" && !url.startsWith("mailto:")) {
    url = `mailto:${url}`;
  } else if (linkType === "phone" && !url.startsWith("tel:")) {
    url = `tel:${url}`;
  }

  return {
    kind: GridCellKind.Custom,
    data: {
      kind: "link-cell",
      url: url,
      text: text,
      align: align?.toLowerCase() as "left" | "center" | "right" | undefined,
      linkType: linkType,
    },
    copyData: text, // Copy the display text, not the URL
    allowOverlay: false,
    readonly: true,
    cursor: "default",
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
export interface GetCellContentOptions {
  columnWidth?: number;
  cellHorizontalPadding?: number;
  cellFont?: string;
}

export function getCellContent(
  cell: Item,
  columns: DataColumn[],
  columnOrder: number[],
  editable: boolean,
  getRowData: (rowIndex: number) => DataRow | null,
  options: GetCellContentOptions = {},
): GridCell {
  const [col, row] = cell;

  // Apply column order first, then filter out hidden columns
  let orderedCols: DataColumn[];
  if (columnOrder.length === columns.length) {
    // Map using columnOrder indices, then filter hidden
    orderedCols = columnOrder.reduce<DataColumn[]>((acc, idx) => {
      const col = columns[idx];
      if (col && !col.hidden) acc.push(col);
      return acc;
    }, []);
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

  const createCell = (): GridCell => {
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
      return createLabelsCell(cellValue, align, column.color, column.badgeColorMapping);
    }

    // Handle AnimatedStatus type - spinner + shimmer label, animated badge, or spinner+timer
    if (column.type === "AnimatedStatus") {
      const mode = column.animatedStatusMode
        ? (column.animatedStatusMode.toLowerCase() as "label" | "badge" | "spinnertimer")
        : "label";
      const normalizedMode =
        mode === "spinnertimer" ? "spinner-timer" : (mode as "label" | "badge");
      return createAnimatedStatusCell(cellValue, align, normalizedMode, column.badgeColorMapping);
    }

    // Handle explicit link type from backend metadata
    if (column.type === "Link" && typeof cellValue === "string") {
      const linkType = column.linkType?.toLowerCase();
      return createLinkCell(cellValue, editable, align, linkType);
    }

    // Handle Date and DateTime types
    if (isDateColumnType(columnType)) {
      const dateCell = createDateCell(cellValue, columnType, editable, align, column.wrapText);
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

    // Default to text
    return createTextCell(cellValue, editable, align, column.wrapText);
  };

  const gridCell = createCell();
  const withTruncation = truncateCellDisplayData(
    gridCell,
    options.columnWidth,
    options.cellHorizontalPadding ?? DENSITY_CONFIG[Densities.Medium].cellHorizontalPadding,
    options.cellFont ?? getCellFont(),
    column.wrapText,
  );
  if (column.hasCellAction) {
    return { ...withTruncation, cursor: "pointer" };
  }
  return withTruncation;
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
    colorValue.indexOf("(") !== -1;

  if (isDirectColor) {
    return { bg: colorValue, text: undefined };
  }

  const lowerColor = colorValue.toLowerCase().replace(/\s+/g, "-");
  const dark = isDarkMode();

  // Shadcn/Tailwind often use raw HSL components in variables
  const wrapInHsl = (val: string) => {
    if (!val) return val;
    if (
      val.startsWith("#") ||
      val.startsWith("rgb") ||
      val.startsWith("hsl") ||
      val.indexOf("(") !== -1
    )
      return val;
    if (val.split(/[\s,]+/).filter(Boolean).length >= 3) return `hsl(${val})`;
    return val;
  };

  // Use shade variants to match BadgeWidget styling (light bg, dark text)
  const bgShade = dark ? "800" : "200";
  const fgShade = dark ? "100" : "800";
  let bgColor = getCSSVariable(`--${lowerColor}-${bgShade}`);
  let fgColor = getCSSVariable(`--${lowerColor}-${fgShade}`);

  // Fall back to base color variables if shade variants aren't available
  if (!bgColor) {
    bgColor = getCSSVariable(`--${lowerColor}`) || getCSSVariable(`--color-${lowerColor}`);
  }
  if (!fgColor) {
    fgColor =
      getCSSVariable(`--${lowerColor}-foreground`) ||
      getCSSVariable(`--color-${lowerColor}-foreground`);
  }

  return {
    bg: wrapInHsl(bgColor) || undefined,
    text: wrapInHsl(fgColor) || undefined,
  };
}
