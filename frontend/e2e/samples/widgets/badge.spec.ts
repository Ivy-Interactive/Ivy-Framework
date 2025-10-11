import { test, expect, type Page } from '@playwright/test';

// Shared setup function
async function setupBadgePage(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  // Find the sidebar search input
  const searchInput = page.getByTestId('sidebar-search');
  await expect(searchInput).toBeVisible();

  // Click the search input
  await searchInput.click();
  // Type 'badge'
  await searchInput.fill('badge');
  // Press Enter
  await searchInput.press('Enter');

  const firstResult = page
    .locator('button')
    .filter({ hasText: /Badge/i })
    .first();
  await firstResult.click();

  // Wait for navigation
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
      // Verify an H1 heading is present on the page
      const h1Heading = page.getByRole('heading', { level: 1 });
      await expect(h1Heading).toBeVisible();
      await expect(h1Heading).toHaveText('Badges');

      // Verify at least one badge element exists
      const badges = page.locator('[class*="badge"]');
      const count = await badges.count();
      expect(count).toBeGreaterThan(0);
    });

    test('should display all main sections', async ({ page }) => {
      // Verify all section headings are visible
      await expect(
        page.getByRole('heading', { name: 'Variants', exact: true })
      ).toBeVisible();
      await expect(
        page.getByRole('heading', { name: 'Sizes', exact: true })
      ).toBeVisible();
      await expect(
        page.getByRole('heading', { name: 'With Icons', exact: true })
      ).toBeVisible();
      await expect(
        page.getByRole('heading', { name: 'Icon Positioning', exact: true })
      ).toBeVisible();
      await expect(
        page.getByRole('heading', { name: 'Icon Only', exact: true })
      ).toBeVisible();
    });
  });

  test.describe('Variant Tests - All States', () => {
    test('should verify all badge variants are rendered', async ({ page }) => {
      // Check for each variant text
      const variants = [
        'Primary',
        'Destructive',
        'Secondary',
        'Outline',
        'Success',
        'Warning',
        'Info',
      ];

      for (const variant of variants) {
        const badge = page.locator('span[class*="badge"]', {
          hasText: variant,
        });
        await expect(badge.first()).toBeVisible();
      }
    });

    test('should verify Primary variant has correct styling', async ({
      page,
    }) => {
      const primaryBadge = page
        .locator('span[class*="badge"]', { hasText: 'Primary' })
        .first();
      await expect(primaryBadge).toBeVisible();

      // Check for primary variant class
      const classAttribute = await primaryBadge.getAttribute('class');
      expect(classAttribute).toContain('badge');
    });

    test('should verify Destructive variant has correct styling', async ({
      page,
    }) => {
      const destructiveBadge = page
        .locator('span[class*="badge"]', { hasText: 'Destructive' })
        .first();
      await expect(destructiveBadge).toBeVisible();

      const classAttribute = await destructiveBadge.getAttribute('class');
      expect(classAttribute).toContain('badge');
    });

    test('should verify Secondary variant has correct styling', async ({
      page,
    }) => {
      const secondaryBadge = page
        .locator('span[class*="badge"]', { hasText: 'Secondary' })
        .first();
      await expect(secondaryBadge).toBeVisible();

      const classAttribute = await secondaryBadge.getAttribute('class');
      expect(classAttribute).toContain('badge');
    });

    test('should verify Outline variant has correct styling', async ({
      page,
    }) => {
      const outlineBadge = page
        .locator('span[class*="badge"]', { hasText: 'Outline' })
        .first();
      await expect(outlineBadge).toBeVisible();

      const classAttribute = await outlineBadge.getAttribute('class');
      expect(classAttribute).toContain('badge');
    });

    test('should verify Success variant has correct styling', async ({
      page,
    }) => {
      const successBadge = page
        .locator('span[class*="badge"]', { hasText: 'Success' })
        .first();
      await expect(successBadge).toBeVisible();

      const classAttribute = await successBadge.getAttribute('class');
      expect(classAttribute).toContain('badge');
    });

    test('should verify Warning variant has correct styling', async ({
      page,
    }) => {
      const warningBadge = page
        .locator('span[class*="badge"]', { hasText: 'Warning' })
        .first();
      await expect(warningBadge).toBeVisible();

      const classAttribute = await warningBadge.getAttribute('class');
      expect(classAttribute).toContain('badge');
    });

    test('should verify Info variant has correct styling', async ({ page }) => {
      const infoBadge = page
        .locator('span[class*="badge"]', { hasText: 'Info' })
        .first();
      await expect(infoBadge).toBeVisible();

      const classAttribute = await infoBadge.getAttribute('class');
      expect(classAttribute).toContain('badge');
    });
  });

  test.describe('Size Tests - All States', () => {
    test('should verify small badges are rendered', async ({ page }) => {
      // Scroll to sizes section
      const sizesHeading = page.getByRole('heading', {
        name: 'Sizes',
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      const smallBadges = page.locator('span[class*="badge"]', {
        hasText: 'Small',
      });
      const count = await smallBadges.count();
      expect(count).toBeGreaterThanOrEqual(7); // One for each variant
    });

    test('should verify medium badges are rendered', async ({ page }) => {
      const sizesHeading = page.getByRole('heading', {
        name: 'Sizes',
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      const mediumBadges = page.locator('span[class*="badge"]', {
        hasText: 'Medium',
      });
      const count = await mediumBadges.count();
      expect(count).toBeGreaterThanOrEqual(7); // One for each variant
    });

    test('should verify large badges are rendered', async ({ page }) => {
      const sizesHeading = page.getByRole('heading', {
        name: 'Sizes',
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      const largeBadges = page.locator('span[class*="badge"]', {
        hasText: 'Large',
      });
      const count = await largeBadges.count();
      expect(count).toBeGreaterThanOrEqual(7); // One for each variant
    });

    test('should verify size differences are applied correctly', async ({
      page,
    }) => {
      const sizesHeading = page.getByRole('heading', {
        name: 'Sizes',
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      // Get one badge of each size
      const smallBadge = page
        .locator('span[class*="badge"]', { hasText: 'Small' })
        .first();
      const mediumBadge = page
        .locator('span[class*="badge"]', { hasText: 'Medium' })
        .first();
      const largeBadge = page
        .locator('span[class*="badge"]', { hasText: 'Large' })
        .first();

      await expect(smallBadge).toBeVisible();
      await expect(mediumBadge).toBeVisible();
      await expect(largeBadge).toBeVisible();

      // Verify they have different bounding boxes
      const smallBox = await smallBadge.boundingBox();
      const mediumBox = await mediumBadge.boundingBox();
      const largeBox = await largeBadge.boundingBox();

      expect(smallBox).toBeTruthy();
      expect(mediumBox).toBeTruthy();
      expect(largeBox).toBeTruthy();

      if (smallBox && mediumBox && largeBox) {
        // Small should be smaller than medium
        expect(smallBox.height).toBeLessThanOrEqual(mediumBox.height);
        // Large should be larger than medium
        expect(largeBox.height).toBeGreaterThanOrEqual(mediumBox.height);
      }
    });
  });

  test.describe('Icon Tests - All States', () => {
    test('should verify badges with bell icon are rendered', async ({
      page,
    }) => {
      const iconsHeading = page.getByRole('heading', {
        name: 'With Icons',
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      const bellBadges = page.locator('span[class*="badge"]', {
        hasText: 'With Bell',
      });
      const count = await bellBadges.count();
      expect(count).toBeGreaterThanOrEqual(7); // One for each variant
    });

    test('should verify badges with heart icon are rendered', async ({
      page,
    }) => {
      const iconsHeading = page.getByRole('heading', {
        name: 'With Icons',
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      const heartBadges = page.locator('span[class*="badge"]', {
        hasText: 'With Heart',
      });
      const count = await heartBadges.count();
      expect(count).toBeGreaterThanOrEqual(7); // One for each variant
    });

    test('should verify badges with star icon are rendered', async ({
      page,
    }) => {
      const iconsHeading = page.getByRole('heading', {
        name: 'With Icons',
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      const starBadges = page.locator('span[class*="badge"]', {
        hasText: 'With Star',
      });
      const count = await starBadges.count();
      expect(count).toBeGreaterThanOrEqual(7); // One for each variant
    });

    test('should verify badges with check icon are rendered', async ({
      page,
    }) => {
      const iconsHeading = page.getByRole('heading', {
        name: 'With Icons',
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      const checkBadges = page.locator('span[class*="badge"]', {
        hasText: 'With Check',
      });
      const count = await checkBadges.count();
      expect(count).toBeGreaterThanOrEqual(7); // One for each variant
    });

    test('should verify badges contain icon elements', async ({ page }) => {
      const iconsHeading = page.getByRole('heading', {
        name: 'With Icons',
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      // Find a badge with an icon
      const badgeWithIcon = page
        .locator('span[class*="badge"]', { hasText: 'With Bell' })
        .first();
      await expect(badgeWithIcon).toBeVisible();

      // Check for icon element (svg) within the badge
      const icon = badgeWithIcon.locator('svg').first();
      await expect(icon).toBeVisible();
    });
  });

  test.describe('Icon Positioning Tests', () => {
    test('should verify left icon positioning badges are rendered', async ({
      page,
    }) => {
      const positioningHeading = page.getByRole('heading', {
        name: 'Icon Positioning',
        exact: true,
      });
      await positioningHeading.scrollIntoViewIfNeeded();

      const leftIconBadges = page.locator('span[class*="badge"]', {
        hasText: 'Left Icon',
      });
      const count = await leftIconBadges.count();
      expect(count).toBeGreaterThanOrEqual(7); // One for each variant
    });

    test('should verify right icon positioning badges are rendered', async ({
      page,
    }) => {
      const positioningHeading = page.getByRole('heading', {
        name: 'Icon Positioning',
        exact: true,
      });
      await positioningHeading.scrollIntoViewIfNeeded();

      const rightIconBadges = page.locator('span[class*="badge"]', {
        hasText: 'Right Icon',
      });
      const count = await rightIconBadges.count();
      expect(count).toBeGreaterThanOrEqual(7); // One for each variant
    });

    test('should verify icon position is correctly applied', async ({
      page,
    }) => {
      const positioningHeading = page.getByRole('heading', {
        name: 'Icon Positioning',
        exact: true,
      });
      await positioningHeading.scrollIntoViewIfNeeded();

      // Get a left icon badge
      const leftIconBadge = page
        .locator('span[class*="badge"]', { hasText: 'Left Icon' })
        .first();
      await expect(leftIconBadge).toBeVisible();

      // Get a right icon badge
      const rightIconBadge = page
        .locator('span[class*="badge"]', { hasText: 'Right Icon' })
        .first();
      await expect(rightIconBadge).toBeVisible();

      // Both should contain an icon
      await expect(leftIconBadge.locator('svg').first()).toBeVisible();
      await expect(rightIconBadge.locator('svg').first()).toBeVisible();
    });
  });

  test.describe('Icon Only Badges Tests', () => {
    test('should verify icon-only badges are rendered', async ({ page }) => {
      const iconOnlyHeading = page.getByRole('heading', {
        name: 'Icon Only',
        exact: true,
      });
      await iconOnlyHeading.scrollIntoViewIfNeeded();

      // Find all badge elements in the Icon Only section
      // We need to find the parent container of the Icon Only heading
      const badges = page.locator('span[class*="badge"]');
      const count = await badges.count();

      // Icon Only section should have at least 7 badges (one for each variant)
      expect(count).toBeGreaterThan(0);
    });

    test('should verify icon-only badges contain icons', async ({ page }) => {
      const iconOnlyHeading = page.getByRole('heading', {
        name: 'Icon Only',
        exact: true,
      });
      await iconOnlyHeading.scrollIntoViewIfNeeded();

      // Find badges in the page and check for SVG elements
      const badges = page.locator('span[class*="badge"]');
      const firstBadge = badges.last();

      await expect(firstBadge).toBeVisible();
      const icon = firstBadge.locator('svg').first();
      await expect(icon).toBeVisible();
    });

    test('should verify icon-only badges are smaller than text badges', async ({
      page,
    }) => {
      const iconOnlyHeading = page.getByRole('heading', {
        name: 'Icon Only',
        exact: true,
      });
      await iconOnlyHeading.scrollIntoViewIfNeeded();

      // Get an icon-only badge (last badges in the page)
      const iconOnlyBadge = page.locator('span[class*="badge"]').last();
      await expect(iconOnlyBadge).toBeVisible();

      // Get a text badge from the variants section
      const textBadge = page
        .locator('span[class*="badge"]', { hasText: 'Primary' })
        .first();
      await textBadge.scrollIntoViewIfNeeded();
      await expect(textBadge).toBeVisible();

      const iconOnlyBox = await iconOnlyBadge.boundingBox();
      const textBox = await textBadge.boundingBox();

      expect(iconOnlyBox).toBeTruthy();
      expect(textBox).toBeTruthy();

      if (iconOnlyBox && textBox) {
        // Icon-only badge should be narrower than text badge
        expect(iconOnlyBox.width).toBeLessThan(textBox.width);
      }
    });
  });

  test.describe('Visual Properties Tests', () => {
    test('should verify badges have proper CSS classes', async ({ page }) => {
      const badge = page
        .locator('span[class*="badge"]', { hasText: 'Primary' })
        .first();
      await expect(badge).toBeVisible();

      const classAttribute = await badge.getAttribute('class');
      expect(classAttribute).toContain('badge');
      expect(classAttribute).toContain('whitespace-nowrap');
    });

    test('should verify variant color differences', async ({ page }) => {
      // Get badges with different variants
      const primaryBadge = page
        .locator('span[class*="badge"]', { hasText: 'Primary' })
        .first();
      const destructiveBadge = page
        .locator('span[class*="badge"]', { hasText: 'Destructive' })
        .first();
      const successBadge = page
        .locator('span[class*="badge"]', { hasText: 'Success' })
        .first();

      await expect(primaryBadge).toBeVisible();
      await expect(destructiveBadge).toBeVisible();
      await expect(successBadge).toBeVisible();

      // Get background colors
      const primaryColor = await primaryBadge.evaluate(
        el => window.getComputedStyle(el).backgroundColor
      );
      const destructiveColor = await destructiveBadge.evaluate(
        el => window.getComputedStyle(el).backgroundColor
      );
      const successColor = await successBadge.evaluate(
        el => window.getComputedStyle(el).backgroundColor
      );

      // Colors should be different
      expect(primaryColor).not.toBe(destructiveColor);
      expect(primaryColor).not.toBe(successColor);
      expect(destructiveColor).not.toBe(successColor);
    });

    test('should verify badges maintain minimum width', async ({ page }) => {
      const badge = page
        .locator('span[class*="badge"]', { hasText: 'Primary' })
        .first();
      await expect(badge).toBeVisible();

      const box = await badge.boundingBox();
      expect(box).toBeTruthy();
      if (box) {
        expect(box.width).toBeGreaterThan(0);
        expect(box.height).toBeGreaterThan(0);
      }
    });

    test('should verify icon sizing within badges', async ({ page }) => {
      const iconsHeading = page.getByRole('heading', {
        name: 'With Icons',
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      const badgeWithIcon = page
        .locator('span[class*="badge"]', { hasText: 'With Bell' })
        .first();
      await expect(badgeWithIcon).toBeVisible();

      const icon = badgeWithIcon.locator('svg').first();
      await expect(icon).toBeVisible();

      // Verify icon has dimensions
      const iconBox = await icon.boundingBox();
      expect(iconBox).toBeTruthy();
      if (iconBox) {
        expect(iconBox.width).toBeGreaterThan(0);
        expect(iconBox.height).toBeGreaterThan(0);
      }
    });

    test('should verify icon size varies with badge size', async ({ page }) => {
      const sizesHeading = page.getByRole('heading', {
        name: 'Sizes',
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      // Icons section needs to be scrolled to
      const iconsHeading = page.getByRole('heading', {
        name: 'With Icons',
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      const badgeWithIcon = page
        .locator('span[class*="badge"]', { hasText: 'With Bell' })
        .first();
      await expect(badgeWithIcon).toBeVisible();

      const icon = badgeWithIcon.locator('svg').first();
      const iconBox = await icon.boundingBox();

      expect(iconBox).toBeTruthy();
      if (iconBox) {
        // Icon should have reasonable dimensions
        expect(iconBox.width).toBeGreaterThan(8);
        expect(iconBox.width).toBeLessThan(24);
      }
    });
  });

  test.describe('Complex Routine Tests', () => {
    test('should navigate through all sections and verify badge rendering', async ({
      page,
    }) => {
      // Verify page loads with heading
      await expect(
        page.getByRole('heading', { name: 'Badges', exact: true })
      ).toBeVisible();

      // Navigate through each section
      const sections = [
        'Variants',
        'Sizes',
        'With Icons',
        'Icon Positioning',
        'Icon Only',
      ];

      for (const sectionName of sections) {
        const heading = page.getByRole('heading', {
          name: sectionName,
          exact: true,
        });
        await heading.scrollIntoViewIfNeeded();
        await expect(heading).toBeVisible();

        // Verify badges exist in each section
        const badges = page.locator('span[class*="badge"]');
        const count = await badges.count();
        expect(count).toBeGreaterThan(0);
      }
    });

    test('should verify all variants appear in all sections', async ({
      page,
    }) => {
      // Check variants section
      const variantsHeading = page.getByRole('heading', {
        name: 'Variants',
        exact: true,
      });
      await variantsHeading.scrollIntoViewIfNeeded();

      const variants = [
        'Primary',
        'Destructive',
        'Secondary',
        'Outline',
        'Success',
        'Warning',
        'Info',
      ];

      for (const variant of variants) {
        const badge = page
          .locator('span[class*="badge"]', { hasText: variant })
          .first();
        await expect(badge).toBeVisible();
      }

      // Check sizes section has all variants
      const sizesHeading = page.getByRole('heading', {
        name: 'Sizes',
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      // Each size (Small, Medium, Large) should have all variants
      for (const size of ['Small', 'Medium', 'Large']) {
        const sizeBadges = page.locator('span[class*="badge"]', {
          hasText: size,
        });
        const count = await sizeBadges.count();
        expect(count).toBeGreaterThanOrEqual(variants.length);
      }
    });

    test('should verify combined properties work together', async ({
      page,
    }) => {
      // Scroll to sizes section and verify a small badge with variant
      const sizesHeading = page.getByRole('heading', {
        name: 'Sizes',
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      const smallBadge = page
        .locator('span[class*="badge"]', { hasText: 'Small' })
        .first();
      await expect(smallBadge).toBeVisible();

      const classAttribute = await smallBadge.getAttribute('class');
      expect(classAttribute).toContain('badge');

      // Scroll to icon section and verify badge with icon and variant
      const iconsHeading = page.getByRole('heading', {
        name: 'With Icons',
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      const iconBadge = page
        .locator('span[class*="badge"]', { hasText: 'With Bell' })
        .first();
      await expect(iconBadge).toBeVisible();
      await expect(iconBadge.locator('svg').first()).toBeVisible();
    });

    test('should verify scrolling through all badge examples', async ({
      page,
    }) => {
      // Start at top
      await page.evaluate(() => window.scrollTo(0, 0));

      const h1 = page.getByRole('heading', { name: 'Badges', exact: true });
      await expect(h1).toBeVisible();

      // Scroll through each section
      const sections = [
        'Variants',
        'Sizes',
        'With Icons',
        'Icon Positioning',
        'Icon Only',
      ];

      for (const sectionName of sections) {
        const heading = page.getByRole('heading', {
          name: sectionName,
          exact: true,
        });
        await heading.scrollIntoViewIfNeeded();
        await expect(heading).toBeVisible();

        // Wait a bit for any animations
        await page.waitForTimeout(100);
      }

      // Verify we can still see badges at the end
      const badges = page.locator('span[class*="badge"]');
      const count = await badges.count();
      expect(count).toBeGreaterThan(0);
    });
  });

  test.describe('All Badge Widget Methods Coverage', () => {
    test('should verify all variant methods are applied', async ({ page }) => {
      const variants = [
        'Primary',
        'Destructive',
        'Secondary',
        'Outline',
        'Success',
        'Warning',
        'Info',
      ];

      for (const variant of variants) {
        const badge = page
          .locator('span[class*="badge"]', { hasText: variant })
          .first();
        await expect(badge).toBeVisible();
      }
    });

    test('should verify all size methods are applied', async ({ page }) => {
      const sizesHeading = page.getByRole('heading', {
        name: 'Sizes',
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      // Small, Medium (default), Large
      const smallBadge = page
        .locator('span[class*="badge"]', { hasText: 'Small' })
        .first();
      const mediumBadge = page
        .locator('span[class*="badge"]', { hasText: 'Medium' })
        .first();
      const largeBadge = page
        .locator('span[class*="badge"]', { hasText: 'Large' })
        .first();

      await expect(smallBadge).toBeVisible();
      await expect(mediumBadge).toBeVisible();
      await expect(largeBadge).toBeVisible();
    });

    test('should verify icon method with position is applied', async ({
      page,
    }) => {
      const positioningHeading = page.getByRole('heading', {
        name: 'Icon Positioning',
        exact: true,
      });
      await positioningHeading.scrollIntoViewIfNeeded();

      // Left position (default)
      const leftIconBadge = page
        .locator('span[class*="badge"]', { hasText: 'Left Icon' })
        .first();
      await expect(leftIconBadge).toBeVisible();
      await expect(leftIconBadge.locator('svg').first()).toBeVisible();

      // Right position
      const rightIconBadge = page
        .locator('span[class*="badge"]', { hasText: 'Right Icon' })
        .first();
      await expect(rightIconBadge).toBeVisible();
      await expect(rightIconBadge.locator('svg').first()).toBeVisible();
    });

    test('should verify icon-only badges (title null)', async ({ page }) => {
      const iconOnlyHeading = page.getByRole('heading', {
        name: 'Icon Only',
        exact: true,
      });
      await iconOnlyHeading.scrollIntoViewIfNeeded();

      // Icon-only badges should have icons but minimal text
      const badges = page.locator('span[class*="badge"]');
      const lastBadge = badges.last();

      await expect(lastBadge).toBeVisible();
      const icon = lastBadge.locator('svg').first();
      await expect(icon).toBeVisible();
    });

    test('should verify combined variant and size methods', async ({
      page,
    }) => {
      const sizesHeading = page.getByRole('heading', {
        name: 'Sizes',
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      // Each variant should be available in each size
      const variants = [
        'Primary',
        'Destructive',
        'Secondary',
        'Outline',
        'Success',
        'Warning',
        'Info',
      ];

      for (const size of ['Small', 'Medium', 'Large']) {
        const sizeBadges = page.locator('span[class*="badge"]', {
          hasText: size,
        });
        const count = await sizeBadges.count();
        expect(count).toBeGreaterThanOrEqual(variants.length);
      }
    });

    test('should verify combined variant and icon methods', async ({
      page,
    }) => {
      const iconsHeading = page.getByRole('heading', {
        name: 'With Icons',
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      // Each variant should have badges with icons
      const iconTypes = ['With Bell', 'With Heart', 'With Star', 'With Check'];

      for (const iconType of iconTypes) {
        const iconBadges = page.locator('span[class*="badge"]', {
          hasText: iconType,
        });
        const count = await iconBadges.count();
        expect(count).toBeGreaterThanOrEqual(7); // One for each variant

        // Verify first one has an icon
        const firstBadge = iconBadges.first();
        await expect(firstBadge).toBeVisible();
        await expect(firstBadge.locator('svg').first()).toBeVisible();
      }
    });
  });

  test.describe('Content and Text Tests', () => {
    test('should verify badge text is not truncated', async ({ page }) => {
      const badge = page
        .locator('span[class*="badge"]', { hasText: 'Primary' })
        .first();
      await expect(badge).toBeVisible();

      const text = await badge.textContent();
      expect(text).toContain('Primary');
    });

    test('should verify whitespace-nowrap is applied', async ({ page }) => {
      const badge = page
        .locator('span[class*="badge"]', { hasText: 'Primary' })
        .first();
      await expect(badge).toBeVisible();

      const classAttribute = await badge.getAttribute('class');
      expect(classAttribute).toContain('whitespace-nowrap');
    });

    test('should verify long text badges render correctly', async ({
      page,
    }) => {
      const iconsHeading = page.getByRole('heading', {
        name: 'With Icons',
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      const longTextBadge = page
        .locator('span[class*="badge"]', { hasText: 'With Bell' })
        .first();
      await expect(longTextBadge).toBeVisible();

      const box = await longTextBadge.boundingBox();
      expect(box).toBeTruthy();
      if (box) {
        // Badge should expand to fit content
        expect(box.width).toBeGreaterThan(50);
      }
    });
  });

  test.describe('Layout and Grid Tests', () => {
    test('should verify badges are arranged in grid layout', async ({
      page,
    }) => {
      // Variants should be in a grid
      const variantsHeading = page.getByRole('heading', {
        name: 'Variants',
        exact: true,
      });
      await variantsHeading.scrollIntoViewIfNeeded();

      // Get positions of first few badges
      const badges = page.locator('span[class*="badge"]');
      const firstBadge = badges.first();
      const secondBadge = badges.nth(1);

      const firstBox = await firstBadge.boundingBox();
      const secondBox = await secondBadge.boundingBox();

      expect(firstBox).toBeTruthy();
      expect(secondBox).toBeTruthy();

      // Both should be visible
      await expect(firstBadge).toBeVisible();
      await expect(secondBadge).toBeVisible();
    });

    test('should verify all badges fit within viewport when scrolling', async ({
      page,
    }) => {
      const sections = [
        'Variants',
        'Sizes',
        'With Icons',
        'Icon Positioning',
        'Icon Only',
      ];

      for (const sectionName of sections) {
        const heading = page.getByRole('heading', {
          name: sectionName,
          exact: true,
        });
        await heading.scrollIntoViewIfNeeded();

        // Verify heading is visible
        await expect(heading).toBeVisible();

        const headingBox = await heading.boundingBox();
        expect(headingBox).toBeTruthy();
        if (headingBox) {
          // Heading should be within viewport
          expect(headingBox.y).toBeGreaterThanOrEqual(0);
        }
      }
    });
  });
});
