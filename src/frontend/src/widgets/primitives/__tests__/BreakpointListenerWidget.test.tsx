import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { act } from "react";
import { createRoot, Root } from "react-dom/client";

// Control the breakpoint the widget observes.
const mockBreakpoint = vi.fn().mockReturnValue("mobile");
vi.mock("@/hooks/use-responsive", () => ({
  useBreakpoint: () => mockBreakpoint(),
}));

// Capture events the widget sends back to the server.
const sentEvents: Array<{ name: string; id: string; args: unknown[] }> = [];
vi.mock("@/components/event-handler/hooks", () => ({
  useEventHandler: () => (name: string, id: string, args: unknown[]) =>
    sentEvents.push({ name, id, args }),
}));

import { BreakpointListenerWidget } from "../BreakpointListenerWidget";

describe("BreakpointListenerWidget", () => {
  let container: HTMLDivElement;
  let root: Root;

  beforeEach(() => {
    sentEvents.length = 0;
    mockBreakpoint.mockReturnValue("mobile");
    container = document.createElement("div");
    document.body.appendChild(container);
    root = createRoot(container);
  });

  afterEach(() => {
    act(() => root.unmount());
    container.remove();
  });

  const render = () =>
    act(() => {
      root.render(<BreakpointListenerWidget id="bp" events={["OnChange"]} />);
    });

  it("renders nothing visible", () => {
    render();
    expect(container.innerHTML).toBe("");
  });

  it("reports the active breakpoint as the PascalCase enum name on mount", () => {
    render();
    expect(sentEvents).toEqual([{ name: "OnChange", id: "bp", args: ["Mobile"] }]);
  });

  it("re-fires when the breakpoint changes", () => {
    render();
    sentEvents.length = 0;
    mockBreakpoint.mockReturnValue("desktop");
    render(); // re-render with the new breakpoint
    expect(sentEvents).toEqual([{ name: "OnChange", id: "bp", args: ["Desktop"] }]);
  });

  it("does not re-fire when the breakpoint is unchanged", () => {
    render();
    sentEvents.length = 0;
    render(); // same breakpoint
    expect(sentEvents).toHaveLength(0);
  });

  it("stays silent when OnChange is not subscribed", () => {
    act(() => {
      root.render(<BreakpointListenerWidget id="bp" events={[]} />);
    });
    expect(sentEvents).toHaveLength(0);
  });
});
