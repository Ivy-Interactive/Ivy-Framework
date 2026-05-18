import { useCallback, useRef, useState, useMemo } from "react";
import { useStream } from "@/components/stream-handler/hooks";
import { DataColumn, DataTableCellUpdate, DataRow } from "../../types/types";

interface UseCellUpdatesProps {
  streamId: string | undefined;
  columns: DataColumn[];
  idColumnName: string | null | undefined;
  getBaseRowData: (rowIndex: number) => DataRow | null;
}

type CellOverrides = Map<string, Map<string, unknown>>;

export const useCellUpdates = ({
  streamId,
  columns,
  idColumnName,
  getBaseRowData,
}: UseCellUpdatesProps) => {
  const overridesRef = useRef<CellOverrides>(new Map());
  const [version, setVersion] = useState(0);
  const pendingRef = useRef(false);

  const onStreamData = useCallback((update: DataTableCellUpdate) => {
    const rowKey = String(update.rowId);
    let rowOverrides = overridesRef.current.get(rowKey);
    if (!rowOverrides) {
      rowOverrides = new Map();
      overridesRef.current.set(rowKey, rowOverrides);
    }
    rowOverrides.set(update.columnName, update.value);

    if (!pendingRef.current) {
      pendingRef.current = true;
      requestAnimationFrame(() => {
        pendingRef.current = false;
        setVersion((v) => v + 1);
      });
    }
  }, []);

  useStream<DataTableCellUpdate>(streamId, onStreamData);

  const colIndexMap = useMemo(() => {
    const map = new Map<string, number>();
    columns.forEach((col, idx) => map.set(col.name, idx));
    return map;
  }, [columns]);

  const getRowData = useCallback(
    (rowIndex: number): DataRow | null => {
      const baseRow = getBaseRowData(rowIndex);
      if (!baseRow || !idColumnName) return baseRow;

      const idColIdx = colIndexMap.get(idColumnName);
      if (idColIdx === undefined) return baseRow;

      const rowId = baseRow.values[idColIdx];
      if (rowId == null) return baseRow;

      const rowOverrides = overridesRef.current.get(String(rowId));
      if (!rowOverrides || rowOverrides.size === 0) return baseRow;

      const newValues = [...baseRow.values];
      for (const [colName, value] of rowOverrides) {
        const colIdx = colIndexMap.get(colName);
        if (colIdx !== undefined) {
          newValues[colIdx] = value as DataRow["values"][number];
        }
      }
      return { values: newValues };
    },
    [getBaseRowData, colIndexMap, idColumnName, version],
  );

  return { getRowData };
};
