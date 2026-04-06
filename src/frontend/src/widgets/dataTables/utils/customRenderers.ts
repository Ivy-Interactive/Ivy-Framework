import { CustomRenderer, GridCellKind, CustomCell, type Theme } from "@glideapps/glide-data-grid";
import { getIconImage, isValidIconName } from "./iconRenderer";

/** Glide passes a realized theme with computed `baseFontFull` to measure/draw. */
type GridDrawTheme = Theme & { baseFontFull: string };

/**
 * Data structure for icon cells
 */
export interface IconCellData {
  kind: "icon-cell";
  iconName: string;
  align?: "left" | "center" | "right";
}

/**
 * Type definition for icon custom cells
 */
export type IconCell = CustomCell<IconCellData>;

/**
 * Data structure for link cells
 */
export interface LinkCellData {
  kind: "link-cell";
  url: string;
  align?: "left" | "center" | "right";
}

/**
 * Type definition for link custom cells
 */
export type LinkCell = CustomCell<LinkCellData>;

/**
 * Multi-badge cell: per-label background/text (Bubble cells only support one theme per cell).
 */
export interface LabelsBadgesCellData {
  kind: "labels-badges-cell";
  items: { text: string; bg?: string; fg?: string }[];
  align?: "left" | "center" | "right";
}

export type LabelsBadgesCell = CustomCell<LabelsBadgesCellData>;

function measureLabelsBadgesWidth(
  ctx: CanvasRenderingContext2D,
  items: LabelsBadgesCellData["items"],
  theme: GridDrawTheme,
): number {
  if (items.length === 0) return theme.cellHorizontalPadding * 2;
  ctx.font = theme.baseFontFull;
  let w = theme.cellHorizontalPadding * 2 - theme.bubbleMargin;
  for (const item of items) {
    w += ctx.measureText(item.text).width + theme.bubblePadding * 2 + theme.bubbleMargin;
  }
  return w;
}

/**
 * Custom cell renderer for label columns with per-badge colors (badge color mapping).
 */
export const labelsBadgesCellRenderer: CustomRenderer<LabelsBadgesCell> = {
  kind: GridCellKind.Custom,

  isMatch: (cell: CustomCell): cell is LabelsBadgesCell =>
    cell.kind === GridCellKind.Custom &&
    (cell.data as LabelsBadgesCellData | undefined)?.kind === "labels-badges-cell",

  measure: (ctx, cell, theme) => measureLabelsBadgesWidth(ctx, cell.data.items, theme),

  draw: (args, cell) => {
    const { ctx, rect, theme, highlighted } = args;
    const { items, align = "left" } = cell.data;
    if (items.length === 0) return true;

    const { x, y, width: w, height: h } = rect;
    ctx.font = theme.baseFontFull;
    ctx.textBaseline = "middle";

    const bubbleH = theme.bubbleHeight;
    const pad = theme.bubblePadding;
    const margin = theme.bubbleMargin;
    const radius = theme.roundingRadius ?? bubbleH / 2;
    const hPad = theme.cellHorizontalPadding;

    let rowWidth = -margin;
    for (const item of items) {
      rowWidth += ctx.measureText(item.text).width + pad * 2 + margin;
    }

    let renderX = x + hPad;
    if (align === "center") {
      renderX = x + (w - rowWidth) / 2;
    } else if (align === "right") {
      renderX = x + w - rowWidth - hPad;
    }

    for (const item of items) {
      if (renderX > x + w) break;
      const textW = ctx.measureText(item.text).width;
      const boxW = textW + pad * 2;
      const bg = item.bg ?? (highlighted ? theme.bgBubbleSelected : theme.bgBubble);
      const fg = item.fg ?? theme.textBubble;

      ctx.fillStyle = bg;
      const bx = renderX;
      const by = y + (h - bubbleH) / 2;
      ctx.beginPath();
      if (typeof ctx.roundRect === "function") {
        ctx.roundRect(bx, by, boxW, bubbleH, radius);
      } else {
        const r = Math.min(radius, bubbleH / 2, boxW / 2);
        ctx.moveTo(bx + r, by);
        ctx.arcTo(bx + boxW, by, bx + boxW, by + bubbleH, r);
        ctx.arcTo(bx + boxW, by + bubbleH, bx, by + bubbleH, r);
        ctx.arcTo(bx, by + bubbleH, bx, by, r);
        ctx.arcTo(bx, by, bx + boxW, by, r);
        ctx.closePath();
      }
      ctx.fill();

      ctx.fillStyle = fg;
      ctx.fillText(item.text, bx + pad, y + h / 2);

      renderX += boxW + margin;
    }

    return true;
  },
};

