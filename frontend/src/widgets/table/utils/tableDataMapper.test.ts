import { describe, it, expect, vi } from 'vitest';
import * as arrow from 'apache-arrow';
import { convertArrowTableToData } from './tableDataMapper';

describe('tableDataMapper', () => {
  describe('convertArrowTableToData', () => {
    it.skip('should convert Arrow table with basic data types', () => {
      const mockField1 = { name: 'id', type: { toString: () => 'int64' } };
      const mockField2 = { name: 'name', type: { toString: () => 'utf8' } };
      const mockField3 = { name: 'active', type: { toString: () => 'bool' } };

      const mockSchema = {
        fields: [mockField1, mockField2, mockField3],
      };

      const mockColumn1 = { get: vi.fn() };
      const mockColumn2 = { get: vi.fn() };
      const mockColumn3 = { get: vi.fn() };

      const mockTable = {
        schema: mockSchema,
        numRows: 2,
        numCols: 3,
        getChildAt: vi.fn(),
      } as unknown as arrow.Table;

      // Setup mock return values
      mockColumn1.get.mockReturnValueOnce(1).mockReturnValueOnce(2);
      mockColumn2.get.mockReturnValueOnce('Alice').mockReturnValueOnce('Bob');
      mockColumn3.get.mockReturnValueOnce(true).mockReturnValueOnce(false);

      (mockTable.getChildAt as ReturnType<typeof vi.fn>)
        .mockReturnValueOnce(mockColumn1)
        .mockReturnValueOnce(mockColumn2)
        .mockReturnValueOnce(mockColumn3)
        .mockReturnValueOnce(mockColumn1)
        .mockReturnValueOnce(mockColumn2)
        .mockReturnValueOnce(mockColumn3);

      const result = convertArrowTableToData(mockTable, 5);

      expect(result.columns).toEqual([
        { name: 'id', type: 'int64', width: 150 },
        { name: 'name', type: 'utf8', width: 150 },
        { name: 'active', type: 'bool', width: 150 },
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
        { name: 'id', type: 'int64', width: 150 },
      ]);
      expect(result.rows).toEqual([{ values: [] }]);
    });

    it.skip('should handle various data types correctly', () => {
      const mockFields = [
        { name: 'int_col', type: { toString: () => 'int32' } },
        { name: 'float_col', type: { toString: () => 'float64' } },
        { name: 'string_col', type: { toString: () => 'utf8' } },
        { name: 'bool_col', type: { toString: () => 'bool' } },
      ];
      const mockSchema = { fields: mockFields };

      const mockColumns = mockFields.map(() => ({ get: vi.fn() }));
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
        .mockReturnValueOnce(mockColumns[3]);

      const result = convertArrowTableToData(mockTable, 5);

      expect(result.rows).toEqual([{ values: [42, 3.14, 'test', true] }]);
    });
  });
});
