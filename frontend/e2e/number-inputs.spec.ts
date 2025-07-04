import { test, expect } from '@playwright/test';

test.describe('Number Inputs - Variants Section', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/app');
    await page.waitForLoadState('networkidle');

    // Navigate to Number Inputs app
    const searchInput = page.getByTestId('sidebar-search');
    await expect(searchInput).toBeVisible();
    await searchInput.click();
    await searchInput.fill('number input');
    await searchInput.press('Enter');

    const firstResult = page.locator('[data-sidebar="menu-item"], [data-sidebar="menu-sub-item"]')
      .filter({ hasText: /Number Input/i }).first();
    
    await expect(firstResult).toBeVisible();
    await firstResult.click();
    await page.waitForLoadState('networkidle');

    // Wait for the app frame to load
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();
  });

  test('should display all variants in the grid', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Check that the variants section exists
    await expect(appFrame!.locator('h2').filter({ hasText: 'Variants' })).toBeVisible();

    await expect(appFrame!.getByTestId('number-input-nullable-main')).toBeVisible();
    await expect(appFrame!.getByTestId('number-input-int-main')).toBeVisible();
    await expect(appFrame!.getByTestId('number-input-int-disabled-main')).toBeVisible();
    await expect(appFrame!.getByTestId('number-input-int-invalid-main')).toBeVisible();


    await expect(appFrame!.getByTestId('number-input-nullable-slider-main')).toBeVisible();
    await expect(appFrame!.getByTestId('number-input-int-slider-main')).toBeVisible();
    await expect(appFrame!.getByTestId('number-input-int-disabled-slider-main')).toBeVisible();
    await expect(appFrame!.getByTestId('number-input-int-invalid-slider-main')).toBeVisible();
  });

}); 