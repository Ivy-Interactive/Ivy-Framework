import DataEditor, {
  CompactSelection,
  DataEditorRef,
  GridCell,
  GridSelection,
  Item,
  Theme,
} from '@glideapps/glide-data-grid';
import React, {
  useMemo,
  useCallback,
  useEffect,
  useRef,
  useState,
} from 'react';
import { useTable } from './DataTableContext';
import { tableStyles } from './styles/style';
import { useThemeWithMonitoring } from '@/components/theme-provider';
import { getSelectionProps } from './utils/selectionModes';
import { getCellContent as getCellContentUtil } from './utils/cellContent';
import { convertToGridColumns } from './utils/columnHelpers';
import { iconCellRenderer } from './utils/customRenderers';
import { generateHeaderIcons, addStandardIcons } from './utils/headerIcons';
import { ThemeColors } from '@/lib/color-utils';

interface TableEditorProps {
  hasOptions?: boolean;
}

export const DataTableEditor: React.FC<TableEditorProps> = ({
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
    showColumnTypeIcons,
  } = config;

  const selectionProps = getSelectionProps(selectionMode);

  // Use the enhanced theme hook with custom DataGrid theme generator
  const { customTheme: tableTheme } = useThemeWithMonitoring<Partial<Theme>>({
    monitorDOM: true,
    monitorSystem: true,
    customThemeGenerator: (
      colors: ThemeColors,
      isDark: boolean
    ): Partial<Theme> => ({
      bgCell: colors.background || (isDark ? '#000000' : '#ffffff'),
      bgHeader: colors.background || (isDark ? '#1a1a1f' : '#f9fafb'),
      bgHeaderHasFocus: colors.muted || (isDark ? '#26262b' : '#f3f4f6'),
      bgHeaderHovered: colors.accent || (isDark ? '#26262b' : '#e5e7eb'),
      textHeader: colors.foreground || (isDark ? '#f8f8f8' : '#111827'),
      textDark: colors.foreground || (isDark ? '#f8f8f8' : '#111827'),
      textMedium: colors.mutedForeground || (isDark ? '#a1a1aa' : '#6b7280'),
      textLight: colors.mutedForeground || (isDark ? '#71717a' : '#9ca3af'),
      // bgIconHeader is the background color for icon areas, should be subtle
      bgIconHeader: colors.muted || (isDark ? '#26262b' : '#f3f4f6'),
      // accentColor affects icon foreground colors in headers
      accentColor:
        colors.primary || colors.accent || (isDark ? '#60a5fa' : '#3b82f6'),
      horizontalBorderColor: colors.border || (isDark ? '#404045' : '#d1d5db'),
      linkColor:
        colors.primary || colors.accent || (isDark ? '#3b82f6' : '#2563eb'),
      borderColor: 'transparent',
      cellHorizontalPadding: 16,
      cellVerticalPadding: 8,
      headerIconSize: 20,
      // Add proper text colors for group headers and icons
      textGroupHeader:
        colors.mutedForeground || (isDark ? '#a1a1aa' : '#6b7280'),
      // Icon foreground color
      fgIconHeader: colors.mutedForeground || (isDark ? '#9ca3af' : '#6b7280'),
    }),
  });

  const gridRef = useRef<DataEditorRef>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const [containerWidth, setContainerWidth] = useState<number>(0);
  const [gridSelection, setGridSelection] = useState<GridSelection>({
    columns: CompactSelection.empty(),
    rows: CompactSelection.empty(),
  });
  const scrollThreshold = 10;

  // Generate header icons map for all column icons
  const headerIcons = useMemo(() => {
    const baseIcons = generateHeaderIcons(columns);
    return addStandardIcons(baseIcons);
  }, [columns]);

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
      // Only handle sorting if it's enabled globally
      if (!allowSorting) return;

      // Get visible columns to map the correct column index
      const visibleColumns = columns.filter(c => !c.hidden);
      const column = visibleColumns[col];

      // Check if this specific column is sortable (defaults to true if not specified)
      if (column && (column.sortable ?? true)) {
        handleSort(column.name);
      }
    },
    [columns, handleSort, allowSorting]
  );

  // Handle selection changes
  const handleGridSelectionChange = useCallback(
    (newSelection: GridSelection) => {
      setGridSelection(newSelection);
    },
    []
  );

  // Convert columns to grid format with proper widths
  const gridColumns = convertToGridColumns(
    columns,
    columnOrder,
    columnWidths,
    containerWidth,
    showGroups ?? false,
    showColumnTypeIcons ?? true
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
        headerIcons={headerIcons}
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
        gridSelection={gridSelection}
        onGridSelectionChange={handleGridSelectionChange}
        width={containerWidth}
        rowMarkers={showIndexColumn ? 'number' : 'none'}
        onColumnMoved={allowColumnReordering ? handleColumnReorder : undefined}
        groupHeaderHeight={showGroups ? 36 : undefined}
      />
    </div>
  );
};
