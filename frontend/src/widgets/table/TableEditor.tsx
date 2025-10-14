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
import { getIconImage, isValidIconName } from './utils/iconRenderer';
import { getSelectionProps } from './utils/selectionModes';
import { getCellContent as getCellContentUtil } from './utils/cellContent';

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
      return getCellContentUtil(cell, data, columns, columnOrder, editable);
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
        group: showGroups ? col.group : undefined,
      };
    }

    return {
      title: col.name,
      width: baseWidth,
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

        if (!iconName) return false;

        // Validate icon exists
        if (!isValidIconName(iconName)) {
          // Draw error indicator for invalid icon
          ctx.fillStyle = theme.textDark;
          ctx.font = '12px sans-serif';
          ctx.fillText(
            '?',
            rect.x + rect.width / 2 - 4,
            rect.y + rect.height / 2 + 4
          );
          return true;
        }

        // Get icon image (cached or newly created)
        const iconImage = getIconImage(iconName, {
          size: 20,
          color: theme.textDark,
          strokeWidth: 2,
        });

        if (iconImage && iconImage.complete) {
          // Draw the icon centered in the cell
          const iconSize = 20;
          const x = rect.x + (rect.width - iconSize) / 2;
          const y = rect.y + (rect.height - iconSize) / 2;
          ctx.drawImage(iconImage, x, y, iconSize, iconSize);
          return true;
        }

        // If image is not complete, draw placeholder
        ctx.fillStyle = theme.textMedium;
        ctx.beginPath();
        ctx.arc(
          rect.x + rect.width / 2,
          rect.y + rect.height / 2,
          4,
          0,
          2 * Math.PI
        );
        ctx.fill();

        return true;
      },
      // Support copying icon name
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      onPaste: (value: string, data: any) => {
        if (typeof value === 'string' && isValidIconName(value)) {
          return {
            ...data,
            iconName: value,
          };
        }
        return undefined;
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
