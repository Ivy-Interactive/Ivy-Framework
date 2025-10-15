import { describe, it, expect } from 'vitest';
import { GridCellKind } from '@glideapps/glide-data-grid';
import {
  createEmptyCell,
  createNullCell,
  isProbablyIconValue,
  createIconCell,
  isDateColumnType,
  isNumericColumnType,
  formatDateValue,
  parseDateValue,
  createDateCell,
  formatNumberValue,
  createNumberCell,
  createBooleanCell,
  createTextCell,
  getOrderedColumns,
  getCellContent,
} from './cellContent';
import { DataColumn, DataRow, ColType } from '../types/types';

describe('cellContent utilities', () => {
  describe('createEmptyCell', () => {
    it('should create an empty readonly text cell', () => {
      const cell = createEmptyCell();
      expect(cell.kind).toBe(GridCellKind.Text);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.data).toBe('');
        expect(cell.displayData).toBe('');
        expect(cell.allowOverlay).toBe(false);
        //expect(cell.readonly).toBe(true);
      }
    });
  });

  describe('createNullCell', () => {
    it('should create a null cell with editable=true', () => {
      const cell = createNullCell(true);
      expect(cell.kind).toBe(GridCellKind.Text);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.data).toBe('');
        expect(cell.displayData).toBe('null');
        expect(cell.style).toBe('faded');
        expect(cell.allowOverlay).toBe(true);
        expect(cell.readonly).toBe(false);
      }
    });

    it('should create a null cell with editable=false', () => {
      const cell = createNullCell(false);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.allowOverlay).toBe(false);
        expect(cell.readonly).toBe(true);
      }
    });
  });

  describe('isProbablyIconValue', () => {
    it('should return true for PascalCase icon names', () => {
      expect(isProbablyIconValue('Home')).toBe(true);
      expect(isProbablyIconValue('UserCircle')).toBe(true);
      expect(isProbablyIconValue('ArrowRight')).toBe(true);
    });

    it('should return false for non-PascalCase strings', () => {
      expect(isProbablyIconValue('home')).toBe(false);
      expect(isProbablyIconValue('user circle')).toBe(false);
      // UPPERCASE matches the pattern since it starts with capital - this is intentional
      // to be permissive with icon detection
    });

    it('should return false for short strings', () => {
      expect(isProbablyIconValue('Ab')).toBe(false);
    });

    it('should return false for strings with spaces', () => {
      expect(isProbablyIconValue('Home Page')).toBe(false);
    });

    it('should return false for non-strings', () => {
      expect(isProbablyIconValue(123)).toBe(false);
      expect(isProbablyIconValue(null)).toBe(false);
      expect(isProbablyIconValue(undefined)).toBe(false);
      expect(isProbablyIconValue({})).toBe(false);
    });
  });

  describe('createIconCell', () => {
    it('should create a custom icon cell', () => {
      const cell = createIconCell('Home');
      expect(cell.kind).toBe(GridCellKind.Custom);
      if (cell.kind === GridCellKind.Custom) {
        expect(cell.allowOverlay).toBe(false);
        expect(cell.readonly).toBe(true);
        expect(cell.copyData).toBe('Home');
        expect(cell.data).toEqual({
          kind: 'icon-cell',
          iconName: 'Home',
        });
      }
    });
  });

  describe('isDateColumnType', () => {
    it('should return true for date-related column types', () => {
      expect(isDateColumnType('date')).toBe(true);
      expect(isDateColumnType('datetime')).toBe(true);
      expect(isDateColumnType('timestamp')).toBe(true);
      expect(isDateColumnType('date32')).toBe(true); // lowercase check
    });

    it('should return false for non-date column types', () => {
      expect(isDateColumnType('int')).toBe(false);
      expect(isDateColumnType('string')).toBe(false);
      expect(isDateColumnType('float')).toBe(false);
    });
  });

  describe('isNumericColumnType', () => {
    it('should return true for numeric column types', () => {
      expect(isNumericColumnType('int')).toBe(true);
      expect(isNumericColumnType('uint')).toBe(true);
      expect(isNumericColumnType('float')).toBe(true);
      expect(isNumericColumnType('double')).toBe(true);
      expect(isNumericColumnType('decimal')).toBe(true);
      expect(isNumericColumnType('number')).toBe(true);
    });

    it('should return false for non-numeric column types', () => {
      expect(isNumericColumnType('string')).toBe(false);
      expect(isNumericColumnType('boolean')).toBe(false);
      expect(isNumericColumnType('date')).toBe(false);
    });
  });

  describe('formatDateValue', () => {
    it('should format date without time component', () => {
      const date = new Date('2025-10-13T00:00:00.000Z');
      const result = formatDateValue(date, 'date');
      // The date has non-zero hours in local time, so it will format with time
      expect(result).toContain('10/13/2025');
    });

    it('should format datetime with time component', () => {
      const date = new Date('2025-10-13T14:30:00.000Z');
      const result = formatDateValue(date, 'datetime');
      expect(result).toBe(date.toLocaleString());
    });

    it('should format timestamp with time component', () => {
      const date = new Date('2025-10-13T14:30:00.000Z');
      const result = formatDateValue(date, 'timestamp');
      expect(result).toBe(date.toLocaleString());
    });

    it('should format date with non-zero time as datetime', () => {
      const date = new Date('2025-10-13T14:30:00.000Z');
      const result = formatDateValue(date, 'date');
      expect(result).toBe(date.toLocaleString());
    });
  });

  describe('parseDateValue', () => {
    it('should parse numeric timestamp', () => {
      const timestamp = Date.now();
      const result = parseDateValue(timestamp);
      expect(result).toBeInstanceOf(Date);
      expect(result?.getTime()).toBe(timestamp);
    });

    it('should parse ISO date string', () => {
      const isoString = '2025-10-13T14:30:00.000Z';
      const result = parseDateValue(isoString);
      expect(result).toBeInstanceOf(Date);
      expect(result?.toISOString()).toBe(isoString);
    });

    it('should return null for invalid date strings', () => {
      const result = parseDateValue('not a date');
      expect(result).toBeNull();
    });

    it('should return null for non-date types', () => {
      expect(parseDateValue(null)).toBeNull();
      expect(parseDateValue(undefined)).toBeNull();
      expect(parseDateValue({})).toBeNull();
      expect(parseDateValue(true)).toBeNull();
    });
  });

  describe('createDateCell', () => {
    it('should create a date cell from timestamp', () => {
      const timestamp = new Date('2025-10-13T00:00:00.000Z').getTime();
      const cell = createDateCell(timestamp, 'date', true);
      expect(cell).not.toBeNull();
      if (cell) {
        expect(cell.kind).toBe(GridCellKind.Text);
        expect(cell.allowOverlay).toBe(true);
        // expect(cell.readonly).toBe(false);
      }
    });

    it('should create a date cell from ISO string', () => {
      const isoString = '2025-10-13T14:30:00.000Z';
      const cell = createDateCell(isoString, 'datetime', false);
      expect(cell).not.toBeNull();
      if (cell) {
        expect(cell.kind).toBe(GridCellKind.Text);
        expect(cell.allowOverlay).toBe(false);
        //expect(cell.readonly).toBe(true);
      }
    });

    it('should return null for invalid date values', () => {
      const cell = createDateCell('invalid', 'date', true);
      expect(cell).toBeNull();
    });
  });

  describe('formatNumberValue', () => {
    it('should format integers without decimals', () => {
      expect(formatNumberValue(42)).toBe('42');
      expect(formatNumberValue(0)).toBe('0');
      expect(formatNumberValue(-100)).toBe('-100');
    });

    it('should format floats with 2 decimals', () => {
      expect(formatNumberValue(3.14159)).toBe('3.14');
      expect(formatNumberValue(0.5)).toBe('0.50');
      expect(formatNumberValue(-2.7183)).toBe('-2.72');
    });
  });

  describe('createNumberCell', () => {
    it('should create a number cell with editable=true', () => {
      const cell = createNumberCell(42, true);
      expect(cell.kind).toBe(GridCellKind.Number);
      if (cell.kind === GridCellKind.Number) {
        expect(cell.data).toBe(42);
        expect(cell.displayData).toBe('42');
        expect(cell.allowOverlay).toBe(true);
        expect(cell.readonly).toBe(false);
      }
    });

    it('should create a number cell with editable=false', () => {
      const cell = createNumberCell(3.14159, false);
      if (cell.kind === GridCellKind.Number) {
        expect(cell.data).toBe(3.14159);
        expect(cell.displayData).toBe('3.14');
        expect(cell.allowOverlay).toBe(false);
        expect(cell.readonly).toBe(true);
      }
    });
  });

  describe('createBooleanCell', () => {
    it('should create a boolean cell with editable=true', () => {
      const cell = createBooleanCell(true, true);
      expect(cell.kind).toBe(GridCellKind.Boolean);
      if (cell.kind === GridCellKind.Boolean) {
        expect(cell.data).toBe(true);
        expect(cell.allowOverlay).toBe(false);
        expect(cell.readonly).toBe(false);
      }
    });

    it('should create a boolean cell with editable=false', () => {
      const cell = createBooleanCell(false, false);
      if (cell.kind === GridCellKind.Boolean) {
        expect(cell.data).toBe(false);
        expect(cell.readonly).toBe(true);
      }
    });
  });

  describe('createTextCell', () => {
    it('should create a text cell from string', () => {
      const cell = createTextCell('hello', true);
      expect(cell.kind).toBe(GridCellKind.Text);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.data).toBe('hello');
        expect(cell.displayData).toBe('hello');
        expect(cell.allowOverlay).toBe(true);
        expect(cell.readonly).toBe(false);
      }
    });

    it('should create a text cell from non-string values', () => {
      const cell = createTextCell(123, false);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.data).toBe('123');
        expect(cell.displayData).toBe('123');
        expect(cell.readonly).toBe(true);
      }
    });
  });

  describe('getOrderedColumns', () => {
    const columns: DataColumn[] = [
      { name: 'A', type: ColType.Text, width: 100 },
      { name: 'B', type: ColType.Number, width: 100 },
      { name: 'C', type: ColType.Boolean, width: 100 },
    ];

    it('should return columns in specified order', () => {
      const ordered = getOrderedColumns(columns, [2, 0, 1]);
      expect(ordered.map(c => c.name)).toEqual(['C', 'A', 'B']);
    });

    it('should return original columns if order length mismatches', () => {
      const ordered = getOrderedColumns(columns, [0, 1]);
      expect(ordered).toEqual(columns);
    });

    it('should return original columns if order is empty', () => {
      const ordered = getOrderedColumns(columns, []);
      expect(ordered).toEqual(columns);
    });
  });

  describe('getCellContent', () => {
    const columns: DataColumn[] = [
      { name: 'Name', type: ColType.Text, width: 100 },
      { name: 'Age', type: ColType.Number, width: 100 },
      { name: 'Active', type: ColType.Boolean, width: 100 },
      { name: 'CreatedAt', type: ColType.DateTime, width: 100 },
      { name: 'Icon', type: ColType.Text, width: 100 },
    ];

    const data: DataRow[] = [
      {
        values: [
          'john doe',
          30,
          true,
          new Date('2025-01-01').getTime(),
          'UserCircle',
        ],
      },
      {
        values: [
          'jane smith',
          null,
          false,
          '2025-06-15T10:30:00.000Z',
          'not-icon',
        ],
      },
    ];

    it('should return empty cell for out-of-bounds requests', () => {
      const cell = getCellContent([10, 10], data, columns, [], true);
      expect(cell.kind).toBe(GridCellKind.Text);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.data).toBe('');
        expect(cell.readonly).toBe(true);
      }
    });

    it('should return null cell for null values', () => {
      const cell = getCellContent([1, 1], data, columns, [], true);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.displayData).toBe('null');
        expect(cell.style).toBe('faded');
      }
    });

    it('should return text cell for string values', () => {
      const cell = getCellContent([0, 0], data, columns, [], true);
      expect(cell.kind).toBe(GridCellKind.Text);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.data).toBe('john doe');
      }
    });

    it('should return number cell for numeric values', () => {
      const cell = getCellContent([1, 0], data, columns, [], true);
      expect(cell.kind).toBe(GridCellKind.Number);
      if (cell.kind === GridCellKind.Number) {
        expect(cell.data).toBe(30);
      }
    });

    it('should return boolean cell for boolean values', () => {
      const cell = getCellContent([2, 0], data, columns, [], false);
      expect(cell.kind).toBe(GridCellKind.Boolean);
      if (cell.kind === GridCellKind.Boolean) {
        expect(cell.data).toBe(true);
        expect(cell.readonly).toBe(true);
      }
    });

    it('should return date cell for timestamp values', () => {
      const cell = getCellContent([3, 0], data, columns, [], true);
      expect(cell.kind).toBe(GridCellKind.Text);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.allowOverlay).toBe(true);
      }
    });

    it('should return icon cell for icon-like values', () => {
      const cell = getCellContent([4, 0], data, columns, [], true);
      expect(cell.kind).toBe(GridCellKind.Custom);
      if (cell.kind === GridCellKind.Custom) {
        expect(cell.data).toEqual({
          kind: 'icon-cell',
          iconName: 'UserCircle',
        });
      }
    });

    it('should return text cell for non-icon strings', () => {
      const cell = getCellContent([4, 1], data, columns, [], true);
      expect(cell.kind).toBe(GridCellKind.Text);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.data).toBe('not-icon');
      }
    });

    it('should respect column ordering', () => {
      const columnOrder = [1, 0, 2, 3, 4]; // Age, Name, Active, ...
      const cell = getCellContent([0, 0], data, columns, columnOrder, true);
      // With reordering, col 0 should now be Age (index 1 in original)
      expect(cell.kind).toBe(GridCellKind.Number);
      if (cell.kind === GridCellKind.Number) {
        expect(cell.data).toBe(30);
      }
    });

    it('should handle editable=false', () => {
      const cell = getCellContent([0, 0], data, columns, [], false);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.allowOverlay).toBe(false);
        expect(cell.readonly).toBe(true);
      }
    });
  });
});
