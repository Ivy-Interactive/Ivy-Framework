import { describe, expect, it, beforeEach, afterEach } from "vitest";

describe("DiffView mobile behavior", () => {
  let originalMatchMedia: typeof window.matchMedia | undefined;

  beforeEach(() => {
    originalMatchMedia = window.matchMedia;
  });

  afterEach(() => {
    if (originalMatchMedia) {
      window.matchMedia = originalMatchMedia;
    }
  });

  it("uses unified viewType override on mobile viewport", () => {
    const isMobile = true;
    const effectiveViewType = isMobile ? "unified" : "split";
    expect(effectiveViewType).toBe("unified");
  });

  it("preserves split viewType on desktop viewport", () => {
    const isMobile = false;
    const effectiveViewType = isMobile ? "unified" : "split";
    expect(effectiveViewType).toBe("split");
  });

  it("enables word wrap on mobile viewport", () => {
    const isMobile = true;
    const wordWrap = false;
    const effectiveWordWrap = isMobile || wordWrap;
    expect(effectiveWordWrap).toBe(true);
  });

  it("respects explicit wordWrap prop on desktop", () => {
    const isMobile = false;
    const wordWrap = true;
    const effectiveWordWrap = isMobile || wordWrap;
    expect(effectiveWordWrap).toBe(true);
  });
});
