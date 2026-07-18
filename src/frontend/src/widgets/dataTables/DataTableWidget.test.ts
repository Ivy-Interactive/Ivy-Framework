import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

/**
 * Source-level tests verifying DataTable container styling handles both
 * constrained (explicit pixel height) and unconstrained (height="Full") parents.
 */
describe("DataTableWidget - container style for height modes", () => {
  const source = fs.readFileSync(path.resolve(__dirname, "./DataTableWidget.tsx"), "utf-8");

  // Isolate the `if (height === "Full") { ... }` branch so assertions about the
  // Full-height container don't accidentally match the explicit-height `else` branch.
  const fullBranch = (() => {
    const start = source.indexOf('if (height === "Full")');
    const elseIndex = source.indexOf("} else {", start);
    return start >= 0 && elseIndex > start ? source.slice(start, elseIndex) : "";
  })();

  it('should set display flex on outer container when height is "Full"', () => {
    expect(source).toContain('containerStyle.display = "flex"');
    expect(source).toContain('containerStyle.flexDirection = "column"');
  });

  it("should set flexGrow for Full height mode", () => {
    expect(source).toContain("containerStyle.flexGrow = 1");
  });

  it("should move explicit heights to flex-basis so tables shrink in tabs", () => {
    expect(source).toContain("containerStyle.flexBasis = containerStyle.height");
    expect(source).toContain("delete containerStyle.height");
  });

  it("should compute a density-aware minimum height before page scroll", () => {
    expect(source).toContain("getDataTableMinHeight");
    expect(source).toContain("containerStyle.minHeight = minHeight");
  });

  it('should not clamp the "Full" height branch with a percentage maxHeight', () => {
    // Regression guard for issue #1695 / PR #4485: a percentage max-height
    // collapses the table to its min-height inside the scrolling app host.
    // flexGrow fills the flex parent, so the Full branch must not set maxHeight.
    expect(fullBranch).not.toBe("");
    expect(fullBranch).not.toContain('containerStyle.maxHeight = "100%"');
    // The fill behaviour and responsive lower bound must remain in place.
    expect(fullBranch).toContain("containerStyle.flexGrow = 1");
    expect(fullBranch).toContain("containerStyle.minHeight = minHeight");
  });

  it("should apply getHeight for explicit pixel heights", () => {
    expect(source).toContain("...getHeight(height)");
  });
});
