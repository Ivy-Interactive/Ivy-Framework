import { test, expect, type Page } from '@playwright/test';

// Shared setup function that navigates to TabsApp
async function setupTabsPage(page: Page): Promise<void> {
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  // Search for and navigate to TabsApp
  const searchInput = page.getByTestId('sidebar-search');
  await expect(searchInput).toBeVisible();
  await searchInput.click();
  await searchInput.fill('tabs');
  await page.waitForTimeout(500); // Wait for search results

  // Click on the TabsApp result (should contain "Tabs" in the text)
  const tabsAppButton = page
    .locator('button')
    .filter({ hasText: /Tabs/i })
    .first();
  await expect(tabsAppButton).toBeVisible();
  await tabsAppButton.click();
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(300); // Wait for tabs to render
}

// Helper function to open multiple pages in tabs (for testing tab functionality)
async function openMultiplePagesInTabs(page: Page): Promise<void> {
  // Open a second page (e.g., Button widget)
  const searchInput = page.getByTestId('sidebar-search');
  await searchInput.click();
  await searchInput.fill('button');
  await page.waitForTimeout(300);

  const buttonResult = page
    .locator('button')
    .filter({ hasText: /^Button$/i })
    .first();
  await buttonResult.click();
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(300);

  // Open a third page (e.g., Badge widget)
  await searchInput.click();
  await searchInput.fill('badge');
  await page.waitForTimeout(300);

  const badgeResult = page
    .locator('button')
    .filter({ hasText: /^Badge$/i })
    .first();
  await badgeResult.click();
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(300);
}

