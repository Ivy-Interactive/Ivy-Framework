import { describe, it, expect } from "vitest";
import { tableStyles } from "./style";

describe("tableStyles.table.container", () => {
  it("should include height 100% for constrained parents", () => {
    expect(tableStyles.table.container.height).toBe("100%");
  });

  it("should include flex: 1 for flex parents", () => {
    expect(tableStyles.table.container.flex).toBe(1);
  });

  it("should be a flex column container", () => {
    expect(tableStyles.table.container.display).toBe("flex");
    expect(tableStyles.table.container.flexDirection).toBe("column");
  });
});
