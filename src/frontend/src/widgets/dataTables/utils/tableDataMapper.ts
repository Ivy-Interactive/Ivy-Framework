import * as arrow from "apache-arrow";
import { convertUnscaledDecimalRawToNumber } from "./arrowDecimal";
import { DataColumn, DataRow, ColType } from "../types/types";

function calculateColumnWidth(
  columnName: string,
  columnData: arrow.Vector,
  field: arrow.Field,
  maxSampleSize = 100,
): number {
  const minWidth = 80;
  const maxWidth = 400;
  const charWidth = 8; // Approximate pixel width per character
  const padding = 40; // Cell padding + icons
  const isDecimal = arrow.DataType.isDecimal(field.type);

  // Start with header width
  let maxLength = columnName.length;

  // Sample data to calculate content width
  const sampleSize = Math.min(maxSampleSize, columnData.length);
  for (let i = 0; i < sampleSize; i++) {
    let value = columnData.get(i);
    if (value != null) {
      if (isDecimal) {
        value = convertUnscaledDecimalRawToNumber(value, field.type.scale);
      }
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
    lowerType.indexOf("int") !== -1 ||
    lowerType.indexOf("float") !== -1 ||
    lowerType.indexOf("double") !== -1 ||
    lowerType.indexOf("decimal") !== -1
  ) {
    return ColType.Number;
  }
  if (lowerType.indexOf("bool") !== -1) {
    return ColType.Boolean;
  }
  if (lowerType.indexOf("date") !== -1 || lowerType.indexOf("timestamp") !== -1) {
    return lowerType.indexOf("timestamp") !== -1 ? ColType.DateTime : ColType.Date;
  }
  // Default to Text for strings and unknown types
  return ColType.Text;
}

export function convertDecimalValue(
  value: { valueOf?: (scale: number) => unknown; toString: () => string },
  scale: number,
): number | null {
  try {
    const result = typeof value.valueOf === "function" ? value.valueOf(scale) : null;
    if (typeof result === "number") return result;
    if (typeof result === "bigint") return Number(result);
  } catch {
    // fall through
  }

  try {
    const str = value.toString();
    const negative = str.startsWith("-");
    const abs = negative ? str.slice(1) : str;
    if (scale === 0) return Number(str);
    if (abs.length <= scale) return Number(`${negative ? "-" : ""}0.${abs.padStart(scale, "0")}`);
    return Number(`${negative ? "-" : ""}${abs.slice(0, -scale)}.${abs.slice(-scale)}`);
  } catch {
    return null;
  }
}

export function convertArrowTableToData(
  table: arrow.Table,
  requestedCount: number,
): {
  columns: DataColumn[];
  rows: DataRow[];
  hasMore: boolean;
} {
  const columns: DataColumn[] = table.schema.fields.reduce<DataColumn[]>(
    (acc, field: arrow.Field) => {
      if (field.name !== "_hiddenKey") {
        // Find the actual index in the original table (accounting for filtered _hiddenKey)
        const originalIndex = table.schema.fields.findIndex((f) => f.name === field.name);
        const columnData = table.getChildAt(originalIndex);
        const width = columnData ? calculateColumnWidth(field.name, columnData, field) : 150;

        // Infer type from Arrow field type (no metadata parsing)
        const type = mapArrowTypeToColType(field.type.toString());

        acc.push({
          name: field.name,
          type,
          width,
        });
      }
      return acc;
    },
    [],
  );

  // Precompute decimal column scales for proper Decimal128 → JS number conversion.
  // Arrow's DecimalBigNum.valueOf(scale) needs the scale to divide the unscaled integer.
  const decimalScales = new Map<number, number>();
  for (let j = 0; j < table.schema.fields.length; j++) {
    const field = table.schema.fields[j];
    if (/decimal/.test(field.type.toString().toLowerCase())) {
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
          value = convertDecimalValue(
            value as { valueOf?: (scale: number) => unknown; toString: () => string },
            scale,
          );
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
