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

export interface TableProps {
  connection: DataTableConnection;
  editable?: boolean;
  onCellUpdate?: (row: number, col: number, value: unknown) => void;
}
