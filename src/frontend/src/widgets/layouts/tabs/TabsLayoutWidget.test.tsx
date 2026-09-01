import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import { TabsLayoutWidget } from "./TabsLayoutWidget";
import { TabWidget } from "./TabWidget";
import { EventHandlerProvider } from "@/components/event-handler";

let container: HTMLDivElement;
let root: Root;
const eventHandler = vi.fn();

function mount(element: React.ReactElement) {
  act(() => {
    root.render(<EventHandlerProvider eventHandler={eventHandler}>{element}</EventHandlerProvider>);
  });
}

beforeEach(() => {
  container = document.createElement("div");
  document.body.appendChild(container);
  root = createRoot(container);
  eventHandler.mockClear();
});

afterEach(() => {
  act(() => {
    root.unmount();
  });
  container.remove();
});

describe("TabsLayoutWidget - Content variant", () => {
  it("renders tab buttons with subtle hover styles and not hover:bg-secondary", () => {
    mount(
      <TabsLayoutWidget id="test-tabs" variant="Content" selectedIndex={0} events={["OnSelect"]}>
        <TabWidget id="tab-1" title="Tab 1">
          <div>Content 1</div>
        </TabWidget>
        <TabWidget id="tab-2" title="Tab 2">
          <div>Content 2</div>
        </TabWidget>
      </TabsLayoutWidget>,
    );

    const tabButtons = container.querySelectorAll("button[role='tab']");
    expect(tabButtons.length).toBe(2);

    tabButtons.forEach((tabButton) => {
      const className = tabButton.getAttribute("class") || "";
      expect(className).not.toContain("hover:bg-secondary");
      expect(className).toContain("hover:text-foreground");
      expect(className).toContain("hover:bg-muted/50");
    });
  });

  it("does not render dead static hover highlight element", () => {
    mount(
      <TabsLayoutWidget id="test-tabs" variant="Content" selectedIndex={0} events={[]}>
        <TabWidget id="tab-1" title="Tab 1">
          <div>Content 1</div>
        </TabWidget>
      </TabsLayoutWidget>,
    );

    const highlight = container.querySelector(".bg-accent\\/20");
    expect(highlight).toBeNull();
  });
});
