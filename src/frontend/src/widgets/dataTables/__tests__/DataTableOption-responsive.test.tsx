import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { DataTableOption } from "../DataTableOption";
import { Filter } from "lucide-react";
import * as BreakpointContext from "@/hooks/use-breakpoint-context";

describe("DataTableOption responsive behavior", () => {
  it("renders as popover on mobile breakpoint", () => {
    vi.spyOn(BreakpointContext, "useCurrentBreakpoint").mockReturnValue("mobile");

    render(
      <DataTableOption icon={Filter} label="Filter" displayMode="inline">
        <div data-testid="filter-content">Filter content</div>
      </DataTableOption>,
    );

    // On mobile, should render as popover (button without inline expansion)
    const button = screen.getByRole("button", { name: /filter/i });
    expect(button).toBeInTheDocument();

    // The popover content should not be visible initially (Radix Popover behavior)
    expect(screen.queryByTestId("filter-content")).not.toBeInTheDocument();
  });

  it("renders inline with fixed width on desktop breakpoint", () => {
    vi.spyOn(BreakpointContext, "useCurrentBreakpoint").mockReturnValue("desktop");

    const { container } = render(
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
    const button = screen.getByRole("button", { name: /filter/i });
    expect(button).toBeInTheDocument();

    // Content should be visible
    expect(screen.getByTestId("filter-content")).toBeInTheDocument();

    // Check that the expansion container uses desktop width (w-[450px])
    const expansionContainer = container.querySelector(".w-\\[450px\\]");
    expect(expansionContainer).toBeInTheDocument();
  });

  it("renders inline with constrained width on tablet breakpoint", () => {
    vi.spyOn(BreakpointContext, "useCurrentBreakpoint").mockReturnValue("tablet");

    const { container } = render(
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
    const button = screen.getByRole("button", { name: /filter/i });
    expect(button).toBeInTheDocument();

    // Content should be visible
    expect(screen.getByTestId("filter-content")).toBeInTheDocument();

    // Check that the expansion container uses responsive width (max-w-[450px] w-[calc(100vw-8rem)])
    const expansionContainer = container.querySelector(".max-w-\\[450px\\]");
    expect(expansionContainer).toBeInTheDocument();

    // Verify the calc width is also present
    const calcWidthContainer = container.querySelector('[class*="w-\\[calc"]');
    expect(calcWidthContainer).toBeInTheDocument();
  });

  it("respects explicit popover displayMode on desktop", () => {
    vi.spyOn(BreakpointContext, "useCurrentBreakpoint").mockReturnValue("desktop");

    render(
      <DataTableOption icon={Filter} label="Filter" displayMode="popover">
        <div data-testid="filter-content">Filter content</div>
      </DataTableOption>,
    );

    // Even on desktop, if displayMode is explicitly "popover", it should render as popover
    const button = screen.getByRole("button", { name: /filter/i });
    expect(button).toBeInTheDocument();

    // The popover content should not be visible initially
    expect(screen.queryByTestId("filter-content")).not.toBeInTheDocument();
  });
});
