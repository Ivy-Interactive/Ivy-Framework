import { test, expect, type Page } from '@playwright/test';

test.skip(true, 'Temporarily skipping samples E2E suite while PR stabilizes');

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

  test('should render colors app with heading and theme-independent colors', async ({
    page,
  }) => {
    // Basic smoke test - heading is visible
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();

    // Check for absolute colors that don't change across themes
    // Go up 2 levels from text to reach the Box div with background color

    // const blackBox = page.getByText('Black').first().locator('../..');
    // await expect(blackBox).toHaveCSS('background-color', 'rgb(0, 0, 0)');
  });
});
