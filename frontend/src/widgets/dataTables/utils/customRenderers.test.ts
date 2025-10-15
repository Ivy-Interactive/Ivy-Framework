import { describe, it, expect, vi, beforeAll } from 'vitest';
import { GridCellKind } from '@glideapps/glide-data-grid';
import { iconCellRenderer, IconCell } from './customRenderers';
import * as iconRendererModule from './iconRenderer';

// Mock HTMLImageElement for Node environment
class MockImage {
  src = '';
  complete = true;
  onload: (() => void) | null = null;
  onerror: (() => void) | null = null;
}

beforeAll(() => {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  global.Image = MockImage as any;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  global.HTMLImageElement = MockImage as any;
});

describe('customRenderers', () => {
  describe('iconCellRenderer', () => {
    describe('isMatch', () => {
      it('should match icon cells', () => {
        const iconCell: IconCell = {
          kind: GridCellKind.Custom,
          allowOverlay: false,
          copyData: 'Activity',
          data: {
            kind: 'icon-cell',
            iconName: 'Activity',
          },
        };

        expect(iconCellRenderer.isMatch(iconCell)).toBe(true);
      });

      it('should not match non-custom cells', () => {
        const textCell = {
          kind: GridCellKind.Text,
          data: 'test',
          displayData: 'test',
          allowOverlay: false,
          copyData: 'test',
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
        } as any;

        expect(iconCellRenderer.isMatch(textCell)).toBe(false);
      });

      it('should not match custom cells with wrong kind', () => {
        const customCell = {
          kind: GridCellKind.Custom,
          allowOverlay: false,
          copyData: 'test',
          data: {
            kind: 'other-cell',
            value: 'test',
          },
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
        } as any;

        expect(iconCellRenderer.isMatch(customCell)).toBe(false);
      });
    });

    describe('draw', () => {
      const mockCtx = {
        fillStyle: '',
        font: '',
        fillText: vi.fn(),
        drawImage: vi.fn(),
        beginPath: vi.fn(),
        arc: vi.fn(),
        fill: vi.fn(),
      };

      const mockTheme = {
        textDark: '#000',
        textMedium: '#666',
      };

      const mockRect = {
        x: 0,
        y: 0,
        width: 100,
        height: 40,
      };

      const mockArgs = {
        ctx: mockCtx,
        theme: mockTheme,
        rect: mockRect,
        col: 0,
        row: 0,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any;

      beforeAll(() => {
        vi.clearAllMocks();
      });

      it('should return false when iconName is missing', () => {
        const cell: IconCell = {
          kind: GridCellKind.Custom,
          allowOverlay: false,
          copyData: '',
          data: {
            kind: 'icon-cell',
            iconName: '',
          },
        };

        const result = iconCellRenderer.draw(mockArgs, cell);
        expect(result).toBe(false);
      });

      it('should draw error indicator for invalid icon', () => {
        vi.spyOn(iconRendererModule, 'isValidIconName').mockReturnValue(false);

        const cell: IconCell = {
          kind: GridCellKind.Custom,
          allowOverlay: false,
          copyData: 'InvalidIcon',
          data: {
            kind: 'icon-cell',
            iconName: 'InvalidIcon',
          },
        };

        const result = iconCellRenderer.draw(mockArgs, cell);

        expect(result).toBe(true);
        // Default alignment is left, so x = rect.x + 16
        expect(mockCtx.fillText).toHaveBeenCalledWith('?', 16, 24);
        expect(mockCtx.fillStyle).toBe('#000');
      });

      it('should draw icon when image is complete', () => {
        vi.spyOn(iconRendererModule, 'isValidIconName').mockReturnValue(true);

        const mockImage = new MockImage();
        mockImage.complete = true;
        vi.spyOn(iconRendererModule, 'getIconImage').mockReturnValue(
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          mockImage as any
        );

        const cell: IconCell = {
          kind: GridCellKind.Custom,
          allowOverlay: false,
          copyData: 'Activity',
          data: {
            kind: 'icon-cell',
            iconName: 'Activity',
          },
        };

        const result = iconCellRenderer.draw(mockArgs, cell);

        expect(result).toBe(true);
        expect(mockCtx.drawImage).toHaveBeenCalledWith(
          mockImage,
          16, // padding (left-aligned)
          10, // (40 - 20) / 2 (vertically centered)
          20,
          20
        );
      });

      it('should draw placeholder when image is not complete', () => {
        vi.spyOn(iconRendererModule, 'isValidIconName').mockReturnValue(true);

        const mockImage = new MockImage();
        mockImage.complete = false;
        vi.spyOn(iconRendererModule, 'getIconImage').mockReturnValue(
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          mockImage as any
        );

        const cell: IconCell = {
          kind: GridCellKind.Custom,
          allowOverlay: false,
          copyData: 'Activity',
          data: {
            kind: 'icon-cell',
            iconName: 'Activity',
          },
        };

        const result = iconCellRenderer.draw(mockArgs, cell);

        expect(result).toBe(true);
        expect(mockCtx.beginPath).toHaveBeenCalled();
        // Icon is now left-aligned with padding of 16 + 10 = 26
        expect(mockCtx.arc).toHaveBeenCalledWith(26, 20, 4, 0, 2 * Math.PI);
        expect(mockCtx.fill).toHaveBeenCalled();
        expect(mockCtx.fillStyle).toBe('#666');
      });

      it('should draw placeholder when getIconImage returns null', () => {
        vi.spyOn(iconRendererModule, 'isValidIconName').mockReturnValue(true);
        vi.spyOn(iconRendererModule, 'getIconImage').mockReturnValue(null);

        const cell: IconCell = {
          kind: GridCellKind.Custom,
          allowOverlay: false,
          copyData: 'Activity',
          data: {
            kind: 'icon-cell',
            iconName: 'Activity',
          },
        };

        const result = iconCellRenderer.draw(mockArgs, cell);

        expect(result).toBe(true);
        expect(mockCtx.beginPath).toHaveBeenCalled();
      });
    });

    describe('onPaste', () => {
      it('should return updated data for valid icon name', () => {
        vi.spyOn(iconRendererModule, 'isValidIconName').mockReturnValue(true);

        const data = {
          kind: 'icon-cell' as const,
          iconName: 'Activity',
        };

        const result = iconCellRenderer.onPaste?.('Archive', data);

        expect(result).toEqual({
          kind: 'icon-cell',
          iconName: 'Archive',
        });
      });

      it('should return undefined for invalid icon name', () => {
        vi.spyOn(iconRendererModule, 'isValidIconName').mockReturnValue(false);

        const data = {
          kind: 'icon-cell' as const,
          iconName: 'Activity',
        };

        const result = iconCellRenderer.onPaste?.('InvalidIcon', data);

        expect(result).toBeUndefined();
      });

      it('should return undefined for non-string values', () => {
        const data = {
          kind: 'icon-cell' as const,
          iconName: 'Activity',
        };

        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const result = iconCellRenderer.onPaste?.(123 as any, data);

        expect(result).toBeUndefined();
      });
    });

    describe('renderer properties', () => {
      it('should have correct kind', () => {
        expect(iconCellRenderer.kind).toBe(GridCellKind.Custom);
      });

      it('should have isMatch function', () => {
        expect(typeof iconCellRenderer.isMatch).toBe('function');
      });

      it('should have draw function', () => {
        expect(typeof iconCellRenderer.draw).toBe('function');
      });

      it('should have onPaste function', () => {
        expect(typeof iconCellRenderer.onPaste).toBe('function');
      });
    });
  });
});
