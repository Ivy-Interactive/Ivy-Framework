import { describe, it, expect, vi, beforeAll, beforeEach } from "vitest";
import { GridCellKind } from "@glideapps/glide-data-grid";
import { iconCellRenderer, IconCell } from "./customRenderers";

// Mock HTMLImageElement for Node environment
class MockImage {
  src = "";
  complete = true;
  onload: (() => void) | null = null;
  onerror: (() => void) | null = null;
}

beforeAll(() => {
  global.Image = MockImage as any;
  global.HTMLImageElement = MockImage as any;
});

describe("customRenderers", () => {
  describe("iconCellRenderer", () => {
    describe("isMatch", () => {
      it("should match icon cells", () => {
        const iconCell: IconCell = {
          kind: GridCellKind.Custom,
          allowOverlay: false,
          copyData: "Activity",
          data: {
            kind: "icon-cell",
            iconName: "Activity",
          },
        };

        expect(iconCellRenderer.isMatch(iconCell)).toBe(true);
      });

      it("should not match non-custom cells", () => {
        const textCell = {
          kind: GridCellKind.Text,
          data: "test",
          displayData: "test",
          allowOverlay: false,
          copyData: "test",
        } as any;

        expect(iconCellRenderer.isMatch(textCell)).toBe(false);
      });

      it("should not match custom cells with wrong kind", () => {
        const customCell = {
          kind: GridCellKind.Custom,
          allowOverlay: false,
          copyData: "test",
          data: {
            kind: "other-cell",
            value: "test",
          },
        } as any;

        expect(iconCellRenderer.isMatch(customCell)).toBe(false);
      });
    });

    describe("draw", () => {
      const mockCtx = {
        fillStyle: "",
        font: "",
        fillText: vi.fn(),
        drawImage: vi.fn(),
        beginPath: vi.fn(),
        arc: vi.fn(),
        fill: vi.fn(),
      };

      const mockTheme = {
        textDark: "#000",
        textMedium: "#666",
      };

      const mockRect = {
        x: 0,
        y: 0,
        width: 100,
        height: 40,
      };

      const mockArgs = {
        ctx: mockCtx,
        theme: mockTheme,
        rect: mockRect,
        col: 0,
        row: 0,
      } as any;

      beforeEach(() => {
        vi.clearAllMocks();
      });

      it("should return false when iconName is missing", () => {
        const cell: IconCell = {
          kind: GridCellKind.Custom,
          allowOverlay: false,
          copyData: "",
          data: {
            kind: "icon-cell",
            iconName: "",
          },
        };

        const result = iconCellRenderer.draw(mockArgs, cell);
        expect(result).toBe(false);
      });

      it("should draw error indicator for invalid icon", () => {
        const cell: IconCell = {
          kind: GridCellKind.Custom,
          allowOverlay: false,
          copyData: "InvalidIcon",
          data: {
            kind: "icon-cell",
            iconName: "InvalidIcon",
          },
        };

        const result = iconCellRenderer.draw(mockArgs, cell);

        expect(result).toBe(true);
        expect(mockCtx.fillText).toHaveBeenCalledWith("?", 16, 24);
        expect(mockCtx.fillStyle).toBe("#000");
      });

      it("should draw icon when image is complete", () => {
        const cell: IconCell = {
          kind: GridCellKind.Custom,
          allowOverlay: false,
          copyData: "Activity",
          data: {
            kind: "icon-cell",
            iconName: "Activity",
          },
        };

        const result = iconCellRenderer.draw(mockArgs, cell);

        expect(result).toBe(true);
        expect(mockCtx.drawImage).toHaveBeenCalled();
      });
    });

    describe("onPaste", () => {
      it("should return updated data for valid icon name", () => {
        const data = {
          kind: "icon-cell" as const,
          iconName: "Activity",
        };

        const result = iconCellRenderer.onPaste?.("Archive", data);

        expect(result).toEqual({
          kind: "icon-cell",
          iconName: "Archive",
        });
      });

      it("should return undefined for invalid icon name", () => {
        const data = {
          kind: "icon-cell" as const,
          iconName: "Activity",
        };

        const result = iconCellRenderer.onPaste?.("InvalidIcon", data);

        expect(result).toBeUndefined();
      });

      it("should return undefined for non-string values", () => {
        const data = {
          kind: "icon-cell" as const,
          iconName: "Activity",
        };

        const result = iconCellRenderer.onPaste?.(123 as any, data);

        expect(result).toBeUndefined();
      });
    });

    describe("renderer properties", () => {
      it("should have correct kind", () => {
        expect(iconCellRenderer.kind).toBe(GridCellKind.Custom);
      });

      it("should have isMatch function", () => {
        expect(typeof iconCellRenderer.isMatch).toBe("function");
      });

      it("should have draw function", () => {
        expect(typeof iconCellRenderer.draw).toBe("function");
      });

      it("should have onPaste function", () => {
        expect(typeof iconCellRenderer.onPaste).toBe("function");
      });
    });
  });
});
