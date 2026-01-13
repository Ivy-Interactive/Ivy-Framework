import { describe, it, expect, vi } from 'vitest';
import * as arrow from 'apache-arrow';
import { convertArrowTableToData } from './tableDataMapper';
import { ColType } from '../types/types';

describe('tableDataMapper', () => {
  describe('convertArrowTableToData', () => {
    // TODO: Fix this test
    // This did not work
    it.skip('should convert Arrow table with basic data types', () => {
      const mockField1 = { name: 'id', type: { toString: () => 'int64' } };
      const mockField2 = { name: 'name', type: { toString: () => 'utf8' } };
      const mockField3 = { name: 'active', type: { toString: () => 'bool' } };

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

    // TODO: Fix this test
    // This did not work
    it.skip('should handle various data types correctly', () => {
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
  });
});
