import { describe, it, expect, beforeEach, vi, afterEach } from "vitest";
import React from "react";
import { createRoot, type Root } from "react-dom/client";
import { act } from "react";
import { _resetForTesting, _getRegistrySize, _isListenerInstalled } from "../shortcutRegistry";

// Must import useShortcut after registry so the mock is set up
import { useShortcut } from "../useShortcut";

function TestComponent({
  id,
  shortcutKey,
  handler,
  disabled,
  elementRef,
}: {
  id: string;
  shortcutKey: string | undefined;
  handler: () => void;
  disabled?: boolean;
  elementRef?: React.RefObject<HTMLElement | null>;
}) {
  useShortcut(id, shortcutKey, handler, { disabled, elementRef });
  return React.createElement("div", { "data-testid": "test-component" });
}

describe("useShortcut", () => {
  let container: HTMLDivElement;
  let root: Root;

  beforeEach(() => {
    _resetForTesting();
    container = document.createElement("div");
    document.body.appendChild(container);
    root = createRoot(container);
  });

  afterEach(() => {
    act(() => {
      root.unmount();
    });
    document.body.removeChild(container);
    _resetForTesting();
  });

  it("should register on mount and unregister on unmount", () => {
    const handler = vi.fn();

    act(() => {
      root.render(
        React.createElement(TestComponent, {
          id: "hook-1",
          shortcutKey: "k",
          handler,
        }),
      );
    });

    expect(_getRegistrySize()).toBe(1);
    expect(_isListenerInstalled()).toBe(true);

    act(() => {
      root.unmount();
    });

    // Re-create root since we unmounted
    root = createRoot(container);

    expect(_getRegistrySize()).toBe(0);
    expect(_isListenerInstalled()).toBe(false);
  });

  it("should not register when disabled", () => {
    const handler = vi.fn();

    act(() => {
      root.render(
        React.createElement(TestComponent, {
          id: "hook-disabled",
          shortcutKey: "k",
          handler,
          disabled: true,
        }),
      );
    });

    expect(_getRegistrySize()).toBe(0);
  });

  it("should not register when shortcutKey is undefined", () => {
    const handler = vi.fn();

    act(() => {
      root.render(
        React.createElement(TestComponent, {
          id: "hook-no-key",
          shortcutKey: undefined,
          handler,
        }),
      );
    });

    expect(_getRegistrySize()).toBe(0);
  });

  it("should make isActive return false when element is in aria-hidden container", () => {
    const handler = vi.fn();
    const ref = { current: null as HTMLElement | null };

    // Create element inside aria-hidden container
    const hiddenContainer = document.createElement("div");
    hiddenContainer.setAttribute("aria-hidden", "true");
    const element = document.createElement("button");
    hiddenContainer.appendChild(element);
    document.body.appendChild(hiddenContainer);
    ref.current = element;

    act(() => {
      root.render(
        React.createElement(TestComponent, {
          id: "hook-hidden",
          shortcutKey: "k",
          handler,
          elementRef: ref as React.RefObject<HTMLElement>,
        }),
      );
    });

    // Fire keydown - handler should NOT be called because element is in aria-hidden container
    const event = new KeyboardEvent("keydown", {
      code: "KeyK",
      bubbles: true,
      cancelable: true,
    });
    window.dispatchEvent(event);

    expect(handler).not.toHaveBeenCalled();

    document.body.removeChild(hiddenContainer);
  });

  it("should fire handler when element is not in aria-hidden container", () => {
    const handler = vi.fn();
    const ref = { current: null as HTMLElement | null };

    const element = document.createElement("button");
    document.body.appendChild(element);
    ref.current = element;

    act(() => {
      root.render(
        React.createElement(TestComponent, {
          id: "hook-visible",
          shortcutKey: "k",
          handler,
          elementRef: ref as React.RefObject<HTMLElement>,
        }),
      );
    });

    const event = new KeyboardEvent("keydown", {
      code: "KeyK",
      bubbles: true,
      cancelable: true,
    });
    window.dispatchEvent(event);

    expect(handler).toHaveBeenCalledTimes(1);

    document.body.removeChild(element);
  });
});
