import * as arrow from 'apache-arrow';
import { DataColumn, DataRow } from '../types/types';

function calculateColumnWidth(
  columnName: string,
  columnData: arrow.Vector,
  maxSampleSize = 100
): number {
  const minWidth = 80;
  const maxWidth = 400;
  const charWidth = 8; // Approximate pixel width per character
  const padding = 40; // Cell padding + icons

  // Start with header width
  let maxLength = columnName.length;

  // Sample data to calculate content width
  const sampleSize = Math.min(maxSampleSize, columnData.length);
  for (let i = 0; i < sampleSize; i++) {
    const value = columnData.get(i);
    if (value != null) {
      const strValue = String(value);
      maxLength = Math.max(maxLength, strValue.length);
    }
  }

  const calculatedWidth = maxLength * charWidth + padding;
  return Math.min(Math.max(calculatedWidth, minWidth), maxWidth);
}

export function convertArrowTableToData(
  table: arrow.Table,
  requestedCount: number
): {
  columns: DataColumn[];
  rows: DataRow[];
  hasMore: boolean;
} {
  const columns: DataColumn[] = table.schema.fields.map(
    (field: arrow.Field, index: number) => {
      const columnData = table.getChildAt(index);
      const width = columnData
        ? calculateColumnWidth(field.name, columnData)
        : 150;

      // Parse metadata from Arrow schema
      const metadata = field.metadata;
      const renderType = metadata?.get(
        'render_type'
      ) as DataColumn['renderType'];
      const iconSet = metadata?.get('icon_set') as DataColumn['iconSet'];
      const group = metadata?.get('group') as string | undefined;

      return {
        name: field.name,
        type: field.type.toString(),
        width,
        ...(renderType && { renderType }),
        ...(iconSet && { iconSet }),
        ...(group && { group }),
      };
    }
  );

  const rows: DataRow[] = [];
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

  const hasMore = table.numRows === requestedCount;

  return {
    columns,
    rows,
    hasMore,
  };
}
