import { describe, it, expect } from 'vitest';
import {
  getColumnTypeIcon,
  reorderColumns,
  convertToGridColumns,
} from './columnHelpers';
import type { DataColumn } from '../types/types';
import { ColType } from '../types/types';

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
      { name: 'First', type: ColType.Text, width: 100 },
      { name: 'Second', type: ColType.Number, width: 100 },
      { name: 'Third', type: ColType.Boolean, width: 100 },
      { name: 'Fourth', type: ColType.Date, width: 100 },
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

  describe('convertToGridColumns', () => {
    const mockColumns: DataColumn[] = [
      { name: 'ID', type: ColType.Number, width: 80 },
      { name: 'Name', type: ColType.Text, width: 150 },
      { name: 'Status', type: ColType.Boolean, width: 100 },
      { name: 'Created', type: ColType.Date, width: 120 },
    ];

    it('should convert columns to grid columns without reordering', () => {
      const columnWidths = {};
      const result = convertToGridColumns(
        mockColumns,
        [],
        columnWidths,
        0,
        false
      );

      expect(result).toEqual([
        { title: 'ID', width: 80, group: undefined },
        { title: 'Name', width: 150, group: undefined },
        { title: 'Status', width: 100, group: undefined },
        { title: 'Created', width: 120, group: undefined },
      ]);
    });

    it('should apply column ordering', () => {
      const columnOrder = [2, 0, 1, 3]; // Status, ID, Name, Created
      const columnWidths = {};
      const result = convertToGridColumns(
        mockColumns,
        columnOrder,
        columnWidths,
        0,
        false
      );

      expect(result.map(col => col.title)).toEqual([
        'Status',
        'ID',
        'Name',
        'Created',
      ]);
    });

    it('should use custom column widths when provided', () => {
      const columnWidths = {
        '0': 100, // ID: 80 -> 100
        '1': 200, // Name: 150 -> 200
      };
      const result = convertToGridColumns(
        mockColumns,
        [],
        columnWidths,
        0,
        false
      );

      expect('width' in result[0] && result[0].width).toBe(100);
      expect('width' in result[1] && result[1].width).toBe(200);
      expect('width' in result[2] && result[2].width).toBe(100); // unchanged
      expect('width' in result[3] && result[3].width).toBe(120); // unchanged
    });

    it('should make last column fill remaining width', () => {
      const columnWidths = {};
      const containerWidth = 600;
      const result = convertToGridColumns(
        mockColumns,
        [],
        columnWidths,
        containerWidth,
        false
      );

      // ID (80) + Name (150) + Status (100) = 330
      // Remaining: 600 - 330 = 270
      // Last column should be max(120, 270) - 10 = 260
      expect('width' in result[3] && result[3].width).toBe(260);
    });

    it('should not expand last column if containerWidth is 0', () => {
      const columnWidths = {};
      const result = convertToGridColumns(
        mockColumns,
        [],
        columnWidths,
        0,
        false
      );

      expect('width' in result[3] && result[3].width).toBe(120);
    });

    it('should include groups when showGroups is true', () => {
      const columnsWithGroups: DataColumn[] = [
        { name: 'ID', type: ColType.Number, width: 80, group: 'Identity' },
        { name: 'Name', type: ColType.Text, width: 150, group: 'Identity' },
        {
          name: 'Status',
          type: ColType.Boolean,
          width: 100,
          group: 'Metadata',
        },
      ];

      const result = convertToGridColumns(columnsWithGroups, [], {}, 0, true);

      expect(result[0].group).toBe('Identity');
      expect(result[1].group).toBe('Identity');
      expect(result[2].group).toBe('Metadata');
    });

    it('should not include groups when showGroups is false', () => {
      const columnsWithGroups: DataColumn[] = [
        { name: 'ID', type: ColType.Number, width: 80, group: 'Identity' },
        { name: 'Name', type: ColType.Text, width: 150, group: 'Identity' },
      ];

      const result = convertToGridColumns(columnsWithGroups, [], {}, 0, false);

      expect(result[0].group).toBeUndefined();
      expect(result[1].group).toBeUndefined();
    });

    it('should handle column ordering with custom widths', () => {
      const columnOrder = [1, 0, 2, 3]; // Name, ID, Status, Created
      const columnWidths = {
        '0': 100, // ID
        '1': 200, // Name
      };

      const result = convertToGridColumns(
        mockColumns,
        columnOrder,
        columnWidths,
        0,
        false
      );

      // After reordering: Name, ID, Status, Created
      expect(result[0].title).toBe('Name');
      expect('width' in result[0] && result[0].width).toBe(200); // Name has custom width
      expect(result[1].title).toBe('ID');
      expect('width' in result[1] && result[1].width).toBe(100); // ID has custom width
      expect(result[2].title).toBe('Status');
      expect('width' in result[2] && result[2].width).toBe(100); // Status uses original width
    });

    it('should handle empty columns array', () => {
      const result = convertToGridColumns([], [], {}, 0, false);
      expect(result).toEqual([]);
    });

    it('should handle single column', () => {
      const singleColumn: DataColumn[] = [
        { name: 'Only', type: ColType.Text, width: 100 },
      ];

      const result = convertToGridColumns(singleColumn, [], {}, 500, false);

      // Last (and only) column should expand: max(100, 500 - 0) - 10 = 490
      expect('width' in result[0] && result[0].width).toBe(490);
    });

    it('should filter out hidden columns', () => {
      const columns: DataColumn[] = [
        { name: 'Visible1', type: ColType.Text, width: 100 },
        { name: 'Hidden1', type: ColType.Text, width: 100, hidden: true },
        { name: 'Visible2', type: ColType.Number, width: 100 },
        { name: 'Hidden2', type: ColType.Boolean, width: 100, hidden: true },
      ];

      const result = convertToGridColumns(columns, [], {}, 0, false);

      expect(result).toHaveLength(2);
      expect(result[0].title).toBe('Visible1');
      expect(result[1].title).toBe('Visible2');
    });

    it('should use custom header when provided', () => {
      const columns: DataColumn[] = [
        {
          name: 'col1',
          header: 'Custom Header 1',
          type: ColType.Text,
          width: 100,
        },
        { name: 'col2', type: ColType.Number, width: 100 },
      ];

      const result = convertToGridColumns(columns, [], {}, 0, false);

      expect(result[0].title).toBe('Custom Header 1');
      expect(result[1].title).toBe('col2');
    });

    it('should apply column order property', () => {
      const columns: DataColumn[] = [
        { name: 'Third', type: ColType.Text, width: 100, order: 2 },
        { name: 'First', type: ColType.Number, width: 100, order: 0 },
        { name: 'Second', type: ColType.Boolean, width: 100, order: 1 },
      ];

      const result = convertToGridColumns(columns, [], {}, 0, false);

      expect(result[0].title).toBe('First');
      expect(result[1].title).toBe('Second');
      expect(result[2].title).toBe('Third');
    });

    it('should handle columns without order property', () => {
      const columns: DataColumn[] = [
        { name: 'A', type: ColType.Text, width: 100 },
        { name: 'B', type: ColType.Number, width: 100 },
      ];

      const result = convertToGridColumns(columns, [], {}, 0, false);

      expect(result[0].title).toBe('A');
      expect(result[1].title).toBe('B');
    });

    it('should prioritize order property over columnOrder array', () => {
      const columns: DataColumn[] = [
        { name: 'C', type: ColType.Text, width: 100, order: 2 },
        { name: 'A', type: ColType.Number, width: 100, order: 0 },
        { name: 'B', type: ColType.Boolean, width: 100, order: 1 },
      ];

      // columnOrder would suggest B, C, A but order property should win
      const result = convertToGridColumns(columns, [1, 2, 0], {}, 0, false);

      expect(result[0].title).toBe('A');
      expect(result[1].title).toBe('B');
      expect(result[2].title).toBe('C');
    });

    it('should filter hidden columns and apply order', () => {
      const columns: DataColumn[] = [
        { name: 'D', type: ColType.Text, width: 100, order: 3, hidden: true },
        { name: 'B', type: ColType.Number, width: 100, order: 1 },
        { name: 'A', type: ColType.Boolean, width: 100, order: 0 },
        { name: 'C', type: ColType.Date, width: 100, order: 2 },
      ];

      const result = convertToGridColumns(columns, [], {}, 0, false);

      expect(result).toHaveLength(3);
      expect(result[0].title).toBe('A');
      expect(result[1].title).toBe('B');
      expect(result[2].title).toBe('C');
    });

    it('should handle partial order values', () => {
      const columns: DataColumn[] = [
        { name: 'NoOrder1', type: ColType.Text, width: 100 },
        { name: 'First', type: ColType.Number, width: 100, order: 0 },
        { name: 'NoOrder2', type: ColType.Boolean, width: 100 },
      ];

      const result = convertToGridColumns(columns, [], {}, 0, false);

      // Columns with order come first, then columns without order
      expect(result[0].title).toBe('First');
      // NoOrder columns should maintain their relative position
      expect(result.map(r => r.title)).toContain('NoOrder1');
      expect(result.map(r => r.title)).toContain('NoOrder2');
    });
  });
});
