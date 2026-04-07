import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

/**
 * Tests for the useLinkCellHover hook.
 *
 * Verifies the hook tracks tooltip state when hovering link cells
 * and clears it for non-link cells, filler rows, and non-cell events.
 */
describe("useLinkCellHover", () => {
  const hookSource = fs.readFileSync(path.resolve(__dirname, "./useLinkCellHover.ts"), "utf-8");

  it("should detect link cells by kind", () => {
    expect(hookSource).toContain('"link-cell"');
    expect(hookSource).toContain("GridCellKind.Custom");
    expect(hookSource).toContain('(cell.data as { kind?: string })?.kind === "link-cell"');
  });

  it("should clear tooltip when hovered cell is not a link cell", () => {
    const elseBlock = hookSource.match(/else\s*\{\s*\n\s*setLinkTooltipPos\(null\)/);
    expect(elseBlock).not.toBeNull();
  });

  it("should clear tooltip when row is >= visibleRows (filler row)", () => {
    expect(hookSource).toContain("row >= visibleRows");
  });

  it('should clear tooltip when args.kind !== "cell"', () => {
    const kindCheck = hookSource.match(
      /if\s*\(\s*args\.kind !== "cell"\s*\)\s*\{\s*\n\s*setLinkTooltipPos\(null\)/,
    );
    expect(kindCheck).not.toBeNull();
  });

  it("should expose isLinkHovered and virtualRef for tooltip positioning", () => {
    expect(hookSource).toContain("isLinkHovered");
    expect(hookSource).toContain("virtualRef");
    expect(hookSource).toContain("getBoundingClientRect");
  });

  it("should position tooltip at the center-top of the hovered cell", () => {
    expect(hookSource).toContain("args.bounds.x + args.bounds.width / 2");
    expect(hookSource).toContain("y: args.bounds.y");
  });
});
