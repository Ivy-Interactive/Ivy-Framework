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

function queryByText(text: string) {
  return Array.from(document.body.querySelectorAll("*")).find((el) => el.textContent === text);
}

function clickConfirmationButton(text: string) {
  const alert = document.body.querySelector('[role="alertdialog"]');
  if (!alert) throw new Error("Confirmation dialog not found");
  const button = Array.from(alert.querySelectorAll("button")).find((b) => b.textContent === text);
  if (!button) throw new Error(`Confirmation button "${text}" not found`);
  act(() => {
    button.dispatchEvent(new MouseEvent("click", { bubbles: true }));
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
  it("fires OnClose immediately when the X button is clicked on a dismissable dialog", () => {
    mount(
      <DialogWidget id="test-dialog" events={["OnClose"]}>
        <DialogHeaderWidget id="header" title="Title" />
      </DialogWidget>,
    );

    clickCloseButton();

    expect(eventHandler).toHaveBeenCalledWith("OnClose", "test-dialog", []);
  });

  it("fires OnClose immediately when non-dismissable but no confirmation text is set", () => {
    mount(
      <DialogWidget id="test-dialog" events={["OnClose"]} dismissable={false}>
        <DialogHeaderWidget id="header" title="Title" />
      </DialogWidget>,
    );

    clickCloseButton();

    expect(eventHandler).toHaveBeenCalledWith("OnClose", "test-dialog", []);
  });

  it("shows a confirmation before closing a non-dismissable dialog with a confirmation message", () => {
    mount(
      <DialogWidget
        id="test-dialog"
        events={["OnClose"]}
        dismissable={false}
        confirmationMessage="Your changes will be lost."
      >
        <DialogHeaderWidget id="header" title="Title" />
      </DialogWidget>,
    );

    clickCloseButton();

    expect(eventHandler).not.toHaveBeenCalled();
    expect(queryByText("Are you sure?")).toBeTruthy();
    expect(queryByText("Your changes will be lost.")).toBeTruthy();
  });

  it("fires OnClose only after confirming the close", () => {
    mount(
      <DialogWidget
        id="test-dialog"
        events={["OnClose"]}
        dismissable={false}
        confirmationMessage="Your changes will be lost."
      >
        <DialogHeaderWidget id="header" title="Title" />
      </DialogWidget>,
    );

    clickCloseButton();
    expect(eventHandler).not.toHaveBeenCalled();

    clickConfirmationButton("Close");

    expect(eventHandler).toHaveBeenCalledWith("OnClose", "test-dialog", []);
  });

  it("keeps the dialog open when the close confirmation is cancelled", () => {
    mount(
      <DialogWidget
        id="test-dialog"
        events={["OnClose"]}
        dismissable={false}
        confirmationMessage="Your changes will be lost."
      >
        <DialogHeaderWidget id="header" title="Title" />
      </DialogWidget>,
    );

    clickCloseButton();

    clickConfirmationButton("Cancel");

    expect(eventHandler).not.toHaveBeenCalled();
  });
});
