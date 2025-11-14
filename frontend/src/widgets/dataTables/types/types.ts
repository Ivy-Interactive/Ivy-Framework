import { MenuItem } from '@/types/widgets';

export interface DataRow {
  values: (string | number | boolean | Date | string[] | null)[];
}

export enum ColType {
  Number = 'Number',
  Text = 'Text',
  Boolean = 'Boolean',
  Date = 'Date',
  DateTime = 'DateTime',
  Icon = 'Icon',
  Labels = 'Labels',
  Link = 'Link',
}

export enum SortDirection {
  Ascending = 'Ascending',
  Descending = 'Descending',
  None = 'None',
}

export enum Align {
  Left = 'Left',
  Center = 'Center',
  Right = 'Right',
}

export interface DataColumn {
  name: string;
  header?: string;
  type: ColType;
  group?: string;
  width: number | string;
  hidden?: boolean;
  sortable?: boolean;
  sortDirection?: SortDirection;
  filterable?: boolean;
  align?: Align;
  order?: number;
  icon?: string | null;
  help?: string | null;
  iconSet?: 'lucide' | 'custom';
}

export interface DataTableConnection {
  port: number;
  path: string;
  connectionId: string;
  sourceId: string;
}

export interface DataTableConfig {
  filterType?: FilterTypes;
  freezeColumns?: number | null;
  allowSorting?: boolean;
  allowFiltering?: boolean;
  allowLlmFiltering?: boolean;
  allowColumnReordering?: boolean;
  allowColumnResizing?: boolean;
  allowCopySelection?: boolean;
  selectionMode?: SelectionModes;
  showIndexColumn?: boolean;
  showGroups?: boolean;
  showColumnTypeIcons?: boolean;
  showVerticalBorders?: boolean;
  batchSize?: number;
  loadAllRows?: boolean;
  enableCellClickEvents?: boolean;
  showSearch?: boolean;
  enableRowHover?: boolean;
}

export interface TableProps {
  id: string;
  columns: DataColumn[];
  connection: DataTableConnection;
  config?: DataTableConfig;
  editable?: boolean;
  width?: string;
  height?: string;
  rowActions?: MenuItem[];
  onCellUpdate?: (row: number, col: number, value: unknown) => void;
}

export enum FilterTypes {
  List = 'List',
  Query = 'Query',
}

export enum SelectionModes {
  Cells = 'Cells',
  Rows = 'Rows',
  Columns = 'Columns',
}

/**
 * Event args for row action click events
 */
export interface RowActionClickEventArgs {
  /**
   * The ID of the action that was clicked (from MenuItem.tag or MenuItem.label)
   */
  actionId: string;
  /**
   * The event name of the action (from MenuItem.tag or MenuItem.label)
   */
  eventName: string;
  /**
   * The index of the clicked row
   */
  rowIndex: number;
  /**
   * The data for the clicked row, keyed by column name
   */
  rowData: Record<string, unknown>;
}
