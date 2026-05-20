import { describe, expect, it, vi, afterEach } from "vitest";
import { getCellFont, truncateTextWithEllipsis } from "./canvasText";

function mockCanvasMeasure(measure: (text: string) => number) {
  const mockMeasureText = vi.fn((text: string) => ({ width: measure(text) }));
  const mockGetContext = vi.fn().mockReturnValue({
    font: "",
    measureText: mockMeasureText,
  });
  vi.spyOn(document, "createElement").mockReturnValue({
    getContext: mockGetContext,
  } as unknown as HTMLCanvasElement);
  return mockMeasureText;
}

describe("truncateTextWithEllipsis", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  const font = getCellFont();

  it("returns empty string for empty input", () => {
    mockCanvasMeasure(() => 0);
    expect(truncateTextWithEllipsis("", 100, font)).toBe("");
  });

  it("returns original text when it fits", () => {
    mockCanvasMeasure((text) => text.length * 8);
    expect(truncateTextWithEllipsis("Hi", 200, font)).toBe("Hi");
  });

  it("truncates long text with ellipsis", () => {
    mockCanvasMeasure((text) => text.length * 8);
    const long = "This is a very long cell value that should not fit";
    const result = truncateTextWithEllipsis(long, 80, font);
    expect(result.endsWith("\u2026")).toBe(true);
    expect(result.length).toBeLessThan(long.length);
  });

  it("returns ellipsis when max width is tiny", () => {
    mockCanvasMeasure((text) => text.length * 8);
    expect(truncateTextWithEllipsis("Hello", 1, font)).toBe("\u2026");
  });
});
