import { test, expect } from '@playwright/test';

test('debug sidebar search', async ({ page }) => {
  await page.goto('/app');
  await page.waitForLoadState('networkidle');

  // Find the sidebar search input
  const searchInput = page.getByTestId('sidebar-search');
  await expect(searchInput).toBeVisible();

  // Click the search input
  await searchInput.click();
  // Type 'bool'
  await searchInput.fill('bool');
  // Press Enter
  await searchInput.press('Enter');

  const firstResult = page
    .locator('[data-sidebar="menu-item"], [data-sidebar="menu-sub-item"]')
    .filter({ hasText: /Bool Input/i })
    .first();

  await expect(firstResult).toBeVisible();
  await firstResult.click();

  // Wait for the page to load
  await page.waitForLoadState('networkidle');

  // Check that we can see the main content
  await expect(page.locator('body')).toBeVisible();

  // Look for h1 saying "Bool Input"
  const appFrameElement = await page.waitForSelector(
    'iframe[src*="bool-input"]'
  );
  const appFrame = await appFrameElement.contentFrame();
  expect(appFrame).not.toBeNull();

  // Now look for the h1 in the app frame
  const h1 = appFrame!.locator('h1');
  await expect(h1).toContainText('BoolInput');
});
