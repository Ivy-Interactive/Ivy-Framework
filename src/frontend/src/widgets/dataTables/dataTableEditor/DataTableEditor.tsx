import React, { useCallback, useEffect, useMemo, useRef } from "react";
import {
  CompactSelection,
  CustomRenderer,
  DataEditorRef,
  getDefaultTheme,
  GridMouseCellEventArgs,
  GridMouseEventArgs,
  Item,
} from "@glideapps/glide-data-grid";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";
import { useTable } from "../dataTableContext";
import { getSelectionProps } from "../utils/selectionModes";
import {
  animatedStatusCellRenderer,
  iconCellRenderer,
  labelsBadgesCellRenderer,
  linkCellRenderer,
} from "../utils/customRenderers";
import { generateHeaderIcons, mergeSortIndicatorSprites } from "../utils/headerIcons";
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
  useCellHoverTooltip,
  useDoubleTapLink,
  useMobileColumnResize,
  getActiveColumnIndex,
} from "../hooks";
import { useFooterColumnLayout } from "../hooks/useFooterColumnLayout";
import { GridContainer } from "../components/GridContainer";
import { MobileColumnResizeOverlay } from "../components/MobileColumnResizeOverlay";
import { AggregateFooter } from "../DataTableFooter";
import { MenuItem } from "@/types/widgets";
import { DENSITY_CONFIG } from "./constants";
import { useCellContent, useGridColumns, useHeaderMenu } from "./hooks";
import { getOrderedVisibleDataColumns } from "../utils/columnHelpers";
import type { SpriteMap } from "@glideapps/glide-data-grid";

interface TableEditorProps {
  widgetId: string;
  events: string[];
  hasOptions?: boolean;
  rowActions?: MenuItem[];
  perRowActions?: Record<string, MenuItem[]>;
  footer?: React.ReactNode;
  showAggregateFooter?: boolean;
  headerIcons?: SpriteMap;
}

