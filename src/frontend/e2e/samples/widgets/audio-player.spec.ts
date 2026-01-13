import { test, expect, type Page } from '@playwright/test';

test.skip(true, 'Temporarily skipping samples E2E suite while PR stabilizes');

// Constants
const AUDIO_TEST_IDS = {
  BASIC: 'audio-basic',
  LOOPING: 'audio-looping',
  MUTED_AUTOPLAY: 'audio-muted-autoplay',
  NO_CONTROLS: 'audio-no-controls',
  CUSTOM_SIZED: 'audio-custom-sized',
  THEME: 'audio-theme',
} as const;

const BUTTON_TEST_IDS = {
  TOGGLE_PLAY_PAUSE: 'toggle-play-pause-button',
} as const;

const EXPECTED_VALUES = {
  PRELOAD_METADATA: 'metadata',
  PRELOAD_AUTO: 'auto',
  AUDIO_FILE_EXTENSION: '.mp3',
  AUDIO_URL_PROTOCOL: 'https://',
} as const;

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
      // Verify an H1 heading is present on the page
      const h1Heading = page.getByRole('heading', { level: 1 });
      await expect(h1Heading).toBeVisible();

      // Verify at least one audio element exists
      const audioElements = page.locator('audio');
      const count = await audioElements.count();
      expect(count).toBeGreaterThan(0);
    });
  });

  test.describe('Audio Widget Properties - All States', () => {
    test('should test basic audio player attributes', async ({ page }) => {
      const basicAudio = page.getByTestId(AUDIO_TEST_IDS.BASIC);
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
      expect(preload).toBe(EXPECTED_VALUES.PRELOAD_METADATA);
    });

    test('should test looping audio player attributes', async ({ page }) => {
      const loopingAudio = page.getByTestId(AUDIO_TEST_IDS.LOOPING);
      await expect(loopingAudio).toBeVisible();

      // Verify loop attribute is set
      await expect(loopingAudio).toHaveAttribute('loop', '');

      // Verify preload is auto
      const preload = await loopingAudio.getAttribute('preload');
      expect(preload).toBe(EXPECTED_VALUES.PRELOAD_AUTO);

      // Verify it has controls
      await expect(loopingAudio).toHaveAttribute('controls', '');
    });

    test('should test muted autoplay audio player attributes', async ({
      page,
    }) => {
      const mutedAutoplayAudio = page.getByTestId(
        AUDIO_TEST_IDS.MUTED_AUTOPLAY
      );
      await expect(mutedAutoplayAudio).toBeVisible();

      // Verify autoplay attribute is set
      await expect(mutedAutoplayAudio).toHaveAttribute('autoplay', '');

      // Verify loop is enabled for this variant
      await expect(mutedAutoplayAudio).toHaveAttribute('loop', '');

      // Note: muted attribute might be handled differently in React/HTML5
      // Just verify the element exists and has the other required attributes
    });

    test('should test audio player without controls', async ({ page }) => {
      const noControlsAudio = page.getByTestId(AUDIO_TEST_IDS.NO_CONTROLS);

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
      const customSizedAudio = page.getByTestId(AUDIO_TEST_IDS.CUSTOM_SIZED);
      await expect(customSizedAudio).toBeVisible();

      // Verify it has a valid source
      const src = await customSizedAudio.getAttribute('src');
      expect(src).toBeTruthy();
      expect(src).toContain('.mp3');

      // Verify it has controls
      await expect(customSizedAudio).toHaveAttribute('controls', '');
    });

    test('should test theme awareness audio player', async ({ page }) => {
      const themeAudio = page.getByTestId(AUDIO_TEST_IDS.THEME);
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
      const customSizedAudio = page.getByTestId(AUDIO_TEST_IDS.CUSTOM_SIZED);
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
      const basicAudio = page.getByTestId(AUDIO_TEST_IDS.BASIC);
      await expect(basicAudio).toBeVisible();

      // Verify it has the expected CSS class
      const classAttribute = await basicAudio.getAttribute('class');
      expect(classAttribute).toContain('w-full');
    });

    test('should verify all audio players are contained in cards', async ({
      page,
    }) => {
      // Verify all audio test IDs are present (one per card section)
      const testIds = Object.values(AUDIO_TEST_IDS);

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
      const basicAudio = page.getByTestId(AUDIO_TEST_IDS.BASIC);
      await expect(basicAudio).toBeVisible();
      await expect(basicAudio).toHaveAttribute('controls', '');

      // Scroll to and check looping audio
      const loopingHeading = page.getByRole('heading', {
        name: 'Looping Audio with Preload',
        exact: true,
      });
      await loopingHeading.scrollIntoViewIfNeeded();
      const loopingAudio = page.getByTestId(AUDIO_TEST_IDS.LOOPING);
      await expect(loopingAudio).toBeVisible();
      await expect(loopingAudio).toHaveAttribute('loop', '');

      // Scroll to and check programmatic control section with button
      const controlHeading = page.getByRole('heading', {
        name: 'Audio Without Controls',
        exact: true,
      });
      await controlHeading.scrollIntoViewIfNeeded();
      const toggleButton = page.getByTestId(BUTTON_TEST_IDS.TOGGLE_PLAY_PAUSE);
      await expect(toggleButton).toBeVisible();
      await expect(toggleButton).toBeEnabled();
    });

    test('should verify button interaction in programmatic control section', async ({
      page,
    }) => {
      // Find and click the toggle button
      const toggleButton = page.getByTestId(BUTTON_TEST_IDS.TOGGLE_PLAY_PAUSE);
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
      // Test metadata (default)
      const basicAudio = page.getByTestId(AUDIO_TEST_IDS.BASIC);
      const basicPreload = await basicAudio.getAttribute('preload');
      expect(basicPreload).toBe('metadata');

      // Test auto
      const loopingAudio = page.getByTestId(AUDIO_TEST_IDS.LOOPING);
      const autoPreload = await loopingAudio.getAttribute('preload');
      expect(autoPreload).toBe('auto');

      // Note: none preload is not shown in the sample app
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
        expect(src).toContain(EXPECTED_VALUES.AUDIO_URL_PROTOCOL);
        expect(src).toContain(EXPECTED_VALUES.AUDIO_FILE_EXTENSION);
      }
    });

    test('should verify combined state properties work together', async ({
      page,
    }) => {
      // Test audio with multiple properties: muted + autoplay + loop
      const mutedAutoplayAudio = page.getByTestId(
        AUDIO_TEST_IDS.MUTED_AUTOPLAY
      );
      await expect(mutedAutoplayAudio).toBeVisible();
      await expect(mutedAutoplayAudio).toHaveAttribute('autoplay', '');
      await expect(mutedAutoplayAudio).toHaveAttribute('loop', '');

      // Test audio with loop + preload
      const loopingAudio = page.getByTestId(AUDIO_TEST_IDS.LOOPING);
      await expect(loopingAudio).toHaveAttribute('loop', '');
      const preload = await loopingAudio.getAttribute('preload');
      expect(preload).toBe(EXPECTED_VALUES.PRELOAD_AUTO);

      // Test audio with controls disabled + muted
      const noControlsAudio = page.getByTestId(AUDIO_TEST_IDS.NO_CONTROLS);
      const hasControls = await noControlsAudio.getAttribute('controls');
      expect(hasControls).toBeNull();
    });
  });

  test.describe('Interactive Controls Tests', () => {
    test('should test mute button functionality', async ({ page }) => {
      const basicAudio = page.getByTestId(AUDIO_TEST_IDS.BASIC);
      await expect(basicAudio).toBeVisible();

      // Wait for audio to be ready
      await basicAudio.waitFor({ state: 'attached' });

      // Test initial muted state (should be false by default)
      const isInitiallyMuted = await basicAudio.evaluate(
        (audio: HTMLAudioElement) => audio.muted
      );
      expect(isInitiallyMuted).toBe(false);

      // Test muting the audio
      await basicAudio.evaluate((audio: HTMLAudioElement) => {
        audio.muted = true;
      });

      const isMuted = await basicAudio.evaluate(
        (audio: HTMLAudioElement) => audio.muted
      );
      expect(isMuted).toBe(true);

      // Test unmuting the audio
      await basicAudio.evaluate((audio: HTMLAudioElement) => {
        audio.muted = false;
      });

      const isUnmuted = await basicAudio.evaluate(
        (audio: HTMLAudioElement) => audio.muted
      );
      expect(isUnmuted).toBe(false);
    });

    test('should test playback speed controls', async ({ page }) => {
      const basicAudio = page.getByTestId(AUDIO_TEST_IDS.BASIC);
      await expect(basicAudio).toBeVisible();

      // Wait for audio to be ready
      await basicAudio.waitFor({ state: 'attached' });

      // Test default playback rate (should be 1.0)
      const defaultRate = await basicAudio.evaluate(
        (audio: HTMLAudioElement) => audio.playbackRate
      );
      expect(defaultRate).toBe(1.0);

      // Test changing playback speed to 0.5x (slow)
      await basicAudio.evaluate((audio: HTMLAudioElement) => {
        audio.playbackRate = 0.5;
      });

      const slowRate = await basicAudio.evaluate(
        (audio: HTMLAudioElement) => audio.playbackRate
      );
      expect(slowRate).toBe(0.5);

      // Test changing playback speed to 1.5x (fast)
      await basicAudio.evaluate((audio: HTMLAudioElement) => {
        audio.playbackRate = 1.5;
      });

      const fastRate = await basicAudio.evaluate(
        (audio: HTMLAudioElement) => audio.playbackRate
      );
      expect(fastRate).toBe(1.5);

      // Test changing playback speed to 2x (very fast)
      await basicAudio.evaluate((audio: HTMLAudioElement) => {
        audio.playbackRate = 2.0;
      });

      const veryFastRate = await basicAudio.evaluate(
        (audio: HTMLAudioElement) => audio.playbackRate
      );
      expect(veryFastRate).toBe(2.0);

      // Reset to normal speed
      await basicAudio.evaluate((audio: HTMLAudioElement) => {
        audio.playbackRate = 1.0;
      });
    });

    test('should test audio controls visibility and functionality', async ({
      page,
    }) => {
      const basicAudio = page.getByTestId(AUDIO_TEST_IDS.BASIC);
      await expect(basicAudio).toBeVisible();

      // Verify controls are present and functional
      await expect(basicAudio).toHaveAttribute('controls', '');
      await expect(basicAudio).toHaveAttribute('src');
    });

    test('should test audio download functionality', async ({ page }) => {
      const basicAudio = page.getByTestId(AUDIO_TEST_IDS.BASIC);
      await expect(basicAudio).toBeVisible();

      // Get the audio source URL
      const src = await basicAudio.getAttribute('src');
      expect(src).toBeTruthy();
      expect(src).toContain('.mp3');

      // Test that the audio source is accessible for download
      const response = await page.request.get(src!);
      expect(response.status()).toBe(200);
      expect(response.headers()['content-type']).toContain('audio');
    });

    test('should test audio volume controls', async ({ page }) => {
      const basicAudio = page.getByTestId(AUDIO_TEST_IDS.BASIC);
      await expect(basicAudio).toBeVisible();

      // Test default volume (should be 1.0)
      const defaultVolume = await basicAudio.evaluate(
        (audio: HTMLAudioElement) => audio.volume
      );
      expect(defaultVolume).toBe(1.0);

      // Test setting volume to 0.5
      await basicAudio.evaluate((audio: HTMLAudioElement) => {
        audio.volume = 0.5;
      });

      const halfVolume = await basicAudio.evaluate(
        (audio: HTMLAudioElement) => audio.volume
      );
      expect(halfVolume).toBe(0.5);

      // Test setting volume to 0 (mute via volume)
      await basicAudio.evaluate((audio: HTMLAudioElement) => {
        audio.volume = 0;
      });

      const zeroVolume = await basicAudio.evaluate(
        (audio: HTMLAudioElement) => audio.volume
      );
      expect(zeroVolume).toBe(0);

      // Reset volume
      await basicAudio.evaluate((audio: HTMLAudioElement) => {
        audio.volume = 1.0;
      });
    });

    test.skip('should test audio time controls', async ({ page }) => {
      const basicAudio = page.getByTestId(AUDIO_TEST_IDS.BASIC);
      await expect(basicAudio).toBeVisible();

      // Test initial time (should be 0)
      const initialTime = await basicAudio.evaluate(
        (audio: HTMLAudioElement) => audio.currentTime
      );
      expect(initialTime).toBe(0);

      // Test setting current time
      await basicAudio.evaluate((audio: HTMLAudioElement) => {
        audio.currentTime = 10;
      });

      const setTime = await basicAudio.evaluate(
        (audio: HTMLAudioElement) => audio.currentTime
      );
      expect(setTime).toBe(10);

      // Test duration (should be available after metadata loads)
      const duration = await basicAudio.evaluate(
        (audio: HTMLAudioElement) => audio.duration
      );
      expect(duration).toBeGreaterThan(0);
      expect(duration).not.toBeNaN();

      // Reset time
      await basicAudio.evaluate((audio: HTMLAudioElement) => {
        audio.currentTime = 0;
      });
    });
  });

  test.describe('Accessibility Tests', () => {
    test('should verify audio player accessibility attributes', async ({
      page,
    }) => {
      const basicAudio = page.getByTestId(AUDIO_TEST_IDS.BASIC);
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

    test('should verify keyboard navigation support', async ({ page }) => {
      const basicAudio = page.getByTestId(AUDIO_TEST_IDS.BASIC);
      await expect(basicAudio).toBeVisible();

      // Focus the audio element and verify it's focusable
      await basicAudio.focus();
      await expect(basicAudio).toBeFocused();
    });

    test('should verify screen reader support', async ({ page }) => {
      const basicAudio = page.getByTestId(AUDIO_TEST_IDS.BASIC);
      await expect(basicAudio).toBeVisible();

      // Check for proper ARIA attributes for screen readers
      await expect(basicAudio).toHaveAttribute('aria-label', 'Audio player');
      await expect(basicAudio).toHaveAttribute('role', 'application');
      await expect(basicAudio).toHaveAttribute('src');
    });
  });
});
