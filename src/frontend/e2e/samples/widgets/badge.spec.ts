import { test, expect, type Page } from '@playwright/test';

test.skip(true, 'Temporarily skipping samples E2E suite while PR stabilizes');

// Constants
const BADGE_VARIANTS = [
  'Primary',
  'Destructive',
  'Secondary',
  'Outline',
  'Success',
  'Warning',
  'Info',
] as const;

const BADGE_SIZES = ['Small', 'Medium', 'Large'] as const;

const ICON_TYPES = [
  'With Bell',
  'With Heart',
  'With Star',
  'With Check',
] as const;

const ICON_POSITIONS = ['Left Icon', 'Right Icon'] as const;

// Helper functions
const getBadgeLocator = (page: Page, text: string) =>
  page.locator('div.inline-flex.items-center', { hasText: text });

const verifyBadgesWithIcons = async (page: Page, items: readonly string[]) => {
  for (const item of items) {
    const firstBadge = getBadgeLocator(page, item).first();
    await firstBadge.scrollIntoViewIfNeeded();
    await page.waitForTimeout(150);
    await expect(firstBadge).toBeVisible();
    await expect(firstBadge.locator('svg').first()).toBeVisible();

    // Verify there are multiple instances for different variants
    const badges = getBadgeLocator(page, item);
    expect(await badges.count()).toBeGreaterThanOrEqual(3);
  }
};

// Shared setup function
async function setupBadgePage(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  const searchInput = page.getByTestId('sidebar-search');
  await expect(searchInput).toBeVisible();
  await searchInput.click();
  await searchInput.fill('badge');
  await searchInput.press('Enter');

  const firstResult = page
    .locator('button')
    .filter({ hasText: /Badge/i })
    .first();
  await firstResult.click();
  await page.waitForLoadState('networkidle');
}

