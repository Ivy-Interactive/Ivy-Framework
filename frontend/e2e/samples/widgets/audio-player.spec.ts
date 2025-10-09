import { test, expect, type Page } from '@playwright/test';

// Shared setup function for audio player tests
async function setupAudioPlayerPage(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  // Navigate to Audio Player app
  const searchInput = page.getByTestId('sidebar-search');
  await expect(searchInput).toBeVisible();
  await searchInput.click();
  await searchInput.fill('audio player');
  await searchInput.press('Enter');

  const firstResult = page
    .locator('button')
    .filter({ hasText: /Audio Player/i })
    .first();

  await expect(firstResult).toBeVisible();
  await firstResult.click();
  await page.waitForLoadState('networkidle');
}

test.describe('Audio Player Tests', () => {
  test.beforeEach(async ({ page }) => {
    await setupAudioPlayerPage(page);
  });

  test.describe('Smoke Tests', () => {
    test('should render audio player app', async ({ page }) => {
      // Verify the page title/heading is present
      await expect(
        page.getByRole('heading', { name: /Audio Player Widget Examples/i })
      ).toBeVisible();
    });

    test('should display all audio player sections', async ({ page }) => {
      // Verify all main sections are visible
      await expect(
        page.getByRole('heading', { name: /Basic Audio Player/i })
      ).toBeVisible();
      await expect(
        page.getByRole('heading', { name: /Looping Audio with Preload/i })
      ).toBeVisible();
      await expect(
        page.getByRole('heading', { name: /Muted Autoplay Audio/i })
      ).toBeVisible();
      await expect(
        page.getByRole('heading', { name: /Audio Without Controls/i })
      ).toBeVisible();
      await expect(
        page.getByRole('heading', { name: /Custom Sized Audio Player/i })
      ).toBeVisible();
    });

    test('should have at least one audio element visible', async ({ page }) => {
      const audioElements = page.locator('audio');
      await expect(audioElements.first()).toBeVisible();
      const count = await audioElements.count();
      expect(count).toBeGreaterThan(0);
    });
  });

  test.describe('Basic Audio Player State', () => {
    test('should render basic audio player with controls', async ({ page }) => {
      // Get the first audio element (basic audio player)
      const basicAudio = page
        .locator('audio')
        .filter({ has: page.locator('[controls]') })
        .first();
      await expect(basicAudio).toBeVisible();

      // Verify it has controls
      await expect(basicAudio).toHaveAttribute('controls', '');

      // Verify default attributes
      await expect(basicAudio).not.toHaveAttribute('autoplay');
      await expect(basicAudio).not.toHaveAttribute('loop');
      await expect(basicAudio).not.toHaveAttribute('muted');
    });

    test('should have valid audio source', async ({ page }) => {
      const basicAudio = page.locator('audio').first();
      const src = await basicAudio.getAttribute('src');

      expect(src).toBeTruthy();
      expect(src).toContain('.mp3');
    });

    test('should have correct preload attribute', async ({ page }) => {
      const basicAudio = page.locator('audio').first();
      const preload = await basicAudio.getAttribute('preload');

      // Default preload should be 'metadata'
      expect(preload).toBe('metadata');
    });
  });

  test.describe('Audio Player Variants', () => {
    test('should test looping audio player', async ({ page }) => {
      // Find audio with loop attribute
      const loopingAudio = page.locator('audio[loop]').first();
      await expect(loopingAudio).toBeVisible();

      // Verify loop attribute is set
      await expect(loopingAudio).toHaveAttribute('loop', '');

      // Verify preload is auto
      const preload = await loopingAudio.getAttribute('preload');
      expect(preload).toBe('auto');
    });

    test('should test muted autoplay audio player', async ({ page }) => {
      // Find audio with both muted and autoplay attributes
      const mutedAutoplayAudio = page.locator('audio[muted][autoplay]').first();
      await expect(mutedAutoplayAudio).toBeVisible();

      // Verify muted attribute is set
      await expect(mutedAutoplayAudio).toHaveAttribute('muted', '');

      // Verify autoplay attribute is set
      await expect(mutedAutoplayAudio).toHaveAttribute('autoplay', '');

      // Verify loop is enabled for this variant
      await expect(mutedAutoplayAudio).toHaveAttribute('loop', '');
    });

    test('should test audio player without controls', async ({ page }) => {
      // Find audio without controls (muted but not autoplay with loop)
      const noControlsAudio = page.locator('audio[muted]:not([loop])').first();
      await expect(noControlsAudio).toBeVisible();

      // Verify controls attribute is not present
      const hasControls = await noControlsAudio.getAttribute('controls');
      expect(hasControls).toBeNull();
    });

    test('should test custom sized audio player', async ({ page }) => {
      // Get all audio elements and check for custom sizing
      const audioElements = page.locator('audio');
      const count = await audioElements.count();

      // Verify we have multiple audio elements
      expect(count).toBeGreaterThanOrEqual(5);

      // At least one should have custom styling
      const customSizedAudio = audioElements.nth(4); // Custom sized is typically the 5th
      await expect(customSizedAudio).toBeVisible();

      // Verify it has a src attribute
      const src = await customSizedAudio.getAttribute('src');
      expect(src).toBeTruthy();
    });
  });

  test.describe('Audio Player Controls and Interactions', () => {
    test('should display controls on audio elements with controls attribute', async ({
      page,
    }) => {
      // Get all audio elements with controls
      const audioWithControls = page.locator('audio[controls]');
      const count = await audioWithControls.count();

      expect(count).toBeGreaterThan(0);

      // Verify first audio with controls is visible and interactive
      const firstAudio = audioWithControls.first();
      await expect(firstAudio).toBeVisible();
      await expect(firstAudio).toHaveAttribute('controls', '');
    });

    test('should verify audio player accessibility attributes', async ({
      page,
    }) => {
      const audioElement = page.locator('audio').first();
      await expect(audioElement).toBeVisible();

      // Check for accessibility attributes
      const ariaLabel = await audioElement.getAttribute('aria-label');
      expect(ariaLabel).toBeTruthy();

      const role = await audioElement.getAttribute('role');
      expect(role).toBeTruthy();
    });
  });

  test.describe('Audio Player Preload Strategies', () => {
    test('should have different preload strategies', async ({ page }) => {
      const audioElements = page.locator('audio');
      const count = await audioElements.count();

      // Collect all preload values
      const preloadValues: string[] = [];
      for (let i = 0; i < count; i++) {
        const preload = await audioElements.nth(i).getAttribute('preload');
        if (preload) {
          preloadValues.push(preload);
        }
      }

      // Verify we have preload attributes set
      expect(preloadValues.length).toBeGreaterThan(0);

      // Verify we have at least metadata or auto
      const hasMetadata = preloadValues.includes('metadata');
      const hasAuto = preloadValues.includes('auto');

      expect(hasMetadata || hasAuto).toBeTruthy();
    });
  });

  test.describe('Visual Properties', () => {
    test('should verify audio players have appropriate styling', async ({
      page,
    }) => {
      const audioElements = page.locator('audio');
      const firstAudio = audioElements.first();

      // Check that the audio element has width styling
      const classAttribute = await firstAudio.getAttribute('class');
      expect(classAttribute).toContain('w-full');
    });

    test('should verify cards contain audio players properly', async ({
      page,
    }) => {
      // Find all cards that should contain audio players
      const cards = page.locator('[class*="card"]');
      const cardCount = await cards.count();

      // We should have at least 5 cards for the different audio examples
      expect(cardCount).toBeGreaterThanOrEqual(5);
    });
  });

  test.describe('Complex Routine Tests', () => {
    test('should navigate through all audio player examples', async ({
      page,
    }) => {
      // Verify page loads
      await expect(
        page.getByRole('heading', { name: /Audio Player Widget Examples/i })
      ).toBeVisible();

      // Scroll through each section and verify visibility
      const sections = [
        /Basic Audio Player/i,
        /Looping Audio with Preload/i,
        /Muted Autoplay Audio/i,
        /Audio Without Controls/i,
        /Custom Sized Audio Player/i,
        /Theme Awareness/i,
      ];

      for (const section of sections) {
        const heading = page.getByRole('heading', { name: section });
        await heading.scrollIntoViewIfNeeded();
        await expect(heading).toBeVisible();
      }

      // Verify all audio elements are present after scrolling
      const audioElements = page.locator('audio');
      const count = await audioElements.count();
      expect(count).toBeGreaterThanOrEqual(5);
    });

    test('should verify usage examples code block is present', async ({
      page,
    }) => {
      // Scroll to usage examples section
      const usageHeading = page.getByRole('heading', {
        name: /Usage Examples/i,
      });
      await usageHeading.scrollIntoViewIfNeeded();
      await expect(usageHeading).toBeVisible();

      // Verify code block is present
      const codeBlock = page.locator('pre').filter({ hasText: /new Audio/i });
      await expect(codeBlock).toBeVisible();

      // Verify code contains expected content
      const codeContent = await codeBlock.textContent();
      expect(codeContent).toContain('new Audio');
      expect(codeContent).toContain('.Loop(true)');
      expect(codeContent).toContain('.Preload(AudioPreload.Auto)');
    });

    test('should verify button for programmatic control example', async ({
      page,
    }) => {
      // Find the "Toggle Play/Pause" button in the programmatic control section
      const toggleButton = page.getByRole('button', {
        name: /Toggle Play\/Pause/i,
      });
      await toggleButton.scrollIntoViewIfNeeded();
      await expect(toggleButton).toBeVisible();

      // Click the button (should trigger a toast in the sample)
      await toggleButton.click();

      // Note: We can't easily test for the toast without additional setup,
      // but we verify the button is clickable
      await expect(toggleButton).toBeEnabled();
    });
  });

  test.describe('Error States', () => {
    test('should verify all audio elements have valid sources', async ({
      page,
    }) => {
      const audioElements = page.locator('audio');
      const count = await audioElements.count();

      // Check each audio element has a src
      for (let i = 0; i < count; i++) {
        const audio = audioElements.nth(i);
        const src = await audio.getAttribute('src');
        expect(src).toBeTruthy();
        expect(src?.length).toBeGreaterThan(0);
      }
    });

    test('should handle audio loading gracefully', async ({ page }) => {
      // Verify no error alerts are displayed for valid audio sources
      const errorAlerts = page.locator('[role="alert"]').filter({
        hasText: /Failed to load audio file/i,
      });
      const errorCount = await errorAlerts.count();

      // Should be 0 since all sources are valid in the sample
      expect(errorCount).toBe(0);
    });
  });

  test.describe('Theme Integration', () => {
    test('should display theme awareness section', async ({ page }) => {
      const themeSection = page.getByRole('heading', {
        name: /Theme Awareness/i,
      });
      await themeSection.scrollIntoViewIfNeeded();
      await expect(themeSection).toBeVisible();

      // Verify description text is present
      await expect(
        page.getByText(
          /The audio player automatically adapts to your current theme/i
        )
      ).toBeVisible();
    });
  });
});
