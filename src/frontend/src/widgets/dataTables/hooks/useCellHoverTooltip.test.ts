import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

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
    expect(hookSource).not.toContain("args.bounds.y + args.bounds.height");
  });
});
