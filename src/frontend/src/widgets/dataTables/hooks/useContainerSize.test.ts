import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

/**
 * Source-level tests for useContainerSize.
 * Sizing follows the framework responsive pattern (useBreakpoint + window resize),
 * not ResizeObserver.
 */
describe("useContainerSize - layout measurement without ResizeObserver", () => {
  const hookSource = fs.readFileSync(path.resolve(__dirname, "./useContainerSize.ts"), "utf-8");

  it("should not use ResizeObserver", () => {
    expect(hookSource).not.toContain("new ResizeObserver");
  });

  it("should remeasure when the responsive breakpoint changes", () => {
    expect(hookSource).toContain("useCurrentBreakpoint");
    expect(hookSource).toContain("breakpoint");
  });

  it("should measure via clientWidth and clientHeight", () => {
    expect(hookSource).toContain("el.clientWidth");
    expect(hookSource).toContain("el.clientHeight");
  });

  it("should listen for window resize with debounce like useBreakpoint", () => {
    expect(hookSource).toContain('window.addEventListener("resize"');
    expect(hookSource).toContain('window.removeEventListener("resize"');
    expect(hookSource).toContain("RESIZE_DEBOUNCE_MS");
    expect(hookSource).toContain("setTimeout");
  });

  it("should retry initial measurement via requestAnimationFrame", () => {
    expect(hookSource).toContain("retries++ < 10");
    expect(hookSource).toContain("requestAnimationFrame(tryInit)");
  });

  it("should read scroll area height from the glide scroller element", () => {
    expect(hookSource).toContain(".dvn-scroller");
    expect(hookSource).toContain("scroller.clientHeight");
  });
});

describe("DataTableWidget - flex sizing for unconstrained parents", () => {
  const widgetSource = fs.readFileSync(path.resolve(__dirname, "../DataTableWidget.tsx"), "utf-8");

  it('should remove height: 100% when height is "Full"', () => {
    expect(widgetSource).toContain("delete containerStyle.height");
  });

  it('should set flexGrow to 1 when height is "Full"', () => {
    expect(widgetSource).toContain("flexGrow = 1");
  });

  it("should move explicit height to flexBasis for proportional shrinking", () => {
    expect(widgetSource).toContain("containerStyle.flexBasis = containerStyle.height");
    expect(widgetSource).toContain("delete containerStyle.height");
  });

  it("should compute min height from density and table chrome via getDataTableMinHeight", () => {
    expect(widgetSource).toContain("getDataTableMinHeight");
    expect(widgetSource).toContain("containerStyle.minHeight = minHeight");
  });
});
