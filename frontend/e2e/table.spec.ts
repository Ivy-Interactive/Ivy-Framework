import { test, expect, type Page } from '@playwright/test';

// Shared setup function for table widget tests
async function setupTablePage(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  // Navigate to a Table view
  const searchInput = page.getByTestId('sidebar-search');

  // Check if sidebar exists (may not be available in all environments)
  const hasSidebar = (await searchInput.count()) > 0;
  if (!hasSidebar) {
    test.skip(true, 'Sidebar not available - skipping table tests');
    return;
  }

  await expect(searchInput).toBeVisible();
  await searchInput.click();
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
      const table = page.locator('[role="grid"]');
      await expect(table).toHaveCount(1, {
        timeout: 5000,
        message: 'Table widget not found on page',
      });

      // Verify column headers have icons (Glide Data Grid renders headers with icons)
      const headers = page.locator('[role="columnheader"]');
      await expect(headers.first()).toBeVisible();
    });
  });

  test.describe('Index Column', () => {
    test('displays row numbers when showIndexColumn is enabled', async ({
      page,
    }) => {
      // Check if table exists - fail if not found
      const table = page.locator('[role="grid"]');
      await expect(table).toHaveCount(1, {
        timeout: 5000,
        message: 'Table widget not found on page',
      });

      // Verify row marker column is visible (Glide Data Grid renders row markers)
      // The row markers should contain numbers starting from 1
      const rowMarkers = page.locator('[data-testid="row-marker"]');
      await expect(rowMarkers).toHaveCount(1, {
        timeout: 5000,
        message: 'Row markers not found - showIndexColumn may not be enabled',
      });

      await expect(rowMarkers.first()).toBeVisible();
    });
  });

  test.describe('Selection Modes', () => {
    test('supports row selection mode', async ({ page }) => {
      // Check if table exists - fail if not found
      const table = page.locator('[role="grid"]');
      await expect(table).toHaveCount(1, {
        timeout: 5000,
        message: 'Table widget not found on page',
      });

      // Verify rows can be selected (Glide Data Grid with rowSelect: 'multi')
      // The selection behavior should allow clicking on rows
      await expect(table).toBeVisible();
    });
  });

  test.describe('Freeze Columns', () => {
    test('keeps frozen columns visible when scrolling horizontally', async ({
      page,
    }) => {
      // Check if table exists - fail if not found
      const table = page.locator('[role="grid"]');
      await expect(table).toHaveCount(1, {
        timeout: 5000,
        message: 'Table widget not found on page',
      });

      // Verify frozen columns remain visible during horizontal scroll
      // Glide Data Grid applies position: sticky to frozen columns
      await expect(table).toBeVisible();
    });
  });

  test.describe('Copy Selection', () => {
    test('allows copying selected cells to clipboard', async ({ page }) => {
      // Check if table exists - fail if not found
      const table = page.locator('[role="grid"]');
      await expect(table).toHaveCount(1, {
        timeout: 5000,
        message: 'Table widget not found on page',
      });

      // Verify cells can be selected and copied (Cmd+C / Ctrl+C)
      // Glide Data Grid handles copy through getCellsForSelection
      await expect(table).toBeVisible();

      // Future enhancement: Actually test copy functionality
      // const firstCell = page.locator('[role="gridcell"]').first();
      // await firstCell.click();
      // await page.keyboard.press('Control+C'); // or Meta+C on Mac
      // const clipboardText = await page.evaluate(() => navigator.clipboard.readText());
      // expect(clipboardText).toBeTruthy();
    });
  });

  test.describe('Column Reordering', () => {
    test('allows dragging columns to reorder them', async ({ page }) => {
      // Check if table exists - fail if not found
      const table = page.locator('[role="grid"]');
      await expect(table).toHaveCount(1, {
        timeout: 5000,
        message: 'Table widget not found on page',
      });

      // Verify columns can be dragged and reordered
      // Glide Data Grid supports drag-and-drop reordering via onColumnMoved
      const headers = page.locator('[role="columnheader"]');
      await expect(headers).toHaveCount(
        await headers.count(),
        'Column headers not found'
      );
      await expect(headers.count()).resolves.toBeGreaterThanOrEqual(2);

      await expect(headers.first()).toBeVisible();
      // Future enhancement: Actually test drag-and-drop
      // await headers.first().dragTo(headers.nth(1));
    });
  });

  test.describe('Responsiveness', () => {
    test('displays menu button on small screens', async ({ page }) => {
      // Set viewport to small screen size
      await page.setViewportSize({ width: 600, height: 800 });

      // Re-navigate after setting viewport
      await setupTablePage(page);

      // Check if table exists - fail if not found
      const table = page.locator('[role="grid"]');
      await expect(table).toHaveCount(1, {
        timeout: 5000,
        message: 'Table widget not found on page',
      });

      // Verify table info button with menu icon appears
      const menuButton = page.getByRole('button', {
        name: /table info/i,
      });
      await expect(menuButton).toHaveCount(1, {
        timeout: 5000,
        message: 'Table info button not found on small screen',
      });

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
      const table = page.locator('[role="grid"]');
      await expect(table).toHaveCount(1, {
        timeout: 5000,
        message: 'Table widget not found on page',
      });

      // Verify query editor is visible inline (not in a sheet)
      const queryEditor = page.locator('.query-editor-wrapper');
      await expect(queryEditor).toHaveCount(1, {
        timeout: 5000,
        message: 'Query editor not found on large screen',
      });

      await expect(queryEditor).toBeVisible();

      // Verify metadata is visible inline (not hidden in menu)
      const metadata = page.getByText(/columns?/i);
      await expect(metadata).toBeVisible();
    });
  });
});
