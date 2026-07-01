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

  it("shows a confirmation before closing a non-dismissable dialog with confirmation text", () => {
    mount(
      <DialogWidget
        id="test-dialog"
        events={["OnClose"]}
        dismissable={false}
        closeConfirmationTitle="Discard changes?"
        closeConfirmationDescription="Your changes will be lost."
        closeConfirmationButton="Discard"
        closeConfirmationCancelButton="Keep editing"
      >
        <DialogHeaderWidget id="header" title="Title" />
      </DialogWidget>,
    );

    clickCloseButton();

    expect(eventHandler).not.toHaveBeenCalled();
    expect(queryByText("Discard changes?")).toBeTruthy();
    expect(queryByText("Your changes will be lost.")).toBeTruthy();
  });

  it("fires OnClose only after confirming the close", () => {
    mount(
      <DialogWidget
        id="test-dialog"
        events={["OnClose"]}
        dismissable={false}
        closeConfirmationTitle="Discard changes?"
        closeConfirmationButton="Discard"
      >
        <DialogHeaderWidget id="header" title="Title" />
      </DialogWidget>,
    );

    clickCloseButton();
    expect(eventHandler).not.toHaveBeenCalled();

    const confirmButton = queryByText("Discard") as HTMLElement | undefined;
    expect(confirmButton).toBeTruthy();
    act(() => {
      confirmButton!.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });

    expect(eventHandler).toHaveBeenCalledWith("OnClose", "test-dialog", []);
  });

  it("keeps the dialog open when the close confirmation is cancelled", () => {
    mount(
      <DialogWidget
        id="test-dialog"
        events={["OnClose"]}
        dismissable={false}
        closeConfirmationTitle="Discard changes?"
        closeConfirmationCancelButton="Keep editing"
      >
        <DialogHeaderWidget id="header" title="Title" />
      </DialogWidget>,
    );

    clickCloseButton();

    const cancelButton = queryByText("Keep editing") as HTMLElement | undefined;
    expect(cancelButton).toBeTruthy();
    act(() => {
      cancelButton!.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });

    expect(eventHandler).not.toHaveBeenCalled();
  });
});
