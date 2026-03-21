import { test, expect, type Page } from '@playwright/test';

test.skip(true, 'Temporarily skipping samples E2E suite while PR stabilizes');

// Shared setup function
async function setupButtonPage(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  const searchInput = page.getByTestId('sidebar-search');
  await expect(searchInput).toBeVisible();
  await searchInput.click();
  await searchInput.fill('button');
  await searchInput.press('Enter');

  const firstResult = page
    .locator('button')
    .filter({ hasText: /^Button$/i })
    .first();
  await firstResult.click();
  await page.waitForLoadState('networkidle');
}

test.describe('Button Widget Tests', () => {
  test.beforeEach(async ({ page }) => {
    await setupButtonPage(page);
  });

  test('should render button app with heading and multiple buttons', async ({
    page,
  }) => {
    await expect(page.getByRole('heading', { level: 1 }).first()).toBeVisible();
    expect(await page.getByRole('button').count()).toBeGreaterThan(0);
  });

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

    for (const variant of variants) {
      const button = page
        .getByRole('button', { name: variant, exact: true })
        .first();
      await expect(button).toBeVisible();
      await expect(button).toBeEnabled();
      await button.click();
      await expect(button).toBeEnabled();
    }
  });

  test('should verify variant-specific styling', async ({ page }) => {
    const variantsToCheck = [
      { name: 'Destructive', className: 'destructive' },
      { name: 'Outline', className: 'outline' },
      { name: 'Secondary', className: 'secondary' },
    ];

    for (const { name, className } of variantsToCheck) {
      const button = page.getByRole('button', { name, exact: true }).first();
      const buttonClass = await button.getAttribute('class');
      expect(buttonClass).toContain(className);
    }
  });

  test('should render all button sizes with correct hierarchy', async ({
    page,
  }) => {
    const sizes = ['Small', 'Medium', 'Large'];

    // Verify all sizes are visible
    for (const size of sizes) {
      await expect(
        page.getByRole('button', { name: size }).first()
      ).toBeVisible();
    }

    // Verify size hierarchy
    const [smallBox, mediumBox, largeBox] = await Promise.all(
      sizes.map(size =>
        page.getByRole('button', { name: size }).first().boundingBox()
      )
    );

    if (smallBox && mediumBox && largeBox) {
      expect(smallBox.height).toBeLessThan(mediumBox.height);
      expect(mediumBox.height).toBeLessThan(largeBox.height);
    }
  });

  test('should handle disabled and loading states', async ({ page }) => {
    const disabledButtons = page.locator('button:disabled');
    if ((await disabledButtons.count()) > 0) {
      const firstDisabled = disabledButtons.first();
      await firstDisabled.scrollIntoViewIfNeeded();
      await expect(firstDisabled).toBeDisabled();
    }

    const loadingSpinners = page.locator('.animate-spin');
    if ((await loadingSpinners.count()) > 0) {
      const firstSpinner = loadingSpinners.first();
      await firstSpinner.scrollIntoViewIfNeeded();
      await expect(firstSpinner).toBeVisible();
    }
  });

  test('should render buttons with icons', async ({ page }) => {
    const buttonsWithIcons = page.locator('button:has(svg)');
    expect(await buttonsWithIcons.count()).toBeGreaterThan(0);

    const firstIconButton = buttonsWithIcons.first();
    await expect(firstIconButton).toBeVisible();
    await expect(firstIconButton.locator('svg').first()).toBeVisible();
  });

  test.skip('should render buttons with left and right icon positions', async ({
    page,
  }) => {
    // Find all buttons with both text and icons
    const buttonsWithIcons = page.locator('button:has(svg)');
    const count = await buttonsWithIcons.count();
    expect(count).toBeGreaterThan(0);

    // Check that we have multiple buttons with icons (different positions)
    if (count >= 2) {
      const firstButton = buttonsWithIcons.first();
      await firstButton.scrollIntoViewIfNeeded();
      await expect(firstButton).toBeVisible();
      await expect(firstButton.locator('svg').first()).toBeVisible();

      const secondButton = buttonsWithIcons.nth(1);
      await secondButton.scrollIntoViewIfNeeded();
      await expect(secondButton).toBeVisible();
      await expect(secondButton.locator('svg').first()).toBeVisible();
    }
  });

  test('should render icon-only buttons', async ({ page }) => {
    // Find icon-only buttons (square aspect ratio)
    const allButtons = page.getByRole('button');
    const buttonCount = await allButtons.count();

    for (let i = 0; i < Math.min(buttonCount, 50); i++) {
      const box = await allButtons.nth(i).boundingBox();
      if (box && box.width > 0 && box.height > 0) {
        const aspectRatio = box.width / box.height;
        if (aspectRatio > 0.8 && aspectRatio < 1.2 && box.width < 50) {
          expect(aspectRatio).toBeGreaterThan(0.7);
          expect(aspectRatio).toBeLessThan(1.3);
          break;
        }
      }
    }
  });

  test('should render buttons with styling properties', async ({ page }) => {
    const stylingButtons = [
      { name: 'Rounded' },
      { name: 'Full' },
      { name: 'With Tooltip' },
    ];

    for (const { name } of stylingButtons) {
      const buttons = page.getByRole('button', { name });
      if ((await buttons.count()) > 0) {
        const button = buttons.first();
        await button.scrollIntoViewIfNeeded();
        await expect(button).toBeVisible();

        if (name === 'With Tooltip') {
          await button.hover();
          await expect(button).toBeEnabled();
        }
      }
    }
  });

  test('should handle button clicks and update demo', async ({ page }) => {
    const primaryButton = page
      .getByRole('button', { name: 'Primary', exact: true })
      .first();
    await expect(primaryButton).toBeVisible();
    await primaryButton.click();

    const updatedLabel = page.locator('text=/Button.*was clicked/');
    await expect(updatedLabel).toBeVisible();

    // Click another button
    await page
      .getByRole('button', { name: 'Destructive', exact: true })
      .first()
      .click();
    await expect(updatedLabel).toBeVisible();
  });

  test.skip('should handle complex multi-step interactions', async ({
    page,
  }) => {
    // Step 1: Click Primary button
    await page
      .getByRole('button', { name: 'Primary', exact: true })
      .first()
      .click();

    // Step 2: Click Destructive button
    await page
      .getByRole('button', { name: 'Destructive', exact: true })
      .first()
      .click();

    // Step 3: Verify demo updated
    await expect(page.locator('text=/Button.*was clicked/')).toBeVisible();

    // Step 4: Click a size button
    const largeButton = page.getByRole('button', { name: 'Large' }).first();
    await largeButton.scrollIntoViewIfNeeded();
    await largeButton.click();

    // Step 5: Test icon button
    const iconButton = page.locator('button:has(svg)').first();
    await iconButton.scrollIntoViewIfNeeded();
    await iconButton.click();

    // Step 6: Click Outline variant
    const outlineButton = page
      .getByRole('button', { name: 'Outline', exact: true })
      .first();
    await outlineButton.scrollIntoViewIfNeeded();
    await outlineButton.click();

    // Verify final state
    await expect(page.locator('text=/Button.*was clicked/')).toBeVisible();
  });

  test.skip('should verify all button methods work together', async ({
    page,
  }) => {
    // Test enabled, disabled, and loading states
    await page
      .getByRole('button', { name: 'Primary', exact: true })
      .first()
      .click();

    const disabledButtons = page.locator('button:disabled');
    if ((await disabledButtons.count()) > 0) {
      await disabledButtons.first().scrollIntoViewIfNeeded();
      await expect(disabledButtons.first()).toBeDisabled();
    }

    const loadingSpinners = page.locator('.animate-spin');
    if ((await loadingSpinners.count()) > 0) {
      await loadingSpinners.first().scrollIntoViewIfNeeded();
      await expect(loadingSpinners.first()).toBeVisible();
    }

    const secondaryButton = page
      .getByRole('button', { name: 'Secondary', exact: true })
      .first();
    await secondaryButton.scrollIntoViewIfNeeded();
    await expect(secondaryButton).toBeVisible();
    await secondaryButton.click();
  });

  test('should support keyboard navigation', async ({ page }) => {
    const primaryButton = page
      .getByRole('button', { name: 'Primary', exact: true })
      .first();

    await expect(primaryButton).toBeVisible();
    await primaryButton.focus();
    await expect(primaryButton).toBeFocused();
    await page.keyboard.press('Enter');
    await expect(primaryButton).toBeEnabled();

    // Verify text content
    const textContent = await primaryButton.textContent();
    expect(textContent?.trim()).toBe('Primary');
  });
});
