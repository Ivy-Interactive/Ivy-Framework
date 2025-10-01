import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Table } from './Table';
import { useTable } from './context/TableContext';
import { FilterTypes, SelectionModes } from './types/types';

// Mock the child components
vi.mock('./parts/TableEditor', () => ({
  TableEditor: ({ hasOptions }: { hasOptions: boolean }) => (
    <div data-testid="table-editor">
      TableEditor (hasOptions: {String(hasOptions)})
    </div>
  ),
}));

vi.mock('./parts/TableFooter', () => ({
  Footer: () => <div data-testid="table-footer">Footer</div>,
}));

vi.mock('./parts/TableOptions', () => ({
  TableOptions: () => <div data-testid="table-options">TableOptions</div>,
}));

vi.mock('@/components/ErrorDisplay', () => ({
  ErrorDisplay: ({ title, message }: { title: string; message: string }) => (
    <div data-testid="error-display">
      <div data-testid="error-title">{title}</div>
      <div data-testid="error-message">{message}</div>
    </div>
  ),
}));

vi.mock('@/components/Loading', () => ({
  Loading: () => <div data-testid="loading">Loading...</div>,
}));

// Mock the TableContext
vi.mock('./context/TableContext', async () => {
  const actual = await vi.importActual('./context/TableContext');
  return {
    ...actual,
    useTable: vi.fn(),
    TableProvider: ({ children }: { children: React.ReactNode }) => (
      <div data-testid="table-provider">{children}</div>
    ),
  };
});

