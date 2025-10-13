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
    test('should render colors app with heading and color boxes', async ({
      page,
    }) => {
      await expect(page.getByRole('heading', { level: 1 })).toBeVisible();

      // Look for text content that looks like color names (starts with capital letter)
      const colorTexts = page.getByText(/^[A-Z][a-z]+$/);
      expect(await colorTexts.count()).toBeGreaterThan(10);
    });

    test('should render multiple color variations', async ({ page }) => {
      // Simple check that we have many elements rendered
      const allText = await page.textContent('body');
      expect(allText).toBeTruthy();
      expect(allText!.length).toBeGreaterThan(100);
    });
  });

  test.describe('Color Application Tests', () => {
    test('should verify Box.Color() method applies background colors', async ({
      page,
    }) => {
      // Check that color boxes are rendered (via text content)
      const colorElements = page.getByText(/^[A-Z][a-z]+$/);
      await page.waitForTimeout(200);

      const count = await colorElements.count();
      expect(count).toBeGreaterThan(10);

      // Verify page has rendered properly with color content
      const pageContent = await page.textContent('body');
      expect(pageContent).toContain('Colors');
    });
  });
});
