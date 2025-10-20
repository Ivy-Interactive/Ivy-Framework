import { Theme } from '@glideapps/glide-data-grid';
import { Theme as AppTheme } from '@/components/theme-provider/types';

// Helper to get computed CSS variable value
function getCSSVariable(variable: string): string {
  if (typeof document === 'undefined') return '';
  return getComputedStyle(document.documentElement)
    .getPropertyValue(variable)
    .trim();
}

// Determine if we should use dark theme colors
function isDarkTheme(theme: AppTheme): boolean {
  if (theme === 'dark') return true;
  if (theme === 'light') return false;
  // For 'system', check the actual system preference
  if (typeof window === 'undefined') return false;
  return window.matchMedia('(prefers-color-scheme: dark)').matches;
}

// Generate theme based on current theme state
export function getTableTheme(theme: AppTheme): Partial<Theme> {
  const isDark = isDarkTheme(theme);

  return {
    bgCell: getCSSVariable('--background') || (isDark ? '#000000' : '#ffffff'),
    bgHeader:
      getCSSVariable('--background') || (isDark ? '#000000' : '#ffffff'),
    bgHeaderHasFocus:
      getCSSVariable('--muted') || (isDark ? '#26262b' : '#f3f4f6'),
    bgHeaderHovered:
      getCSSVariable('--accent') || (isDark ? '#26262b' : '#e5e7eb'),
    textDark:
      getCSSVariable('--foreground') || (isDark ? '#f8f8f8' : '#111827'),
    textMedium:
      getCSSVariable('--muted-foreground') || (isDark ? '#a1a1aa' : '#6b7280'),
    textLight:
      getCSSVariable('--muted-foreground') || (isDark ? '#71717a' : '#9ca3af'),
    borderColor: 'transparent',
    horizontalBorderColor:
      getCSSVariable('--border') || (isDark ? '#404045' : '#d1d5db'),
    linkColor: getCSSVariable('--accent') || (isDark ? '#26262b' : '#e5e7eb'),
    cellHorizontalPadding: 16,
    cellVerticalPadding: 8,
    headerIconSize: 16,
  };
}
