import type { GridColumn } from '@glideapps/glide-data-grid';
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

/**
 * Converts data columns to GridColumn format with proper widths and groups
 * Filters out hidden columns and applies column ordering
 */
export function convertToGridColumns(
  columns: DataColumn[],
  columnOrder: number[],
  columnWidths: Record<string, number>,
  containerWidth: number,
  showGroups: boolean
): GridColumn[] {
  // Filter out hidden columns first
  const visibleColumns = columns.filter(col => !col.hidden);

  // Apply column order if available (using order property or columnOrder array)
  let orderedColumns = visibleColumns;

  // If columns have explicit order property, use that
  const hasOrderProperty = visibleColumns.some(col => col.order !== undefined);
  if (hasOrderProperty) {
    orderedColumns = [...visibleColumns].sort((a, b) => {
      const orderA = a.order ?? Number.MAX_SAFE_INTEGER;
      const orderB = b.order ?? Number.MAX_SAFE_INTEGER;
      return orderA - orderB;
    });
  } else if (columnOrder.length === columns.length) {
    // Otherwise use the columnOrder array (from user reordering)
    orderedColumns = columnOrder
      .map(idx => columns[idx])
      .filter(col => !col.hidden);
  }

  return orderedColumns.map((col, index) => {
    const originalIndex = columns.indexOf(col);
    const baseWidth = columnWidths[originalIndex.toString()] || col.width;

    // Make the last column fill the remaining space
    if (index === orderedColumns.length - 1 && containerWidth > 0) {
      const totalWidthOfOtherColumns = orderedColumns
        .slice(0, -1)
        .reduce((sum, c) => {
          const idx = columns.indexOf(c);
          return sum + (columnWidths[idx.toString()] || c.width);
        }, 0);

      const remainingWidth = containerWidth - totalWidthOfOtherColumns;
      return {
        title: col.header || col.name,
        width: Math.max(baseWidth, remainingWidth) - 10,
        group: showGroups ? col.group : undefined,
        // TODO: Custom header rendering needed for Lucide icons
        // icon: col.icon ?? undefined,
      };
    }

    return {
      title: col.header || col.name,
      width: baseWidth,
      group: showGroups ? col.group : undefined,
      // TODO: Custom header rendering needed for Lucide icons
      // icon: col.icon ?? undefined,
    };
  });
}
