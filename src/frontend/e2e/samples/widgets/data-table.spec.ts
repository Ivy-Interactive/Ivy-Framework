import { test, expect } from '@playwright/test';

test.describe('DataTable Widget Tests', () => {
  test.skip('should navigate to data table page and find element by testId', async ({
    page,
  }) => {
    await page.goto('/widgets/data-table?chrome=false');
    await page.waitForLoadState('networkidle');
    const dataTable = page.getByTestId('data-table');
    await expect(dataTable).toBeVisible();
  });
});
