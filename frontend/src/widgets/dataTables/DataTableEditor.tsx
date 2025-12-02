import {
  CustomRenderer,
  DataEditorRef,
  GridCell,
  GridCellKind,
  Item,
} from '@glideapps/glide-data-grid';
import React, { useMemo, useCallback, useRef } from 'react';
import { useTable } from './DataTableContext';
import { getSelectionProps } from './utils/selectionModes';
import { getCellContent as getCellContentUtil } from './utils/cellContent';
import { convertToGridColumns } from './utils/columnHelpers';
import { iconCellRenderer, linkCellRenderer } from './utils/customRenderers';
import { generateHeaderIcons, addStandardIcons } from './utils/headerIcons';
import { useColumnGroups } from './hooks/useColumnGroups';
import {
  useContainerSize,
  useSearch,
  useTableTheme,
  useGridSelection,
  useCellInteractions,
  useRowHover,
  useEmptyRows,
  useDataLoading,
} from './hooks';
import { GridContainer } from './components/GridContainer';
import { MenuItem } from '@/types/widgets';

interface TableEditorProps {
  widgetId: string;
  hasOptions?: boolean;
  rowActions?: MenuItem[];
  footer?: React.ReactNode;
}

const rowHeight = 38;
const GROUP_HEADER_HEIGHT = 36;

export const DataTableEditor: React.FC<TableEditorProps> = ({
  widgetId,
  hasOptions = false,
  rowActions,
  footer,
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
    enableCellClickEvents,
    showSearch: showSearchConfig,
    showColumnTypeIcons,
    showVerticalBorders,
    enableRowHover,
  } = config;

  const selectionProps = getSelectionProps(selectionMode);

  // Container sizing
  const { containerRef, containerWidth, containerHeight } = useContainerSize();

  // Search functionality
  const { showSearch, setShowSearch } = useSearch(showSearchConfig ?? false);

  // Grid ref
  const gridRef = useRef<DataEditorRef | null>(null);

  // Get cell content
  const getCellContent = useCallback(
    (cell: Item): GridCell => {
      const [, row] = cell;
      // If this is an empty filler row, return empty text cell
      if (row >= visibleRows) {
        return {
          kind: GridCellKind.Text,
          data: '',
          displayData: '',
          allowOverlay: false,
        };
      }
      return getCellContentUtil(cell, data, columns, columnOrder, editable);
    },
    [data, columns, columnOrder, editable, visibleRows]
  );

  // Grid selection
  const { gridSelection, handleGridSelectionChange } = useGridSelection({
    visibleRows,
    getCellContent,
  });

  // Cell interactions
  const { handleCellClicked, handleCellActivated } = useCellInteractions({
    widgetId,
    columns,
    visibleRows,
    enableCellClickEvents: enableCellClickEvents ?? false,
    getCellContent,
  });

  // Row hover and actions
  const { hoverRow, actionButtonsTop, onItemHovered, handleRowActionClick } =
    useRowHover({
      widgetId,
      columns,
      visibleRows,
      enableRowHover: enableRowHover ?? false,
      rowActions,
      gridRef,
      containerRef,
      getCellContent,
    });

  // Table theme
  const { tableTheme, getRowThemeOverride } = useTableTheme({
    showVerticalBorders: showVerticalBorders ?? false,
    enableRowHover: enableRowHover ?? false,
    visibleRows,
    hoverRow,
  });

  // Empty rows calculation
  const { emptyRowsCount, totalRows } = useEmptyRows({
    containerHeight,
    visibleRows,
    hasMore,
    showGroups: showGroups ?? false,
    rowHeight,
  });

  // Data loading
  const { handleVisibleRegionChanged } = useDataLoading({
    containerRef,
    visibleRows,
    isLoading,
    hasMore,
    loadMoreData,
    rowHeight,
  });

  // Generate header icons map for all column icons
  const headerIcons = useMemo(() => {
    const baseIcons = generateHeaderIcons(columns);
    return addStandardIcons(baseIcons);
  }, [columns]);

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

  // Convert columns to grid format with proper widths
  const gridColumns = convertToGridColumns(
    columns,
    columnOrder,
    columnWidths,
    containerWidth,
    showGroups ?? false,
    showColumnTypeIcons ?? true
  );

  // Use column groups hook when showGroups is enabled
  const columnGroupsHook = useColumnGroups(gridColumns);
  const shouldUseColumnGroups = showGroups ?? false;

  // Use grouped columns if showGroups is enabled, otherwise use regular columns
  const finalColumns = shouldUseColumnGroups
    ? columnGroupsHook.columns
    : gridColumns;

  if (finalColumns.length === 0) {
    return null;
  }

  return (
    <GridContainer
      gridRef={gridRef}
      containerRef={containerRef}
      hasOptions={hasOptions}
      columns={finalColumns}
      rows={totalRows}
      getCellContent={getCellContent}
      customRenderers={
        [
          iconCellRenderer,
          linkCellRenderer,
        ] as unknown as readonly CustomRenderer[]
      }
      headerIcons={headerIcons}
      onColumnResize={allowColumnResizing ? handleColumnResize : undefined}
      onVisibleRegionChanged={handleVisibleRegionChanged}
      onHeaderClicked={allowSorting ? handleHeaderMenuClick : undefined}
      theme={tableTheme}
      rowHeight={rowHeight}
      headerHeight={rowHeight}
      freezeColumns={freezeColumns ?? 0}
      getCellsForSelection={(allowCopySelection ?? true) ? true : undefined}
      rowSelect={selectionProps.rowSelect}
      columnSelect={selectionProps.columnSelect}
      rangeSelect={selectionProps.rangeSelect}
      gridSelection={gridSelection}
      onGridSelectionChange={handleGridSelectionChange}
      width={containerWidth}
      rowMarkers={showIndexColumn ? 'number' : 'none'}
      onColumnMoved={allowColumnReordering ? handleColumnReorder : undefined}
      groupHeaderHeight={showGroups ? GROUP_HEADER_HEIGHT : undefined}
      onCellClicked={handleCellClicked}
      onCellActivated={handleCellActivated}
      onGroupHeaderClicked={
        shouldUseColumnGroups
          ? columnGroupsHook.onGroupHeaderClicked
          : undefined
      }
      showSearch={showSearchConfig ? showSearch : false}
      onSearchClose={() => setShowSearch(false)}
      onItemHovered={enableRowHover ? onItemHovered : undefined}
      getRowThemeOverride={
        enableRowHover || emptyRowsCount > 0 ? getRowThemeOverride : undefined
      }
      rowActions={rowActions}
      actionButtonsTop={actionButtonsTop}
      hoverRow={hoverRow}
      onRowActionClick={handleRowActionClick}
      footer={footer}
    />
  );
};
