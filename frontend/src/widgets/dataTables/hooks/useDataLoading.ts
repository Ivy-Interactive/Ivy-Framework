import { useCallback, useEffect } from 'react';

interface UseDataLoadingProps {
  containerRef: React.RefObject<HTMLDivElement | null>;
  visibleRows: number;
  isLoading: boolean;
  hasMore: boolean;
  loadMoreData: () => Promise<void>;
  rowHeight: number;
}

const scrollThreshold = 10;

/**
 * Hook to handle automatic data loading when container is large or when scrolling
 */
export const useDataLoading = ({
  containerRef,
  visibleRows,
  isLoading,
  hasMore,
  loadMoreData,
  rowHeight,
}: UseDataLoadingProps) => {
  // Check if we need to load more data when container height is large or when visible rows change
  useEffect(() => {
    if (!containerRef.current || visibleRows === 0 || isLoading) {
      return;
    }

    // Calculate if the container height can show more rows than we have loaded
    const containerHeight = containerRef.current.clientHeight;
    const availableHeight = containerHeight + rowHeight;
    const visibleRowCapacity = Math.ceil(availableHeight / rowHeight);

    // If container can show more rows than we have, and we have more data available, load it
    // This will keep loading until we have enough rows to fill the container
    if (visibleRowCapacity > visibleRows && hasMore) {
      loadMoreData();
    }
  }, [visibleRows, hasMore, isLoading, loadMoreData, containerRef, rowHeight]);

  // Handle scroll events
  const handleVisibleRegionChanged = useCallback(
    (range: { x: number; y: number; width: number; height: number }) => {
      const bottomRow = range.y + range.height;
      const shouldLoadMore = bottomRow >= visibleRows - scrollThreshold;
      if (!isLoading && shouldLoadMore && hasMore) {
        loadMoreData();
      }
    },
    [visibleRows, hasMore, loadMoreData, isLoading]
  );

  return {
    handleVisibleRegionChanged,
  };
};
