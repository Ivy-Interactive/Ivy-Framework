import { test, expect, type Page } from '@playwright/test';

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

const SECTIONS = {
  MAIN: 'Badges',
  VARIANTS: 'Variants',
  SIZES: 'Sizes',
  WITH_ICONS: 'With Icons',
  ICON_POSITIONING: 'Icon Positioning',
  ICON_ONLY: 'Icon Only',
} as const;

// Helper functions
const getBadgeLocator = (page: Page, text: string) =>
  page.locator('div.inline-flex.items-center', { hasText: text });

const scrollToSection = async (page: Page, sectionName: string) => {
  const heading = page.getByRole('heading', { name: sectionName, exact: true });
  await heading.scrollIntoViewIfNeeded();
  await page.waitForTimeout(100);
  return heading;
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
    test('should render badge app and display main heading', async ({
      page,
    }) => {
      const h1Heading = page.getByRole('heading', { level: 1 });
      await expect(h1Heading).toBeVisible();
      await expect(h1Heading).toHaveText(SECTIONS.MAIN);

      const badges = page.locator('div.inline-flex.items-center.rounded-md');
      expect(await badges.count()).toBeGreaterThan(0);
    });

    test('should display all main sections', async ({ page }) => {
      for (const section of Object.values(SECTIONS).slice(1)) {
        await expect(
          page.getByRole('heading', { name: section, exact: true })
        ).toBeVisible();
      }
    });
  });

  test.describe('Variant Tests - All States', () => {
    test('should verify all badge variants are rendered with correct styling', async ({
      page,
    }) => {
      for (const variant of BADGE_VARIANTS) {
        const badge = getBadgeLocator(page, variant).first();
        await expect(badge).toBeVisible();
        const classAttribute = await badge.getAttribute('class');
        expect(classAttribute).toContain('inline-flex');
      }
    });
  });

  test.describe('Size Tests - All States', () => {
    test('should verify all size badges are rendered', async ({ page }) => {
      await scrollToSection(page, SECTIONS.SIZES);

      for (const size of BADGE_SIZES) {
        const sizeBadges = getBadgeLocator(page, size);
        expect(await sizeBadges.count()).toBeGreaterThanOrEqual(
          BADGE_VARIANTS.length
        );
      }
    });

    test('should verify size differences are applied correctly', async ({
      page,
    }) => {
      await scrollToSection(page, SECTIONS.SIZES);

      const badges = await Promise.all(
        BADGE_SIZES.map(size => getBadgeLocator(page, size).first())
      );

      for (const badge of badges) {
        await expect(badge).toBeVisible();
      }

      const boxes = await Promise.all(badges.map(badge => badge.boundingBox()));

      if (boxes.every(box => box)) {
        expect(boxes[0]!.height).toBeLessThanOrEqual(boxes[1]!.height);
        expect(boxes[2]!.height).toBeGreaterThanOrEqual(boxes[1]!.height);
      }
    });
  });

  test.describe('Icon Tests - All States', () => {
    test('should verify all icon badges are rendered with icon elements', async ({
      page,
    }) => {
      await scrollToSection(page, SECTIONS.WITH_ICONS);

      for (const iconType of ICON_TYPES) {
        const iconBadges = getBadgeLocator(page, iconType);
        expect(await iconBadges.count()).toBeGreaterThanOrEqual(
          BADGE_VARIANTS.length
        );

        const firstBadge = iconBadges.first();
        await expect(firstBadge).toBeVisible();
        await expect(firstBadge.locator('svg').first()).toBeVisible();
      }
    });
  });

  test.describe('Icon Positioning Tests', () => {
    test('should verify icon positioning badges are rendered with icons', async ({
      page,
    }) => {
      await scrollToSection(page, SECTIONS.ICON_POSITIONING);

      for (const position of ICON_POSITIONS) {
        const positionBadges = getBadgeLocator(page, position);
        expect(await positionBadges.count()).toBeGreaterThanOrEqual(
          BADGE_VARIANTS.length
        );

        const firstBadge = positionBadges.first();
        await expect(firstBadge).toBeVisible();
        await expect(firstBadge.locator('svg').first()).toBeVisible();
      }
    });
  });

  test.describe('Icon Only Badges Tests', () => {
    test('should verify icon-only badges are rendered with icons and smaller width', async ({
      page,
    }) => {
      await scrollToSection(page, SECTIONS.ICON_ONLY);

      const iconOnlyBadge = page.locator('div.inline-flex.items-center').last();
      await expect(iconOnlyBadge).toBeVisible();
      await expect(iconOnlyBadge.locator('svg').first()).toBeVisible();

      const textBadge = getBadgeLocator(page, BADGE_VARIANTS[0]).first();
      await textBadge.scrollIntoViewIfNeeded();

      const iconOnlyBox = await iconOnlyBadge.boundingBox();
      const textBox = await textBadge.boundingBox();

      if (iconOnlyBox && textBox) {
        expect(iconOnlyBox.width).toBeLessThan(textBox.width);
      }
    });
  });

  test.describe('Visual Properties Tests', () => {
    test('should verify badges have proper CSS classes and dimensions', async ({
      page,
    }) => {
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

    test('should verify variant color differences', async ({ page }) => {
      const testVariants = [
        BADGE_VARIANTS[0],
        BADGE_VARIANTS[1],
        BADGE_VARIANTS[4],
      ];
      const badges = await Promise.all(
        testVariants.map(variant => getBadgeLocator(page, variant).first())
      );

      for (const badge of badges) {
        await expect(badge).toBeVisible();
      }

      const colors = await Promise.all(
        badges.map(badge =>
          badge.evaluate(el => window.getComputedStyle(el).backgroundColor)
        )
      );

      expect(colors[0]).not.toBe(colors[1]);
      expect(colors[0]).not.toBe(colors[2]);
      expect(colors[1]).not.toBe(colors[2]);
    });

    test('should verify icon sizing within badges', async ({ page }) => {
      await scrollToSection(page, SECTIONS.WITH_ICONS);

      const badgeWithIcon = getBadgeLocator(page, ICON_TYPES[0]).first();
      await expect(badgeWithIcon).toBeVisible();

      const icon = badgeWithIcon.locator('svg').first();
      await expect(icon).toBeVisible();

      const iconBox = await icon.boundingBox();
      expect(iconBox).toBeTruthy();
      if (iconBox) {
        expect(iconBox.width).toBeGreaterThan(8);
        expect(iconBox.width).toBeLessThan(24);
      }
    });
  });

  test.describe('Complex Routine Tests', () => {
    test('should navigate through all sections and verify content', async ({
      page,
    }) => {
      await expect(
        page.getByRole('heading', { name: SECTIONS.MAIN, exact: true })
      ).toBeVisible();

      for (const section of Object.values(SECTIONS).slice(1)) {
        const heading = await scrollToSection(page, section);
        await expect(heading).toBeVisible();
        expect(
          await page.locator('div.inline-flex.items-center').count()
        ).toBeGreaterThan(0);
      }
    });

    test('should verify all variants across multiple sections', async ({
      page,
    }) => {
      await scrollToSection(page, SECTIONS.VARIANTS);

      for (const variant of BADGE_VARIANTS) {
        await expect(getBadgeLocator(page, variant).first()).toBeVisible();
      }

      await scrollToSection(page, SECTIONS.SIZES);

      for (const size of BADGE_SIZES) {
        expect(
          await getBadgeLocator(page, size).count()
        ).toBeGreaterThanOrEqual(BADGE_VARIANTS.length);
      }
    });

    test('should verify combined properties work together', async ({
      page,
    }) => {
      await scrollToSection(page, SECTIONS.SIZES);
      const smallBadge = getBadgeLocator(page, BADGE_SIZES[0]).first();
      await expect(smallBadge).toBeVisible();
      expect(await smallBadge.getAttribute('class')).toContain('inline-flex');

      await scrollToSection(page, SECTIONS.WITH_ICONS);
      const iconBadge = getBadgeLocator(page, ICON_TYPES[0]).first();
      await expect(iconBadge).toBeVisible();
      await expect(iconBadge.locator('svg').first()).toBeVisible();
    });
  });

  test.describe('All Badge Widget Methods Coverage', () => {
    test('should verify all variant methods are applied', async ({ page }) => {
      for (const variant of BADGE_VARIANTS) {
        await expect(getBadgeLocator(page, variant).first()).toBeVisible();
      }
    });

    test('should verify all size methods are applied', async ({ page }) => {
      await scrollToSection(page, SECTIONS.SIZES);

      for (const size of BADGE_SIZES) {
        await expect(getBadgeLocator(page, size).first()).toBeVisible();
      }
    });

    test('should verify icon method with position is applied', async ({
      page,
    }) => {
      await scrollToSection(page, SECTIONS.ICON_POSITIONING);

      for (const position of ICON_POSITIONS) {
        const badge = getBadgeLocator(page, position).first();
        await expect(badge).toBeVisible();
        await expect(badge.locator('svg').first()).toBeVisible();
      }
    });

    test('should verify icon-only badges (title null)', async ({ page }) => {
      await scrollToSection(page, SECTIONS.ICON_ONLY);

      const lastBadge = page.locator('div.inline-flex.items-center').last();
      await expect(lastBadge).toBeVisible();
      await expect(lastBadge.locator('svg').first()).toBeVisible();
    });

    test('should verify combined variant and size methods', async ({
      page,
    }) => {
      await scrollToSection(page, SECTIONS.SIZES);

      for (const size of BADGE_SIZES) {
        expect(
          await getBadgeLocator(page, size).count()
        ).toBeGreaterThanOrEqual(BADGE_VARIANTS.length);
      }
    });

    test('should verify combined variant and icon methods', async ({
      page,
    }) => {
      await scrollToSection(page, SECTIONS.WITH_ICONS);

      for (const iconType of ICON_TYPES) {
        const iconBadges = getBadgeLocator(page, iconType);
        expect(await iconBadges.count()).toBeGreaterThanOrEqual(
          BADGE_VARIANTS.length
        );

        const firstBadge = iconBadges.first();
        await expect(firstBadge).toBeVisible();
        await expect(firstBadge.locator('svg').first()).toBeVisible();
      }
    });
  });

  test.describe('Content and Text Tests', () => {
    test('should verify badge text and styling', async ({ page }) => {
      const badge = getBadgeLocator(page, BADGE_VARIANTS[0]).first();
      await expect(badge).toBeVisible();

      const text = await badge.textContent();
      expect(text).toContain(BADGE_VARIANTS[0]);

      const classAttribute = await badge.getAttribute('class');
      expect(classAttribute).toContain('whitespace-nowrap');
    });

    test('should verify long text badges render correctly', async ({
      page,
    }) => {
      await scrollToSection(page, SECTIONS.WITH_ICONS);

      const longTextBadge = getBadgeLocator(page, ICON_TYPES[0]).first();
      await expect(longTextBadge).toBeVisible();

      const box = await longTextBadge.boundingBox();
      expect(box).toBeTruthy();
      if (box) {
        expect(box.width).toBeGreaterThan(50);
      }
    });
  });

  test.describe('Layout and Grid Tests', () => {
    test('should verify badges are arranged in grid layout', async ({
      page,
    }) => {
      await scrollToSection(page, SECTIONS.VARIANTS);

      const badges = page.locator('div.inline-flex.items-center');
      const firstBadge = badges.first();
      const secondBadge = badges.nth(1);

      await expect(firstBadge).toBeVisible();
      await expect(secondBadge).toBeVisible();

      expect(await firstBadge.boundingBox()).toBeTruthy();
      expect(await secondBadge.boundingBox()).toBeTruthy();
    });

    test('should verify all badges fit within viewport when scrolling', async ({
      page,
    }) => {
      for (const section of Object.values(SECTIONS).slice(1)) {
        const heading = await scrollToSection(page, section);
        await expect(heading).toBeVisible();

        const headingBox = await heading.boundingBox();
        expect(headingBox).toBeTruthy();
        if (headingBox) {
          expect(headingBox.y).toBeGreaterThanOrEqual(0);
        }
      }
    });
  });
});
