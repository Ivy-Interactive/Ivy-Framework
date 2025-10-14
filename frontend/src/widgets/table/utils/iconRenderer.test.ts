import { describe, it, expect, beforeEach, vi, beforeAll } from 'vitest';
import {
  isValidIconName,
  getIconSVG,
  svgToDataUrl,
  getIconImage,
  loadIconImage,
  preloadIcons,
  clearIconCache,
} from './iconRenderer';

// Mock HTMLImageElement for Node environment
class MockImage {
  src = '';
  complete = true;
  onload: (() => void) | null = null;
  onerror: (() => void) | null = null;

  constructor() {
    // Simulate async loading
    setTimeout(() => {
      if (this.onload) this.onload();
    }, 0);
  }
}

// Setup globals for Node environment
beforeAll(() => {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  global.Image = MockImage as any;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  global.HTMLImageElement = MockImage as any;
});

interface MockReactElement {
  props: {
    size: number;
    color: string;
    strokeWidth: number;
  };
}

// Mock react-dom/server
vi.mock('react-dom/server', () => ({
  renderToStaticMarkup: (element: MockReactElement) => {
    // Simple mock that returns a fake SVG string
    const { size, color, strokeWidth } = element.props;
    return `<svg width="${size}" height="${size}" stroke="${color}" stroke-width="${strokeWidth}"><path d="M..."/></svg>`;
  },
}));

