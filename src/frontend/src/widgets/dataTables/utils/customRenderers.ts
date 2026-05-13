import { CustomRenderer, GridCellKind, CustomCell, type Theme } from "@glideapps/glide-data-grid";
import { icons } from "lucide-react";
import { createElement } from "react";
import { renderToStaticMarkup } from "react-dom/server";
import { LruMap } from "./lruMap";

const MAX_ICON_IMAGE_CACHE = 128;
const iconImageCache = new LruMap<string, HTMLImageElement>(MAX_ICON_IMAGE_CACHE);

function isValidIconName(name: string): boolean {
  return name in icons;
}

function getIconImage(
  iconName: string,
  options: { size?: number; color?: string } = {},
): HTMLImageElement | null {
  const { color = "#666" } = options;
  const cacheKey = `${iconName}-${color}`;
  const cached = iconImageCache.get(cacheKey);
  if (cached) return cached;

  const IconComponent = icons[iconName as keyof typeof icons];
  if (!IconComponent) return null;

  const svg = renderToStaticMarkup(
    createElement(IconComponent, {
      size: 24,
      color,
      strokeWidth: 2,
    }),
  );
  const img = new Image();
  img.src = `data:image/svg+xml;base64,${btoa(svg)}`;
  iconImageCache.set(cacheKey, img);
  return img;
}

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
  text?: string; // Optional display text (falls back to url if missing)
  align?: "left" | "center" | "right";
  linkType?: "url" | "email" | "phone"; // For frontend handling
}

/**
 * Type definition for link custom cells
 */
export type LinkCell = CustomCell<LinkCellData>;

type GridDrawTheme = Theme & { baseFontFull: string };

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

