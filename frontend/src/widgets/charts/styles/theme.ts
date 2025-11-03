import { ThemeColors } from '@/lib/color-utils';

/**
 * Chart theme colors interface
 */
export interface ChartThemeColors {
  foreground: string;
  mutedForeground: string;
  fontSans: string;
  background: string;
}

/**
 * Extract chart-specific theme colors from full theme colors
 * @param colors - Full theme colors from CSS variables
 * @param isDark - Whether dark mode is active
 * @returns Chart theme colors
 */
export const getChartThemeColors = (
  colors: ThemeColors,
  isDark: boolean
): ChartThemeColors => {
  // Read font-sans CSS variable with error handling (similar to getChartColors)
  const getFontSans = (): string => {
    if (typeof document === 'undefined') {
      return 'Geist, sans-serif'; // SSR fallback
    }

    try {
      const font = getComputedStyle(document.documentElement)
        .getPropertyValue('--font-sans')
        .trim();
      return font || 'Geist, sans-serif';
    } catch (error) {
      console.warn('Failed to read font from CSS:', error);
      return 'Geist, sans-serif';
    }
  };

  return {
    foreground: colors.foreground || (isDark ? '#f8f8f8' : '#000000'),
    mutedForeground: colors.mutedForeground || (isDark ? '#a1a1aa' : '#666666'),
    fontSans: getFontSans(),
    background: colors.background || (isDark ? '#000000' : '#ffffff'),
  };
};

/**
 * Generate text style for ECharts
 */
export const generateTextStyle = (
  foreground: string = '#000000',
  fontSans: string = 'Geist, sans-serif'
) => ({
  color: foreground,
  fontFamily: fontSans,
  fontSize: 12,
});

/**
 * Generate axis label style for ECharts
 */
export const generateAxisLabelStyle = (
  mutedForeground: string = '#666666',
  fontSans: string = 'Geist, sans-serif'
) => ({
  color: mutedForeground,
  fontFamily: fontSans,
  fontSize: 11,
});
