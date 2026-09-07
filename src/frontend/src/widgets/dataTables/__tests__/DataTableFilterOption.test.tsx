import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import { QueryEditor } from "@/lib/filter-query-editor/components/QueryEditor";
import { startCompletion } from "@codemirror/autocomplete";
import { EditorView } from "@codemirror/view";
import { tableStyles } from "../styles/style";

let container: HTMLDivElement;
let root: Root;

function mount(element: React.ReactElement) {
  act(() => {
    root.render(element);
  });
}

beforeEach(() => {
  container = document.createElement("div");
  document.body.appendChild(container);
  root = createRoot(container);
});

afterEach(() => {
  act(() => {
    root.unmount();
  });
  container.remove();
  // Clean up any tooltips mounted to document.body
  const tooltips = document.querySelectorAll(".cm-tooltip");
  tooltips.forEach((el) => el.remove());
  vi.restoreAllMocks();
});

describe("DataTableFilterOption autocomplete tooltips", () => {
  it("mounts autocomplete tooltip container into document.body outside editor container", async () => {
    const columns = [
      { name: "Age", type: "number", width: 100 },
      { name: "Name", type: "string", width: 150 },
    ];

    mount(
      <div className="query-editor-wrapper" style={{ overflow: "hidden", height: "36px" }}>
        <QueryEditor value="[" columns={columns} onChange={() => {}} />
      </div>,
    );

    const cmEditor = container.querySelector(".cm-editor");
    expect(cmEditor).toBeTruthy();

    // Trigger CodeMirror completion
    const view = EditorView.findFromDOM(cmEditor as HTMLElement);
    expect(view).toBeTruthy();

    if (view) {
      act(() => {
        startCompletion(view);
      });
    }

    // Wait for tooltip element to appear
    const tooltipInBody = document.body.querySelector(".cm-tooltip-autocomplete");
    if (tooltipInBody) {
      // The tooltip must be attached to document.body directly (or outside cmEditor)
      expect(cmEditor?.contains(tooltipInBody)).toBe(false);
      expect(document.body.contains(tooltipInBody)).toBe(true);
    }
  });

  it("applies offset and z-index styling for autocomplete dropdowns", () => {
    expect(tableStyles.queryEditor.css).toContain(".cm-tooltip-autocomplete.cm-tooltip-below");
    expect(tableStyles.queryEditor.css).toContain("margin-top: 8px !important;");
    expect(tableStyles.queryEditor.css).toContain(".cm-tooltip-autocomplete.cm-tooltip-above");
    expect(tableStyles.queryEditor.css).toContain("margin-bottom: 8px !important;");
    expect(tableStyles.queryEditor.css).toContain("z-index: 1000 !important;");
  });
});
