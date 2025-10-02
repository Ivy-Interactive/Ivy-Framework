import { icons } from 'lucide-react';
import { createRoot } from 'react-dom/client';
import React from 'react';

// Cache for icon images
const iconCache = new Map<string, HTMLImageElement>();
const iconLoadPromises = new Map<string, Promise<HTMLImageElement>>();

/**
 * Converts a Lucide icon name to an SVG data URL and loads it as an image
 */
export async function loadIconImage(
  iconName: string
): Promise<HTMLImageElement | null> {
  // Check cache first
  if (iconCache.has(iconName)) {
    return iconCache.get(iconName)!;
  }

  // Check if already loading
  if (iconLoadPromises.has(iconName)) {
    return iconLoadPromises.get(iconName)!;
  }

  // Create loading promise
  const loadPromise = new Promise<HTMLImageElement>((resolve, reject) => {
    try {
      // Get the icon component from lucide-react
      const IconComponent = icons[iconName as keyof typeof icons];

      if (!IconComponent) {
        reject(new Error(`Icon ${iconName} not found`));
        return;
      }

      // Create a temporary container to render the icon
      const container = document.createElement('div');
      container.style.position = 'absolute';
      container.style.left = '-9999px';
      container.style.width = '24px';
      container.style.height = '24px';
      document.body.appendChild(container);

      // Create root and render the icon
      const root = createRoot(container);
      const iconElement = React.createElement(IconComponent, {
        size: 20,
        color: '#666',
        strokeWidth: 2,
      });

      root.render(iconElement);

      // Wait for render to complete
      setTimeout(() => {
        const svgElement = container.querySelector('svg');

        if (!svgElement) {
          root.unmount();
          document.body.removeChild(container);
          reject(new Error(`Failed to render SVG for icon: ${iconName}`));
          return;
        }

        // Get the SVG as a string
        const svgString = new XMLSerializer().serializeToString(svgElement);

        // Clean up
        root.unmount();
        document.body.removeChild(container);

        // Convert SVG to data URL
        const svgDataUrl = `data:image/svg+xml;base64,${btoa(svgString)}`;

        // Create image from data URL
        const img = new Image();
        img.onload = () => {
          iconCache.set(iconName, img);
          resolve(img);
        };
        img.onerror = () => {
          reject(new Error(`Failed to load icon: ${iconName}`));
        };
        img.src = svgDataUrl;
      }, 50); // Small delay to ensure render is complete
    } catch (error) {
      reject(error);
    }
  });

  iconLoadPromises.set(iconName, loadPromise);

  try {
    const img = await loadPromise;
    iconLoadPromises.delete(iconName);
    return img;
  } catch (error) {
    iconLoadPromises.delete(iconName);
    console.error(`Error loading icon ${iconName}:`, error);
    return null;
  }
}

/**
 * Gets a cached icon image if available
 */
export function getCachedIcon(iconName: string): HTMLImageElement | null {
  return iconCache.get(iconName) || null;
}

/**
 * Preloads multiple icons
 */
export async function preloadIcons(iconNames: string[]): Promise<void> {
  await Promise.all(iconNames.map(name => loadIconImage(name)));
}