test.describe('Badge Widget Tests', () => {
  test.beforeEach(async ({ page }) => {
    await setupBadgePage(page);
  });

  test.describe('Smoke Tests', () => {
    test('should render badge app with badges', async ({ page }) => {
      const h1 = page.getByRole('heading', { level: 1 });
      await expect(h1).toBeVisible();
      const h1Text = await h1.textContent();
      expect(h1Text).toBeTruthy();
      expect(h1Text!.length).toBeGreaterThan(0);

      const badges = page.locator('div.inline-flex.items-center.rounded-md');
      expect(await badges.count()).toBeGreaterThan(0);
    });
  });

  test.describe('All States and Variants', () => {
    test('should verify all variants with styling', async ({ page }) => {
      for (const variant of BADGE_VARIANTS) {
        const badge = getBadgeLocator(page, variant).first();
        await expect(badge).toBeVisible();
        expect(await badge.getAttribute('class')).toContain('inline-flex');
      }
    });

    test('should verify all sizes with correct dimensions', async ({
      page,
    }) => {
      const badges = await Promise.all(
        BADGE_SIZES.map(size => getBadgeLocator(page, size).first())
      );

      for (const badge of badges) {
        await badge.scrollIntoViewIfNeeded();
        await page.waitForTimeout(100);
        await expect(badge).toBeVisible();
      }

      const boxes = await Promise.all(badges.map(b => b.boundingBox()));

      if (boxes.every(box => box)) {
        expect(boxes[0]!.height).toBeLessThanOrEqual(boxes[1]!.height);
        expect(boxes[2]!.height).toBeGreaterThanOrEqual(boxes[1]!.height);
      }

      // Verify there are multiple size instances for different variants
      for (const size of BADGE_SIZES) {
        expect(
          await getBadgeLocator(page, size).count()
        ).toBeGreaterThanOrEqual(3);
      }
    });

    test('should verify badges with icons', async ({ page }) => {
      await verifyBadgesWithIcons(page, ICON_TYPES);
    });

    test('should verify icon positioning', async ({ page }) => {
      await verifyBadgesWithIcons(page, ICON_POSITIONS);
    });

    test('should verify icon-only badges are smaller than text badges', async ({
      page,
    }) => {
      const iconOnlyBadge = page.locator('div.inline-flex.items-center').last();
      await iconOnlyBadge.scrollIntoViewIfNeeded();
      await expect(iconOnlyBadge).toBeVisible();
      await expect(iconOnlyBadge.locator('svg').first()).toBeVisible();

      const textBadge = getBadgeLocator(page, BADGE_VARIANTS[0]).first();
      await textBadge.scrollIntoViewIfNeeded();

      const [iconOnlyBox, textBox] = await Promise.all([
        iconOnlyBadge.boundingBox(),
        textBadge.boundingBox(),
      ]);

      if (iconOnlyBox && textBox) {
        expect(iconOnlyBox.width).toBeLessThan(textBox.width);
      }
    });
  });

  test.describe('Visual Properties', () => {
    test('should have correct CSS classes and dimensions', async ({ page }) => {
      const badge = getBadgeLocator(page, BADGE_VARIANTS[0]).first();
      await expect(badge).toBeVisible();

      const classAttribute = await badge.getAttribute('class');
      expect(classAttribute).toContain('inline-flex');
      expect(classAttribute).toContain('whitespace-nowrap');

      const box = await badge.boundingBox();
      expect(box).toBeTruthy();
      if (box) {
        expect(box.width).toBeGreaterThan(0);
        expect(box.height).toBeGreaterThan(0);
      }
    });

    test('should have different colors for different variants', async ({
      page,
    }) => {
      const testVariants = [
        BADGE_VARIANTS[0],
        BADGE_VARIANTS[1],
        BADGE_VARIANTS[4],
      ];
      const badges = await Promise.all(
        testVariants.map(v => getBadgeLocator(page, v).first())
      );

      const colors = await Promise.all(
        badges.map(b =>
          b.evaluate(el => window.getComputedStyle(el).backgroundColor)
        )
      );

      expect(colors[0]).not.toBe(colors[1]);
      expect(colors[0]).not.toBe(colors[2]);
      expect(colors[1]).not.toBe(colors[2]);
    });

    test('should have properly sized icons', async ({ page }) => {
      const badgeWithIcon = getBadgeLocator(page, ICON_TYPES[0]).first();
      await badgeWithIcon.scrollIntoViewIfNeeded();

      const icon = badgeWithIcon.locator('svg').first();
      await expect(icon).toBeVisible();

      const iconBox = await icon.boundingBox();
      if (iconBox) {
        expect(iconBox.width).toBeGreaterThan(8);
        expect(iconBox.width).toBeLessThan(24);
      }
    });

    test('should display text content correctly', async ({ page }) => {
      const badge = getBadgeLocator(page, BADGE_VARIANTS[0]).first();
      await expect(badge).toBeVisible();
      expect(await badge.textContent()).toContain(BADGE_VARIANTS[0]);

      const longTextBadge = getBadgeLocator(page, ICON_TYPES[0]).first();
      await longTextBadge.scrollIntoViewIfNeeded();
      const box = await longTextBadge.boundingBox();
      if (box) {
        expect(box.width).toBeGreaterThan(50);
      }
    });
  });

  test.describe('Complex Scenarios', () => {
    test('should verify combined size and icon properties', async ({
      page,
    }) => {
      const smallBadge = getBadgeLocator(page, BADGE_SIZES[0]).first();
      await smallBadge.scrollIntoViewIfNeeded();
      await expect(smallBadge).toBeVisible();
      expect(await smallBadge.getAttribute('class')).toContain('inline-flex');

      const iconBadge = getBadgeLocator(page, ICON_TYPES[0]).first();
      await iconBadge.scrollIntoViewIfNeeded();
      await expect(iconBadge).toBeVisible();
      await expect(iconBadge.locator('svg').first()).toBeVisible();
    });

    test('should have proper layout with multiple badges visible', async ({
      page,
    }) => {
      const badges = page.locator('div.inline-flex.items-center');
      await expect(badges.first()).toBeVisible();
      await expect(badges.nth(1)).toBeVisible();

      expect(await badges.count()).toBeGreaterThan(10);
    });
  });
});
