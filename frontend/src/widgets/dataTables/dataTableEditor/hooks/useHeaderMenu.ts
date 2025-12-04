import { useCallback } from 'react';
import { DataColumn } from '../../types/types';

interface UseHeaderMenuProps {
  columns: DataColumn[];
  allowSorting: boolean;
  handleSort: (columnName: string) => void;
}

/**
 * Hook for handling header menu interactions (e.g., sorting)
 */
export const useHeaderMenu = ({
  columns,
  allowSorting,
  handleSort,
}: UseHeaderMenuProps) => {
  const handleHeaderMenuClick = useCallback(
    (col: number) => {
      // Only handle sorting if it's enabled globally
      if (!allowSorting) return;

      // Get visible columns to map the correct column index
      const visibleColumns = columns.filter(c => !c.hidden);
      const column = visibleColumns[col];

      // Check if this specific column is sortable (defaults to true if not specified)
      if (column && (column.sortable ?? true)) {
        handleSort(column.name);
      }
    },
    [columns, handleSort, allowSorting]
  );

  return { handleHeaderMenuClick };
};
