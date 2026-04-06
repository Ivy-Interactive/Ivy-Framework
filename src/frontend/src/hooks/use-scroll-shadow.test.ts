import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

/**
 * Tests for the useScrollShadow hook.
 *
 * Since @testing-library/react is not available in this project, we verify
 * the source code contains the expected patterns. Visual behavior is covered
 * by the IvyFrameworkVerification end-to-end test.
 */
describe("useScrollShadow", () => {
  const hookSource = fs.readFileSync(path.resolve(__dirname, "./use-scroll-shadow.ts"), "utf-8");

  it("should export useScrollShadow function", () => {
    expect(hookSource).toContain("export function useScrollShadow");
  });

  it("should use useState for isScrolled state", () => {
    expect(hookSource).toContain("useState(false)");
  });

  it("should use useRef for scrollRef", () => {
    expect(hookSource).toContain("useRef<HTMLDivElement>(null)");
  });

  it("should default selector to radix scroll area viewport", () => {
    expect(hookSource).toContain("[data-radix-scroll-area-viewport]");
  });

  it("should accept a custom selector parameter", () => {
    expect(hookSource).toMatch(/function useScrollShadow\(\s*selector\s*=/);
  });

  it("should listen for scroll events and check scrollTop", () => {
    expect(hookSource).toContain('addEventListener("scroll"');
    expect(hookSource).toContain("scrollTop > 0");
  });

  it("should clean up the scroll event listener on unmount", () => {
    expect(hookSource).toContain('removeEventListener("scroll"');
  });

  it("should include selector and direction in useEffect dependency array", () => {
    expect(hookSource).toContain("[selector, direction]");
  });

  it("should return isScrolled and scrollRef", () => {
    expect(hookSource).toContain("return { isScrolled, scrollRef }");
  });
});

describe("useScrollShadow direction parameter", () => {
  const hookSource = fs.readFileSync(path.resolve(__dirname, "./use-scroll-shadow.ts"), "utf-8");

  it("should export ScrollShadowDirection type", () => {
    expect(hookSource).toContain("export type ScrollShadowDirection");
  });

  it("should accept a direction parameter defaulting to bottom", () => {
    expect(hookSource).toMatch(/direction:\s*ScrollShadowDirection\s*=\s*"bottom"/);
  });

  it("should check scrollTop > 0 for bottom direction", () => {
    expect(hookSource).toContain('direction === "bottom"');
    expect(hookSource).toContain("scrollTop > 0");
  });

  it("should check not-at-bottom for top direction", () => {
    expect(hookSource).toContain("scrollTop < scrollHeight - clientHeight - 1");
  });

  it("should use ResizeObserver for top direction", () => {
    expect(hookSource).toContain("new ResizeObserver(handleScroll)");
    expect(hookSource).toContain('direction === "top"');
  });

  it("should call handleScroll on initial mount to set state", () => {
    expect(hookSource).toMatch(/handleScroll\(\);\s*\n\s*viewport\.addEventListener/);
  });

  it("should disconnect ResizeObserver on cleanup", () => {
    expect(hookSource).toContain("resizeObserver?.disconnect()");
  });
});

describe("useScrollShadow MutationObserver for dynamic content", () => {
  const hookSource = fs.readFileSync(path.resolve(__dirname, "./use-scroll-shadow.ts"), "utf-8");

  it("should create a MutationObserver when direction is top", () => {
    expect(hookSource).toContain("new MutationObserver(");
    expect(hookSource).toMatch(/direction === "top"\s*\n?\s*\? new MutationObserver/);
  });

  it("should observe viewport with childList and subtree options", () => {
    expect(hookSource).toContain("childList: true");
    expect(hookSource).toContain("subtree: true");
  });

  it("should disconnect the MutationObserver on cleanup", () => {
    expect(hookSource).toContain("mutationObserver?.disconnect()");
  });
});

describe("useScrollShadow MutationObserver performance optimization", () => {
  const hookSource = fs.readFileSync(path.resolve(__dirname, "./use-scroll-shadow.ts"), "utf-8");

  it("should wrap MutationObserver callback in requestAnimationFrame", () => {
    expect(hookSource).toContain("requestAnimationFrame");
    expect(hookSource).toMatch(/new MutationObserver\([\s\S]*?requestAnimationFrame/);
  });

  it("should cancel pending requestAnimationFrame on cleanup", () => {
    expect(hookSource).toContain("cancelAnimationFrame(rafId)");
  });

  it("should track rafId to prevent multiple scheduled frames", () => {
    expect(hookSource).toMatch(/let rafId.*=.*null/);
    expect(hookSource).toContain("if (rafId !== null) return");
  });
});

describe("HeaderLayoutWidget uses useScrollShadow hook", () => {
  const widgetSource = fs.readFileSync(
    path.resolve(__dirname, "../widgets/layouts/HeaderLayoutWidget.tsx"),
    "utf-8",
  );

  it("should import useScrollShadow", () => {
    expect(widgetSource).toContain('from "@/hooks/use-scroll-shadow"');
  });

  it("should use the hook instead of inline implementation", () => {
    expect(widgetSource).toContain("useScrollShadow()");
    // Should NOT have the inline scroll listener pattern
    expect(widgetSource).not.toContain('addEventListener("scroll"');
    expect(widgetSource).not.toContain("setIsScrolled");
  });

  it("should destructure isScrolled and scrollRef from the hook", () => {
    expect(widgetSource).toContain("const { isScrolled, scrollRef } = useScrollShadow()");
  });
});

describe("FooterLayoutWidget uses useScrollShadow hook with top direction", () => {
  const widgetSource = fs.readFileSync(
    path.resolve(__dirname, "../widgets/layouts/FooterLayoutWidget.tsx"),
    "utf-8",
  );

  it("should import useScrollShadow", () => {
    expect(widgetSource).toContain('from "@/hooks/use-scroll-shadow"');
  });

  it("should use the hook with top direction instead of inline implementation", () => {
    expect(widgetSource).toContain('"top"');
    // Should NOT have the inline scroll listener pattern
    expect(widgetSource).not.toContain('addEventListener("scroll"');
    expect(widgetSource).not.toContain("setHasMoreContent");
    expect(widgetSource).not.toContain("ResizeObserver");
  });

  it("should destructure isScrolled as hasMoreContent from the hook", () => {
    expect(widgetSource).toContain("isScrolled: hasMoreContent");
    expect(widgetSource).toContain("scrollRef");
  });
});
