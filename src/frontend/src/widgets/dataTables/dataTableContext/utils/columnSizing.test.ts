import { describe, it, expect, vi } from "vitest";
import * as arrow from "apache-arrow";
import {
  parseSize,
  parseSizeGrow,
  parseSizeMin,
  estimateHeaderWidth,
  estimateContentWidth,
  getSizeMode,
} from "./columnSizing";

describe("parseSize", () => {
  // Fixed size types
  it("should parse Px size", () => {
    expect(parseSize("Px:200")).toBe(200);
  });

  it("should parse Rem size", () => {
    expect(parseSize("Rem:10")).toBe(160);
  });

  it("should parse Units size", () => {
    expect(parseSize("Units:20")).toBe(80);
  });

  it("should parse Units:1", () => {
    expect(parseSize("Units:1")).toBe(4);
  });

  // Grow/proportional types return 0
  it("should return 0 for Fraction", () => {
    expect(parseSize("Fraction:0.5")).toBe(0);
  });

  it("should return 0 for Fraction:1", () => {
    expect(parseSize("Fraction:1")).toBe(0);
  });

  it("should return 0 for Auto", () => {
    expect(parseSize("Auto")).toBe(0);
  });

  it("should return 0 for Grow:2", () => {
    expect(parseSize("Grow:2")).toBe(0);
  });

  it("should return 0 for Grow without value", () => {
    expect(parseSize("Grow")).toBe(0);
  });

  it("should return 0 for Full", () => {
    expect(parseSize("Full")).toBe(0);
  });

  it("should return 0 for Fit", () => {
    expect(parseSize("Fit")).toBe(0);
  });

  it("should return 0 for Screen", () => {
    expect(parseSize("Screen")).toBe(0);
  });

  it("should return 0 for MinContent", () => {
    expect(parseSize("MinContent")).toBe(0);
  });

  it("should return 0 for MaxContent", () => {
    expect(parseSize("MaxContent")).toBe(0);
  });

  it("should return 0 for Shrink:1", () => {
    expect(parseSize("Shrink:1")).toBe(0);
  });

  it("should return 0 for Shrink without value", () => {
    expect(parseSize("Shrink")).toBe(0);
  });

  // Edge cases
  it("should return 150 for undefined", () => {
    expect(parseSize(undefined)).toBe(150);
  });

  it("should return number as-is", () => {
    expect(parseSize(42)).toBe(42);
  });

  it("should return 150 for empty string", () => {
    expect(parseSize("")).toBe(150);
  });

  it("should return 150 for unknown type", () => {
    expect(parseSize("Unknown:5")).toBe(150);
  });

  // Min/max in primary parsing (ignored)
  it("should parse only primary part from min/max string (Fraction)", () => {
    expect(parseSize("Fraction:0.5,Px:100,Px:500")).toBe(0);
  });

  it("should parse only primary part from min/max string (Px)", () => {
    expect(parseSize("Px:200,Px:100,Px:500")).toBe(200);
  });
});

describe("parseSizeGrow", () => {
  // Fraction types
  it("should return 0.5 for Fraction:0.5", () => {
    expect(parseSizeGrow("Fraction:0.5")).toBe(0.5);
  });

  it("should return 0.333 for Fraction:0.333", () => {
    expect(parseSizeGrow("Fraction:0.333")).toBe(0.333);
  });

  it("should return 1 for Fraction:1", () => {
    expect(parseSizeGrow("Fraction:1")).toBe(1);
  });

  // Grow types
  it("should return 2 for Grow:2", () => {
    expect(parseSizeGrow("Grow:2")).toBe(2);
  });

  it("should return 1 for Grow without value", () => {
    expect(parseSizeGrow("Grow")).toBe(1);
  });

  it("should return 1 for Grow:1", () => {
    expect(parseSizeGrow("Grow:1")).toBe(1);
  });

  // Other grow types
  it("should return 1 for Auto", () => {
    expect(parseSizeGrow("Auto")).toBe(1);
  });

  it("should return 1 for Full", () => {
    expect(parseSizeGrow("Full")).toBe(1);
  });

  it("should return 1 for Screen", () => {
    expect(parseSizeGrow("Screen")).toBe(1);
  });

  // Non-grow types
  it("should return undefined for Px", () => {
    expect(parseSizeGrow("Px:200")).toBeUndefined();
  });

  it("should return undefined for Rem", () => {
    expect(parseSizeGrow("Rem:10")).toBeUndefined();
  });

  it("should return undefined for Units", () => {
    expect(parseSizeGrow("Units:20")).toBeUndefined();
  });

  it("should return undefined for Fit", () => {
    expect(parseSizeGrow("Fit")).toBeUndefined();
  });

  it("should return undefined for MinContent", () => {
    expect(parseSizeGrow("MinContent")).toBeUndefined();
  });

  it("should return undefined for MaxContent", () => {
    expect(parseSizeGrow("MaxContent")).toBeUndefined();
  });

  it("should return undefined for Shrink", () => {
    expect(parseSizeGrow("Shrink:1")).toBeUndefined();
  });

  // Edge cases
  it("should return undefined for undefined", () => {
    expect(parseSizeGrow(undefined)).toBeUndefined();
  });

  it("should return undefined for number", () => {
    expect(parseSizeGrow(42)).toBeUndefined();
  });

  it("should extract grow from min/max string", () => {
    expect(parseSizeGrow("Fraction:0.5,Px:100,Px:500")).toBe(0.5);
  });
});

