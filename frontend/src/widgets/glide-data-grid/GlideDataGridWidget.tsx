import { useEventHandler } from '@/components/EventHandlerContext';
import { getHeight, getWidth } from '@/lib/styles';
import React, { useMemo } from 'react';
import { GridContainer } from './components/GridContainer';
import { GridHeader } from './components/GridHeader';
import { GlideDataGridProvider } from './GlideDataGridProvider';
import { useGlideDataGrid } from './hooks/useGlideDataGrid';
import { classNames } from './styles';
import type {
  DataTableData,
  EditableGridCell,
  GlideDataGridProps,
  GridColumn,
  Item,
} from './types';

/**
 * GlideDataGridWidget - Composable data grid component for Ivy Framework
 *
 * This component uses a provider pattern with context for complete separation of concerns.
 * The provider manages all state and configuration, while child components are parameter-free
 * and access everything they need through context.
 *
 * Architecture:
 * - GlideDataGridProvider: Manages all state, data fetching, and configuration
 * - GridHeader: Title, description, and controls (parameter-free)
 * - GridContainer: Data display and error handling (parameter-free)
 * - DataGrid: Core grid component (parameter-free)
 *
 * All child components are composable and can be used independently within the provider.
 */

function GlideDataGridLayout() {
  const { props } = useGlideDataGrid();
  const { width, height } = props;

  const styles = useMemo(
    () => ({
      ...getWidth(width),
      ...getHeight(height),
    }),
    [width, height]
  );

  return (
    <div style={styles} className={classNames.container.main}>
      <GridHeader />
      <GridContainer />
    </div>
  );
}

const GlideDataGridWidget: React.FC<GlideDataGridProps> = props => {
  const eventHandler = useEventHandler();

  // Wrap event handlers with Ivy event system
  const configuration = {
    ...props,
    onCellEdited: (cell: Item, newValue: EditableGridCell) => {
      props.onCellEdited?.(cell, newValue);
      eventHandler('OnCellEdited', props.id, [cell, newValue]);
    },
    onColumnResize: (column: GridColumn, newSize: number) => {
      props.onColumnResize?.(column, newSize);
      eventHandler('OnColumnResize', props.id, [column.id, newSize]);
    },
    onDataReceived: (data: DataTableData) => {
      props.onDataReceived?.(data);
      eventHandler('OnDataReceived', props.id, [data]);
    },
    onError: (error: string) => {
      props.onError?.(error);
      eventHandler('OnError', props.id, [error]);
    },
    onStreamComplete: () => {
      props.onStreamComplete?.();
      eventHandler('OnStreamComplete', props.id, []);
    },
  };

  return (
    <GlideDataGridProvider {...configuration}>
      <GlideDataGridLayout />
    </GlideDataGridProvider>
  );
};

export default GlideDataGridWidget;
