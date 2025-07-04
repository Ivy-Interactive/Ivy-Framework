// Color utility functions for parsing and validating colors
// Supports hex colors (#RRGGBB), RGB values (rgb(r,g,b)), and color names from the Colors enum

export type ColorInputVariant = 'Picker' | 'Text' | 'PickerText' | 'Palette';

// Color names mapping from the Colors enum
const COLOR_NAMES: Record<string, string> = {
  'black': '#000000',
  'white': '#FFFFFF',
  'slate': '#6A7489',
  'gray': '#6E727F',
  'zinc': '#717179',
  'neutral': '#737373',
  'stone': '#76716D',
  'red': '#DD5860',
  'orange': '#DC824D',
  'amber': '#DEB145',
  'yellow': '#E5E04C',
  'lime': '#AFD953',
  'green': '#86D26F',
  'emerald': '#76CD94',
  'teal': '#5B9BA8',
  'cyan': '#4469C0',
  'sky': '#373BDA',
  'blue': '#381FF4',
  'indigo': '#4B28E2',
  'violet': '#6637D1',
  'purple': '#844CC0',
  'fuchsia': '#A361AF',
  'pink': '#C377A0',
  'rose': '#E48E91',
  'primary': '#74C997',
  'secondary': '#C2CBC7',
  'destructive': '#DD5860'
};

/**
 * Validates if a string is a valid hex color
 */
export function isValidHexColor(color: string): boolean {
  const hexRegex = /^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/;
  return hexRegex.test(color);
}

/**
 * Validates if a string is a valid RGB color
 */
export function isValidRgbColor(color: string): boolean {
  const rgbRegex = /^rgb\(\s*\d+\s*,\s*\d+\s*,\s*\d+\s*\)$/;
  if (!rgbRegex.test(color)) return false;
  
  // Extract RGB values and validate ranges
  const match = color.match(/rgb\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)/);
  if (!match) return false;
  
  const [, r, g, b] = match;
  const red = parseInt(r);
  const green = parseInt(g);
  const blue = parseInt(b);
  
  return red >= 0 && red <= 255 && 
         green >= 0 && green <= 255 && 
         blue >= 0 && blue <= 255;
}

/**
 * Validates if a string is a valid color name from the Colors enum
 */
export function isValidColorName(color: string): boolean {
  return color.toLowerCase() in COLOR_NAMES;
}

/**
 * Validates if a string is a valid color (hex, RGB, or color name)
 */
export function isValidColor(color: string): boolean {
  if (!color || typeof color !== 'string') return false;
  
  return isValidHexColor(color) || 
         isValidRgbColor(color) || 
         isValidColorName(color);
}

/**
 * Converts a color string to a standardized format
 * Returns hex color for valid inputs, null for invalid
 */
export function normalizeColor(color: string): string | null {
  if (!color || typeof color !== 'string') return null;
  
  const trimmedColor = color.trim();
  
  // Already a valid hex color
  if (isValidHexColor(trimmedColor)) {
    return trimmedColor;
  }
  
  // RGB color - convert to hex
  if (isValidRgbColor(trimmedColor)) {
    const match = trimmedColor.match(/rgb\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)/);
    if (match) {
      const [, r, g, b] = match;
      const red = parseInt(r);
      const green = parseInt(g);
      const blue = parseInt(b);
      return `#${red.toString(16).padStart(2, '0')}${green.toString(16).padStart(2, '0')}${blue.toString(16).padStart(2, '0')}`;
    }
  }
  
  // Color name - convert to hex
  if (isValidColorName(trimmedColor)) {
    return COLOR_NAMES[trimmedColor.toLowerCase()];
  }
  
  return null;
}

/**
 * Gets the display value for a color (for showing in input fields)
 * Returns the original input if it's a valid color, otherwise returns the normalized hex
 */
export function getColorDisplayValue(color: string | null): string {
  if (!color) return '';
  
  const normalized = normalizeColor(color);
  if (!normalized) return color; // Return original if invalid
  
  // If it's already a hex color, return as is
  if (isValidHexColor(color)) return color;
  
  // If it's RGB, return the hex equivalent
  if (isValidRgbColor(color)) return normalized;
  
  // If it's a color name, return the hex equivalent
  if (isValidColorName(color)) return normalized;
  
  return color;
}

/**
 * Gets all available color names from the Colors enum
 */
export function getAvailableColorNames(): string[] {
  return Object.keys(COLOR_NAMES);
}

/**
 * Gets the hex value for a color name
 */
export function getColorHex(colorName: string): string | null {
  return COLOR_NAMES[colorName.toLowerCase()] || null;
} 