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

    if (!iconName) return false;

    // Validate icon exists
    if (!isValidIconName(iconName)) {
      // Draw error indicator for invalid icon
      ctx.fillStyle = theme.textDark;
      ctx.font = '12px sans-serif';
      ctx.fillText(
        '?',
        rect.x + rect.width / 2 - 4,
        rect.y + rect.height / 2 + 4
      );
      return true;
    }

    // Get icon image (cached or newly created)
    const iconImage = getIconImage(iconName, {
      size: 20,
      color: theme.textDark,
      strokeWidth: 2,
    });

    if (iconImage && iconImage.complete) {
      // Draw the icon centered in the cell
      const iconSize = 20;
      const x = rect.x + (rect.width - iconSize) / 2;
      const y = rect.y + (rect.height - iconSize) / 2;
      ctx.drawImage(iconImage, x, y, iconSize, iconSize);
      return true;
    }

    // If image is not complete, draw placeholder
    ctx.fillStyle = theme.textMedium;
    ctx.beginPath();
    ctx.arc(
      rect.x + rect.width / 2,
      rect.y + rect.height / 2,
      4,
      0,
      2 * Math.PI
    );
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
