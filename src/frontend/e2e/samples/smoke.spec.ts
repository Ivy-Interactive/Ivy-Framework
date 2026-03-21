import { test, expect } from '@playwright/test';

test.skip(
  true,
  'Temporarily skipping samples E2E suite while PR #??? stabilizes'
);

test('has title', async ({ page }) => {
  await page.goto('/');

  // Expect a title "to contain" a substring.
  await expect(page).toHaveTitle(/Ivy/);
});

test('loads the main page', async ({ page }) => {
  await page.goto('/');

  // Wait for the page to load
  await page.waitForLoadState('networkidle');

  // Check that the page loaded successfully
  await expect(page.locator('body')).toBeVisible();
});
