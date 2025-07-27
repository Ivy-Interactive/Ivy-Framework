import { GridColumn } from '@glideapps/glide-data-grid';
import { DataTableColumn } from '../types';

export function createGlideColumns(
  columns: DataTableColumn[],
  columnWidths: Record<number, number>,
  resizableColumns: boolean = true
): GridColumn[] {
  return columns.map((col, index) => ({
    title: col.name,
    id: index.toString(), // Use index as ID for proper resizing
    width: columnWidths[index] || getDefaultColumnWidth(col.type),
    resizable: resizableColumns,
    sortable: true,
    hasMenu: true,
    themeOverride: {
      textHeader: '#374151',
      bgHeader: '#f9fafb',
    },
  }));
}

export function getDefaultColumnWidth(columnType: string): number {
  if (columnType.includes('bool')) {
    return 80;
  }

  if (columnType.includes('int') || columnType.includes('float')) {
    return 120;
  }

  if (columnType.includes('date') || columnType.includes('time')) {
    return 180;
  }

  return 150;
}
