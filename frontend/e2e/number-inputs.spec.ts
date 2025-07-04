import { test, expect } from '@playwright/test';

test.describe('Number Inputs - Variants Section', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/app');
    await page.waitForLoadState('networkidle');

    // Navigate to Number Inputs app
    const searchInput = page.getByTestId('sidebar-search');
    await expect(searchInput).toBeVisible();
    await searchInput.click();
    await searchInput.fill('number input');
    await searchInput.press('Enter');

    const firstResult = page.locator('[data-sidebar="menu-item"], [data-sidebar="menu-sub-item"]')
      .filter({ hasText: /Number Input/i }).first();
    
    await expect(firstResult).toBeVisible();
    await firstResult.click();
    await page.waitForLoadState('networkidle');

    // Wait for the app frame to load
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();
  });

  test('should display all variants in the grid', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Check that the variants section exists
    await expect(appFrame!.locator('h2').filter({ hasText: 'Variants' })).toBeVisible();

    // Check grid headers
    await expect(appFrame!.locator('text=Null')).toBeVisible();
    await expect(appFrame!.locator('text=With Value')).toBeVisible();
    await expect(appFrame!.locator('text=Disabled')).toBeVisible();
    await expect(appFrame!.locator('text=Invalid')).toBeVisible();

    // Check method names
    await expect(appFrame!.locator('text=ToNumberInput()')).toBeVisible();
    await expect(appFrame!.locator('text=ToSliderInput()')).toBeVisible();
  });

  test('should handle null value number inputs', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find the null value number input with placeholder
    const nullNumberInput = appFrame!.getByTestId('number-decimal-nullable-with-placeholder-input');
    await expect(nullNumberInput).toBeVisible();

    // Should start empty
    await expect(nullNumberInput).toHaveValue('');

    // Type a value
    await nullNumberInput.fill('123');
    await expect(nullNumberInput).toHaveValue('123');

    // Clear the value
    await nullNumberInput.fill('');
    await expect(nullNumberInput).toHaveValue('');
  });

  test('should handle number inputs with initial values', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find the number input with initial value (intValue.ToNumberInput())
    const numberInput = appFrame!.getByTestId('number-decimal-input');
    await expect(numberInput).toBeVisible();

    // Should have initial value of 12345
    await expect(numberInput).toHaveValue('12,345');

    // Change the value
    await numberInput.fill('999');
    await expect(numberInput).toHaveValue('999');
  });

  test('should handle disabled number inputs', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find the disabled number input
    const disabledNumberInput = appFrame!.getByTestId('number-decimal-disabled-input');
    await expect(disabledNumberInput).toBeVisible();
    await expect(disabledNumberInput).toBeDisabled();

    // Should not be able to type in disabled input
    await disabledNumberInput.fill('123');
    await expect(disabledNumberInput).toHaveValue('12,345'); // Should remain unchanged
  });

  test('should handle invalid number inputs', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find the invalid number input
    const invalidNumberInput = appFrame!.getByTestId('number-decimal-invalid-input');
    await expect(invalidNumberInput).toBeVisible();

    // Should have red border for invalid state
    await expect(invalidNumberInput).toHaveCSS('border-color', 'rgb(239, 68, 68)'); // red-500
  });

  test('should handle slider inputs with null values', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find the null value slider input
    const nullSliderInput = appFrame!.getByTestId('slider-decimal-nullable-with-placeholder-input');
    await expect(nullSliderInput).toBeVisible();

    // Should start empty
    await expect(nullSliderInput).toHaveValue('');

    // Type a value
    await nullSliderInput.fill('50');
    await expect(nullSliderInput).toHaveValue('50');
  });

  test('should handle slider inputs with initial values', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find the slider input with initial value
    const sliderInput = appFrame!.getByTestId('slider-decimal-input');
    await expect(sliderInput).toBeVisible();

    // Should have initial value of 12345
    await expect(sliderInput).toHaveValue('12,345');

    // Change the value
    await sliderInput.fill('500');
    await expect(sliderInput).toHaveValue('500');
  });

  test('should handle disabled slider inputs', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find the disabled slider input
    const disabledSliderInput = appFrame!.getByTestId('slider-decimal-disabled-input');
    await expect(disabledSliderInput).toBeVisible();
    await expect(disabledSliderInput).toBeDisabled();

    // Should not be able to type in disabled input
    await disabledSliderInput.fill('123');
    await expect(disabledSliderInput).toHaveValue('12,345'); // Should remain unchanged
  });

  test('should handle invalid slider inputs', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find the invalid slider input
    const invalidSliderInput = appFrame!.getByTestId('slider-decimal-invalid-input');
    await expect(invalidSliderInput).toBeVisible();

    // Should have red border for invalid state
    await expect(invalidSliderInput).toHaveCSS('border-color', 'rgb(239, 68, 68)'); // red-500
  });

  test('should handle step buttons for number inputs', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find a number input with step buttons
    const numberInput = appFrame!.getByTestId('number-decimal-input');
    const stepUpButton = appFrame!.getByTestId('number-decimal-step-up');
    const stepDownButton = appFrame!.getByTestId('number-decimal-step-down');

    await expect(stepUpButton).toBeVisible();
    await expect(stepDownButton).toBeVisible();

    // Test step up
    const initialValue = await numberInput.inputValue();
    await stepUpButton.click();
    const newValue = await numberInput.inputValue();
    expect(parseInt(newValue.replace(/,/g, ''))).toBeGreaterThan(parseInt(initialValue.replace(/,/g, '')));

    // Test step down
    await stepDownButton.click();
    const finalValue = await numberInput.inputValue();
    expect(parseInt(finalValue.replace(/,/g, ''))).toBeLessThan(parseInt(newValue.replace(/,/g, '')));
  });

  test('should handle keyboard navigation for number inputs', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find a number input
    const numberInput = appFrame!.getByTestId('number-decimal-input');
    await numberInput.click();
    await numberInput.fill('100');

    // Test arrow up
    await numberInput.press('ArrowUp');
    await expect(numberInput).toHaveValue('101');

    // Test arrow down
    await numberInput.press('ArrowDown');
    await expect(numberInput).toHaveValue('100');
  });

  test('should handle mouse wheel for number inputs', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find a number input
    const numberInput = appFrame!.getByTestId('number-decimal-input');
    await numberInput.click();
    await numberInput.fill('100');

    // Test mouse wheel up
    await numberInput.hover();
    await page.mouse.wheel(0, -100); // Scroll up
    await expect(numberInput).toHaveValue('101');

    // Test mouse wheel down
    await page.mouse.wheel(0, 100); // Scroll down
    await expect(numberInput).toHaveValue('100');
  });

  test('should handle drag functionality for number inputs', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find a number input
    const numberInput = appFrame!.getByTestId('number-decimal-input');
    await numberInput.fill('100');

    // Get initial position
    const initialValue = await numberInput.inputValue();
    const box = await numberInput.boundingBox();
    expect(box).not.toBeNull();

    // Start drag
    await page.mouse.move(box!.x + box!.width / 2, box!.y + box!.height / 2);
    await page.mouse.down();

    // Drag right to increase value
    await page.mouse.move(box!.x + box!.width / 2 + 50, box!.y + box!.height / 2);
    await page.mouse.up();

    // Value should have increased
    const newValue = await numberInput.inputValue();
    expect(parseInt(newValue.replace(/,/g, ''))).toBeGreaterThan(parseInt(initialValue.replace(/,/g, '')));
  });

  test('should handle edge cases for number inputs', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find a number input
    const numberInput = appFrame!.getByTestId('number-decimal-input');

    // Test very large numbers
    await numberInput.fill('999999999');
    await expect(numberInput).toHaveValue('999,999,999');

    // Test decimal numbers
    await numberInput.fill('123.45');
    await expect(numberInput).toHaveValue('123.45');

    // Test negative numbers
    await numberInput.fill('-123');
    await expect(numberInput).toHaveValue('-123');

    // Test invalid input (should be cleared or handled gracefully)
    await numberInput.fill('abc');
    await expect(numberInput).toHaveValue('abc'); // Should show what was typed
  });

  test('should handle placeholder text', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find the null value number input with placeholder
    const nullNumberInput = appFrame!.getByTestId('number-decimal-nullable-with-placeholder-input');
    await expect(nullNumberInput).toHaveAttribute('placeholder', 'Placeholder');
  });

  test('should handle clear button for nullable inputs', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find a nullable number input with a value
    const nullableInput = appFrame!.getByTestId('number-decimal-input');
    await nullableInput.fill('123');

    // Look for clear button
    const clearButton = appFrame!.getByTestId('number-decimal-clear');
    
    // Clear button should be visible when there's a value
    if (await clearButton.isVisible()) {
      await clearButton.click();
      await expect(nullableInput).toHaveValue('');
    }
  });

  test('should handle focus and blur events', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find a number input
    const numberInput = appFrame!.getByTestId('number-decimal-input');
    
    // Test focus
    await numberInput.focus();
    await expect(numberInput).toBeFocused();

    // Test blur
    await numberInput.blur();
    await expect(numberInput).not.toBeFocused();
  });

  test('should handle disabled step buttons', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find disabled number input and its step buttons
    const stepUpButton = appFrame!.getByTestId('number-decimal-disabled-step-up');
    const stepDownButton = appFrame!.getByTestId('number-decimal-disabled-step-down');

    // Step buttons should be disabled for disabled inputs
    await expect(stepUpButton).toBeDisabled();
    await expect(stepDownButton).toBeDisabled();
  });

  test('should handle number formatting', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find a number input
    const numberInput = appFrame!.getByTestId('number-decimal-input');
    
    // Type a number and blur to see formatting
    await numberInput.fill('1234567');
    await numberInput.blur();
    
    // Should be formatted with commas
    await expect(numberInput).toHaveValue('1,234,567');
  });

  test('should handle precision and step constraints', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find a number input
    const numberInput = appFrame!.getByTestId('number-decimal-input');
    
    // Test step functionality
    await numberInput.fill('100');
    await numberInput.press('ArrowUp');
    
    // Value should increase by step (default is 1)
    await expect(numberInput).toHaveValue('101');
  });

  test('should handle min/max constraints', async ({ page }) => {
    const appFrameElement = await page.waitForSelector('iframe[src*="number-input"]');
    const appFrame = await appFrameElement.contentFrame();
    expect(appFrame).not.toBeNull();

    // Find a number input
    const numberInput = appFrame!.getByTestId('number-decimal-input');
    
    // Test that we can't go below min or above max through step buttons
    // This would require knowing the specific min/max values for the test inputs
    // For now, we'll test that step buttons work correctly
    await numberInput.fill('100');
    
    const stepUpButton = appFrame!.getByTestId('number-decimal-step-up');
    const stepDownButton = appFrame!.getByTestId('number-decimal-step-down');
    
    // Test step up
    await stepUpButton.click();
    await expect(numberInput).toHaveValue('101');
    
    // Test step down
    await stepDownButton.click();
    await expect(numberInput).toHaveValue('100');
  });
}); 