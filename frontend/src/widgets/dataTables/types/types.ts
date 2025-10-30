export interface DataRow {
  values: (string | number | boolean | null)[];
}

export enum ColType {
  Number = 'Number',
  Text = 'Text',
  Boolean = 'Boolean',
  Date = 'Date',
  DateTime = 'DateTime',
  Icon = 'Icon',
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

export interface DataTableConfiguration {
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
  batchSize?: number;
  loadAllRows?: boolean;
}

export interface TableProps {
  columns: DataColumn[];
  connection: DataTableConnection;
  configuration?: DataTableConfiguration;
  editable?: boolean;
  width?: string;
  height?: string;
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