/**
 * Custom cell renderer for displaying Lucide icons in table cells
 */
export const iconCellRenderer: CustomRenderer<IconCell> = {
  kind: GridCellKind.Custom,

  isMatch: (cell: CustomCell): cell is IconCell =>
    cell.kind === GridCellKind.Custom &&
    (cell.data as IconCellData | undefined)?.kind === "icon-cell",

  draw: (args, cell) => {
    const { ctx, rect, theme } = args;
    const iconName = cell.data?.iconName;
    const align = cell.data?.align || "left";

    if (!iconName) return false;

    // Validate icon exists
    if (!isValidIconName(iconName)) {
      // Draw error indicator for invalid icon
      ctx.fillStyle = theme.textDark;
      ctx.font = "12px sans-serif";
      const errorX =
        align === "center"
          ? rect.x + rect.width / 2 - 4
          : align === "right"
            ? rect.x + rect.width - 20
            : rect.x + 16;
      ctx.fillText("?", errorX, rect.y + rect.height / 2 + 4);
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
        case "center":
          x = rect.x + (rect.width - iconSize) / 2;
          break;
        case "right":
          x = rect.x + rect.width - iconSize - padding;
          break;
        case "left":
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
      case "center":
        centerX = rect.x + rect.width / 2;
        break;
      case "right":
        centerX = rect.x + rect.width - padding - 10;
        break;
      case "left":
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
    if (typeof value === "string" && isValidIconName(value)) {
      return {
        ...data,
        iconName: value,
      };
    }
    return undefined;
  },
};

/**
 * Custom cell renderer for displaying links with underline in table cells
 */
export const linkCellRenderer: CustomRenderer<LinkCell> = {
  kind: GridCellKind.Custom,

  isMatch: (cell: CustomCell): cell is LinkCell =>
    cell.kind === GridCellKind.Custom &&
    (cell.data as LinkCellData | undefined)?.kind === "link-cell",

  draw: (args, cell) => {
    const { ctx, rect, theme } = args;
    const url = cell.data?.url;
    const align = cell.data?.align || "left";

    if (!url) return false;

    // Use linkColor from theme (should be blue)
    const linkColor = theme.linkColor || theme.accentColor || "#2563eb";
    const padding = theme.cellHorizontalPadding ?? 8;

    ctx.save();
    ctx.font = `${theme.baseFontStyle} ${theme.fontFamily}`;
    ctx.fillStyle = linkColor;
    ctx.textBaseline = "middle";

    // Calculate text position based on alignment
    const textMetrics = ctx.measureText(url);
    let textX: number;

    switch (align) {
      case "center":
        textX = rect.x + (rect.width - textMetrics.width) / 2;
        break;
      case "right":
        textX = rect.x + rect.width - textMetrics.width - padding;
        break;
      case "left":
      default:
        textX = rect.x + padding;
    }

    const textY = rect.y + rect.height / 2;

    // Draw the text
    ctx.fillText(url, textX, textY);

    // Draw underline
    const underlineY = textY + 8;
    ctx.strokeStyle = linkColor;
    ctx.lineWidth = 1;
    ctx.beginPath();
    ctx.moveTo(textX, underlineY);
    ctx.lineTo(textX + textMetrics.width, underlineY);
    ctx.stroke();

    ctx.restore();

    return true;
  },
};
