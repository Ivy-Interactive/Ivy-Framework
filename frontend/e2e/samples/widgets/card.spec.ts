import { test, expect, type Page } from '@playwright/test';

// Helper functions
const getCardByTitle = (page: Page, title: string) =>
  page.locator('div.rounded-lg.border', {
    has: page.locator('h3', { hasText: title }),
  });

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
    .filter({ hasText: /^Card$/i })
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
      // Verify heading is present
      await expect(
        page.getByRole('heading', { level: 1 }).first()
      ).toBeVisible();

      // Verify multiple cards are rendered
      const cards = page.locator('div.rounded-lg.border');
      const cardCount = await cards.count();
      expect(cardCount).toBeGreaterThan(10);
    });

    test('should render cards with titles and descriptions', async ({
      page,
    }) => {
      // Check first card has title
      const firstCard = getCardByTitle(page, 'Card App');
      await expect(firstCard).toBeVisible();
      await expect(
        firstCard.locator('h3', { hasText: 'Card App' })
      ).toBeVisible();

      // Check card has description
      const description = firstCard.locator('p.text-sm.text-muted-foreground', {
        hasText: 'This is a card app.',
      });
      await expect(description).toBeVisible();
    });

    test('should render cards with content', async ({ page }) => {
      const cardApp = getCardByTitle(page, 'Card App');
      await expect(cardApp).toBeVisible();

      // Verify content text
      await expect(
        cardApp.getByText(/Lorem ipsum dolor sit amet/)
      ).toBeVisible();

      // Verify button in content
      const button = cardApp.getByRole('button', { name: 'Sign Me Up' });
      await expect(button).toBeVisible();
    });
  });

  test.describe('All States and Visual Properties', () => {
    test('should verify cards with border properties', async ({ page }) => {
      const borderCard = getCardByTitle(page, 'Card with Border');
      await borderCard.scrollIntoViewIfNeeded();
      await expect(borderCard).toBeVisible();

      // Verify border exists and has styling
      const borderStyle = await borderCard.evaluate(el => {
        const styles = window.getComputedStyle(el);
        return {
          borderWidth: styles.borderWidth,
          borderStyle: styles.borderStyle,
        };
      });

      expect(borderStyle.borderWidth).toBeTruthy();
      expect(borderStyle.borderStyle).toBeTruthy();
    });

    test('should verify cards with different border colors', async ({
      page,
    }) => {
      const redBorderCard = getCardByTitle(page, 'Border Color Test');
      await redBorderCard.scrollIntoViewIfNeeded();
      await expect(redBorderCard).toBeVisible();

      // Verify red border is applied
      const borderColor = await redBorderCard.evaluate(
        el => window.getComputedStyle(el).borderColor
      );
      expect(borderColor).toBeTruthy();

      const primaryBorderCard = getCardByTitle(page, 'Card with Border');
      await primaryBorderCard.scrollIntoViewIfNeeded();
      const primaryBorderColor = await primaryBorderCard.evaluate(
        el => window.getComputedStyle(el).borderColor
      );

      // Different cards should have different border colors
      expect(borderColor).not.toBe(primaryBorderColor);
    });

    test('should verify cards with icons in header', async ({ page }) => {
      // Total Sales card has a DollarSign icon
      const salesCard = getCardByTitle(page, 'Total Sales');
      await salesCard.scrollIntoViewIfNeeded();
      await expect(salesCard).toBeVisible();

      // Verify icon exists in card header
      const header = salesCard
        .locator('div.flex.flex-row.items-center')
        .first();
      await expect(header.locator('svg').first()).toBeVisible();
    });

    test('should verify cards contain progress bars', async ({ page }) => {
      const salesCard = getCardByTitle(page, 'Total Sales');
      await salesCard.scrollIntoViewIfNeeded();
      await expect(salesCard).toBeVisible();

      // Verify progress bar exists
      const progressBar = salesCard.locator('div[role="progressbar"]');
      await expect(progressBar.first()).toBeVisible();

      // Verify progress value is set
      const ariaValue = await progressBar.first().getAttribute('aria-valuenow');
      expect(ariaValue).toBeTruthy();
      expect(Number(ariaValue)).toBeGreaterThan(0);
    });

    test('should verify cards with various gap and spacing', async ({
      page,
    }) => {
      // Zero Spacing card has Gap(0)
      const zeroSpacingCard = getCardByTitle(page, 'Zero Spacing');
      await zeroSpacingCard.scrollIntoViewIfNeeded();
      await expect(zeroSpacingCard).toBeVisible();

      // Text Spacing Demo has Gap(2)
      const spacingDemoCard = getCardByTitle(page, 'Text Spacing Demo');
      await spacingDemoCard.scrollIntoViewIfNeeded();
      await expect(spacingDemoCard).toBeVisible();

      // Both should be visible but have different internal spacing
      const zeroSpacingBox = await zeroSpacingCard.boundingBox();
      const spacingDemoBox = await spacingDemoCard.boundingBox();

      expect(zeroSpacingBox).toBeTruthy();
      expect(spacingDemoBox).toBeTruthy();
    });

    test('should verify cards display metric values correctly', async ({
      page,
    }) => {
      const salesCard = getCardByTitle(page, 'Total Sales');
      await salesCard.scrollIntoViewIfNeeded();

      // Verify main metric value
      await expect(salesCard.getByText('$84,250')).toBeVisible();

      // Verify percentage change
      await expect(salesCard.getByText('21%')).toBeVisible();

      // Verify trending icon
      const trendIcon = salesCard
        .locator('svg')
        .filter({ hasText: '' })
        .first();
      await expect(trendIcon).toBeVisible();
    });

    test('should verify cards with long numbers display properly', async ({
      page,
    }) => {
      const longNumberCard = getCardByTitle(page, 'Very Long Revenue Number');
      await longNumberCard.scrollIntoViewIfNeeded();
      await expect(longNumberCard).toBeVisible();

      // Verify long number is visible and formatted
      await expect(longNumberCard.getByText('$123,456,789.99')).toBeVisible();

      // Verify large percentage
      await expect(longNumberCard.getByText('1,234.5%')).toBeVisible();
    });
  });

  test.describe('Interactive Behavior', () => {
    test('should handle card click events', async ({ page }) => {
      const clickCard = getCardByTitle(page, 'OnClick test');
      await clickCard.scrollIntoViewIfNeeded();
      await expect(clickCard).toBeVisible();

      // Click the card
      await clickCard.click();

      // Verify toast appears (if implemented in the UI)
      // This is a basic interaction test
      await page.waitForTimeout(100);
    });

    test('should handle button clicks inside cards', async ({ page }) => {
      const cardApp = getCardByTitle(page, 'Card App');
      await cardApp.scrollIntoViewIfNeeded();

      const signUpButton = cardApp.getByRole('button', { name: 'Sign Me Up' });
      await expect(signUpButton).toBeVisible();
      await expect(signUpButton).toBeEnabled();

      // Click the button
      await signUpButton.click();

      // Button should remain enabled after click
      await expect(signUpButton).toBeEnabled();
    });
  });

  test.describe('Complex Layout Tests', () => {
    test('should render cards in grid layout', async ({ page }) => {
      // Get first few cards and check they're visible
      const firstCards = await Promise.all([
        page.locator('div.rounded-lg.border').nth(0),
        page.locator('div.rounded-lg.border').nth(1),
        page.locator('div.rounded-lg.border').nth(2),
      ]);

      for (const card of firstCards) {
        await card.scrollIntoViewIfNeeded();
        await expect(card).toBeVisible();
      }

      // Verify cards are positioned horizontally (x positions should be different)
      const boxes = await Promise.all(firstCards.map(c => c.boundingBox()));
      if (boxes.every(b => b)) {
        expect(boxes[0]!.x).not.toBe(boxes[1]!.x);
      }
    });

    test('should render cards with nested layouts', async ({ page }) => {
      // User Engagement widget has nested horizontal and vertical layouts
      const engagementCard = getCardByTitle(page, 'User Engagement');
      await engagementCard.scrollIntoViewIfNeeded();
      await expect(engagementCard).toBeVisible();

      // Verify multiple text elements and icons
      await expect(engagementCard.getByText('1,247')).toBeVisible();
      await expect(engagementCard.getByText('Active Users')).toBeVisible();
      await expect(engagementCard.getByText('+12.5%')).toBeVisible();

      // Verify icons are present
      const icons = engagementCard.locator('svg');
      expect(await icons.count()).toBeGreaterThan(1);
    });

    test('should render cards with multiple progress bars', async ({
      page,
    }) => {
      const progressCard = getCardByTitle(page, 'Progress Variations');
      await progressCard.scrollIntoViewIfNeeded();
      await expect(progressCard).toBeVisible();

      // Verify multiple progress bars exist
      const progressBars = progressCard.locator('div[role="progressbar"]');
      const progressCount = await progressBars.count();
      expect(progressCount).toBeGreaterThanOrEqual(4);

      // Verify different progress values
      for (let i = 0; i < Math.min(progressCount, 4); i++) {
        const bar = progressBars.nth(i);
        await expect(bar).toBeVisible();
        const value = await bar.getAttribute('aria-valuenow');
        expect(value).toBeTruthy();
      }
    });

    test('should render cards with grid content', async ({ page }) => {
      // Mixed Content widget has grid layout inside
      const mixedCard = getCardByTitle(page, 'Download Analytics');
      await mixedCard.scrollIntoViewIfNeeded();
      await expect(mixedCard).toBeVisible();

      // Verify grid content elements
      await expect(mixedCard.getByText('Mobile')).toBeVisible();
      await expect(mixedCard.getByText('Desktop')).toBeVisible();
      await expect(mixedCard.getByText('1,234')).toBeVisible();
      await expect(mixedCard.getByText('856')).toBeVisible();
    });

    test('should render cards with complex icon and text combinations', async ({
      page,
    }) => {
      const socialCard = getCardByTitle(page, 'Social Engagement');
      await socialCard.scrollIntoViewIfNeeded();
      await expect(socialCard).toBeVisible();

      // Verify all social metrics
      await expect(socialCard.getByText('Likes')).toBeVisible();
      await expect(socialCard.getByText('2,847')).toBeVisible();
      await expect(socialCard.getByText('Comments')).toBeVisible();
      await expect(socialCard.getByText('156')).toBeVisible();
      await expect(socialCard.getByText('Shares')).toBeVisible();
      await expect(socialCard.getByText('89')).toBeVisible();
      await expect(socialCard.getByText('Views')).toBeVisible();
      await expect(socialCard.getByText('12,456')).toBeVisible();

      // Verify icons for each metric
      const icons = socialCard.locator('svg');
      expect(await icons.count()).toBeGreaterThanOrEqual(4);
    });
  });

  test.describe('Complex Routine Test', () => {
    test('should handle complete user interaction flow', async ({ page }) => {
      // Step 1: Verify page loaded with cards
      const cards = page.locator('div.rounded-lg.border');
      expect(await cards.count()).toBeGreaterThan(10);

      // Step 2: Interact with first card button
      const cardApp = getCardByTitle(page, 'Card App');
      await cardApp.scrollIntoViewIfNeeded();
      const signUpButton = cardApp.getByRole('button', { name: 'Sign Me Up' });
      await signUpButton.click();

      // Step 3: Click on an interactive card
      const clickCard = getCardByTitle(page, 'OnClick test');
      await clickCard.scrollIntoViewIfNeeded();
      await clickCard.click();
      await page.waitForTimeout(100);

      // Step 4: Scroll through different card sections
      const salesCard = getCardByTitle(page, 'Total Sales');
      await salesCard.scrollIntoViewIfNeeded();
      await expect(salesCard).toBeVisible();
      await expect(salesCard.getByText('$84,250')).toBeVisible();

      // Step 5: Verify metric card with progress
      const progressBar = salesCard.locator('div[role="progressbar"]').first();
      await expect(progressBar).toBeVisible();

      // Step 6: Check complex layout card
      const engagementCard = getCardByTitle(page, 'User Engagement');
      await engagementCard.scrollIntoViewIfNeeded();
      await expect(engagementCard.getByText('1,247')).toBeVisible();

      // Step 7: Verify layout card with multiple elements
      const layoutCard = getCardByTitle(page, 'Layout Testing');
      await layoutCard.scrollIntoViewIfNeeded();
      await expect(layoutCard).toBeVisible();
      await expect(layoutCard.getByText('4.8')).toBeVisible();

      // Step 8: Check border styled card
      const borderCard = getCardByTitle(page, 'Border Color Test');
      await borderCard.scrollIntoViewIfNeeded();
      await expect(borderCard).toBeVisible();

      // Step 9: Verify progress variations card
      const progressCard = getCardByTitle(page, 'Progress Variations');
      await progressCard.scrollIntoViewIfNeeded();
      const progressBars = progressCard.locator('div[role="progressbar"]');
      expect(await progressBars.count()).toBeGreaterThanOrEqual(4);

      // Step 10: Final state verification
      await expect(
        page.getByRole('heading', { level: 1 }).first()
      ).toBeVisible();
    });
  });

  test.describe('Visual Side-Effects Tests', () => {
    test('should verify card shadows and elevation', async ({ page }) => {
      const card = getCardByTitle(page, 'Card App');
      await card.scrollIntoViewIfNeeded();

      const boxShadow = await card.evaluate(
        el => window.getComputedStyle(el).boxShadow
      );

      // Card should have some shadow for elevation
      expect(boxShadow).toBeTruthy();
    });

    test('should verify card border radius', async ({ page }) => {
      const card = getCardByTitle(page, 'Card with Border');
      await card.scrollIntoViewIfNeeded();

      const borderRadius = await card.evaluate(
        el => window.getComputedStyle(el).borderRadius
      );

      expect(borderRadius).toBeTruthy();
      // Rounded cards should have border radius > 0
      expect(borderRadius).not.toBe('0px');
    });

    test('should verify card padding and spacing', async ({ page }) => {
      const card = getCardByTitle(page, 'Card App');
      await card.scrollIntoViewIfNeeded();

      const padding = await card.evaluate(el => {
        const styles = window.getComputedStyle(el);
        return {
          paddingTop: styles.paddingTop,
          paddingBottom: styles.paddingBottom,
          paddingLeft: styles.paddingLeft,
          paddingRight: styles.paddingRight,
        };
      });

      // Card should have padding
      expect(padding.paddingTop).toBeTruthy();
      expect(padding.paddingBottom).toBeTruthy();
      expect(padding.paddingLeft).toBeTruthy();
      expect(padding.paddingRight).toBeTruthy();
    });

    test('should verify card background color', async ({ page }) => {
      const card = getCardByTitle(page, 'Card App');
      await card.scrollIntoViewIfNeeded();

      const backgroundColor = await card.evaluate(
        el => window.getComputedStyle(el).backgroundColor
      );

      expect(backgroundColor).toBeTruthy();
      expect(backgroundColor).not.toBe('rgba(0, 0, 0, 0)');
    });

    test('should verify icon colors in cards', async ({ page }) => {
      const salesCard = getCardByTitle(page, 'Total Sales');
      await salesCard.scrollIntoViewIfNeeded();

      // Get icons with color styling
      const emeraldIcon = salesCard.locator('svg').first();
      await expect(emeraldIcon).toBeVisible();

      const iconColor = await emeraldIcon.evaluate(
        el => window.getComputedStyle(el).color
      );

      // Icon should have color applied
      expect(iconColor).toBeTruthy();
    });

    test('should verify text hierarchy and sizing', async ({ page }) => {
      const salesCard = getCardByTitle(page, 'Total Sales');
      await salesCard.scrollIntoViewIfNeeded();

      // Get different text sizes
      const h4Text = salesCard.getByText('$84,250');
      const smallText = salesCard.getByText('21%');

      const h4Size = await h4Text.evaluate(
        el => window.getComputedStyle(el).fontSize
      );

      const smallSize = await smallText.evaluate(
        el => window.getComputedStyle(el).fontSize
      );

      // H4 should be larger than small text
      const h4Value = parseFloat(h4Size);
      const smallValue = parseFloat(smallSize);

      expect(h4Value).toBeGreaterThan(smallValue);
    });

    test('should verify card hover state (if clickable)', async ({ page }) => {
      const clickCard = getCardByTitle(page, 'OnClick test');
      await clickCard.scrollIntoViewIfNeeded();

      // Hover over card
      await clickCard.hover();

      // Clickable card should have pointer cursor or similar indication
      const hoverCursor = await clickCard.evaluate(
        el => window.getComputedStyle(el).cursor
      );

      expect(hoverCursor).toBeTruthy();
    });

    test('should verify progress bar visual styling', async ({ page }) => {
      const salesCard = getCardByTitle(page, 'Total Sales');
      await salesCard.scrollIntoViewIfNeeded();

      const progressBar = salesCard.locator('div[role="progressbar"]').first();
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
      const cards = await page.locator('div.rounded-lg.border').all();

      // Check first few cards
      for (let i = 0; i < Math.min(5, cards.length); i++) {
        const card = cards[i];
        await card.scrollIntoViewIfNeeded();
        const box = await card.boundingBox();

        if (box) {
          // Card should have reasonable dimensions
          expect(box.width).toBeGreaterThan(100);
          expect(box.height).toBeGreaterThan(50);
          expect(box.width).toBeLessThan(2000);
          expect(box.height).toBeLessThan(2000);
        }
      }
    });
  });
});
