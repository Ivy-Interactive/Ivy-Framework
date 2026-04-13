import * as arrow from "apache-arrow";

/**
 * Extract the _hiddenKey value from an Arrow table for a given row index.
 * Returns null if the table, schema, _hiddenKey column, or value is missing/empty.
 */
export function getHiddenKeyValue(
  table: arrow.Table | null,
  rowIndex: number,
): string | number | null {
  if (!table) return null;

  const schema = table.schema;
  if (!schema || !schema.fields) return null;

  let hiddenKeyIndex = -1;
  for (let i = 0; i < schema.fields.length; i++) {
    const field = schema.fields[i];
    if (field && field.name === "_hiddenKey") {
      hiddenKeyIndex = i;
      break;
    }
  }

  if (hiddenKeyIndex === -1) return null;

  const column = table.getChildAt(hiddenKeyIndex);
  if (!column) return null;

  const value = column.get(rowIndex);
  if (value === null || value === undefined || value === "") {
    return null;
  }

  return String(value);
}
