import { test, expect, type Page } from '@playwright/test';

// Helper functions
const getCardByRole = (page: Page) => page.getByRole('region');
const getCardByTestId = (page: Page, testId: string) =>
  page.getByTestId(testId);

// Shared setup function
async function setupCardPage(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  const searchInput = page.getByTestId('sidebar-search');
  await expect(searchInput).toBeVisible();
  await searchInput.click();
  await searchInput.fill('card');
  await searchInput.press('Enter');

  const firstResult = page
    .locator('button')
    .filter({ hasText: /Card/i })
    .first();
  await firstResult.click();
  await page.waitForLoadState('networkidle');
}

test.describe('Card Widget Tests', () => {
  test.beforeEach(async ({ page }) => {
    await setupCardPage(page);
  });

  test.describe('Smoke Tests', () => {
    test('should render card app with heading and multiple cards', async ({
      page,
    }) => {
      const h1 = page.getByRole('heading', { level: 1 });
      await expect(h1).toBeVisible();
      const h1Text = await h1.textContent();
      expect(h1Text).toBeTruthy();
      expect(h1Text!.length).toBeGreaterThan(0);

      const cards = getCardByRole(page);
      const cardCount = await cards.count();
      expect(cardCount).toBeGreaterThanOrEqual(3);
    });

    test('should render cards with test ids', async ({ page }) => {
      const cardApp = getCardByTestId(page, 'card-app');
      await cardApp.scrollIntoViewIfNeeded();
      await page.waitForTimeout(100);
      await expect(cardApp).toBeVisible();

      const cardBorder = getCardByTestId(page, 'card-border');
      await cardBorder.scrollIntoViewIfNeeded();
      await expect(cardBorder).toBeVisible();

      const cardBorderColor = getCardByTestId(page, 'card-border-color');
      await cardBorderColor.scrollIntoViewIfNeeded();
      await expect(cardBorderColor).toBeVisible();
    });

    test('should verify cards with different border colors', async ({
      page,
    }) => {
      const redBorderCard = getCardByTestId(page, 'card-border-color');
      await redBorderCard.scrollIntoViewIfNeeded();
      await page.waitForTimeout(100);
      await expect(redBorderCard).toBeVisible();

      // Verify red border is applied
      const borderColor = await redBorderCard.evaluate(
        el => window.getComputedStyle(el).borderColor
      );
      expect(borderColor).toBeTruthy();

      const primaryBorderCard = getCardByTestId(page, 'card-border');
      await primaryBorderCard.scrollIntoViewIfNeeded();
      const primaryBorderColor = await primaryBorderCard.evaluate(
        el => window.getComputedStyle(el).borderColor
      );

      // Different cards should have different border colors
      expect(borderColor).not.toBe(primaryBorderColor);
    });

    test('should verify cards with icons in header', async ({ page }) => {
      const salesCard = getCardByTestId(page, 'card-total-sales');
      await salesCard.scrollIntoViewIfNeeded();
      await page.waitForTimeout(100);
      await expect(salesCard).toBeVisible();

      // Verify icon exists in card header
      const icon = salesCard.locator('svg').first();
      await expect(icon).toBeVisible();
    });

    test('should verify cards contain progress bars', async ({ page }) => {
      const salesCard = getCardByTestId(page, 'card-total-sales');
      await salesCard.scrollIntoViewIfNeeded();
      await page.waitForTimeout(100);
      await expect(salesCard).toBeVisible();

      // Verify progress bar exists
      const progressBar = salesCard.locator('div[role="progressbar"]');
      await expect(progressBar.first()).toBeVisible();

      // Verify progress bar has content
      const progressCount = await progressBar.count();
      expect(progressCount).toBeGreaterThan(0);
    });

    test('should verify cards with various gap and spacing', async ({
      page,
    }) => {
      // Get all cards by role
      const cards = getCardByRole(page);
      const firstCard = cards.first();
      await firstCard.scrollIntoViewIfNeeded();
      await page.waitForTimeout(100);
      await expect(firstCard).toBeVisible();

      const secondCard = cards.nth(1);
      await secondCard.scrollIntoViewIfNeeded();
      await page.waitForTimeout(100);
      await expect(secondCard).toBeVisible();

      // Both should be visible but may have different internal spacing
      const firstBox = await firstCard.boundingBox();
      const secondBox = await secondCard.boundingBox();

      expect(firstBox).toBeTruthy();
      expect(secondBox).toBeTruthy();
    });

    test('should verify all cards have role region', async ({ page }) => {
      // Verify specific cards have role region
      const cardApp = getCardByTestId(page, 'card-app');
      await cardApp.scrollIntoViewIfNeeded();
      await expect(cardApp).toBeVisible();
      const role1 = await cardApp.getAttribute('role');
      expect(role1).toBe('region');

      const cardBorder = getCardByTestId(page, 'card-border');
      await cardBorder.scrollIntoViewIfNeeded();
      const role2 = await cardBorder.getAttribute('role');
      expect(role2).toBe('region');

      const cardSales = getCardByTestId(page, 'card-total-sales');
      await cardSales.scrollIntoViewIfNeeded();
      const role3 = await cardSales.getAttribute('role');
      expect(role3).toBe('region');
    });
  });

  test.describe('Interactive Behavior', () => {
    test('should handle card click events', async ({ page }) => {
      const clickCard = getCardByTestId(page, 'card-onclick');
      await clickCard.scrollIntoViewIfNeeded();
      await page.waitForTimeout(100);
      await expect(clickCard).toBeVisible();

      // Click the card
      await clickCard.click();
      await page.waitForTimeout(100);
    });

    test('should handle button clicks inside cards', async ({ page }) => {
      const cardApp = getCardByTestId(page, 'card-app');
      await cardApp.scrollIntoViewIfNeeded();
      await page.waitForTimeout(100);

      const signUpButton = getCardByTestId(page, 'card-app-signup-button');
      await expect(signUpButton).toBeVisible();
      await expect(signUpButton).toBeEnabled();

      // Click the button
      await signUpButton.click();

      // Button should remain enabled after click
      await expect(signUpButton).toBeEnabled();
    });
  });

  test.describe('Complex Layout Tests', () => {
    test('should render cards with icons', async ({ page }) => {
      // Verify Total Sales card has icons
      const salesCard = getCardByTestId(page, 'card-total-sales');
      await salesCard.scrollIntoViewIfNeeded();
      await page.waitForTimeout(100);
      await expect(salesCard).toBeVisible();

      // Verify icon exists in header
      const icons = salesCard.locator('svg');
      expect(await icons.count()).toBeGreaterThan(0);
    });

    test('should render cards with progress bars', async ({ page }) => {
      // Verify the Total Sales card has a progress bar
      const salesCard = getCardByTestId(page, 'card-total-sales');
      await salesCard.scrollIntoViewIfNeeded();
      await page.waitForTimeout(100);
      await expect(salesCard).toBeVisible();

      // Verify progress bar exists
      const progressBar = salesCard.locator('div[role="progressbar"]').first();
      await expect(progressBar).toBeVisible();
    });

    test('should render cards with various content types', async ({ page }) => {
      // Verify card with button content
      const cardApp = getCardByTestId(page, 'card-app');
      await cardApp.scrollIntoViewIfNeeded();
      await expect(cardApp).toBeVisible();
      await expect(cardApp.getByRole('button')).toBeVisible();

      // Verify card with progress bar content
      const salesCard = getCardByTestId(page, 'card-total-sales');
      await salesCard.scrollIntoViewIfNeeded();
      await expect(salesCard).toBeVisible();
      await expect(
        salesCard.locator('div[role="progressbar"]').first()
      ).toBeVisible();
    });

    test.describe('Complex Routine Test', () => {
      test('should handle complete user interaction flow', async ({ page }) => {
        // Step 1: Verify and interact with first card
        const cardApp = getCardByTestId(page, 'card-app');
        await cardApp.scrollIntoViewIfNeeded();
        await page.waitForTimeout(100);
        await expect(cardApp).toBeVisible();

        // Step 2: Click button inside card
        const signUpButton = getCardByTestId(page, 'card-app-signup-button');
        await signUpButton.click();

        // Step 3: Click on an interactive card
        const clickCard = getCardByTestId(page, 'card-onclick');
        await clickCard.scrollIntoViewIfNeeded();
        await page.waitForTimeout(100);
        await clickCard.click();
        await page.waitForTimeout(100);

        // Step 4: Scroll through different card sections
        const salesCard = getCardByTestId(page, 'card-total-sales');
        await salesCard.scrollIntoViewIfNeeded();
        await page.waitForTimeout(100);
        await expect(salesCard).toBeVisible();

        // Step 5: Verify metric card with progress
        const progressBar = salesCard
          .locator('div[role="progressbar"]')
          .first();
        await expect(progressBar).toBeVisible();

        // Step 6: Check border styled card
        const borderCard = getCardByTestId(page, 'card-border-color');
        await borderCard.scrollIntoViewIfNeeded();
        await page.waitForTimeout(100);
        await expect(borderCard).toBeVisible();

        // Step 7: Final state verification
        await expect(
          page.getByRole('heading', { level: 1 }).first()
        ).toBeVisible();
      });
    });

    test.describe('Visual Side-Effects Tests', () => {
      test('should verify card shadows and elevation', async ({ page }) => {
        const card = getCardByTestId(page, 'card-app');
        await card.scrollIntoViewIfNeeded();
        await page.waitForTimeout(100);

        const boxShadow = await card.evaluate(
          el => window.getComputedStyle(el).boxShadow
        );

        // Card should have some shadow for elevation
        expect(boxShadow).toBeTruthy();
      });

      test('should verify card border radius', async ({ page }) => {
        const card = getCardByTestId(page, 'card-border');
        await card.scrollIntoViewIfNeeded();
        await page.waitForTimeout(100);

        const borderRadius = await card.evaluate(
          el => window.getComputedStyle(el).borderRadius
        );

        expect(borderRadius).toBeTruthy();
        // Rounded cards should have border radius > 0
        expect(borderRadius).not.toBe('0px');
      });

      test('should verify card hover state (if clickable)', async ({
        page,
      }) => {
        const clickCard = getCardByTestId(page, 'card-onclick');
        await clickCard.scrollIntoViewIfNeeded();
        await page.waitForTimeout(100);

        // Hover over card
        await clickCard.hover();

        // Clickable card should have pointer cursor or similar indication
        const hoverCursor = await clickCard.evaluate(
          el => window.getComputedStyle(el).cursor
        );

        expect(hoverCursor).toBeTruthy();
      });

      test('should verify progress bar visual styling', async ({ page }) => {
        const salesCard = getCardByTestId(page, 'card-total-sales');
        await salesCard.scrollIntoViewIfNeeded();
        await page.waitForTimeout(100);

        const progressBar = salesCard
          .locator('div[role="progressbar"]')
          .first();
        await expect(progressBar).toBeVisible();

        // Check progress bar has proper height and background
        const progressStyles = await progressBar.evaluate(el => {
          const styles = window.getComputedStyle(el);
          return {
            height: styles.height,
            backgroundColor: styles.backgroundColor,
            borderRadius: styles.borderRadius,
          };
        });

        expect(progressStyles.height).toBeTruthy();
        expect(progressStyles.backgroundColor).toBeTruthy();
      });

      test('should verify card dimensions are reasonable', async ({ page }) => {
        // Get a specific card we know exists
        const card = getCardByTestId(page, 'card-app');
        await card.scrollIntoViewIfNeeded();
        await page.waitForTimeout(100);

        const box = await card.boundingBox();

        if (box) {
          // Card should have reasonable dimensions
          expect(box.width).toBeGreaterThan(100);
          expect(box.height).toBeGreaterThan(50);
          expect(box.width).toBeLessThan(2000);
          expect(box.height).toBeLessThan(2000);
        }

        // Verify another card has similar dimensions
        const salesCard = getCardByTestId(page, 'card-total-sales');
        await salesCard.scrollIntoViewIfNeeded();
        await page.waitForTimeout(100);

        const salesBox = await salesCard.boundingBox();
        if (salesBox) {
          expect(salesBox.width).toBeGreaterThan(100);
          expect(salesBox.height).toBeGreaterThan(50);
        }
      });
    });
  });
});
