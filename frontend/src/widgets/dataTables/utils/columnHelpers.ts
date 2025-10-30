import { GridColumn, GridColumnIcon } from '@glideapps/glide-data-grid';
import type { DataColumn } from '../types/types';

/**
 * Maps column icon names or types to appropriate icons
 * Uses built-in GridColumnIcon values where possible, custom names otherwise
 */
export function mapColumnIcon(
  col: DataColumn
): GridColumnIcon | string | undefined {
  // If column has explicit icon, check if we can map it to built-in
  if (col.icon) {
    // Map icons to built-in GridColumnIcon values
    switch (col.icon) {
      case 'Hash':
        return GridColumnIcon.HeaderNumber;
      case 'Type':
      case 'User':
      case 'Mail':
        return GridColumnIcon.HeaderString;
      case 'Calendar':
      case 'Clock':
        return GridColumnIcon.HeaderDate;
      case 'Image':
        return GridColumnIcon.HeaderImage;
      case 'Link':
        return GridColumnIcon.HeaderUri;
      // Map Activity, Flag, and Zap to closest built-in icons
      case 'Activity':
        return GridColumnIcon.HeaderCode; // Use code icon for activity
      case 'Flag':
        return GridColumnIcon.HeaderBoolean; // Use boolean/checkbox for priority flag
      case 'Zap':
        return GridColumnIcon.HeaderEmoji; // Use emoji for zap
      default:
        // Try to use custom icon name for headerIcons lookup
        // But for now, default to a built-in icon
        return GridColumnIcon.HeaderString;
    }
  }

  // If no explicit icon, use column type
  const normalizedType = col.type.toLowerCase();

  if (normalizedType === 'number') {
    return GridColumnIcon.HeaderNumber;
  }
  if (normalizedType === 'text') {
    return GridColumnIcon.HeaderString;
  }
  if (normalizedType === 'date' || normalizedType === 'datetime') {
    return GridColumnIcon.HeaderDate;
  }
  if (normalizedType === 'boolean') {
    return GridColumnIcon.HeaderBoolean;
  }
  if (normalizedType === 'icon') {
    return GridColumnIcon.HeaderImage;
  }

  return GridColumnIcon.HeaderString; // Default
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
  showGroups: boolean,
  showColumnTypeIcons: boolean = true
): GridColumn[] {
  // Filter out hidden columns first
  const visibleColumns = columns.filter(col => !col.hidden);

  // Apply column order if available
  let orderedColumns = visibleColumns;

  // User reordering (columnOrder array) takes precedence over backend order property
  if (columnOrder.length === columns.length) {
    // Use the columnOrder array (from user reordering)
    orderedColumns = columnOrder
      .map(idx => columns[idx])
      .filter(col => !col.hidden);
  } else {
    // Fall back to explicit order property if no user reordering has happened
    const hasOrderProperty = visibleColumns.some(
      col => col.order !== undefined
    );
    if (hasOrderProperty) {
      orderedColumns = [...visibleColumns].sort((a, b) => {
        const orderA = a.order ?? Number.MAX_SAFE_INTEGER;
        const orderB = b.order ?? Number.MAX_SAFE_INTEGER;
        return orderA - orderB;
      });
    }
  }

  return orderedColumns.map((col, index) => {
    const originalIndex = columns.indexOf(col);
    const baseWidth = columnWidths[originalIndex.toString()] || col.width;
    // Ensure width is always a number
    let numericBaseWidth =
      typeof baseWidth === 'string' ? parseFloat(baseWidth) : baseWidth;

    // Fix NaN width - default to 150 if parsing fails
    if (isNaN(numericBaseWidth) || !numericBaseWidth) {
      numericBaseWidth = 150;
    }

    // Make the last column fill the remaining space
    if (index === orderedColumns.length - 1 && containerWidth > 0) {
      const totalWidthOfOtherColumns = orderedColumns
        .slice(0, -1)
        .reduce((sum, c) => {
          const idx = columns.indexOf(c);
          const colWidth = columnWidths[idx.toString()] || c.width;
          const numericColWidth =
            typeof colWidth === 'string' ? parseFloat(colWidth) : colWidth;
          return sum + numericColWidth;
        }, 0);

      const remainingWidth = containerWidth - totalWidthOfOtherColumns;
      return {
        title: col.header || col.name,
        width: Math.max(numericBaseWidth, remainingWidth) - 10,
        group: showGroups ? col.group : undefined,
        icon: showColumnTypeIcons ? mapColumnIcon(col) : undefined,
      };
    }

    return {
      title: col.header || col.name,
      width: numericBaseWidth,
      group: showGroups ? col.group : undefined,
      icon: showColumnTypeIcons ? mapColumnIcon(col) : undefined,
    };
  });
}
