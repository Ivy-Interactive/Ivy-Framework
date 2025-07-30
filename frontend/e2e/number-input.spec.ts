import { test, expect, type Page } from '@playwright/test';

// Shared setup function for number input tests
async function setupNumberInputPage(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  // Navigate to Number Inputs app
  const searchInput = page.getByTestId('sidebar-search');
  await expect(searchInput).toBeVisible();
  await searchInput.click();
  await searchInput.fill('number input');
  await searchInput.press('Enter');

  const firstResult = page
    .locator('button')
    .filter({ hasText: /Number Input/i })
    .first();

  await expect(firstResult).toBeVisible();
  await firstResult.click();
  await page.waitForLoadState('networkidle');
}

test.describe('Number Input Tests', () => {
  test.beforeEach(async ({ page }) => {
    await setupNumberInputPage(page);
  });

  test.describe('Basic Number Input Variants', () => {
    test('should test nullable number input interactions', async ({ page }) => {
      const nullableInput = page.getByTestId('number-input-nullable-main');
      await expect(nullableInput).toBeVisible();

      // Test typing a value
      await nullableInput.clear();
      await nullableInput.fill('123');
      await expect(nullableInput).toHaveValue('123');

      // Test clearing the value
      await nullableInput.clear();
      await expect(nullableInput).toHaveValue('');

      // Test typing a decimal value
      await nullableInput.fill('3.14');
      await expect(nullableInput).toHaveValue('3.14');
    });

    test('should test integer number input interactions', async ({ page }) => {
      const intInput = page.getByTestId('number-input-int-main');
      await expect(intInput).toBeVisible();

      // Test typing a value
      await intInput.clear();
      await intInput.fill('42');
      await expect(intInput).toHaveValue('42');

      // Test typing a decimal value (should still accept it)
      await intInput.clear();
      await intInput.fill('3.14');
      await expect(intInput).toHaveValue('3.14');

      // Test typing an invalid value (should still accept it as text)
      await intInput.clear();
      await intInput.fill('abc');
      await expect(intInput).toHaveValue('abc');
    });

    test('should test disabled number input', async ({ page }) => {
      const disabledInput = page.getByTestId('number-input-int-disabled-main');
      await expect(disabledInput).toBeVisible();

      // Test that the input is disabled
      await expect(disabledInput).toBeDisabled();

      // Test that we can't modify the value
      await expect(disabledInput).not.toBeEditable();
    });

    test('should test invalid number input', async ({ page }) => {
      const invalidInput = page.getByTestId('number-input-int-invalid-main');
      await expect(invalidInput).toBeVisible();

      // Test that the input is interactive
      await invalidInput.clear();
      await invalidInput.fill('999');
      await expect(invalidInput).toHaveValue('999');
    });

    test('should test arrows number input interactions', async ({ page }) => {
      const arrowsInput = page.getByTestId('number-input-int-arrows-main');
      await expect(arrowsInput).toBeVisible();

      // Test typing a value
      await arrowsInput.clear();
      await arrowsInput.fill('10');
      await expect(arrowsInput).toHaveValue('10');

      // Test that arrows are visible (the input should have the pr-14 class for right padding)
      await expect(arrowsInput).toHaveClass(/pr-14/);

      // Test increment/decrement buttons exist (they are inside the input container)
      const incrementButton = arrowsInput
        .locator('..')
        .getByRole('button')
        .first();
      const decrementButton = arrowsInput
        .locator('..')
        .getByRole('button')
        .last();

      await expect(incrementButton).toBeVisible();
      await expect(decrementButton).toBeVisible();
    });
  });

  test.describe('Slider Number Input Variants', () => {
    test('should test nullable slider interactions', async ({ page }) => {
      const nullableSlider = page.getByTestId(
        'number-input-nullable-slider-main'
      );
      await expect(nullableSlider).toBeVisible();

      // For slider inputs, we should test that the element exists and is interactive
      await expect(nullableSlider).not.toHaveAttribute('aria-disabled', 'true');

      // Test that it has the correct data attributes for a slider
      await expect(nullableSlider).toHaveAttribute(
        'data-orientation',
        'horizontal'
      );
    });

    test('should test integer slider interactions', async ({ page }) => {
      const intSlider = page.getByTestId('number-input-int-slider-main');
      await expect(intSlider).toBeVisible();

      // For slider inputs, we should test that the element exists and is interactive
      await expect(intSlider).not.toHaveAttribute('aria-disabled', 'true');

      // Test that it has the correct data attributes for a slider
      await expect(intSlider).toHaveAttribute('data-orientation', 'horizontal');
    });

    test('should test disabled slider', async ({ page }) => {
      const disabledSlider = page.getByTestId(
        'number-input-int-disabled-slider-main'
      );
      await expect(disabledSlider).toBeVisible();

      // Test that the slider is disabled
      await expect(disabledSlider).toHaveAttribute('aria-disabled', 'true');
    });

    test('should test invalid slider interactions', async ({ page }) => {
      const invalidSlider = page.getByTestId(
        'number-input-int-invalid-slider-main'
      );
      await expect(invalidSlider).toBeVisible();

      // For invalid slider inputs, we should test that the element exists and is interactive
      await expect(invalidSlider).not.toHaveAttribute('aria-disabled', 'true');

      // Test that it has the correct data attributes for a slider
      await expect(invalidSlider).toHaveAttribute(
        'data-orientation',
        'horizontal'
      );
    });
  });
});
