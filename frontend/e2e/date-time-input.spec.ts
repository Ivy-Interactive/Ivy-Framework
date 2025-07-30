import { test, expect, type Page } from '@playwright/test';

// Shared setup function for date-time input tests
async function setupDateTimeInputPage(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  // Navigate to DateTimeInput app
  const searchInput = page.getByTestId('sidebar-search');
  await expect(searchInput).toBeVisible();
  await searchInput.click();
  await searchInput.fill('date time input');
  await searchInput.press('Enter');

  const firstResult = page
    .locator('button')
    .filter({ hasText: /date.*time.*input/i })
    .first();

  await expect(firstResult).toBeVisible();
  await firstResult.click();
  await page.waitForLoadState('networkidle');
}

test.describe('DateTime Input Tests', () => {
  test.beforeEach(async ({ page }) => {
    await setupDateTimeInputPage(page);
  });

  test.describe('Date Input Variants', () => {
    test('should test date input interactions', async ({ page }) => {
      const dateInput = page.getByTestId('datetime-input-date-main');
      await expect(dateInput).toBeVisible();

      // Test that the date input is a button (date picker)
      await expect(dateInput).toHaveAttribute('type', 'button');
      await expect(dateInput).toHaveAttribute('aria-haspopup', 'dialog');
    });

    test('should test disabled date input', async ({ page }) => {
      const disabledInput = page.getByTestId(
        'datetime-input-date-disabled-main'
      );
      await expect(disabledInput).toBeVisible();

      // Test that the input is disabled
      await expect(disabledInput).toBeDisabled();
    });

    test('should test invalid date input', async ({ page }) => {
      const invalidInput = page.getByTestId('datetime-input-date-invalid-main');
      await expect(invalidInput).toBeVisible();

      // Test that the date input is a button (date picker)
      await expect(invalidInput).toHaveAttribute('type', 'button');
      await expect(invalidInput).toHaveAttribute('aria-haspopup', 'dialog');
    });

    test('should test nullable date input', async ({ page }) => {
      const nullableInput = page.getByTestId(
        'datetime-input-date-nullable-main'
      );
      await expect(nullableInput).toBeVisible();

      // Test that the date input is a button (date picker)
      await expect(nullableInput).toHaveAttribute('type', 'button');
      await expect(nullableInput).toHaveAttribute('aria-haspopup', 'dialog');
    });

    test('should test nullable invalid date input', async ({ page }) => {
      const nullableInvalidInput = page.getByTestId(
        'datetime-input-date-nullable-invalid-main'
      );
      await expect(nullableInvalidInput).toBeVisible();

      // Test that the date input is a button (date picker)
      await expect(nullableInvalidInput).toHaveAttribute('type', 'button');
      await expect(nullableInvalidInput).toHaveAttribute(
        'aria-haspopup',
        'dialog'
      );
    });
  });

  test.describe('DateTime Input Variants', () => {
    test('should test datetime input interactions', async ({ page }) => {
      const dtInput = page.getByTestId('datetime-input-datetime-main');
      await expect(dtInput).toBeVisible();

      // Test that the datetime input is a button (date picker)
      await expect(dtInput).toHaveAttribute('type', 'button');
      await expect(dtInput).toHaveAttribute('aria-haspopup', 'dialog');
    });

    test('should test disabled datetime input', async ({ page }) => {
      const disabledInput = page.getByTestId(
        'datetime-input-datetime-disabled-main'
      );
      await expect(disabledInput).toBeVisible();

      // Test that the input is disabled
      await expect(disabledInput).toBeDisabled();
    });

    test('should test invalid datetime input', async ({ page }) => {
      const invalidInput = page.getByTestId(
        'datetime-input-datetime-invalid-main'
      );
      await expect(invalidInput).toBeVisible();

      // Test that the datetime input is a button (date picker)
      await expect(invalidInput).toHaveAttribute('type', 'button');
      await expect(invalidInput).toHaveAttribute('aria-haspopup', 'dialog');
    });

    test('should test nullable datetime input', async ({ page }) => {
      const nullableInput = page.getByTestId(
        'datetime-input-datetime-nullable-main'
      );
      await expect(nullableInput).toBeVisible();

      // Test that the datetime input is a button (date picker)
      await expect(nullableInput).toHaveAttribute('type', 'button');
      await expect(nullableInput).toHaveAttribute('aria-haspopup', 'dialog');
    });

    test('should test nullable invalid datetime input', async ({ page }) => {
      const nullableInvalidInput = page.getByTestId(
        'datetime-input-datetime-nullable-invalid-main'
      );
      await expect(nullableInvalidInput).toBeVisible();

      // Test that the datetime input is a button (date picker)
      await expect(nullableInvalidInput).toHaveAttribute('type', 'button');
      await expect(nullableInvalidInput).toHaveAttribute(
        'aria-haspopup',
        'dialog'
      );
    });
  });

  test.describe('Time Input Variants', () => {
    test('should test time input interactions', async ({ page }) => {
      const timeInput = page.getByTestId('datetime-input-time-main');
      await expect(timeInput).toBeVisible();

      // Test that the time input is a div container
      await expect(timeInput).toHaveAttribute(
        'class',
        /relative flex items-center gap-2/
      );
    });

    test('should test disabled time input', async ({ page }) => {
      const disabledInput = page.getByTestId(
        'datetime-input-time-disabled-main'
      );
      await expect(disabledInput).toBeVisible();

      // Test that the time input is a div container
      await expect(disabledInput).toHaveAttribute(
        'class',
        /relative flex items-center gap-2/
      );
    });

    test('should test invalid time input', async ({ page }) => {
      const invalidInput = page.getByTestId('datetime-input-time-invalid-main');
      await expect(invalidInput).toBeVisible();

      // Test that the time input is a div container
      await expect(invalidInput).toHaveAttribute(
        'class',
        /relative flex items-center gap-2/
      );
    });

    test('should test nullable time input', async ({ page }) => {
      const nullableInput = page.getByTestId(
        'datetime-input-time-nullable-main'
      );
      await expect(nullableInput).toBeVisible();

      // Test that the time input is a div container
      await expect(nullableInput).toHaveAttribute(
        'class',
        /relative flex items-center gap-2/
      );
    });

    test('should test nullable invalid time input', async ({ page }) => {
      const nullableInvalidInput = page.getByTestId(
        'datetime-input-time-nullable-invalid-main'
      );
      await expect(nullableInvalidInput).toBeVisible();

      // Test that the time input is a div container
      await expect(nullableInvalidInput).toHaveAttribute(
        'class',
        /relative flex items-center gap-2/
      );
    });
  });

  test.describe('Data Binding Inputs', () => {
    test('should test data binding inputs', async ({ page }) => {
      // Test that data binding inputs are visible
      const bindingInputs = [
        'datetime-input-datetime-binding',
        'datetime-input-dateonly-binding',
        'datetime-input-timeonly-binding',
        'datetime-input-string-binding',
        'datetime-input-datetime-nullable-binding',
        'datetime-input-dateonly-nullable-binding',
        'datetime-input-timeonly-nullable-binding',
      ];

      for (const testId of bindingInputs) {
        const input = page.getByTestId(testId);
        await expect(input).toBeVisible();
      }
    });
  });

  test.describe('Placeholder Examples', () => {
    test('should test placeholder examples', async ({ page }) => {
      // Test that placeholder examples show the correct placeholder text
      const birthdayInput = page.getByTestId(
        'datetime-input-placeholder-birthday'
      );
      await expect(birthdayInput).toContainText('Birthday');

      const meetingInput = page.getByTestId(
        'datetime-input-placeholder-meeting'
      );
      await expect(meetingInput).toContainText('Meeting time');

      const startDateInput = page.getByTestId(
        'datetime-input-placeholder-start-date'
      );
      await expect(startDateInput).toBeVisible();

      const deadlineInput = page.getByTestId(
        'datetime-input-placeholder-deadline'
      );
      await expect(deadlineInput).toBeVisible();

      const startTimeInput = page.getByTestId(
        'datetime-input-placeholder-start-time'
      );
      await expect(startTimeInput).toBeVisible();

      const lunchTimeInput = page.getByTestId(
        'datetime-input-placeholder-lunch-time'
      );
      await expect(lunchTimeInput).toBeVisible();
    });
  });
});
