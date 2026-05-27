import { describe, it, expect } from "vitest";
import { Densities } from "@/types/density";
import { getDataTableMinHeight } from "./dataTableLayout";
import { DENSITY_CONFIG } from "./dataTableEditor/constants";

describe("getDataTableMinHeight", () => {
  it("should reserve space for filter, headers, and multiple readable rows", () => {
    const { rowHeight, groupHeaderHeight } = DENSITY_CONFIG[Densities.Medium];
    const px = parseInt(
      getDataTableMinHeight({
        density: Densities.Medium,
        hasFilter: true,
        hasGroups: true,
      }),
      10,
    );

    // filter + group header + column header + 3 rows + chrome
    expect(px).toBeGreaterThanOrEqual(44 + groupHeaderHeight + rowHeight * 4 + 16);
  });

  it("should include aggregate footer band when present", () => {
    const withoutFooter = parseInt(
      getDataTableMinHeight({ density: Densities.Medium, hasFilter: true }),
      10,
    );
    const withFooter = parseInt(
      getDataTableMinHeight({
        density: Densities.Medium,
        hasFilter: true,
        hasAggregateFooter: true,
      }),
      10,
    );

    expect(withFooter).toBeGreaterThan(withoutFooter);
  });

  it("should be larger than the previous fixed 10rem floor", () => {
    const px = parseInt(getDataTableMinHeight({ density: Densities.Medium, hasFilter: true }), 10);
    expect(px).toBeGreaterThan(160);
  });
});
