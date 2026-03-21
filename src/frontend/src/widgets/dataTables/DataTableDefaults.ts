/**
 * Default values for DataTable configuration objects.
 * These match the C# backend defaults in Ivy.Widgets.DataTables.
 * When values equal the C# default, they are not serialized, so we must apply them here.
 */

import type { DataTableConfig, DataColumn } from './types/types';
import { SortDirection, SelectionModes } from './types/types';
import { applyDefaults } from '@/lib/utils';

// DataTableColumn defaults (DataTableColumn.cs)
export const DATA_COLUMN_DEFAULTS: Partial<DataColumn> = {
  hidden: false,
  sortable: true,
  sortDirection: SortDirection.None,
  filterable: true,
  align: 'Left',
  order: 0,
  icon: null,
  help: null,
};

// DataTableConfig defaults (DataTableConfig.cs)
export const DATA_TABLE_CONFIG_DEFAULTS: Partial<DataTableConfig> = {
  freezeColumns: null,
  allowSorting: true,
  allowFiltering: true,
  allowLlmFiltering: false,
  allowColumnReordering: true,
  allowColumnResizing: true,
  allowCopySelection: true,
  selectionMode: SelectionModes.Cells,
  showIndexColumn: false,
  showGroups: false,
  showColumnTypeIcons: false,
  showVerticalBorders: true,
  batchSize: undefined,
  loadAllRows: false,
  enableCellClickEvents: false,
  showSearch: false,
  idColumnName: null,
};

/**
 * Apply column defaults to a single column.
 */
export function applyColumnDefaults(column: DataColumn): DataColumn {
  return {
    ...DATA_COLUMN_DEFAULTS,
    ...column,
    // Ensure explicit undefined values don't override defaults
    hidden: column.hidden ?? DATA_COLUMN_DEFAULTS.hidden,
    sortable: column.sortable ?? DATA_COLUMN_DEFAULTS.sortable,
    sortDirection: column.sortDirection ?? DATA_COLUMN_DEFAULTS.sortDirection,
    filterable: column.filterable ?? DATA_COLUMN_DEFAULTS.filterable,
    align: column.align ?? DATA_COLUMN_DEFAULTS.align,
    order: column.order ?? DATA_COLUMN_DEFAULTS.order,
  } as DataColumn;
}

/**
 * Apply defaults to all columns in an array.
 */
export function applyColumnsDefaults(columns: DataColumn[]): DataColumn[] {
  return columns.map(applyColumnDefaults);
}

/**
 * Apply config defaults to a DataTableConfig.
 */
export function applyConfigDefaults(
  config: Partial<DataTableConfig> | undefined
): DataTableConfig {
  return applyDefaults(config, DATA_TABLE_CONFIG_DEFAULTS) as DataTableConfig;
}
