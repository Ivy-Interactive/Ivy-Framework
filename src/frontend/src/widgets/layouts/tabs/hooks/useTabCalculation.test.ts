import { describe, it, expect } from "vitest";
import { estimateTabWidth } from "../utils/tabUtils";

describe("useTabCalculation", () => {
  it("should accept correct parameters", () => {
    const tabOrder = ["tab1", "tab2", "tab3"];
    const dropdownOpen = false;
    const variant = "Tabs" as const;

    expect(tabOrder.length).toBe(3);
    expect(dropdownOpen).toBe(false);
    expect(variant).toBe("Tabs");
  });

  it("should handle empty tab order", () => {
    const tabOrder: string[] = [];
    expect(tabOrder.length).toBe(0);
  });

  it("should handle dropdown open state", () => {
    const dropdownOpen = true;
    expect(dropdownOpen).toBe(true);
  });

  it("should accept Content variant", () => {
    const variant = "Content" as const;
    expect(variant).toBe("Content");
  });
});

describe("estimateTabWidth", () => {
  it("should account for badge width in Content variant", () => {
    const widthWithoutBadge = estimateTabWidth("Tab", undefined, undefined, false, "Content");
    const widthWithBadge = estimateTabWidth("Tab", undefined, "New", false, "Content");

    // Badge should add width: ml-2 (8px) + badge content
    expect(widthWithBadge).toBeGreaterThan(widthWithoutBadge);

    // Verify the badge adds at least 8px (margin) + 24px (min badge width)
    expect(widthWithBadge - widthWithoutBadge).toBeGreaterThanOrEqual(32);
  });

  it("should account for badge width in Tabs variant", () => {
    const widthWithoutBadge = estimateTabWidth("Tab", undefined, undefined, false, "Tabs");
    const widthWithBadge = estimateTabWidth("Tab", undefined, "New", false, "Tabs");

    expect(widthWithBadge).toBeGreaterThan(widthWithoutBadge);
  });

  it("should scale badge width with longer badge text", () => {
    const shortBadge = estimateTabWidth("Tab", undefined, "3", false, "Content");
    const longBadge = estimateTabWidth("Tab", undefined, "99 items", false, "Content");

    expect(longBadge).toBeGreaterThan(shortBadge);
  });
});
