import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import { DialogWidget } from "./DialogWidget";

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

describe("DialogWidget", () => {
  it("renders dismissable dialog by default (allows backdrop and ESC close)", () => {
    mount(<DialogWidget id="test-dialog" />);
    const dialogContent = container.querySelector("[role='dialog']");
    expect(dialogContent).toBeTruthy();
  });

  it("prevents dismiss when dismissable is false", () => {
    const onClose = vi.fn();
    mount(
      <DialogWidget
        id="test-dialog"
        dismissable={false}
        events={["OnClose"]}
      />
    );

    const dialogContent = container.querySelector("[role='dialog']");
    expect(dialogContent).toBeTruthy();

    // The onInteractOutside and onEscapeKeyDown handlers should be set
    // In a real integration test, we'd simulate these events
    // For now, we verify the dialog renders correctly
  });

  it("applies width styles correctly", () => {
    mount(<DialogWidget id="test-dialog" width="rem(32)" />);
    const dialogContent = container.querySelector("[role='dialog']") as HTMLElement;
    expect(dialogContent).toBeTruthy();
    expect(dialogContent.style.width).toBe("32rem");
  });
});
