import {
  CustomRenderer,
  GridCellKind,
  CustomCell,
} from '@glideapps/glide-data-grid';
import { getIconImage, isValidIconName } from './iconRenderer';

/**
 * Data structure for icon cells
 */
export interface IconCellData {
  kind: 'icon-cell';
  iconName: string;
  align?: 'left' | 'center' | 'right';
}

/**
 * Type definition for icon custom cells
 */
export type IconCell = CustomCell<IconCellData>;

/**
 * Custom cell renderer for displaying Lucide icons in table cells
 */
export const iconCellRenderer: CustomRenderer<IconCell> = {
  kind: GridCellKind.Custom,

  isMatch: (cell: CustomCell): cell is IconCell =>
    cell.kind === GridCellKind.Custom &&
    (cell.data as IconCellData | undefined)?.kind === 'icon-cell',

  draw: (args, cell) => {
    const { ctx, rect, theme } = args;
    const iconName = cell.data?.iconName;
    const align = cell.data?.align || 'left';

    if (!iconName) return false;

    // Validate icon exists
    if (!isValidIconName(iconName)) {
      // Draw error indicator for invalid icon
      ctx.fillStyle = theme.textDark;
      ctx.font = '12px sans-serif';
      const errorX =
        align === 'center'
          ? rect.x + rect.width / 2 - 4
          : align === 'right'
            ? rect.x + rect.width - 20
            : rect.x + 16;
      ctx.fillText('?', errorX, rect.y + rect.height / 2 + 4);
      return true;
    }

    // Get icon image (cached or newly created)
    const iconImage = getIconImage(iconName, {
      size: 20,
      color: theme.textDark,
      strokeWidth: 2,
    });

    if (iconImage && iconImage.complete) {
      // Draw the icon with specified alignment
      const iconSize = 20;
      const padding = 16;
      let x: number;

      switch (align) {
        case 'center':
          x = rect.x + (rect.width - iconSize) / 2;
          break;
        case 'right':
          x = rect.x + rect.width - iconSize - padding;
          break;
        case 'left':
        default:
          x = rect.x + padding;
      }

      const y = rect.y + (rect.height - iconSize) / 2;
      ctx.drawImage(iconImage, x, y, iconSize, iconSize);
      return true;
    }

    // If image is not complete, draw placeholder with specified alignment
    const padding = 16;
    let centerX: number;

    switch (align) {
      case 'center':
        centerX = rect.x + rect.width / 2;
        break;
      case 'right':
        centerX = rect.x + rect.width - padding - 10;
        break;
      case 'left':
      default:
        centerX = rect.x + padding + 10;
    }

    ctx.fillStyle = theme.textMedium;
    ctx.beginPath();
    ctx.arc(centerX, rect.y + rect.height / 2, 4, 0, 2 * Math.PI);
    ctx.fill();

    return true;
  },

  // Support pasting icon names
  onPaste: (value: string, data: IconCellData) => {
    if (typeof value === 'string' && isValidIconName(value)) {
      return {
        ...data,
        iconName: value,
      };
    }
    return undefined;
  },
};
