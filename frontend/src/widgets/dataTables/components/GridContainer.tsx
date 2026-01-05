import React from 'react';
import DataEditor, {
  CustomRenderer,
  DataEditorRef,
  GridCell,
  GridColumn,
  GridMouseEventArgs,
  GridSelection,
  GroupHeaderClickedEventArgs,
  Item,
  SpriteMap,
  Theme,
} from '@glideapps/glide-data-grid';
import { tableStyles } from '../styles/style';
import { RowActionButtons } from '../dataTableRowAction/index';
import { MenuItem } from '@/types/widgets';

interface GridContainerProps {
  gridRef: React.RefObject<DataEditorRef | null>;
  containerRef: React.RefObject<HTMLDivElement | null>;
  hasOptions: boolean;
  columns: GridColumn[];
  rows: number;
  getCellContent: (cell: Item) => GridCell;
  customRenderers: readonly CustomRenderer[];
  headerIcons: SpriteMap;
  onColumnResize?: (column: GridColumn, newSize: number) => void;
  onVisibleRegionChanged: (range: {
    x: number;
    y: number;
    width: number;
    height: number;
  }) => void;
  onHeaderClicked?: (col: number) => void;
  theme: Partial<Theme> | undefined;
  rowHeight: number;
  headerHeight: number;
  freezeColumns: number;
  getCellsForSelection: true | undefined;
  rowSelect: 'none' | 'multi' | 'single';
  columnSelect: 'none' | 'multi' | 'single';
  rangeSelect: 'none' | 'cell' | 'rect' | 'multi-cell' | 'multi-rect';
  gridSelection: GridSelection;
  onGridSelectionChange: (newSelection: GridSelection) => void;
  width: number;
  rowMarkers: 'number' | 'checkbox' | 'both' | 'none';
  onColumnMoved?: (startIndex: number, endIndex: number) => void;
  groupHeaderHeight?: number;
  onCellClicked: (cell: Item, args: GridMouseEventArgs) => void;
  onCellActivated: (cell: Item) => void;
  onGroupHeaderClicked?: (
    colIndex: number,
    event: GroupHeaderClickedEventArgs
  ) => void;
  showSearch: boolean;
  onSearchClose: () => void;
  onItemHovered?: (args: GridMouseEventArgs) => void;
  getRowThemeOverride?:
    | ((row: number) => Partial<Theme> | undefined)
    | undefined;
  rowActions?: MenuItem[];
  actionButtonsTop: number;
  hoverRow: number | undefined;
  onRowActionClick: (action: MenuItem) => void;
  footer?: React.ReactNode;
}

/**
 * Container component that wraps the DataEditor grid and manages overlays
 */
export const GridContainer: React.FC<GridContainerProps> = ({
  gridRef,
  containerRef,
  hasOptions,
  columns,
  rows,
  getCellContent,
  customRenderers,
  headerIcons,
  onColumnResize,
  onVisibleRegionChanged,
  onHeaderClicked,
  theme,
  rowHeight,
  headerHeight,
  freezeColumns,
  getCellsForSelection,
  rowSelect,
  columnSelect,
  rangeSelect,
  gridSelection,
  onGridSelectionChange,
  width,
  rowMarkers,
  onColumnMoved,
  groupHeaderHeight,
  onCellClicked,
  onCellActivated,
  onGroupHeaderClicked,
  showSearch,
  onSearchClose,
  onItemHovered,
  getRowThemeOverride,
  rowActions,
  actionButtonsTop,
  hoverRow,
  onRowActionClick,
  footer,
}) => {
  const containerStyle = hasOptions
    ? tableStyles.tableEditor.gridContainerWithOptions
    : tableStyles.tableEditor.gridContainer;

  return (
    <div ref={containerRef} style={{ ...containerStyle, position: 'relative' }}>
      <DataEditor
        ref={gridRef}
        columns={columns}
        rows={rows}
        getCellContent={getCellContent}
        customRenderers={customRenderers}
        headerIcons={headerIcons}
        onColumnResize={onColumnResize}
        onVisibleRegionChanged={onVisibleRegionChanged}
        onHeaderClicked={onHeaderClicked}
        smoothScrollX={true}
        smoothScrollY={true}
        theme={theme}
        rowHeight={rowHeight}
        headerHeight={headerHeight}
        freezeColumns={freezeColumns}
        getCellsForSelection={getCellsForSelection}
        keybindings={{ search: false }}
        rowSelect={rowSelect}
        columnSelect={columnSelect}
        rangeSelect={rangeSelect}
        gridSelection={gridSelection}
        onGridSelectionChange={onGridSelectionChange}
        width={width}
        rowMarkers={rowMarkers}
        onColumnMoved={onColumnMoved}
        groupHeaderHeight={groupHeaderHeight}
        cellActivationBehavior="double-click"
        onCellClicked={onCellClicked}
        onCellActivated={onCellActivated}
        onGroupHeaderClicked={onGroupHeaderClicked}
        showSearch={showSearch}
        onSearchClose={onSearchClose}
        onItemHovered={onItemHovered}
        getRowThemeOverride={getRowThemeOverride}
      />

      {/* Row action buttons overlay */}
      {rowActions && rowActions.length > 0 && (
        <RowActionButtons
          actions={rowActions}
          top={actionButtonsTop}
          visible={hoverRow !== undefined}
          onActionClick={onRowActionClick}
        />
      )}

      {/* Footer overlay */}
      {footer}
    </div>
  );
};
