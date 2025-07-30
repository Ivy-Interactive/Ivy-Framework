import type {
  EditableGridCell,
  GridColumn,
  Item,
} from '@glideapps/glide-data-grid';
import React, { useCallback } from 'react';
import { GlideDataGridContext } from './context/GlideDataGridContext';
import { useCellFormatting } from './hooks/useCellFormatting';
import { useColumnResizing } from './hooks/useColumnResizing';
import { useDataTableFetch } from './hooks/useDataTableFetch';
import { useGridSelection } from './hooks/useGridSelection';
import type { GlideDataGridProps } from './types';

export interface GlideDataGridProviderProps extends GlideDataGridProps {
  children: React.ReactNode;
}

export const GlideDataGridProvider: React.FC<
  GlideDataGridProviderProps
> = props => {
  const {
    children,
    connection,
    pageSize = 100,
    onCellEdited,
    onColumnResize,
    onDataReceived,
    onError,
    onStreamComplete,
  } = props;

  // Connection validation
  const isValidConnection = !!(connection.port && connection.path);

  // Data fetching
  const { data, loading, error, isStreaming, refresh } = useDataTableFetch(
    connection,
    {
      pageSize,
      onDataReceived,
      onError,
      onStreamComplete,
    }
  );

  // Column management
  const { columnWidths, onManualResize } = useColumnResizing();

  // Cell formatting
  const { getCellContent } = useCellFormatting(data);

  // Grid selection
  const { gridSelection, setGridSelection } = useGridSelection();

  // Event handlers
  const handleColumnResize = useCallback(
    (column: GridColumn, newSize: number) => {
      onManualResize(column, newSize);
      onColumnResize?.(column, newSize);
    },
    [onManualResize, onColumnResize]
  );

  const handleCellEdited = useCallback(
    (cell: Item, newValue: EditableGridCell) => {
      onCellEdited?.(cell, newValue);
    },
    [onCellEdited]
  );

  return (
    <GlideDataGridContext.Provider
      value={{
        props,
        data,
        loading,
        error,
        isStreaming,
        refresh,
        gridSelection,
        setGridSelection,
        columnWidths,
        onManualResize,
        getCellContent,
        handleColumnResize,
        handleCellEdited,
        isValidConnection,
      }}
    >
      {children}
    </GlideDataGridContext.Provider>
  );
};