describe("parseSizeMin", () => {
  it("should extract Px min from Fraction with min/max", () => {
    expect(parseSizeMin("Fraction:0.5,Px:100,Px:500")).toBe(100);
  });

  it("should extract Units min from Auto with min", () => {
    expect(parseSizeMin("Auto,Units:20")).toBe(80);
  });

  it("should extract Rem min from Grow with min", () => {
    expect(parseSizeMin("Grow:1,Rem:5")).toBe(80);
  });

  it("should return undefined for Px without min", () => {
    expect(parseSizeMin("Px:200")).toBeUndefined();
  });

  it("should return undefined for Fraction without min", () => {
    expect(parseSizeMin("Fraction:0.5")).toBeUndefined();
  });

  it("should return undefined for undefined", () => {
    expect(parseSizeMin(undefined)).toBeUndefined();
  });

  it("should return undefined for number", () => {
    expect(parseSizeMin(42)).toBeUndefined();
  });
});

describe("estimateHeaderWidth", () => {
  it("should return MIN_WIDTH (60) for empty string without font", () => {
    // "" => 0 * 8 + 40 = 40, clamped to min 60
    expect(estimateHeaderWidth("")).toBe(60);
  });

  it("should return clamped value at MAX_AUTO_WIDTH (300) for very long text without font", () => {
    const longText = "A".repeat(100); // 100 * 8 + 40 = 840, clamped to 300
    expect(estimateHeaderWidth(longText)).toBe(300);
  });

  it("should use character-count fallback without font parameter", () => {
    // "Hello" = 5 * 8 + 40 = 80
    expect(estimateHeaderWidth("Hello")).toBe(80);
  });

  it("should use canvas measurement when font is provided and narrow chars produce smaller width than wide chars", () => {
    const widths: Record<string, number> = {
      Test: 50,
      iii: 15,
      WWW: 45,
    };
    const mockMeasureText = vi.fn((text: string) => ({ width: widths[text] ?? 0 }));
    const mockGetContext = vi.fn().mockReturnValue({
      font: "",
      measureText: mockMeasureText,
    });
    vi.spyOn(document, "createElement").mockReturnValue({
      getContext: mockGetContext,
    } as unknown as HTMLCanvasElement);

    // "Test" measured at 50px => 50 + 40 = 90
    expect(estimateHeaderWidth("Test", "bold 13px sans-serif")).toBe(90);
    expect(mockMeasureText).toHaveBeenCalledWith("Test");

    // "iii" measured at 15px => 15 + 40 = 55, clamped to MIN_WIDTH 60
    const narrowWidth = estimateHeaderWidth("iii", "bold 13px sans-serif");
    // "WWW" measured at 45px => 45 + 40 = 85
    const wideWidth = estimateHeaderWidth("WWW", "bold 13px sans-serif");
    expect(narrowWidth).toBeLessThan(wideWidth);

    vi.restoreAllMocks();
  });
});

function makeTable(fields: { name: string; values: (string | number | null)[] }[]): arrow.Table {
  const columns: Record<string, (string | number | null)[]> = {};
  for (const f of fields) {
    columns[f.name] = f.values;
  }
  return arrow.tableFromArrays(columns);
}

