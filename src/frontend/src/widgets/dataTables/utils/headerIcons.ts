import type { SpriteMap, SpriteProps } from '@glideapps/glide-data-grid';

/**
 * Map of Lucide icon paths
 * These are simplified paths for common icons used in the data table
 */
const ICON_PATHS: Record<string, string> = {
  // User icon
  User: 'M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2 M12 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z',
  // Mail icon
  Mail: 'M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z M22 6l-10 7L2 6',
  // Hash icon
  Hash: 'M4 9h16 M4 15h16 M10 3L8 21 M16 3l-2 18',
  // Calendar icon
  Calendar:
    'M3 6a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V6z M16 2v4 M8 2v4 M3 10h18',
  // Clock icon
  Clock: 'M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20z M12 6v6l4 2',
  // Activity icon
  Activity: 'M22 12h-4l-3 9L9 3l-3 9H2',
  // Flag icon
  Flag: 'M4 15s1-1 4-1 5 2 8 2 4-1 4-1V3s-1 1-4 1-5-2-8-2-4 1-4 1z M4 22v-7',
  // Zap icon
  Zap: 'M13 2L3 14h9l-1 8 10-12h-9l1-8z',
  // Info icon
  Info: 'M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20z M12 16v-4 M12 8h.01',
  // ChevronUp
  ChevronUp: 'M18 15l-6-6-6 6',
  // ChevronDown
  ChevronDown: 'M6 9l6 6 6-6',
  // Filter
  Filter: 'M22 3H2l8 9.46V19l4 2v-8.54L22 3z',
  // Search
  Search: 'M21 21l-6-6m2-5a7 7 0 1 1-14 0 7 7 0 0 1 14 0z',
  // Settings
  Settings: 'M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20z M12 8v8 M8 12h8',
  // MoreVertical
  MoreVertical:
    'M12 5a1 1 0 1 0 0-2 1 1 0 0 0 0 2z M12 13a1 1 0 1 0 0-2 1 1 0 0 0 0 2z M12 21a1 1 0 1 0 0-2 1 1 0 0 0 0 2z',
  // HelpCircle
  HelpCircle:
    'M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20z M9 9a3 3 0 0 1 6 0c0 2-3 3-3 3 M12 17h.01',
};

/**
 * Converts a Lucide icon to Glide Data Grid format
 * The function accepts SpriteProps with fgColor and bgColor
 * Icons are rendered as outlines only, with optional background
 */
function createIconGenerator(iconName: string) {
  return (props: SpriteProps): string => {
    const { fgColor, bgColor } = props;
    const pathData = ICON_PATHS[iconName];

    if (!pathData) {
      console.warn(`Icon ${iconName} not found in ICON_PATHS`);
      // Return empty SVG as fallback
      return `<svg width="24" height="24" fill="none" xmlns="http://www.w3.org/2000/svg"></svg>`;
    }

    // Create SVG string in the exact format Glide Data Grid expects
    // Using viewBox 0 0 24 24 (standard Lucide size) and 24x24 size for better visibility
    // Include background circle if bgColor is provided and not transparent
    const bgElement =
      bgColor && bgColor !== 'transparent'
        ? `<circle cx="12" cy="12" r="10" fill="${bgColor}" opacity="0.1"/>`
        : '';

    // Use the fgColor for the stroke, which should be properly set by the theme
    return `<svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">${bgElement}<path d="${pathData}" stroke="${fgColor}" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" fill="none"/></svg>`;
  };
}

/**
 * Generates header icons map from column icons
 * @param columns - Array of columns with potential icon properties
 * @returns SpriteMap for Glide Data Grid headerIcons prop
 */
export function generateHeaderIcons(
  columns: Array<{ icon?: string | null }>
): SpriteMap {
  const icons: SpriteMap = {};
  const processedIcons = new Set<string>();

  // Process all unique icons from columns
  columns.forEach(col => {
    if (col.icon && !processedIcons.has(col.icon)) {
      processedIcons.add(col.icon);
      icons[col.icon] = createIconGenerator(col.icon);
    }
  });
  return icons;
}

/**
 * Adds additional icons to the header icons map
 * Useful for adding standard icons that might not be in columns
 */
export function addStandardIcons(baseIcons: SpriteMap): SpriteMap {
  const standardIcons = [
    'ChevronUp',
    'ChevronDown',
    'Filter',
    'Search',
    'Settings',
    'MoreVertical',
    'Info',
    'HelpCircle',
  ];

  const extendedIcons = { ...baseIcons };

  standardIcons.forEach(iconName => {
    if (!extendedIcons[iconName]) {
      extendedIcons[iconName] = createIconGenerator(iconName);
    }
  });

  return extendedIcons;
}
