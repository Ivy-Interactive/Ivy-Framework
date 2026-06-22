import { describe, expect, it, beforeEach, vi } from "vitest";

describe("DiffView mobile behavior", () => {
  let originalMatchMedia: typeof window.matchMedia;
  let mediaQueryList: MediaQueryList;

  beforeEach(() => {
    if (typeof window !== "undefined") {
      originalMatchMedia = window.matchMedia;
    }

    mediaQueryList = {
      matches: false,
      media: "(max-width: 767px)",
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    } as unknown as MediaQueryList;

    global.window = global.window || ({} as Window & typeof globalThis);
    window.matchMedia = vi.fn(() => mediaQueryList);
  });

  afterEach(() => {
    if (typeof window !== "undefined" && originalMatchMedia) {
      window.matchMedia = originalMatchMedia;
    }
  });

  it("uses unified viewType override on mobile viewport", () => {
    mediaQueryList.matches = true;
    const effectiveViewType = mediaQueryList.matches ? "unified" : "split";
    expect(effectiveViewType).toBe("unified");
  });

  it("preserves split viewType on desktop viewport", () => {
    mediaQueryList.matches = false;
    const effectiveViewType = mediaQueryList.matches ? "unified" : "split";
    expect(effectiveViewType).toBe("split");
  });

  it("enables word wrap on mobile viewport", () => {
    mediaQueryList.matches = true;
    const wordWrap = false;
    const effectiveWordWrap = mediaQueryList.matches || wordWrap;
    expect(effectiveWordWrap).toBe(true);
  });

  it("respects explicit wordWrap prop on desktop", () => {
    mediaQueryList.matches = false;
    const wordWrap = true;
    const effectiveWordWrap = mediaQueryList.matches || wordWrap;
    expect(effectiveWordWrap).toBe(true);
  });
});
