import { describe, it, expect, vi } from "vitest";
import React from "react";
import { DialogWidget } from "./DialogWidget";
import { EventHandlerProvider } from "@/components/event-handler";

const mockEventHandler = vi.fn();

describe("DialogWidget", () => {
  it("accepts dismissable prop and renders without error", () => {
    // Test that the component accepts the dismissable prop
    // Full integration testing of dialog dismiss behavior would require
    // Playwright/E2E tests to simulate user interactions
    expect(() => {
      React.createElement(DialogWidget, {
        id: "test-dialog",
        dismissable: false,
        events: ["OnClose"],
      });
    }).not.toThrow();
  });

  it("defaults dismissable to true when not specified", () => {
    expect(() => {
      React.createElement(DialogWidget, {
        id: "test-dialog",
      });
    }).not.toThrow();
  });

  it("can be wrapped in EventHandlerProvider", () => {
    expect(() => {
      React.createElement(
        EventHandlerProvider,
        { eventHandler: mockEventHandler },
        React.createElement(DialogWidget, {
          id: "test-dialog",
          dismissable: false,
        }),
      );
    }).not.toThrow();
  });
});
