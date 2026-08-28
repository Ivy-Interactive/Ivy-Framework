import { getCSSVariable } from "@/lib/theme";
import { Densities } from "@/types/density";
import { DENSITY_CONFIG } from "../dataTableEditor/constants";

const ELLIPSIS = "\u2026";

let measureCanvas: HTMLCanvasElement | null = null;

function getMeasureContext(): CanvasRenderingContext2D {
  if (!measureCanvas) {
    measureCanvas = document.createElement("canvas");
  }
  const ctx = measureCanvas.getContext("2d");
  if (!ctx) {
    throw new Error("Failed to create canvas 2d context for text measurement");
  }
  return ctx;
}

/** Canvas font string aligned with glide grid theme and table density. */
export function getCellFont(density: Densities = Densities.Medium): string {
  const { fontSize } = DENSITY_CONFIG[density];
  const rawFontSans = getCSSVariable("--font-sans") || "Geist";
  const fontSans = rawFontSans.replace(/^["']|["']$/g, "") || "Geist";
  return `${fontSize} ${fontSans}, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif`;
}

/**
 * Truncates text to fit within maxWidth pixels, appending an ellipsis when clipped.
 */
export function truncateTextWithEllipsis(
  text: string,
  maxWidth: number,
  font: string,
  ellipsis: string = ELLIPSIS,
): string {
  if (!text || maxWidth <= 0) return "";
  const ctx = getMeasureContext();
  ctx.font = font;
  if (ctx.measureText(text).width <= maxWidth) return text;

  const ellipsisWidth = ctx.measureText(ellipsis).width;
  const targetWidth = maxWidth - ellipsisWidth;
  if (targetWidth <= 0) return ellipsis;

  let low = 0;
  let high = text.length;
  while (low < high) {
    const mid = Math.ceil((low + high) / 2);
    if (ctx.measureText(text.slice(0, mid)).width <= targetWidth) {
      low = mid;
    } else {
      high = mid - 1;
    }
  }
  return text.slice(0, low) + ellipsis;
}

export interface DrawTruncatedTextOptions {
  text: string;
  rect: { x: number; y: number; width: number; height: number };
  font: string;
  color: string;
  align?: "left" | "center" | "right";
  padding?: number;
  underline?: boolean;
}

/**
 * Draws single-line text clipped to the cell rect with ellipsis when it overflows.
 */
export function drawTruncatedText(
  ctx: CanvasRenderingContext2D,
  options: DrawTruncatedTextOptions,
): string {
  const { text, rect, font, color, align = "left", padding = 8, underline = false } = options;
  if (!text) return "";

  const maxWidth = Math.max(0, rect.width - padding * 2);
  ctx.save();
  ctx.beginPath();
  ctx.rect(rect.x, rect.y, rect.width, rect.height);
  ctx.clip();

  ctx.font = font;
  const displayText = truncateTextWithEllipsis(text, maxWidth, font);
  const textWidth = ctx.measureText(displayText).width;
  const textY = rect.y + rect.height / 2;

  let textX: number;
  if (align === "center") {
    textX = rect.x + (rect.width - textWidth) / 2;
  } else if (align === "right") {
    textX = rect.x + rect.width - textWidth - padding;
  } else {
    textX = rect.x + padding;
  }

  ctx.fillStyle = color;
  ctx.textBaseline = "middle";
  ctx.fillText(displayText, textX, textY);

  if (underline) {
    ctx.strokeStyle = color;
    ctx.lineWidth = 1;
    ctx.beginPath();
    const underlineY = textY + 8;
    ctx.moveTo(textX, underlineY);
    ctx.lineTo(textX + textWidth, underlineY);
    ctx.stroke();
  }

  ctx.restore();
  return displayText;
}

export function getMaxTextWidth(columnWidth: number, cellHorizontalPadding: number): number {
  return Math.max(0, columnWidth - cellHorizontalPadding * 2);
}
