import { describe, it, expect, beforeEach, vi } from "vitest";
import {
  registerShortcut,
  unregisterShortcut,
  _resetForTesting,
  _getRegistrySize,
  _isListenerInstalled,
  type ShortcutRegistration,
} from "../shortcutRegistry";
import type { ParsedShortcut } from "../shortcut";

function makeShortcut(key: string, modifiers?: Partial<ParsedShortcut>): ParsedShortcut {
  return {
    ctrl: false,
    shift: false,
    alt: false,
    meta: false,
    key,
    ...modifiers,
  };
}

function makeRegistration(
  overrides: Partial<ShortcutRegistration> & { id: string },
): ShortcutRegistration {
  return {
    shortcut: makeShortcut("k"),
    handler: vi.fn(),
    isActive: () => true,
    skipInInputs: true,
    ...overrides,
  };
}

function fireKeydown(code: string, options?: Partial<KeyboardEvent>) {
  const event = new KeyboardEvent("keydown", {
    code,
    bubbles: true,
    cancelable: true,
    ...options,
  });
  window.dispatchEvent(event);
  return event;
}

describe("shortcutRegistry", () => {
  beforeEach(() => {
    _resetForTesting();
  });

  it("should call handler when matching keydown fires", () => {
    const handler = vi.fn();
    registerShortcut(
      makeRegistration({
        id: "btn-1",
        shortcut: makeShortcut("k"),
        handler,
      }),
    );

    fireKeydown("KeyK");
    expect(handler).toHaveBeenCalledTimes(1);
  });

  it("should not call handler after unregister", () => {
    const handler = vi.fn();
    registerShortcut(
      makeRegistration({
        id: "btn-2",
        shortcut: makeShortcut("k"),
        handler,
      }),
    );

    unregisterShortcut("btn-2");
    fireKeydown("KeyK");
    expect(handler).not.toHaveBeenCalled();
  });

  it("should only fire active registration when two share the same key", () => {
    const activeHandler = vi.fn();
    const inactiveHandler = vi.fn();

    registerShortcut(
      makeRegistration({
        id: "btn-active",
        shortcut: makeShortcut("m"),
        handler: activeHandler,
        isActive: () => true,
      }),
    );

    registerShortcut(
      makeRegistration({
        id: "btn-inactive",
        shortcut: makeShortcut("m"),
        handler: inactiveHandler,
        isActive: () => false,
      }),
    );

    fireKeydown("KeyM");
    expect(activeHandler).toHaveBeenCalledTimes(1);
    expect(inactiveHandler).not.toHaveBeenCalled();
  });

  it("should debounce rapid keydowns within 300ms", () => {
    const handler = vi.fn();
    registerShortcut(
      makeRegistration({
        id: "btn-debounce",
        shortcut: makeShortcut("k"),
        handler,
      }),
    );

    fireKeydown("KeyK");
    fireKeydown("KeyK");
    fireKeydown("KeyK");
    expect(handler).toHaveBeenCalledTimes(1);
  });

  it("should fire again after debounce period elapses", () => {
    vi.useFakeTimers();
    const handler = vi.fn();
    registerShortcut(
      makeRegistration({
        id: "btn-debounce-2",
        shortcut: makeShortcut("k"),
        handler,
      }),
    );

    fireKeydown("KeyK");
    expect(handler).toHaveBeenCalledTimes(1);

    vi.advanceTimersByTime(350);
    fireKeydown("KeyK");
    expect(handler).toHaveBeenCalledTimes(2);

    vi.useRealTimers();
  });

  it("should skip non-modifier shortcut when target is INPUT element", () => {
    const handler = vi.fn();
    registerShortcut(
      makeRegistration({
        id: "btn-input",
        shortcut: makeShortcut("k"),
        handler,
        skipInInputs: true,
      }),
    );

    const input = document.createElement("input");
    document.body.appendChild(input);
    input.focus();

    const event = new KeyboardEvent("keydown", {
      code: "KeyK",
      bubbles: true,
      cancelable: true,
    });
    input.dispatchEvent(event);

    expect(handler).not.toHaveBeenCalled();
    document.body.removeChild(input);
  });

  it("should fire modifier shortcut even when target is INPUT element", () => {
    const handler = vi.fn();
    registerShortcut(
      makeRegistration({
        id: "btn-mod-input",
        shortcut: makeShortcut("k", { ctrl: true }),
        handler,
        skipInInputs: false,
      }),
    );

    const input = document.createElement("input");
    document.body.appendChild(input);
    input.focus();

    const event = new KeyboardEvent("keydown", {
      code: "KeyK",
      ctrlKey: true,
      bubbles: true,
      cancelable: true,
    });
    input.dispatchEvent(event);

    expect(handler).toHaveBeenCalledTimes(1);
    document.body.removeChild(input);
  });

  it("should not install global listener when registry is empty", () => {
    expect(_isListenerInstalled()).toBe(false);
    expect(_getRegistrySize()).toBe(0);
  });

  it("should install listener on first registration and remove on last unregister", () => {
    registerShortcut(makeRegistration({ id: "btn-lazy" }));
    expect(_isListenerInstalled()).toBe(true);

    unregisterShortcut("btn-lazy");
    expect(_isListenerInstalled()).toBe(false);
  });

  it("should match shift modifier correctly", () => {
    const handler = vi.fn();
    registerShortcut(
      makeRegistration({
        id: "btn-shift",
        shortcut: makeShortcut("k", { shift: true }),
        handler,
      }),
    );

    // Without shift - should not fire
    fireKeydown("KeyK");
    expect(handler).not.toHaveBeenCalled();

    // With shift - should fire
    fireKeydown("KeyK", { shiftKey: true });
    expect(handler).toHaveBeenCalledTimes(1);
  });

  it("should not fire for non-matching key code", () => {
    const handler = vi.fn();
    registerShortcut(
      makeRegistration({
        id: "btn-wrong-key",
        shortcut: makeShortcut("k"),
        handler,
      }),
    );

    fireKeydown("KeyJ");
    expect(handler).not.toHaveBeenCalled();
  });
});
