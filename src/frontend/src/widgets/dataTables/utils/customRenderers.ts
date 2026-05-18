import { CustomRenderer, GridCellKind, CustomCell, type Theme } from "@glideapps/glide-data-grid";
import { icons } from "lucide-react";
import { createElement } from "react";
import { renderToStaticMarkup } from "react-dom/server";
import { drawTruncatedText, truncateTextWithEllipsis } from "./canvasText";
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
 *
 * Three visual modes:
 * - "label": spinner + shimmering text while running; plain text otherwise.
 * - "badge": rounded pill whose text shimmers while running; static pill otherwise.
 * - "spinner-timer": small spinner + plain text. No transition on value change.
 */
export interface AnimatedStatusCellData {
  kind: "animated-status-cell";
  mode: "label" | "badge" | "spinner-timer";
  state: "running" | "done" | "idle";
  statusText: string;
  rightLabel?: string;
  align?: "left" | "center" | "right";
  badgeBg?: string;
  badgeFg?: string;
}

export type AnimatedStatusCell = CustomCell<AnimatedStatusCellData>;

const SPINNER_RADIUS = 6;
const ICON_TEXT_GAP = 6;
// Slow, subtle shimmer — a single highlight pass takes ~2.4s and the highlight
// band is narrow so the text reads as gently breathing rather than scrolling.
const SHIMMER_PERIOD_MS = 2400;
const SPINNER_PERIOD_MS = 1400;

function drawRoundRect(
  ctx: CanvasRenderingContext2D,
  x: number,
  y: number,
  w: number,
  h: number,
  r: number,
) {
  const radius = Math.min(r, h / 2, w / 2);
  ctx.beginPath();
  if (typeof ctx.roundRect === "function") {
    ctx.roundRect(x, y, w, h, radius);
  } else {
    ctx.moveTo(x + radius, y);
    ctx.arcTo(x + w, y, x + w, y + h, radius);
    ctx.arcTo(x + w, y + h, x, y + h, radius);
    ctx.arcTo(x, y + h, x, y, radius);
    ctx.arcTo(x, y, x + w, y, radius);
    ctx.closePath();
  }
}

