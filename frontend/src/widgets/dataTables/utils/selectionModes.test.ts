import { describe, it, expect } from 'vitest';
import { getSelectionProps } from './selectionModes';
import { SelectionModes } from '../types/types';

describe('selectionModes', () => {
  describe('getSelectionProps', () => {
    it('should map Cells mode to rect selection only', () => {
      const result = getSelectionProps(SelectionModes.Cells);
      expect(result).toEqual({
        rowSelect: 'none',
        columnSelect: 'none',
        rangeSelect: 'rect',
      });
    });

    it('should map Rows mode to multi-row selection only', () => {
      const result = getSelectionProps(SelectionModes.Rows);
      expect(result).toEqual({
        rowSelect: 'multi',
        columnSelect: 'none',
        rangeSelect: 'none',
      });
    });

    it('should map Columns mode to multi-column selection only', () => {
      const result = getSelectionProps(SelectionModes.Columns);
      expect(result).toEqual({
        rowSelect: 'none',
        columnSelect: 'multi',
        rangeSelect: 'none',
      });
    });

    it('should handle undefined selection mode with default', () => {
      const result = getSelectionProps(undefined);
      expect(result).toEqual({
        rowSelect: 'none',
        columnSelect: 'none',
        rangeSelect: 'rect',
      });
    });
  });
});
