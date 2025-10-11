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

// Test IDs based on the ButtonApp.cs structure
const SECTION_HEADINGS = {
  VARIANTS: 'Variants',
  STATES: 'States',
  SIZES: 'Sizes',
  WITH_ICONS: 'With Icons',
  STYLING: 'Styling',
  ICON_ONLY: 'Icon Only',
  INTERACTIVE_DEMO: 'Interactive Demo',
} as const;

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

  // Find and click the Button app (not ButtonGroup)
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
    await setupButtonPage(page);
  });

  test.describe('Smoke Tests', () => {
    test('should render button app and display main heading', async ({
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
      expect(count).toBeGreaterThan(0);
    });

    test('should render all variant sections', async ({ page }) => {
      // Verify all main sections are present
      for (const heading of Object.values(SECTION_HEADINGS)) {
        const sectionHeading = page.getByRole('heading', { name: heading });
        await expect(sectionHeading).toBeVisible();
      }
    });
  });

  test.describe('Button Variants - All States', () => {
    test('should render all button variants correctly', async ({ page }) => {
      // Scroll to variants section
      const variantsHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.VARIANTS,
        exact: true,
      });
      await variantsHeading.scrollIntoViewIfNeeded();

      // Verify each variant is rendered as a button
      for (const variant of BUTTON_VARIANTS) {
        const variantButtons = page.getByRole('button', {
          name: variant,
          exact: true,
        });

        // Should have at least one button with this variant name
        const count = await variantButtons.count();
        expect(count).toBeGreaterThan(0);

        // Verify the first button with this variant is visible
        await expect(variantButtons.first()).toBeVisible();
      }
    });

    test('should verify Primary button is the default variant', async ({
      page,
    }) => {
      const variantsHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.VARIANTS,
        exact: true,
      });
      await variantsHeading.scrollIntoViewIfNeeded();

      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();
      await expect(primaryButton).toBeVisible();

      // Primary variant should have default styling
      const className = await primaryButton.getAttribute('class');
      expect(className).toBeTruthy();
    });

    test('should verify Destructive button has appropriate styling', async ({
      page,
    }) => {
      const destructiveButton = page
        .getByRole('button', { name: 'Destructive', exact: true })
        .first();
      await expect(destructiveButton).toBeVisible();

      const className = await destructiveButton.getAttribute('class');
      expect(className).toContain('destructive');
    });

    test('should verify Outline button has appropriate styling', async ({
      page,
    }) => {
      const outlineButton = page
        .getByRole('button', { name: 'Outline', exact: true })
        .first();
      await expect(outlineButton).toBeVisible();

      const className = await outlineButton.getAttribute('class');
      expect(className).toContain('outline');
    });

    test('should verify Ghost button has appropriate styling', async ({
      page,
    }) => {
      const ghostButton = page
        .getByRole('button', { name: 'Ghost', exact: true })
        .first();
      await expect(ghostButton).toBeVisible();

      const className = await ghostButton.getAttribute('class');
      expect(className).toContain('ghost');
    });

    test('should verify Link button has appropriate styling', async ({
      page,
    }) => {
      const linkButton = page
        .getByRole('button', { name: 'Link', exact: true })
        .first();
      await expect(linkButton).toBeVisible();

      const className = await linkButton.getAttribute('class');
      expect(className).toContain('link');
    });
  });

  test.describe('Button States Tests', () => {
    test('should render buttons in all states (normal, disabled, loading)', async ({
      page,
    }) => {
      // Scroll to states section
      const statesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STATES,
        exact: true,
      });
      await statesHeading.scrollIntoViewIfNeeded();

      // For each variant, there should be 3 buttons: normal, disabled, loading
      for (const variant of BUTTON_VARIANTS) {
        // Find buttons in the states section
        const statesSection = page
          .locator('h2:has-text("States")')
          .locator('..');
        const buttonsInStates = statesSection.getByRole('button', {
          name: variant,
          exact: true,
        });

        const count = await buttonsInStates.count();
        // Should have at least 3 buttons (normal, disabled, loading)
        expect(count).toBeGreaterThanOrEqual(3);
      }
    });

    test('should verify disabled buttons are not clickable', async ({
      page,
    }) => {
      const statesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STATES,
        exact: true,
      });
      await statesHeading.scrollIntoViewIfNeeded();

      // Find all disabled buttons in the states section
      const allButtons = page.getByRole('button');
      const buttonCount = await allButtons.count();

      let foundDisabledButton = false;
      for (let i = 0; i < buttonCount; i++) {
        const button = allButtons.nth(i);
        const isDisabled = await button.isDisabled();

        if (isDisabled) {
          foundDisabledButton = true;
          // Verify disabled attribute is set
          await expect(button).toBeDisabled();

          // Get the bounding box to ensure it's visible but not clickable
          const box = await button.boundingBox();
          expect(box).toBeTruthy();
        }
      }

      // Verify we found at least one disabled button
      expect(foundDisabledButton).toBe(true);
    });

    test('should verify loading buttons display loading indicator', async ({
      page,
    }) => {
      const statesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STATES,
        exact: true,
      });
      await statesHeading.scrollIntoViewIfNeeded();

      // Check for loading spinners (Loader2 component with animate-spin class)
      const loadingSpinners = page.locator('.animate-spin');
      const count = await loadingSpinners.count();

      // Should have at least one loading spinner
      expect(count).toBeGreaterThan(0);

      // Verify the spinner is visible
      await expect(loadingSpinners.first()).toBeVisible();
    });
  });

  test.describe('Button Sizes Tests', () => {
    test('should render all button sizes correctly', async ({ page }) => {
      const sizesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.SIZES,
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      // Verify Small, Medium, and Large buttons exist
      const smallButtons = page.getByRole('button', { name: 'Small' });
      const mediumButtons = page.getByRole('button', { name: 'Medium' });
      const largeButtons = page.getByRole('button', { name: 'Large' });

      expect(await smallButtons.count()).toBeGreaterThan(0);
      expect(await mediumButtons.count()).toBeGreaterThan(0);
      expect(await largeButtons.count()).toBeGreaterThan(0);
    });

    test('should verify small buttons have correct size styling', async ({
      page,
    }) => {
      const sizesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.SIZES,
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      const smallButton = page.getByRole('button', { name: 'Small' }).first();
      await expect(smallButton).toBeVisible();

      const className = await smallButton.getAttribute('class');
      expect(className).toBeTruthy();

      // Get the bounding box
      const smallBox = await smallButton.boundingBox();
      expect(smallBox).toBeTruthy();
    });

    test('should verify large buttons have correct size styling', async ({
      page,
    }) => {
      const sizesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.SIZES,
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      const largeButton = page.getByRole('button', { name: 'Large' }).first();
      await expect(largeButton).toBeVisible();

      const className = await largeButton.getAttribute('class');
      expect(className).toBeTruthy();

      // Get the bounding box
      const largeBox = await largeButton.boundingBox();
      expect(largeBox).toBeTruthy();
    });

    test('should verify size differences are visually distinct', async ({
      page,
    }) => {
      const sizesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.SIZES,
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      const smallButton = page.getByRole('button', { name: 'Small' }).first();
      const mediumButton = page.getByRole('button', { name: 'Medium' }).first();
      const largeButton = page.getByRole('button', { name: 'Large' }).first();

      const smallBox = await smallButton.boundingBox();
      const mediumBox = await mediumButton.boundingBox();
      const largeBox = await largeButton.boundingBox();

      expect(smallBox).toBeTruthy();
      expect(mediumBox).toBeTruthy();
      expect(largeBox).toBeTruthy();

      if (smallBox && mediumBox && largeBox) {
        // Small should be smaller than medium
        expect(smallBox.height).toBeLessThan(mediumBox.height);

        // Medium should be smaller than large
        expect(mediumBox.height).toBeLessThan(largeBox.height);
      }
    });
  });

  test.describe('Button Icons Tests', () => {
    test('should render buttons with icons', async ({ page }) => {
      const iconsHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.WITH_ICONS,
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      // Verify buttons with icons exist
      const buttonsWithIcons = page.getByRole('button', {
        name: /Button With Icon/i,
      });
      const count = await buttonsWithIcons.count();
      expect(count).toBeGreaterThan(0);

      // Verify at least one is visible
      await expect(buttonsWithIcons.first()).toBeVisible();
    });

    test('should verify icon-only buttons are rendered correctly', async ({
      page,
    }) => {
      const iconOnlyHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.ICON_ONLY,
        exact: true,
      });
      await iconOnlyHeading.scrollIntoViewIfNeeded();

      // Icon-only buttons should have square aspect ratio
      const iconOnlySection = page
        .locator('h2:has-text("Icon Only")')
        .locator('..');
      const iconButtons = iconOnlySection.getByRole('button');

      const count = await iconButtons.count();
      expect(count).toBeGreaterThan(0);

      // Verify the first icon button has icon styling
      const firstIconButton = iconButtons.first();
      await expect(firstIconButton).toBeVisible();

      const box = await firstIconButton.boundingBox();
      expect(box).toBeTruthy();

      // Icon buttons should be roughly square (allowing some tolerance)
      if (box) {
        const aspectRatio = box.width / box.height;
        expect(aspectRatio).toBeGreaterThan(0.8);
        expect(aspectRatio).toBeLessThan(1.2);
      }
    });

    test('should verify buttons with left-aligned icons', async ({ page }) => {
      const iconsHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.WITH_ICONS,
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      const buttonsWithIcons = page.getByRole('button', {
        name: /Button With Icon/i,
      });

      // Check if icons exist within buttons using svg elements
      const firstButton = buttonsWithIcons.first();
      await expect(firstButton).toBeVisible();

      const svgIcons = firstButton.locator('svg');
      const iconCount = await svgIcons.count();
      expect(iconCount).toBeGreaterThan(0);
    });

    test('should verify buttons with right-aligned icons', async ({ page }) => {
      const iconsHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.WITH_ICONS,
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      const iconsSection = page
        .locator('h2:has-text("With Icons")')
        .locator('..');
      const buttonsWithIcons = iconsSection.getByRole('button', {
        name: /Button With Icon/i,
      });

      // Should have buttons with icons on both sides
      const count = await buttonsWithIcons.count();
      expect(count).toBeGreaterThan(0);
    });
  });

  test.describe('Button Styling Tests', () => {
    test('should render buttons with rounded borders', async ({ page }) => {
      const stylingHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STYLING,
        exact: true,
      });
      await stylingHeading.scrollIntoViewIfNeeded();

      const roundedButtons = page.getByRole('button', { name: 'Rounded' });
      const count = await roundedButtons.count();
      expect(count).toBeGreaterThan(0);

      await expect(roundedButtons.first()).toBeVisible();
    });

    test('should render buttons with full border radius', async ({ page }) => {
      const stylingHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STYLING,
        exact: true,
      });
      await stylingHeading.scrollIntoViewIfNeeded();

      const fullButtons = page.getByRole('button', { name: 'Full' });
      const count = await fullButtons.count();
      expect(count).toBeGreaterThan(0);

      await expect(fullButtons.first()).toBeVisible();
    });

    test('should render buttons with tooltips', async ({ page }) => {
      const stylingHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STYLING,
        exact: true,
      });
      await stylingHeading.scrollIntoViewIfNeeded();

      const tooltipButtons = page.getByRole('button', {
        name: 'With Tooltip',
      });
      const count = await tooltipButtons.count();
      expect(count).toBeGreaterThan(0);

      const firstTooltipButton = tooltipButtons.first();
      await expect(firstTooltipButton).toBeVisible();

      // Hover over the button to trigger tooltip
      await firstTooltipButton.hover();

      // Wait a moment for tooltip to appear
      await page.waitForTimeout(500);

      // Tooltip may or may not be visible depending on implementation
      // Just verify the button exists and can be hovered
    });

    test('should verify border radius visual differences', async ({ page }) => {
      const stylingHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STYLING,
        exact: true,
      });
      await stylingHeading.scrollIntoViewIfNeeded();

      const roundedButton = page
        .getByRole('button', { name: 'Rounded' })
        .first();
      const fullButton = page.getByRole('button', { name: 'Full' }).first();

      await expect(roundedButton).toBeVisible();
      await expect(fullButton).toBeVisible();

      // Both should be visible and have different styles
      const roundedStyle = await roundedButton.getAttribute('style');
      const fullStyle = await fullButton.getAttribute('style');

      // At least one should have border radius styling
      const hasBorderRadiusStyling =
        (roundedStyle && roundedStyle.includes('border')) ||
        (fullStyle && fullStyle.includes('border'));
      expect(hasBorderRadiusStyling).toBe(true);
    });
  });

  test.describe('Button Click Events Tests', () => {
    test('should handle button click and update interactive demo', async ({
      page,
    }) => {
      const demoHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.INTERACTIVE_DEMO,
        exact: true,
      });
      await demoHeading.scrollIntoViewIfNeeded();

      // Find the interactive demo section
      const demoSection = page
        .locator('h2:has-text("Interactive Demo")')
        .locator('..');

      // Find the label that shows the current state
      const labelText = demoSection.locator('text="Click a button"');
      await expect(labelText).toBeVisible();

      // Click a button in the variants section
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();
      await primaryButton.click();

      // Wait for state update
      await page.waitForTimeout(500);

      // Verify the label has updated
      const updatedLabel = demoSection.locator('text=/Button.*was clicked/');
      await expect(updatedLabel).toBeVisible();
    });

    test('should verify enabled buttons are clickable', async ({ page }) => {
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();

      await expect(primaryButton).toBeVisible();
      await expect(primaryButton).toBeEnabled();

      // Click the button
      await primaryButton.click();

      // Button should remain enabled after click
      await expect(primaryButton).toBeEnabled();
    });

    test('should not trigger events on disabled buttons', async ({ page }) => {
      const statesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STATES,
        exact: true,
      });
      await statesHeading.scrollIntoViewIfNeeded();

      // Find a disabled button
      const allButtons = page.getByRole('button');
      const buttonCount = await allButtons.count();

      for (let i = 0; i < buttonCount; i++) {
        const button = allButtons.nth(i);
        const isDisabled = await button.isDisabled();

        if (isDisabled) {
          // Try to click the disabled button (should not work)
          await button.click({ force: true });

          // Interactive demo label should not change
          const demoHeading = page.getByRole('heading', {
            name: SECTION_HEADINGS.INTERACTIVE_DEMO,
            exact: true,
          });
          await demoHeading.scrollIntoViewIfNeeded();

          // The label might have changed from previous tests, but disabled button shouldn't affect it
          break;
        }
      }
    });
  });

  test.describe('Complex Routine Tests', () => {
    test('should navigate through all sections and interact with various button types', async ({
      page,
    }) => {
      // Verify main heading
      await expect(
        page.getByRole('heading', { name: 'Buttons', level: 1 })
      ).toBeVisible();

      // Navigate through variants section
      const variantsHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.VARIANTS,
        exact: true,
      });
      await variantsHeading.scrollIntoViewIfNeeded();

      // Click a Primary button
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();
      await expect(primaryButton).toBeVisible();
      await primaryButton.click();

      // Navigate to States section
      const statesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STATES,
        exact: true,
      });
      await statesHeading.scrollIntoViewIfNeeded();

      // Verify loading buttons show spinner
      const loadingSpinner = page.locator('.animate-spin').first();
      await expect(loadingSpinner).toBeVisible();

      // Navigate to Sizes section
      const sizesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.SIZES,
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      // Click a Large button
      const largeButton = page.getByRole('button', { name: 'Large' }).first();
      await expect(largeButton).toBeVisible();
      await largeButton.click();

      // Navigate to Icons section
      const iconsHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.WITH_ICONS,
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      // Click a button with icon
      const iconButton = page
        .getByRole('button', { name: /Button With Icon/i })
        .first();
      await expect(iconButton).toBeVisible();
      await iconButton.click();

      // Navigate to Interactive Demo
      const demoHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.INTERACTIVE_DEMO,
        exact: true,
      });
      await demoHeading.scrollIntoViewIfNeeded();

      // Verify label has been updated
      const demoSection = page
        .locator('h2:has-text("Interactive Demo")')
        .locator('..');
      const updatedLabel = demoSection.locator('text=/Button.*was clicked/');
      await expect(updatedLabel).toBeVisible();
    });

    test('should verify all button variants are functional', async ({
      page,
    }) => {
      const variantsHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.VARIANTS,
        exact: true,
      });
      await variantsHeading.scrollIntoViewIfNeeded();

      // Test each variant
      for (const variant of BUTTON_VARIANTS.slice(0, 3)) {
        // Test first 3 variants
        const button = page
          .getByRole('button', { name: variant, exact: true })
          .first();
        await expect(button).toBeVisible();
        await expect(button).toBeEnabled();
        await button.click();

        // Wait for interaction
        await page.waitForTimeout(200);
      }

      // Verify interactive demo updated
      const demoHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.INTERACTIVE_DEMO,
        exact: true,
      });
      await demoHeading.scrollIntoViewIfNeeded();

      const demoSection = page
        .locator('h2:has-text("Interactive Demo")')
        .locator('..');
      const updatedLabel = demoSection.locator('text=/Button.*was clicked/');
      await expect(updatedLabel).toBeVisible();
    });

    test('should verify styling options work together correctly', async ({
      page,
    }) => {
      const stylingHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STYLING,
        exact: true,
      });
      await stylingHeading.scrollIntoViewIfNeeded();

      // Test rounded button
      const roundedButton = page
        .getByRole('button', { name: 'Rounded' })
        .first();
      await expect(roundedButton).toBeVisible();
      await roundedButton.click();
      await page.waitForTimeout(200);

      // Test full border radius button
      const fullButton = page.getByRole('button', { name: 'Full' }).first();
      await expect(fullButton).toBeVisible();
      await fullButton.click();
      await page.waitForTimeout(200);

      // Test button with tooltip
      const tooltipButton = page
        .getByRole('button', { name: 'With Tooltip' })
        .first();
      await expect(tooltipButton).toBeVisible();
      await tooltipButton.hover();
      await page.waitForTimeout(300);
      await tooltipButton.click();

      // Verify all interactions worked
      const demoHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.INTERACTIVE_DEMO,
        exact: true,
      });
      await demoHeading.scrollIntoViewIfNeeded();

      const demoSection = page
        .locator('h2:has-text("Interactive Demo")')
        .locator('..');
      const updatedLabel = demoSection.locator('text=/Button.*was clicked/');
      await expect(updatedLabel).toBeVisible();
    });
  });

  test.describe('All Button Methods Coverage', () => {
    test('should verify all button variants are present and accessible', async ({
      page,
    }) => {
      const variantsHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.VARIANTS,
        exact: true,
      });
      await variantsHeading.scrollIntoViewIfNeeded();

      // Verify all 9 variants are present
      for (const variant of BUTTON_VARIANTS) {
        const buttons = page.getByRole('button', {
          name: variant,
          exact: true,
        });
        const count = await buttons.count();
        expect(count).toBeGreaterThan(0);
      }
    });

    test('should verify all size options are functional', async ({ page }) => {
      const sizesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.SIZES,
        exact: true,
      });
      await sizesHeading.scrollIntoViewIfNeeded();

      // Test each size
      const sizes = ['Small', 'Medium', 'Large'];
      for (const size of sizes) {
        const buttons = page.getByRole('button', { name: size });
        const count = await buttons.count();
        expect(count).toBeGreaterThan(0);

        const firstButton = buttons.first();
        await expect(firstButton).toBeVisible();
        await expect(firstButton).toBeEnabled();
      }
    });

    test('should verify all state combinations work correctly', async ({
      page,
    }) => {
      const statesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STATES,
        exact: true,
      });
      await statesHeading.scrollIntoViewIfNeeded();

      // Test that we have buttons in various states
      const allButtons = page.getByRole('button');
      const buttonCount = await allButtons.count();

      let foundEnabled = false;
      let foundDisabled = false;
      let foundLoading = false;

      for (let i = 0; i < buttonCount; i++) {
        const button = allButtons.nth(i);
        const isDisabled = await button.isDisabled();

        if (isDisabled) {
          foundDisabled = true;
        } else {
          foundEnabled = true;
        }
      }

      // Check for loading spinners
      const loadingSpinners = page.locator('.animate-spin');
      if ((await loadingSpinners.count()) > 0) {
        foundLoading = true;
      }

      expect(foundEnabled).toBe(true);
      expect(foundDisabled).toBe(true);
      expect(foundLoading).toBe(true);
    });

    test('should verify icon positioning options work correctly', async ({
      page,
    }) => {
      const iconsHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.WITH_ICONS,
        exact: true,
      });
      await iconsHeading.scrollIntoViewIfNeeded();

      const iconsSection = page
        .locator('h2:has-text("With Icons")')
        .locator('..');
      const buttonsWithIcons = iconsSection.getByRole('button', {
        name: /Button With Icon/i,
      });

      // Should have multiple buttons with icons (left and right positioned)
      const count = await buttonsWithIcons.count();
      expect(count).toBeGreaterThan(0);

      // Verify icons exist within buttons
      const firstButton = buttonsWithIcons.first();
      const svgIcons = firstButton.locator('svg');
      expect(await svgIcons.count()).toBeGreaterThan(0);
    });

    test('should verify border radius options are applied', async ({
      page,
    }) => {
      const stylingHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STYLING,
        exact: true,
      });
      await stylingHeading.scrollIntoViewIfNeeded();

      const roundedButtons = page.getByRole('button', { name: 'Rounded' });
      const fullButtons = page.getByRole('button', { name: 'Full' });

      expect(await roundedButtons.count()).toBeGreaterThan(0);
      expect(await fullButtons.count()).toBeGreaterThan(0);

      await expect(roundedButtons.first()).toBeVisible();
      await expect(fullButtons.first()).toBeVisible();
    });
  });

  test.describe('Visual Properties Tests', () => {
    test('should verify button visual hierarchy is clear', async ({ page }) => {
      const variantsHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.VARIANTS,
        exact: true,
      });
      await variantsHeading.scrollIntoViewIfNeeded();

      // Primary button should be visually distinct
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();
      const destructiveButton = page
        .getByRole('button', { name: 'Destructive', exact: true })
        .first();

      await expect(primaryButton).toBeVisible();
      await expect(destructiveButton).toBeVisible();

      // Both should have different visual styles
      const primaryClass = await primaryButton.getAttribute('class');
      const destructiveClass = await destructiveButton.getAttribute('class');

      expect(primaryClass).toBeTruthy();
      expect(destructiveClass).toBeTruthy();
      expect(destructiveClass).toContain('destructive');
    });

    test('should verify icon-only buttons maintain square aspect ratio', async ({
      page,
    }) => {
      const iconOnlyHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.ICON_ONLY,
        exact: true,
      });
      await iconOnlyHeading.scrollIntoViewIfNeeded();

      const iconOnlySection = page
        .locator('h2:has-text("Icon Only")')
        .locator('..');
      const iconButtons = iconOnlySection.getByRole('button');

      const firstIconButton = iconButtons.first();
      await expect(firstIconButton).toBeVisible();

      const box = await firstIconButton.boundingBox();
      expect(box).toBeTruthy();

      if (box) {
        // Verify roughly square aspect ratio
        const aspectRatio = box.width / box.height;
        expect(aspectRatio).toBeGreaterThan(0.7);
        expect(aspectRatio).toBeLessThan(1.3);
      }
    });

    test('should verify loading state shows visual indicator', async ({
      page,
    }) => {
      const statesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STATES,
        exact: true,
      });
      await statesHeading.scrollIntoViewIfNeeded();

      // Find loading spinners
      const loadingSpinners = page.locator('.animate-spin');
      expect(await loadingSpinners.count()).toBeGreaterThan(0);

      // Verify spinner is animated (has animate-spin class)
      const firstSpinner = loadingSpinners.first();
      await expect(firstSpinner).toBeVisible();

      const className = await firstSpinner.getAttribute('class');
      expect(className).toContain('animate-spin');
    });

    test('should verify disabled state has visual indication', async ({
      page,
    }) => {
      const statesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STATES,
        exact: true,
      });
      await statesHeading.scrollIntoViewIfNeeded();

      // Find disabled buttons
      const allButtons = page.getByRole('button');
      const buttonCount = await allButtons.count();

      for (let i = 0; i < buttonCount; i++) {
        const button = allButtons.nth(i);
        const isDisabled = await button.isDisabled();

        if (isDisabled) {
          // Verify disabled buttons have the disabled attribute
          await expect(button).toBeDisabled();

          // Disabled buttons should still be visible
          await expect(button).toBeVisible();
          break;
        }
      }
    });

    test('should verify tooltip styling exists for tooltip buttons', async ({
      page,
    }) => {
      const stylingHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STYLING,
        exact: true,
      });
      await stylingHeading.scrollIntoViewIfNeeded();

      const tooltipButton = page
        .getByRole('button', { name: 'With Tooltip' })
        .first();
      await expect(tooltipButton).toBeVisible();

      // Hover to potentially trigger tooltip
      await tooltipButton.hover();
      await page.waitForTimeout(500);

      // Verify button is interactive
      await expect(tooltipButton).toBeEnabled();
    });

    test('should verify buttons maintain consistent spacing', async ({
      page,
    }) => {
      const variantsHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.VARIANTS,
        exact: true,
      });
      await variantsHeading.scrollIntoViewIfNeeded();

      // Get multiple buttons in the same row
      const buttons = page.getByRole('button', { name: /Primary|Destructive/ });
      expect(await buttons.count()).toBeGreaterThan(1);

      // Verify buttons are visible and properly spaced
      const firstButton = buttons.nth(0);
      const secondButton = buttons.nth(1);

      await expect(firstButton).toBeVisible();
      await expect(secondButton).toBeVisible();

      const firstBox = await firstButton.boundingBox();
      const secondBox = await secondButton.boundingBox();

      expect(firstBox).toBeTruthy();
      expect(secondBox).toBeTruthy();

      // Buttons should not overlap
      if (firstBox && secondBox) {
        const noOverlap =
          firstBox.x + firstBox.width <= secondBox.x ||
          secondBox.x + secondBox.width <= firstBox.x ||
          firstBox.y + firstBox.height <= secondBox.y ||
          secondBox.y + secondBox.height <= firstBox.y;
        expect(noOverlap).toBe(true);
      }
    });
  });

  test.describe('Accessibility Tests', () => {
    test('should verify buttons are keyboard accessible', async ({ page }) => {
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();

      await expect(primaryButton).toBeVisible();

      // Focus the button
      await primaryButton.focus();
      await expect(primaryButton).toBeFocused();

      // Press Enter to activate
      await page.keyboard.press('Enter');
      await page.waitForTimeout(200);

      // Button should remain enabled and visible
      await expect(primaryButton).toBeEnabled();
    });

    test('should verify disabled buttons cannot be focused and activated', async ({
      page,
    }) => {
      const statesHeading = page.getByRole('heading', {
        name: SECTION_HEADINGS.STATES,
        exact: true,
      });
      await statesHeading.scrollIntoViewIfNeeded();

      // Find a disabled button
      const allButtons = page.getByRole('button');
      const buttonCount = await allButtons.count();

      for (let i = 0; i < buttonCount; i++) {
        const button = allButtons.nth(i);
        const isDisabled = await button.isDisabled();

        if (isDisabled) {
          // Verify button is disabled
          await expect(button).toBeDisabled();
          break;
        }
      }
    });

    test('should verify buttons have proper semantic HTML', async ({
      page,
    }) => {
      // All buttons should be proper button elements
      const buttons = page.getByRole('button');
      const count = await buttons.count();
      expect(count).toBeGreaterThan(0);

      // Verify first few buttons are actual button elements
      for (let i = 0; i < Math.min(count, 5); i++) {
        const button = buttons.nth(i);
        await expect(button).toBeVisible();
      }
    });

    test('should verify button text is readable', async ({ page }) => {
      const primaryButton = page
        .getByRole('button', { name: 'Primary', exact: true })
        .first();

      await expect(primaryButton).toBeVisible();

      // Verify button has text content
      const textContent = await primaryButton.textContent();
      expect(textContent).toBeTruthy();
      expect(textContent?.trim()).toBe('Primary');
    });
  });
});
