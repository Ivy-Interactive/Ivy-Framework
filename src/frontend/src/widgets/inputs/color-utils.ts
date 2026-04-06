import { logger } from "@/lib/logger";

// Hoisted color map for backend Colors enum
export const enumColorsToCssVar: Record<string, string> = {
  black: "var(--color-black)",
  white: "var(--color-white)",
  slate: "var(--color-slate)",
  gray: "var(--color-gray)",
  zinc: "var(--color-zinc)",
  neutral: "var(--color-neutral)",
  stone: "var(--color-stone)",
  red: "var(--color-red)",
  orange: "var(--color-orange)",
  amber: "var(--color-amber)",
  yellow: "var(--color-yellow)",
  lime: "var(--color-lime)",
  green: "var(--color-green)",
  emerald: "var(--color-emerald)",
  teal: "var(--color-teal)",
  cyan: "var(--color-cyan)",
  sky: "var(--color-sky)",
  blue: "var(--color-blue)",
  indigo: "var(--color-indigo)",
  violet: "var(--color-violet)",
  purple: "var(--color-purple)",
  fuchsia: "var(--color-fuchsia)",
  pink: "var(--color-pink)",
  rose: "var(--color-rose)",
  primary: "var(--color-primary)",
  secondary: "var(--color-secondary)",
  destructive: "var(--color-destructive)",
  success: "var(--color-success)",
  warning: "var(--color-warning)",
  info: "var(--color-info)",
  muted: "var(--color-muted)",
};

export function getThemeColorHex(cssVar: string): string | undefined {
  if (typeof window === "undefined") return undefined;
  const value = getComputedStyle(document.documentElement).getPropertyValue(cssVar).trim();
  if (/^#[0-9a-fA-F]{6}$/.test(value)) return value;
  return undefined;
}

/**
 * Converts various color formats to hex.
 * Supported formats: hex (#rrggbb / #rrggbbaa), rgb(), rgba(), named colors
 * Unsupported formats: oklch() - returns fallback color (#000000)
 */
export function convertToHex(colorValue: string): string {
  if (!colorValue) return "";
  if (colorValue.startsWith("#")) {
    return colorValue;
  }
  const rgbaMatch = colorValue.match(/rgba\((\d+),\s*(\d+),\s*(\d+),\s*([\d.]+)\)/);
  if (rgbaMatch) {
    const r = parseInt(rgbaMatch[1]);
    const g = parseInt(rgbaMatch[2]);
    const b = parseInt(rgbaMatch[3]);
    const a = Math.round(parseFloat(rgbaMatch[4]) * 255);
    const hex = `#${r.toString(16).padStart(2, "0")}${g.toString(16).padStart(2, "0")}${b.toString(16).padStart(2, "0")}`;
    if (a < 255) return hex + a.toString(16).padStart(2, "0");
    return hex;
  }
  const rgbMatch = colorValue.match(/rgb\((\d+),\s*(\d+),\s*(\d+)\)/);
  if (rgbMatch) {
    const r = parseInt(rgbMatch[1]);
    const g = parseInt(rgbMatch[2]);
    const b = parseInt(rgbMatch[3]);
    return `#${r.toString(16).padStart(2, "0")}${g.toString(16).padStart(2, "0")}${b.toString(16).padStart(2, "0")}`;
  }
  const hslMatch = colorValue.match(/hsla?\((\d+),\s*(\d+)%?,\s*(\d+)%?(?:,\s*[\d.]+)?\)/);
  if (hslMatch) {
    const h = parseInt(hslMatch[1]) / 360;
    const s = parseInt(hslMatch[2]) / 100;
    const l = parseInt(hslMatch[3]) / 100;
    let r, g, b;
    if (s === 0) {
      r = g = b = l; // achromatic
    } else {
      const hue2rgb = (p: number, q: number, t: number) => {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1 / 6) return p + (q - p) * 6 * t;
        if (t < 1 / 2) return q;
        if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
        return p;
      };
      const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
      const p = 2 * l - q;
      r = hue2rgb(p, q, h + 1 / 3);
      g = hue2rgb(p, q, h);
      b = hue2rgb(p, q, h - 1 / 3);
    }
    const toHex = (x: number) => {
      const hex = Math.round(x * 255).toString(16);
      return hex.length === 1 ? "0" + hex : hex;
    };
    return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
  }
  // More comprehensive OKLCH detection
  const isOklch = /^oklch\s*\(/i.test(colorValue.trim());
  if (isOklch) {
    logger.warn(`OKLCH color format not supported: ${colorValue}`);
    return "#000000"; // Default fallback
  }
  // Use theme color if available
  const lowerValue = colorValue.toLowerCase();
  if (enumColorsToCssVar[lowerValue]) {
    const cssVar = enumColorsToCssVar[lowerValue].replace("var(", "").replace(")", "");
    const themeHex = getThemeColorHex(cssVar);
    if (themeHex) return themeHex;
  }
  return colorValue;
}

export function getDisplayColor(displayValue: string): string {
  if (!displayValue) return "#000000";
  const hexValue = convertToHex(displayValue);
  if (hexValue.startsWith("var(")) return "#000000";
  if (hexValue.startsWith("#") && hexValue.length === 9) {
    return hexValue.slice(0, 7);
  }
  return hexValue.startsWith("#") ? hexValue : "#000000";
}

export function parseHexAlpha(hex: string): { rgb: string; alpha: number } {
  if (!hex || !hex.startsWith("#")) return { rgb: hex || "#000000", alpha: 255 };
  const clean = hex.slice(1);
  if (clean.length === 8) {
    return {
      rgb: "#" + clean.slice(0, 6),
      alpha: parseInt(clean.slice(6, 8), 16),
    };
  }
  return { rgb: hex.length === 7 ? hex : "#000000", alpha: 255 };
}

export function combineHexAlpha(rgb: string, alpha: number): string {
  const base = rgb.startsWith("#") ? rgb : "#" + rgb;
  const hex6 = base.length === 7 ? base : "#000000";
  if (alpha >= 255) return hex6; // fully opaque → keep 6-char hex
  const aa = Math.max(0, Math.min(255, alpha)).toString(16).padStart(2, "0");
  return hex6 + aa;
}