export const DataTableEditor: React.FC<TableEditorProps> = ({
  widgetId,
  events,
  hasOptions = false,
  rowActions,
  perRowActions,
  footer,
  showAggregateFooter = false,
  headerIcons: providedHeaderIcons,
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
    activeSort,
    getRowData,
    arrowTableRef,
    loadMoreData,
    handleColumnResize,
    handleSort,
    handleColumnReorder,
  } = useTable();

  const densityConfig = DENSITY_CONFIG[density];

  const hasWrappingColumns = columns.some((c) => c.wrapText && !c.hidden);
  const effectiveRowHeight = hasWrappingColumns
    ? densityConfig.rowHeight * 3
    : densityConfig.rowHeight;

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
    headerIcons: customHeaderIcons,
  } = config;

  const selectionProps = getSelectionProps(selectionMode);

  const { containerRef, containerWidth, containerHeight, scrollContainerHeight } =
    useContainerSize();

  const gridHeight = containerHeight > 0 ? containerHeight : undefined;

  // Search functionality
  const { showSearch, setShowSearch } = useSearch(showSearchConfig ?? false);

  // Grid ref
  const gridRef = useRef<DataEditorRef | null>(null);
  const prevVisibleScrollOriginRef = useRef<{ x: number; y: number } | null>(null);
  const [visibleScrollX, setVisibleScrollX] = React.useState(0);

  const sortingEnabled = allowSorting ?? true;
  // Header clicks sort columns; column-select on mousedown would paint accentColor headers.
  const columnSelect = sortingEnabled ? "none" : selectionProps.columnSelect;

  // Cell content
  const { getCellContent } = useCellContent({
    columns,
    columnOrder,
    columnWidths,
    density,
    editable,
    visibleRows,
    getRowData,
  });

  // Grid selection
  const { gridSelection, handleGridSelectionChange, setGridSelection } = useGridSelection({
    visibleRows,
    getCellContent,
    allowSorting: sortingEnabled,
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
  const {
    hoverRow,
    actionButtonsTop,
    actionButtonsHeight,
    onItemHovered,
    handleRowActionClick,
    resolvedRowActions,
    clearRowHover,
    syncRowChromeFromCellArgs,
  } = useRowHover({
    widgetId,
    events,
    visibleRows,
    enableRowHover: enableRowHover ?? false,
    rowActions,
    perRowActions,
    containerRef,
    arrowTableRef,
  });

  const headerFont = useMemo(() => {
    const t = getDefaultTheme();
    return `${t.headerFontStyle} ${t.fontFamily}`;
  }, []);

  // Cell hover tooltips (truncated text + link hint)
  const {
    cellTooltip,
    isCellTooltipOpen,
    virtualRef,
    onItemHovered: onCellTooltipHovered,
    supportsHoverTooltip,
    clearCellHoverTooltip,
  } = useCellHoverTooltip({
    columns,
    columnOrder,
    columnWidths,
    density,
    getCellContent,
    visibleRows,
    headerFont,
  });

  // Table theme
  const { tableTheme, getRowThemeOverride, isDark } = useTableTheme({
    showVerticalBorders: showVerticalBorders ?? false,
    enableRowHover: enableRowHover ?? false,
    visibleRows,
    hoverRow,
    density,
    suppressHeaderHoverHighlight: sortingEnabled,
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
    rowHeight: effectiveRowHeight,
  });

  // Data loading
  const { handleVisibleRegionChanged } = useDataLoading({
    containerRef,
    visibleRows,
    isLoading,
    hasMore,
    loadMoreData,
    rowHeight: effectiveRowHeight,
  });

  // Generate header icons map for all column icons
  const headerIcons = useMemo(() => {
    const baseIcons = {
      ...generateHeaderIcons(columns, customHeaderIcons),
      ...providedHeaderIcons,
    };
    return mergeSortIndicatorSprites(baseIcons);
  }, [columns, customHeaderIcons, providedHeaderIcons]);

  // Header menu handling
  const { handleHeaderMenuClick } = useHeaderMenu({
    columns,
    columnOrder,
    allowSorting: sortingEnabled,
    handleSort,
    setGridSelection,
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
    activeSort,
  });

  const groupHeaderHeight = showGroups ? densityConfig.groupHeaderHeight : 0;

  const selectedColIndex = useMemo(() => getActiveColumnIndex(gridSelection), [gridSelection]);

  const mobileColumnResizeLayoutKey = `${visibleScrollX}:${JSON.stringify(columnWidths)}:${finalColumns.length}:${selectedColIndex ?? ""}`;

  const {
    useMobileHandles: useMobileColumnResizeHandles,
    handles: mobileResizeHandles,
    onHandlePointerDown: onMobileResizeHandlePointerDown,
  } = useMobileColumnResize({
    enabled: allowColumnResizing ?? true,
    gridRef,
    containerRef,
    columns: finalColumns,
    groupHeaderHeight,
    selectedColIndex,
    onColumnResize: handleColumnResize,
    layoutKey: mobileColumnResizeLayoutKey,
  });

  // Double-tap link cells on touch (desktop uses ⌘/Ctrl+click via useCellInteractions)
  useDoubleTapLink({
    containerRef,
    gridRef,
    getCellContent,
    columns: finalColumns,
    headerHeight: densityConfig.rowHeight,
    rowHeight: effectiveRowHeight,
    visibleRows,
  });

  const orderedDataColumns = useMemo(
    () => getOrderedVisibleDataColumns(columns, columnOrder),
    [columns, columnOrder],
  );

  const [cellActionIndicator, setCellActionIndicator] = React.useState<{
    x: number;
    y: number;
    width: number;
    height: number;
  } | null>(null);

  const handleVisibleRegionChangedForGrid = useCallback(
    (range: { x: number; y: number; width: number; height: number }) => {
      handleVisibleRegionChanged(range);
      setVisibleScrollX(range.x);
      const prev = prevVisibleScrollOriginRef.current;
      const verticalScrolled = prev !== null && prev.y !== range.y;
      prevVisibleScrollOriginRef.current = { x: range.x, y: range.y };
      if (!verticalScrolled) return;
      clearRowHover();
      clearCellHoverTooltip();
      setCellActionIndicator(null);
    },
    [handleVisibleRegionChanged, clearRowHover, clearCellHoverTooltip],
  );

  // Compose onItemHovered: keep existing hover behavior and track actionable-cell affordance.
  const handleItemHovered = useCallback(
    (args: GridMouseEventArgs) => {
      onCellTooltipHovered(args);
      onItemHovered(args);

      if (args.kind !== "cell") {
        setCellActionIndicator(null);
        return;
      }

      const [col, row] = args.location;
      if (row >= visibleRows) {
        setCellActionIndicator(null);
        return;
      }

      const hoveredColumn = orderedDataColumns[col];
      if (!hoveredColumn?.hasCellAction) {
        setCellActionIndicator(null);
        return;
      }

      setCellActionIndicator({
        x: args.bounds.x,
        y: args.bounds.y,
        width: args.bounds.width,
        height: args.bounds.height,
      });
    },
    [onCellTooltipHovered, onItemHovered, orderedDataColumns, visibleRows],
  );

  const handleCellClickedForGrid = useCallback(
    (cell: Item, args: GridMouseEventArgs) => {
      handleCellClicked(cell, args);
      if ((enableRowHover ?? false) && args.kind === "cell") {
        const [, row] = cell;
        if (row < visibleRows) {
          syncRowChromeFromCellArgs(args as GridMouseCellEventArgs);
        }
      }
    },
    [handleCellClicked, enableRowHover, visibleRows, syncRowChromeFromCellArgs],
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

  // Handle click outside to deselect cells
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      // Check if click is outside the container
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        // Always clear the selection - React handles no-op if already empty
        setGridSelection({
          columns: CompactSelection.empty(),
          rows: CompactSelection.empty(),
        });
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [setGridSelection]);

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

  const cellTooltipNode = (
    <TooltipProvider
      key={cellTooltip ? `${cellTooltip.x},${cellTooltip.y},${cellTooltip.content}` : "hidden"}
      delayDuration={300}
    >
      <Tooltip open={isCellTooltipOpen}>
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
        <TooltipContent
          side="top"
          className="pointer-events-none max-w-sm whitespace-pre-wrap break-words"
        >
          <div>{cellTooltip?.content}</div>
          {cellTooltip?.hint ? (
            <div className="mt-1 text-xs text-muted-foreground">{cellTooltip.hint}</div>
          ) : null}
        </TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );

  const cellActionIndicatorNode = cellActionIndicator ? (
    <div
      style={{
        position: "fixed",
        left: cellActionIndicator.x,
        top: cellActionIndicator.y,
        width: cellActionIndicator.width,
        height: cellActionIndicator.height,
        pointerEvents: "none",
        zIndex: 20,
        backgroundColor: isDark ? "rgba(59, 130, 246, 0.08)" : "rgba(59, 130, 246, 0.06)",
        border: `1px solid ${isDark ? "rgba(59, 130, 246, 0.3)" : "rgba(59, 130, 246, 0.2)"}`,
        borderRadius: "2px",
        boxSizing: "border-box",
      }}
      aria-hidden
    />
  ) : null;

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
            animatedStatusCellRenderer,
          ] as unknown as readonly CustomRenderer[]
        }
        headerIcons={headerIcons}
        onColumnResize={
          allowColumnResizing && !useMobileColumnResizeHandles ? handleColumnResize : undefined
        }
        columnResizeOverlay={
          useMobileColumnResizeHandles ? (
            <MobileColumnResizeOverlay
              handles={mobileResizeHandles}
              onHandlePointerDown={onMobileResizeHandlePointerDown}
            />
          ) : undefined
        }
        onVisibleRegionChanged={handleVisibleRegionChangedForGrid}
        onHeaderClicked={allowSorting ? handleHeaderMenuClick : undefined}
        theme={tableTheme}
        rowHeight={effectiveRowHeight}
        headerHeight={densityConfig.rowHeight}
        freezeColumns={freezeColumns ?? 0}
        getCellsForSelection={(allowCopySelection ?? true) ? true : undefined}
        rowSelect={selectionProps.rowSelect}
        columnSelect={columnSelect}
        rangeSelect={selectionProps.rangeSelect}
        gridSelection={gridSelection}
        onGridSelectionChange={handleGridSelectionChange}
        height={gridHeight}
        rowMarkers={showIndexColumn ? "number" : "none"}
        onColumnMoved={allowColumnReordering ? handleColumnReorder : undefined}
        groupHeaderHeight={showGroups ? densityConfig.groupHeaderHeight : undefined}
        onCellClicked={handleCellClickedForGrid}
        onCellActivated={handleCellActivated}
        onGroupHeaderClicked={shouldUseColumnGroups ? onGroupHeaderClicked : undefined}
        showSearch={showSearchConfig ? showSearch : false}
        onSearchClose={onSearchClose}
        onSearchResultsChanged={showSearchConfig ? onSearchResultsChanged : undefined}
        highlightRegions={showSearchConfig ? highlightRegions : undefined}
        onItemHovered={handleItemHovered}
        getRowThemeOverride={enableRowHover || emptyRowsCount > 0 ? getRowThemeOverride : undefined}
        rowActions={resolvedRowActions}
        actionButtonsTop={actionButtonsTop}
        actionButtonsHeight={actionButtonsHeight}
        hoverRow={hoverRow}
        onRowActionClick={handleRowActionClick}
        footer={footerNode}
        hasEmptyRows={emptyRowsCount > 0}
      />
      {supportsHoverTooltip ? cellTooltipNode : null}
      {cellActionIndicatorNode}
    </>
  );
};
