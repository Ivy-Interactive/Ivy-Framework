import { describe, expect, it } from "vitest";
import { formatShortcutForDisplay } from "./shortcut";

describe("formatShortcutForDisplay", () => {
  it("renders Backspace as ⌫", () => {
    const result = formatShortcutForDisplay("Backspace");
    expect(result).toContain("⌫");
  });

  it("renders Delete as ⌦", () => {
    const result = formatShortcutForDisplay("Delete");
    expect(result).toContain("⌦");
  });

  it("renders Enter as ↵", () => {
    const result = formatShortcutForDisplay("Enter");
    expect(result).toContain("↵");
  });

  it("renders Escape as Esc", () => {
    const result = formatShortcutForDisplay("Escape");
    expect(result).toContain("Esc");
  });

  it("renders Ctrl+Backspace with both modifier and ⌫", () => {
    const result = formatShortcutForDisplay("Ctrl+Backspace");
    expect(result).toContain("⌫");
    // Should have 3 parts: modifier, "+", symbol
    expect(result.length).toBe(3);
  });

  it("renders regular key A as A", () => {
    const result = formatShortcutForDisplay("A");
    expect(result).toContain("A");
  });

  it("returns empty array for undefined input", () => {
    const result = formatShortcutForDisplay(undefined);
    expect(result).toEqual([]);
  });

  it("renders Tab as ⇥", () => {
    const result = formatShortcutForDisplay("Tab");
    expect(result).toContain("⇥");
  });

  it("renders arrow keys as symbols", () => {
    expect(formatShortcutForDisplay("ArrowUp")).toContain("↑");
    expect(formatShortcutForDisplay("ArrowDown")).toContain("↓");
    expect(formatShortcutForDisplay("ArrowLeft")).toContain("←");
    expect(formatShortcutForDisplay("ArrowRight")).toContain("→");
  });
});
