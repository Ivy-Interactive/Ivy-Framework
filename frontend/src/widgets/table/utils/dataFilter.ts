import type { DataTableData } from '../types';

export const filterDataTableData = (
  filterValue: string,
  data: DataTableData
): DataTableData => {
  if (!filterValue.trim()) {
    return data;
  }
  console.log('filterDataTableData', filterValue, data);
  const searchTerm = filterValue;

  const filteredRows = data.rows.filter(row =>
    row.values.some(value => {
      if (value === null) return false;
      console.log('filterDataTableData value', String(value), searchTerm);
      const includes = String(value).includes(searchTerm);
      console.log('filterDataTableData value', includes);
      return includes;
    })
  );

  return {
    ...data,
    rows: filteredRows,
    totalRows: filteredRows.length,
    hasMore: false,
  };
};
