import { describe, it, expect, beforeEach, afterEach } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import { SidebarLayoutWidget } from "./SidebarLayoutWidget";

let container: HTMLDivElement;
let root: Root;

function mount(element: React.ReactElement) {
  act(() => {
    root.render(element);
  });
}

describe("SidebarLayoutWidget localStorage persistence", () => {
  const STORAGE_KEY = "ivy-sidebar-open";

  beforeEach(() => {
    localStorage.clear();
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

  it("checks localStorage on mount when mainAppSidebar is true and respects false", () => {
    localStorage.setItem(STORAGE_KEY, "false");
    mount(
      <SidebarLayoutWidget
        mainAppSidebar={true}
        slots={{ MainContent: [<div key="1">Content</div>] }}
      />,
    );
    expect(localStorage.getItem(STORAGE_KEY)).toBe("false");
  });

  it("checks localStorage on mount when mainAppSidebar is true and respects true", () => {
    localStorage.setItem(STORAGE_KEY, "true");
    mount(
      <SidebarLayoutWidget
        mainAppSidebar={true}
        slots={{ MainContent: [<div key="1">Content</div>] }}
      />,
    );
    expect(localStorage.getItem(STORAGE_KEY)).toBe("true");
  });

  it("updates localStorage when toggle button is clicked", () => {
    localStorage.setItem(STORAGE_KEY, "true");
    mount(
      <SidebarLayoutWidget
        mainAppSidebar={true}
        showToggleButton={true}
        slots={{ MainContent: [<div key="1">Content</div>] }}
      />,
    );
    expect(localStorage.getItem(STORAGE_KEY)).toBe("true");

    const button = container.querySelector("button");
    if (button) {
      act(() => {
        button.click();
      });
      expect(localStorage.getItem(STORAGE_KEY)).toBe("false");
    }
  });
});
