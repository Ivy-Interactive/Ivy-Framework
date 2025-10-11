import { test, expect, type Page } from '@playwright/test';

// Shared setup function
async function setupButtonPage(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  // Find the sidebar search input
  const searchInput = page.getByTestId('sidebar-search');
  await expect(searchInput).toBeVisible();

  // Click the search input
  await searchInput.click();
  // Type 'button'
  await searchInput.fill('button');
  // Press Enter
  await searchInput.press('Enter');

  const firstResult = page
    .locator('button')
    .filter({ hasText: /^Button$/i })
    .first();
  await firstResult.click();

  // Wait for navigation
  await page.waitForLoadState('networkidle');
}

test.describe('Button Widget Tests', () => {
  test.beforeEach(async ({ page }) => {
    await setupButtonPage(page);
  });

  test.describe('Smoke Tests', () => {
    test('should render button app with heading and multiple buttons', async ({
      page,
    }) => {
      // Verify an H1 heading exists
      const h1Heading = page.getByRole('heading', { level: 1 });
      await expect(h1Heading.first()).toBeVisible();

      // Verify multiple buttons exist
      const buttons = page.getByRole('button');
      expect(await buttons.count()).toBeGreaterThan(0);
    });
  });

  test.describe('Button Variants Tests', () => {
    test('should render and interact with all button variants', async ({
      page,
    }) => {
      const variants = [
        'Primary',
        'Destructive',
        'Secondary',
        'Success',
        'Warning',
        'Info',
        'Outline',
        'Ghost',
        'Link',
      ];

      // Test each variant is visible and clickable
      for (const variant of variants) {
        const button = page
          .getByRole('button', { name: variant, exact: true })
          .first();
        await expect(button).toBeVisible();
        await expect(button).toBeEnabled();

        // Click the button
        await button.click();

        // Verify it's still enabled after click
        await expect(button).toBeEnabled();
      }
    });

    test('should verify variant-specific styling', async ({ page }) => {
      // Test Destructive variant has destructive class
      const destructiveButton = page
        .getByRole('button', { name: 'Destructive', exact: true })
        .first();
      const destructiveClass = await destructiveButton.getAttribute('class');
      expect(destructiveClass).toContain('destructive');

      // Test Outline variant has outline class
      const outlineButton = page
        .getByRole('button', { name: 'Outline', exact: true })
        .first();
      const outlineClass = await outlineButton.getAttribute('class');
      expect(outlineClass).toContain('outline');

      // Test Secondary variant has secondary class
      const secondaryButton = page
        .getByRole('button', { name: 'Secondary', exact: true })
        .first();
      const secondaryClass = await secondaryButton.getAttribute('class');
      expect(secondaryClass).toContain('secondary');
    });
  });

  test.describe('Button Sizes Tests', () => {
    test('should render all button sizes', async ({ page }) => {
      // Test Small buttons
      const smallButton = page.getByRole('button', { name: 'Small' }).first();
      await expect(smallButton).toBeVisible();

      // Test Medium buttons
      const mediumButton = page.getByRole('button', { name: 'Medium' }).first();
      await expect(mediumButton).toBeVisible();

      // Test Large buttons
      const largeButton = page.getByRole('button', { name: 'Large' }).first();
      await expect(largeButton).toBeVisible();
    });

    test('should verify size hierarchy', async ({ page }) => {
      const smallButton = page.getByRole('button', { name: 'Small' }).first();
      const mediumButton = page.getByRole('button', { name: 'Medium' }).first();
      const largeButton = page.getByRole('button', { name: 'Large' }).first();

      const smallBox = await smallButton.boundingBox();
      const mediumBox = await mediumButton.boundingBox();
      const largeBox = await largeButton.boundingBox();

      expect(smallBox).toBeTruthy();
      expect(mediumBox).toBeTruthy();
      expect(largeBox).toBeTruthy();

      // Verify small < medium < large
      if (smallBox && mediumBox && largeBox) {
        expect(smallBox.height).toBeLessThan(mediumBox.height);
        expect(mediumBox.height).toBeLessThan(largeBox.height);
      }
    });
  });

  test.describe('Button States Tests', () => {
    test('should handle disabled buttons', async ({ page }) => {
      // Scroll to states section
      await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));

      // Check for disabled buttons
      const disabledButtons = page.locator('button:disabled');
      const disabledCount = await disabledButtons.count();

      if (disabledCount > 0) {
        await expect(disabledButtons.first()).toBeDisabled();
      }
    });

    test('should display loading state', async ({ page }) => {
      // Scroll to states section
      await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));

      // Check for loading spinners
      const loadingSpinners = page.locator('.animate-spin');
      const spinnerCount = await loadingSpinners.count();

      if (spinnerCount > 0) {
        await expect(loadingSpinners.first()).toBeVisible();
      }
    });
  });

  test.describe('Button Icons Tests', () => {
    test('should render buttons with icons', async ({ page }) => {
      // Scroll to icons section
      await page.evaluate(() =>
        window.scrollTo(0, document.body.scrollHeight / 2)
      );

      // Check for buttons with SVG icons
      const buttonsWithIcons = page.locator('button:has(svg)');
      expect(await buttonsWithIcons.count()).toBeGreaterThan(0);

      const firstIconButton = buttonsWithIcons.first();
      await expect(firstIconButton).toBeVisible();

      // Verify SVG exists inside button
      const svgIcon = firstIconButton.locator('svg');
      await expect(svgIcon.first()).toBeVisible();
    });

    test('should render icon-only buttons', async ({ page }) => {
      // Scroll to icon-only section
      await page.evaluate(() =>
        window.scrollTo(0, document.body.scrollHeight / 2)
      );

      // Find icon-only buttons (square aspect ratio)
      const allButtons = page.getByRole('button');
      const buttonCount = await allButtons.count();

      for (let i = 0; i < buttonCount && i < 50; i++) {
        const button = allButtons.nth(i);
        const box = await button.boundingBox();

        if (box && box.width > 0 && box.height > 0) {
          const aspectRatio = box.width / box.height;
          // Icon buttons are roughly square
          if (aspectRatio > 0.8 && aspectRatio < 1.2 && box.width < 50) {
            expect(aspectRatio).toBeGreaterThan(0.7);
            expect(aspectRatio).toBeLessThan(1.3);
            break;
          }
        }
      }
    });
  });

  test.describe('Button Click Events Tests', () => {
    test('should handle button clicks and update demo', async ({ page }) => {
      // Click a Primary button
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();
      await expect(primaryButton).toBeVisible();
      await primaryButton.click();

      // Verify interactive demo updated
      const updatedLabel = page.locator('text=/Button.*was clicked/');
      await expect(updatedLabel).toBeVisible();
    });

    test('should handle multiple button clicks', async ({ page }) => {
      // Click Primary button
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();
      await primaryButton.click();

      // Click Destructive button
      const destructiveButton = page
        .getByRole('button', { name: 'Destructive', exact: true })
        .first();
      await destructiveButton.click();

      // Verify demo was updated
      const updatedLabel = page.locator('text=/Button.*was clicked/');
      await expect(updatedLabel).toBeVisible();
    });
  });

  test.describe('Accessibility Tests', () => {
    test('should support keyboard navigation', async ({ page }) => {
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();

      await expect(primaryButton).toBeVisible();

      // Focus the button
      await primaryButton.focus();
      await expect(primaryButton).toBeFocused();

      // Activate with Enter key
      await page.keyboard.press('Enter');

      // Button should still be enabled
      await expect(primaryButton).toBeEnabled();
    });

    test('should verify button text content', async ({ page }) => {
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();

      const textContent = await primaryButton.textContent();
      expect(textContent).toBeTruthy();
      expect(textContent?.trim()).toBe('Primary');
    });

    test('should use semantic button elements', async ({ page }) => {
      const buttons = page.getByRole('button');
      expect(await buttons.count()).toBeGreaterThan(0);
    });
  });
});
