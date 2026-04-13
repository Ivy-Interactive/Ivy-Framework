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

  // Semantic colors
  success: string;
  successForeground: string;
  warning: string;
  warningForeground: string;
  info: string;
  infoForeground: string;

  // Neutral colors
  slate: string;
  slateForeground: string;
  gray: string;
  grayForeground: string;
  zinc: string;
  zincForeground: string;
  neutral: string;
  neutralForeground: string;
  stone: string;
  stoneForeground: string;
  black: string;
  blackForeground: string;
  white: string;
  whiteForeground: string;

  // Chromatic colors
  red: string;
  redForeground: string;
  orange: string;
  orangeForeground: string;
  amber: string;
  amberForeground: string;
  yellow: string;
  yellowForeground: string;
  lime: string;
  limeForeground: string;
  green: string;
  greenForeground: string;
  emerald: string;
  emeraldForeground: string;
  teal: string;
  tealForeground: string;
  cyan: string;
  cyanForeground: string;
  sky: string;
  skyForeground: string;
  blue: string;
  blueForeground: string;
  indigo: string;
  indigoForeground: string;
  violet: string;
  violetForeground: string;
  purple: string;
  purpleForeground: string;
  fuchsia: string;
  fuchsiaForeground: string;
  pink: string;
  pinkForeground: string;
  rose: string;
  roseForeground: string;
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

    // Semantic colors
    success: getCSSVariable("--success"),
    successForeground: getCSSVariable("--success-foreground"),
    warning: getCSSVariable("--warning"),
    warningForeground: getCSSVariable("--warning-foreground"),
    info: getCSSVariable("--info"),
    infoForeground: getCSSVariable("--info-foreground"),

    // Neutral colors
    slate: getCSSVariable("--slate"),
    slateForeground: getCSSVariable("--slate-foreground"),
    gray: getCSSVariable("--gray"),
    grayForeground: getCSSVariable("--gray-foreground"),
    zinc: getCSSVariable("--zinc"),
    zincForeground: getCSSVariable("--zinc-foreground"),
    neutral: getCSSVariable("--neutral"),
    neutralForeground: getCSSVariable("--neutral-foreground"),
    stone: getCSSVariable("--stone"),
    stoneForeground: getCSSVariable("--stone-foreground"),
    black: getCSSVariable("--black"),
    blackForeground: getCSSVariable("--black-foreground"),
    white: getCSSVariable("--white"),
    whiteForeground: getCSSVariable("--white-foreground"),

    // Chromatic colors
    red: getCSSVariable("--red"),
    redForeground: getCSSVariable("--red-foreground"),
    orange: getCSSVariable("--orange"),
    orangeForeground: getCSSVariable("--orange-foreground"),
    amber: getCSSVariable("--amber"),
    amberForeground: getCSSVariable("--amber-foreground"),
    yellow: getCSSVariable("--yellow"),
    yellowForeground: getCSSVariable("--yellow-foreground"),
    lime: getCSSVariable("--lime"),
    limeForeground: getCSSVariable("--lime-foreground"),
    green: getCSSVariable("--green"),
    greenForeground: getCSSVariable("--green-foreground"),
    emerald: getCSSVariable("--emerald"),
    emeraldForeground: getCSSVariable("--emerald-foreground"),
    teal: getCSSVariable("--teal"),
    tealForeground: getCSSVariable("--teal-foreground"),
    cyan: getCSSVariable("--cyan"),
    cyanForeground: getCSSVariable("--cyan-foreground"),
    sky: getCSSVariable("--sky"),
    skyForeground: getCSSVariable("--sky-foreground"),
    blue: getCSSVariable("--blue"),
    blueForeground: getCSSVariable("--blue-foreground"),
    indigo: getCSSVariable("--indigo"),
    indigoForeground: getCSSVariable("--indigo-foreground"),
    violet: getCSSVariable("--violet"),
    violetForeground: getCSSVariable("--violet-foreground"),
    purple: getCSSVariable("--purple"),
    purpleForeground: getCSSVariable("--purple-foreground"),
    fuchsia: getCSSVariable("--fuchsia"),
    fuchsiaForeground: getCSSVariable("--fuchsia-foreground"),
    pink: getCSSVariable("--pink"),
    pinkForeground: getCSSVariable("--pink-foreground"),
    rose: getCSSVariable("--rose"),
    roseForeground: getCSSVariable("--rose-foreground"),
  };
}