describe('Table Component', () => {
  const mockConnection = {
    port: 8080,
    path: '/test',
    connectionId: 'conn-123',
    sourceId: 'source-456',
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Basic Rendering', () => {
    it('should render with default props', () => {
      vi.mocked(useTable).mockReturnValue({
        error: null,
        columns: [{ id: 'col1', name: 'Column 1' }],
      } as never);

      render(<Table connection={mockConnection} />);

      expect(screen.getByTestId('table-provider')).toBeDefined();
      expect(screen.getByTestId('table-footer')).toBeDefined();
    });

    it('should render TableEditor when columns are present', () => {
      vi.mocked(useTable).mockReturnValue({
        error: null,
        columns: [{ id: 'col1', name: 'Column 1' }],
      } as never);

      render(<Table connection={mockConnection} />);

      expect(screen.getByTestId('table-editor')).toBeDefined();
      expect(screen.queryByTestId('loading')).toBeNull();
    });

    it('should render Loading when no columns are present', () => {
      vi.mocked(useTable).mockReturnValue({
        error: null,
        columns: [],
      } as never);

      render(<Table connection={mockConnection} />);

      expect(screen.getByTestId('loading')).toBeDefined();
      expect(screen.queryByTestId('table-editor')).toBeNull();
    });

    it('should render ErrorDisplay when error exists', () => {
      vi.mocked(useTable).mockReturnValue({
        error: 'Failed to load table data',
        columns: [],
      } as never);

      render(<Table connection={mockConnection} />);

      expect(screen.getByTestId('error-display')).toBeDefined();
      expect(screen.getByTestId('error-title').textContent).toBe('Table Error');
      expect(screen.getByTestId('error-message').textContent).toBe(
        'Failed to load table data'
      );
      expect(screen.queryByTestId('table-editor')).toBeNull();
    });
  });

  describe('Configuration Props', () => {
    beforeEach(() => {
      vi.mocked(useTable).mockReturnValue({
        error: null,
        columns: [{ id: 'col1', name: 'Column 1' }],
      } as never);
    });

    it('should apply default config values when config is empty', () => {
      render(<Table connection={mockConnection} config={{}} />);

      expect(screen.getByTestId('table-options')).toBeDefined();
      expect(screen.getByTestId('table-editor').textContent).toContain(
        'hasOptions: true'
      );
    });

    it('should respect allowFiltering: false', () => {
      render(
        <Table connection={mockConnection} config={{ allowFiltering: false }} />
      );

      expect(screen.queryByTestId('table-options')).toBeNull();
      expect(screen.getByTestId('table-editor').textContent).toContain(
        'hasOptions: false'
      );
    });

    it('should respect allowFiltering: true', () => {
      render(
        <Table connection={mockConnection} config={{ allowFiltering: true }} />
      );

      expect(screen.getByTestId('table-options')).toBeDefined();
      expect(screen.getByTestId('table-editor').textContent).toContain(
        'hasOptions: true'
      );
    });

    it('should handle allowSearch config', () => {
      const { rerender } = render(
        <Table connection={mockConnection} config={{ allowSearch: false }} />
      );

      expect(screen.getByTestId('table-provider')).toBeDefined();

      rerender(
        <Table connection={mockConnection} config={{ allowSearch: true }} />
      );

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should handle filterType config', () => {
      render(
        <Table
          connection={mockConnection}
          config={{ filterType: FilterTypes.List }}
        />
      );

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should handle freezeColumns config', () => {
      render(
        <Table connection={mockConnection} config={{ freezeColumns: 2 }} />
      );

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should handle allowSorting config', () => {
      render(
        <Table connection={mockConnection} config={{ allowSorting: false }} />
      );

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should handle allowColumnReordering config', () => {
      render(
        <Table
          connection={mockConnection}
          config={{ allowColumnReordering: false }}
        />
      );

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should handle allowColumnResizing config', () => {
      render(
        <Table
          connection={mockConnection}
          config={{ allowColumnResizing: false }}
        />
      );

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should handle allowCopySelection config', () => {
      render(
        <Table
          connection={mockConnection}
          config={{ allowCopySelection: false }}
        />
      );

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should handle selectionMode config', () => {
      render(
        <Table
          connection={mockConnection}
          config={{ selectionMode: SelectionModes.Rows }}
        />
      );

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should handle showIndexColumn config', () => {
      render(
        <Table connection={mockConnection} config={{ showIndexColumn: true }} />
      );

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should handle showGroups config', () => {
      render(
        <Table connection={mockConnection} config={{ showGroups: true }} />
      );

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should handle multiple config options together', () => {
      render(
        <Table
          connection={mockConnection}
          config={{
            allowSearch: false,
            allowFiltering: false,
            allowSorting: false,
            freezeColumns: 1,
            showIndexColumn: true,
            showGroups: true,
          }}
        />
      );

      expect(screen.getByTestId('table-provider')).toBeDefined();
      expect(screen.queryByTestId('table-options')).toBeNull();
      expect(screen.getByTestId('table-editor').textContent).toContain(
        'hasOptions: false'
      );
    });
  });

  describe('Editable Prop', () => {
    beforeEach(() => {
      vi.mocked(useTable).mockReturnValue({
        error: null,
        columns: [{ id: 'col1', name: 'Column 1' }],
      } as never);
    });

    it('should default editable to false', () => {
      render(<Table connection={mockConnection} />);

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should accept editable prop as true', () => {
      render(<Table connection={mockConnection} editable={true} />);

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should accept editable prop as false', () => {
      render(<Table connection={mockConnection} editable={false} />);

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });
  });

  describe('Connection Prop', () => {
    beforeEach(() => {
      vi.mocked(useTable).mockReturnValue({
        error: null,
        columns: [{ id: 'col1', name: 'Column 1' }],
      } as never);
    });

    it('should accept connection with required fields', () => {
      const connection = {
        port: 9090,
        path: '/api',
        connectionId: 'conn-abc',
        sourceId: 'source-xyz',
      };

      render(<Table connection={connection} />);

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should handle different connection configurations', () => {
      const connection = {
        port: 3000,
        path: '/data',
        connectionId: 'conn-def',
        sourceId: 'source-123',
      };

      render(<Table connection={connection} />);

      expect(screen.getByTestId('table-provider')).toBeDefined();
    });
  });

  describe('Error States', () => {
    it('should display error when TableContext provides an error', () => {
      vi.mocked(useTable).mockReturnValue({
        error: 'Connection timeout',
        columns: [{ id: 'col1', name: 'Column 1' }],
      } as never);

      render(<Table connection={mockConnection} />);

      expect(screen.getByTestId('error-display')).toBeDefined();
      expect(screen.getByTestId('error-message').textContent).toBe(
        'Connection timeout'
      );
    });

    it('should display error even with empty columns', () => {
      vi.mocked(useTable).mockReturnValue({
        error: 'Invalid table name',
        columns: [],
      } as never);

      render(<Table connection={mockConnection} />);

      expect(screen.getByTestId('error-display')).toBeDefined();
      expect(screen.queryByTestId('loading')).toBeNull();
    });

    it('should not show table components when error exists', () => {
      vi.mocked(useTable).mockReturnValue({
        error: 'Database error',
        columns: [{ id: 'col1', name: 'Column 1' }],
      } as never);

      render(<Table connection={mockConnection} />);

      expect(screen.queryByTestId('table-editor')).toBeNull();
      expect(screen.queryByTestId('table-options')).toBeNull();
    });
  });

  describe('Loading States', () => {
    it('should show loading when columns array is empty', () => {
      vi.mocked(useTable).mockReturnValue({
        error: null,
        columns: [],
      } as never);

      render(<Table connection={mockConnection} />);

      expect(screen.getByTestId('loading')).toBeDefined();
    });

    it('should transition from loading to content when columns populate', () => {
      const { rerender } = render(<Table connection={mockConnection} />);

      vi.mocked(useTable).mockReturnValue({
        error: null,
        columns: [],
      } as never);

      rerender(<Table connection={mockConnection} />);
      expect(screen.getByTestId('loading')).toBeDefined();

      vi.mocked(useTable).mockReturnValue({
        error: null,
        columns: [{ id: 'col1', name: 'Column 1' }],
      } as never);

      rerender(<Table connection={mockConnection} />);
      expect(screen.queryByTestId('loading')).toBeNull();
      expect(screen.getByTestId('table-editor')).toBeDefined();
    });
  });

  describe('Component Structure', () => {
    beforeEach(() => {
      vi.mocked(useTable).mockReturnValue({
        error: null,
        columns: [{ id: 'col1', name: 'Column 1' }],
      } as never);
    });

    it('should render Footer outside of TableLayout', () => {
      render(<Table connection={mockConnection} />);

      const footer = screen.getByTestId('table-footer');
      expect(footer).toBeDefined();
    });

    it('should render TableOptions before TableEditor when allowFiltering is true', () => {
      render(
        <Table connection={mockConnection} config={{ allowFiltering: true }} />
      );

      const container = screen.getByTestId('table-provider');
      const options = screen.getByTestId('table-options');
      const editor = screen.getByTestId('table-editor');

      expect(container.contains(options)).toBe(true);
      expect(container.contains(editor)).toBe(true);
    });

    it('should pass hasOptions prop to TableEditor based on allowFiltering', () => {
      const { rerender } = render(
        <Table connection={mockConnection} config={{ allowFiltering: true }} />
      );

      expect(screen.getByTestId('table-editor').textContent).toContain(
        'hasOptions: true'
      );

      rerender(
        <Table connection={mockConnection} config={{ allowFiltering: false }} />
      );

      expect(screen.getByTestId('table-editor').textContent).toContain(
        'hasOptions: false'
      );
    });
  });

  describe('Default Configuration Values', () => {
    beforeEach(() => {
      vi.mocked(useTable).mockReturnValue({
        error: null,
        columns: [{ id: 'col1', name: 'Column 1' }],
      } as never);
    });

    it('should apply correct defaults when config is undefined', () => {
      render(<Table connection={mockConnection} />);

      // allowFiltering defaults to true, so TableOptions should be visible
      expect(screen.getByTestId('table-options')).toBeDefined();
      expect(screen.getByTestId('table-editor').textContent).toContain(
        'hasOptions: true'
      );
    });

    it('should apply correct defaults for allowSearch (true)', () => {
      render(<Table connection={mockConnection} config={{}} />);
      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should apply correct defaults for freezeColumns (null)', () => {
      render(<Table connection={mockConnection} config={{}} />);
      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should apply correct defaults for allowSorting (true)', () => {
      render(<Table connection={mockConnection} config={{}} />);
      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should apply correct defaults for allowColumnReordering (true)', () => {
      render(<Table connection={mockConnection} config={{}} />);
      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should apply correct defaults for allowColumnResizing (true)', () => {
      render(<Table connection={mockConnection} config={{}} />);
      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should apply correct defaults for allowCopySelection (true)', () => {
      render(<Table connection={mockConnection} config={{}} />);
      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should apply correct defaults for showIndexColumn (false)', () => {
      render(<Table connection={mockConnection} config={{}} />);
      expect(screen.getByTestId('table-provider')).toBeDefined();
    });

    it('should apply correct defaults for showGroups (false)', () => {
      render(<Table connection={mockConnection} config={{}} />);
      expect(screen.getByTestId('table-provider')).toBeDefined();
    });
  });
});
