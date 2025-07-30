import { GridCellKind } from '@glideapps/glide-data-grid';

/**
 * Determines the appropriate GridCellKind based on data value and column type
 */
export function getGridCellKind(
  value: unknown,
  columnType: string
): GridCellKind {
  if (value === null || value === undefined) {
    return GridCellKind.Text;
  }

  // Check column type first (from Arrow schema)
  if (
    columnType.includes('int') ||
    columnType.includes('float') ||
    columnType.includes('double') ||
    columnType.includes('decimal')
  ) {
    return GridCellKind.Number;
  }

  if (columnType.includes('bool')) {
    return GridCellKind.Boolean;
  }

  if (columnType.includes('date') || columnType.includes('time')) {
    return GridCellKind.Text; // Could be extended to custom date cell
  }

  // Fallback to runtime type checking
  if (typeof value === 'number') {
    return GridCellKind.Number;
  }

  if (typeof value === 'boolean') {
    return GridCellKind.Boolean;
  }

  return GridCellKind.Text;
}

/**
 * Safely converts a value to a number for number cells
 */
export function toNumber(value: unknown): number {
  if (typeof value === 'number') {
    return value;
  }

  const parsed = parseFloat(String(value));
  return isNaN(parsed) ? 0 : parsed;
}

/**
 * Safely converts a value to a boolean for boolean cells
 */
export function toBoolean(value: unknown): boolean {
  if (typeof value === 'boolean') {
    return value;
  }

  if (typeof value === 'string') {
    return value.toLowerCase() === 'true';
  }

  return Boolean(value);
}

/**
 * Formats a value for display in a cell
 */
export function formatCellValue(value: unknown, columnType: string): string {
  if (value === null || value === undefined) {
    return '';
  }

  if (columnType.includes('date')) {
    // Could add date formatting here
    return String(value);
  }

  if (columnType.includes('float') || columnType.includes('double')) {
    const num = toNumber(value);
    return num.toLocaleString(undefined, {
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    });
  }

  return String(value);
}
