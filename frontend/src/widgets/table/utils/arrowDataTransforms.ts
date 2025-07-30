import * as arrow from 'apache-arrow';
import { DataTableColumn, DataTableData, DataTableRow } from '../types';

export function convertArrowTableToDataTableData(
  table: arrow.Table,
  requestedPageSize?: number
): DataTableData {
  const columns: DataTableColumn[] = table.schema.fields.map(field => ({
    name: field.name,
    type: field.type.toString(),
  }));

  const rows: DataTableRow[] = [];
  for (let i = 0; i < table.numRows; i++) {
    const values: (string | number | boolean | null)[] = [];
    for (let j = 0; j < table.numCols; j++) {
      const column = table.getChildAt(j);
      if (column) {
        const value = column.get(i);
        values.push(value);
      }
    }
    rows.push({ values });
  }

  const hasMore = requestedPageSize
    ? table.numRows >= requestedPageSize
    : false;

  return {
    columns,
    rows,
    totalRows: table.numRows,
    hasMore,
  };
}
