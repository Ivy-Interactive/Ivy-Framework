import { Condition, Filter, FilterGroup } from '@/services/grpcTableService';

export const createContainsFilter = (
  searchTerm: string,
  columns: string[]
): Filter => {
  if (columns.length === 0) {
    throw new Error('At least one column is required for filtering');
  }

  // For now, let's try with just the first column to debug
  // TODO: Support multiple columns once we confirm the function works
  const condition: Condition = {
    column: columns[0],
    function: 'contains',
    args: [searchTerm],
  };

  console.log('Creating filter condition:', condition);

  return {
    condition,
  };
};

export const combineFiltersWithAnd = (
  filterStrings: string[],
  columns: string[]
): Filter | undefined => {
  if (filterStrings.length === 0) {
    return undefined;
  }

  if (filterStrings.length === 1) {
    return createContainsFilter(filterStrings[0], columns);
  }

  // Multiple filter strings combined with AND
  const filters: Filter[] = filterStrings.map(searchTerm =>
    createContainsFilter(searchTerm, columns)
  );

  const group: FilterGroup = {
    op: 'AND',
    filters,
  };

  console.log('Creating combined filter with AND:', {
    group,
    filterStrings,
    columns,
  });

  return {
    group,
  };
};
