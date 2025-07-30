import { test, expect, type Page } from '@playwright/test';

// Shared setup function
async function setupBoolInputPage(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  // Find the sidebar search input
  const searchInput = page.getByTestId('sidebar-search');
  await expect(searchInput).toBeVisible();

  // Click the search input
  await searchInput.click();
  // Type 'bool'
  await searchInput.fill('bool');
  // Press Enter
  await searchInput.press('Enter');

  const firstResult = page
    .locator('button')
    .filter({ hasText: /Bool Input/i })
    .first();
  await firstResult.click();

  // Wait for navigation
  await page.waitForLoadState('networkidle');
}

test.describe('Bool Input Tests', () => {
  test.beforeEach(async ({ page }) => {
    await setupBoolInputPage(page);
  });

  test.describe('Checkbox Variants', () => {
    test('should test checkbox with description interactions', async ({
      page,
    }) => {
      // Test true state with description
      const trueCheckbox = page.getByTestId(
        'checkbox-true-state-width-description'
      );
      await expect(trueCheckbox).toBeVisible();
      await expect(trueCheckbox).toHaveAttribute('aria-checked', 'true');
      await trueCheckbox.click();
      await expect(trueCheckbox).toHaveAttribute('aria-checked', 'false');
      await trueCheckbox.click();
      await expect(trueCheckbox).toHaveAttribute('aria-checked', 'true');

      // Test false state with description
      const falseCheckbox = page.getByTestId(
        'checkbox-false-state-width-description'
      );
      await expect(falseCheckbox).toBeVisible();
      await expect(falseCheckbox).toHaveAttribute('aria-checked', 'false');
      await falseCheckbox.click();
      await expect(falseCheckbox).toHaveAttribute('aria-checked', 'true');
      await falseCheckbox.click();
      await expect(falseCheckbox).toHaveAttribute('aria-checked', 'false');

      // Test disabled state with description
      const disabledCheckbox = page.getByTestId(
        'checkbox-true-state-width-description-disabled'
      );
      await expect(disabledCheckbox).toBeVisible();
      await expect(disabledCheckbox).toBeDisabled();

      // Test invalid state with description
      const invalidCheckbox = page.getByTestId(
        'checkbox-true-state-width-description-invalid'
      );
      await expect(invalidCheckbox).toBeVisible();
      await expect(invalidCheckbox).toHaveAttribute('aria-checked', 'true');
      await invalidCheckbox.click();
      await expect(invalidCheckbox).toHaveAttribute('aria-checked', 'false');

      // Test null state with description
      const nullCheckbox = page.getByTestId(
        'checkbox-null-state-width-description'
      );
      await expect(nullCheckbox).toBeVisible();
      await expect(nullCheckbox).toHaveAttribute('aria-checked', 'mixed');
      await nullCheckbox.click();
      await expect(nullCheckbox).toHaveAttribute('aria-checked', 'true');
      await nullCheckbox.click();
      await expect(nullCheckbox).toHaveAttribute('aria-checked', 'false');
      await nullCheckbox.click();
      await expect(nullCheckbox).toHaveAttribute('aria-checked', 'mixed');
    });

    test('should test checkbox without description interactions', async ({
      page,
    }) => {
      // Test true state without description
      const trueCheckbox = page.getByTestId('checkbox-true-state-width');
      await expect(trueCheckbox).toBeVisible();
      await expect(trueCheckbox).toHaveAttribute('aria-checked', 'true');
      await trueCheckbox.click();
      await expect(trueCheckbox).toHaveAttribute('aria-checked', 'false');
      await trueCheckbox.click();
      await expect(trueCheckbox).toHaveAttribute('aria-checked', 'true');

      // Test false state without description
      const falseCheckbox = page.getByTestId('checkbox-false-state-width');
      await expect(falseCheckbox).toBeVisible();
      await expect(falseCheckbox).toHaveAttribute('aria-checked', 'false');
      await falseCheckbox.click();
      await expect(falseCheckbox).toHaveAttribute('aria-checked', 'true');
      await falseCheckbox.click();
      await expect(falseCheckbox).toHaveAttribute('aria-checked', 'false');

      // Test disabled state without description
      const disabledCheckbox = page.getByTestId(
        'checkbox-true-state-width-disabled'
      );
      await expect(disabledCheckbox).toBeVisible();
      await expect(disabledCheckbox).toBeDisabled();

      // Test invalid state without description
      const invalidCheckbox = page.getByTestId(
        'checkbox-true-state-width-invalid'
      );
      await expect(invalidCheckbox).toBeVisible();
      await expect(invalidCheckbox).toHaveAttribute('aria-checked', 'true');
      await invalidCheckbox.click();
      await expect(invalidCheckbox).toHaveAttribute('aria-checked', 'false');

      // Test null state without description
      const nullCheckbox = page.getByTestId('checkbox-null-state-width');
      await expect(nullCheckbox).toBeVisible();
      await expect(nullCheckbox).toHaveAttribute('aria-checked', 'mixed');
      await nullCheckbox.click();
      await expect(nullCheckbox).toHaveAttribute('aria-checked', 'true');
      await nullCheckbox.click();
      await expect(nullCheckbox).toHaveAttribute('aria-checked', 'false');
      await nullCheckbox.click();
      await expect(nullCheckbox).toHaveAttribute('aria-checked', 'mixed');
    });
  });

  test.describe('Switch Variants', () => {
    test('should test switch with description interactions', async ({
      page,
    }) => {
      // Test true state with description
      const trueSwitch = page.getByTestId(
        'switch-true-state-width-description'
      );
      await expect(trueSwitch).toBeVisible();
      await expect(trueSwitch).toHaveAttribute('aria-checked', 'true');
      await trueSwitch.click();
      await expect(trueSwitch).toHaveAttribute('aria-checked', 'false');
      await trueSwitch.click();
      await expect(trueSwitch).toHaveAttribute('aria-checked', 'true');

      // Test false state with description
      const falseSwitch = page.getByTestId(
        'switch-false-state-width-description'
      );
      await expect(falseSwitch).toBeVisible();
      await expect(falseSwitch).toHaveAttribute('aria-checked', 'false');
      await falseSwitch.click();
      await expect(falseSwitch).toHaveAttribute('aria-checked', 'true');
      await falseSwitch.click();
      await expect(falseSwitch).toHaveAttribute('aria-checked', 'false');

      // Test disabled state with description
      const disabledSwitch = page.getByTestId(
        'switch-true-state-width-description-disabled'
      );
      await expect(disabledSwitch).toBeVisible();
      await expect(disabledSwitch).toBeDisabled();

      // Test invalid state with description
      const invalidSwitch = page.getByTestId(
        'switch-true-state-width-description-invalid'
      );
      await expect(invalidSwitch).toBeVisible();
      await expect(invalidSwitch).toHaveAttribute('aria-checked', 'true');
      await invalidSwitch.click();
      await expect(invalidSwitch).toHaveAttribute('aria-checked', 'false');
    });

    test('should test switch without description interactions', async ({
      page,
    }) => {
      // Test true state without description
      const trueSwitch = page.getByTestId('switch-true-state-width');
      await expect(trueSwitch).toBeVisible();
      await expect(trueSwitch).toHaveAttribute('aria-checked', 'true');
      await trueSwitch.click();
      await expect(trueSwitch).toHaveAttribute('aria-checked', 'false');
      await trueSwitch.click();
      await expect(trueSwitch).toHaveAttribute('aria-checked', 'true');

      // Test false state without description
      const falseSwitch = page.getByTestId('switch-false-state-width');
      await expect(falseSwitch).toBeVisible();
      await expect(falseSwitch).toHaveAttribute('aria-checked', 'false');
      await falseSwitch.click();
      await expect(falseSwitch).toHaveAttribute('aria-checked', 'true');
      await falseSwitch.click();
      await expect(falseSwitch).toHaveAttribute('aria-checked', 'false');

      // Test disabled state without description
      const disabledSwitch = page.getByTestId(
        'switch-true-state-width-disabled'
      );
      await expect(disabledSwitch).toBeVisible();
      await expect(disabledSwitch).toBeDisabled();

      // Test invalid state without description
      const invalidSwitch = page.getByTestId('switch-true-state-width-invalid');
      await expect(invalidSwitch).toBeVisible();
      await expect(invalidSwitch).toHaveAttribute('aria-checked', 'true');
      await invalidSwitch.click();
      await expect(invalidSwitch).toHaveAttribute('aria-checked', 'false');
    });
  });

  test.describe('Toggle Variants', () => {
    test('should test toggle with description interactions', async ({
      page,
    }) => {
      // Test true state with description
      const trueToggle = page.getByTestId(
        'toggle-true-state-width-description'
      );
      await expect(trueToggle).toBeVisible();
      await expect(trueToggle).toHaveAttribute('aria-pressed', 'true');
      await trueToggle.click();
      await expect(trueToggle).toHaveAttribute('aria-pressed', 'false');
      await trueToggle.click();
      await expect(trueToggle).toHaveAttribute('aria-pressed', 'true');

      // Test false state with description
      const falseToggle = page.getByTestId(
        'toggle-false-state-width-description'
      );
      await expect(falseToggle).toBeVisible();
      await expect(falseToggle).toHaveAttribute('aria-pressed', 'false');
      await falseToggle.click();
      await expect(falseToggle).toHaveAttribute('aria-pressed', 'true');
      await falseToggle.click();
      await expect(falseToggle).toHaveAttribute('aria-pressed', 'false');

      // Test disabled state with description
      const disabledToggle = page.getByTestId(
        'toggle-true-state-width-description-disabled'
      );
      await expect(disabledToggle).toBeVisible();
      await expect(disabledToggle).toBeDisabled();

      // Test invalid state with description
      const invalidToggle = page.getByTestId(
        'toggle-true-state-width-description-invalid'
      );
      await expect(invalidToggle).toBeVisible();
      await expect(invalidToggle).toHaveAttribute('aria-pressed', 'true');
      await invalidToggle.click();
      await expect(invalidToggle).toHaveAttribute('aria-pressed', 'false');
    });

    test('should test toggle without description interactions', async ({
      page,
    }) => {
      // Test true state without description
      const trueToggle = page.getByTestId('toggle-true-state-width');
      await expect(trueToggle).toBeVisible();
      await expect(trueToggle).toHaveAttribute('aria-pressed', 'true');
      await trueToggle.click();
      await expect(trueToggle).toHaveAttribute('aria-pressed', 'false');
      await trueToggle.click();
      await expect(trueToggle).toHaveAttribute('aria-pressed', 'true');

      // Test false state without description
      const falseToggle = page.getByTestId('toggle-false-state-width');
      await expect(falseToggle).toBeVisible();
      await expect(falseToggle).toHaveAttribute('aria-pressed', 'false');
      await falseToggle.click();
      await expect(falseToggle).toHaveAttribute('aria-pressed', 'true');
      await falseToggle.click();
      await expect(falseToggle).toHaveAttribute('aria-pressed', 'false');

      // Test disabled state without description
      const disabledToggle = page.getByTestId(
        'toggle-true-state-width-disabled'
      );
      await expect(disabledToggle).toBeVisible();
      await expect(disabledToggle).toBeDisabled();

      // Test invalid state without description
      const invalidToggle = page.getByTestId('toggle-true-state-width-invalid');
      await expect(invalidToggle).toBeVisible();
      await expect(invalidToggle).toHaveAttribute('aria-pressed', 'true');
      await invalidToggle.click();
      await expect(invalidToggle).toHaveAttribute('aria-pressed', 'false');
    });
  });
});
