import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import type { WidgetNode } from "@/types/widgets";

// Mock useCurrentBreakpoint to control breakpoint in tests
const mockBreakpoint = vi.fn().mockReturnValue("desktop");
vi.mock("@/hooks/use-breakpoint-context", () => ({
  useCurrentBreakpoint: () => mockBreakpoint(),
}));

// Mock widgetMap with inline component definition (vi.mock is hoisted)
vi.mock("@/widgets/widgetMap", () => {
  const React = require("react");
  const InlineTestWidget: React.FC<Record<string, unknown>> = (props) => {
    return React.createElement(
      "div",
      {
        "data-testid": "test-widget",
        "data-width": props.width,
        "data-height": props.height,
        "data-density": props.density,
      },
      "test",
    );
  };
  return {
    widgetMap: {
      "Ivy.TestWidget": InlineTestWidget,
    },
  };
});

// Mock external widget loader
vi.mock("@/widgets/externalWidgetLoader", () => ({
  isExternalWidget: () => false,
  createLazyExternalWidget: () => null,
}));

import { MemoizedWidget } from "../widgetRenderer";

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
  mockBreakpoint.mockReturnValue("desktop");
});

afterEach(() => {
  act(() => {
    root.unmount();
  });
  container.remove();
});

function makeNode(propsOverride: Record<string, unknown> = {}): WidgetNode {
  return {
    type: "Ivy.TestWidget",
    id: "test-1",
    props: { ...propsOverride },
    events: [],
  };
}

describe("MemoizedWidget responsive props", () => {
  it("hides widget when responsiveVisible resolves to false", () => {
    mockBreakpoint.mockReturnValue("mobile");
    const node = makeNode({
      responsiveVisible: { default: true, mobile: false },
    });
    mount(<MemoizedWidget node={node} />);
    expect(container.querySelector('[data-testid="test-widget"]')).toBeNull();
  });

  it("shows widget when responsiveVisible resolves to true", () => {
    mockBreakpoint.mockReturnValue("desktop");
    const node = makeNode({
      responsiveVisible: { mobile: false, desktop: true },
    });
    mount(<MemoizedWidget node={node} />);
    expect(container.querySelector('[data-testid="test-widget"]')).not.toBeNull();
  });

  it("overrides width from responsiveWidth", () => {
    mockBreakpoint.mockReturnValue("mobile");
    const node = makeNode({
      width: "50%",
      responsiveWidth: { default: "50%", mobile: "100%" },
    });
    mount(<MemoizedWidget node={node} />);
    const el = container.querySelector('[data-testid="test-widget"]');
    expect(el?.getAttribute("data-width")).toBe("100%");
  });

  it("overrides height from responsiveHeight", () => {
    mockBreakpoint.mockReturnValue("tablet");
    const node = makeNode({
      height: "200px",
      responsiveHeight: { default: "200px", tablet: "100px" },
    });
    mount(<MemoizedWidget node={node} />);
    const el = container.querySelector('[data-testid="test-widget"]');
    expect(el?.getAttribute("data-height")).toBe("100px");
  });

  it("overrides density from responsiveDensity", () => {
    mockBreakpoint.mockReturnValue("mobile");
    const node = makeNode({
      responsiveDensity: { default: "Normal", mobile: "Compact" },
    });
    mount(<MemoizedWidget node={node} />);
    const el = container.querySelector('[data-testid="test-widget"]');
    expect(el?.getAttribute("data-density")).toBe("Compact");
  });

  it("passes through props unchanged when no responsive props present", () => {
    const node = makeNode({
      width: "50%",
      height: "200px",
    });
    mount(<MemoizedWidget node={node} />);
    const el = container.querySelector('[data-testid="test-widget"]');
    expect(el?.getAttribute("data-width")).toBe("50%");
    expect(el?.getAttribute("data-height")).toBe("200px");
  });
});
