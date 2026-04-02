import * as arrow from "apache-arrow";
import { DataColumn, DataRow, ColType } from "../types/types";

function calculateColumnWidth(
  columnName: string,
  columnData: arrow.Vector,
  maxSampleSize = 100,
): number {
  const minWidth = 80;
  const maxWidth = 400;
  const charWidth = 8; // Approximate pixel width per character
  const padding = 40; // Cell padding + icons

  // Start with header width
  let maxLength = columnName.length;

  // Sample data to calculate content width
  const sampleSize = Math.min(maxSampleSize, columnData.length);
  for (let i = 0; i < sampleSize; i++) {
    const value = columnData.get(i);
    if (value != null) {
      const strValue = String(value);
      maxLength = Math.max(maxLength, strValue.length);
    }
  }

  const calculatedWidth = maxLength * charWidth + padding;
  return Math.min(Math.max(calculatedWidth, minWidth), maxWidth);
}

/**
 * Maps Arrow field type to ColType enum
 */
function mapArrowTypeToColType(arrowType: string): ColType {
  const lowerType = arrowType.toLowerCase();
  if (
    lowerType.includes("int") ||
    lowerType.includes("float") ||
    lowerType.includes("double") ||
    lowerType.includes("decimal")
  ) {
    return ColType.Number;
  }
  if (lowerType.includes("bool")) {
    return ColType.Boolean;
  }
  if (lowerType.includes("date") || lowerType.includes("timestamp")) {
    return lowerType.includes("timestamp") ? ColType.DateTime : ColType.Date;
  }
  // Default to Text for strings and unknown types
  return ColType.Text;
}

/**
 * Convert a DecimalBigNum value to a JS number using the column's scale.
 * Primary path uses Arrow's valueOf(scale); falls back to string-based conversion
 * when valueOf throws (e.g. RangeError from BigUint64Array alignment on IPC buffers).
 */
export function convertDecimalValue(
  value: { valueOf?: (scale: number) => unknown; toString: () => string },
  scale: number,
): number | null {
  try {
    // Arrow 21.1.0's valueOf(scale) correctly divides unscaled integer by 10^scale.
    // May throw RangeError on IPC buffers due to BigUint64Array alignment.
    const result = typeof value.valueOf === "function" ? value.valueOf(scale) : null;
    if (typeof result === "number") return result;
    if (typeof result === "bigint") return Number(result);
  } catch {
    // Fall through to string-based fallback
  }
  // Fallback: parse the string representation and insert decimal point
  try {
    const str = value.toString();
    const negative = str.startsWith("-");
    const abs = negative ? str.slice(1) : str;
    if (scale === 0) {
      return Number(str);
    } else if (abs.length <= scale) {
      return Number(`${negative ? "-" : ""}0.${abs.padStart(scale, "0")}`);
    } else {
      return Number(`${negative ? "-" : ""}${abs.slice(0, -scale)}.${abs.slice(-scale)}`);
    }
  } catch {
    return null;
  }
}

/**
 * Build a map of column index → decimal scale from an Arrow table schema.
 */
export function buildDecimalScales(schema: arrow.Schema): Map<number, number> {
  const scales = new Map<number, number>();
  for (let j = 0; j < schema.fields.length; j++) {
    const field = schema.fields[j];
    if (field.type.toString().toLowerCase().includes("decimal")) {
      scales.set(j, (field.type as arrow.Decimal).scale);
    }
  }
  return scales;
}

export function convertArrowTableToData(
  table: arrow.Table,
  requestedCount: number,
): {
  columns: DataColumn[];
  rows: DataRow[];
  hasMore: boolean;
} {
  const columns: DataColumn[] = table.schema.fields
    .filter((field: arrow.Field) => field.name !== "_hiddenKey")
    .map((field: arrow.Field) => {
      // Find the actual index in the original table (accounting for filtered _hiddenKey)
      const originalIndex = table.schema.fields.findIndex((f) => f.name === field.name);
      const columnData = table.getChildAt(originalIndex);
      const width = columnData ? calculateColumnWidth(field.name, columnData) : 150;

      // Infer type from Arrow field type (no metadata parsing)
      const type = mapArrowTypeToColType(field.type.toString());

      return {
        name: field.name,
        type,
        width,
      };
    });

  const decimalScales = buildDecimalScales(table.schema);

  const rows: DataRow[] = [];
  for (let i = 0; i < table.numRows; i++) {
    const values: (string | number | boolean | null)[] = [];
    for (let j = 0; j < table.numCols; j++) {
      const column = table.getChildAt(j);
      if (column) {
        let value = column.get(i);
        const scale = decimalScales.get(j);
        if (scale !== undefined && value != null && typeof value === "object") {
          value = convertDecimalValue(value, scale);
        }
        values.push(value);
      }
    }
    rows.push({ values });
  }

  const hasMore = table.numRows === requestedCount;

  return {
    columns,
    rows,
    hasMore,
  };
}
