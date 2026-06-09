import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";
import { getCellTooltipPlacementStyle, type CellHoverTooltipState } from "./useCellHoverTooltip";

const layout = { cellGap: 8, viewportInset: 8 };

function tooltipAt(y: number, cellHeight = 32): CellHoverTooltipState {
  return { x: 100, y, cellHeight, content: "Full value" };
}

describe("useCellHoverTooltip", () => {
  const hookSource = fs.readFileSync(path.resolve(__dirname, "./useCellHoverTooltip.ts"), "utf-8");

  it("should only enable tooltip positioning when hover + fine pointer media queries match", () => {
    expect(hookSource).toContain("(hover: hover)");
    expect(hookSource).toContain("(pointer: fine)");
    expect(hookSource).toContain("tooltipSupported");
  });

  it("should detect truncated text and link cells", () => {
    expect(hookSource).toContain("getTruncatedCellTooltip");
    expect(hookSource).toContain('"link-cell"');
  });

  it("should clear tooltip when row is >= visibleRows (filler row)", () => {
    expect(hookSource).toContain("row >= visibleRows");
  });

  it('should clear tooltip when args.kind !== "cell"', () => {
    const kindCheck = hookSource.match(/if\s*\(\s*args\.kind !== "cell"\s*\)/);
    expect(kindCheck).not.toBeNull();
  });

  it("should expose clearCellHoverTooltip", () => {
    expect(hookSource).toContain("clearCellHoverTooltip");
  });

  it("should position tooltip anchor at the center-top of the hovered cell", () => {
    expect(hookSource).toContain("args.bounds.x + args.bounds.width / 2");
    expect(hookSource).toContain("y: args.bounds.y");
    expect(hookSource).toContain("cellHeight: args.bounds.height");
  });

  it("should compute cellTooltipStyle from tooltip state", () => {
    expect(hookSource).toContain("cellTooltipStyle");
    expect(hookSource).toContain("getCellTooltipPlacementStyle");
  });

  it("should use args.bounds.width for truncation detection instead of base column width", () => {
    expect(hookSource).toContain("const columnWidth = args.bounds.width");
    expect(hookSource).not.toContain("getVisibleColumnWidthAt");
  });
});

describe("getCellTooltipPlacementStyle", () => {
  it("places tooltip above when more space is available above the cell", () => {
    const cellTop = 400;
    const cellHeight = 32;
    const style = getCellTooltipPlacementStyle(tooltipAt(cellTop, cellHeight), 800, layout);
    expect(style).toMatchObject({
      left: 100,
      top: cellTop - layout.cellGap,
      transform: "translate(-50%, -100%)",
    });
    expect(style.maxHeight).toBe(cellTop - layout.cellGap - layout.viewportInset);
  });

  it("places tooltip below when more space is available under the cell", () => {
    const cellTop = 40;
    const cellHeight = 32;
    const cellBottom = cellTop + cellHeight;
    const style = getCellTooltipPlacementStyle(tooltipAt(cellTop, cellHeight), 800, layout);
    expect(style).toMatchObject({
      left: 100,
      top: cellBottom + layout.cellGap,
      transform: "translate(-50%, 0)",
    });
    expect(style.maxHeight).toBe(800 - cellBottom - layout.cellGap - layout.viewportInset);
  });

  it("prefers above when space above and below are equal", () => {
    const viewportHeight = 200;
    const cellHeight = 32;
    const cellTop = (viewportHeight - cellHeight) / 2;
    const style = getCellTooltipPlacementStyle(
      tooltipAt(cellTop, cellHeight),
      viewportHeight,
      layout,
    );
    expect(style.transform).toBe("translate(-50%, -100%)");
  });
});
