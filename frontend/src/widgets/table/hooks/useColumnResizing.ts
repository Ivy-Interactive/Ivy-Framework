import { GridColumn } from '@glideapps/glide-data-grid';
import { useCallback, useState } from 'react';
import { UseColumnResizingReturn } from '../types';

export function useColumnResizing(
  initialWidths: Record<number, number> = {}
): UseColumnResizingReturn {
  const [columnWidths, setColumnWidths] =
    useState<Record<number, number>>(initialWidths);

  const onManualResize = useCallback((column: GridColumn, newSize: number) => {
    const columnIndex = parseInt(column.id || '0') || 0;
    setColumnWidths(prev => ({
      ...prev,
      [columnIndex]: Math.max(50, newSize),
    }));
  }, []);

  return {
    columnWidths,
    onManualResize,
  };
}
