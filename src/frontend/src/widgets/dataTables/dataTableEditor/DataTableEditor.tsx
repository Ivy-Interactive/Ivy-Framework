import React, { useMemo, useRef } from "react";
import { CustomRenderer, DataEditorRef } from "@glideapps/glide-data-grid";
import { useTable } from "../dataTableContext";
import { getSelectionProps } from "../utils/selectionModes";
import { iconCellRenderer, linkCellRenderer } from "../utils/customRenderers";
import { generateHeaderIcons, addStandardIcons } from "../utils/headerIcons";
import {
  useContainerSize,
  useSearch,
  useTableTheme,
  useGridSelection,
  useCellInteractions,
  useRowHover,
  useEmptyRows,
  useDataLoading,
} from "../hooks";
import { useFooterColumnLayout } from "../hooks/useFooterColumnLayout";
import { GridContainer } from "../components/GridContainer";
import { AggregateFooter } from "../DataTableFooter";
import { MenuItem } from "@/types/widgets";
import { ROW_HEIGHT, GROUP_HEADER_HEIGHT } from "./constants";
import { useCellContent, useGridColumns, useHeaderMenu } from "./hooks";
import { getOrderedVisibleDataColumns } from "../utils/columnHelpers";

interface TableEditorProps {
  widgetId: string;
  hasOptions?: boolean;
  rowActions?: MenuItem[];
  footer?: React.ReactNode;
  showAggregateFooter?: boolean;
}

export const DataTableEditor: React.FC<TableEditorProps> = ({
  widgetId,
  hasOptions = false,
  rowActions,
  footer,
  showAggregateFooter = false,
}) => {
  const {
    columns,
    columnWidths,
    visibleRows,
    isLoading,
    hasMore,
    editable,
    config,
    columnOrder,
    getRowData,
    arrowTableRef,
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

  const { containerRef, containerWidth, containerHeight, scrollContainerHeight } =
    useContainerSize();

  // Search functionality
  const { showSearch, setShowSearch } = useSearch(showSearchConfig ?? false);

  // Grid ref
  const gridRef = useRef<DataEditorRef | null>(null);

  // Cell content
  const { getCellContent } = useCellContent({
    columns,
    columnOrder,
    editable,
    visibleRows,
    getRowData,
  });

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
  const { hoverRow, actionButtonsTop, actionButtonsHeight, onItemHovered, handleRowActionClick } =
    useRowHover({
      widgetId,
      visibleRows,
      enableRowHover: enableRowHover ?? false,
      rowActions,
      containerRef,
      arrowTableRef,
    });

  // Table theme
  const { tableTheme, getRowThemeOverride } = useTableTheme({
    showVerticalBorders: showVerticalBorders ?? false,
    enableRowHover: enableRowHover ?? false,
    visibleRows,
    hoverRow,
  });

  const { emptyRowsCount, totalRows } = useEmptyRows({
    scrollContainerHeight,
    visibleRows,
    hasMore,
    showGroups: showGroups ?? false,
    rowHeight: ROW_HEIGHT,
  });

  // Data loading
  const { handleVisibleRegionChanged } = useDataLoading({
    containerRef,
    visibleRows,
    isLoading,
    hasMore,
    loadMoreData,
    rowHeight: ROW_HEIGHT,
  });

  // Generate header icons map for all column icons
  const headerIcons = useMemo(() => {
    const baseIcons = generateHeaderIcons(columns);
    return addStandardIcons(baseIcons);
  }, [columns]);

  // Header menu handling
  const { handleHeaderMenuClick } = useHeaderMenu({
    columns,
    allowSorting: allowSorting ?? true,
    handleSort,
  });

  // Grid columns configuration (last column uses grow:1 to fill space; no manual
  // scrollbar-width subtraction needed)
  const {
    columns: finalColumns,
    shouldUseColumnGroups,
    onGroupHeaderClicked,
  } = useGridColumns({
    columns,
    columnOrder,
    columnWidths,
    containerWidth,
    showGroups: showGroups ?? false,
    showColumnTypeIcons: showColumnTypeIcons ?? true,
  });

  const orderedDataColumns = useMemo(
    () => getOrderedVisibleDataColumns(columns, columnOrder),
    [columns, columnOrder],
  );

  const wantAggregateFooter =
    showAggregateFooter && orderedDataColumns.some((c) => c.footer && c.footer.length > 0);

  const { layout, footerScrollRef } = useFooterColumnLayout(
    containerRef,
    finalColumns,
    getCellContent,
    totalRows,
    containerWidth,
    tableTheme,
    showIndexColumn ?? false,
    visibleRows,
    wantAggregateFooter,
  );

  if (finalColumns.length === 0) {
    return null;
  }

  const footerNode = wantAggregateFooter ? (
    <AggregateFooter
      columns={orderedDataColumns}
      layout={layout}
      footerScrollRef={footerScrollRef}
    />
  ) : (
    footer
  );

  return (
    <GridContainer
      gridRef={gridRef}
      containerRef={containerRef}
      hasOptions={hasOptions}
      columns={finalColumns}
      rows={totalRows}
      getCellContent={getCellContent}
      customRenderers={[iconCellRenderer, linkCellRenderer] as unknown as readonly CustomRenderer[]}
      headerIcons={headerIcons}
      onColumnResize={allowColumnResizing ? handleColumnResize : undefined}
      onVisibleRegionChanged={handleVisibleRegionChanged}
      onHeaderClicked={allowSorting ? handleHeaderMenuClick : undefined}
      theme={tableTheme}
      rowHeight={ROW_HEIGHT}
      headerHeight={ROW_HEIGHT}
      freezeColumns={freezeColumns ?? 0}
      getCellsForSelection={(allowCopySelection ?? true) ? true : undefined}
      rowSelect={selectionProps.rowSelect}
      columnSelect={selectionProps.columnSelect}
      rangeSelect={selectionProps.rangeSelect}
      gridSelection={gridSelection}
      onGridSelectionChange={handleGridSelectionChange}
      width={containerWidth}
      height={containerHeight > 0 ? containerHeight : undefined}
      rowMarkers={showIndexColumn ? "number" : "none"}
      onColumnMoved={allowColumnReordering ? handleColumnReorder : undefined}
      groupHeaderHeight={showGroups ? GROUP_HEADER_HEIGHT : undefined}
      onCellClicked={handleCellClicked}
      onCellActivated={handleCellActivated}
      onGroupHeaderClicked={shouldUseColumnGroups ? onGroupHeaderClicked : undefined}
      showSearch={showSearchConfig ? showSearch : false}
      onSearchClose={() => setShowSearch(false)}
      onItemHovered={enableRowHover ? onItemHovered : undefined}
      getRowThemeOverride={enableRowHover || emptyRowsCount > 0 ? getRowThemeOverride : undefined}
      rowActions={rowActions}
      actionButtonsTop={actionButtonsTop}
      actionButtonsHeight={actionButtonsHeight}
      hoverRow={hoverRow}
      onRowActionClick={handleRowActionClick}
      footer={footerNode}
      hasEmptyRows={emptyRowsCount > 0}
    />
  );
};
