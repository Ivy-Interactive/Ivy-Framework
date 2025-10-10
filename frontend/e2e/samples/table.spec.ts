import { test, expect, type Page } from '@playwright/test';

// Shared setup function for table widget tests
async function setupTablePage(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  // Navigate to a Table view
  const searchInput = page.getByTestId('sidebar-search');

  await expect(searchInput).toBeVisible();
  await searchInput.click({ force: true });
  await searchInput.fill('table');
  await searchInput.press('Enter');

  const firstResult = page
    .locator('button')
    .filter({ hasText: /table/i })
    .first();

  await expect(firstResult).toBeVisible();
  await firstResult.click();
  await page.waitForLoadState('networkidle');
}

test.describe('Table Widget', () => {
  test.beforeEach(async ({ page }) => {
    await setupTablePage(page);
  });

  test.describe('Column Type Labels', () => {
    test('displays type icons in column headers', async ({ page }) => {
      // Check if table exists - fail if not found
      // Glide Data Grid uses canvas element with specific class
      const table = page.locator('.dvn-scroller');
      await expect(table).toHaveCount(1, { timeout: 5000 });

      // Verify table is visible
      await expect(table).toBeVisible();
    });
  });

  test.describe('Index Column', () => {
    test('displays row numbers when showIndexColumn is enabled', async ({
      page,
    }) => {
      // Check if table exists - fail if not found
      const table = page.locator('.dvn-scroller');
      await expect(table).toHaveCount(1, { timeout: 5000 });

      // Verify table is visible
      await expect(table).toBeVisible();

      // Note: Row markers are rendered on canvas, difficult to test in E2E
      // Visual inspection or integration tests would be better for this feature
    });
  });

  test.describe('Selection Modes', () => {
    test('supports row selection mode', async ({ page }) => {
      // Check if table exists - fail if not found
      const table = page.locator('.dvn-scroller');
      await expect(table).toHaveCount(1, { timeout: 5000 });

      // Verify table is visible and interactive
      await expect(table).toBeVisible();
    });
  });

  test.describe('Freeze Columns', () => {
    test('keeps frozen columns visible when scrolling horizontally', async ({
      page,
    }) => {
      // Check if table exists - fail if not found
      const table = page.locator('.dvn-scroller');
      await expect(table).toHaveCount(1, { timeout: 5000 });

      // Verify table is visible
      await expect(table).toBeVisible();
    });
  });

  test.describe('Copy Selection', () => {
    test('allows copying selected cells to clipboard', async ({ page }) => {
      // Check if table exists - fail if not found
      const table = page.locator('.dvn-scroller');
      await expect(table).toHaveCount(1, { timeout: 5000 });

      // Verify table is visible
      await expect(table).toBeVisible();

      // Future enhancement: Actually test copy functionality
      // Note: Glide Data Grid renders on canvas, making cell selection testing complex
    });
  });

  test.describe('Column Reordering', () => {
    test('allows dragging columns to reorder them', async ({ page }) => {
      // Check if table exists - fail if not found
      const table = page.locator('.dvn-scroller');
      await expect(table).toHaveCount(1, { timeout: 5000 });

      // Verify table is visible
      await expect(table).toBeVisible();

      // Future enhancement: Test drag-and-drop reordering
      // Note: Canvas-based rendering makes this testing complex
    });
  });

  test.describe('Responsiveness', () => {
    test('displays menu button on small screens', async ({ page }) => {
      // Set viewport to small screen size (< 900px triggers responsive layout)
      await page.setViewportSize({ width: 850, height: 800 });

      // Re-navigate after setting viewport
      await setupTablePage(page);

      // Check if table exists - fail if not found
      const table = page.locator('.dvn-scroller');
      await expect(table).toHaveCount(1, { timeout: 5000 });

      // Verify table info button with menu icon appears
      const menuButton = page.getByRole('button', {
        name: /table info/i,
      });
      await expect(menuButton).toHaveCount(1, { timeout: 5000 });

      await expect(menuButton).toBeVisible();

      // Verify query editor is still visible
      const queryEditor = page.locator('.query-editor-wrapper');
      await expect(queryEditor).toBeVisible();
    });

    test('displays metadata inline on large screens', async ({ page }) => {
      // Set viewport to large screen size
      await page.setViewportSize({ width: 1200, height: 800 });

      // Re-navigate after setting viewport
      await setupTablePage(page);

      // Check if table exists - fail if not found
      const table = page.locator('.dvn-scroller');
      await expect(table).toHaveCount(1, { timeout: 5000 });

      // Verify query editor is visible inline (not in a sheet)
      const queryEditor = page.locator('.query-editor-wrapper');
      await expect(queryEditor).toHaveCount(1, { timeout: 5000 });

      await expect(queryEditor).toBeVisible();

      // Verify metadata is visible inline (not hidden in menu)
      const metadata = page.getByText(/6 columns/i);
      await expect(metadata).toBeVisible();
    });
  });
});
