import { describe, it, expect } from "vitest";
import {
  computeSelectAllValues,
  computeClearAllValues,
  convertValuesToOriginalType,
} from "../select-utils";

// ---------------------------------------------------------------------------
// computeSelectAllValues
// ---------------------------------------------------------------------------

describe("computeSelectAllValues", () => {
  it("selects all visible options when selection is empty", () => {
    const visible = [{ value: "a" }, { value: "b" }, { value: "c" }];
    const result = computeSelectAllValues([], visible);
    expect(result).toEqual(["a", "b", "c"]);
  });

  it("preserves selections outside visible set", () => {
    const visible = [{ value: "b" }, { value: "c" }];
    const result = computeSelectAllValues(["a"], visible);
    expect(result).toEqual(["a", "b", "c"]);
  });

  it("excludes disabled options", () => {
    const visible = [
      { value: "a", disabled: false },
      { value: "b", disabled: true },
      { value: "c" },
    ];
    const result = computeSelectAllValues([], visible);
    expect(result).toEqual(["a", "c"]);
  });

  it("respects maxSelections cap", () => {
    const visible = [{ value: "a" }, { value: "b" }, { value: "c" }, { value: "d" }];
    const result = computeSelectAllValues([], visible, 2);
    expect(result).toHaveLength(2);
  });

  it("prioritizes outside selections when hitting maxSelections", () => {
    const visible = [{ value: "b" }, { value: "c" }];
    const result = computeSelectAllValues(["a"], visible, 2);
    expect(result).toContain("a");
    expect(result).toHaveLength(2);
  });

  it("deduplicates values", () => {
    const visible = [{ value: "a" }, { value: "b" }];
    const result = computeSelectAllValues(["a"], visible);
    const aCount = result.filter((v) => v.toString() === "a").length;
    expect(aCount).toBe(1);
  });

  it("handles numeric values", () => {
    const visible = [{ value: 1 }, { value: 2 }, { value: 3 }];
    const result = computeSelectAllValues([], visible);
    expect(result).toEqual([1, 2, 3]);
  });

  it("handles null/undefined maxSelections", () => {
    const visible = [{ value: "a" }, { value: "b" }];
    expect(computeSelectAllValues([], visible, null)).toEqual(["a", "b"]);
    expect(computeSelectAllValues([], visible, undefined)).toEqual(["a", "b"]);
  });
});

// ---------------------------------------------------------------------------
// computeClearAllValues
// ---------------------------------------------------------------------------

describe("computeClearAllValues", () => {
  it("returns empty array when minSelections is 0", () => {
    expect(computeClearAllValues(["a", "b", "c"], 0)).toEqual([]);
  });

  it("returns empty array when minSelections is undefined", () => {
    expect(computeClearAllValues(["a", "b", "c"], undefined)).toEqual([]);
  });

  it("returns empty array when minSelections is null", () => {
    expect(computeClearAllValues(["a", "b", "c"], null)).toEqual([]);
  });

  it("keeps first N items when minSelections > 0", () => {
    expect(computeClearAllValues(["a", "b", "c", "d", "e"], 2)).toEqual(["a", "b"]);
  });

  it("keeps all items when minSelections >= selected count", () => {
    expect(computeClearAllValues(["a", "b"], 5)).toEqual(["a", "b"]);
  });

  it("returns empty array for empty selection", () => {
    expect(computeClearAllValues([], 0)).toEqual([]);
  });
});

// ---------------------------------------------------------------------------
// convertValuesToOriginalType
// ---------------------------------------------------------------------------

describe("convertValuesToOriginalType", () => {
  const numberOptions = [
    { value: 1, label: "One" },
    { value: 2, label: "Two" },
    { value: 3, label: "Three" },
  ];

  const stringOptions = [
    { value: "a", label: "A" },
    { value: "b", label: "B" },
    { value: "c", label: "C" },
  ];

  it("returns numbers when original is number array", () => {
    const result = convertValuesToOriginalType(["1", "2"], [1], numberOptions);
    expect(result).toEqual([1, 2]);
  });

  it("returns strings when original is string array", () => {
    const result = convertValuesToOriginalType(["a", "b"], ["a"], stringOptions);
    expect(result).toEqual(["a", "b"]);
  });

  it("returns empty array for empty stringValues with array original", () => {
    const result = convertValuesToOriginalType([], ["a"], stringOptions);
    expect(result).toEqual([]);
  });

  it("returns null for empty stringValues with null original (single select)", () => {
    const result = convertValuesToOriginalType([], null, stringOptions, false);
    expect(result).toBeNull();
  });

  it("returns empty array for empty stringValues with null original (selectMany)", () => {
    const result = convertValuesToOriginalType([], null, stringOptions, true);
    expect(result).toEqual([]);
  });

  it("returns single option value for single select with match", () => {
    const result = convertValuesToOriginalType(["a"], null, stringOptions, false);
    expect(result).toBe("a");
  });

  it("returns option.value type for number options in selectMany with null original", () => {
    const result = convertValuesToOriginalType(["1", "2"], null, numberOptions, true);
    expect(result).toEqual([1, 2]);
  });

  it("returns string values when options map has no match", () => {
    const result = convertValuesToOriginalType(["x"], null, [], false);
    expect(result).toBe("x");
  });

  it("returns empty array for empty stringValues with undefined original (selectMany)", () => {
    const result = convertValuesToOriginalType([], undefined, stringOptions, true);
    expect(result).toEqual([]);
  });
});