export const labelsBadgesCellRenderer: CustomRenderer<LabelsBadgesCell> = {
  kind: GridCellKind.Custom,
  isMatch: (cell: CustomCell): cell is LabelsBadgesCell =>
    cell.kind === GridCellKind.Custom &&
    (cell.data as LabelsBadgesCellData | undefined)?.kind === "labels-badges-cell",
  measure: (ctx, cell, theme) =>
    measureLabelsBadgesWidth(ctx, cell.data.items, theme as GridDrawTheme),
  draw: (args, cell) => {
    const { ctx, rect, theme } = args;
    const { items, align = "left" } = cell.data;
    if (items.length === 0) return true;

    const { x, y, width: w, height: h } = rect;
    ctx.font = (theme as GridDrawTheme).baseFontFull;
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
    if (align === "center") renderX = x + (w - rowWidth) / 2;
    else if (align === "right") renderX = x + w - rowWidth - hPad;

    for (const item of items) {
      if (renderX > x + w) break;
      const textW = ctx.measureText(item.text).width;
      const boxW = textW + pad * 2;
      const bg = item.bg ?? theme.bgBubble;
      const fg = item.fg ?? theme.textBubble;
      const bx = renderX;
      const by = y + (h - bubbleH) / 2;

      ctx.fillStyle = bg;
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
 * Data structure for animated-status cells.
 * Mirrors the look of the `AnimatedStatusLabel` external widget: a spinner + shimmering
 * status text while running, and a check icon + static text when complete.
 */
export interface AnimatedStatusCellData {
  kind: "animated-status-cell";
  state: "running" | "done" | "idle";
  statusText: string;
  rightLabel?: string;
  align?: "left" | "center" | "right";
}

export type AnimatedStatusCell = CustomCell<AnimatedStatusCellData>;

const SPINNER_RADIUS = 6;
const ICON_TEXT_GAP = 6;
const SHIMMER_PERIOD_MS = 1400;
const SPINNER_PERIOD_MS = 900;

function drawSpinner(
  ctx: CanvasRenderingContext2D,
  cx: number,
  cy: number,
  color: string,
  t: number,
) {
  const start = (t / SPINNER_PERIOD_MS) * Math.PI * 2;
  ctx.save();
  ctx.strokeStyle = color;
  ctx.lineWidth = 1.5;
  ctx.lineCap = "round";
  ctx.beginPath();
  ctx.arc(cx, cy, SPINNER_RADIUS, start, start + Math.PI * 1.4);
  ctx.stroke();
  ctx.restore();
}

function drawCheck(ctx: CanvasRenderingContext2D, cx: number, cy: number, color: string) {
  ctx.save();
  ctx.strokeStyle = color;
  ctx.lineWidth = 1.5;
  ctx.lineCap = "round";
  ctx.lineJoin = "round";
  ctx.beginPath();
  ctx.arc(cx, cy, SPINNER_RADIUS, 0, Math.PI * 2);
  ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(cx - 3, cy);
  ctx.lineTo(cx - 1, cy + 2.5);
  ctx.lineTo(cx + 3, cy - 2);
  ctx.stroke();
  ctx.restore();
}

function drawShimmerText(
  ctx: CanvasRenderingContext2D,
  text: string,
  x: number,
  y: number,
  baseColor: string,
  highlightColor: string,
  t: number,
) {
  const width = ctx.measureText(text).width;
  if (width <= 0) return;
  const phase = (t % SHIMMER_PERIOD_MS) / SHIMMER_PERIOD_MS;
  // Sweep the highlight from -0.3 to 1.3 (so it enters/leaves the text smoothly).
  const center = phase * 1.6 - 0.3;
  const gradient = ctx.createLinearGradient(x, 0, x + width, 0);
  const clamp = (v: number) => Math.max(0, Math.min(1, v));
  const left = clamp(center - 0.25);
  const right = clamp(center + 0.25);
  gradient.addColorStop(0, baseColor);
  gradient.addColorStop(left, baseColor);
  gradient.addColorStop(clamp(center), highlightColor);
  gradient.addColorStop(right, baseColor);
  gradient.addColorStop(1, baseColor);
  ctx.save();
  ctx.fillStyle = gradient;
  ctx.fillText(text, x, y);
  ctx.restore();
}

export const animatedStatusCellRenderer: CustomRenderer<AnimatedStatusCell> = {
  kind: GridCellKind.Custom,
  isMatch: (cell: CustomCell): cell is AnimatedStatusCell =>
    cell.kind === GridCellKind.Custom &&
    (cell.data as AnimatedStatusCellData | undefined)?.kind === "animated-status-cell",
  draw: (args, cell) => {
    const { ctx, rect, theme } = args;
    const { state, statusText, rightLabel, align = "left" } = cell.data;
    const t = (args as unknown as { frameTime?: number }).frameTime ?? performance.now();
    const hPad = theme.cellHorizontalPadding ?? 8;

    ctx.save();
    ctx.font = (theme as GridDrawTheme).baseFontFull;
    ctx.textBaseline = "middle";

    const baseColor = theme.textDark;
    const dimColor = theme.textMedium;
    const accentColor = theme.accentColor ?? baseColor;
    const cy = rect.y + rect.height / 2;

    const showIcon = state !== "idle";
    let cursorX = rect.x + hPad;
    if (showIcon) {
      const ix = cursorX + SPINNER_RADIUS;
      if (state === "running") {
        drawSpinner(ctx, ix, cy, accentColor, t);
      } else {
        drawCheck(ctx, ix, cy, accentColor);
      }
      cursorX += SPINNER_RADIUS * 2 + ICON_TEXT_GAP;
    }

    // Reserve room for right label on the right edge.
    let rightX = rect.x + rect.width - hPad;
    if (rightLabel) {
      const rw = ctx.measureText(rightLabel).width;
      ctx.fillStyle = dimColor;
      ctx.fillText(rightLabel, rightX - rw, cy);
      rightX -= rw + ICON_TEXT_GAP;
    }

    let textX = cursorX;
    if (align === "center") {
      const textW = ctx.measureText(statusText).width;
      textX = Math.max(cursorX, rect.x + (rect.width - textW) / 2);
    } else if (align === "right") {
      const textW = ctx.measureText(statusText).width;
      textX = Math.max(cursorX, rightX - textW);
    }

    if (state === "running") {
      drawShimmerText(ctx, statusText, textX, cy, dimColor, baseColor, t);
      args.requestAnimationFrame();
    } else {
      ctx.fillStyle = state === "done" ? baseColor : dimColor;
      ctx.fillText(statusText, textX, cy);
    }

    ctx.restore();
    return true;
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
    const text = cell.data?.text || url; // Use text if provided, fallback to URL
    const align = cell.data?.align || "left";

    if (!url || !text) return false;

    // Use linkColor from theme (should be blue)
    const linkColor = theme.linkColor || theme.accentColor || "#2563eb";
    const padding = theme.cellHorizontalPadding ?? 8;

    ctx.save();
    ctx.font = `${theme.baseFontStyle} ${theme.fontFamily}`;
    ctx.fillStyle = linkColor;
    ctx.textBaseline = "middle";

    // Calculate text position based on alignment
    const textMetrics = ctx.measureText(text); // Measure text, not URL
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
    ctx.fillText(text, textX, textY); // Draw text, not URL

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
