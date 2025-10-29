import { ThemeColors } from '@/lib/color-utils';

/**
 * Chart color schemes
 */
export type ColorScheme = 'Default' | 'Rainbow';

/**
 * Chart color variable names - single source of truth for available chart colors
 * Add new colors here when they are added to index.css
 */
const CHART_COLOR_VARIABLES = [
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
 * Rainbow color scheme - static colors that work in both themes
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
];

/**
 * Read chart colors from CSS variables
 * This function reads chart colors directly from CSS without modifying ThemeColors
 */
function readChartColorsFromCSS(): string[] {
  if (typeof document === 'undefined') return [];

  return CHART_COLOR_VARIABLES.map(varName => {
    const value = getComputedStyle(document.documentElement)
      .getPropertyValue(`--${varName}`)
      .trim();
    return value;
  }).filter(Boolean);
}

/**
 * Get chart colors based on scheme
 *
 * IMPORTANT: Chart colors DO NOT change with theme!
 * Only background, text, axes, and tooltips change with theme.
 * Chart line colors remain consistent across light and dark themes
 * to maintain data color associations that users learn.
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
      // Read chart colors directly from CSS variables
      const colors = readChartColorsFromCSS();
      // Fallback to primary color if no chart colors found
      return colors.length > 0 ? colors : [themeColors.primary];
    }
    case 'Rainbow':
      return rainbowColors;
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
