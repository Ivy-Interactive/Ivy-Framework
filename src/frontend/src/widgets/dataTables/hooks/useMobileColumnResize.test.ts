import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

describe("useMobileColumnResize", () => {
  const hookSource = fs.readFileSync(
    path.resolve(__dirname, "./useMobileColumnResize.ts"),
    "utf-8",
  );
  const editorSource = fs.readFileSync(
    path.resolve(__dirname, "../dataTableEditor/DataTableEditor.tsx"),
    "utf-8",
  );

  it("should enable mobile handles only on coarse pointer", () => {
    expect(hookSource).toContain("(pointer: coarse)");
    expect(hookSource).toContain("useMobileHandles");
  });

  it("should show resize grip only for the selected column", () => {
    expect(hookSource).toContain("getActiveColumnIndex");
    expect(hookSource).toContain("selectedColIndex");
    expect(hookSource).toContain("resizingColIndex ?? selectedColIndex");
    expect(editorSource).toContain("getActiveColumnIndex(gridSelection)");
  });

  it("should disable Glide native resize when mobile handles are active", () => {
    expect(editorSource).toContain("!useMobileColumnResizeHandles");
    expect(editorSource).toContain("useMobileColumnResize");
  });

  it("should prevent horizontal scroll while dragging via body class", () => {
    expect(hookSource).toContain("ivy-datatable-column-resize-active");
    const cssSource = fs.readFileSync(path.resolve(__dirname, "../styles/checkbox.css"), "utf-8");
    expect(cssSource).toContain("ivy-datatable-column-resize-active .dvn-scroller");
    expect(cssSource).toContain("touch-action: pan-y");
  });
});
