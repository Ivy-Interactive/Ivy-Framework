import { describe, it, expect } from "vitest";
import * as arrow from "apache-arrow";
import { getHiddenKeyValue } from "./arrowUtils";

function makeTable(fields: { name: string; values: (string | number | null)[] }[]): arrow.Table {
  const columns: Record<string, (string | number | null)[]> = {};
  for (const f of fields) {
    columns[f.name] = f.values;
  }
  return arrow.tableFromArrays(columns);
}

describe("getHiddenKeyValue", () => {
  it("returns null when table is null", () => {
    expect(getHiddenKeyValue(null, 0)).toBeNull();
  });

  it("returns null when _hiddenKey column does not exist", () => {
    const table = makeTable([{ name: "other", values: ["a", "b"] }]);
    expect(getHiddenKeyValue(table, 0)).toBeNull();
  });

  it("returns null when value is null", () => {
    const table = makeTable([{ name: "_hiddenKey", values: [null] }]);
    expect(getHiddenKeyValue(table, 0)).toBeNull();
  });

  it("returns null when value is empty string", () => {
    const table = makeTable([{ name: "_hiddenKey", values: [""] }]);
    expect(getHiddenKeyValue(table, 0)).toBeNull();
  });

  it("returns stringified value when _hiddenKey has a string value", () => {
    const table = makeTable([{ name: "_hiddenKey", values: ["row-1", "row-2"] }]);
    expect(getHiddenKeyValue(table, 0)).toBe("row-1");
    expect(getHiddenKeyValue(table, 1)).toBe("row-2");
  });

  it("handles numeric row IDs correctly", () => {
    const table = makeTable([{ name: "_hiddenKey", values: [42, 99] }]);
    expect(getHiddenKeyValue(table, 0)).toBe("42");
    expect(getHiddenKeyValue(table, 1)).toBe("99");
  });

  it("works when _hiddenKey is not the first column", () => {
    const table = makeTable([
      { name: "col1", values: ["a", "b"] },
      { name: "col2", values: [1, 2] },
      { name: "_hiddenKey", values: ["key-a", "key-b"] },
    ]);
    expect(getHiddenKeyValue(table, 0)).toBe("key-a");
    expect(getHiddenKeyValue(table, 1)).toBe("key-b");
  });
});
