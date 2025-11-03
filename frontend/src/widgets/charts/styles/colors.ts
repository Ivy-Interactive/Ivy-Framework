import { ThemeColors } from '@/lib/color-utils';

/**
 * Chart color schemes
 */
export type ColorScheme = 'Default' | 'Rainbow';

/**
 * Chart color variables from index.css (--chart-1 through --chart-10)
 * These colors are theme-aware and automatically change for light/dark mode
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
];

/**
 * Rainbow color variables from index.css
 */
const rainbowColorVars = [
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
 * Get chart colors from CSS variables
 * Reads color variables from index.css based on the selected scheme
 *
 * @param scheme - Color scheme ('Default' uses chart-1...5, 'Rainbow' uses blue, cyan, etc.)
 * @param themeColors - Theme colors (used as fallback if CSS variables unavailable)
 * @returns Array of hex color values from CSS variables
 */
export const getChartColors = (
  scheme: ColorScheme,
  themeColors: ThemeColors
): string[] => {
  // SSR protection
  if (typeof document === 'undefined') {
    return [themeColors.primary];
  }

  try {
    // Select color variables based on scheme
    let colorVars: string[];

    if (scheme === 'Rainbow') {
      // Use rainbow color variables: --blue, --cyan, --yellow, etc.
      colorVars = rainbowColorVars;
    } else {
      // Default: Use chart-specific variables: --chart-1, --chart-2, etc.
      colorVars = chartColorVars;
    }

    const colors = colorVars
      .map(name =>
        getComputedStyle(document.documentElement)
          .getPropertyValue(`--${name}`)
          .trim()
      )
      .filter(Boolean);

    // Fallback to primary color if no chart colors found
    return colors.length > 0 ? colors : [themeColors.primary];
  } catch (error) {
    console.warn('Failed to read chart colors from CSS:', error);
    return [themeColors.primary];
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
