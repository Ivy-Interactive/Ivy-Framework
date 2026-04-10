import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";

/**
 * Tests that keyboard shortcuts on buttons do NOT fire when the button
 * is inside an `aria-hidden="true"` container (e.g., an inactive tab).
 *
 * This test exercises the DOM-level behavior without React rendering,
 * simulating the same querySelector + closest check used in ButtonWidget.
 */
describe("Button shortcut aria-hidden check", () => {
  let container: HTMLDivElement;

  beforeEach(() => {
    container = document.createElement("div");
    document.body.appendChild(container);
  });

  afterEach(() => {
    container.remove();
  });

  function createButtonInContainer(ariaHidden: boolean, id: string): HTMLButtonElement {
    const tabPanel = document.createElement("div");
    if (ariaHidden) {
      tabPanel.setAttribute("aria-hidden", "true");
    }
    const button = document.createElement("button");
    button.setAttribute("data-shortcut-id", id);
    tabPanel.appendChild(button);
    container.appendChild(tabPanel);
    return button;
  }

  /**
   * Simulates the visibility check from ButtonWidget's handleKeyDown:
   *   const buttonEl = document.querySelector(`[data-shortcut-id="${id}"]`);
   *   if (buttonEl?.closest('[aria-hidden="true"]')) return;
   */
  function isShortcutBlocked(id: string): boolean {
    const buttonEl = document.querySelector(`[data-shortcut-id="${id}"]`);
    return !!buttonEl?.closest('[aria-hidden="true"]');
  }

  it("should block shortcut when button is inside aria-hidden container", () => {
    createButtonInContainer(true, "btn-1");
    expect(isShortcutBlocked("btn-1")).toBe(true);
  });

  it("should allow shortcut when button is in a visible container", () => {
    createButtonInContainer(false, "btn-2");
    expect(isShortcutBlocked("btn-2")).toBe(false);
  });

  it("should block shortcut when any ancestor has aria-hidden", () => {
    // Nested: outer aria-hidden > inner div > button
    const outer = document.createElement("div");
    outer.setAttribute("aria-hidden", "true");
    const inner = document.createElement("div");
    const button = document.createElement("button");
    button.setAttribute("data-shortcut-id", "btn-nested");
    inner.appendChild(button);
    outer.appendChild(inner);
    container.appendChild(outer);

    expect(isShortcutBlocked("btn-nested")).toBe(true);
  });

  it("should only block the button in the hidden tab, not the active one", () => {
    // Simulates two tab panels: one active, one hidden
    createButtonInContainer(false, "active-btn");
    createButtonInContainer(true, "hidden-btn");

    expect(isShortcutBlocked("active-btn")).toBe(false);
    expect(isShortcutBlocked("hidden-btn")).toBe(true);
  });

  it("should handle keydown event correctly with aria-hidden check", () => {
    createButtonInContainer(true, "hidden-shortcut-btn");
    createButtonInContainer(false, "visible-shortcut-btn");

    const hiddenHandler = vi.fn();
    const visibleHandler = vi.fn();

    // Simulate the shortcut handler logic from ButtonWidget
    function simulateShortcutFire(id: string, handler: () => void) {
      const buttonEl = document.querySelector(`[data-shortcut-id="${id}"]`);
      if (buttonEl?.closest('[aria-hidden="true"]')) {
        return; // Skip — button is in a hidden/inactive tab
      }
      handler();
    }

    simulateShortcutFire("hidden-shortcut-btn", hiddenHandler);
    simulateShortcutFire("visible-shortcut-btn", visibleHandler);

    expect(hiddenHandler).not.toHaveBeenCalled();
    expect(visibleHandler).toHaveBeenCalledOnce();
  });
});
