import { GlideThemeColors } from '../types';

/**
 * Default Ivy-themed color scheme for Glide Data Grid
 */
export const defaultGlideTheme: GlideThemeColors = {
  // Primary colors matching Ivy design system
  accentColor: '#3b82f6',
  accentLight: '#dbeafe',

  // Text colors
  textDark: '#374151',
  textMedium: '#6b7280',
  textLight: '#9ca3af',
  textBubble: '#374151',

  // Header colors
  bgIconHeader: '#f3f4f6',
  fgIconHeader: '#6b7280',
  textHeader: '#374151',
  textHeaderSelected: '#1f2937',

  // Cell colors
  bgCell: '#ffffff',
  bgCellMedium: '#f9fafb',

  // Background colors
  bgHeader: '#f3f4f6',
  bgHeaderHovered: '#e5e7eb',
  bgHeaderHasFocus: '#dbeafe',

  // Border colors
  borderColor: '#e5e7eb',
  drilldownBorder: '#3b82f6',

  // Link color
  linkColor: '#3b82f6',

  // Typography
  headerFontStyle: '600 14px',
  baseFontStyle: '14px',
  fontFamily: 'Inter, system-ui, sans-serif',
};

/**
 * Dark theme variant
 */
export const darkGlideTheme: GlideThemeColors = {
  accentColor: '#60a5fa',
  accentLight: '#1e3a8a',

  textDark: '#f9fafb',
  textMedium: '#d1d5db',
  textLight: '#9ca3af',
  textBubble: '#f9fafb',

  bgIconHeader: '#374151',
  fgIconHeader: '#9ca3af',
  textHeader: '#f9fafb',
  textHeaderSelected: '#ffffff',

  bgCell: '#1f2937',
  bgCellMedium: '#374151',

  bgHeader: '#374151',
  bgHeaderHovered: '#4b5563',
  bgHeaderHasFocus: '#1e3a8a',

  borderColor: '#4b5563',
  drilldownBorder: '#60a5fa',

  linkColor: '#60a5fa',

  headerFontStyle: '600 14px',
  baseFontStyle: '14px',
  fontFamily: 'Inter, system-ui, sans-serif',
};

/**
 * Creates a theme with custom overrides
 */
export function createCustomTheme(
  baseTheme: GlideThemeColors = defaultGlideTheme,
  overrides: Partial<GlideThemeColors> = {}
): GlideThemeColors {
  return {
    ...baseTheme,
    ...overrides,
  };
}
