import { useCallback, useState } from 'react';
import { SortOrder } from '@/services/grpcTableService';
import { DataColumn, SortDirection } from '../../types/types';

interface UseSortingProps {
  allowSorting: boolean;
}

/**
 * Hook for managing table sorting state and logic
 */
export const useSorting = ({ allowSorting }: UseSortingProps) => {
  const [activeSort, setActiveSort] = useState<SortOrder[] | null>(null);

  const handleSort = useCallback(
    (columnName: string) => {
      // Check if sorting is allowed
      if (!allowSorting) return;

      setActiveSort(prevSort => {
        // Check if we're already sorting by this column
        const existingSort = prevSort?.find(sort => sort.column === columnName);

        if (existingSort) {
          // Toggle direction: ASC -> DESC -> remove sort
          if (existingSort.direction === 'ASC') {
            return [{ column: columnName, direction: 'DESC' as const }];
          } else {
            // Remove sort entirely
            return null;
          }
        } else {
          // Replace current sort with new column (ASC by default)
          return [{ column: columnName, direction: 'ASC' }];
        }
      });
    },
    [allowSorting]
  );

  // Initialize sort from column metadata (only on first load)
  const initializeSortFromColumns = useCallback(
    (mergedColumns: DataColumn[]) => {
      if (activeSort === null) {
        const sortedColumn = mergedColumns.find(
          col =>
            col.sortDirection &&
            col.sortDirection !== SortDirection.None &&
            (col.sortable ?? true)
        );
        if (sortedColumn) {
          const direction =
            sortedColumn.sortDirection === SortDirection.Ascending
              ? ('ASC' as const)
              : ('DESC' as const);
          setActiveSort([{ column: sortedColumn.name, direction }]);
          return true; // Indicates sort was initialized
        }
      }
      return false;
    },
    [activeSort]
  );

  return {
    activeSort,
    handleSort,
    initializeSortFromColumns,
  };
};
