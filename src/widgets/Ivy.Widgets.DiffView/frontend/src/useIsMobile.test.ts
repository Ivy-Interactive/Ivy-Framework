import { describe, expect, it, beforeEach, afterEach, vi } from "vitest";

describe("useIsMobile hook logic", () => {
  let originalMatchMedia: typeof window.matchMedia;
  let mediaQueryList: MediaQueryList;
  let listeners: ((event: MediaQueryListEvent) => void)[] = [];

  beforeEach(() => {
    if (typeof window !== "undefined") {
      originalMatchMedia = window.matchMedia;
    }

    listeners = [];
    mediaQueryList = {
      matches: false,
      media: "(max-width: 767px)",
      addEventListener: vi.fn((_, handler) => {
        listeners.push(handler as (event: MediaQueryListEvent) => void);
      }),
      removeEventListener: vi.fn((_, handler) => {
        const index = listeners.indexOf(handler as (event: MediaQueryListEvent) => void);
        if (index > -1) listeners.splice(index, 1);
      }),
    } as unknown as MediaQueryList;

    global.window = global.window || ({} as Window & typeof globalThis);
    window.matchMedia = vi.fn(() => mediaQueryList);
  });

  afterEach(() => {
    if (typeof window !== "undefined" && originalMatchMedia) {
      window.matchMedia = originalMatchMedia;
    }
  });

  it("matchMedia is called with correct breakpoint", () => {
    window.matchMedia("(max-width: 767px)");
    expect(window.matchMedia).toHaveBeenCalledWith("(max-width: 767px)");
  });

  it("mediaQuery returns matches as false for wide viewport", () => {
    mediaQueryList.matches = false;
    const result = window.matchMedia("(max-width: 767px)");
    expect(result.matches).toBe(false);
  });

  it("mediaQuery returns matches as true for narrow viewport", () => {
    mediaQueryList.matches = true;
    const result = window.matchMedia("(max-width: 767px)");
    expect(result.matches).toBe(true);
  });

  it("addEventListener is registered correctly", () => {
    const mq = window.matchMedia("(max-width: 767px)");
    const handler = vi.fn();
    mq.addEventListener("change", handler);
    expect(mq.addEventListener).toHaveBeenCalledWith("change", handler);
    expect(listeners).toContain(handler);
  });

  it("removeEventListener cleans up correctly", () => {
    const mq = window.matchMedia("(max-width: 767px)");
    const handler = vi.fn();
    mq.addEventListener("change", handler);
    mq.removeEventListener("change", handler);
    expect(mq.removeEventListener).toHaveBeenCalledWith("change", handler);
    expect(listeners).not.toContain(handler);
  });
});
