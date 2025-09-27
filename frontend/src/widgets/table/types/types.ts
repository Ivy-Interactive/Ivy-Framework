export interface DataRow {
  values: (string | number | boolean | null)[];
}

export interface DataColumn {
  name: string;
  type: string;
  width: number;
}

export interface DataTableConnection {
  port: number;
  path: string;
  connectionId: string;
  sourceId: string;
}

export interface DataTableConfiguration {
  allowSearch?: boolean;
  filterType?: FilterTypes;
  freezeColumns?: number | null;
  allowSorting?: boolean;
  allowFiltering?: boolean;
  allowColumnReordering?: boolean;
  allowColumnResizing?: boolean;
  allowCopySelection?: boolean;
  selectionMode?: SelectionModes;
  showIndexColumn?: boolean;
  showGroups?: boolean;
}

export interface TableProps {
  connection: DataTableConnection;
  config: DataTableConfiguration;
  editable?: boolean;
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
