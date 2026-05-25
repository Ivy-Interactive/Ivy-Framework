import { describe, expect, it } from "vitest";
import { computeMaxCount, getLevel } from "./levelUtils";
import type { Activity } from "./types";

describe("computeMaxCount", () => {
  it("returns 0 for empty data", () => {
    expect(computeMaxCount([])).toBe(0);
  });

  it("does not return NaN when some counts are missing", () => {
    const data: Activity[] = [
      { date: "2025-01-01", count: 10 },
      { date: "2025-01-02", count: undefined },
      { date: "2025-01-03", count: 5 },
    ];
    const max = computeMaxCount(data);
    expect(Number.isNaN(max)).toBe(false);
    expect(max).toBe(10);
  });

  it("treats null counts as zero", () => {
    const data = [
      { date: "2025-01-01", count: 7 },
      { date: "2025-01-02", count: null },
    ] as Activity[];
    expect(computeMaxCount(data)).toBe(7);
  });
});

describe("getLevel", () => {
  const max = 100;

  it("returns 0 for undefined count", () => {
    expect(getLevel(undefined, max)).toBe(0);
  });

  it("returns 0 for null count", () => {
    expect(getLevel(null, max)).toBe(0);
  });

  it("returns 0 for zero count", () => {
    expect(getLevel(0, max)).toBe(0);
  });

  it("returns 0 when maxCount is 0", () => {
    expect(getLevel(50, 0)).toBe(0);
  });

  it("maps counts to expected levels", () => {
    expect(getLevel(1, max)).toBe(1);
    expect(getLevel(25, max)).toBe(1);
    expect(getLevel(26, max)).toBe(2);
    expect(getLevel(50, max)).toBe(2);
    expect(getLevel(51, max)).toBe(3);
    expect(getLevel(75, max)).toBe(3);
    expect(getLevel(76, max)).toBe(4);
    expect(getLevel(100, max)).toBe(4);
  });

  it("uses a finite maxCount so positive counts are not all level 4", () => {
    const data: Activity[] = [
      { date: "2025-01-01", count: 10 },
      { date: "2025-01-02", count: undefined },
    ];
    const maxCount = computeMaxCount(data);
    expect(getLevel(5, maxCount)).toBe(2);
  });
});
