import { useMemo } from 'react';

interface UseEmptyRowsProps {
  containerHeight: number;
  visibleRows: number;
  hasMore: boolean;
  showGroups: boolean;
  rowHeight: number;
}

const GROUP_HEADER_HEIGHT = 36;

/**
 * Hook to calculate empty rows needed to fill container whitespace
 */
export const useEmptyRows = ({
  containerHeight,
  visibleRows,
  hasMore,
  showGroups,
  rowHeight,
}: UseEmptyRowsProps) => {
  // Calculate whitespace height needed to fill container
  const whitespaceHeight = useMemo(() => {
    // Only add whitespace when there's no more data to load
    if (hasMore || containerHeight === 0 || visibleRows === 0) {
      return 0;
    }

    // Calculate header height (regular header + group header if enabled)
    const headerHeight = rowHeight;
    const groupHeaderHeight = showGroups ? GROUP_HEADER_HEIGHT : 0;
    const totalHeaderHeight = headerHeight + groupHeaderHeight;

    // Calculate total height of visible rows
    const rowsHeight = visibleRows * rowHeight;

    // Calculate whitespace needed
    const calculatedWhitespace =
      containerHeight - totalHeaderHeight - rowsHeight;

    // Only return positive values
    return Math.max(0, calculatedWhitespace);
  }, [containerHeight, visibleRows, hasMore, rowHeight, showGroups]);

  // Calculate number of empty rows needed to fill whitespace
  const emptyRowsCount = useMemo(() => {
    if (whitespaceHeight <= 0) return 0;
    return Math.ceil(whitespaceHeight / rowHeight);
  }, [whitespaceHeight, rowHeight]);

  // Total rows including empty filler rows
  const totalRows = visibleRows + emptyRowsCount;

  return {
    emptyRowsCount,
    totalRows,
  };
};
