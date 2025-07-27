import { useEventHandler } from '@/components/EventHandlerContext';
import { getHeight, getWidth } from '@/lib/styles';
import {
  DataEditor,
  EditableGridCell,
  GridColumn,
  Item,
} from '@glideapps/glide-data-grid';
import '@glideapps/glide-data-grid/dist/index.css';
import React, { useMemo } from 'react';
import { ConnectionAlert } from './components/ConnectionAlert';
import { ErrorAlert } from './components/ErrorAlert';
import { GridHeader } from './components/GridHeader';
import { LoadingSpinner } from './components/LoadingSpinner';
import { useCellFormatting } from './hooks/useCellFormatting';
import { useColumnResizing } from './hooks/useColumnResizing';
import { useDataTableFetch } from './hooks/useDataTableFetch';
import { useGridSelection } from './hooks/useGridSelection';
import { classNames, defaultGlideTheme } from './styles';
import { DataTableData, GlideDataGridProps } from './types';
import { createGlideColumns } from './utils/glideGridColumns';
import {
  calculateGridDimensions,
  parsePixelValue,
} from './utils/gridDimensions';

/**
 * GlideDataGridWidget - High-performance data grid component for Ivy Framework
 *
 * AI AGENT CONTEXT:
 * This component provides a feature-rich data grid built on @glideapps/glide-data-grid.
 * It integrates with Ivy's gRPC streaming system for real-time data display.
 *
 * KEY ARCHITECTURE:
 * - Uses modular hooks for specific concerns (data fetching, column management, selection)
 * - Event handlers are inline for simplicity after refactoring from separate files
 * - Components are extracted for reusability (header, alerts, loading states)
 * - Follows Ivy Framework patterns for styling, events, and data connections
 *
 * IMPORTANT FILES TO UNDERSTAND:
 * - hooks/useDataTableFetch.ts - Core data fetching logic via gRPC
 * - utils/glideGridColumns.ts - Column configuration and type mapping
 * - styles/classNames.ts - Centralized styling definitions
 * - types/index.ts - TypeScript interfaces and props
 *
 * INTEGRATION POINTS:
 * - grpcTableService: Data streaming service
 * - EventHandlerContext: Ivy's event system
 * - getWidth/getHeight: Ivy's styling utilities
 *
 * TODO LIST FOR DEVELOPERS:
 * - [ ] Fix cell Data Grid overlay editor issue (currently not displaying correctly)
 * - [ ] Add cell editing functionality (editable prop exists but not fully implemented)
 * - [ ] Implement column sorting functionality (sortable prop exists but not implemented)
 * - [ ] Put content in content file for future translation
 * - [ ] Add progressive data loading for better initial load times
 * - [ ] Add unit tests for all hooks and utils
 */

const GlideDataGridWidget: React.FC<GlideDataGridProps> = ({
  id,
  connection,
  width,
  height,
  title,
  description,
  showRefreshButton = true,
  showStatus = true,
  resizableColumns = true,
  editable = true,
  rowMarkers = 'both',
  smoothScrollX = true,
  smoothScrollY = true,
  pageSize = 100,
  onCellEdited,
  onColumnResize,
  onDataReceived,
  onError,
  onStreamComplete,
}) => {
  const eventHandler = useEventHandler();
  const { data, loading, error, isStreaming, refresh } = useDataTableFetch(
    connection,
    {
      pageSize,
      onDataReceived: (tableData: DataTableData) => {
        onDataReceived?.(tableData);
        eventHandler('OnDataReceived', id, [tableData]);
      },
      onError: (errorMessage: string) => {
        onError?.(errorMessage);
        eventHandler('OnError', id, [errorMessage]);
      },
      onStreamComplete: () => {
        onStreamComplete?.();
        eventHandler('OnStreamComplete', id, []);
      },
    }
  );

  // Column resizing hook
  // This hook manages column widths and provides a function to handle manual resizing
  const { columnWidths, onManualResize } = useColumnResizing();

  // Cell formatting hook
  // This hook provides a function to format cell content based on its type
  const { getCellContent } = useCellFormatting(data);

  // Grid selection state
  // This manages the selection state of the grid, allowing for multi-select and range selection
  const { gridSelection, setGridSelection } = useGridSelection();

  // Styles for the grid container
  // This uses the provided width and height props to set the grid's dimensions
  const styles = useMemo(
    () => ({
      ...getWidth(width),
      ...getHeight(height),
    }),
    [width, height]
  );

  // Create Glide columns based on the data and column widths
  // This memoizes the column creation to avoid unnecessary recalculations
  const columns = useMemo(() => {
    if (!data?.columns) return [];
    return createGlideColumns(data.columns, columnWidths, resizableColumns);
  }, [data?.columns, columnWidths, resizableColumns]);

  // Calculate grid dimensions based on the container size
  // This uses the styles to determine the width and height in pixels
  const dimensions = useMemo(() => {
    const widthPx = parsePixelValue(styles.width, 800);
    const heightPx = parsePixelValue(styles.height, 600);
    return calculateGridDimensions(widthPx, heightPx);
  }, [styles]);

  // Handle column resizing
  // This function is called when a column is manually resized
  const handleColumnResize = (column: GridColumn, newSize: number) => {
    onManualResize(column, newSize);
    onColumnResize?.(column, newSize);
    eventHandler('OnColumnResize', id, [column.id, newSize]);
  };
  // Handle cell editing
  // This function is called when a cell's value is edited
  const handleCellEdited = (cell: Item, newValue: EditableGridCell) => {
    onCellEdited?.(cell, newValue);
    eventHandler('OnCellEdited', id, [cell, newValue]);
  };

  // Connection validation
  // If the connection is not properly configured, show an alert
  if (!connection.port || !connection.path) {
    return (
      <div style={styles}>
        <ConnectionAlert />
      </div>
    );
  }

  return (
    <div style={styles} className={classNames.container.main}>
      <GridHeader
        title={title}
        description={description}
        showStatus={showStatus}
        showRefreshButton={showRefreshButton}
        loading={loading}
        isStreaming={isStreaming}
        data={data}
        onRefresh={refresh}
      />

      {error && <ErrorAlert error={error} />}

      <div className={classNames.container.content}>
        {loading ? (
          <LoadingSpinner />
        ) : data ? (
          <DataEditor
            getCellContent={getCellContent}
            columns={columns}
            rows={data.rows.length}
            onColumnResize={handleColumnResize}
            onCellEdited={editable ? handleCellEdited : undefined}
            gridSelection={gridSelection}
            onGridSelectionChange={setGridSelection}
            theme={defaultGlideTheme}
            smoothScrollX={smoothScrollX}
            smoothScrollY={smoothScrollY}
            rowMarkers={rowMarkers}
            width={dimensions.width}
            height={dimensions.height}
            isDraggable={false}
            rangeSelect="rect"
            columnSelect="none"
            rowSelect="none"
          />
        ) : (
          <div className={classNames.container.empty}>No data available</div>
        )}
      </div>
    </div>
  );
};

export default GlideDataGridWidget;
