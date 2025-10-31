import { ThemeColors } from '@/lib/color-utils';

/**
 * Chart color schemes
 */
export type ColorScheme = 'Default' | 'Rainbow';

/**
 * Chart color variable names - single source of truth for available chart colors
 * Add new colors here when they are added to index.css
 */
const chartColorVars = [
  'chart-1',
  'chart-2',
  'chart-3',
  'chart-4',
  'chart-5',
  'chart-6',
  'chart-7',
  'chart-8',
  'chart-9',
  'chart-10',
] as const;

/**
 * Rainbow color scheme - color names that ECharts interprets
 * These are NOT CSS variables, just standard color names
 */
const rainbowColors = [
  'blue',
  'cyan',
  'yellow',
  'red',
  'orange',
  'purple',
  'lime',
  'indigo',
  'rose',
  'green',
  'pink',
  'teal',
  'amber',
  'violet',
  'emerald',
  'fuchsia',
  'sky',
] as const;

/**
 * Read chart colors from CSS variables
 * This function reads chart colors directly from CSS without modifying ThemeColors
 */
function readChartColorsFromCSS(): string[] {
  if (typeof document === 'undefined') return [];

  try {
    const docElement = document.documentElement;
    if (!docElement) return [];

    return chartColorVars
      .map(varName => {
        const value = getComputedStyle(docElement)
          .getPropertyValue(`--${varName}`)
          .trim();
        return value;
      })
      .filter(Boolean);
  } catch (error) {
    console.warn('Failed to read chart colors from CSS:', error);
    return [];
  }
}

/**
 * Get chart colors based on scheme
 *
 * - 'Default': Reads from CSS variables (--chart-1, --chart-2, ..., --chart-10)
 *   These are theme-aware - different values in light vs dark themes
 * - 'Rainbow': Returns standard color names for ECharts to interpret
 *   These are NOT CSS variables, just color names like 'blue', 'cyan', etc.
 *
 * @param scheme - Color scheme to use
 * @param themeColors - Current theme colors (used only for fallback)
 * @returns Array of color values
 */
export const getChartColors = (
  scheme: ColorScheme,
  themeColors: ThemeColors
): string[] => {
  switch (scheme) {
    case 'Default': {
      // Read chart colors directly from CSS variables (theme-aware)
      const colors = readChartColorsFromCSS();
      // Fallback to primary color if no chart colors found
      return colors.length > 0 ? colors : [themeColors.primary];
    }
    case 'Rainbow': {
      // Return color names for ECharts to interpret (NOT theme-aware)
      return rainbowColors as unknown as string[];
    }
    default:
      return [];
  }
};

/**
 * Generate gradient colors for area charts
 * @param colors - Base colors array
 * @param opacity - Gradient opacity
 * @returns Array of gradient configurations for ECharts
 */
export const generateGradientColors = (colors: string[], opacity = 0.4) => {
  return colors.map(color => ({
    opacity,
    type: 'linear' as const,
    x: 0,
    y: 0,
    x2: 0,
    y2: 1,
    colorStops: [
      { offset: 0, color },
      { offset: 1, color: 'transparent' },
    ],
  }));
};
