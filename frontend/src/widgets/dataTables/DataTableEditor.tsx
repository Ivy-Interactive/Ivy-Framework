import DataEditor, {
  CompactSelection,
  DataEditorRef,
  GridCell,
  GridCellKind,
  GridSelection,
  GridMouseEventArgs,
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
import { iconCellRenderer, linkCellRenderer } from './utils/customRenderers';
import { generateHeaderIcons, addStandardIcons } from './utils/headerIcons';
import { ThemeColors } from '@/lib/color-utils';
import { useEventHandler } from '@/components/event-handler';
import { validateLinkUrl, validateRedirectUrl } from '@/lib/utils';
import { useColumnGroups } from './hooks/useColumnGroups';
import { RowActionButtons } from './DataTableRowAction';
import { MenuItem } from '@/types/widgets';

interface TableEditorProps {
  widgetId: string;
  hasOptions?: boolean;
  rowActions?: MenuItem[];
  footer?: React.ReactNode;
}

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

  // Use the enhanced theme hook with custom DataGrid theme generator
  const {
    customTheme: tableTheme,
    colors: themeColors,
    isDark,
  } = useThemeWithMonitoring<Partial<Theme>>({
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
      // textHeaderSelected needs to contrast with accentColor background (used when column is sorted)
      textHeaderSelected: colors.foreground || (isDark ? '#f8f8f8' : '#111827'),
      textDark: colors.foreground || (isDark ? '#f8f8f8' : '#111827'),
      textMedium: colors.mutedForeground || (isDark ? '#a1a1aa' : '#6b7280'),
      textLight: colors.mutedForeground || (isDark ? '#71717a' : '#9ca3af'),
      // bgIconHeader is the background color for icon areas, should be subtle
      bgIconHeader: colors.muted || (isDark ? '#26262b' : '#f3f4f6'),
      // accentColor is used as the background for selected cells or highlights
      accentColor: colors.secondary || (isDark ? '#26262b' : '#e5e7eb'),
      // accentFg is the foreground/text color used on top of accentColor backgrounds
      accentFg: colors.muted || (isDark ? '#f8f8f8' : '#18181b'),
      // column focus bg color
      accentLight: colors.muted || (isDark ? '#27272a' : '#e4e4e7'),
      horizontalBorderColor: colors.border || (isDark ? '#404045' : '#d1d5db'),
      linkColor:
        colors.primary || colors.accent || (isDark ? '#3b82f6' : '#2563eb'),
      // Control vertical borders by setting borderColor to transparent when disabled
      borderColor: showVerticalBorders
        ? colors.border || (isDark ? '#404045' : '#d1d5db')
        : 'transparent',
      cellHorizontalPadding: 8,
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
  const [containerHeight, setContainerHeight] = useState<number>(0);
  const [gridSelection, setGridSelection] = useState<GridSelection>({
    columns: CompactSelection.empty(),
    rows: CompactSelection.empty(),
  });
  const [showSearch, setShowSearch] = useState(false);
  const [hoverRow, setHoverRow] = useState<number | undefined>(undefined);
  const [actionButtonsTop, setActionButtonsTop] = useState<number>(0);

  const scrollThreshold = 10;
  const rowHeight = 38;
  const GROUP_HEADER_HEIGHT = 36;

  // Generate header icons map for all column icons
  const headerIcons = useMemo(() => {
    const baseIcons = generateHeaderIcons(columns);
    return addStandardIcons(baseIcons);
  }, [columns]);

  // Track container width and height
  useEffect(() => {
    if (!containerRef.current) return;

    const resizeObserver = new ResizeObserver(entries => {
      for (const entry of entries) {
        setContainerWidth(entry.contentRect.width);
        setContainerHeight(entry.contentRect.height);
      }
    });

    resizeObserver.observe(containerRef.current);

    return () => {
      resizeObserver.disconnect();
    };
  }, []);

  // Check if we need to load more data when container height is large or when visible rows change
  useEffect(() => {
    if (!containerRef.current || visibleRows === 0 || isLoading) {
      return;
    }

    // Calculate if the container height can show more rows than we have loaded
    const containerHeight = containerRef.current.clientHeight;
    const availableHeight = containerHeight + rowHeight;
    const visibleRowCapacity = Math.ceil(availableHeight / rowHeight);

    // If container can show more rows than we have, and we have more data available, load it
    // This will keep loading until we have enough rows to fill the container
    if (visibleRowCapacity > visibleRows && hasMore) {
      loadMoreData();
    }
  }, [visibleRows, hasMore, isLoading, loadMoreData, containerRef]);

  // Handle keyboard shortcut for search (Ctrl/Cmd + F)
  useEffect(() => {
    if (!showSearchConfig) return;

    const handleKeyDown = (event: KeyboardEvent) => {
      if ((event.ctrlKey || event.metaKey) && event.code === 'KeyF') {
        setShowSearch(current => !current);
        event.stopPropagation();
        event.preventDefault();
      }
    };

    window.addEventListener('keydown', handleKeyDown, true);

    return () => {
      window.removeEventListener('keydown', handleKeyDown, true);
    };
  }, [showSearchConfig]);

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
      // Consolidate check for newSelection.current
      if (newSelection.current !== undefined) {
        const [col, row] = newSelection.current.cell;

        // Prevent selection of empty filler rows
        if (row >= visibleRows) {
          // Don't allow selection of empty filler rows
          return;
        }

        // Check if the new selection includes link cells and prevent fuzzy effect
        // by clearing the selection if it's a single link cell click
        const cellContent = getCellContent([col, row]);

        // If it's a link cell, don't allow it to be selected (prevents fuzzy effect)
        if (
          cellContent.kind === GridCellKind.Custom &&
          (cellContent.data as { kind?: string })?.kind === 'link-cell'
        ) {
          // Clear the selection for link cells
          setGridSelection({
            columns: CompactSelection.empty(),
            rows: CompactSelection.empty(),
          });
          return;
        }
      }

      setGridSelection(newSelection);
    },
    [getCellContent, visibleRows]
  );

  // Get event handler for sending events to backend
  const eventHandler = useEventHandler();

  // Handle cell single-clicks (for backend events and link navigation)
  const handleCellClicked = useCallback(
    (cell: Item, args: GridMouseEventArgs) => {
      const [, row] = cell;
      // Prevent interactions with empty filler rows
      if (row >= visibleRows) {
        return;
      }

      const cellContent = getCellContent(cell);

      // Handle Ctrl+Click or Cmd+Click on custom link cells
      if (
        (args.ctrlKey || args.metaKey) &&
        cellContent.kind === GridCellKind.Custom &&
        (cellContent.data as { kind?: string })?.kind === 'link-cell'
      ) {
        const url = (cellContent.data as { url?: string })?.url;

        // Validate URL to prevent open redirect vulnerabilities
        const validatedUrl = validateLinkUrl(url);
        if (validatedUrl === '#') {
          // Invalid URL, don't proceed
          return;
        }

        // External URLs (http/https) open in new tab
        if (
          validatedUrl.startsWith('http://') ||
          validatedUrl.startsWith('https://')
        ) {
          const newWindow = window.open(
            validatedUrl,
            '_blank',
            'noopener,noreferrer'
          );
          newWindow?.focus();
        } else {
          // Internal relative URLs navigate in same tab
          // Validate it's safe for redirect (relative path or same-origin)
          const redirectUrl = validateRedirectUrl(validatedUrl, false);
          if (redirectUrl) {
            window.location.href = redirectUrl;
          }
        }
        return; // Don't proceed with other click handling
      }

      if (enableCellClickEvents) {
        // Get actual cell value
        const visibleColumns = columns.filter(c => !c.hidden);
        const column = visibleColumns[cell[0]];

        // Extract the actual value from the cell based on its kind
        let cellValue: unknown = null;
        if (
          cellContent.kind === 'text' ||
          cellContent.kind === 'number' ||
          cellContent.kind === 'boolean'
        ) {
          cellValue = cellContent.data;
        } else if ('data' in cellContent) {
          // Cast to unknown first, then access the data property
          cellValue = (cellContent as unknown as { data: unknown }).data;
        }

        // Send event to backend as a single object matching CellClickEventArgs structure
        eventHandler('OnCellClick', widgetId, [
          {
            rowIndex: cell[1],
            columnIndex: cell[0],
            columnName: column?.name || '',
            cellValue: cellValue,
          },
        ]);
      }
      // Do NOT prevent default - let selection happen normally!
    },
    [
      enableCellClickEvents,
      eventHandler,
      widgetId,
      columns,
      getCellContent,
      visibleRows,
    ]
  );

  // Handle cell double-clicks/activation (for editing)
  const handleCellActivated = useCallback(
    (cell: Item) => {
      const [, row] = cell;
      // Prevent interactions with empty filler rows
      if (row >= visibleRows) {
        return;
      }

      if (enableCellClickEvents) {
        const cellContent = getCellContent(cell);
        const visibleColumns = columns.filter(c => !c.hidden);
        const column = visibleColumns[cell[0]];

        // Extract the actual value from the cell based on its kind
        let cellValue: unknown = null;
        if (
          cellContent.kind === 'text' ||
          cellContent.kind === 'number' ||
          cellContent.kind === 'boolean'
        ) {
          cellValue = cellContent.data;
        } else if ('data' in cellContent) {
          // Cast to unknown first, then access the data property
          cellValue = (cellContent as unknown as { data: unknown }).data;
        }

        // Send activation event to backend as a single object matching CellClickEventArgs structure
        eventHandler('OnCellActivated', widgetId, [
          {
            rowIndex: cell[1],
            columnIndex: cell[0],
            columnName: column?.name || '',
            cellValue: cellValue,
          },
        ]);
      }
    },
    [
      enableCellClickEvents,
      eventHandler,
      widgetId,
      columns,
      getCellContent,
      visibleRows,
    ]
  );

  // Handle row hover
  const onItemHovered = useCallback(
    (args: GridMouseEventArgs) => {
      if (!enableRowHover) return;
      const [col, row] = args.location;
      // Don't allow hover on empty filler rows
      if (args.kind === 'cell' && row >= visibleRows) {
        setHoverRow(undefined);
        return;
      }
      const newHoverRow = args.kind !== 'cell' ? undefined : row;
      setHoverRow(newHoverRow);

      // Calculate action buttons position if row actions are configured
      if (
        rowActions &&
        rowActions.length > 0 &&
        newHoverRow !== undefined &&
        gridRef.current &&
        containerRef.current
      ) {
        // Use getBounds to get the actual cell position from the grid
        const bounds = gridRef.current.getBounds(col, newHoverRow);
        const containerRect = containerRef.current.getBoundingClientRect();

        if (bounds) {
          // Position button in the center of the row using the actual bounds
          // Subtract container offset to get position relative to container
          const buttonHeight = 24;
          const buttonTop =
            bounds.y -
            containerRect.top +
            bounds.height / 2 -
            buttonHeight / 2 -
            5;
          setActionButtonsTop(buttonTop);
        }
      }
    },
    [enableRowHover, rowActions, visibleRows]
  );

  // Get row theme override for hover effect and empty filler rows
  const getRowThemeOverride = useCallback(
    (row: number) => {
      // If this is an empty filler row, remove all borders and styling
      if (row >= visibleRows) {
        return {
          bgCell: themeColors.background || (isDark ? '#000000' : '#ffffff'),
          bgCellMedium:
            themeColors.background || (isDark ? '#000000' : '#ffffff'),
          borderColor: 'transparent',
          horizontalBorderColor: 'transparent',
        };
      }
      // Handle hover effect for data rows
      if (!enableRowHover || row !== hoverRow) return undefined;
      // Use theme-aware colors for hover effect
      return {
        bgCell: themeColors.muted || (isDark ? '#26262b' : '#f7f7f7'),
        bgCellMedium:
          themeColors.background || (isDark ? '#1f1f23' : '#f0f0f0'),
      };
    },
    [hoverRow, enableRowHover, themeColors, isDark, visibleRows]
  );

  // Get row data as a record of column name -> value
  const getRowData = useCallback(
    (rowIndex: number): Record<string, unknown> => {
      const rowData: Record<string, unknown> = {};
      const visibleColumns = columns.filter(c => !c.hidden);

      visibleColumns.forEach((column, colIndex) => {
        const cell = getCellContent([colIndex, rowIndex]);
        let cellValue: unknown = null;

        if (
          cell.kind === 'text' ||
          cell.kind === 'number' ||
          cell.kind === 'boolean'
        ) {
          cellValue = cell.data;
        } else if ('data' in cell) {
          cellValue = (cell as unknown as { data: unknown }).data;
        }

        rowData[column.name] = cellValue;
      });

      return rowData;
    },
    [columns, getCellContent]
  );

  // Handle row action button click
  const handleRowActionClick = useCallback(
    (action: MenuItem) => {
      if (hoverRow === undefined) return;

      const rowData = getRowData(hoverRow);

      // Get action identifier from tag or label
      const actionId = action.tag?.toString() || action.label || '';

      // Send event to backend's OnRowAction event
      eventHandler('OnRowAction', widgetId, [
        {
          actionId: actionId,
          eventName: actionId,
          rowIndex: hoverRow,
          rowData: rowData,
        },
      ]);
    },
    [hoverRow, getRowData, eventHandler, widgetId]
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

  // Calculate whitespace height needed to fill container
  const whitespaceHeight = useMemo(() => {
    // Only add whitespace when there's no more data to load
    if (hasMore || containerHeight === 0 || visibleRows === 0) {
      return 0;
    }

    // Calculate header height (regular header + group header if enabled)
    const headerHeight = rowHeight;
    const groupHeaderHeight = showGroups ? GROUP_HEADER_HEIGHT : 0;
    const totalHeaderHeight = headerHeight + groupHeaderHeight;

    // Calculate total height of visible rows
    const rowsHeight = visibleRows * rowHeight;

    // Calculate whitespace needed
    const calculatedWhitespace =
      containerHeight - totalHeaderHeight - rowsHeight;

    // Only return positive values
    return Math.max(0, calculatedWhitespace);
  }, [containerHeight, visibleRows, hasMore, rowHeight, showGroups]);

  // Calculate number of empty rows needed to fill whitespace
  const emptyRowsCount = useMemo(() => {
    if (whitespaceHeight <= 0) return 0;
    return Math.ceil(whitespaceHeight / rowHeight);
  }, [whitespaceHeight, rowHeight]);

  // Total rows including empty filler rows
  const totalRows = visibleRows + emptyRowsCount;

  if (finalColumns.length === 0) {
    return null;
  }

  const containerStyle = hasOptions
    ? tableStyles.tableEditor.gridContainerWithOptions
    : tableStyles.tableEditor.gridContainer;

  return (
    <>
      <div
        ref={containerRef}
        style={{ ...containerStyle, position: 'relative' }}
      >
        <DataEditor
          ref={gridRef}
          columns={finalColumns}
          rows={totalRows}
          getCellContent={getCellContent}
          customRenderers={[iconCellRenderer, linkCellRenderer]}
          headerIcons={headerIcons}
          onColumnResize={allowColumnResizing ? handleColumnResize : undefined}
          onVisibleRegionChanged={handleVisibleRegionChanged}
          onHeaderClicked={allowSorting ? handleHeaderMenuClick : undefined}
          smoothScrollX={true}
          smoothScrollY={true}
          theme={tableTheme}
          rowHeight={rowHeight}
          headerHeight={rowHeight}
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
          onColumnMoved={
            allowColumnReordering ? handleColumnReorder : undefined
          }
          groupHeaderHeight={showGroups ? GROUP_HEADER_HEIGHT : undefined}
          cellActivationBehavior="double-click"
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
            enableRowHover || emptyRowsCount > 0
              ? getRowThemeOverride
              : undefined
          }
        />

        {/* Row action buttons overlay */}
        {rowActions && rowActions.length > 0 && (
          <RowActionButtons
            actions={rowActions}
            top={actionButtonsTop}
            visible={hoverRow !== undefined}
            onActionClick={handleRowActionClick}
          />
        )}

        {/* Footer overlay */}
        {footer}
      </div>
    </>
  );
};
