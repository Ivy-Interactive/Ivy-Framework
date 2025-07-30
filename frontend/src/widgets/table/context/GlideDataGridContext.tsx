import type {
  EditableGridCell,
  GridCell,
  GridColumn,
  GridSelection,
  Item,
} from '@glideapps/glide-data-grid';
import { createContext } from 'react';
import type { DataTableData, GlideDataGridProps } from '../types';

export interface GlideDataGridContextValue {
  // All original props available to children
  props: GlideDataGridProps;

  // Data state
  setData: (data: DataTableData | null) => void;
  data: DataTableData | null;
  loading: boolean;
  error: string | null;
  isStreaming: boolean;

  // Actions
  refresh: () => void;

  // Grid state
  gridSelection: GridSelection;
  setGridSelection: (selection: GridSelection) => void;

  // Column management
  columnWidths: Record<number, number>;
  onManualResize: (column: GridColumn, newSize: number) => void;

  // Cell formatting
  getCellContent: (cell: Item) => GridCell;

  // Event handlers
  handleColumnResize: (column: GridColumn, newSize: number) => void;
  handleCellEdited: (cell: Item, newValue: EditableGridCell) => void;

  // Connection validation
  isValidConnection: boolean;
}

export const GlideDataGridContext =
  createContext<GlideDataGridContextValue | null>(null);
