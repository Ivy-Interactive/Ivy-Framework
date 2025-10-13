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
      await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
      expect(await getCardByRole(page).count()).toBeGreaterThanOrEqual(3);
    });

    test('should render cards with test ids and correct role', async ({
      page,
    }) => {
      const testIds = ['card-app', 'card-border', 'card-border-color'];

      for (const testId of testIds) {
        const card = getCardByTestId(page, testId);
        await card.scrollIntoViewIfNeeded();
        await expect(card).toBeVisible();
        expect(await card.getAttribute('role')).toBe('region');
      }
    });

    test('should render cards with icons and progress bars', async ({
      page,
    }) => {
      const salesCard = getCardByTestId(page, 'card-total-sales');
      await salesCard.scrollIntoViewIfNeeded();
      await expect(salesCard).toBeVisible();
      await expect(salesCard.locator('svg').first()).toBeVisible();
      await expect(
        salesCard.locator('div[role="progressbar"]').first()
      ).toBeVisible();
    });
  });

  test.describe('State and Property Tests', () => {
    test('should render cards with different border property variations', async ({
      page,
    }) => {
      const cards = ['card-border', 'card-border-color'];
      const borderWidths: string[] = [];

      for (const id of cards) {
        const card = getCardByTestId(page, id);
        await card.scrollIntoViewIfNeeded();
        await expect(card).toBeVisible();

        const borderWidth = await card.evaluate(
          el => window.getComputedStyle(el).borderTopWidth
        );
        expect(borderWidth).toBeTruthy();
        borderWidths.push(borderWidth);
      }

      expect(borderWidths[0]).not.toBe(borderWidths[1]);
    });

    test('should render cards with optional properties present and absent', async ({
      page,
    }) => {
      // Card with all optional properties (title, description, icon, footer)
      const cardApp = getCardByTestId(page, 'card-app');
      await cardApp.scrollIntoViewIfNeeded();
      await expect(cardApp).toBeVisible();
      await expect(
        getCardByTestId(page, 'card-app-signup-button')
      ).toBeVisible();

      // Card with minimal properties (content only, no footer in some cards)
      const cards = getCardByRole(page);
      const count = await cards.count();
      expect(count).toBeGreaterThan(10);

      // Verify cards render successfully with various property combinations
      for (let i = 0; i < Math.min(count, 5); i++) {
        await expect(cards.nth(i)).toBeVisible();
      }
    });

    test('should handle different hover variant states', async ({ page }) => {
      const clickableCard = getCardByTestId(page, 'card-onclick');
      await clickableCard.scrollIntoViewIfNeeded();
      await clickableCard.hover();
      await expect(clickableCard).toHaveCSS('cursor', 'pointer');

      const nonClickableCard = getCardByTestId(page, 'card-border');
      await nonClickableCard.scrollIntoViewIfNeeded();
      const cursor = await nonClickableCard.evaluate(
        el => window.getComputedStyle(el).cursor
      );
      expect(cursor).not.toBe('pointer');
    });
  });

  test.describe('Interactive Behavior and State Updates', () => {
    test('should handle card and button click events with state persistence', async ({
      page,
    }) => {
      const clickCard = getCardByTestId(page, 'card-onclick');
      await clickCard.scrollIntoViewIfNeeded();
      await expect(clickCard).toBeVisible();

      // Click and verify state persists
      await clickCard.click();
      await page.waitForTimeout(100);
      await expect(clickCard).toBeVisible();
      expect(await clickCard.getAttribute('role')).toBe('region');

      const signUpButton = getCardByTestId(page, 'card-app-signup-button');
      await expect(signUpButton).toBeVisible();
      await expect(signUpButton).toBeEnabled();

      // Click button and verify state persists
      await signUpButton.click();
      await page.waitForTimeout(100);
      await expect(signUpButton).toBeEnabled();
      await expect(signUpButton).toBeVisible();
    });

    test('should maintain all properties after interactions', async ({
      page,
    }) => {
      const borderCard = getCardByTestId(page, 'card-border');
      await borderCard.scrollIntoViewIfNeeded();

      // Capture initial state
      const initialStyles = await borderCard.evaluate(el => {
        const s = window.getComputedStyle(el);
        return {
          borderStyle: s.borderStyle,
          borderWidth: s.borderTopWidth,
          borderRadius: s.borderRadius,
        };
      });

      // Interact with other cards
      const clickCard = getCardByTestId(page, 'card-onclick');
      await clickCard.click();
      await page.waitForTimeout(100);

      // Verify properties unchanged
      const afterStyles = await borderCard.evaluate(el => {
        const s = window.getComputedStyle(el);
        return {
          borderStyle: s.borderStyle,
          borderWidth: s.borderTopWidth,
          borderRadius: s.borderRadius,
        };
      });

      expect(afterStyles.borderStyle).toBe(initialStyles.borderStyle);
      expect(afterStyles.borderWidth).toBe(initialStyles.borderWidth);
      expect(afterStyles.borderRadius).toBe(initialStyles.borderRadius);
    });

    test('should verify hover cursor states', async ({ page }) => {
      const clickCard = getCardByTestId(page, 'card-onclick');
      await clickCard.scrollIntoViewIfNeeded();
      await clickCard.hover();
      await expect(clickCard).toHaveCSS('cursor', 'pointer');

      const nonClickCard = getCardByTestId(page, 'card-border');
      await nonClickCard.scrollIntoViewIfNeeded();
      const cursor = await nonClickCard.evaluate(
        el => window.getComputedStyle(el).cursor
      );
      expect(cursor).not.toBe('pointer');
    });

    test('should support keyboard navigation', async ({ page }) => {
      const signUpButton = getCardByTestId(page, 'card-app-signup-button');
      await signUpButton.scrollIntoViewIfNeeded();
      await signUpButton.focus();
      await expect(signUpButton).toBeFocused();
      await page.keyboard.press('Enter');
      await page.waitForTimeout(100);
      await expect(signUpButton).toBeEnabled();
      await expect(signUpButton).toBeVisible();
    });
  });

  test.describe('Complex Layout Tests', () => {
    test('should render cards with nested layouts and complex content', async ({
      page,
    }) => {
      const salesCard = getCardByTestId(page, 'card-total-sales');
      await salesCard.scrollIntoViewIfNeeded();
      await expect(salesCard).toBeVisible();
      expect(await salesCard.locator('svg').count()).toBeGreaterThan(0);
      await expect(
        salesCard.locator('div[role="progressbar"]').first()
      ).toBeVisible();

      const cardApp = getCardByTestId(page, 'card-app');
      await cardApp.scrollIntoViewIfNeeded();
      await expect(cardApp.getByRole('button')).toBeVisible();
    });

    test('should handle complete user interaction flow', async ({ page }) => {
      const testCards = [
        'card-app',
        'card-onclick',
        'card-border',
        'card-border-color',
        'card-total-sales',
      ];

      for (const testId of testCards) {
        const card = getCardByTestId(page, testId);
        await card.scrollIntoViewIfNeeded();
        await expect(card).toBeVisible();
      }

      const clickCard = getCardByTestId(page, 'card-onclick');
      await clickCard.click();
      await page.waitForTimeout(200);

      const signUpButton = getCardByTestId(page, 'card-app-signup-button');
      await signUpButton.click();
      await page.waitForTimeout(200);

      const salesCard = getCardByTestId(page, 'card-total-sales');
      await expect(
        salesCard.locator('div[role="progressbar"]').first()
      ).toBeVisible();
      expect(await salesCard.locator('svg').count()).toBeGreaterThan(0);

      await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
    });

    test('should verify visual properties', async ({ page }) => {
      const card = getCardByTestId(page, 'card-border');
      await card.scrollIntoViewIfNeeded();

      const boxShadow = await card.evaluate(
        el => window.getComputedStyle(el).boxShadow
      );
      expect(boxShadow).toBeTruthy();

      const borderRadius = await card.evaluate(
        el => window.getComputedStyle(el).borderRadius
      );
      expect(borderRadius).toBeTruthy();
      expect(borderRadius).not.toBe('0px');

      const box = await card.boundingBox();
      expect(box).toBeTruthy();
      if (box) {
        expect(box.width).toBeGreaterThan(100);
        expect(box.height).toBeGreaterThan(50);
      }
    });
  });

  test.describe('Method Verification', () => {
    test('should verify all card methods render correctly', async ({
      page,
    }) => {
      const borderCard = getCardByTestId(page, 'card-border');
      await borderCard.scrollIntoViewIfNeeded();
      await expect(borderCard).toBeVisible();

      const borderWidth = await borderCard.evaluate(
        el => window.getComputedStyle(el).borderTopWidth
      );
      expect(borderWidth).toBeTruthy();

      const borderRadius = await borderCard.evaluate(
        el => window.getComputedStyle(el).borderRadius
      );
      expect(borderRadius).not.toBe('0px');

      const clickCard = getCardByTestId(page, 'card-onclick');
      await clickCard.scrollIntoViewIfNeeded();
      await clickCard.hover();
      await expect(clickCard).toHaveCSS('cursor', 'pointer');
      await clickCard.click();
      await expect(clickCard).toBeVisible();

      const salesCard = getCardByTestId(page, 'card-total-sales');
      await salesCard.scrollIntoViewIfNeeded();
      expect(await salesCard.locator('svg').count()).toBeGreaterThan(0);
    });

    test('should verify method state updates persist through interactions', async ({
      page,
    }) => {
      const testCards = [
        { id: 'card-app', hasButton: true },
        { id: 'card-onclick', clickable: true },
        { id: 'card-border', hasBorder: true },
      ];

      // Verify initial state
      for (const { id } of testCards) {
        const card = getCardByTestId(page, id);
        await card.scrollIntoViewIfNeeded();
        await expect(card).toBeVisible();
      }

      // Perform interactions
      const clickCard = getCardByTestId(page, 'card-onclick');
      await clickCard.click();
      await page.waitForTimeout(150);

      const signUpButton = getCardByTestId(page, 'card-app-signup-button');
      await signUpButton.click();
      await page.waitForTimeout(150);

      // Verify all cards maintain their state and properties
      for (const { id, hasButton, clickable, hasBorder } of testCards) {
        const card = getCardByTestId(page, id);
        await expect(card).toBeVisible();
        expect(await card.getAttribute('role')).toBe('region');

        if (hasButton) {
          await expect(card.getByRole('button')).toBeVisible();
        }

        if (clickable) {
          await card.hover();
          await expect(card).toHaveCSS('cursor', 'pointer');
        }

        if (hasBorder) {
          const borderWidth = await card.evaluate(
            el => window.getComputedStyle(el).borderTopWidth
          );
          expect(borderWidth).toBeTruthy();
        }
      }
    });
  });
});