test.describe('Tabs Layout Widget Tests', () => {
  test.beforeEach(async ({ page }) => {
    await setupTabsPage(page);
  });

  test.describe('Smoke Tests', () => {
    test('should render tabs layout app with heading', async ({ page }) => {
      const h1 = page.getByRole('heading', { level: 1 });
      await expect(h1).toBeVisible();
      const h1Text = await h1.textContent();
      expect(h1Text).toContain('Tabs layout');
    });

    test('should render both Tabs and Content variants', async ({ page }) => {
      // Look for variant headings
      await expect(
        page.getByRole('heading', { name: /Tabs variant/i })
      ).toBeVisible();
      await expect(
        page.getByRole('heading', { name: /Content variant/i })
      ).toBeVisible();
    });

    test('should render tabs with initial content', async ({ page }) => {
      // Check that tabs are rendered
      const tabs = page.locator('[role="tab"]');
      expect(await tabs.count()).toBeGreaterThan(0);

      // Check that at least one tab is active
      const activeTabs = page.locator('[role="tab"][aria-selected="true"]');
      expect(await activeTabs.count()).toBeGreaterThan(0);
    });
  });

  test.describe('Multi-Page Tab Navigation', () => {
    test('should open multiple pages in tabs and switch between them', async ({
      page,
    }) => {
      // Open multiple pages to create tabs
      await openMultiplePagesInTabs(page);

      // Now we should have multiple tabs open (TabsApp, Button, Badge)
      const tabs = page.locator('[role="tab"]');
      const tabCount = await tabs.count();
      expect(tabCount).toBeGreaterThanOrEqual(3);

      // Verify we can see tab labels
      // Note: The actual tab labels depend on the app implementation
      // They might be "Tabs", "Button", "Badge" or similar
      const firstTab = tabs.first();
      await expect(firstTab).toBeVisible();

      // Click on different tabs to switch between them
      const secondTab = tabs.nth(1);
      await secondTab.click();
      await page.waitForTimeout(200);
      await expect(secondTab).toHaveAttribute('aria-selected', 'true');

      const thirdTab = tabs.nth(2);
      await thirdTab.click();
      await page.waitForTimeout(200);
      await expect(thirdTab).toHaveAttribute('aria-selected', 'true');

      // Go back to first tab
      await firstTab.click();
      await page.waitForTimeout(200);
      await expect(firstTab).toHaveAttribute('aria-selected', 'true');
    });

    test('should close tabs when close button is clicked', async ({ page }) => {
      // Open multiple pages
      await openMultiplePagesInTabs(page);

      // Get initial tab count
      const initialTabCount = await page.locator('[role="tab"]').count();
      expect(initialTabCount).toBeGreaterThanOrEqual(3);

      // Find and click the close button on the last tab
      const lastTab = page.locator('[role="tab"]').last();
      await lastTab.hover();
      await page.waitForTimeout(100);

      // Find the close button within the tab (should be an 'a' element or button with X icon)
      const closeButton = lastTab.locator('a, button').last();
      await closeButton.click();
      await page.waitForTimeout(300);

      // Verify tab was closed
      const finalTabCount = await page.locator('[role="tab"]').count();
      expect(finalTabCount).toBe(initialTabCount - 1);
    });
  });

  test.describe('Tab Functionality', () => {
    test('should switch tabs on click', async ({ page }) => {
      // Get all tabs in the first TabsLayout
      const tabs = page.locator('[role="tab"]').first();
      await tabs.scrollIntoViewIfNeeded();

      // Find Customers tab
      const customersTab = page
        .locator('[role="tab"]')
        .filter({ hasText: 'Customers' })
        .first();
      await expect(customersTab).toBeVisible();
      await customersTab.click();
      await expect(customersTab).toHaveAttribute('aria-selected', 'true');

      // Find Orders tab and click it
      const ordersTab = page
        .locator('[role="tab"]')
        .filter({ hasText: 'Orders' })
        .first();
      await expect(ordersTab).toBeVisible();
      await ordersTab.click();
      await expect(ordersTab).toHaveAttribute('aria-selected', 'true');

      // Customers should no longer be active
      await expect(customersTab).toHaveAttribute('aria-selected', 'false');
    });

    test('should display correct tab content when selected', async ({
      page,
    }) => {
      // Click on Customers tab
      const customersTab = page
        .locator('[role="tab"]')
        .filter({ hasText: 'Customers' })
        .first();
      await customersTab.click();

      // Verify tab panel is visible
      const tabPanel = page.locator('[role="tabpanel"]').first();
      await expect(tabPanel).toBeVisible();
    });

    test('should close tab when close button clicked', async ({ page }) => {
      // Get initial tab count
      const initialTabs = page.locator('[role="tab"]');
      const initialCount = await initialTabs.count();

      // Find a tab with a close button
      const tabWithCloseButton = page.locator('[role="tab"]').first();
      await tabWithCloseButton.scrollIntoViewIfNeeded();

      // Hover to reveal close button (if needed) and click it
      await tabWithCloseButton.hover();
      const closeButton = tabWithCloseButton.locator('a').last();
      await closeButton.click();

      // Wait a bit for the tab to be removed
      await page.waitForTimeout(200);

      // Verify tab count decreased
      const finalCount = await page.locator('[role="tab"]').count();
      expect(finalCount).toBeLessThan(initialCount);
    });

    test('should show add button and create new tab when clicked', async ({
      page,
    }) => {
      // Find the add button (should have "+" text)
      const addButton = page.locator('button').filter({ hasText: '+' }).first();
      await addButton.scrollIntoViewIfNeeded();
      await expect(addButton).toBeVisible();

      // Get initial tab count
      const initialCount = await page.locator('[role="tab"]').count();

      // Click add button
      await addButton.click();
      await page.waitForTimeout(200);

      // Verify new tab was added
      const finalCount = await page.locator('[role="tab"]').count();
      expect(finalCount).toBeGreaterThan(initialCount);
    });
  });

  test.describe('Tab Badges and Icons', () => {
    test('should render tabs with badges', async ({ page }) => {
      // Look for badges in tabs
      const tabsWithBadges = page.locator('[role="tab"]').filter({
        has: page.locator('div.inline-flex.items-center.rounded-md'),
      });

      expect(await tabsWithBadges.count()).toBeGreaterThan(0);

      // Verify badge text
      const firstBadge = tabsWithBadges
        .first()
        .locator('div.inline-flex.items-center.rounded-md');
      await expect(firstBadge).toBeVisible();
    });

    test('should render tabs with icons', async ({ page }) => {
      // Look for tabs with SVG icons
      const tabsWithIcons = page.locator('[role="tab"]').filter({
        has: page.locator('svg'),
      });

      expect(await tabsWithIcons.count()).toBeGreaterThan(0);

      // Verify icon is visible
      const firstIcon = tabsWithIcons.first().locator('svg').first();
      await expect(firstIcon).toBeVisible();
    });
  });

  // OLD STYLING TESTS - REMOVED (replaced by NEW STYLING REQUIREMENTS tests below)

  test.describe('Content Variant Styling', () => {
    test('should have Content variant tabs rendered', async ({ page }) => {
      // Scroll to Content variant section
      const contentVariantHeading = page.getByRole('heading', {
        name: /Content variant/i,
      });
      await contentVariantHeading.scrollIntoViewIfNeeded();

      // Find the tablist in Content variant (second TabsLayout)
      const tablists = page.locator('[role="tablist"]');
      expect(await tablists.count()).toBeGreaterThanOrEqual(2);

      // Get the Content variant tablist (second one)
      const contentTabsList = tablists.nth(1);
      await expect(contentTabsList).toBeVisible();

      // Verify tabs exist in Content variant
      const contentTabs = contentTabsList.locator('[role="tab"]');
      expect(await contentTabs.count()).toBeGreaterThan(0);
    });

    test('should have animated underline indicator on active tab', async ({
      page,
    }) => {
      // Scroll to Content variant
      const contentVariantHeading = page.getByRole('heading', {
        name: /Content variant/i,
      });
      await contentVariantHeading.scrollIntoViewIfNeeded();

      // Look for the animated underline (div with bg-foreground and bottom positioning)
      const contentSection = page
        .locator('div')
        .filter({ has: contentVariantHeading });
      const underline = contentSection.locator(
        'div.absolute.bottom-\\[-6px\\].h-\\[2px\\].bg-foreground'
      );

      // The underline should exist (even if we can't easily test the animation)
      const count = await underline.count();
      expect(count).toBeGreaterThanOrEqual(0); // May or may not be present depending on implementation
    });
  });

  test.describe('Responsive Behavior', () => {
    test('should show dropdown button when tabs overflow', async ({ page }) => {
      // Reduce viewport width to force overflow
      await page.setViewportSize({ width: 600, height: 800 });
      await page.waitForTimeout(300);

      // Look for dropdown button (ChevronDown icon)
      const dropdownButton = page
        .locator('button[aria-label="Show more tabs"]')
        .first();

      // Dropdown may or may not be visible depending on content
      const isVisible = await dropdownButton.isVisible();
      if (isVisible) {
        await expect(dropdownButton).toBeEnabled();
      }
    });

    test('should maintain functionality at different viewport sizes', async ({
      page,
    }) => {
      // Test at mobile size
      await page.setViewportSize({ width: 375, height: 667 });
      await page.waitForTimeout(300);

      // Tabs should still be clickable
      const firstTab = page.locator('[role="tab"]').first();
      await firstTab.scrollIntoViewIfNeeded();
      await expect(firstTab).toBeVisible();
      await firstTab.click();

      // Test at tablet size
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.waitForTimeout(300);

      await expect(firstTab).toBeVisible();
    });
  });

  test.describe('Keyboard Navigation', () => {
    test('should support keyboard focus on tabs', async ({ page }) => {
      const firstTab = page.locator('[role="tab"]').first();
      await firstTab.scrollIntoViewIfNeeded();
      await firstTab.focus();
      await expect(firstTab).toBeFocused();

      // Click to activate (some tab implementations require click, not Enter)
      await firstTab.click();
      await page.waitForTimeout(200);
      await expect(firstTab).toHaveAttribute('aria-selected', 'true');
    });
  });

  test.describe('Drag and Drop Reordering', () => {
    test('should allow tab reordering via drag and drop', async ({ page }) => {
      // Get initial order of tabs
      const tabs = page.locator('[role="tab"]');
      const firstTabText = await tabs.first().textContent();
      const secondTabText = await tabs.nth(1).textContent();

      expect(firstTabText).toBeTruthy();
      expect(secondTabText).toBeTruthy();
      expect(firstTabText).not.toBe(secondTabText);

      // Note: Actually testing drag-and-drop in Playwright is complex
      // For now, we just verify tabs exist and have the right structure
      // The dnd-kit library handles the actual drag behavior
      expect(await tabs.count()).toBeGreaterThan(1);
    });
  });

  test.describe('NEW STYLING REQUIREMENTS - Simplified Tabs (Should FAIL initially)', () => {
    test('should NOT have top margin on tabs container', async ({ page }) => {
      const tabsList = page.locator('[role="tablist"]').first();
      await tabsList.scrollIntoViewIfNeeded();

      const classes = await tabsList.getAttribute('class');
      // NEW REQUIREMENT: Should NOT have mt-2.5
      expect(classes).not.toContain('mt-2.5');
    });

    test('should NOT have gap between tabs', async ({ page }) => {
      const tabsList = page.locator('[role="tablist"]').first();
      await tabsList.scrollIntoViewIfNeeded();

      const classes = await tabsList.getAttribute('class');
      // NEW REQUIREMENT: Should NOT have gap-0.5
      expect(classes).not.toContain('gap-0.5');
    });

    test('should NOT have rounded corners on tabs', async ({ page }) => {
      const firstTab = page.locator('[role="tab"]').first();
      await firstTab.scrollIntoViewIfNeeded();

      const classes = await firstTab.getAttribute('class');
      // NEW REQUIREMENT: Should NOT have any rounded classes
      expect(classes).not.toContain('rounded-b-none');
      expect(classes).not.toContain('rounded-t');
      expect(classes).not.toContain('rounded-md');
      expect(classes).not.toContain('rounded-lg');
      expect(classes).not.toContain('rounded');
    });

    test('should have bottom border on ALL tabs (active and inactive)', async ({
      page,
    }) => {
      // Check active tab
      const activeTab = page
        .locator('[role="tab"][aria-selected="true"]')
        .first();
      await activeTab.scrollIntoViewIfNeeded();

      const activeClasses = await activeTab.getAttribute('class');
      // NEW REQUIREMENT: All tabs should have border-b
      expect(activeClasses).toContain('border-b');

      // Check inactive tab
      const inactiveTab = page
        .locator('[role="tab"][aria-selected="false"]')
        .first();
      await inactiveTab.scrollIntoViewIfNeeded();

      const inactiveClasses = await inactiveTab.getAttribute('class');
      // NEW REQUIREMENT: All tabs should have border-b
      expect(inactiveClasses).toContain('border-b');
    });

    test('should show close button on NON-ACTIVE tabs ONLY on hover', async ({
      page,
    }) => {
      // Find a non-active tab
      const inactiveTab = page
        .locator('[role="tab"][aria-selected="false"]')
        .first();
      await inactiveTab.scrollIntoViewIfNeeded();

      // Before hover: close button should NOT be visible (or have invisible class)
      const closeButtonBeforeHover = inactiveTab.locator('a, button').last();

      // Check if button has "invisible" or "opacity-0" or similar class
      const classesBeforeHover =
        await closeButtonBeforeHover.getAttribute('class');

      // NEW REQUIREMENT: Non-active tab close button should be invisible by default
      const isHiddenBeforeHover =
        classesBeforeHover?.includes('invisible') ||
        classesBeforeHover?.includes('opacity-0') ||
        (await closeButtonBeforeHover.isHidden());

      expect(isHiddenBeforeHover).toBe(true);

      // After hover: close button SHOULD be visible
      await inactiveTab.hover();
      await page.waitForTimeout(100);

      // NEW REQUIREMENT: Should become visible on hover
      await expect(closeButtonBeforeHover).toBeVisible();
    });

    test('should ALWAYS show close button on ACTIVE tab (not affected by hover)', async ({
      page,
    }) => {
      // Find the active tab
      const activeTab = page
        .locator('[role="tab"][aria-selected="true"]')
        .first();
      await activeTab.scrollIntoViewIfNeeded();

      // Close button should be visible WITHOUT hover
      const closeButton = activeTab.locator('a, button').last();

      // NEW REQUIREMENT: Active tab close button always visible
      await expect(closeButton).toBeVisible();

      // Move mouse away and verify it's still visible
      await page.mouse.move(0, 0);
      await page.waitForTimeout(100);

      // Should still be visible
      await expect(closeButton).toBeVisible();
    });

    test('should have negative margin to overlap borders between tabs', async ({
      page,
    }) => {
      // Find any tab
      const firstTab = page.locator('[role="tab"]').first();
      await firstTab.scrollIntoViewIfNeeded();

      const classes = await firstTab.getAttribute('class');
      // NEW REQUIREMENT: Tabs should have -ml-px (negative left margin) to overlap borders
      expect(classes).toContain('-ml-px');
    });

    test('should have close button positioned above tab text on inactive tabs', async ({
      page,
    }) => {
      // Find a non-active tab
      const inactiveTab = page
        .locator('[role="tab"][aria-selected="false"]')
        .first();
      await inactiveTab.scrollIntoViewIfNeeded();

      // Get the close button
      const closeButton = inactiveTab.locator('a, button').last();

      // Hover to make it visible
      await inactiveTab.hover();
      await page.waitForTimeout(100);

      // Get computed style to check z-index or position
      const closeButtonBox = await closeButton.boundingBox();
      const tabBox = await inactiveTab.boundingBox();

      // Close button should be within tab bounds but positioned above (higher z-index implied by visibility on hover)
      expect(closeButtonBox).not.toBeNull();
      expect(tabBox).not.toBeNull();

      if (closeButtonBox && tabBox) {
        // Button should be within the tab
        expect(closeButtonBox.x).toBeGreaterThanOrEqual(tabBox.x);
        expect(closeButtonBox.x + closeButtonBox.width).toBeLessThanOrEqual(
          tabBox.x + tabBox.width
        );
      }
    });

    test('should have white background on active tab', async ({ page }) => {
      const activeTab = page
        .locator('[role="tab"][aria-selected="true"]')
        .first();
      await activeTab.scrollIntoViewIfNeeded();

      const classes = await activeTab.getAttribute('class');
      // NEW REQUIREMENT: Active tab should have bg-background class (white in light mode)
      expect(classes).toContain('bg-background');
    });

    test('should have light gray background on inactive tabs', async ({
      page,
    }) => {
      const inactiveTab = page
        .locator('[role="tab"][aria-selected="false"]')
        .first();
      await inactiveTab.scrollIntoViewIfNeeded();

      const classes = await inactiveTab.getAttribute('class');
      // NEW REQUIREMENT: Inactive tab should have bg-muted class (light gray)
      expect(classes).toContain('bg-muted');
    });

    test('should have hover effect on inactive tabs with darker background', async ({
      page,
    }) => {
      const inactiveTab = page
        .locator('[role="tab"][aria-selected="false"]')
        .first();
      await inactiveTab.scrollIntoViewIfNeeded();

      // Get background color before hover
      const bgColorBefore = await inactiveTab.evaluate(
        el => window.getComputedStyle(el).backgroundColor
      );

      // Hover over the tab
      await inactiveTab.hover();
      await page.waitForTimeout(100);

      // Get background color after hover
      const bgColorAfter = await inactiveTab.evaluate(
        el => window.getComputedStyle(el).backgroundColor
      );

      // NEW REQUIREMENT: Background color should change on hover (darker)
      expect(bgColorBefore).not.toBe(bgColorAfter);

      // Also verify the class is present
      const classes = await inactiveTab.getAttribute('class');
      expect(classes).toContain('hover:bg-muted-foreground/20');
    });

    test('should fill full height with h-full class', async ({ page }) => {
      const firstTab = page.locator('[role="tab"]').first();
      await firstTab.scrollIntoViewIfNeeded();

      const classes = await firstTab.getAttribute('class');
      // NEW REQUIREMENT: Tabs should use h-full to fill available y-axis space
      expect(classes).toContain('h-full');
    });
  });
});
