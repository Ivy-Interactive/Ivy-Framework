import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import { HtmlRenderer } from "./HtmlRenderer";

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

describe("HtmlRenderer link handling", () => {
  it("renders custom protocol links when onLinkClick is provided", () => {
    const onLinkClick = vi.fn();
    const content = '<a href="plan://03290">Plan 03290</a>';
    mount(<HtmlRenderer content={content} onLinkClick={onLinkClick} />);

    const link = container.querySelector("a");
    expect(link).not.toBeNull();
    expect(link!.getAttribute("href")).toBe("plan://03290");
    expect(link!.textContent).toBe("Plan 03290");
  });

  it("replaces invalid custom protocols with # when onLinkClick is not provided", () => {
    const content = '<a href="plan://03290">Plan 03290</a>';
    mount(<HtmlRenderer content={content} />);

    const link = container.querySelector("a");
    expect(link).not.toBeNull();
    expect(link!.getAttribute("href")).toBe("#");
  });

  it("calls onLinkClick when link is clicked", () => {
    const onLinkClick = vi.fn();
    const content = '<a href="plan://03290">Plan 03290</a>';
    mount(<HtmlRenderer content={content} onLinkClick={onLinkClick} />);

    const link = container.querySelector("a");
    expect(link).not.toBeNull();

    act(() => {
      link!.click();
    });

    expect(onLinkClick).toHaveBeenCalledTimes(1);
    expect(onLinkClick).toHaveBeenCalledWith("plan://03290");
  });
});
