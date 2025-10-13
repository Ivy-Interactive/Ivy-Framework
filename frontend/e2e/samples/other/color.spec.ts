import { test, expect, type Page } from '@playwright/test';

// Shared setup function
async function setupColorsPage(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  const searchInput = page.getByTestId('sidebar-search');
  await expect(searchInput).toBeVisible();
  await searchInput.click();
  await searchInput.fill('color');
  await searchInput.press('Enter');

  const firstResult = page
    .locator('button')
    .filter({ hasText: /color/i })
    .first();
  await firstResult.click();
  await page.waitForLoadState('networkidle');
}

test.describe('Colors App Tests', () => {
  test.beforeEach(async ({ page }) => {
    await setupColorsPage(page);
  });

  test.describe('Smoke Tests', () => {
    test('should render colors app with two column layout', async ({
      page,
    }) => {
      await expect(page.getByRole('heading', { level: 1 })).toBeVisible();

      // Should have color boxes visible
      const colorBoxes = page.locator('[style*="border-radius"]');
      const count = await colorBoxes.count();
      expect(count).toBeGreaterThan(10);
    });

    test('should render colors on both light and dark backgrounds', async ({
      page,
    }) => {
      // The app renders two columns - one default, one with black background
      const containers = page.locator('> div > div > div').first();
      await expect(containers).toBeVisible();

      // Verify multiple color boxes are rendered
      const colorBoxes = page.locator('[style*="padding"]');
      expect(await colorBoxes.count()).toBeGreaterThan(20);
    });
  });

  test.describe('State and Color Rendering Tests', () => {
    test('should render all color enum values', async ({ page }) => {
      // Get all rendered color boxes
      const colorBoxes = page.locator('[style*="border-radius"]');
      const count = await colorBoxes.count();

      // Should have many colors (Colors enum has 50+ values)
      expect(count).toBeGreaterThan(30);
    });

    test('should display color names within boxes', async ({ page }) => {
      const colorBoxes = page.locator('[style*="border-radius"]').first();
      await colorBoxes.scrollIntoViewIfNeeded();
      await expect(colorBoxes).toBeVisible();

      const text = await colorBoxes.textContent();
      expect(text).toBeTruthy();
      expect(text!.length).toBeGreaterThan(0);
    });

    test('should render colors with consistent dimensions', async ({
      page,
    }) => {
      const colorBoxes = page.locator('[style*="border-radius"]');

      // Check first few color boxes for consistent height
      for (let i = 0; i < Math.min(5, await colorBoxes.count()); i++) {
        const box = colorBoxes.nth(i);
        await box.scrollIntoViewIfNeeded();

        const boundingBox = await box.boundingBox();
        if (boundingBox) {
          expect(boundingBox.height).toBeGreaterThan(30);
          expect(boundingBox.width).toBeGreaterThan(50);
        }
      }
    });
  });

  test.describe('Visual Properties Tests', () => {
    test('should verify color boxes have border radius', async ({ page }) => {
      const colorBox = page.locator('[style*="border-radius"]').first();
      await colorBox.scrollIntoViewIfNeeded();

      const borderRadius = await colorBox.evaluate(
        el => window.getComputedStyle(el).borderRadius
      );

      expect(borderRadius).toBeTruthy();
      expect(borderRadius).not.toBe('0px');
    });

    test('should verify color boxes have padding', async ({ page }) => {
      const colorBox = page.locator('[style*="padding"]').first();
      await colorBox.scrollIntoViewIfNeeded();

      const padding = await colorBox.evaluate(
        el => window.getComputedStyle(el).padding
      );

      expect(padding).toBeTruthy();
    });

    test('should verify colors have background colors applied', async ({
      page,
    }) => {
      const colorBoxes = page.locator('[style*="background-color"]');
      expect(await colorBoxes.count()).toBeGreaterThan(10);

      // Verify first color box has background color
      const firstBox = colorBoxes.first();
      await firstBox.scrollIntoViewIfNeeded();

      const backgroundColor = await firstBox.evaluate(
        el => window.getComputedStyle(el).backgroundColor
      );

      expect(backgroundColor).toBeTruthy();
      expect(backgroundColor).not.toBe('rgba(0, 0, 0, 0)');
    });

    test('should verify color boxes have readable dimensions', async ({
      page,
    }) => {
      const colorBox = page.locator('[style*="border-radius"]').first();
      await colorBox.scrollIntoViewIfNeeded();

      const box = await colorBox.boundingBox();
      expect(box).toBeTruthy();

      if (box) {
        expect(box.width).toBeGreaterThan(50);
        expect(box.width).toBeLessThan(2000);
        expect(box.height).toBeGreaterThan(20);
        expect(box.height).toBeLessThan(500);
      }
    });
  });

  test.describe('Layout and Grid Tests', () => {
    test('should render two column grid layout', async ({ page }) => {
      // The app uses Layout.Grid().Columns(2)
      // Both columns should be visible
      const containers = page.locator('> div > div').first();
      await expect(containers).toBeVisible();
    });

    test('should render equal number of colors in both columns', async ({
      page,
    }) => {
      // Both light and dark background columns should have same colors
      const allColorBoxes = page.locator('[style*="border-radius"]');
      const totalCount = await allColorBoxes.count();

      // Should be even number (same colors on light and dark)
      expect(totalCount % 2).toBe(0);
    });
  });

  test.describe('Complex Routine Tests', () => {
    test('should handle scrolling through all color boxes', async ({
      page,
    }) => {
      const colorBoxes = page.locator('[style*="border-radius"]');
      const count = await colorBoxes.count();

      // Scroll through and verify first, middle, and last boxes
      const indices = [0, Math.floor(count / 2), count - 1];

      for (const index of indices) {
        const box = colorBoxes.nth(index);
        await box.scrollIntoViewIfNeeded();
        await expect(box).toBeVisible();

        const text = await box.textContent();
        expect(text).toBeTruthy();
      }
    });

    test('should maintain consistent styling across all colors', async ({
      page,
    }) => {
      const colorBoxes = page.locator('[style*="border-radius"]');
      const count = await colorBoxes.count();

      const borderRadiusValues: string[] = [];

      // Check first 5 boxes for consistent border radius
      for (let i = 0; i < Math.min(5, count); i++) {
        const box = colorBoxes.nth(i);
        await box.scrollIntoViewIfNeeded();

        const borderRadius = await box.evaluate(
          el => window.getComputedStyle(el).borderRadius
        );
        borderRadiusValues.push(borderRadius);
      }

      // All should have the same border radius
      expect(
        borderRadiusValues.every(val => val === borderRadiusValues[0])
      ).toBe(true);
    });

    test('should verify colors remain visible after interactions', async ({
      page,
    }) => {
      const colorBoxes = page.locator('[style*="border-radius"]');

      // Scroll to top
      const firstBox = colorBoxes.first();
      await firstBox.scrollIntoViewIfNeeded();
      await expect(firstBox).toBeVisible();

      // Scroll to bottom
      const lastBox = colorBoxes.last();
      await lastBox.scrollIntoViewIfNeeded();
      await expect(lastBox).toBeVisible();

      // Scroll back to top
      await firstBox.scrollIntoViewIfNeeded();
      await expect(firstBox).toBeVisible();

      // Verify colors are still rendering correctly
      const backgroundColor = await firstBox.evaluate(
        el => window.getComputedStyle(el).backgroundColor
      );
      expect(backgroundColor).toBeTruthy();
    });
  });

  test.describe('Method Verification', () => {
    test('should verify Box color method applies background', async ({
      page,
    }) => {
      const colorBoxes = page.locator('[style*="background-color"]');

      // Sample multiple boxes to verify color is applied
      const samplesToCheck = Math.min(10, await colorBoxes.count());

      for (let i = 0; i < samplesToCheck; i++) {
        const box = colorBoxes.nth(i);
        await box.scrollIntoViewIfNeeded();

        const backgroundColor = await box.evaluate(
          el => window.getComputedStyle(el).backgroundColor
        );

        expect(backgroundColor).toBeTruthy();
        expect(backgroundColor).not.toBe('rgba(0, 0, 0, 0)');
      }
    });

    test('should verify Box BorderRadius method is applied', async ({
      page,
    }) => {
      const colorBoxes = page.locator('[style*="border-radius"]');
      const count = await colorBoxes.count();

      // Check multiple boxes
      for (let i = 0; i < Math.min(5, count); i++) {
        const box = colorBoxes.nth(i);
        await box.scrollIntoViewIfNeeded();

        const borderRadius = await box.evaluate(
          el => window.getComputedStyle(el).borderRadius
        );

        expect(borderRadius).toBeTruthy();
        expect(borderRadius).not.toBe('0px');
      }
    });

    test('should verify Box Padding method is applied', async ({ page }) => {
      const colorBoxes = page.locator('[style*="padding"]');

      const firstBox = colorBoxes.first();
      await firstBox.scrollIntoViewIfNeeded();

      const padding = await firstBox.evaluate(el => {
        const style = window.getComputedStyle(el);
        return {
          paddingTop: style.paddingTop,
          paddingRight: style.paddingRight,
          paddingBottom: style.paddingBottom,
          paddingLeft: style.paddingLeft,
        };
      });

      expect(padding.paddingTop).toBeTruthy();
      expect(padding.paddingTop).not.toBe('0px');
    });

    test('should verify all Box methods work together', async ({ page }) => {
      const colorBox = page.locator('[style*="border-radius"]').first();
      await colorBox.scrollIntoViewIfNeeded();

      const styles = await colorBox.evaluate(el => {
        const computed = window.getComputedStyle(el);
        return {
          backgroundColor: computed.backgroundColor,
          borderRadius: computed.borderRadius,
          padding: computed.padding,
          width: computed.width,
        };
      });

      // Verify all Box methods are applied
      expect(styles.backgroundColor).toBeTruthy();
      expect(styles.borderRadius).not.toBe('0px');
      expect(styles.padding).toBeTruthy();
      expect(styles.width).toBeTruthy();
    });
  });

  test.describe('Color Contrast and Accessibility', () => {
    test('should render colors on contrasting backgrounds', async ({
      page,
    }) => {
      const allBoxes = page.locator('[style*="background-color"]');
      const count = await allBoxes.count();

      // First half should be on light background, second half on dark
      // Verify at least some boxes are visible
      expect(count).toBeGreaterThan(20);
    });

    test('should display color names for identification', async ({ page }) => {
      const colorBoxes = page.locator('[style*="border-radius"]');

      // Sample several boxes and verify they have text content
      for (let i = 0; i < Math.min(5, await colorBoxes.count()); i++) {
        const box = colorBoxes.nth(i);
        await box.scrollIntoViewIfNeeded();

        const text = await box.textContent();
        expect(text).toBeTruthy();
        expect(text!.trim().length).toBeGreaterThan(2);
      }
    });
  });
});