describe("estimateContentWidth", () => {
  it("should return undefined for empty table (0 rows)", () => {
    const table = makeTable([{ name: "col", values: [] }]);
    expect(estimateContentWidth(table, 0, "fit")).toBeUndefined();
  });

  it("should return undefined for out-of-bounds column index", () => {
    const table = makeTable([{ name: "col", values: ["hello"] }]);
    expect(estimateContentWidth(table, 5, "fit")).toBeUndefined();
    expect(estimateContentWidth(table, -1, "fit")).toBeUndefined();
  });

  it("should sample up to 20 rows maximum", () => {
    // Create a table with 30 rows, first 20 are short, last 10 are very long
    const values = [
      ...Array.from({ length: 20 }, () => "short"),
      ...Array.from({ length: 10 }, () => "a".repeat(50)),
    ];
    const table = makeTable([{ name: "col", values }]);
    const result = estimateContentWidth(table, 0, "max");
    // "short" = 5 chars * 8 + 24 = 64px
    // If it sampled beyond 20, "a".repeat(50) = 50*8+24 = 424 → capped at 400
    // Since max of first 20 should be 64, not 400
    expect(result).toBe(64);
  });

  it("mode 'max' should return width of the longest cell content", () => {
    const table = makeTable([{ name: "col", values: ["ab", "abcdef", "abc"] }]);
    // "abcdef" = 6*8+24 = 72
    expect(estimateContentWidth(table, 0, "max")).toBe(72);
  });

  it("mode 'min' should return width of the shortest cell content", () => {
    const table = makeTable([{ name: "col", values: ["ab", "abcdef", "abc"] }]);
    // "ab" = 2*8+24 = 40 → clamped to MIN_CONTENT_WIDTH = 60
    expect(estimateContentWidth(table, 0, "min")).toBe(60);
  });

  it("mode 'fit' should return same as max", () => {
    const table = makeTable([{ name: "col", values: ["ab", "abcdef", "abc"] }]);
    const fitResult = estimateContentWidth(table, 0, "fit");
    const maxResult = estimateContentWidth(table, 0, "max");
    expect(fitResult).toBe(maxResult);
  });

  it("should handle null values (treated as empty string)", () => {
    const table = makeTable([{ name: "col", values: [null, "hello", null] }]);
    // null → "" → 0*8+24 = 24 → clamped to 60
    // "hello" → 5*8+24 = 64
    expect(estimateContentWidth(table, 0, "min")).toBe(60);
    expect(estimateContentWidth(table, 0, "max")).toBe(64);
  });

  it("should respect MIN_CONTENT_WIDTH (60px) floor", () => {
    const table = makeTable([{ name: "col", values: ["a"] }]);
    // "a" = 1*8+24 = 32 → clamped to 60
    expect(estimateContentWidth(table, 0, "fit")).toBe(60);
  });

  it("should respect MAX_CONTENT_WIDTH (400px) cap", () => {
    const table = makeTable([{ name: "col", values: ["a".repeat(100)] }]);
    // 100*8+24 = 824 → capped at 400
    expect(estimateContentWidth(table, 0, "fit")).toBe(400);
  });

  it("should use character-count fallback without font parameter", () => {
    const table = makeTable([{ name: "col", values: ["Hello"] }]);
    // "Hello" = 5*8+24 = 64
    expect(estimateContentWidth(table, 0, "fit")).toBe(64);
  });

  it("should use canvas measurement when font is provided", async () => {
    // Reset module cache to get a fresh _measureCanvas (null)
    vi.resetModules();

    const mockMeasureText = vi.fn((text: string) => ({
      width: text === "Hello" ? 40 : text === "World" ? 50 : 0,
    }));
    vi.spyOn(document, "createElement").mockReturnValue({
      getContext: vi.fn().mockReturnValue({
        font: "",
        measureText: mockMeasureText,
      }),
    } as unknown as HTMLCanvasElement);

    const { estimateContentWidth: freshEstimateContentWidth } = await import("./columnSizing");
    const table = makeTable([{ name: "col", values: ["Hello", "World"] }]);
    // "World" measured at 50px => 50 + 24 = 74
    expect(freshEstimateContentWidth(table, 0, "max", "13px sans-serif")).toBe(74);
    expect(mockMeasureText).toHaveBeenCalledWith("Hello");
    expect(mockMeasureText).toHaveBeenCalledWith("World");

    vi.restoreAllMocks();
  });

  it("should produce different widths for narrow vs wide chars when using canvas measurement", async () => {
    vi.resetModules();

    const mockMeasureText = vi.fn((text: string) => ({
      width: text === "iii" ? 12 : text === "WWW" ? 42 : 0,
    }));
    vi.spyOn(document, "createElement").mockReturnValue({
      getContext: vi.fn().mockReturnValue({
        font: "",
        measureText: mockMeasureText,
      }),
    } as unknown as HTMLCanvasElement);

    const { estimateContentWidth: freshEstimateContentWidth } = await import("./columnSizing");
    const narrowTable = makeTable([{ name: "col", values: ["iii"] }]);
    const wideTable = makeTable([{ name: "col", values: ["WWW"] }]);

    const narrowWidth = freshEstimateContentWidth(narrowTable, 0, "fit", "13px sans-serif");
    const wideWidth = freshEstimateContentWidth(wideTable, 0, "fit", "13px sans-serif");
    // "iii" = 12+24=36 → clamped to 60; "WWW" = 42+24=66
    expect(narrowWidth).toBeLessThan(wideWidth!);

    vi.restoreAllMocks();
  });
});

describe("getSizeMode", () => {
  it("should return 'fit' for Fit", () => {
    expect(getSizeMode("Fit")).toBe("fit");
  });

  it("should return 'min' for MinContent", () => {
    expect(getSizeMode("MinContent")).toBe("min");
  });

  it("should return 'max' for MaxContent", () => {
    expect(getSizeMode("MaxContent")).toBe("max");
  });

  it("should return undefined for Px:200", () => {
    expect(getSizeMode("Px:200")).toBeUndefined();
  });

  it("should return undefined for Auto", () => {
    expect(getSizeMode("Auto")).toBeUndefined();
  });

  it("should return undefined for Fraction:0.5", () => {
    expect(getSizeMode("Fraction:0.5")).toBeUndefined();
  });

  it("should return undefined for undefined", () => {
    expect(getSizeMode(undefined)).toBeUndefined();
  });

  it("should return undefined for number", () => {
    expect(getSizeMode(42)).toBeUndefined();
  });

  it("should return 'fit' for Fit with min/max suffix", () => {
    expect(getSizeMode("Fit,Px:100")).toBe("fit");
  });
});
