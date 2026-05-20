import { describe, expect, it, vi, afterEach } from "vitest";
import { GridCellKind } from "@glideapps/glide-data-grid";
import { getCellDisplayLabel, getTruncatedCellTooltip } from "./cellTooltip";
import { getCellFont } from "./canvasText";

function mockCanvasMeasure(measure: (text: string) => number) {
  const mockMeasureText = vi.fn((text: string) => ({ width: measure(text) }));
  vi.spyOn(document, "createElement").mockReturnValue({
    getContext: vi.fn().mockReturnValue({
      font: "",
      measureText: mockMeasureText,
    }),
  } as unknown as HTMLCanvasElement);
}

describe("getTruncatedCellTooltip", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const font = getCellFont();

  it("returns full text when display would be ellipsized", () => {
    mockCanvasMeasure((text) => text.length * 8);
    const cell = {
      kind: GridCellKind.Text as const,
      data: "A very long value that will not fit",
      displayData: "A very lon\u2026",
      allowOverlay: false,
    };
    expect(getTruncatedCellTooltip(cell, 80, 8, font)).toBe("A very long value that will not fit");
  });

  it("returns null when text fits", () => {
    mockCanvasMeasure((text) => text.length * 8);
    const cell = {
      kind: GridCellKind.Text as const,
      data: "Short",
      displayData: "Short",
      allowOverlay: false,
    };
    expect(getTruncatedCellTooltip(cell, 200, 8, font)).toBeNull();
  });

  it("returns null for wrapText columns", () => {
    mockCanvasMeasure((text) => text.length * 8);
    const cell = {
      kind: GridCellKind.Text as const,
      data: "Long text",
      displayData: "Long text",
      allowOverlay: false,
    };
    expect(getTruncatedCellTooltip(cell, 50, 8, font, true)).toBeNull();
  });
});

describe("getCellDisplayLabel", () => {
  it("reads link cell display text", () => {
    const label = getCellDisplayLabel({
      kind: GridCellKind.Custom,
      data: { kind: "link-cell", url: "https://example.com", text: "Example" },
      copyData: "Example",
      allowOverlay: false,
    });
    expect(label).toBe("Example");
  });
});
