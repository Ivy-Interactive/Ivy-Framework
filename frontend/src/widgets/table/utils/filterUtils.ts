import { Condition, Filter, FilterGroup } from '@/services/grpcTableService';

export const createContainsFilter = (
  searchTerm: string,
  columns: string[]
): Filter => {
  if (columns.length === 0) {
    throw new Error('At least one column is required for filtering');
  }

  if (columns.length === 1) {
    // Single column filter
    const condition: Condition = {
      column: columns[0],
      function: 'contains',
      args: [searchTerm],
    };

    console.log('Creating single column filter condition:', condition);

    return {
      condition,
    };
  }

  // Multiple columns filter with OR logic - search term should match ANY column
  const filters: Filter[] = columns.map(column => ({
    condition: {
      column,
      function: 'contains',
      args: [searchTerm],
    },
  }));

  const group: FilterGroup = {
    op: 'OR',
    filters,
  };

  console.log('Creating multi-column filter with OR logic:', {
    searchTerm,
    columns,
    group,
  });

  return {
    group,
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
