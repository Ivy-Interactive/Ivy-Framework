import { describe, it, expect, beforeEach, vi } from "vitest";
import {
  registerShortcut,
  unregisterShortcut,
  getRegisteredShortcuts,
  serializeShortcut,
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
    displayKey: "K",
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

describe("serializeShortcut", () => {
  it("serializes a simple key", () => {
    expect(serializeShortcut(makeShortcut("k"))).toBe("k");
  });

  it("serializes ctrl+key", () => {
    expect(serializeShortcut(makeShortcut("k", { ctrl: true }))).toBe("ctrl+k");
  });

  it("serializes multiple modifiers in canonical order", () => {
    expect(
      serializeShortcut(makeShortcut("s", { ctrl: true, meta: true, alt: true, shift: true })),
    ).toBe("ctrl+meta+alt+shift+s");
  });

  it("normalizes key to lowercase", () => {
    expect(serializeShortcut(makeShortcut("K", { ctrl: true }))).toBe("ctrl+k");
  });

  it("omits false modifiers", () => {
    expect(serializeShortcut(makeShortcut("x", { alt: true }))).toBe("alt+x");
  });
});

describe("getRegisteredShortcuts", () => {
  beforeEach(() => {
    _resetForTesting();
  });

  it("returns empty array when no shortcuts registered", () => {
    expect(getRegisteredShortcuts()).toEqual([]);
  });

  it("returns correct entries after registration", () => {
    registerShortcut(
      makeRegistration({
        id: "btn-1",
        shortcut: makeShortcut("k", { ctrl: true }),
        displayKey: "Ctrl+K",
      }),
    );
    registerShortcut(
      makeRegistration({
        id: "btn-2",
        shortcut: makeShortcut("s", { alt: true }),
        displayKey: "Alt+S",
      }),
    );

    const shortcuts = getRegisteredShortcuts();
    expect(shortcuts).toHaveLength(2);
    expect(shortcuts).toEqual(
      expect.arrayContaining([
        expect.objectContaining({
          id: "btn-1",
          shortcutKey: "ctrl+k",
          displayKey: "Ctrl+K",
          isActive: true,
        }),
        expect.objectContaining({
          id: "btn-2",
          shortcutKey: "alt+s",
          displayKey: "Alt+S",
          isActive: true,
        }),
      ]),
    );
  });

  it("reflects unregistration", () => {
    registerShortcut(
      makeRegistration({
        id: "btn-1",
        shortcut: makeShortcut("k", { ctrl: true }),
        displayKey: "Ctrl+K",
      }),
    );
    registerShortcut(
      makeRegistration({
        id: "btn-2",
        shortcut: makeShortcut("s", { alt: true }),
        displayKey: "Alt+S",
      }),
    );

    unregisterShortcut("btn-1");

    const shortcuts = getRegisteredShortcuts();
    expect(shortcuts).toHaveLength(1);
    expect(shortcuts[0].id).toBe("btn-2");
  });

  it("reflects isActive state", () => {
    registerShortcut(
      makeRegistration({
        id: "btn-active",
        shortcut: makeShortcut("a", { ctrl: true }),
        displayKey: "Ctrl+A",
        isActive: () => true,
      }),
    );
    registerShortcut(
      makeRegistration({
        id: "btn-inactive",
        shortcut: makeShortcut("b", { ctrl: true }),
        displayKey: "Ctrl+B",
        isActive: () => false,
      }),
    );

    const shortcuts = getRegisteredShortcuts();
    const active = shortcuts.find((s) => s.id === "btn-active");
    const inactive = shortcuts.find((s) => s.id === "btn-inactive");
    expect(active?.isActive).toBe(true);
    expect(inactive?.isActive).toBe(false);
  });
});

describe("conflict detection", () => {
  beforeEach(() => {
    _resetForTesting();
    vi.stubEnv("NODE_ENV", "development");
  });

  it("warns when two shortcuts share the same key combination", () => {
    const warnSpy = vi.spyOn(console, "warn").mockImplementation(() => {});

    const shortcut = makeShortcut("k", { ctrl: true });
    registerShortcut(makeRegistration({ id: "btn-1", shortcut, displayKey: "Ctrl+K" }));
    registerShortcut(makeRegistration({ id: "btn-2", shortcut, displayKey: "Ctrl+K" }));

    expect(warnSpy).toHaveBeenCalledOnce();
    expect(warnSpy).toHaveBeenCalledWith(
      expect.stringContaining('[Ivy] Shortcut conflict: "ctrl+k"'),
      expect.arrayContaining(["btn-2", "btn-1"]),
    );

    warnSpy.mockRestore();
    vi.unstubAllEnvs();
  });

  it("does not warn for different key combinations", () => {
    const warnSpy = vi.spyOn(console, "warn").mockImplementation(() => {});

    registerShortcut(
      makeRegistration({
        id: "btn-1",
        shortcut: makeShortcut("k", { ctrl: true }),
        displayKey: "Ctrl+K",
      }),
    );
    registerShortcut(
      makeRegistration({
        id: "btn-2",
        shortcut: makeShortcut("s", { ctrl: true }),
        displayKey: "Ctrl+S",
      }),
    );

    expect(warnSpy).not.toHaveBeenCalled();

    warnSpy.mockRestore();
    vi.unstubAllEnvs();
  });

  it("does not warn in production mode", () => {
    vi.stubEnv("NODE_ENV", "production");
    const warnSpy = vi.spyOn(console, "warn").mockImplementation(() => {});

    const shortcut = makeShortcut("k", { ctrl: true });
    registerShortcut(makeRegistration({ id: "btn-1", shortcut, displayKey: "Ctrl+K" }));
    registerShortcut(makeRegistration({ id: "btn-2", shortcut, displayKey: "Ctrl+K" }));

    expect(warnSpy).not.toHaveBeenCalled();

    warnSpy.mockRestore();
    vi.unstubAllEnvs();
  });
});