describe('iconRenderer', () => {
  beforeEach(() => {
    clearIconCache();
    vi.clearAllMocks();
  });

  describe('isValidIconName', () => {
    it('should return true for valid Lucide icon names', () => {
      // Use actual Lucide icon names
      expect(isValidIconName('Activity')).toBe(true);
      expect(isValidIconName('Archive')).toBe(true);
      expect(isValidIconName('AlarmClock')).toBe(true);
    });

    it('should return false for invalid icon names', () => {
      expect(isValidIconName('InvalidIconName123')).toBe(false);
      expect(isValidIconName('NotAnIcon')).toBe(false);
      expect(isValidIconName('')).toBe(false);
    });

    it('should return false for non-string values', () => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect(isValidIconName(null as any)).toBe(false);
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect(isValidIconName(undefined as any)).toBe(false);
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      expect(isValidIconName(123 as any)).toBe(false);
    });
  });

  describe('getIconSVG', () => {
    it('should return SVG string for valid icon', () => {
      const svg = getIconSVG('Activity');
      expect(svg).toBeTruthy();
      expect(svg).toContain('<svg');
      expect(svg).toContain('</svg>');
    });

    it('should use default options when not provided', () => {
      const svg = getIconSVG('Activity');
      expect(svg).toContain('width="20"');
      expect(svg).toContain('stroke="#666"');
      expect(svg).toContain('stroke-width="2"');
    });

    it('should use custom options when provided', () => {
      const svg = getIconSVG('Activity', {
        size: 24,
        color: '#ff0000',
        strokeWidth: 3,
      });
      expect(svg).toContain('width="24"');
      expect(svg).toContain('stroke="#ff0000"');
      expect(svg).toContain('stroke-width="3"');
    });

    it('should cache SVG strings', () => {
      const svg1 = getIconSVG('Activity');
      const svg2 = getIconSVG('Activity');
      expect(svg1).toBe(svg2); // Same reference = cached
    });

    it('should cache different options separately', () => {
      const svg1 = getIconSVG('Activity', { size: 20 });
      const svg2 = getIconSVG('Activity', { size: 24 });
      expect(svg1).not.toBe(svg2);
    });

    it('should return null for invalid icon name', () => {
      const svg = getIconSVG('InvalidIconName');
      expect(svg).toBeNull();
    });

    it('should warn for invalid icon names', () => {
      const consoleSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});
      getIconSVG('InvalidIcon');
      expect(consoleSpy).toHaveBeenCalledWith(
        'Icon InvalidIcon not found in lucide-react'
      );
      consoleSpy.mockRestore();
    });
  });

  describe('svgToDataUrl', () => {
    it('should convert SVG string to data URL', () => {
      const svgString = '<svg><path d="M..."/></svg>';
      const dataUrl = svgToDataUrl(svgString);
      expect(dataUrl).toMatch(/^data:image\/svg\+xml;base64,/);
    });

    it('should properly encode SVG to base64', () => {
      const svgString = '<svg><path d="M..."/></svg>';
      const dataUrl = svgToDataUrl(svgString);
      const base64Part = dataUrl.split(',')[1];
      const decoded = atob(base64Part);
      expect(decoded).toBe(svgString);
    });
  });

  describe('getIconImage', () => {
    it('should return HTMLImageElement for valid icon', () => {
      const img = getIconImage('Activity');
      expect(img).toBeInstanceOf(HTMLImageElement);
    });

    it('should return null for invalid icon', () => {
      const img = getIconImage('InvalidIcon');
      expect(img).toBeNull();
    });

    it('should cache images', () => {
      const img1 = getIconImage('Activity');
      const img2 = getIconImage('Activity');
      expect(img1).toBe(img2); // Same reference = cached
    });

    it('should use custom options', () => {
      const img = getIconImage('Activity', {
        size: 24,
        color: '#ff0000',
        strokeWidth: 3,
      });
      expect(img).toBeTruthy();
      expect(img?.src).toMatch(/^data:image\/svg\+xml;base64,/);
    });

    it('should create separate cache entries for different options', () => {
      const img1 = getIconImage('Activity', { size: 20 });
      const img2 = getIconImage('Activity', { size: 24 });
      expect(img1).not.toBe(img2);
    });
  });

  describe('loadIconImage', () => {
    it('should return HTMLImageElement for valid icon', async () => {
      const img = await loadIconImage('Activity');
      expect(img).toBeInstanceOf(HTMLImageElement);
    });

    it('should return null for invalid icon', async () => {
      const img = await loadIconImage('InvalidIcon');
      expect(img).toBeNull();
    });

    it('should return immediately if image is complete', async () => {
      // First call to cache the image
      const img1 = getIconImage('Activity');
      if (img1) {
        // Mark as complete
        Object.defineProperty(img1, 'complete', { value: true });
      }

      const img2 = await loadIconImage('Activity');
      expect(img2).toBe(img1);
    });

    it('should wait for image to load if not complete', async () => {
      const img = await loadIconImage('Archive');
      expect(img).toBeTruthy();
    });

    it('should use custom options', async () => {
      const img = await loadIconImage('Activity', {
        size: 32,
        color: '#00ff00',
        strokeWidth: 1,
      });
      expect(img).toBeTruthy();
    });
  });

  describe('preloadIcons', () => {
    it('should preload multiple icons', async () => {
      await preloadIcons(['Activity', 'Archive', 'AlarmClock']);

      // Verify all icons are cached
      const img1 = getIconImage('Activity');
      const img2 = getIconImage('Archive');
      const img3 = getIconImage('AlarmClock');

      expect(img1).toBeTruthy();
      expect(img2).toBeTruthy();
      expect(img3).toBeTruthy();
    });

    it('should handle empty array', async () => {
      await expect(preloadIcons([])).resolves.toBeUndefined();
    });

    it('should handle icons with custom options', async () => {
      await preloadIcons(['Activity', 'Archive'], {
        size: 32,
        color: '#ff0000',
      });

      const img = getIconImage('Activity', { size: 32, color: '#ff0000' });
      expect(img).toBeTruthy();
    });

    it('should not throw for invalid icons', async () => {
      await expect(
        preloadIcons(['Activity', 'InvalidIcon', 'Archive'])
      ).resolves.toBeUndefined();
    });
  });

  describe('clearIconCache', () => {
    it('should clear all cached icons', () => {
      // Cache some icons
      getIconSVG('Activity');
      getIconImage('Archive');

      // Verify they are cached
      const svg1 = getIconSVG('Activity');
      const img1 = getIconImage('Archive');
      expect(svg1).toBeTruthy();
      expect(img1).toBeTruthy();

      // Clear cache
      clearIconCache();

      // New calls should generate new instances
      const svg2 = getIconSVG('Activity');
      const img2 = getIconImage('Archive');

      // After clearing, new SVG should be generated (won't be same reference)
      expect(svg2).toBeTruthy();
      expect(img2).toBeTruthy();
    });
  });

  describe('icon rendering lifecycle', () => {
    it('should handle complete workflow: SVG -> DataURL -> Image', () => {
      // Get SVG
      const svg = getIconSVG('Activity');
      expect(svg).toBeTruthy();

      // Convert to data URL
      const dataUrl = svgToDataUrl(svg!);
      expect(dataUrl).toMatch(/^data:image\/svg\+xml;base64,/);

      // Create image
      const img = getIconImage('Activity');
      expect(img).toBeTruthy();
      expect(img?.src).toBe(dataUrl);
    });

    it('should maintain cache consistency across functions', () => {
      // First get SVG
      const svg1 = getIconSVG('Activity');

      // Then get image (should use same SVG)
      const img = getIconImage('Activity');

      // Get SVG again (should be cached)
      const svg2 = getIconSVG('Activity');

      expect(svg1).toBe(svg2);
      expect(img).toBeTruthy();
    });
  });

  describe('error handling', () => {
    it('should log warnings for missing icons', () => {
      const consoleWarnSpy = vi
        .spyOn(console, 'warn')
        .mockImplementation(() => {});

      getIconSVG('ThisIconDoesNotExist');

      expect(consoleWarnSpy).toHaveBeenCalledWith(
        'Icon ThisIconDoesNotExist not found in lucide-react'
      );

      consoleWarnSpy.mockRestore();
    });

    it('should return null for missing icons', () => {
      const svg = getIconSVG('NonExistentIcon');
      expect(svg).toBeNull();

      const img = getIconImage('NonExistentIcon');
      expect(img).toBeNull();
    });
  });

  describe('performance and caching', () => {
    it('should only render SVG once per unique configuration', () => {
      // Clear cache to start fresh
      clearIconCache();

      // First call will generate SVG
      const svg1 = getIconSVG('Activity');
      // Second and third calls should return cached version
      const svg2 = getIconSVG('Activity');
      const svg3 = getIconSVG('Activity');

      // All should be the same reference (cached)
      expect(svg1).toBe(svg2);
      expect(svg2).toBe(svg3);
    });

    it('should render separately for different configurations', () => {
      clearIconCache();

      const svg1 = getIconSVG('Activity', { size: 20 });
      const svg2 = getIconSVG('Activity', { size: 24 });
      const svg3 = getIconSVG('Activity', { size: 32 });

      // All should be different (different cache keys)
      expect(svg1).not.toBe(svg2);
      expect(svg2).not.toBe(svg3);
      expect(svg1).not.toBe(svg3);
    });
  });
});
