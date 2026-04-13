import { describe, it, expect } from "vitest";
import { reorderColumns, convertToGridColumns } from "./columnHelpers";
import type { DataColumn } from "../types/types";
import { ColType } from "../types/types";

describe("columnHelpers", () => {
  describe("reorderColumns", () => {
    const mockColumns: DataColumn[] = [
      { name: "First", type: ColType.Text, width: 100 },
      { name: "Second", type: ColType.Number, width: 100 },
      { name: "Third", type: ColType.Boolean, width: 100 },
      { name: "Fourth", type: ColType.Date, width: 100 },
    ];

    it("should move column from start to middle", () => {
      const result = reorderColumns(mockColumns, 0, 2);
      expect(result.map((c) => c.name)).toEqual(["Second", "Third", "First", "Fourth"]);
    });

    it("should move column from middle to start", () => {
      const result = reorderColumns(mockColumns, 2, 0);
      expect(result.map((c) => c.name)).toEqual(["Third", "First", "Second", "Fourth"]);
    });

    it("should move column from middle to end", () => {
      const result = reorderColumns(mockColumns, 1, 3);
      expect(result.map((c) => c.name)).toEqual(["First", "Third", "Fourth", "Second"]);
    });

    it("should not modify original array", () => {
      const original = [...mockColumns];
      reorderColumns(mockColumns, 0, 2);
      expect(mockColumns).toEqual(original);
    });

    it("should handle same start and end index", () => {
      const result = reorderColumns(mockColumns, 1, 1);
      expect(result).toEqual(mockColumns);
    });
  });

  describe("convertToGridColumns", () => {
    const mockColumns: DataColumn[] = [
      { name: "ID", type: ColType.Number, width: 80 },
      { name: "Name", type: ColType.Text, width: 150 },
      { name: "Status", type: ColType.Boolean, width: 100 },
      { name: "Created", type: ColType.Date, width: 120 },
    ];

    it("should convert columns to grid columns without reordering", () => {
      const columnWidths = {};
      const result = convertToGridColumns(mockColumns, [], columnWidths, false);

      expect(result).toEqual([
        { title: "ID", width: 80, group: undefined, icon: "headerNumber" },
        { title: "Name", width: 150, group: undefined, icon: "headerString" },
        {
          title: "Status",
          width: 100,
          group: undefined,
          icon: "headerBoolean",
        },
        { title: "Created", width: 120, grow: 1, group: undefined, icon: "headerDate" },
      ]);
    });

    it("should apply column ordering", () => {
      const columnOrder = [2, 0, 1, 3]; // Status, ID, Name, Created
      const columnWidths = {};
      const result = convertToGridColumns(mockColumns, columnOrder, columnWidths, false);

      expect(result.map((col) => col.title)).toEqual(["Status", "ID", "Name", "Created"]);
    });

    it("should use custom column widths when provided", () => {
      const columnWidths = {
        "0": 100, // ID: 80 -> 100
        "1": 200, // Name: 150 -> 200
      };
      const result = convertToGridColumns(mockColumns, [], columnWidths, false);

      expect("width" in result[0] && result[0].width).toBe(100);
      expect("width" in result[1] && result[1].width).toBe(200);
      expect("width" in result[2] && result[2].width).toBe(100); // unchanged
      expect("width" in result[3] && result[3].width).toBe(120); // unchanged
    });

    it("should make last column fill remaining width with grow", () => {
      const columnWidths = {};
      const result = convertToGridColumns(mockColumns, [], columnWidths, false);

      // Last column uses grow: 1 to fill remaining space (avoids scrollbar gap)
      expect("width" in result[3] && result[3].width).toBe(120);
      expect("grow" in result[3] && result[3].grow).toBe(1);
    });

    it("should always expand last column with grow regardless of container measurement", () => {
      const columnWidths = {};
      const result = convertToGridColumns(mockColumns, [], columnWidths, false);

      expect("width" in result[3] && result[3].width).toBe(120);
      expect("grow" in result[3] && result[3].grow).toBe(1);
    });

    it("should include groups when showGroups is true", () => {
      const columnsWithGroups: DataColumn[] = [
        { name: "ID", type: ColType.Number, width: 80, group: "Identity" },
        { name: "Name", type: ColType.Text, width: 150, group: "Identity" },
        {
          name: "Status",
          type: ColType.Boolean,
          width: 100,
          group: "Metadata",
        },
      ];

      const result = convertToGridColumns(columnsWithGroups, [], {}, true);

      expect(result[0].group).toBe("Identity");
      expect(result[1].group).toBe("Identity");
      expect(result[2].group).toBe("Metadata");
    });

    it("should not include groups when showGroups is false", () => {
      const columnsWithGroups: DataColumn[] = [
        { name: "ID", type: ColType.Number, width: 80, group: "Identity" },
        { name: "Name", type: ColType.Text, width: 150, group: "Identity" },
      ];

      const result = convertToGridColumns(columnsWithGroups, [], {}, false);

      expect(result[0].group).toBeUndefined();
      expect(result[1].group).toBeUndefined();
    });

    it("should handle column ordering with custom widths", () => {
      const columnOrder = [1, 0, 2, 3]; // Name, ID, Status, Created
      const columnWidths = {
        "0": 100, // ID
        "1": 200, // Name
      };

      const result = convertToGridColumns(mockColumns, columnOrder, columnWidths, false);

      // After reordering: Name, ID, Status, Created
      expect(result[0].title).toBe("Name");
      expect("width" in result[0] && result[0].width).toBe(200); // Name has custom width
      expect(result[1].title).toBe("ID");
      expect("width" in result[1] && result[1].width).toBe(100); // ID has custom width
      expect(result[2].title).toBe("Status");
      expect("width" in result[2] && result[2].width).toBe(100); // Status uses original width
    });

    it("should handle empty columns array", () => {
      const result = convertToGridColumns([], [], {}, false);
      expect(result).toEqual([]);
    });

    it("should handle single column", () => {
      const singleColumn: DataColumn[] = [{ name: "Only", type: ColType.Text, width: 100 }];

      const result = convertToGridColumns(singleColumn, [], {}, false);

      // Last (and only) column uses grow: 1 to fill remaining space
      expect("width" in result[0] && result[0].width).toBe(100);
      expect("grow" in result[0] && result[0].grow).toBe(1);
    });

    it("should filter out hidden columns", () => {
      const columns: DataColumn[] = [
        { name: "Visible1", type: ColType.Text, width: 100 },
        { name: "Hidden1", type: ColType.Text, width: 100, hidden: true },
        { name: "Visible2", type: ColType.Number, width: 100 },
        { name: "Hidden2", type: ColType.Boolean, width: 100, hidden: true },
      ];

      const result = convertToGridColumns(columns, [], {}, false);

      expect(result).toHaveLength(2);
      expect(result[0].title).toBe("Visible1");
      expect(result[1].title).toBe("Visible2");
    });

    it("should use custom header when provided", () => {
      const columns: DataColumn[] = [
        {
          name: "col1",
          header: "Custom Header 1",
          type: ColType.Text,
          width: 100,
        },
        { name: "col2", type: ColType.Number, width: 100 },
      ];

      const result = convertToGridColumns(columns, [], {}, false);

      expect(result[0].title).toBe("Custom Header 1");
      expect(result[1].title).toBe("col2");
    });

    it("should apply column order property", () => {
      const columns: DataColumn[] = [
        { name: "Third", type: ColType.Text, width: 100, order: 2 },
        { name: "First", type: ColType.Number, width: 100, order: 0 },
        { name: "Second", type: ColType.Boolean, width: 100, order: 1 },
      ];

      const result = convertToGridColumns(columns, [], {}, false);

      expect(result[0].title).toBe("First");
      expect(result[1].title).toBe("Second");
      expect(result[2].title).toBe("Third");
    });

    it("should handle columns without order property", () => {
      const columns: DataColumn[] = [
        { name: "A", type: ColType.Text, width: 100 },
        { name: "B", type: ColType.Number, width: 100 },
      ];

      const result = convertToGridColumns(columns, [], {}, false);

      expect(result[0].title).toBe("A");
      expect(result[1].title).toBe("B");
    });

    it("should prioritize order property over columnOrder array", () => {
      const columns: DataColumn[] = [
        { name: "C", type: ColType.Text, width: 100, order: 2 },
        { name: "A", type: ColType.Number, width: 100, order: 0 },
        { name: "B", type: ColType.Boolean, width: 100, order: 1 },
      ];

      // columnOrder would suggest B, C, A but order property should win
      const result = convertToGridColumns(columns, [1, 2, 0], {}, false);

      expect(result[0].title).toBe("A");
      expect(result[1].title).toBe("B");
      expect(result[2].title).toBe("C");
    });

    it("should filter hidden columns and apply order", () => {
      const columns: DataColumn[] = [
        { name: "D", type: ColType.Text, width: 100, order: 3, hidden: true },
        { name: "B", type: ColType.Number, width: 100, order: 1 },
        { name: "A", type: ColType.Boolean, width: 100, order: 0 },
        { name: "C", type: ColType.Date, width: 100, order: 2 },
      ];

      const result = convertToGridColumns(columns, [], {}, false);

      expect(result).toHaveLength(3);
      expect(result[0].title).toBe("A");
      expect(result[1].title).toBe("B");
      expect(result[2].title).toBe("C");
    });

    it("should handle partial order values", () => {
      const columns: DataColumn[] = [
        { name: "NoOrder1", type: ColType.Text, width: 100 },
        { name: "First", type: ColType.Number, width: 100, order: 0 },
        { name: "NoOrder2", type: ColType.Boolean, width: 100 },
      ];

      const result = convertToGridColumns(columns, [], {}, false);

      // Columns with order come first, then columns without order
      expect(result[0].title).toBe("First");
      // NoOrder columns should maintain their relative position
      expect(result.map((r) => r.title)).toContain("NoOrder1");
      expect(result.map((r) => r.title)).toContain("NoOrder2");
    });

    it("should apply grow: 0.5 for Fraction:0.5 column", () => {
      const columns: DataColumn[] = [
        { name: "Fixed", type: ColType.Text, width: 200 },
        { name: "Frac", type: ColType.Text, width: 60, originalWidth: "Fraction:0.5" },
      ];
      const result = convertToGridColumns(columns, [], {}, false);
      expect(result[1].grow).toBe(0.5);
    });

    it("should apply grow: 1 for Auto column", () => {
      const columns: DataColumn[] = [
        { name: "Fixed", type: ColType.Text, width: 200 },
        { name: "AutoCol", type: ColType.Text, width: 60, originalWidth: "Auto" },
      ];
      const result = convertToGridColumns(columns, [], {}, false);
      expect(result[1].grow).toBe(1);
    });

    it("should apply grow: 2 for Grow:2 column", () => {
      const columns: DataColumn[] = [
        { name: "Fixed", type: ColType.Text, width: 200 },
        { name: "GrowCol", type: ColType.Text, width: 60, originalWidth: "Grow:2" },
      ];
      const result = convertToGridColumns(columns, [], {}, false);
      expect(result[1].grow).toBe(2);
    });

    it("should apply grow: 1 for Full column", () => {
      const columns: DataColumn[] = [
        { name: "Fixed", type: ColType.Text, width: 200 },
        { name: "FullCol", type: ColType.Text, width: 60, originalWidth: "Full" },
      ];
      const result = convertToGridColumns(columns, [], {}, false);
      expect(result[1].grow).toBe(1);
    });

    it("should apply grow: 1 for Screen column", () => {
      const columns: DataColumn[] = [
        { name: "Fixed", type: ColType.Text, width: 200 },
        { name: "ScreenCol", type: ColType.Text, width: 60, originalWidth: "Screen" },
      ];
      const result = convertToGridColumns(columns, [], {}, false);
      expect(result[1].grow).toBe(1);
    });

    it("should NOT apply grow for Px column", () => {
      const columns: DataColumn[] = [
        { name: "PxCol", type: ColType.Text, width: 200, originalWidth: "Px:200" },
        { name: "Other", type: ColType.Text, width: 100 },
      ];
      const result = convertToGridColumns(columns, [], {}, false);
      expect(result[0].grow).toBeUndefined();
    });

    it("should NOT apply grow for Fit column", () => {
      const columns: DataColumn[] = [
        { name: "FitCol", type: ColType.Text, width: 60, originalWidth: "Fit" },
        { name: "Other", type: ColType.Text, width: 100 },
      ];
      const result = convertToGridColumns(columns, [], {}, false);
      expect(result[0].grow).toBeUndefined();
    });

    it("should NOT apply grow for MinContent column", () => {
      const columns: DataColumn[] = [
        { name: "MinCol", type: ColType.Text, width: 60, originalWidth: "MinContent" },
        { name: "Other", type: ColType.Text, width: 100 },
      ];
      const result = convertToGridColumns(columns, [], {}, false);
      expect(result[0].grow).toBeUndefined();
    });

    it("should apply grow and respect min constraint", () => {
      const columns: DataColumn[] = [
        { name: "Fixed", type: ColType.Text, width: 200 },
        {
          name: "FracMin",
          type: ColType.Text,
          width: 60,
          originalWidth: "Fraction:0.5,Px:100,Px:500",
        },
      ];
      const result = convertToGridColumns(columns, [], {}, false);
      expect(result[1].grow).toBe(0.5);
      expect("width" in result[1] && result[1].width).toBeGreaterThanOrEqual(100);
    });

    it("should apply grow only to fraction/grow columns in a mix", () => {
      const columns: DataColumn[] = [
        { name: "Fixed", type: ColType.Number, width: 200, originalWidth: "Px:200" },
        { name: "Auto", type: ColType.Text, width: 60, originalWidth: "Auto" },
        { name: "Frac", type: ColType.Text, width: 60, originalWidth: "Fraction:0.3" },
      ];
      const result = convertToGridColumns(columns, [], {}, false);
      expect(result[0].grow).toBeUndefined(); // Px: no grow
      expect(result[1].grow).toBe(1); // Auto: grow 1
      expect(result[2].grow).toBe(0.3); // Fraction: grow 0.3
    });

    it("should still default last column to grow: 1 when no column has explicit grow type", () => {
      const columns: DataColumn[] = [
        { name: "A", type: ColType.Text, width: 100 },
        { name: "B", type: ColType.Text, width: 100 },
      ];
      const result = convertToGridColumns(columns, [], {}, false);
      expect(result[0].grow).toBeUndefined();
      expect(result[1].grow).toBe(1);
    });

    it("should assign equal grow to all Fraction:0.25 columns", () => {
      const columns: DataColumn[] = [
        { name: "A", type: ColType.Text, width: 60, originalWidth: "Fraction:0.25" },
        { name: "B", type: ColType.Text, width: 60, originalWidth: "Fraction:0.25" },
        { name: "C", type: ColType.Text, width: 60, originalWidth: "Fraction:0.25" },
        { name: "D", type: ColType.Text, width: 60, originalWidth: "Fraction:0.25" },
      ];
      const result = convertToGridColumns(columns, [], {}, false);
      expect(result[0].grow).toBe(0.25);
      expect(result[1].grow).toBe(0.25);
      expect(result[2].grow).toBe(0.25);
      expect(result[3].grow).toBe(0.25);
    });

    it("should preserve 2:1 ratio for Grow:2 + Grow:1", () => {
      const columns: DataColumn[] = [
        { name: "Wide", type: ColType.Text, width: 60, originalWidth: "Grow:2" },
        { name: "Narrow", type: ColType.Text, width: 60, originalWidth: "Grow:1" },
      ];
      const result = convertToGridColumns(columns, [], {}, false);
      expect(result[0].grow).toBe(2);
      expect(result[1].grow).toBe(1);
    });
  });
});
