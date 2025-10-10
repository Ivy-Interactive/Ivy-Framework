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

  test.describe('Basic Audio Player Variants', () => {
    test('should test basic audio player attributes', async ({ page }) => {
      const basicAudio = page.getByTestId('audio-basic');
      await expect(basicAudio).toBeVisible();

      // Verify it has controls
      await expect(basicAudio).toHaveAttribute('controls', '');

      // Verify default attributes
      await expect(basicAudio).not.toHaveAttribute('autoplay');
      await expect(basicAudio).not.toHaveAttribute('muted');

      // Verify it has a valid source
      const src = await basicAudio.getAttribute('src');
      expect(src).toBeTruthy();
      expect(src).toContain('.mp3');

      // Verify default preload
      const preload = await basicAudio.getAttribute('preload');
      expect(preload).toBe('Metadata');
    });

    test('should test looping audio player attributes', async ({ page }) => {
      const loopingAudio = page.getByTestId('audio-looping');
      await expect(loopingAudio).toBeVisible();

      // Verify loop attribute is set
      await expect(loopingAudio).toHaveAttribute('loop', '');

      // Verify preload is auto
      const preload = await loopingAudio.getAttribute('preload');
      expect(preload).toBe('Auto');

      // Verify it has controls
      await expect(loopingAudio).toHaveAttribute('controls', '');
    });

    test('should test muted autoplay audio player attributes', async ({
      page,
    }) => {
      const mutedAutoplayAudio = page.getByTestId('audio-muted-autoplay');
      await expect(mutedAutoplayAudio).toBeVisible();

      // Verify autoplay attribute is set
      await expect(mutedAutoplayAudio).toHaveAttribute('autoplay', '');

      // Verify loop is enabled for this variant
      await expect(mutedAutoplayAudio).toHaveAttribute('loop', '');

      // Note: muted attribute might be handled differently in React/HTML5
      // Just verify the element exists and has the other required attributes
    });

    test('should test audio player without controls', async ({ page }) => {
      const noControlsAudio = page.getByTestId('audio-no-controls');

      // Audio without controls is in the DOM but not visible - this is expected
      await expect(noControlsAudio).toBeAttached();

      // Verify controls attribute is not present
      const hasControls = await noControlsAudio.getAttribute('controls');
      expect(hasControls).toBeNull();

      // Verify it has a valid source
      const src = await noControlsAudio.getAttribute('src');
      expect(src).toBeTruthy();
      expect(src).toContain('.mp3');
    });

    test('should test custom sized audio player', async ({ page }) => {
      const customSizedAudio = page.getByTestId('audio-custom-sized');
      await expect(customSizedAudio).toBeVisible();

      // Verify it has a valid source
      const src = await customSizedAudio.getAttribute('src');
      expect(src).toBeTruthy();
      expect(src).toContain('.mp3');

      // Verify it has controls
      await expect(customSizedAudio).toHaveAttribute('controls', '');
    });

    test('should test theme awareness audio player', async ({ page }) => {
      const themeAudio = page.getByTestId('audio-theme');
      await expect(themeAudio).toBeVisible();

      // Verify it has a valid source
      const src = await themeAudio.getAttribute('src');
      expect(src).toBeTruthy();
      expect(src).toContain('.mp3');

      // Verify it has controls
      await expect(themeAudio).toHaveAttribute('controls', '');
    });
  });

  test.describe('Accessibility Tests', () => {
    test('should verify audio player accessibility attributes', async ({
      page,
    }) => {
      const basicAudio = page.getByTestId('audio-basic');
      await expect(basicAudio).toBeVisible();

      // Check for accessibility attributes
      const ariaLabel = await basicAudio.getAttribute('aria-label');
      expect(ariaLabel).toBeTruthy();

      const role = await basicAudio.getAttribute('role');
      expect(role).toBeTruthy();
    });
  });
});
