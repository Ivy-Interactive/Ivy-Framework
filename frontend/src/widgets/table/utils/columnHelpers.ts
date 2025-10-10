import type { DataColumn } from '../types/types';

/**
 * Maps data column types to lucide-react icon names
 */
export function getColumnTypeIcon(type: string): string {
  const normalizedType = type.toLowerCase();

  // Numeric types
  if (
    normalizedType.includes('int') ||
    normalizedType.includes('float') ||
    normalizedType.includes('double') ||
    normalizedType.includes('decimal') ||
    normalizedType.includes('number')
  ) {
    return 'Hash';
  }

  // Date/time types
  if (
    normalizedType.includes('date') ||
    normalizedType.includes('time') ||
    normalizedType.includes('timestamp')
  ) {
    return 'Calendar';
  }

  // Boolean types
  if (normalizedType.includes('bool')) {
    return 'ToggleLeft';
  }

  // Default to string/text icon
  return 'Type';
}

/**
 * Reorders columns array by moving a column from startIndex to endIndex
 * Returns a new array without modifying the original
 */
export function reorderColumns(
  columns: DataColumn[],
  startIndex: number,
  endIndex: number
): DataColumn[] {
  const result = [...columns];
  const [removed] = result.splice(startIndex, 1);
  result.splice(endIndex, 0, removed);
  return result;
}
