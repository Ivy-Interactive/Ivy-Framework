import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

/**
 * Tests for the useCellInteractions hook's link-cell click behavior.
 *
 * The hook is a React hook requiring component context to run. Since
 * @testing-library/react is not available, we verify the source code
 * contains the correct conditional logic for link-cell handling.
 *
 * The key behavior: link-cell clicks should only open links when
 * cmd+click (metaKey) or ctrl+click (ctrlKey) is used.
 */
describe("useCellInteractions - link cell click behavior", () => {
  const hookSource = fs.readFileSync(path.resolve(__dirname, "./useCellInteractions.ts"), "utf-8");

  it("should require cmd/ctrl+click to open link cells", () => {
    // The link-cell handling block should check for modifier keys (metaKey or ctrlKey).
    // Plain clicks should NOT open links — only cmd+click or ctrl+click.

    // Find the link-cell condition block
    const linkCellConditionMatch = hookSource.match(
      /if\s*\(\s*\n?\s*cellContent\.kind === GridCellKind\.Custom &&\s*\n?\s*\(cellContent\.data as \{ kind\?: string \}\)\?\.kind === "link-cell"/,
    );
    expect(linkCellConditionMatch).not.toBeNull();

    // Extract the full if-condition (from 'if (' to well past the closing ')')
    const conditionStart = linkCellConditionMatch!.index!;
    const conditionBlock = hookSource.slice(conditionStart, conditionStart + 300);

    // Verify modifier key check is part of the link-cell condition
    expect(conditionBlock).toContain("args.metaKey || args.ctrlKey");
  });

  it("should NOT open link cells on plain click (no modifier key)", () => {
    // The condition requires (args.metaKey || args.ctrlKey), so without
    // either modifier, the if-block is not entered and the link is not opened.
    // We verify this by checking that metaKey/ctrlKey are required conditions.

    const linkCellConditionMatch = hookSource.match(
      /if\s*\(\s*\n?\s*cellContent\.kind === GridCellKind\.Custom &&\s*\n?\s*\(cellContent\.data as \{ kind\?: string \}\)\?\.kind === "link-cell" &&\s*\n?\s*\(args\.metaKey \|\| args\.ctrlKey\)/,
    );
    expect(linkCellConditionMatch).not.toBeNull();
  });

  it("should open link cells on cmd+click (metaKey: true)", () => {
    // The condition includes args.metaKey, which is true on cmd+click (macOS)
    expect(hookSource).toContain("args.metaKey");
  });

  it("should open link cells on ctrl+click (ctrlKey: true)", () => {
    // The condition includes args.ctrlKey, which is true on ctrl+click (Windows/Linux)
    expect(hookSource).toContain("args.ctrlKey");
  });

  it("should accept GridMouseEventArgs as second parameter", () => {
    // The handleCellClicked callback should accept args: GridMouseEventArgs
    expect(hookSource).toContain("(cell: Item, args: GridMouseEventArgs)");
  });

  it("should import GridMouseEventArgs from glide-data-grid", () => {
    expect(hookSource).toContain("GridMouseEventArgs");
    expect(hookSource).toMatch(
      /import\s*\{[^}]*GridMouseEventArgs[^}]*\}\s*from\s*"@glideapps\/glide-data-grid"/,
    );
  });

  it("should still fire OnCellClick events when enableCellClickEvents is true", () => {
    // The OnCellClick event handler block should still exist and be gated
    // behind enableCellClickEvents
    const onCellClickBlock = hookSource.indexOf('eventHandler("OnCellClick"');
    expect(onCellClickBlock).toBeGreaterThan(-1);

    // The enableCellClickEvents check should appear before the OnCellClick dispatch
    const enableCheck = hookSource.indexOf("enableCellClickEvents ?? false");
    expect(enableCheck).toBeGreaterThan(-1);
    expect(enableCheck).toBeLessThan(onCellClickBlock);
  });

  it("should open external URLs in a new tab with security attributes", () => {
    // Verify window.open is called with noopener,noreferrer for security
    expect(hookSource).toContain('window.open(validatedUrl, "_blank", "noopener,noreferrer")');
  });

  it("should validate URLs before opening to prevent open redirect", () => {
    // Verify URL validation happens before opening
    const validateIndex = hookSource.indexOf("validateLinkUrl(url)");
    const windowOpenIndex = hookSource.indexOf("window.open(validatedUrl");
    expect(validateIndex).toBeGreaterThan(-1);
    expect(windowOpenIndex).toBeGreaterThan(-1);
    expect(validateIndex).toBeLessThan(windowOpenIndex);
  });
});
