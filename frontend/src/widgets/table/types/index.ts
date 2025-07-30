import type {
  EditableGridCell,
  GridCell,
  GridColumn,
  GridSelection,
  Item,
} from '@glideapps/glide-data-grid';

// Re-export Glide types for convenience
export type { EditableGridCell, GridCell, GridColumn, GridSelection, Item };

// Data interfaces
export interface DataTableColumn {
  name: string;
  type: string;
}

export interface DataTableRow {
  values: (string | number | boolean | null)[];
}

export interface DataTableData {
  columns: DataTableColumn[];
  rows: DataTableRow[];
  totalRows: number;
  hasMore: boolean;
}

export interface DataTableConnection {
  port: number;
  path: string;
  connectionId: string;
  sourceId: string;
}

// Component props
export interface GlideDataGridProps {
  id: string;
  connection: DataTableConnection;
  width?: string;
  height?: string;
  title?: string;
  description?: string;
  showRefreshButton?: boolean;
  showStatus?: boolean;

  // Glide-specific features
  resizableColumns?: boolean;
  sortable?: boolean;
  editable?: boolean;
  virtualScrolling?: boolean;
  rowMarkers?: 'both' | 'number' | 'checkbox' | 'none';
  smoothScrollX?: boolean;
  smoothScrollY?: boolean;

  // Performance
  pageSize?: number;

  // Events
  onCellEdited?: (cell: Item, newValue: EditableGridCell) => void;
  onColumnResize?: (column: GridColumn, newSize: number) => void;
  onDataReceived?: (data: DataTableData) => void;
  onError?: (error: string) => void;
  onStreamComplete?: () => void;
}

// Hook return types (individual hooks now)

export interface UseColumnResizingReturn {
  columnWidths: Record<number, number>;
  onManualResize: (column: GridColumn, newSize: number) => void;
}

export interface UseCellFormattingReturn {
  getCellContent: (cell: Item) => GridCell;
}

// Theme types
export interface GlideThemeColors {
  accentColor: string;
  accentLight: string;
  textDark: string;
  textMedium: string;
  textLight: string;
  textBubble: string;
  bgIconHeader: string;
  fgIconHeader: string;
  textHeader: string;
  textHeaderSelected: string;
  bgCell: string;
  bgCellMedium: string;
  bgHeader: string;
  bgHeaderHovered: string;
  bgHeaderHasFocus: string;
  borderColor: string;
  drilldownBorder: string;
  linkColor: string;
  headerFontStyle: string;
  baseFontStyle: string;
  fontFamily: string;
}
