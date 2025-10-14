import { icons } from 'lucide-react';
import { createElement } from 'react';
import { renderToStaticMarkup } from 'react-dom/server';

// Cache for SVG strings
const svgCache = new Map<string, string>();

// Cache for icon images
const iconImageCache = new Map<string, HTMLImageElement>();

/**
 * Validates if an icon name exists in lucide-react
 */
export function isValidIconName(iconName: string): boolean {
  return iconName in icons;
}

/**
 * Gets the SVG string for a Lucide icon
 */
export function getIconSVG(
  iconName: string,
  options: {
    size?: number;
    color?: string;
    strokeWidth?: number;
  } = {}
): string | null {
  const { size = 20, color = '#666', strokeWidth = 2 } = options;
  const cacheKey = `${iconName}-${size}-${color}-${strokeWidth}`;

  // Check cache first
  if (svgCache.has(cacheKey)) {
    return svgCache.get(cacheKey)!;
  }

  // Get the icon component from lucide-react
  const IconComponent = icons[iconName as keyof typeof icons];

  if (!IconComponent) {
    console.warn(`Icon ${iconName} not found in lucide-react`);
    return null;
  }

  try {
    // Render to static SVG string
    const element = createElement(IconComponent, {
      size,
      color,
      strokeWidth,
    });

    const svgString = renderToStaticMarkup(element);
    svgCache.set(cacheKey, svgString);
    return svgString;
  } catch (error) {
    console.error(`Error rendering icon ${iconName}:`, error);
    return null;
  }
}

/**
 * Converts SVG string to data URL
 */
export function svgToDataUrl(svgString: string): string {
  // Encode SVG string to base64
  const base64 = btoa(svgString);
  return `data:image/svg+xml;base64,${base64}`;
}

/**
 * Converts a Lucide icon name to an HTMLImageElement
 * Uses cached images when available
 */
export function getIconImage(
  iconName: string,
  options: {
    size?: number;
    color?: string;
    strokeWidth?: number;
  } = {}
): HTMLImageElement | null {
  const { size = 20, color = '#666', strokeWidth = 2 } = options;
  const cacheKey = `${iconName}-${size}-${color}-${strokeWidth}`;

  // Check cache first
  if (iconImageCache.has(cacheKey)) {
    return iconImageCache.get(cacheKey)!;
  }

  // Get SVG string
  const svgString = getIconSVG(iconName, options);
  if (!svgString) {
    return null;
  }

  // Convert to data URL and create image
  const dataUrl = svgToDataUrl(svgString);
  const img = new Image();
  img.src = dataUrl;

  // Cache the image
  iconImageCache.set(cacheKey, img);

  return img;
}

/**
 * Asynchronously loads an icon image
 * Useful for preloading or when you need to ensure image is fully loaded
 */
export async function loadIconImage(
  iconName: string,
  options: {
    size?: number;
    color?: string;
    strokeWidth?: number;
  } = {}
): Promise<HTMLImageElement | null> {
  const img = getIconImage(iconName, options);

  if (!img) {
    return null;
  }

  // If image is already complete, return it
  if (img.complete) {
    return img;
  }

  // Wait for image to load
  return new Promise((resolve, reject) => {
    img.onload = () => resolve(img);
    img.onerror = () => reject(new Error(`Failed to load icon: ${iconName}`));
  });
}

/**
 * Preloads multiple icons
 */
export async function preloadIcons(
  iconNames: string[],
  options?: {
    size?: number;
    color?: string;
    strokeWidth?: number;
  }
): Promise<void> {
  await Promise.all(iconNames.map(name => loadIconImage(name, options)));
}

/**
 * Clears the icon caches
 */
export function clearIconCache(): void {
  svgCache.clear();
  iconImageCache.clear();
}
