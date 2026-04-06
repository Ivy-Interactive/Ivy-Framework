import { describe, it, expect } from "vitest";
import { resolveColor, CHROMATIC_COLORS, DEFAULT_SEMANTIC_COLORS } from "./SignatureInputWidget";

/** A full color map combining chromatic + default semantic colors, matching runtime defaults. */
const fullColorMap: Record<string, string> = {
  ...CHROMATIC_COLORS,
  ...DEFAULT_SEMANTIC_COLORS,
};

describe("resolveColor", () => {
  it("returns fallback when color is undefined", () => {
    expect(resolveColor(undefined, "#000", fullColorMap)).toBe("#000");
  });

  it("returns fallback when color is empty string", () => {
    expect(resolveColor("", "#000", fullColorMap)).toBe("#000");
  });

  it("passes through hex colors", () => {
    expect(resolveColor("#ff0000", "#000", fullColorMap)).toBe("#ff0000");
  });

  it("passes through rgb colors", () => {
    expect(resolveColor("rgb(255,0,0)", "#000", fullColorMap)).toBe("rgb(255,0,0)");
  });

  it("passes through rgba colors", () => {
    expect(resolveColor("rgba(255,0,0,0.5)", "#000", fullColorMap)).toBe("rgba(255,0,0,0.5)");
  });

  it("resolves named colors from colorMap", () => {
    expect(resolveColor("blue", "#000", fullColorMap)).toBe("#3b82f6");
  });

  it("performs case-insensitive lookup", () => {
    expect(resolveColor("Blue", "#000", fullColorMap)).toBe("#3b82f6");
    expect(resolveColor("BLUE", "#000", fullColorMap)).toBe("#3b82f6");
  });

  it("resolves semantic colors", () => {
    expect(resolveColor("primary", "#000", fullColorMap)).toBe("#3b82f6");
    expect(resolveColor("destructive", "#000", fullColorMap)).toBe("#ef4444");
  });

  it("returns fallback for unknown color names", () => {
    expect(resolveColor("nonexistent", "#fff", fullColorMap)).toBe("#fff");
  });
});

describe("CHROMATIC_COLORS", () => {
  it("contains all expected named colors", () => {
    const expectedColors = [
      "black",
      "white",
      "red",
      "blue",
      "green",
      "yellow",
      "orange",
      "purple",
      "pink",
      "gray",
      "slate",
      "zinc",
      "neutral",
      "stone",
      "amber",
      "lime",
      "emerald",
      "teal",
      "cyan",
      "sky",
      "indigo",
      "violet",
      "fuchsia",
      "rose",
    ];
    for (const color of expectedColors) {
      expect(CHROMATIC_COLORS).toHaveProperty(color);
    }
  });

  it("all values are valid hex strings", () => {
    for (const [key, value] of Object.entries(CHROMATIC_COLORS)) {
      expect(value, `${key} should be a valid hex color`).toMatch(/^#[0-9a-f]{6}$/);
    }
  });
});

describe("DEFAULT_SEMANTIC_COLORS", () => {
  it("contains all expected semantic colors", () => {
    const expectedKeys = [
      "primary",
      "secondary",
      "destructive",
      "muted",
      "foreground",
      "background",
    ];
    for (const key of expectedKeys) {
      expect(DEFAULT_SEMANTIC_COLORS).toHaveProperty(key);
    }
  });

  it("semantic aliases match their base chromatic colors", () => {
    expect(DEFAULT_SEMANTIC_COLORS.primary).toBe(CHROMATIC_COLORS.blue);
    expect(DEFAULT_SEMANTIC_COLORS.destructive).toBe(CHROMATIC_COLORS.red);
    expect(DEFAULT_SEMANTIC_COLORS.secondary).toBe(CHROMATIC_COLORS.gray);
  });

  it("all values are valid hex strings", () => {
    for (const [key, value] of Object.entries(DEFAULT_SEMANTIC_COLORS)) {
      expect(value, `${key} should be a valid hex color`).toMatch(/^#[0-9a-f]{6}$/);
    }
  });
});
