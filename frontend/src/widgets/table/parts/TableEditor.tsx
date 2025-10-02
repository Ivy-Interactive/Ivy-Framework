import DataEditor, {
  DataEditorRef,
  GridCell,
  GridCellKind,
  GridColumn,
  Item,
} from '@glideapps/glide-data-grid';
import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useTable } from '../context/TableContext';
import { tableStyles } from '../styles/style';
import { tableTheme } from '../styles/theme';

interface TableEditorProps {
  hasOptions?: boolean;
}

export const TableEditor: React.FC<TableEditorProps> = ({
  hasOptions = false,
}) => {
  const {
    data,
    columns,
    columnWidths,
    visibleRows,
    isLoading,
    hasMore,
    editable,
    config,
    loadMoreData,
    handleColumnResize,
    handleSort,
  } = useTable();

  const {
    allowColumnReordering,
    allowColumnResizing,
    allowCopySelection,
    allowSorting,
    freezeColumns,
  } = config;

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
      const bottomRow = range.y + range.height;
      const shouldLoadMore = bottomRow >= visibleRows - scrollThreshold;
      if (!isLoading && shouldLoadMore && hasMore) {
        loadMoreData();
      }
    },
    [visibleRows, hasMore, loadMoreData, isLoading]
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
      const columnType = columns[col].type.toLowerCase();

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
      // Handle Date and DateTime types (check before numbers since timestamps are numbers)
      if (columnType.includes('date') || columnType.includes('timestamp')) {
        // Arrow can encode dates as milliseconds (number) or ISO strings
        let dateValue: Date | null = null;

        if (typeof cellValue === 'number') {
          dateValue = new Date(cellValue);
        } else if (typeof cellValue === 'string') {
          dateValue = new Date(cellValue);
        }

        // Check if it's a valid date
        if (dateValue && !isNaN(dateValue.getTime())) {
          // Format based on whether it includes time component
          const hasTime =
            columnType.includes('datetime') ||
            columnType.includes('timestamp') ||
            dateValue.getHours() !== 0 ||
            dateValue.getMinutes() !== 0 ||
            dateValue.getSeconds() !== 0;

          const displayData = hasTime
            ? dateValue.toLocaleString()
            : dateValue.toLocaleDateString();

          return {
            kind: GridCellKind.Text,
            data: displayData,
            displayData,
            allowOverlay: editable,
            readonly: !editable,
          };
        }
      }

      // Handle numeric types (int, uint, float, double, decimal, etc.)
      if (typeof cellValue === 'number') {
        // Check if column type indicates a numeric type
        const isNumericType =
          columnType.includes('int') ||
          columnType.includes('float') ||
          columnType.includes('double') ||
          columnType.includes('decimal') ||
          columnType.includes('number');

        if (isNumericType) {
          // Format floating point numbers with appropriate decimals
          const displayData = Number.isInteger(cellValue)
            ? cellValue.toString()
            : cellValue.toFixed(2);

          return {
            kind: GridCellKind.Number,
            data: cellValue,
            displayData,
            allowOverlay: editable,
            readonly: !editable,
          };
        }
      }

      // Handle boolean types
      if (typeof cellValue === 'boolean') {
        return {
          kind: GridCellKind.Boolean,
          data: cellValue,
          allowOverlay: false,
          readonly: !editable,
        };
      }

      // Default to text for strings and other types
      return {
        kind: GridCellKind.Text,
        data: String(cellValue),
        displayData: String(cellValue),
        allowOverlay: editable,
        readonly: !editable,
      };
    },
    [data, columns, editable]
  );

  // Handle column header click for sorting
  const handleHeaderMenuClick = useCallback(
    (col: number) => {
      // Only handle sorting if it's enabled
      if (!allowSorting) return;

      const columnName = columns[col]?.name;
      if (columnName) {
        handleSort(columnName);
      }
    },
    [columns, handleSort, allowSorting]
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
        width: Math.max(baseWidth, remainingWidth) - 10,
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

  const containerStyle = hasOptions
    ? tableStyles.tableEditor.gridContainerWithOptions
    : tableStyles.tableEditor.gridContainer;

  return (
    <div ref={containerRef} style={containerStyle}>
      <DataEditor
        ref={gridRef}
        columns={gridColumns}
        rows={visibleRows}
        getCellContent={getCellContent}
        onColumnResize={allowColumnResizing ? handleColumnResize : undefined}
        onVisibleRegionChanged={handleVisibleRegionChanged}
        onHeaderClicked={allowSorting ? handleHeaderMenuClick : undefined}
        smoothScrollX={true}
        smoothScrollY={true}
        theme={tableTheme}
        rowHeight={38}
        headerHeight={32}
        freezeColumns={freezeColumns ?? 0}
        getCellsForSelection={(allowCopySelection ?? true) ? true : undefined}
        keybindings={{ search: false }}
        columnSelect="none"
        rangeSelect="rect"
        width={containerWidth}
        // TODO: inmplement handler for onColumnMoved
        onColumnMoved={allowColumnReordering ? () => {} : undefined}
      />
    </div>
  );
};
