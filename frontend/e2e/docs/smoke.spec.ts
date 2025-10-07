import { test, expect } from '@playwright/test';

test.describe('Ivy.Docs Smoke Tests', () => {
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

  test('loads documentation content', async ({ page }) => {
    await page.goto('/');

    // Wait for the page to load
    await page.waitForLoadState('networkidle');

    // Check for common documentation elements
    // This may need to be adjusted based on the actual Ivy.Docs structure
    const mainContent = page
      .locator('main, [role="main"], .content, #content')
      .first();
    await expect(mainContent).toBeVisible();

    // Check that there's some text content loaded
    const textContent = await mainContent.textContent();
    expect(textContent).toBeTruthy();
    expect(textContent!.length).toBeGreaterThan(100); // Ensure there's substantial content
  });

  test('sidebar or navigation is visible', async ({ page }) => {
    await page.goto('/');

    // Wait for the page to load
    await page.waitForLoadState('networkidle');

    // Check for navigation elements - adjust selectors based on actual Ivy.Docs structure
    const navigation = page
      .locator('nav, aside, .sidebar, .navigation, [role="navigation"]')
      .first();
    await expect(navigation).toBeVisible();
  });
});
