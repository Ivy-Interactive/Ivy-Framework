import { useMemo } from "react";
import { GROUP_HEADER_HEIGHT } from "../dataTableEditor/constants";

interface UseEmptyRowsProps {
  scrollContainerHeight: number;
  visibleRows: number;
  hasMore: boolean;
  showGroups: boolean;
  rowHeight: number;
}

/**
 * Calculates empty filler rows to fill the container when data is sparse.
 * Uses floor to avoid overflow (extra row would trigger unwanted scrollbar).
 */
export const useEmptyRows = ({
  scrollContainerHeight,
  visibleRows,
  hasMore,
  showGroups,
  rowHeight,
}: UseEmptyRowsProps) => {
  const whitespaceHeight = useMemo(() => {
    if (hasMore || scrollContainerHeight === 0 || visibleRows === 0) return 0;

    const totalHeaderHeight = rowHeight + (showGroups ? GROUP_HEADER_HEIGHT : 0);
    const rowsHeight = visibleRows * rowHeight;
    const safeHeight = Math.floor(scrollContainerHeight);

    return Math.max(0, safeHeight - totalHeaderHeight - rowsHeight);
  }, [scrollContainerHeight, visibleRows, hasMore, rowHeight, showGroups]);

  const emptyRowsCount = useMemo(() => {
    if (whitespaceHeight <= 0) return 0;
    return Math.floor(whitespaceHeight / rowHeight);
  }, [whitespaceHeight, rowHeight]);

  return {
    emptyRowsCount,
    totalRows: visibleRows + emptyRowsCount,
  };
};
