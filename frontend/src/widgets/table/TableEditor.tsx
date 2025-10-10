import DataEditor, {
  DataEditorRef,
  GridCell,
  GridCellKind,
  GridColumn,
  Item,
  CustomRenderer,
} from '@glideapps/glide-data-grid';
import React, {
  useCallback,
  useEffect,
  useRef,
  useState,
  useMemo,
} from 'react';
import { useTable } from './TableContext';
import { tableStyles } from './styles/style';
import { tableTheme } from './styles/theme';
import { loadIconImage, getCachedIcon } from './utils/iconRenderer';
import { getColumnTypeIcon } from './utils/columnHelpers';
import { getSelectionProps } from './utils/selectionModes';

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
    columnOrder,
    loadMoreData,
    handleColumnResize,
    handleSort,
    handleColumnReorder,
  } = useTable();

  const {
    allowColumnReordering,
    allowColumnResizing,
    allowCopySelection,
    allowSorting,
    freezeColumns,
    showIndexColumn,
    selectionMode,
    showGroups,
  } = config;

  const selectionProps = getSelectionProps(selectionMode);

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

      // Apply column order mapping
      const orderedCols =
        columnOrder.length === columns.length
          ? columnOrder.map(idx => columns[idx])
          : columns;

      // Safety check
      if (row >= data.length || col >= orderedCols.length) {
        return {
          kind: GridCellKind.Text,
          data: '',
          displayData: '',
          allowOverlay: false,
          readonly: true,
        };
      }

      const rowData = data[row];
      const originalColumnIndex = columns.indexOf(orderedCols[col]);
      const cellValue = rowData.values[originalColumnIndex];
      const columnType = orderedCols[col].type.toLowerCase();

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

      // Handle icon types - detect by checking if value looks like a Lucide icon name
      // Icons are PascalCase strings that match known icon names
      const isProbablyIcon =
        typeof cellValue === 'string' &&
        /^[A-Z][a-zA-Z0-9]*$/.test(cellValue) &&
        cellValue.length > 2 &&
        // Only treat as icon if it's likely to be an icon value (not regular text)
        // This heuristic works because Lucide icons are all PascalCase without spaces
        !cellValue.includes(' ');

      if (isProbablyIcon) {
        return {
          kind: GridCellKind.Custom,
          allowOverlay: false,
          readonly: true,
          copyData: String(cellValue),
          data: {
            kind: 'icon-cell',
            iconName: String(cellValue),
          },
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
    [data, columns, columnOrder, editable]
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
  // Apply column order if available
  const orderedColumns =
    columnOrder.length === columns.length
      ? columnOrder.map(idx => columns[idx])
      : columns;

  const gridColumns: GridColumn[] = orderedColumns.map((col, index) => {
    const originalIndex = columns.indexOf(col);
    const baseWidth = columnWidths[originalIndex.toString()] || col.width;
    const icon = getColumnTypeIcon(col.type);

    // Make the last column fill the remaining space
    if (index === orderedColumns.length - 1 && containerWidth > 0) {
      const totalWidthOfOtherColumns = orderedColumns
        .slice(0, -1)
        .reduce((sum, c) => {
          const idx = columns.indexOf(c);
          return sum + (columnWidths[idx.toString()] || c.width);
        }, 0);

      const remainingWidth = containerWidth - totalWidthOfOtherColumns;
      return {
        title: col.name,
        width: Math.max(baseWidth, remainingWidth) - 10,
        icon,
        group: showGroups ? col.group : undefined,
      };
    }

    return {
      title: col.name,
      width: baseWidth,
      icon,
      group: showGroups ? col.group : undefined,
    };
  });

  // Custom cell renderer for icons
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const iconCellRenderer: CustomRenderer<any> = useMemo(
    () => ({
      kind: GridCellKind.Custom,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      isMatch: (cell: any): cell is any =>
        cell.kind === GridCellKind.Custom && cell.data?.kind === 'icon-cell',
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      draw: (args: any, cell: any) => {
        const { ctx, rect, theme } = args;
        const iconName = cell.data?.iconName;

        if (!iconName) return true;

        // Check if the icon is already cached
        const iconImage = getCachedIcon(iconName);

        if (iconImage) {
          // Draw the icon centered in the cell
          const iconSize = 20;
          const x = rect.x + (rect.width - iconSize) / 2;
          const y = rect.y + (rect.height - iconSize) / 2;
          ctx.drawImage(iconImage, x, y, iconSize, iconSize);
        } else {
          // If icon is not loaded yet, draw a placeholder and trigger loading
          ctx.fillStyle = theme.textMedium;
          ctx.beginPath();
          ctx.arc(
            rect.x + rect.width / 2,
            rect.y + rect.height / 2,
            6,
            0,
            2 * Math.PI
          );
          ctx.fill();

          // Trigger icon loading in the background
          // Note: The grid will automatically redraw on next frame or scroll
          loadIconImage(iconName);
        }

        return true;
      },
    }),
    []
  );

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
        customRenderers={[iconCellRenderer]}
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
        rowSelect={selectionProps.rowSelect}
        columnSelect={selectionProps.columnSelect}
        rangeSelect={selectionProps.rangeSelect}
        width={containerWidth}
        rowMarkers={showIndexColumn ? 'number' : 'none'}
        onColumnMoved={allowColumnReordering ? handleColumnReorder : undefined}
        groupHeaderHeight={showGroups ? 36 : undefined}
      />
    </div>
  );
};
