import { describe, it, expect } from "vitest";
import { parseHexAlpha, combineHexAlpha, enumColorsToCssVar } from "../color-utils";

// ---------------------------------------------------------------------------
// parseHexAlpha
// ---------------------------------------------------------------------------

describe("parseHexAlpha", () => {
  it("parses 8-char hex into rgb and alpha", () => {
    expect(parseHexAlpha("#FF000080")).toEqual({ rgb: "#FF0000", alpha: 128 });
  });

  it("parses 8-char hex with full alpha", () => {
    expect(parseHexAlpha("#FF0000FF")).toEqual({ rgb: "#FF0000", alpha: 255 });
  });

  it("parses 8-char hex with zero alpha", () => {
    expect(parseHexAlpha("#FF000000")).toEqual({ rgb: "#FF0000", alpha: 0 });
  });

  it("treats 6-char hex as fully opaque", () => {
    expect(parseHexAlpha("#FF0000")).toEqual({ rgb: "#FF0000", alpha: 255 });
  });

  it("returns default for empty input", () => {
    expect(parseHexAlpha("")).toEqual({ rgb: "#000000", alpha: 255 });
  });

  it("returns input as rgb when missing # prefix", () => {
    const result = parseHexAlpha("FF0000");
    expect(result).toEqual({ rgb: "FF0000", alpha: 255 });
  });

  it("returns default for null-like input", () => {
    expect(parseHexAlpha(undefined as unknown as string)).toEqual({
      rgb: "#000000",
      alpha: 255,
    });
  });

  it("handles short hex strings by falling back to #000000", () => {
    expect(parseHexAlpha("#FFF")).toEqual({ rgb: "#000000", alpha: 255 });
  });
});

// ---------------------------------------------------------------------------
// combineHexAlpha
// ---------------------------------------------------------------------------

describe("combineHexAlpha", () => {
  it("combines rgb and alpha into 8-char hex", () => {
    expect(combineHexAlpha("#FF0000", 128)).toBe("#FF000080");
  });

  it("returns 6-char hex when alpha is 255 (fully opaque)", () => {
    expect(combineHexAlpha("#FF0000", 255)).toBe("#FF0000");
  });

  it("returns 6-char hex when alpha exceeds 255", () => {
    expect(combineHexAlpha("#FF0000", 300)).toBe("#FF0000");
  });

  it("clamps alpha to 0 minimum", () => {
    expect(combineHexAlpha("#FF0000", -10)).toBe("#FF000000");
  });

  it("combines with zero alpha", () => {
    expect(combineHexAlpha("#FF0000", 0)).toBe("#FF000000");
  });

  it("prepends # when input lacks it", () => {
    expect(combineHexAlpha("FF0000", 128)).toBe("#FF000080");
  });

  it("falls back to #000000 for invalid base length", () => {
    expect(combineHexAlpha("#FFF", 128)).toBe("#00000080");
  });
});

// ---------------------------------------------------------------------------
// enumColorsToCssVar
// ---------------------------------------------------------------------------

describe("enumColorsToCssVar", () => {
  const expectedColors = [
    "black",
    "white",
    "slate",
    "gray",
    "zinc",
    "neutral",
    "stone",
    "red",
    "orange",
    "amber",
    "yellow",
    "lime",
    "green",
    "emerald",
    "teal",
    "cyan",
    "sky",
    "blue",
    "indigo",
    "violet",
    "purple",
    "fuchsia",
    "pink",
    "rose",
    "primary",
    "secondary",
    "destructive",
    "success",
    "warning",
    "info",
    "muted",
  ];

  it("contains all expected color names", () => {
    for (const color of expectedColors) {
      expect(enumColorsToCssVar).toHaveProperty(color);
    }
  });

  it("all values follow var(--color-<name>) format", () => {
    for (const [name, value] of Object.entries(enumColorsToCssVar)) {
      expect(value).toBe(`var(--color-${name})`);
    }
  });

  it("has exactly the expected number of entries", () => {
    expect(Object.keys(enumColorsToCssVar)).toHaveLength(expectedColors.length);
  });
});
