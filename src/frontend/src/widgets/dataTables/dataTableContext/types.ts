import React from 'react';
import * as arrow from 'apache-arrow';
import { Filter, SortOrder } from '@/services/grpcTableService';
import { GridColumn } from '@glideapps/glide-data-grid';
import {
  DataColumn,
  DataRow,
  DataTableConfig,
  DataTableConnection,
} from '../types/types';

export interface TableContextType {
  // State
  columns: DataColumn[];
  columnWidths: Record<string, number>;
  visibleRows: number;
  isLoading: boolean;
  hasMore: boolean;
  error: string | null;
  editable: boolean;
  connection: DataTableConnection;
  config: DataTableConfig;
  activeFilter: Filter | null;
  activeSort: SortOrder[] | null;
  columnOrder: number[];
  // Arrow table accessor - data is accessed directly from Arrow table via gRPC
  getRowData: (rowIndex: number) => DataRow | null;
  arrowTableRef: React.RefObject<arrow.Table | null>;

  // Methods
  loadMoreData: () => Promise<void>;
  handleColumnResize: (column: GridColumn, newSize: number) => void;
  handleSort: (columnName: string) => void;
  setActiveFilter: (filter: Filter | null) => void;
  setError: (error: string | null) => void;
  handleColumnReorder: (startIndex: number, endIndex: number) => void;
}

export interface TableProviderProps {
  children: React.ReactNode;
  columns: DataColumn[];
  connection: DataTableConnection;
  config: DataTableConfig;
  editable?: boolean;
}
