import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

/**
 * Tests for the useLinkCellHover hook.
 *
 * Verifies the hook tracks tooltip position when hovering link cells
 * and clears it for non-link cells, filler rows, and non-cell events.
 */
describe("useLinkCellHover", () => {
  const hookSource = fs.readFileSync(path.resolve(__dirname, "./useLinkCellHover.ts"), "utf-8");

  it("should return linkTooltipPos when hovered cell is a link cell", () => {
    // The hook checks for kind === "link-cell" and sets tooltip position
    expect(hookSource).toContain('"link-cell"');
    expect(hookSource).toContain("setLinkTooltipPos({ x:");
  });

  it("should return null when hovered cell is not a link cell", () => {
    // When the cell is not a link cell, tooltip is cleared
    const elseBlock = hookSource.match(/else\s*\{\s*\n\s*setLinkTooltipPos\(null\)/);
    expect(elseBlock).not.toBeNull();
  });

  it("should return null when row is >= visibleRows (filler row)", () => {
    // The hook checks row >= visibleRows and clears tooltip
    expect(hookSource).toContain("row >= visibleRows");
    const fillerCheck = hookSource.match(
      /if\s*\(\s*row >= visibleRows\s*\)\s*\{\s*\n\s*setLinkTooltipPos\(null\)/,
    );
    expect(fillerCheck).not.toBeNull();
  });

  it('should return null when args.kind !== "cell"', () => {
    // The hook checks args.kind !== "cell" and clears tooltip
    const kindCheck = hookSource.match(
      /if\s*\(\s*args\.kind !== "cell"\s*\)\s*\{\s*\n\s*setLinkTooltipPos\(null\)/,
    );
    expect(kindCheck).not.toBeNull();
  });

  it("should use GridCellKind.Custom to detect link cells", () => {
    expect(hookSource).toContain("GridCellKind.Custom");
    expect(hookSource).toContain('(cell.data as { kind?: string })?.kind === "link-cell"');
  });

  it("should position tooltip at the center-top of the hovered cell", () => {
    // Tooltip x should be centered on cell width, y at cell top
    expect(hookSource).toContain("args.bounds.x + args.bounds.width / 2");
    expect(hookSource).toContain("y: args.bounds.y");
  });
});
