import { describe, it, expect } from 'vitest';
import { getColumnTypeIcon, reorderColumns } from './columnHelpers';
import type { DataColumn } from '../types/types';

describe('columnHelpers', () => {
  describe('getColumnTypeIcon', () => {
    it('should return Hash icon for numeric types', () => {
      expect(getColumnTypeIcon('int64')).toBe('Hash');
      expect(getColumnTypeIcon('int32')).toBe('Hash');
      expect(getColumnTypeIcon('float64')).toBe('Hash');
      expect(getColumnTypeIcon('double')).toBe('Hash');
    });

    it('should return Calendar icon for date/time types', () => {
      expect(getColumnTypeIcon('date')).toBe('Calendar');
      expect(getColumnTypeIcon('datetime')).toBe('Calendar');
      expect(getColumnTypeIcon('timestamp')).toBe('Calendar');
    });

    it('should return ToggleLeft icon for boolean types', () => {
      expect(getColumnTypeIcon('bool')).toBe('ToggleLeft');
      expect(getColumnTypeIcon('boolean')).toBe('ToggleLeft');
    });

    it('should return Type icon for string types', () => {
      expect(getColumnTypeIcon('utf8')).toBe('Type');
      expect(getColumnTypeIcon('string')).toBe('Type');
    });

    it('should return Type icon for unknown types', () => {
      expect(getColumnTypeIcon('unknown')).toBe('Type');
      expect(getColumnTypeIcon('custom_type')).toBe('Type');
    });
  });

  describe('reorderColumns', () => {
    const mockColumns: DataColumn[] = [
      { name: 'First', type: 'string', width: 100 },
      { name: 'Second', type: 'int64', width: 100 },
      { name: 'Third', type: 'bool', width: 100 },
      { name: 'Fourth', type: 'date', width: 100 },
    ];

    it('should move column from start to middle', () => {
      const result = reorderColumns(mockColumns, 0, 2);
      expect(result.map(c => c.name)).toEqual([
        'Second',
        'Third',
        'First',
        'Fourth',
      ]);
    });

    it('should move column from middle to start', () => {
      const result = reorderColumns(mockColumns, 2, 0);
      expect(result.map(c => c.name)).toEqual([
        'Third',
        'First',
        'Second',
        'Fourth',
      ]);
    });

    it('should move column from middle to end', () => {
      const result = reorderColumns(mockColumns, 1, 3);
      expect(result.map(c => c.name)).toEqual([
        'First',
        'Third',
        'Fourth',
        'Second',
      ]);
    });

    it('should not modify original array', () => {
      const original = [...mockColumns];
      reorderColumns(mockColumns, 0, 2);
      expect(mockColumns).toEqual(original);
    });

    it('should handle same start and end index', () => {
      const result = reorderColumns(mockColumns, 1, 1);
      expect(result).toEqual(mockColumns);
    });
  });
});
