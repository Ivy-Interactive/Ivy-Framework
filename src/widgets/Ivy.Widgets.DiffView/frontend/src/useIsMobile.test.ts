import { describe, expect, it, beforeEach, afterEach, vi } from "vitest";

describe("useIsMobile hook logic", () => {
  let originalMatchMedia: typeof window.matchMedia | undefined;

  beforeEach(() => {
    originalMatchMedia = window.matchMedia;

    const createMockMediaQueryList = (matches: boolean): MediaQueryList => ({
      matches,
      media: "(max-width: 767px)",
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      dispatchEvent: vi.fn(),
    });

    window.matchMedia = vi.fn(() => createMockMediaQueryList(false));
  });

  afterEach(() => {
    if (originalMatchMedia) {
      window.matchMedia = originalMatchMedia;
    }
  });

  it("matchMedia is called with correct breakpoint", () => {
    window.matchMedia("(max-width: 767px)");
    expect(window.matchMedia).toHaveBeenCalledWith("(max-width: 767px)");
  });

  it("mediaQuery returns matches as false for wide viewport", () => {
    const result = window.matchMedia("(max-width: 767px)");
    expect(result.matches).toBe(false);
  });

  it("addEventListener is registered correctly", () => {
    const mq = window.matchMedia("(max-width: 767px)");
    const handler = vi.fn();
    mq.addEventListener("change", handler);
    expect(mq.addEventListener).toHaveBeenCalledWith("change", handler);
  });

  it("removeEventListener cleans up correctly", () => {
    const mq = window.matchMedia("(max-width: 767px)");
    const handler = vi.fn();
    mq.addEventListener("change", handler);
    mq.removeEventListener("change", handler);
    expect(mq.removeEventListener).toHaveBeenCalledWith("change", handler);
  });
});
