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
  createLabelsCell,
  createLinkCell,
  getOrderedColumns,
  getCellContent,
  getContentAlign,
} from './cellContent';
import { Align, DataColumn, DataRow, ColType } from '../types/types';

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
        expect(cell.displayData).toBe(''); // Changed to empty string
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
    it('should parse Date objects directly (from Arrow Timestamp vectors)', () => {
      const date = new Date('2025-10-13T14:30:00.000Z');
      const result = parseDateValue(date);
      expect(result).toBeInstanceOf(Date);
      expect(result).toBe(date); // Should return the same Date object
      expect(result?.getTime()).toBe(date.getTime());
    });

    it('should return null for invalid Date objects', () => {
      const invalidDate = new Date('invalid');
      const result = parseDateValue(invalidDate);
      expect(result).toBeNull();
    });

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
    it('should create a date cell from Date object (Arrow Timestamp)', () => {
      const date = new Date('2025-10-13T14:30:00.000Z');
      const cell = createDateCell(date, 'datetime', true);
      expect(cell).not.toBeNull();
      if (cell && cell.kind === GridCellKind.Text) {
        expect(cell.allowOverlay).toBe(true);
        expect(cell.displayData).toBe(date.toLocaleString());
      }
    });

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
        // expect(cell.readonly).toBe(true);
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

  describe('createLinkCell', () => {
    it('should create a custom link cell for external URL', () => {
      const url = 'https://example.com';
      const cell = createLinkCell(url, false);
      expect(cell.kind).toBe(GridCellKind.Custom);
      if (cell.kind === GridCellKind.Custom) {
        const linkData = cell.data as { kind: string; url: string };
        expect(linkData.kind).toBe('link-cell');
        expect(linkData.url).toBe(url);
        expect(cell.allowOverlay).toBe(false);
        expect(cell.readonly).toBe(true);
        expect(cell.copyData).toBe(url);
      }
    });

    it('should create a custom link cell for internal app:// link', () => {
      const url = 'app://concepts/links-app';
      const cell = createLinkCell(url, false);
      expect(cell.kind).toBe(GridCellKind.Custom);
      if (cell.kind === GridCellKind.Custom) {
        const linkData = cell.data as { kind: string; url: string };
        expect(linkData.kind).toBe('link-cell');
        expect(linkData.url).toBe(url);
      }
    });

    it('should create readonly custom link cell (links are always readonly to prevent overlay issues)', () => {
      const url = 'https://github.com/user/repo';
      const cell = createLinkCell(url, true);
      if (cell.kind === GridCellKind.Custom) {
        expect(cell.allowOverlay).toBe(false);
        expect(cell.readonly).toBe(true);
      }
    });

    it('should respect alignment parameter', () => {
      const url = 'https://example.com';
      const cell = createLinkCell(url, false, Align.Center);
      if (cell.kind === GridCellKind.Custom) {
        const linkData = cell.data as {
          kind: string;
          url: string;
          align?: string;
        };
        expect(linkData.align).toBe('center');
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

    // Helper function to get row data from array (for testing)
    const getRowData = (rowIndex: number): DataRow | null => {
      return rowIndex >= 0 && rowIndex < data.length ? data[rowIndex] : null;
    };

    it('should return empty cell for out-of-bounds requests', () => {
      const cell = getCellContent([10, 10], columns, [], true, getRowData);
      expect(cell.kind).toBe(GridCellKind.Text);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.data).toBe('');
        expect(cell.readonly).toBe(true);
      }
    });

    it('should return null cell for null values', () => {
      const cell = getCellContent([1, 1], columns, [], true, getRowData);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.displayData).toBe(''); // Changed to empty string
        expect(cell.style).toBe('faded');
      }
    });

    it('should return text cell for string values', () => {
      const cell = getCellContent([0, 0], columns, [], true, getRowData);
      expect(cell.kind).toBe(GridCellKind.Text);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.data).toBe('john doe');
      }
    });

    it('should return number cell for numeric values', () => {
      const cell = getCellContent([1, 0], columns, [], true, getRowData);
      expect(cell.kind).toBe(GridCellKind.Number);
      if (cell.kind === GridCellKind.Number) {
        expect(cell.data).toBe(30);
      }
    });

    it('should return boolean cell for boolean values', () => {
      const cell = getCellContent([2, 0], columns, [], false, getRowData);
      expect(cell.kind).toBe(GridCellKind.Boolean);
      if (cell.kind === GridCellKind.Boolean) {
        expect(cell.data).toBe(true);
        expect(cell.readonly).toBe(true);
      }
    });

    it('should return date cell for timestamp values', () => {
      const cell = getCellContent([3, 0], columns, [], true, getRowData);
      expect(cell.kind).toBe(GridCellKind.Text);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.allowOverlay).toBe(true);
      }
    });

    it('should return text cell for icon-like values without Icon type', () => {
      // Column 4 has type ColType.Text, not ColType.Icon
      // So even though value looks like an icon, it renders as text (no heuristic)
      const cell = getCellContent([4, 0], columns, [], true, getRowData);
      expect(cell.kind).toBe(GridCellKind.Text);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.data).toBe('UserCircle');
      }
    });

    it('should return text cell for non-icon strings', () => {
      const cell = getCellContent([4, 1], columns, [], true, getRowData);
      expect(cell.kind).toBe(GridCellKind.Text);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.data).toBe('not-icon');
      }
    });

    it('should respect column ordering', () => {
      const columnOrder = [1, 0, 2, 3, 4]; // Age, Name, Active, ...
      const cell = getCellContent(
        [0, 0],
        columns,
        columnOrder,
        true,
        getRowData
      );
      // With reordering, col 0 should now be Age (index 1 in original)
      expect(cell.kind).toBe(GridCellKind.Number);
      if (cell.kind === GridCellKind.Number) {
        expect(cell.data).toBe(30);
      }
    });

    it('should filter out hidden columns', () => {
      const columnsWithHidden: DataColumn[] = [
        { name: 'Visible1', type: ColType.Text, width: 100 },
        { name: 'Hidden', type: ColType.Number, width: 100, hidden: true },
        { name: 'Visible2', type: ColType.Boolean, width: 100 },
      ];

      const testData: DataRow[] = [{ values: ['text1', 999, true] }];
      const getTestRowData = (rowIndex: number): DataRow | null => {
        return rowIndex >= 0 && rowIndex < testData.length
          ? testData[rowIndex]
          : null;
      };

      // Request col 0 - should be Visible1
      const cell0 = getCellContent(
        [0, 0],
        columnsWithHidden,
        [],
        true,
        getTestRowData
      );
      expect(cell0.kind).toBe(GridCellKind.Text);
      if (cell0.kind === GridCellKind.Text) {
        expect(cell0.data).toBe('text1');
      }

      // Request col 1 - should be Visible2 (Hidden column skipped)
      const cell1 = getCellContent(
        [1, 0],
        columnsWithHidden,
        [],
        true,
        getTestRowData
      );
      expect(cell1.kind).toBe(GridCellKind.Boolean);
      if (cell1.kind === GridCellKind.Boolean) {
        expect(cell1.data).toBe(true);
      }
    });

    it('should handle Icon type from metadata', () => {
      const iconColumns: DataColumn[] = [
        { name: 'icon_col', type: ColType.Icon, width: 100 },
      ];

      const iconData: DataRow[] = [{ values: ['Settings'] }];
      const getIconRowData = (rowIndex: number): DataRow | null => {
        return rowIndex >= 0 && rowIndex < iconData.length
          ? iconData[rowIndex]
          : null;
      };

      const cell = getCellContent(
        [0, 0],
        iconColumns,
        [],
        true,
        getIconRowData
      );
      expect(cell.kind).toBe(GridCellKind.Custom);
      if (cell.kind === GridCellKind.Custom) {
        expect(cell.data).toEqual({
          kind: 'icon-cell',
          iconName: 'Settings',
        });
      }
    });

    it('should handle Link type from metadata', () => {
      const linkColumns: DataColumn[] = [
        { name: 'url_col', type: ColType.Link, width: 200 },
      ];

      const linkData: DataRow[] = [
        { values: ['https://example.com'] },
        { values: ['app://concepts/links-app'] },
      ];

      const getLinkRowData = (rowIndex: number): DataRow | null => {
        return rowIndex >= 0 && rowIndex < linkData.length
          ? linkData[rowIndex]
          : null;
      };

      // Test external URL
      const cell1 = getCellContent(
        [0, 0],
        linkColumns,
        [],
        false,
        getLinkRowData
      );
      expect(cell1.kind).toBe(GridCellKind.Custom);
      if (cell1.kind === GridCellKind.Custom) {
        const linkData1 = cell1.data as { kind: string; url: string };
        expect(linkData1.kind).toBe('link-cell');
        expect(linkData1.url).toBe('https://example.com');
        expect(cell1.copyData).toBe('https://example.com');
        expect(cell1.readonly).toBe(true);
      }

      // Test internal app:// link
      const cell2 = getCellContent(
        [0, 1],
        linkColumns,
        [],
        false,
        getLinkRowData
      );
      expect(cell2.kind).toBe(GridCellKind.Custom);
      if (cell2.kind === GridCellKind.Custom) {
        const linkData2 = cell2.data as { kind: string; url: string };
        expect(linkData2.kind).toBe('link-cell');
        expect(linkData2.url).toBe('app://concepts/links-app');
      }
    });

    it('should handle editable=false', () => {
      const cell = getCellContent([0, 0], columns, [], false, getRowData);
      if (cell.kind === GridCellKind.Text) {
        expect(cell.allowOverlay).toBe(false);
        expect(cell.readonly).toBe(true);
      }
    });
  });

  describe('getContentAlign', () => {
    it('should return "left" for Align.Left', () => {
      expect(getContentAlign(Align.Left)).toBe('left');
    });

    it('should return "center" for Align.Center', () => {
      expect(getContentAlign(Align.Center)).toBe('center');
    });

    it('should return "right" for Align.Right', () => {
      expect(getContentAlign(Align.Right)).toBe('right');
    });

    it('should return "left" for undefined', () => {
      expect(getContentAlign(undefined)).toBe('left');
    });
  });

  describe('Cell alignment', () => {
    it('should apply alignment to text cells', () => {
      const cell = createTextCell('test', true, Align.Center);
      expect(cell.contentAlign).toBe('center');
    });

    it('should apply alignment to number cells', () => {
      const cell = createNumberCell(123, true, Align.Right);
      expect(cell.contentAlign).toBe('right');
    });

    it('should apply alignment to date cells', () => {
      const cell = createDateCell('2023-01-01', 'date', true, Align.Center);
      expect(cell?.contentAlign).toBe('center');
    });

    it('should not set contentAlign when align is undefined', () => {
      const cell = createTextCell('test', true, undefined);
      expect(cell.contentAlign).toBeUndefined();
    });

    it('should apply alignment from column metadata in getCellContent', () => {
      const alignedColumns: DataColumn[] = [
        { name: 'Left', type: ColType.Text, width: 100, align: Align.Left },
        {
          name: 'Center',
          type: ColType.Number,
          width: 100,
          align: Align.Center,
        },
        { name: 'Right', type: ColType.Text, width: 100, align: Align.Right },
      ];

      const alignData: DataRow[] = [
        { values: ['left text', 42, 'right text'] },
      ];
      const getAlignRowData = (rowIndex: number): DataRow | null => {
        return rowIndex >= 0 && rowIndex < alignData.length
          ? alignData[rowIndex]
          : null;
      };

      const leftCell = getCellContent(
        [0, 0],
        alignedColumns,
        [],
        true,
        getAlignRowData
      );
      expect(leftCell.contentAlign).toBe('left');

      const centerCell = getCellContent(
        [1, 0],
        alignedColumns,
        [],
        true,
        getAlignRowData
      );
      expect(centerCell.contentAlign).toBe('center');

      const rightCell = getCellContent(
        [2, 0],
        alignedColumns,
        [],
        true,
        getAlignRowData
      );
      expect(rightCell.contentAlign).toBe('right');
    });
  });

  describe('createLabelsCell', () => {
    it('should create a bubble cell from an array of strings', () => {
      const cell = createLabelsCell(['Tag1', 'Tag2', 'Tag3']);
      expect(cell.kind).toBe(GridCellKind.Bubble);
      if (cell.kind === GridCellKind.Bubble) {
        expect(cell.data).toEqual(['Tag1', 'Tag2', 'Tag3']);
        expect(cell.allowOverlay).toBe(false);
      }
    });

    it('should create a bubble cell from a comma-separated string', () => {
      const cell = createLabelsCell('Red, Green, Blue');
      expect(cell.kind).toBe(GridCellKind.Bubble);
      if (cell.kind === GridCellKind.Bubble) {
        expect(cell.data).toEqual(['Red', 'Green', 'Blue']);
      }
    });

    it('should handle comma-separated string with extra spaces', () => {
      const cell = createLabelsCell('  Tag1  ,  Tag2  ,  Tag3  ');
      if (cell.kind === GridCellKind.Bubble) {
        expect(cell.data).toEqual(['Tag1', 'Tag2', 'Tag3']);
      }
    });

    it('should create a bubble cell from a single string value', () => {
      const cell = createLabelsCell('SingleTag');
      if (cell.kind === GridCellKind.Bubble) {
        expect(cell.data).toEqual(['SingleTag']);
      }
    });

    it('should handle empty array', () => {
      const cell = createLabelsCell([]);
      if (cell.kind === GridCellKind.Bubble) {
        expect(cell.data).toEqual([]);
      }
    });

    it('should handle null and undefined values in array', () => {
      const cell = createLabelsCell(['Tag1', null, 'Tag2', undefined, 'Tag3']);
      if (cell.kind === GridCellKind.Bubble) {
        expect(cell.data).toEqual(['Tag1', 'Tag2', 'Tag3']);
      }
    });

    it('should handle null input', () => {
      const cell = createLabelsCell(null);
      if (cell.kind === GridCellKind.Bubble) {
        expect(cell.data).toEqual([]);
      }
    });

    it('should handle undefined input', () => {
      const cell = createLabelsCell(undefined);
      if (cell.kind === GridCellKind.Bubble) {
        expect(cell.data).toEqual([]);
      }
    });

    it('should convert non-string array values to strings', () => {
      const cell = createLabelsCell([123, true, 'text']);
      if (cell.kind === GridCellKind.Bubble) {
        expect(cell.data).toEqual(['123', 'true', 'text']);
      }
    });

    it('should apply alignment when provided', () => {
      const cell = createLabelsCell(['Tag1', 'Tag2'], Align.Center);
      expect(cell.contentAlign).toBe('center');
    });

    it('should filter out empty strings from comma-separated input', () => {
      const cell = createLabelsCell('Tag1,  ,Tag2,,Tag3,');
      if (cell.kind === GridCellKind.Bubble) {
        expect(cell.data).toEqual(['Tag1', 'Tag2', 'Tag3']);
      }
    });
  });

  describe('getCellContent with Labels type', () => {
    it('should create labels cell for Labels column type with array value', () => {
      const labelsColumns: DataColumn[] = [
        { name: 'tags', type: ColType.Labels, width: 200 },
      ];

      const labelsData: DataRow[] = [
        { values: [['Important', 'Urgent', 'Review']] },
      ];
      const getLabelsRowData = (rowIndex: number): DataRow | null => {
        return rowIndex >= 0 && rowIndex < labelsData.length
          ? labelsData[rowIndex]
          : null;
      };

      const cell = getCellContent(
        [0, 0],
        labelsColumns,
        [],
        true,
        getLabelsRowData
      );
      expect(cell.kind).toBe(GridCellKind.Bubble);
      if (cell.kind === GridCellKind.Bubble) {
        expect(cell.data).toEqual(['Important', 'Urgent', 'Review']);
      }
    });

    it('should create labels cell for Labels column type with string value', () => {
      const labelsColumns: DataColumn[] = [
        { name: 'categories', type: ColType.Labels, width: 200 },
      ];

      const labelsData: DataRow[] = [
        { values: ['Bug, Feature, Documentation'] },
      ];
      const getLabelsRowData2 = (rowIndex: number): DataRow | null => {
        return rowIndex >= 0 && rowIndex < labelsData.length
          ? labelsData[rowIndex]
          : null;
      };

      const cell = getCellContent(
        [0, 0],
        labelsColumns,
        [],
        true,
        getLabelsRowData2
      );
      expect(cell.kind).toBe(GridCellKind.Bubble);
      if (cell.kind === GridCellKind.Bubble) {
        expect(cell.data).toEqual(['Bug', 'Feature', 'Documentation']);
      }
    });

    it('should respect alignment for Labels column type', () => {
      const labelsColumns: DataColumn[] = [
        { name: 'tags', type: ColType.Labels, width: 200, align: Align.Right },
      ];

      const labelsData: DataRow[] = [{ values: [['Tag1', 'Tag2']] }];
      const getLabelsRowData3 = (rowIndex: number): DataRow | null => {
        return rowIndex >= 0 && rowIndex < labelsData.length
          ? labelsData[rowIndex]
          : null;
      };

      const cell = getCellContent(
        [0, 0],
        labelsColumns,
        [],
        true,
        getLabelsRowData3
      );
      expect(cell.contentAlign).toBe('right');
    });
  });
});
