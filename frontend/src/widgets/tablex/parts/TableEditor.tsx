import DataEditor, {
  DataEditorRef,
  GridCell,
  GridCellKind,
  GridColumn,
  Item,
} from '@glideapps/glide-data-grid';
import React, { useCallback, useRef, useState, useEffect } from 'react';
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
  const containerRef = useRef<HTMLDivElement>(null);
  const [containerWidth, setContainerWidth] = useState<number>(0);
  const scrollThreshold = 10;

  // Track container width
  useEffect(() => {
    if (!containerRef.current) return;

    const resizeObserver = new ResizeObserver(entries => {
      for (const entry of entries) {
        setContainerWidth(entry.contentRect.width);
      }
    });

    resizeObserver.observe(containerRef.current);

    return () => {
      resizeObserver.disconnect();
    };
  }, []);

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
  const gridColumns: GridColumn[] = columns.map((col, index) => {
    const baseWidth = columnWidths[index.toString()] || col.width;

    // Make the last column fill the remaining space
    if (index === columns.length - 1 && containerWidth > 0) {
      const totalWidthOfOtherColumns = columns
        .slice(0, -1)
        .reduce(
          (sum, _, idx) =>
            sum + (columnWidths[idx.toString()] || columns[idx].width),
          0
        );

      const remainingWidth = containerWidth - totalWidthOfOtherColumns;
      return {
        title: col.name,
        width: Math.max(baseWidth, remainingWidth),
      };
    }

    return {
      title: col.name,
      width: baseWidth,
    };
  });

  if (gridColumns.length === 0) {
    return null;
  }

  return (
    <div ref={containerRef} style={tableStyles.gridContainer}>
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
        columnSelect="single"
        rangeSelect="rect"
        width={containerWidth}
      />
    </div>
  );
};
