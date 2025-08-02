import DataEditor, {
  DataEditorRef,
  GridCell,
  GridCellKind,
  GridColumn,
  Item,
} from '@glideapps/glide-data-grid';
import React, { useCallback, useRef } from 'react';
import { useTable } from '../context/TableContext';
import { tableStyles } from '../styles';
import { tableTheme } from '../styles/theme';

export const TableEditor: React.FC = () => {
  const {
    data,
    columns,
    columnWidths,
    visibleRows,
    hasMore,
    editable,
    loadMoreData,
    handleColumnResize,
  } = useTable();

  const scrollTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const gridRef = useRef<DataEditorRef>(null);
  const scrollThreshold = 10;

  // Handle scroll events
  const handleVisibleRegionChanged = useCallback(
    (range: { x: number; y: number; width: number; height: number }) => {
      // Clear any existing timeout
      if (scrollTimeoutRef.current) {
        clearTimeout(scrollTimeoutRef.current);
      }

      // Debounce the scroll check
      scrollTimeoutRef.current = setTimeout(() => {
        const bottomRow = range.y + range.height;
        const shouldLoadMore = bottomRow >= visibleRows - scrollThreshold;

        if (shouldLoadMore && hasMore) {
          loadMoreData();
        }
      }, 100);
    },
    [visibleRows, hasMore, loadMoreData]
  );

  // Get cell content
  const getCellContent = useCallback(
    (cell: Item): GridCell => {
      const [col, row] = cell;

      // Safety check
      if (row >= data.length || col >= columns.length) {
        return {
          kind: GridCellKind.Text,
          data: '',
          displayData: '',
          allowOverlay: false,
          readonly: true,
        };
      }

      const rowData = data[row];
      const cellValue = rowData.values[col];
      const columnType = columns[col].type;

      // Handle null/undefined values
      if (cellValue === null || cellValue === undefined) {
        return {
          kind: GridCellKind.Text,
          data: '',
          displayData: 'null',
          allowOverlay: editable,
          readonly: !editable,
          style: 'faded',
        };
      }

      // Determine cell type based on Arrow data type and value
      if (typeof cellValue === 'number' && columnType.includes('int')) {
        return {
          kind: GridCellKind.Number,
          data: cellValue,
          displayData: cellValue.toString(),
          allowOverlay: editable,
          readonly: !editable,
        };
      } else if (typeof cellValue === 'boolean') {
        return {
          kind: GridCellKind.Boolean,
          data: cellValue,
          allowOverlay: false,
          readonly: !editable,
        };
      } else {
        // Default to text for strings and other types
        return {
          kind: GridCellKind.Text,
          data: String(cellValue),
          displayData: String(cellValue),
          allowOverlay: editable,
          readonly: !editable,
        };
      }
    },
    [data, columns, editable]
  );

  // Convert our columns to GridColumn format with current widths
  const gridColumns: GridColumn[] = columns.map((col, index) => ({
    title: col.name,
    width: columnWidths[index.toString()] || col.width,
  }));

  if (gridColumns.length === 0) {
    return null;
  }

  return (
    <div style={tableStyles.gridContainer}>
      <DataEditor
        ref={gridRef}
        columns={gridColumns}
        rows={visibleRows}
        getCellContent={getCellContent}
        onColumnResize={handleColumnResize}
        onVisibleRegionChanged={handleVisibleRegionChanged}
        smoothScrollX={true}
        smoothScrollY={true}
        theme={tableTheme}
        rowHeight={36}
        headerHeight={36}
        freezeColumns={1}
        getCellsForSelection={true}
        keybindings={{ search: false }}
        rightElement={<div className={tableStyles.rightPadding} />}
        columnSelect="single"
        rangeSelect="rect"
      />
    </div>
  );
};
