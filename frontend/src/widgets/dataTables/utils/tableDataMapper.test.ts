import { describe, it, expect, vi } from 'vitest';
import * as arrow from 'apache-arrow';
import { convertArrowTableToData } from './tableDataMapper';
import { ColType, SortDirection, Align } from '../types/types';

describe('tableDataMapper', () => {
  describe('convertArrowTableToData', () => {
    it('should convert Arrow table with basic data types', () => {
      const mockField1 = {
        name: 'id',
        type: { toString: () => 'int64' },
        metadata: null,
      };
      const mockField2 = {
        name: 'name',
        type: { toString: () => 'utf8' },
        metadata: null,
      };
      const mockField3 = {
        name: 'active',
        type: { toString: () => 'bool' },
        metadata: null,
      };

      const mockSchema = {
        fields: [mockField1, mockField2, mockField3],
      };

      const mockColumn1 = { get: vi.fn(), length: 2 };
      const mockColumn2 = { get: vi.fn(), length: 2 };
      const mockColumn3 = { get: vi.fn(), length: 2 };

      const mockTable = {
        schema: mockSchema,
        numRows: 2,
        numCols: 3,
        getChildAt: vi.fn(),
      } as unknown as arrow.Table;

      // Setup mock return values for row iteration
      mockColumn1.get.mockImplementation((i: number) => (i === 0 ? 1 : 2));
      mockColumn2.get.mockImplementation((i: number) =>
        i === 0 ? 'Alice' : 'Bob'
      );
      mockColumn3.get.mockImplementation((i: number) =>
        i === 0 ? true : false
      );

      // getChildAt is called: 3 times for width calc, then 3 times per row (2 rows)
      (mockTable.getChildAt as ReturnType<typeof vi.fn>).mockImplementation(
        (index: number) => {
          if (index === 0) return mockColumn1;
          if (index === 1) return mockColumn2;
          if (index === 2) return mockColumn3;
          return null;
        }
      );

      const result = convertArrowTableToData(mockTable, 5);

      expect(result.columns).toEqual([
        { name: 'id', type: ColType.Number, width: expect.any(Number) },
        { name: 'name', type: ColType.Text, width: expect.any(Number) },
        { name: 'active', type: ColType.Boolean, width: expect.any(Number) },
      ]);

      expect(result.rows).toEqual([
        { values: [1, 'Alice', true] },
        { values: [2, 'Bob', false] },
      ]);

      expect(result.hasMore).toBe(false);
    });

    it('should handle null values', () => {
      const mockField = {
        name: 'nullable_field',
        type: { toString: () => 'utf8' },
      };
      const mockSchema = { fields: [mockField] };

      const mockColumn = { get: vi.fn() };
      const mockTable = {
        schema: mockSchema,
        numRows: 2,
        numCols: 1,
        getChildAt: vi.fn().mockReturnValue(mockColumn),
      } as unknown as arrow.Table;

      mockColumn.get.mockReturnValueOnce('value').mockReturnValueOnce(null);

      const result = convertArrowTableToData(mockTable, 5);

      expect(result.rows).toEqual([{ values: ['value'] }, { values: [null] }]);
    });

    it('should handle empty table', () => {
      const mockSchema = { fields: [] };
      const mockTable = {
        schema: mockSchema,
        numRows: 0,
        numCols: 0,
        getChildAt: vi.fn(),
      } as unknown as arrow.Table;

      const result = convertArrowTableToData(mockTable, 5);

      expect(result.columns).toEqual([]);
      expect(result.rows).toEqual([]);
      expect(result.hasMore).toBe(false);
    });

    it('should set hasMore to true when table rows equal requested count', () => {
      const mockField = { name: 'id', type: { toString: () => 'int64' } };
      const mockSchema = { fields: [mockField] };

      const mockColumn = { get: vi.fn().mockReturnValue(1) };
      const mockTable = {
        schema: mockSchema,
        numRows: 5,
        numCols: 1,
        getChildAt: vi.fn().mockReturnValue(mockColumn),
      } as unknown as arrow.Table;

      const result = convertArrowTableToData(mockTable, 5);

      expect(result.hasMore).toBe(true);
    });

    it('should set hasMore to false when table rows less than requested count', () => {
      const mockField = { name: 'id', type: { toString: () => 'int64' } };
      const mockSchema = { fields: [mockField] };

      const mockColumn = { get: vi.fn().mockReturnValue(1) };
      const mockTable = {
        schema: mockSchema,
        numRows: 3,
        numCols: 1,
        getChildAt: vi.fn().mockReturnValue(mockColumn),
      } as unknown as arrow.Table;

      const result = convertArrowTableToData(mockTable, 5);

      expect(result.hasMore).toBe(false);
    });

    it('should handle missing column data gracefully', () => {
      const mockField = { name: 'id', type: { toString: () => 'int64' } };
      const mockSchema = { fields: [mockField] };

      const mockTable = {
        schema: mockSchema,
        numRows: 1,
        numCols: 1,
        getChildAt: vi.fn().mockReturnValue(null),
      } as unknown as arrow.Table;

      const result = convertArrowTableToData(mockTable, 5);

      expect(result.columns).toEqual([
        { name: 'id', type: ColType.Number, width: 150 },
      ]);
      expect(result.rows).toEqual([{ values: [] }]);
    });

    it('should handle various data types correctly', () => {
      const mockFields = [
        { name: 'int_col', type: { toString: () => 'int32' }, metadata: null },
        {
          name: 'float_col',
          type: { toString: () => 'float64' },
          metadata: null,
        },
        {
          name: 'string_col',
          type: { toString: () => 'utf8' },
          metadata: null,
        },
        { name: 'bool_col', type: { toString: () => 'bool' }, metadata: null },
      ];
      const mockSchema = { fields: mockFields };

      const mockColumns = mockFields.map(() => ({ get: vi.fn(), length: 1 }));
      const mockTable = {
        schema: mockSchema,
        numRows: 1,
        numCols: 4,
        getChildAt: vi.fn(),
      } as unknown as arrow.Table;

      mockColumns[0].get.mockReturnValue(42);
      mockColumns[1].get.mockReturnValue(3.14);
      mockColumns[2].get.mockReturnValue('test');
      mockColumns[3].get.mockReturnValue(true);

      (mockTable.getChildAt as ReturnType<typeof vi.fn>)
        .mockReturnValueOnce(mockColumns[0])
        .mockReturnValueOnce(mockColumns[1])
        .mockReturnValueOnce(mockColumns[2])
        .mockReturnValueOnce(mockColumns[3])
        .mockReturnValueOnce(mockColumns[0])
        .mockReturnValueOnce(mockColumns[1])
        .mockReturnValueOnce(mockColumns[2])
        .mockReturnValueOnce(mockColumns[3]);

      const result = convertArrowTableToData(mockTable, 5);

      expect(result.rows).toEqual([{ values: [42, 3.14, 'test', true] }]);
    });

    it('should parse metadata for render_type and iconSet', () => {
      const mockMetadata = new Map([
        ['render_type', 'icon'],
        ['icon_set', 'lucide'],
      ]);

      const mockField = {
        name: 'status_icon',
        type: { toString: () => 'utf8' },
        metadata: mockMetadata,
      };

      const mockSchema = { fields: [mockField] };
      const mockColumn = {
        get: vi.fn().mockReturnValue('Activity'),
        length: 1,
      };
      const mockTable = {
        schema: mockSchema,
        numRows: 1,
        numCols: 1,
        getChildAt: vi.fn().mockReturnValue(mockColumn),
      } as unknown as arrow.Table;

      const result = convertArrowTableToData(mockTable, 5);

      expect(result.columns).toEqual([
        {
          name: 'status_icon',
          type: ColType.Icon, // render_type takes precedence
          width: expect.any(Number),
          iconSet: 'lucide',
        },
      ]);
    });

    it('should handle fields without metadata', () => {
      const mockField = {
        name: 'regular_field',
        type: { toString: () => 'utf8' },
        metadata: null,
      };

      const mockSchema = { fields: [mockField] };
      const mockColumn = { get: vi.fn().mockReturnValue('test'), length: 1 };
      const mockTable = {
        schema: mockSchema,
        numRows: 1,
        numCols: 1,
        getChildAt: vi.fn().mockReturnValue(mockColumn),
      } as unknown as arrow.Table;

      const result = convertArrowTableToData(mockTable, 5);

      expect(result.columns).toEqual([
        {
          name: 'regular_field',
          type: ColType.Text,
          width: expect.any(Number),
        },
      ]);
    });

    it('should parse all metadata properties', () => {
      const mockMetadata = new Map([
        ['header', 'Custom Header'],
        ['render_type', 'text'],
        ['icon_set', 'lucide'],
        ['group', 'Group A'],
        ['hidden', 'true'],
        ['sortable', 'false'],
        ['sort_direction', 'ascending'],
        ['filterable', 'true'],
        ['align', 'center'],
        ['order', '5'],
        ['icon', 'User'],
        ['help', 'Help text'],
      ]);

      const mockField = {
        name: 'test_col',
        type: { toString: () => 'utf8' },
        metadata: mockMetadata,
      };

      const mockSchema = { fields: [mockField] };
      const mockColumn = { get: vi.fn().mockReturnValue('test'), length: 1 };
      const mockTable = {
        schema: mockSchema,
        numRows: 1,
        numCols: 1,
        getChildAt: vi.fn().mockReturnValue(mockColumn),
      } as unknown as arrow.Table;

      const result = convertArrowTableToData(mockTable, 5);

      expect(result.columns[0]).toEqual({
        name: 'test_col',
        type: ColType.Text,
        width: expect.any(Number),
        header: 'Custom Header',
        group: 'Group A',
        hidden: true,
        sortable: false,
        sortDirection: SortDirection.Ascending,
        filterable: true,
        align: Align.Center,
        order: 5,
        icon: 'User',
        help: 'Help text',
        iconSet: 'lucide',
      });
    });

    it('should handle sort_direction values', () => {
      const testCases = [
        { input: 'ascending', expected: SortDirection.Ascending },
        { input: 'descending', expected: SortDirection.Descending },
        { input: 'none', expected: SortDirection.None },
      ];

      testCases.forEach(({ input, expected }) => {
        const mockMetadata = new Map([['sort_direction', input]]);
        const mockField = {
          name: 'col',
          type: { toString: () => 'utf8' },
          metadata: mockMetadata,
        };

        const mockSchema = { fields: [mockField] };
        const mockColumn = { get: vi.fn(), length: 1 };
        const mockTable = {
          schema: mockSchema,
          numRows: 1,
          numCols: 1,
          getChildAt: vi.fn().mockReturnValue(mockColumn),
        } as unknown as arrow.Table;

        const result = convertArrowTableToData(mockTable, 5);
        expect(result.columns[0].sortDirection).toBe(expected);
      });
    });

    it('should handle align values', () => {
      const testCases = [
        { input: 'left', expected: Align.Left },
        { input: 'center', expected: Align.Center },
        { input: 'right', expected: Align.Right },
      ];

      testCases.forEach(({ input, expected }) => {
        const mockMetadata = new Map([['align', input]]);
        const mockField = {
          name: 'col',
          type: { toString: () => 'utf8' },
          metadata: mockMetadata,
        };

        const mockSchema = { fields: [mockField] };
        const mockColumn = { get: vi.fn(), length: 1 };
        const mockTable = {
          schema: mockSchema,
          numRows: 1,
          numCols: 1,
          getChildAt: vi.fn().mockReturnValue(mockColumn),
        } as unknown as arrow.Table;

        const result = convertArrowTableToData(mockTable, 5);
        expect(result.columns[0].align).toBe(expected);
      });
    });

    it('should handle null values for icon and help', () => {
      const mockMetadata = new Map([
        ['icon', 'null'],
        ['help', 'null'],
      ]);

      const mockField = {
        name: 'col',
        type: { toString: () => 'utf8' },
        metadata: mockMetadata,
      };

      const mockSchema = { fields: [mockField] };
      const mockColumn = { get: vi.fn(), length: 1 };
      const mockTable = {
        schema: mockSchema,
        numRows: 1,
        numCols: 1,
        getChildAt: vi.fn().mockReturnValue(mockColumn),
      } as unknown as arrow.Table;

      const result = convertArrowTableToData(mockTable, 5);
      expect(result.columns[0].icon).toBe(null);
      expect(result.columns[0].help).toBe(null);
    });

    it('should handle type inference from Arrow types', () => {
      const typeTests = [
        { arrowType: 'int32', expectedType: ColType.Number },
        { arrowType: 'int64', expectedType: ColType.Number },
        { arrowType: 'float64', expectedType: ColType.Number },
        { arrowType: 'double', expectedType: ColType.Number },
        { arrowType: 'decimal', expectedType: ColType.Number },
        { arrowType: 'bool', expectedType: ColType.Boolean },
        { arrowType: 'boolean', expectedType: ColType.Boolean },
        { arrowType: 'date', expectedType: ColType.Date },
        { arrowType: 'timestamp', expectedType: ColType.DateTime },
        { arrowType: 'utf8', expectedType: ColType.Text },
        { arrowType: 'string', expectedType: ColType.Text },
      ];

      typeTests.forEach(({ arrowType, expectedType }) => {
        const mockField = {
          name: 'col',
          type: { toString: () => arrowType },
          metadata: null,
        };

        const mockSchema = { fields: [mockField] };
        const mockColumn = { get: vi.fn(), length: 1 };
        const mockTable = {
          schema: mockSchema,
          numRows: 1,
          numCols: 1,
          getChildAt: vi.fn().mockReturnValue(mockColumn),
        } as unknown as arrow.Table;

        const result = convertArrowTableToData(mockTable, 5);
        expect(result.columns[0].type).toBe(expectedType);
      });
    });

    it('should prioritize render_type over Arrow type', () => {
      const mockMetadata = new Map([['render_type', 'icon']]);

      const mockField = {
        name: 'col',
        type: { toString: () => 'utf8' }, // Arrow says text
        metadata: mockMetadata,
      };

      const mockSchema = { fields: [mockField] };
      const mockColumn = { get: vi.fn(), length: 1 };
      const mockTable = {
        schema: mockSchema,
        numRows: 1,
        numCols: 1,
        getChildAt: vi.fn().mockReturnValue(mockColumn),
      } as unknown as arrow.Table;

      const result = convertArrowTableToData(mockTable, 5);
      expect(result.columns[0].type).toBe(ColType.Icon); // render_type wins
    });
  });
});
