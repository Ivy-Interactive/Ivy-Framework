import React, { useMemo, useRef } from 'react';
import * as arrow from 'apache-arrow';
import { Filter } from '@/services/grpcTableService';
import { TableContext } from './tableContext';
import { TableProviderProps, TableContextType } from './types';
import {
  useDataLoading,
  useColumnManagement,
  useSorting,
  useRowData,
} from './hooks';

export const TableProvider: React.FC<TableProviderProps> = ({
  children,
  columns: columnsProp,
  connection,
  config,
  editable = false,
}) => {
  const [visibleRows, setVisibleRows] = React.useState(0);
  const [error, setError] = React.useState<string | null>(null);
  const [activeFilter, setActiveFilter] = React.useState<Filter | null>(null);

  const arrowTableRef = useRef<arrow.Table | null>(null);
  const { allowColumnResizing, allowSorting } = config;

  // Column management
  const {
    columns,
    setColumns,
    columnWidths,
    columnOrder,
    resetColumnWidths,
    initializeColumnOrder,
    initializeColumnWidths,
    handleColumnResize,
    handleColumnReorder,
  } = useColumnManagement({
    columnsProp,
    allowColumnResizing: allowColumnResizing ?? true,
  });

  // Sorting
  const { activeSort, handleSort, initializeSortFromColumns } = useSorting({
    allowSorting: allowSorting ?? true,
  });

  // Reset column widths when connection changes
  React.useEffect(() => {
    resetColumnWidths();
  }, [connection, resetColumnWidths]);

  // Data loading
  const { isLoading, hasMore, loadMoreData } = useDataLoading({
    connection,
    config,
    columnsProp,
    activeFilter,
    activeSort,
    columnOrderLength: columnOrder.length,
    arrowTableRef,
    setColumns,
    setVisibleRows,
    setError,
    initializeColumnOrder,
    initializeColumnWidths,
    initializeSortFromColumns,
  });

  // Row data accessor
  const { getRowData } = useRowData(arrowTableRef);

  const value: TableContextType = useMemo(() => {
    const contextValue: TableContextType = {
      columns,
      columnWidths,
      visibleRows,
      isLoading,
      hasMore,
      error,
      editable,
      connection,
      config,
      activeFilter,
      activeSort,
      columnOrder,
      getRowData,
      arrowTableRef,
      loadMoreData,
      handleColumnResize,
      handleSort,
      setActiveFilter,
      setError,
      handleColumnReorder,
    };
    return contextValue;
  }, [
    columns,
    columnWidths,
    visibleRows,
    isLoading,
    hasMore,
    error,
    editable,
    connection,
    config,
    activeFilter,
    activeSort,
    columnOrder,
    getRowData,
    arrowTableRef,
    loadMoreData,
    handleColumnResize,
    handleSort,
    setActiveFilter,
    setError,
    handleColumnReorder,
  ]);

  return (
    <TableContext.Provider value={value}>{children}</TableContext.Provider>
  );
};
