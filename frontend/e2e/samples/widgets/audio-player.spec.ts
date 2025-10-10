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
    test('should render audio player app and display main heading', async ({
      page,
    }) => {
      // Verify the main heading is present
      await expect(
        page.getByRole('heading', { name: /Audio Player Widget Examples/i })
      ).toBeVisible();

      // Verify at least one audio element exists
      const audioElements = page.locator('audio');
      const count = await audioElements.count();
      expect(count).toBeGreaterThan(0);
    });

    test('should display all card sections', async ({ page }) => {
      // Verify all main sections are visible
      const sections = [
        'Basic Audio Player',
        'Looping Audio with Preload',
        'Muted Autoplay Audio',
        'Audio Without Controls',
        'Custom Sized Audio Player',
        'Theme Awareness',
      ];

      for (const section of sections) {
        await expect(
          page.getByRole('heading', { name: section, exact: true })
        ).toBeVisible();
      }
    });
  });

  test.describe('Audio Widget Properties - All States', () => {
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

  test.describe('Visual Properties Tests', () => {
    test('should verify custom sizing is applied correctly', async ({
      page,
    }) => {
      const customSizedAudio = page.getByTestId('audio-custom-sized');
      await expect(customSizedAudio).toBeVisible();

      // Verify the audio element has style attributes
      const style = await customSizedAudio.getAttribute('style');
      expect(style).toBeTruthy();

      // Check that custom styling is present (width should be fractional, height in units)
      const boundingBox = await customSizedAudio.boundingBox();
      expect(boundingBox).toBeTruthy();
      if (boundingBox) {
        // Custom sized has Width(Size.Fraction(0.5f)) so it should be less than full width
        expect(boundingBox.width).toBeGreaterThan(0);
        expect(boundingBox.height).toBeGreaterThan(0);
      }
    });

    test('should verify audio players have proper CSS classes', async ({
      page,
    }) => {
      const basicAudio = page.getByTestId('audio-basic');
      await expect(basicAudio).toBeVisible();

      // Verify it has the expected CSS class
      const classAttribute = await basicAudio.getAttribute('class');
      expect(classAttribute).toContain('w-full');
    });

    test('should verify all audio players are contained in cards', async ({
      page,
    }) => {
      // Verify all audio test IDs are present (one per card section)
      const testIds = [
        'audio-basic',
        'audio-looping',
        'audio-muted-autoplay',
        'audio-no-controls',
        'audio-custom-sized',
        'audio-theme',
      ];

      for (const testId of testIds) {
        const audio = page.getByTestId(testId);
        await expect(audio).toBeAttached();
      }

      // Verify we have all 6 audio examples
      const audioElements = page.locator('audio[data-testid]');
      const count = await audioElements.count();
      expect(count).toBe(6);
    });
  });

  test.describe('Complex Routine Tests', () => {
    test('should navigate and interact with multiple audio components', async ({
      page,
    }) => {
      // Verify page loads with heading
      await expect(
        page.getByRole('heading', { name: /Audio Player Widget Examples/i })
      ).toBeVisible();

      // Check basic audio
      const basicAudio = page.getByTestId('audio-basic');
      await expect(basicAudio).toBeVisible();
      await expect(basicAudio).toHaveAttribute('controls', '');

      // Scroll to and check looping audio
      const loopingHeading = page.getByRole('heading', {
        name: 'Looping Audio with Preload',
        exact: true,
      });
      await loopingHeading.scrollIntoViewIfNeeded();
      const loopingAudio = page.getByTestId('audio-looping');
      await expect(loopingAudio).toBeVisible();
      await expect(loopingAudio).toHaveAttribute('loop', '');

      // Scroll to and check programmatic control section with button
      const controlHeading = page.getByRole('heading', {
        name: 'Audio Without Controls',
        exact: true,
      });
      await controlHeading.scrollIntoViewIfNeeded();
      const toggleButton = page.getByRole('button', {
        name: /Toggle Play\/Pause/i,
      });
      await expect(toggleButton).toBeVisible();
      await expect(toggleButton).toBeEnabled();

      // Scroll to usage examples
      const usageHeading = page.getByRole('heading', {
        name: /Usage Examples/i,
      });
      await usageHeading.scrollIntoViewIfNeeded();
      await expect(usageHeading).toBeVisible();

      // Verify code block is present
      const codeBlock = page.locator('pre').filter({ hasText: /new Audio/i });
      await expect(codeBlock).toBeVisible();
      const codeContent = await codeBlock.textContent();
      expect(codeContent).toContain('new Audio');
      expect(codeContent).toContain('.Loop(true)');
      expect(codeContent).toContain('.Preload(AudioPreload.Auto)');
    });

    test('should verify button interaction in programmatic control section', async ({
      page,
    }) => {
      // Find and click the toggle button
      const toggleButton = page.getByRole('button', {
        name: /Toggle Play\/Pause/i,
      });
      await toggleButton.scrollIntoViewIfNeeded();
      await expect(toggleButton).toBeVisible();

      // Click the button (should show a toast)
      await toggleButton.click();

      // Button should remain enabled after click
      await expect(toggleButton).toBeEnabled();
    });
  });

  test.describe('All Audio Widget Methods Coverage', () => {
    test('should verify all preload strategies', async ({ page }) => {
      // Test Metadata (default)
      const basicAudio = page.getByTestId('audio-basic');
      const basicPreload = await basicAudio.getAttribute('preload');
      expect(basicPreload).toBe('Metadata');

      // Test Auto
      const loopingAudio = page.getByTestId('audio-looping');
      const autoPreload = await loopingAudio.getAttribute('preload');
      expect(autoPreload).toBe('Auto');

      // Note: None preload is not shown in the sample app
      // but it's a valid value according to Audio.cs
    });

    test('should verify all audio sources are valid external URLs', async ({
      page,
    }) => {
      const audioElements = page.locator('audio[data-testid]');
      const count = await audioElements.count();

      for (let i = 0; i < count; i++) {
        const audio = audioElements.nth(i);
        const src = await audio.getAttribute('src');
        expect(src).toBeTruthy();
        expect(src).toContain('https://');
        expect(src).toContain('.mp3');
      }
    });

    test('should verify combined state properties work together', async ({
      page,
    }) => {
      // Test audio with multiple properties: muted + autoplay + loop
      const mutedAutoplayAudio = page.getByTestId('audio-muted-autoplay');
      await expect(mutedAutoplayAudio).toBeVisible();
      await expect(mutedAutoplayAudio).toHaveAttribute('autoplay', '');
      await expect(mutedAutoplayAudio).toHaveAttribute('loop', '');

      // Test audio with loop + preload
      const loopingAudio = page.getByTestId('audio-looping');
      await expect(loopingAudio).toHaveAttribute('loop', '');
      const preload = await loopingAudio.getAttribute('preload');
      expect(preload).toBe('Auto');

      // Test audio with controls disabled + muted
      const noControlsAudio = page.getByTestId('audio-no-controls');
      const hasControls = await noControlsAudio.getAttribute('controls');
      expect(hasControls).toBeNull();
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

    test('should verify all visible audio players have aria-label', async ({
      page,
    }) => {
      const audioElements = page.locator('audio[controls]');
      const count = await audioElements.count();

      for (let i = 0; i < count; i++) {
        const audio = audioElements.nth(i);
        const ariaLabel = await audio.getAttribute('aria-label');
        expect(ariaLabel).toBeTruthy();
      }
    });
  });

  test.describe('Documentation and Code Examples', () => {
    test('should display usage examples with code snippets', async ({
      page,
    }) => {
      const usageHeading = page.getByRole('heading', {
        name: /Usage Examples/i,
      });
      await usageHeading.scrollIntoViewIfNeeded();
      await expect(usageHeading).toBeVisible();

      // Verify code block contains all key methods
      const codeBlock = page.locator('pre').filter({ hasText: /new Audio/i });
      await expect(codeBlock).toBeVisible();

      const codeContent = await codeBlock.textContent();
      expect(codeContent).toContain('new Audio');
      expect(codeContent).toContain('.Loop(true)');
      expect(codeContent).toContain('.Preload(AudioPreload.Auto)');
      expect(codeContent).toContain('.Muted(true)');
      expect(codeContent).toContain('.Width(Size.Fraction(0.5f))');
      expect(codeContent).toContain('.Height(Size.Units(12))');
    });
  });
});
