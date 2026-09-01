import { describe, it, expect, beforeEach, afterEach } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import { Dialog, DialogContent, DialogHeader } from "./dialog";
import { Sheet, SheetContent } from "./sheet";
import { NumberRangeInputWidget } from "@/widgets/inputs/NumberRangeInputWidget";
import { EventHandlerProvider } from "@/components/event-handler";

let container: HTMLDivElement;
let root: Root;

function mount(element: React.ReactElement) {
  act(() => {
    root.render(<EventHandlerProvider eventHandler={() => {}}>{element}</EventHandlerProvider>);
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

describe("Close and Clear Button Hover Styles", () => {
  describe("DialogHeader close button", () => {
    it("renders close button with standardized hover colors and inheriting icon", () => {
      mount(
        <Dialog open={true}>
          <DialogContent>
            <DialogHeader>
              <span>Title</span>
            </DialogHeader>
          </DialogContent>
        </Dialog>,
      );

      const closeButton = Array.from(document.body.querySelectorAll("button")).find(
        (b) => b.querySelector(".sr-only")?.textContent === "Close",
      );
      expect(closeButton).toBeDefined();

      const btnClasses = closeButton?.className ?? "";
      expect(btnClasses).toContain("text-muted-foreground");
      expect(btnClasses).toContain("hover:bg-accent");
      expect(btnClasses).toContain("hover:text-accent-foreground");
      expect(btnClasses).toContain("transition-colors");
      expect(btnClasses).toContain("rounded-selector");

      const icon = closeButton?.querySelector("svg");
      expect(icon).not.toBeNull();
      const iconClasses = icon?.getAttribute("class") ?? "";
      expect(iconClasses).toContain("size-4");
      expect(iconClasses).not.toContain("text-muted-foreground");
      expect(iconClasses).not.toContain("hover:text-foreground");
    });
  });

  describe("SheetContent close button", () => {
    it("renders close button with standardized hover colors and inheriting icon", () => {
      mount(
        <Sheet open={true}>
          <SheetContent>
            <div>Content</div>
          </SheetContent>
        </Sheet>,
      );

      const closeButton = Array.from(document.body.querySelectorAll("button")).find(
        (b) => b.querySelector(".sr-only")?.textContent === "Close",
      );
      expect(closeButton).toBeDefined();

      const btnClasses = closeButton?.className ?? "";
      expect(btnClasses).toContain("text-muted-foreground");
      expect(btnClasses).toContain("hover:bg-accent");
      expect(btnClasses).toContain("hover:text-accent-foreground");
      expect(btnClasses).toContain("transition-colors");
      expect(btnClasses).toContain("rounded-selector");

      const icon = closeButton?.querySelector("svg");
      expect(icon).not.toBeNull();
      const iconClasses = icon?.getAttribute("class") ?? "";
      expect(iconClasses).toContain("size-4");
      expect(iconClasses).not.toContain("text-muted-foreground");
      expect(iconClasses).not.toContain("hover:text-foreground");
    });
  });

  describe("NumberRangeInputWidget clear button", () => {
    it("renders clear button with standardized hover colors and inheriting icon", () => {
      mount(
        <NumberRangeInputWidget
          id="num-range"
          lowerValue={10}
          upperValue={20}
          nullable={true}
          events={["OnChange"]}
        />,
      );

      const clearButton = container.querySelector('button[aria-label="Clear"]');
      expect(clearButton).not.toBeNull();

      const btnClasses = clearButton?.className ?? "";
      expect(btnClasses).toContain("text-muted-foreground");
      expect(btnClasses).toContain("hover:bg-accent");
      expect(btnClasses).toContain("hover:text-accent-foreground");
      expect(btnClasses).toContain("transition-colors");
      expect(btnClasses).toContain("rounded-selector");

      const icon = clearButton?.querySelector("svg");
      expect(icon).not.toBeNull();
      const iconClasses = icon?.getAttribute("class") ?? "";
      expect(iconClasses).toContain("size-4");
      expect(iconClasses).not.toContain("text-muted-foreground");
      expect(iconClasses).not.toContain("hover:text-foreground");
    });
  });
});
