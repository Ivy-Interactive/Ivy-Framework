import React, { useCallback, useMemo, useRef } from "react";
import { CustomRenderer, DataEditorRef, GridMouseEventArgs } from "@glideapps/glide-data-grid";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import { useTable } from "../dataTableContext";
import { getSelectionProps } from "../utils/selectionModes";
import {
  iconCellRenderer,
  labelsBadgesCellRenderer,
  linkCellRenderer,
} from "../utils/customRenderers";
import { generateHeaderIcons, addStandardIcons } from "../utils/headerIcons";
import {
  useContainerSize,
  useSearch,
  useSearchNavigation,
  useTableTheme,
  useGridSelection,
  useCellInteractions,
  useRowHover,
  useEmptyRows,
  useDataLoading,
  useLinkCellHover,
} from "../hooks";
import { useFooterColumnLayout } from "../hooks/useFooterColumnLayout";
import { GridContainer } from "../components/GridContainer";
import { AggregateFooter } from "../DataTableFooter";
import { MenuItem } from "@/types/widgets";
import { DENSITY_CONFIG } from "./constants";
import { useCellContent, useGridColumns, useHeaderMenu } from "./hooks";
import { getOrderedVisibleDataColumns } from "../utils/columnHelpers";

interface TableEditorProps {
  widgetId: string;
  events: string[];
  hasOptions?: boolean;
  rowActions?: MenuItem[];
  footer?: React.ReactNode;
  showAggregateFooter?: boolean;
}

export const DataTableEditor: React.FC<TableEditorProps> = ({
  widgetId,
  events,
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
    density,
    getRowData,
    arrowTableRef,
    loadMoreData,
    handleColumnResize,
    handleSort,
    handleColumnReorder,
  } = useTable();

  const densityConfig = DENSITY_CONFIG[density];

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
  const { gridSelection, handleGridSelectionChange, setGridSelection } = useGridSelection({
    visibleRows,
    getCellContent,
  });

  // Cell interactions
  const { handleCellClicked, handleCellActivated } = useCellInteractions({
    widgetId,
    events,
    columns,
    visibleRows,
    enableCellClickEvents: enableCellClickEvents ?? false,
    getCellContent,
    arrowTableRef,
  });

  // Row hover and actions
  const { hoverRow, actionButtonsTop, actionButtonsHeight, onItemHovered, handleRowActionClick } =
    useRowHover({
      widgetId,
      events,
      visibleRows,
      enableRowHover: enableRowHover ?? false,
      rowActions,
      containerRef,
      arrowTableRef,
    });

  // Link cell hover tooltip
  const {
    isLinkHovered,
    virtualRef,
    onItemHovered: onLinkCellHovered,
    linkTooltipPos,
  } = useLinkCellHover({
    getCellContent,
    visibleRows,
  });

  // Compose onItemHovered: always call link hover, additionally call row hover when enabled
  const handleItemHovered = useCallback(
    (args: GridMouseEventArgs) => {
      onLinkCellHovered(args);
      onItemHovered(args);
    },
    [onLinkCellHovered, onItemHovered],
  );

  // Table theme
  const { tableTheme, getRowThemeOverride, isDark } = useTableTheme({
    showVerticalBorders: showVerticalBorders ?? false,
    enableRowHover: enableRowHover ?? false,
    visibleRows,
    hoverRow,
    density,
  });

  const { onSearchResultsChanged, onSearchClose, highlightRegions } = useSearchNavigation(
    gridRef,
    containerRef,
    setGridSelection,
    isDark,
    showSearch,
    setShowSearch,
  );

  const { emptyRowsCount, totalRows } = useEmptyRows({
    scrollContainerHeight,
    visibleRows,
    hasMore,
    showGroups: showGroups ?? false,
    rowHeight: densityConfig.rowHeight,
  });

  // Data loading
  const { handleVisibleRegionChanged } = useDataLoading({
    containerRef,
    visibleRows,
    isLoading,
    hasMore,
    loadMoreData,
    rowHeight: densityConfig.rowHeight,
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

  const isMac = typeof navigator !== "undefined" && /Mac|iPhone|iPad/.test(navigator.platform);
  const tooltipLabel = isMac ? "\u2318+click to open link" : "Ctrl+click to open link";

  const linkTooltipNode = (
    <TooltipProvider
      key={linkTooltipPos ? `${linkTooltipPos.x},${linkTooltipPos.y}` : "hidden"}
      delayDuration={0}
    >
      <Tooltip open={isLinkHovered}>
        <TooltipTrigger asChild>
          <div
            ref={(node) => {
              if (node) {
                node.getBoundingClientRect = virtualRef.getBoundingClientRect;
              }
            }}
            style={{ position: "absolute", top: 0, left: 0, pointerEvents: "none" }}
          />
        </TooltipTrigger>
        <TooltipContent side="top" className="pointer-events-none">
          {tooltipLabel}
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );

  return (
    <>
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
            labelsBadgesCellRenderer,
          ] as unknown as readonly CustomRenderer[]
        }
        headerIcons={headerIcons}
        onColumnResize={allowColumnResizing ? handleColumnResize : undefined}
        onVisibleRegionChanged={handleVisibleRegionChanged}
        onHeaderClicked={allowSorting ? handleHeaderMenuClick : undefined}
        theme={tableTheme}
        rowHeight={densityConfig.rowHeight}
        headerHeight={densityConfig.rowHeight}
        freezeColumns={freezeColumns ?? 0}
        getCellsForSelection={(allowCopySelection ?? true) ? true : undefined}
        rowSelect={selectionProps.rowSelect}
        columnSelect={selectionProps.columnSelect}
        rangeSelect={selectionProps.rangeSelect}
        gridSelection={gridSelection}
        onGridSelectionChange={handleGridSelectionChange}
        height={
          containerHeight > 0 ? containerHeight : containerRef.current?.clientHeight || undefined
        }
        rowMarkers={showIndexColumn ? "number" : "none"}
        onColumnMoved={allowColumnReordering ? handleColumnReorder : undefined}
        groupHeaderHeight={showGroups ? densityConfig.groupHeaderHeight : undefined}
        onCellClicked={handleCellClicked}
        onCellActivated={handleCellActivated}
        onGroupHeaderClicked={shouldUseColumnGroups ? onGroupHeaderClicked : undefined}
        showSearch={showSearchConfig ? showSearch : false}
        onSearchClose={onSearchClose}
        onSearchResultsChanged={showSearchConfig ? onSearchResultsChanged : undefined}
        highlightRegions={showSearchConfig ? highlightRegions : undefined}
        onItemHovered={handleItemHovered}
        getRowThemeOverride={enableRowHover || emptyRowsCount > 0 ? getRowThemeOverride : undefined}
        rowActions={rowActions}
        actionButtonsTop={actionButtonsTop}
        actionButtonsHeight={actionButtonsHeight}
        hoverRow={hoverRow}
        onRowActionClick={handleRowActionClick}
        footer={footerNode}
        hasEmptyRows={emptyRowsCount > 0}
      />
      {linkTooltipNode}
    </>
  );
};
