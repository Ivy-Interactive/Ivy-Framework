import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

/**
 * Source-level tests verifying DataTable container styling handles both
 * constrained (explicit pixel height) and unconstrained (height="Full") parents.
 */
describe("DataTableWidget - container style for height modes", () => {
  const source = fs.readFileSync(path.resolve(__dirname, "./DataTableWidget.tsx"), "utf-8");

  it('should set display flex on outer container when height is "Full"', () => {
    expect(source).toContain('containerStyle.display = "flex"');
    expect(source).toContain('containerStyle.flexDirection = "column"');
  });

  it("should set flexGrow for Full height mode", () => {
    expect(source).toContain("containerStyle.flexGrow = 1");
  });

  it("should cap explicit heights with min(preferred, 100%) so tables shrink in tabs", () => {
    expect(source).toContain("containerStyle.height = `min(${containerStyle.height}, 100%)`");
  });

  it("should compute a density-aware minimum height before page scroll", () => {
    expect(source).toContain("getDataTableMinHeight");
    expect(source).toContain("containerStyle.minHeight = minHeight");
  });

  it("should set maxHeight to 100% so tables shrink with the viewport", () => {
    expect(source).toContain('containerStyle.maxHeight = "100%"');
  });

  it("should apply getHeight for explicit pixel heights", () => {
    expect(source).toContain("...getHeight(height)");
  });
});
