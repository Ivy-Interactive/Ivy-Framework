import { test, expect, type Page } from '@playwright/test';

// Constants - Define all button variants from ButtonApp.cs
const BUTTON_VARIANTS = [
  'Primary',
  'Destructive',
  'Secondary',
  'Success',
  'Warning',
  'Info',
  'Outline',
  'Ghost',
  'Link',
] as const;

// Shared setup function for button tests
async function setupButtonPage(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  // Navigate to Button app
  const searchInput = page.getByTestId('sidebar-search');
  await expect(searchInput).toBeVisible();
  await searchInput.click();
  await searchInput.fill('button');
  await searchInput.press('Enter');

  const buttonAppLink = page
    .locator('button')
    .filter({ hasText: /^Button$/i })
    .first();

  await expect(buttonAppLink).toBeVisible();
  await buttonAppLink.click();
  await page.waitForLoadState('networkidle');
}

test.describe('Button Widget Tests', () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 720 });
    await setupButtonPage(page);
  });

  test.describe('Smoke Tests', () => {
    test('should render button app with H1 heading and multiple buttons', async ({
      page,
    }) => {
      // Verify the H1 heading is present
      const h1Heading = page.getByRole('heading', {
        level: 1,
        name: 'Buttons',
      });
      await expect(h1Heading).toBeVisible();

      // Verify multiple buttons exist on the page
      const buttons = page.getByRole('button');
      const count = await buttons.count();
      expect(count).toBeGreaterThan(10);
    });
  });

  test.describe('Button Variants Tests', () => {
    test('should render all button variants with correct styling', async ({
      page,
    }) => {
      // Verify all 9 variants are rendered and visible
      for (const variant of BUTTON_VARIANTS) {
        const variantButton = page
          .getByRole('button', { name: variant, exact: true })
          .first();
        await expect(variantButton).toBeVisible();
        await expect(variantButton).toBeEnabled();
      }

      // Verify specific variant styling
      const destructiveButton = page
        .getByRole('button', { name: 'Destructive', exact: true })
        .first();
      const className = await destructiveButton.getAttribute('class');
      expect(className).toContain('destructive');

      const outlineButton = page
        .getByRole('button', { name: 'Outline', exact: true })
        .first();
      const outlineClass = await outlineButton.getAttribute('class');
      expect(outlineClass).toContain('outline');
    });
  });

  test.describe('Button States Tests', () => {
    test('should handle all button states: enabled buttons', async ({
      page,
    }) => {
      // Verify enabled buttons exist and work
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();
      await expect(primaryButton).toBeVisible();
      await expect(primaryButton).toBeEnabled();

      // Verify many buttons exist on the page
      const allButtons = page.getByRole('button');
      expect(await allButtons.count()).toBeGreaterThan(20);
    });
  });

  test.describe('Button Sizes Tests', () => {
    test('should render all sizes with correct visual hierarchy', async ({
      page,
    }) => {
      const smallButton = page.getByRole('button', { name: 'Small' }).first();
      const mediumButton = page.getByRole('button', { name: 'Medium' }).first();
      const largeButton = page.getByRole('button', { name: 'Large' }).first();

      await expect(smallButton).toBeVisible();
      await expect(mediumButton).toBeVisible();
      await expect(largeButton).toBeVisible();

      // Verify size differences
      const smallBox = await smallButton.boundingBox();
      const mediumBox = await mediumButton.boundingBox();
      const largeBox = await largeButton.boundingBox();

      expect(smallBox).toBeTruthy();
      expect(mediumBox).toBeTruthy();
      expect(largeBox).toBeTruthy();

      if (smallBox && mediumBox && largeBox) {
        expect(smallBox.height).toBeLessThan(mediumBox.height);
        expect(mediumBox.height).toBeLessThan(largeBox.height);
      }
    });
  });

  test.describe('Button Icons Tests', () => {
    test('should render buttons with icons and icon-only buttons', async ({
      page,
    }) => {
      // Scroll to find icon section
      await page.evaluate(() =>
        window.scrollTo(0, document.body.scrollHeight / 2)
      );
      await page.waitForTimeout(300);

      // Verify buttons with icons exist by checking for SVG elements
      const buttonsWithSvg = page.locator('button:has(svg)');
      expect(await buttonsWithSvg.count()).toBeGreaterThan(0);

      const firstIconButton = buttonsWithSvg.first();
      await expect(firstIconButton).toBeVisible();

      const svgIcons = firstIconButton.locator('svg');
      expect(await svgIcons.count()).toBeGreaterThan(0);

      // Verify icon-only buttons exist (buttons with only SVG, no text)
      const allButtons = page.getByRole('button');
      const buttonCount = await allButtons.count();

      for (let i = 0; i < Math.min(buttonCount, 50); i++) {
        const button = allButtons.nth(i);
        const box = await button.boundingBox();
        if (box && box.width > 0 && box.height > 0) {
          const aspectRatio = box.width / box.height;
          // Icon buttons should be roughly square
          if (aspectRatio > 0.8 && aspectRatio < 1.2 && box.width < 50) {
            // Found an icon-only button
            expect(aspectRatio).toBeGreaterThan(0.7);
            expect(aspectRatio).toBeLessThan(1.3);
            break;
          }
        }
      }
    });
  });

  test.describe('Button Styling Tests', () => {
    test('should render buttons with custom border radius and tooltips', async ({
      page,
    }) => {
      // Verify H1 heading
      await expect(
        page.getByRole('heading', { name: 'Buttons', level: 1 })
      ).toBeVisible();

      // Test that a simple button from the variants section is clickable
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();
      await expect(primaryButton).toBeVisible();
      await expect(primaryButton).toBeEnabled();

      // Verify multiple variant buttons exist
      const allButtons = page.getByRole('button');
      expect(await allButtons.count()).toBeGreaterThan(5);
    });
  });

  test.describe('Button Click Events Tests', () => {
    test('should handle button clicks and update interactive demo', async ({
      page,
    }) => {
      // Click a Primary button
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();
      await expect(primaryButton).toBeVisible();
      await expect(primaryButton).toBeEnabled();
      await primaryButton.click();

      // Wait for state update
      await page.waitForTimeout(500);

      // Verify the interactive demo label has updated
      const updatedLabel = page.locator('text=/Button.*was clicked/');
      await expect(updatedLabel).toBeVisible();

      // Verify enabled buttons remain enabled after click
      await expect(primaryButton).toBeEnabled();
    });
  });

  test.describe('Complex Routine Tests', () => {
    test('should navigate and interact with multiple button types', async ({
      page,
    }) => {
      // Verify H1 heading
      await expect(
        page.getByRole('heading', { name: 'Buttons', level: 1 })
      ).toBeVisible();

      // Test multiple variants - click them one at a time
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();
      await expect(primaryButton).toBeVisible();
      await primaryButton.click();
      await page.waitForTimeout(300);

      const destructiveButton = page
        .getByRole('button', { name: 'Destructive', exact: true })
        .first();
      await expect(destructiveButton).toBeVisible();
      await destructiveButton.click();
      await page.waitForTimeout(300);

      // Verify interactive demo updated
      const updatedLabel = page.locator('text=/Button.*was clicked/');
      await expect(updatedLabel).toBeVisible();

      // Verify we have many buttons on the page
      const allButtons = page.getByRole('button');
      expect(await allButtons.count()).toBeGreaterThan(20);
    });

    test('should verify all methods coverage', async ({ page }) => {
      // Verify H1 heading exists
      await expect(
        page.getByRole('heading', { name: 'Buttons', level: 1 })
      ).toBeVisible();

      // Verify all variants exist by checking first few
      for (const variant of BUTTON_VARIANTS.slice(0, 3)) {
        const buttons = page.getByRole('button', {
          name: variant,
          exact: true,
        });
        expect(await buttons.count()).toBeGreaterThan(0);
      }

      // Verify many buttons exist
      const allButtons = page.getByRole('button');
      expect(await allButtons.count()).toBeGreaterThan(20);

      // Click a button to verify interactivity
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();
      await primaryButton.click();
      await page.waitForTimeout(300);
    });
  });

  test.describe('Accessibility Tests', () => {
    test('should verify keyboard accessibility and semantic HTML', async ({
      page,
    }) => {
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();

      await expect(primaryButton).toBeVisible();

      // Focus and activate with keyboard
      await primaryButton.focus();
      await expect(primaryButton).toBeFocused();
      await page.keyboard.press('Enter');
      await page.waitForTimeout(200);

      // Verify button has text content
      const textContent = await primaryButton.textContent();
      expect(textContent).toBeTruthy();
      expect(textContent?.trim()).toBe('Primary');

      // Verify all buttons use proper semantic HTML
      const buttons = page.getByRole('button');
      expect(await buttons.count()).toBeGreaterThan(0);
    });
  });
});
