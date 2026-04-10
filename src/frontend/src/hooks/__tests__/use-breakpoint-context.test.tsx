import { describe, it, expect, beforeEach, afterEach } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import { BreakpointProvider, useCurrentBreakpoint } from "../use-breakpoint-context";

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

function BreakpointDisplay() {
  const bp = useCurrentBreakpoint();
  return <span data-testid="bp">{bp}</span>;
}

describe("BreakpointProvider", () => {
  it("renders children with a valid breakpoint value", () => {
    mount(
      <BreakpointProvider>
        <BreakpointDisplay />
      </BreakpointProvider>,
    );
    const text = container.querySelector('[data-testid="bp"]')!.textContent;
    expect(["mobile", "tablet", "desktop", "wide"]).toContain(text);
  });

  it("useCurrentBreakpoint returns breakpoint from context", () => {
    mount(
      <BreakpointProvider>
        <BreakpointDisplay />
      </BreakpointProvider>,
    );
    const text = container.querySelector('[data-testid="bp"]')!.textContent;
    expect(text).toBeTruthy();
    expect(typeof text).toBe("string");
  });
});
