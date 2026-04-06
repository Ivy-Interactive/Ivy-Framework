import { describe, it, expect, vi, afterEach } from "vitest";
import { convertToHex, getThemeColorHex, getDisplayColor } from "../color-utils";

// ---------------------------------------------------------------------------
// convertToHex
// ---------------------------------------------------------------------------

describe("convertToHex", () => {
  it("returns empty string for empty input", () => {
    expect(convertToHex("")).toBe("");
  });

  it("passes through hex values unchanged", () => {
    expect(convertToHex("#ff0000")).toBe("#ff0000");
  });

  it("passes through 8-char hex values unchanged", () => {
    expect(convertToHex("#ff000080")).toBe("#ff000080");
  });

  it("converts rgb to hex", () => {
    expect(convertToHex("rgb(255, 0, 0)")).toBe("#ff0000");
  });

  it("converts rgb with different values", () => {
    expect(convertToHex("rgb(0, 128, 255)")).toBe("#0080ff");
  });

  it("converts rgba with full opacity to 6-char hex", () => {
    expect(convertToHex("rgba(255, 0, 0, 1)")).toBe("#ff0000");
  });

  it("converts rgba with partial alpha to 8-char hex", () => {
    expect(convertToHex("rgba(255, 0, 0, 0.5)")).toBe("#ff000080");
  });

  it("converts rgba with zero alpha", () => {
    expect(convertToHex("rgba(255, 0, 0, 0)")).toBe("#ff000000");
  });

  it("converts hsl to hex", () => {
    expect(convertToHex("hsl(0, 100%, 50%)")).toBe("#ff0000");
  });

  it("converts achromatic hsl (grey)", () => {
    expect(convertToHex("hsl(0, 0%, 50%)")).toBe("#808080");
  });

  it("returns #000000 for oklch (unsupported)", () => {
    expect(convertToHex("oklch(0.5 0.2 30)")).toBe("#000000");
  });

  it("returns the value as-is for unknown color formats", () => {
    expect(convertToHex("not-a-color")).toBe("not-a-color");
  });

  it("looks up named colors from enumColorsToCssVar and falls back to input when no DOM", () => {
    // Without a proper DOM (getComputedStyle returns empty), named colors fall through
    expect(convertToHex("red")).toBe("red");
  });

  it("resolves named colors to hex via theme color lookup when DOM is available", () => {
    vi.spyOn(globalThis, "getComputedStyle").mockReturnValue({
      getPropertyValue: () => "#ef4444",
    } as unknown as CSSStyleDeclaration);

    expect(convertToHex("red")).toBe("#ef4444");

    vi.restoreAllMocks();
  });

  it("falls back to raw color name when theme returns rgb instead of hex", () => {
    vi.spyOn(globalThis, "getComputedStyle").mockReturnValue({
      getPropertyValue: () => "rgb(255, 0, 0)",
    } as unknown as CSSStyleDeclaration);

    expect(convertToHex("red")).toBe("red");

    vi.restoreAllMocks();
  });

  it("falls back to raw color name when theme returns empty string", () => {
    vi.spyOn(globalThis, "getComputedStyle").mockReturnValue({
      getPropertyValue: () => "",
    } as unknown as CSSStyleDeclaration);

    expect(convertToHex("red")).toBe("red");

    vi.restoreAllMocks();
  });
});

// ---------------------------------------------------------------------------
// getThemeColorHex
// ---------------------------------------------------------------------------

describe("getThemeColorHex", () => {
  const originalWindow = globalThis.window;

  afterEach(() => {
    vi.restoreAllMocks();
    // Restore window
    if (originalWindow === undefined) {
      // @ts-expect-error - test cleanup
      delete globalThis.window;
    }
  });

  it("returns undefined when window is undefined", () => {
    const savedWindow = globalThis.window;
    // @ts-expect-error - simulating SSR
    delete globalThis.window;
    expect(getThemeColorHex("--color-red")).toBeUndefined();
    globalThis.window = savedWindow;
  });

  it("returns hex when computed style provides valid #rrggbb", () => {
    vi.spyOn(globalThis, "getComputedStyle").mockReturnValue({
      getPropertyValue: () => "#ff0000",
    } as unknown as CSSStyleDeclaration);

    expect(getThemeColorHex("--color-red")).toBe("#ff0000");
  });

  it("returns undefined for non-hex computed values", () => {
    vi.spyOn(globalThis, "getComputedStyle").mockReturnValue({
      getPropertyValue: () => "rgb(255, 0, 0)",
    } as unknown as CSSStyleDeclaration);

    expect(getThemeColorHex("--color-red")).toBeUndefined();
  });
});

// ---------------------------------------------------------------------------
// getDisplayColor
// ---------------------------------------------------------------------------

describe("getDisplayColor", () => {
  it("returns #000000 for empty string", () => {
    expect(getDisplayColor("")).toBe("#000000");
  });

  it("returns valid hex color as-is", () => {
    expect(getDisplayColor("#ff0000")).toBe("#ff0000");
  });

  it("strips alpha channel from 9-char hex", () => {
    expect(getDisplayColor("#ff000080")).toBe("#ff0000");
  });

  it("returns #000000 for CSS var result", () => {
    // convertToHex returns "var(--color-red)" for named colors without DOM
    // but getDisplayColor should fall back since var( prefix
    expect(getDisplayColor("oklch(0.5 0.2 30)")).toBe("#000000");
  });

  it("returns #000000 for non-hex string", () => {
    expect(getDisplayColor("not-a-color")).toBe("#000000");
  });

  it("converts rgb value to hex and returns it", () => {
    expect(getDisplayColor("rgb(255, 0, 0)")).toBe("#ff0000");
  });
});
