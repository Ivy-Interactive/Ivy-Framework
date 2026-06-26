import { describe, it, expect, beforeEach, afterEach } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import { TableWidget } from "./TableWidget";

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
});

describe("TableWidget", () => {
  it("stretches to full width by default", () => {
    mount(<TableWidget id="t" />);
    const table = container.querySelector("table")!;
    expect(table.className).toContain("w-full");
    expect(table.className).not.toContain("w-fit");
  });

  it("shrinks to fit content when width is Fit", () => {
    mount(<TableWidget id="t" width="Fit" />);
    const table = container.querySelector("table")!;
    expect(table.className).toContain("w-fit");
    expect(table.className).not.toContain("w-full");
    const wrapper = container.firstElementChild as HTMLElement;
    expect(wrapper.style.width).toBe("fit-content");
  });

  it("uses auto table-layout by default and fixed when requested", () => {
    mount(<TableWidget id="t" />);
    expect(container.querySelector("table")!.style.tableLayout).toBe("auto");

    mount(<TableWidget id="t" layout="Fixed" />);
    expect(container.querySelector("table")!.style.tableLayout).toBe("fixed");
  });
});