function drawSpinner(
  ctx: CanvasRenderingContext2D,
  cx: number,
  cy: number,
  color: string,
  t: number,
  radius: number = SPINNER_RADIUS,
) {
  const start = (t / SPINNER_PERIOD_MS) * Math.PI * 2;
  ctx.save();
  ctx.strokeStyle = color;
  ctx.lineWidth = 1.5;
  ctx.lineCap = "round";
  ctx.beginPath();
  ctx.arc(cx, cy, radius, start, start + Math.PI * 1.4);
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
  // Sweep the highlight from -0.2 to 1.2 (so it enters/leaves the text smoothly).
  const center = phase * 1.4 - 0.2;
  const gradient = ctx.createLinearGradient(x, 0, x + width, 0);
  const clamp = (v: number) => Math.max(0, Math.min(1, v));
  // Narrower band (~0.18 wide) for a subtler sweep.
  const left = clamp(center - 0.18);
  const right = clamp(center + 0.18);
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

type AnimatedDrawArgs = Parameters<CustomRenderer<AnimatedStatusCell>["draw"]>[0];

function drawLabelMode(args: AnimatedDrawArgs, cell: AnimatedStatusCell, t: number, hPad: number) {
  const { ctx, rect, theme } = args;
  const { state, statusText, rightLabel, align = "left" } = cell.data;

  const baseColor = theme.textDark;
  const dimColor = theme.textMedium;
  const accentColor = theme.accentColor ?? baseColor;
  const cy = rect.y + rect.height / 2;
  const font = (theme as GridDrawTheme).baseFontFull;

  let left = rect.x + hPad;
  if (state === "running") {
    drawSpinner(ctx, left + SPINNER_RADIUS, cy, accentColor, t);
    left += SPINNER_RADIUS * 2 + ICON_TEXT_GAP;
    args.requestAnimationFrame();
  }

  let right = rect.x + rect.width - hPad;
  if (rightLabel) {
    const rightRect = { x: rect.x, y: rect.y, width: rect.width, height: rect.height };
    const drawn = drawTruncatedText(ctx, {
      text: rightLabel,
      rect: rightRect,
      font,
      color: dimColor,
      align: "right",
      padding: hPad,
    });
    ctx.font = font;
    right -= ctx.measureText(drawn).width + ICON_TEXT_GAP;
  }

  const textRect = {
    x: left,
    y: rect.y,
    width: Math.max(0, right - left),
    height: rect.height,
  };
  if (textRect.width <= 0) return;

  if (state === "running") {
    ctx.save();
    ctx.beginPath();
    ctx.rect(textRect.x, textRect.y, textRect.width, textRect.height);
    ctx.clip();
    ctx.font = font;
    ctx.textBaseline = "middle";
    const truncated = truncateTextWithEllipsis(statusText, textRect.width, font);
    const textW = ctx.measureText(truncated).width;
    const textX =
      align === "center"
        ? textRect.x + (textRect.width - textW) / 2
        : align === "right"
          ? textRect.x + textRect.width - textW
          : textRect.x;
    drawShimmerText(ctx, truncated, textX, cy, dimColor, baseColor, t);
    ctx.restore();
  } else {
    drawTruncatedText(ctx, {
      text: statusText,
      rect: textRect,
      font,
      color: state === "done" ? baseColor : dimColor,
      align,
      padding: 0,
    });
  }
}

function drawBadgeMode(args: AnimatedDrawArgs, cell: AnimatedStatusCell, t: number, hPad: number) {
  const { ctx, rect, theme } = args;
  const { state, statusText, align = "left", badgeBg, badgeFg } = cell.data;

  const bg = badgeBg ?? theme.bgBubble;
  const fg = badgeFg ?? theme.textBubble;
  const bubbleH = theme.bubbleHeight;
  const pad = theme.bubblePadding;
  const radius = theme.roundingRadius ?? bubbleH / 2;
  const font = (theme as GridDrawTheme).baseFontFull;
  const maxBadgeTextWidth = Math.max(0, rect.width - hPad * 2 - pad * 2);
  const displayText = truncateTextWithEllipsis(statusText, maxBadgeTextWidth, font);
  const textW = ctx.measureText(displayText).width;
  const boxW = textW + pad * 2;

  let bx = rect.x + hPad;
  if (align === "center") bx = rect.x + (rect.width - boxW) / 2;
  else if (align === "right") bx = rect.x + rect.width - boxW - hPad;
  const by = rect.y + (rect.height - bubbleH) / 2;
  const cy = by + bubbleH / 2;

  ctx.save();
  ctx.fillStyle = bg;
  drawRoundRect(ctx, bx, by, boxW, bubbleH, radius);
  ctx.fill();
  ctx.restore();

  if (state === "running") {
    drawShimmerText(ctx, displayText, bx + pad, cy, fg, "rgba(255,255,255,0.55)", t);
    args.requestAnimationFrame();
  } else {
    ctx.fillStyle = fg;
    ctx.fillText(displayText, bx + pad, cy);
  }
}

function drawSpinnerTimerMode(
  args: AnimatedDrawArgs,
  cell: AnimatedStatusCell,
  t: number,
  hPad: number,
) {
  const { ctx, rect, theme } = args;
  const { state, statusText, align = "left" } = cell.data;

  const baseColor = theme.textDark;
  const dimColor = theme.textMedium;
  const accentColor = theme.accentColor ?? baseColor;
  const cy = rect.y + rect.height / 2;
  const r = SPINNER_RADIUS - 1;
  const font = (theme as GridDrawTheme).baseFontFull;

  let left = rect.x + hPad;
  if (state === "running") {
    drawSpinner(ctx, left + r, cy, accentColor, t, r);
    left += r * 2 + ICON_TEXT_GAP;
    args.requestAnimationFrame();
  }

  const textRect = {
    x: left,
    y: rect.y,
    width: Math.max(0, rect.x + rect.width - hPad - left),
    height: rect.height,
  };
  if (textRect.width <= 0) return;

  drawTruncatedText(ctx, {
    text: statusText,
    rect: textRect,
    font,
    color: state === "idle" ? dimColor : baseColor,
    align,
    padding: 0,
  });
}

export const animatedStatusCellRenderer: CustomRenderer<AnimatedStatusCell> = {
  kind: GridCellKind.Custom,
  isMatch: (cell: CustomCell): cell is AnimatedStatusCell =>
    cell.kind === GridCellKind.Custom &&
    (cell.data as AnimatedStatusCellData | undefined)?.kind === "animated-status-cell",
  draw: (args, cell) => {
    const { ctx, theme } = args;
    const t = (args as unknown as { frameTime?: number }).frameTime ?? performance.now();
    const hPad = theme.cellHorizontalPadding ?? 8;

    ctx.save();
    ctx.font = (theme as GridDrawTheme).baseFontFull;
    ctx.textBaseline = "middle";

    const mode = cell.data.mode ?? "label";
    if (mode === "badge") drawBadgeMode(args, cell, t, hPad);
    else if (mode === "spinner-timer") drawSpinnerTimerMode(args, cell, t, hPad);
    else drawLabelMode(args, cell, t, hPad);

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

    const linkColor = theme.linkColor || theme.accentColor || "#2563eb";
    const padding = theme.cellHorizontalPadding ?? 8;
    const font = (theme as GridDrawTheme).baseFontFull;

    drawTruncatedText(ctx, {
      text,
      rect,
      font,
      color: linkColor,
      align,
      padding,
      underline: true,
    });

    return true;
  },
};
