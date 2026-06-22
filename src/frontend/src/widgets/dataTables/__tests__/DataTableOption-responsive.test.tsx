import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import { DataTableOption } from "../DataTableOption";
import { Filter } from "lucide-react";
import * as BreakpointContext from "@/hooks/use-breakpoint-context";

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
  vi.restoreAllMocks();
});

describe("DataTableOption responsive behavior", () => {
  it("renders inline (not popover) and fills available width on mobile breakpoint", () => {
    vi.spyOn(BreakpointContext, "useCurrentBreakpoint").mockReturnValue("mobile");

    mount(
      <DataTableOption
        icon={Filter}
        label="Filter"
        displayMode="inline"
        inlineDirection="right"
        defaultExpanded={true}
      >
        <div data-testid="filter-content">Filter content</div>
      </DataTableOption>,
    );

    const button = container.querySelector("button");
    expect(button).toBeTruthy();
    expect(button?.textContent).toContain("Filter");

    // Behavior stays inline on mobile — the content is rendered inline (not
    // hidden behind a popover trigger).
    const filterContent = container.querySelector('[data-testid="filter-content"]');
    expect(filterContent).toBeTruthy();

    // On compact screens the container goes full-width and the expanded panel
    // flexes to fill the remaining space rather than using a fixed 450px width.
    const containerEl = container.querySelector(".w-full");
    expect(containerEl).toBeTruthy();
    const flexPanel = container.querySelector(".flex-1");
    expect(flexPanel).toBeTruthy();
    // No fixed desktop width on mobile.
    expect(container.querySelector(".w-\\[450px\\]")).toBeFalsy();
  });

  it("renders inline with fixed width on desktop breakpoint", () => {
    vi.spyOn(BreakpointContext, "useCurrentBreakpoint").mockReturnValue("desktop");

    mount(
      <DataTableOption
        icon={Filter}
        label="Filter"
        displayMode="inline"
        inlineDirection="right"
        defaultExpanded={true}
      >
        <div data-testid="filter-content">Filter content</div>
      </DataTableOption>,
    );

    // On desktop with expanded state, should render inline expansion
    const button = container.querySelector("button");
    expect(button).toBeTruthy();

    // Content should be visible
    const filterContent = container.querySelector('[data-testid="filter-content"]');
    expect(filterContent).toBeTruthy();

    // Check that the expansion container uses desktop width (w-[450px])
    const expansionContainer = container.querySelector(".w-\\[450px\\]");
    expect(expansionContainer).toBeTruthy();
  });

  it("renders inline and fills available width on tablet breakpoint", () => {
    vi.spyOn(BreakpointContext, "useCurrentBreakpoint").mockReturnValue("tablet");

    mount(
      <DataTableOption
        icon={Filter}
        label="Filter"
        displayMode="inline"
        inlineDirection="right"
        defaultExpanded={true}
      >
        <div data-testid="filter-content">Filter content</div>
      </DataTableOption>,
    );

    // On tablet with expanded state, should render inline expansion
    const button = container.querySelector("button");
    expect(button).toBeTruthy();

    // Content should be visible
    const filterContent = container.querySelector('[data-testid="filter-content"]');
    expect(filterContent).toBeTruthy();

    // Tablet is also treated as compact: full-width container + flexing panel,
    // no fixed desktop width.
    expect(container.querySelector(".w-full")).toBeTruthy();
    expect(container.querySelector(".flex-1")).toBeTruthy();
    expect(container.querySelector(".w-\\[450px\\]")).toBeFalsy();
  });

  it("respects explicit popover displayMode on desktop", () => {
    vi.spyOn(BreakpointContext, "useCurrentBreakpoint").mockReturnValue("desktop");

    mount(
      <DataTableOption icon={Filter} label="Filter" displayMode="popover">
        <div data-testid="filter-content">Filter content</div>
      </DataTableOption>,
    );

    // Even on desktop, if displayMode is explicitly "popover", it should render as popover
    const button = container.querySelector("button");
    expect(button).toBeTruthy();

    // The popover content should not be visible initially
    const filterContent = container.querySelector('[data-testid="filter-content"]');
    expect(filterContent).toBeFalsy();
  });
});
