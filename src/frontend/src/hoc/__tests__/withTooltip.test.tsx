import { describe, it, expect, beforeEach, afterEach } from "vitest";
import React, { act, createRef } from "react";
import { createRoot, Root } from "react-dom/client";
import withTooltip from "../withTooltip";

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

const TestButton = React.forwardRef<
  HTMLButtonElement,
  React.JSX.IntrinsicAttributes & { label?: string }
>(({ label, ...props }, ref) => (
  <button ref={ref} {...props}>
    {label ?? "click"}
  </button>
));
TestButton.displayName = "TestButton";

const WrappedButton = withTooltip(TestButton);

describe("withTooltip forwardRef", () => {
  it("forwards ref to the underlying DOM element", () => {
    const ref = createRef<HTMLButtonElement>();
    mount(<WrappedButton ref={ref} label="hello" />);
    expect(ref.current).toBeInstanceOf(HTMLButtonElement);
    expect(ref.current!.textContent).toBe("hello");
  });

  it("forwards ref even when tooltipText is provided", () => {
    const ref = createRef<HTMLButtonElement>();
    mount(<WrappedButton ref={ref} tooltipText="my tip" label="btn" />);
    expect(ref.current).toBeInstanceOf(HTMLButtonElement);
    expect(ref.current!.textContent).toBe("btn");
  });

  it("renders without tooltip when tooltipText is not provided", () => {
    mount(<WrappedButton label="plain" />);
    expect(container.querySelector("button")!.textContent).toBe("plain");
  });

  it("sets displayName correctly", () => {
    expect(WrappedButton.displayName).toBe("withTooltip(TestButton)");
  });

  it("falls back to Component name for displayName", () => {
    function MyWidget() {
      return <span />;
    }
    const Wrapped = withTooltip(MyWidget);
    expect(Wrapped.displayName).toBe("withTooltip(MyWidget)");
  });
});
