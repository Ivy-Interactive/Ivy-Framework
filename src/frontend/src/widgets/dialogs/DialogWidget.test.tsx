import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import { DialogWidget } from "./DialogWidget";
import { DialogHeaderWidget } from "./DialogHeaderWidget";
import { EventHandlerProvider } from "@/components/event-handler";

let container: HTMLDivElement;
let root: Root;
const eventHandler = vi.fn();

function mount(element: React.ReactElement) {
  act(() => {
    root.render(<EventHandlerProvider eventHandler={eventHandler}>{element}</EventHandlerProvider>);
  });
}

function clickCloseButton() {
  const closeButton = Array.from(document.body.querySelectorAll("button")).find(
    (b) => b.querySelector(".sr-only")?.textContent === "Close",
  );
  if (!closeButton) throw new Error("Close (X) button not found");
  act(() => {
    closeButton.dispatchEvent(new MouseEvent("click", { bubbles: true }));
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

describe("DialogWidget", () => {
  it("fires OnClose immediately when the X button is clicked", () => {
    mount(
      <DialogWidget id="test-dialog" events={["OnClose"]}>
        <DialogHeaderWidget id="header" title="Title" />
      </DialogWidget>,
    );

    clickCloseButton();

    expect(eventHandler).toHaveBeenCalledWith("OnClose", "test-dialog", []);
  });
});
