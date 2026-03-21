/**
 * Color utility functions for theme management
 */

/**
 * Get computed CSS variable value and optionally convert to hex
 */
export function getCSSVariable(variable: string): string {
  if (typeof document === 'undefined') return '';

  const value = getComputedStyle(document.documentElement)
    .getPropertyValue(variable)
    .trim();

  // If it's already a hex color, return it
  if (value.startsWith('#')) return value;

  return value;
}

/**
 * Check if the document is in dark mode
 */
export function isDarkMode(): boolean {
  if (typeof document === 'undefined') return false;
  return document.documentElement.classList.contains('dark');
}

/**
 * Check system preference for dark mode
 */
export function getSystemThemePreference(): 'light' | 'dark' {
  if (typeof window === 'undefined') return 'light';
  return window.matchMedia('(prefers-color-scheme: dark)').matches
    ? 'dark'
    : 'light';
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

export function getThemeColors(): ThemeColors {
  return {
    background: getCSSVariable('--background'),
    foreground: getCSSVariable('--foreground'),
    card: getCSSVariable('--card'),
    cardForeground: getCSSVariable('--card-foreground'),
    popover: getCSSVariable('--popover'),
    popoverForeground: getCSSVariable('--popover-foreground'),
    primary: getCSSVariable('--primary'),
    primaryForeground: getCSSVariable('--primary-foreground'),
    secondary: getCSSVariable('--secondary'),
    secondaryForeground: getCSSVariable('--secondary-foreground'),
    muted: getCSSVariable('--muted'),
    mutedForeground: getCSSVariable('--muted-foreground'),
    accent: getCSSVariable('--accent'),
    accentForeground: getCSSVariable('--accent-foreground'),
    destructive: getCSSVariable('--destructive'),
    destructiveForeground: getCSSVariable('--destructive-foreground'),
    border: getCSSVariable('--border'),
    input: getCSSVariable('--input'),
    ring: getCSSVariable('--ring'),
    radius: getCSSVariable('--radius'),
  };
}
