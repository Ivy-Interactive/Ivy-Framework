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

  // Precompute decimal column scales for proper Decimal128 → JS number conversion.
  // Arrow's DecimalBigNum.valueOf(scale) needs the scale to divide the unscaled integer.
  const decimalScales = new Map<number, number>();
  for (let j = 0; j < table.schema.fields.length; j++) {
    const field = table.schema.fields[j];
    if (field.type.toString().toLowerCase().includes("decimal")) {
      const scale = (field.type as arrow.Decimal).scale;
      decimalScales.set(j, scale);
    }
  }

  const rows: DataRow[] = [];
  for (let i = 0; i < table.numRows; i++) {
    const values: (string | number | boolean | null)[] = [];
    for (let j = 0; j < table.numCols; j++) {
      const column = table.getChildAt(j);
      if (column) {
        let value = column.get(i);
        // Arrow Decimal128 values are DecimalBigNum objects containing unscaled integers.
        // We must pass the column's scale to valueOf() so it divides correctly.
        const scale = decimalScales.get(j);
        if (scale !== undefined && value != null && typeof value === "object") {
          // Try calling valueOf with scale parameter
          const convertedValue = typeof value.valueOf === "function" ? value.valueOf(scale) : value;

          // Handle different return types from valueOf()
          if (typeof convertedValue === "bigint") {
            // Convert bigint to number by dividing by 10^scale
            value = Number(convertedValue) / Math.pow(10, scale);
          } else if (typeof convertedValue === "number") {
            value = convertedValue;
          } else {
            // Fallback: parse string representation and convert
            // This handles cases where valueOf() returns a string or doesn't work as expected
            try {
              const unscaledValue = BigInt(value.toString());
              value = Number(unscaledValue) / Math.pow(10, scale);
            } catch {
              value = null; // Unable to convert, set to null
            }
          }
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
