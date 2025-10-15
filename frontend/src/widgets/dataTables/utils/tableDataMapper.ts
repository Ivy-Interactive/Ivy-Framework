import * as arrow from 'apache-arrow';
import {
  DataColumn,
  DataRow,
  ColType,
  SortDirection,
  Align,
} from '../types/types';

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

/**
 * Maps Arrow field type to ColType enum
 * If render_type metadata is provided, it takes precedence
 */
function mapArrowTypeToColType(
  arrowType: string,
  renderType?: string
): ColType {
  // If render_type is explicitly set, use it (takes precedence)
  if (renderType) {
    switch (renderType.toLowerCase()) {
      case 'icon':
        return ColType.Icon;
      case 'text':
        return ColType.Text;
      case 'number':
        return ColType.Number;
      case 'boolean':
        return ColType.Boolean;
      case 'date':
        return ColType.Date;
      case 'datetime':
        return ColType.DateTime;
    }
  }

  // Otherwise infer from Arrow type
  const lowerType = arrowType.toLowerCase();
  if (
    lowerType.includes('int') ||
    lowerType.includes('float') ||
    lowerType.includes('double') ||
    lowerType.includes('decimal')
  ) {
    return ColType.Number;
  }
  if (lowerType.includes('bool')) {
    return ColType.Boolean;
  }
  if (lowerType.includes('date') || lowerType.includes('timestamp')) {
    return lowerType.includes('timestamp') ? ColType.DateTime : ColType.Date;
  }
  // Default to Text for strings and unknown types
  return ColType.Text;
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
      const renderType = metadata?.get('render_type') as string | undefined;
      const iconSet = metadata?.get('icon_set') as DataColumn['iconSet'];
      const group = metadata?.get('group') as string | undefined;
      const header = metadata?.get('header') as string | undefined;
      const icon = metadata?.get('icon') as string | undefined;
      const help = metadata?.get('help') as string | undefined;

      // Parse boolean properties
      const hidden = metadata?.get('hidden') === 'true';
      const sortable = metadata?.get('sortable');
      const filterable = metadata?.get('filterable');

      // Parse sort direction
      const sortDirectionStr = metadata?.get('sort_direction') as
        | string
        | undefined;
      let sortDirection: SortDirection | undefined;
      if (sortDirectionStr) {
        switch (sortDirectionStr.toLowerCase()) {
          case 'ascending':
            sortDirection = SortDirection.Ascending;
            break;
          case 'descending':
            sortDirection = SortDirection.Descending;
            break;
          case 'none':
            sortDirection = SortDirection.None;
            break;
        }
      }

      // Parse alignment
      const alignStr = metadata?.get('align') as string | undefined;
      let align: Align | undefined;
      if (alignStr) {
        switch (alignStr.toLowerCase()) {
          case 'left':
            align = Align.Left;
            break;
          case 'center':
            align = Align.Center;
            break;
          case 'right':
            align = Align.Right;
            break;
        }
      }

      // Parse order (column display order)
      const orderStr = metadata?.get('order') as string | undefined;
      const order = orderStr ? parseInt(orderStr, 10) : undefined;

      // Map Arrow type to ColType, with render_type taking precedence
      const type = mapArrowTypeToColType(field.type.toString(), renderType);

      return {
        name: field.name,
        type,
        width,
        ...(header && { header }),
        ...(group && { group }),
        ...(hidden && { hidden }),
        ...(sortable !== undefined && { sortable: sortable === 'true' }),
        ...(sortDirection && { sortDirection }),
        ...(filterable !== undefined && { filterable: filterable === 'true' }),
        ...(align && { align }),
        ...(order !== undefined && !isNaN(order) && { order }),
        ...(icon !== undefined && { icon: icon === 'null' ? null : icon }),
        ...(help !== undefined && { help: help === 'null' ? null : help }),
        ...(iconSet && { iconSet }),
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
