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
 * The key behavior: link-cell clicks should always open links, regardless
 * of whether enableCellClickEvents is true or false.
 */
describe("useCellInteractions - link cell click behavior", () => {
  const hookSource = fs.readFileSync(path.resolve(__dirname, "./useCellInteractions.ts"), "utf-8");

  it("should open link cells without checking enableCellClickEvents", () => {
    // The link-cell handling block should NOT be gated behind enableCellClickEvents.
    // Previously, the condition included `!(enableCellClickEvents ?? false)` which
    // prevented links from opening when OnCellClick events were enabled.

    // Find the link-cell condition block
    const linkCellConditionMatch = hookSource.match(
      /if\s*\(\s*\n?\s*cellContent\.kind === GridCellKind\.Custom &&\s*\n?\s*\(cellContent\.data as \{ kind\?: string \}\)\?\.kind === "link-cell"/,
    );
    expect(linkCellConditionMatch).not.toBeNull();

    // Extract the full if-condition (from 'if (' to the closing ')')
    const conditionStart = linkCellConditionMatch!.index!;
    const conditionBlock = hookSource.slice(conditionStart, conditionStart + 200);

    // Verify enableCellClickEvents is NOT part of the link-cell condition
    expect(conditionBlock).not.toContain("enableCellClickEvents");
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
