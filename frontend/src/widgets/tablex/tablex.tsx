import DataEditor, {
  GridCell,
  GridCellKind,
  GridColumn,
  Item,
  Theme,
} from '@glideapps/glide-data-grid';
import '@glideapps/glide-data-grid/dist/index.css';
import React, { useCallback, useEffect, useRef, useState } from 'react';

// Types
interface User {
  id: number;
  name: string;
  email: string;
  role: string;
  status: 'active' | 'inactive';
}

// Mock data generator
const generateMockUser = (index: number): User => ({
  id: index,
  name: `User ${index}`,
  email: `user${index}@example.com`,
  role: ['Admin', 'User', 'Manager', 'Developer'][index % 4],
  status: index % 3 === 0 ? 'inactive' : 'active',
});

// Simulate API fetch with delay
const fetchUsers = async (
  startIndex: number,
  count: number
): Promise<{ users: User[]; hasMore: boolean }> => {
  await new Promise(resolve => setTimeout(resolve, 500));
  const maxRecords = 500; // Total available records on "server"

  if (startIndex >= maxRecords) {
    return { users: [], hasMore: false };
  }

  const actualCount = Math.min(count, maxRecords - startIndex);
  const users = Array.from({ length: actualCount }, (_, i) =>
    generateMockUser(startIndex + i)
  );

  return {
    users,
    hasMore: startIndex + actualCount < maxRecords,
  };
};

// Column definitions
const columns: GridColumn[] = [
  { title: 'ID', width: 80 },
  { title: 'Name', width: 150 },
  { title: 'Email', width: 250 },
  { title: 'Role', width: 120 },
  { title: 'Status', width: 100 },
];

// Custom theme
const theme: Partial<Theme> = {
  bgCell: '#fff',
  bgHeader: '#f9fafb',
  bgHeaderHasFocus: '#f3f4f6',
  bgHeaderHovered: '#e5e7eb',
  textDark: '#111827',
  textMedium: '#6b7280',
  textLight: '#9ca3af',
  borderColor: '#e5e7eb',
  linkColor: '#e5e7eb',
  cellHorizontalPadding: 8,
  cellVerticalPadding: 3,
};

export const InfiniteScrollGlideGrid: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [visibleRows, setVisibleRows] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const loadingRef = useRef(false);
  const scrollTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const batchSize = 50;
  const scrollThreshold = 10; // Load more when within 10 rows of the bottom

  // Load initial data
  useEffect(() => {
    const loadInitialData = async () => {
      setIsLoading(true);
      const result = await fetchUsers(0, batchSize);
      setUsers(result.users);
      setVisibleRows(result.users.length);
      setHasMore(result.hasMore);
      setIsLoading(false);
    };
    loadInitialData();
  }, []);

  // Load more data
  const loadMoreData = useCallback(async () => {
    if (loadingRef.current || !hasMore) return;

    loadingRef.current = true;
    setIsLoading(true);

    const result = await fetchUsers(users.length, batchSize);

    if (result.users.length > 0) {
      setUsers(prev => [...prev, ...result.users]);
      setVisibleRows(prev => prev + result.users.length);
    }

    setHasMore(result.hasMore);
    setIsLoading(false);
    loadingRef.current = false;
  }, [users.length, hasMore]);

  // Handle scroll events
  const handleVisibleRegionChanged = useCallback(
    (range: { x: number; y: number; width: number; height: number }) => {
      // Clear any existing timeout
      if (scrollTimeoutRef.current) {
        clearTimeout(scrollTimeoutRef.current);
      }

      // Debounce the scroll check
      scrollTimeoutRef.current = setTimeout(() => {
        const bottomRow = range.y + range.height;
        const shouldLoadMore = bottomRow >= visibleRows - scrollThreshold;

        if (shouldLoadMore && hasMore && !loadingRef.current) {
          loadMoreData();
        }
      }, 100);
    },
    [visibleRows, hasMore, loadMoreData]
  );

  // Get cell content
  const getCellContent = useCallback(
    (cell: Item): GridCell => {
      const [col, row] = cell;

      // Safety check
      if (row >= users.length) {
        return {
          kind: GridCellKind.Text,
          data: '',
          displayData: '',
          allowOverlay: false,
          readonly: true,
        };
      }

      const user = users[row];

      switch (col) {
        case 0:
          return {
            kind: GridCellKind.Number,
            data: user.id,
            displayData: user.id.toString(),
            allowOverlay: false,
            readonly: true,
          };
        case 1:
          return {
            kind: GridCellKind.Text,
            data: user.name,
            displayData: user.name,
            allowOverlay: false,
            readonly: true,
          };
        case 2:
          return {
            kind: GridCellKind.Text,
            data: user.email,
            displayData: user.email,
            allowOverlay: false,
            readonly: true,
          };
        case 3:
          return {
            kind: GridCellKind.Text,
            data: user.role,
            displayData: user.role,
            allowOverlay: false,
            readonly: true,
          };
        case 4:
          return {
            kind: GridCellKind.Text,
            data: user.status,
            displayData: user.status,
            allowOverlay: false,
            readonly: true,
            themeOverride: {
              textDark: user.status === 'active' ? '#059669' : '#dc2626',
              bgCell: user.status === 'active' ? '#d1fae5' : '#fee2e2',
            },
          };
        default:
          return {
            kind: GridCellKind.Text,
            data: '',
            displayData: '',
            allowOverlay: false,
            readonly: true,
          };
      }
    },
    [users]
  );

  // Handle cell edits (not used in this example but required by the component)
  const onCellEdited = useCallback(() => {
    // This is a read-only grid, so we don't handle edits
  }, []);

  return (
    <div className="p-4">
      <h1 className="text-2xl font-bold mb-4">
        Infinite Scroll with Glide Data Grid
      </h1>

      <div className="mb-2 text-sm text-gray-600 flex items-center gap-4">
        <span>Showing {visibleRows} rows</span>
        {isLoading && (
          <span className="flex items-center">
            <div className="animate-spin h-4 w-4 border-2 border-gray-500 border-t-transparent rounded-full mr-2"></div>
            Loading more...
          </span>
        )}
        {!hasMore && <span className="text-gray-500">All data loaded</span>}
      </div>

      <div style={{ height: '600px', width: '100%' }}>
        <DataEditor
          columns={columns}
          rows={visibleRows}
          getCellContent={getCellContent}
          onCellEdited={onCellEdited}
          onVisibleRegionChanged={handleVisibleRegionChanged}
          smoothScrollX={true}
          smoothScrollY={true}
          theme={theme}
          rowHeight={36}
          headerHeight={36}
          freezeColumns={1}
          getCellsForSelection={true}
          keybindings={{ search: false }}
          rightElement={<div className="pr-2" />}
        />
      </div>

      <div className="mt-4 text-sm text-gray-500">
        The grid dynamically grows as you scroll. Only loaded rows are rendered
        in the DOM.
      </div>
    </div>
  );
};

export default InfiniteScrollGlideGrid;
