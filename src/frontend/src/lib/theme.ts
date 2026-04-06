/**
 * Color utility functions for theme management
 */

/**
 * Get computed CSS variable value and optionally convert to hex
 */
export function getCSSVariable(variable: string): string {
  if (typeof document === "undefined") return "";

  const value = getComputedStyle(document.documentElement).getPropertyValue(variable).trim();

  // If it's already a hex color, return it
  if (value.startsWith("#")) return value;

  return value;
}

/**
 * Check if the document is in dark mode
 */
export function isDarkMode(): boolean {
  if (typeof document === "undefined") return false;
  return document.documentElement.classList.contains("dark");
}

/**
 * Check system preference for dark mode
 */
export function getSystemThemePreference(): "light" | "dark" {
  if (typeof window === "undefined") return "light";
  return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

/**
 * Get all theme CSS variables
 */
export interface ThemeColors {
  background: string;
  foreground: string;
  card: string;
  cardForeground: string;
  popover: string;
  popoverForeground: string;
  primary: string;
  primaryForeground: string;
  secondary: string;
  secondaryForeground: string;
  muted: string;
  mutedForeground: string;
  accent: string;
  accentForeground: string;
  destructive: string;
  destructiveForeground: string;
  border: string;
  input: string;
  ring: string;
  radius: string;
}

export const CHROMATIC_COLORS: Record<string, string> = {
  black: "#000000",
  white: "#ffffff",
  slate: "#64748b",
  gray: "#6b7280",
  zinc: "#71717a",
  neutral: "#737373",
  stone: "#78716c",
  red: "#ef4444",
  orange: "#f97316",
  amber: "#f59e0b",
  yellow: "#eab308",
  lime: "#84cc16",
  green: "#22c55e",
  emerald: "#10b981",
  teal: "#14b8a6",
  cyan: "#06b6d4",
  sky: "#0ea5e9",
  blue: "#3b82f6",
  indigo: "#6366f1",
  violet: "#8b5cf6",
  purple: "#a855f7",
  fuchsia: "#d946ef",
  pink: "#ec4899",
  rose: "#f43f5e",
};

export const DEFAULT_SEMANTIC_COLORS: Record<string, string> = {
  primary: "#3b82f6",
  secondary: "#6b7280",
  destructive: "#ef4444",
  muted: "#6b7280",
  foreground: "#000000",
  background: "#ffffff",
};

export function resolveColor(
  color: string | undefined,
  fallback: string,
  colorMap: Record<string, string>,
): string {
  if (!color) return fallback;
  if (color.startsWith("#") || color.startsWith("rgb")) return color;
  return colorMap[color.toLowerCase()] ?? fallback;
}

export function getThemeColors(): ThemeColors {
  return {
    background: getCSSVariable("--background"),
    foreground: getCSSVariable("--foreground"),
    card: getCSSVariable("--card"),
    cardForeground: getCSSVariable("--card-foreground"),
    popover: getCSSVariable("--popover"),
    popoverForeground: getCSSVariable("--popover-foreground"),
    primary: getCSSVariable("--primary"),
    primaryForeground: getCSSVariable("--primary-foreground"),
    secondary: getCSSVariable("--secondary"),
    secondaryForeground: getCSSVariable("--secondary-foreground"),
    muted: getCSSVariable("--muted"),
    mutedForeground: getCSSVariable("--muted-foreground"),
    accent: getCSSVariable("--accent"),
    accentForeground: getCSSVariable("--accent-foreground"),
    destructive: getCSSVariable("--destructive"),
    destructiveForeground: getCSSVariable("--destructive-foreground"),
    border: getCSSVariable("--border"),
    input: getCSSVariable("--input"),
    ring: getCSSVariable("--ring"),
    radius: getCSSVariable("--radius"),
  };
}
