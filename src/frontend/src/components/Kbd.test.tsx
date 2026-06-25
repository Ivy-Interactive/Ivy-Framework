import { describe, it, expect, beforeEach, afterEach } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import { Kbd, ShortcutKeys } from "./Kbd";

// Note: jsdom reports a non-Mac userAgent, so `isMac` is false here. Modifier keys
// therefore render as text ("Ctrl", "Alt") rather than the Mac symbols (⌘, ⌥).

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

describe("Kbd", () => {
  it("renders each key in a combination as its own standalone cap", () => {
    mount(<Kbd>Ctrl+Shift+C</Kbd>);
    const caps = container.querySelectorAll("kbd");
    expect(caps).toHaveLength(3);
  });

  it("renders a single key as a single cap", () => {
    mount(<Kbd>A</Kbd>);
    const caps = container.querySelectorAll("kbd");
    expect(caps).toHaveLength(1);
    expect(caps[0].textContent).toBe("A");
  });

  it("uppercases a single-letter key", () => {
    mount(<Kbd>c</Kbd>);
    expect(container.querySelector("kbd")!.textContent).toBe("C");
  });

  it("renders modifier keys as text on non-Mac platforms", () => {
    mount(<Kbd>Ctrl+C</Kbd>);
    const caps = container.querySelectorAll("kbd");
    expect(caps[0].textContent).toBe("Ctrl");
    expect(caps[1].textContent).toBe("C");
  });

  it("renders an icon (svg) for navigation keys like Enter", () => {
    mount(<Kbd>Ctrl+Enter</Kbd>);
    const caps = container.querySelectorAll("kbd");
    expect(caps).toHaveLength(2);
    // The Enter cap should contain an icon, not text.
    const enterCap = caps[1];
    expect(enterCap.querySelector("svg")).not.toBeNull();
    expect(enterCap.getAttribute("aria-label")).toBe("Enter");
  });

  it("renders arrow keys as icons", () => {
    mount(<Kbd>ArrowUp</Kbd>);
    const cap = container.querySelector("kbd")!;
    expect(cap.querySelector("svg")).not.toBeNull();
    expect(cap.getAttribute("aria-label")).toBe("Arrow Up");
  });

  it("renders non-string children inside a single cap unchanged", () => {
    mount(
      <Kbd>
        <span data-testid="custom">x</span>
      </Kbd>,
    );
    const caps = container.querySelectorAll("kbd");
    expect(caps).toHaveLength(1);
    expect(caps[0].querySelector('[data-testid="custom"]')).not.toBeNull();
  });

  it("tokenizes the `keys` prop into standalone caps", () => {
    mount(<Kbd keys="Ctrl+Shift+C" />);
    const caps = container.querySelectorAll("kbd");
    expect(caps).toHaveLength(3);
  });

  it("prefers the `keys` prop over children", () => {
    mount(<Kbd keys="Ctrl+K">ignored</Kbd>);
    const caps = container.querySelectorAll("kbd");
    expect(caps).toHaveLength(2);
    expect(caps[0].textContent).toBe("Ctrl");
    expect(caps[1].textContent).toBe("K");
  });

  it("keeps caps square via a fixed height and matching minimum width", () => {
    mount(<Kbd keys="A" />);
    const cap = container.querySelector("kbd")!;
    // h-5 fixes the height and min-w-5 matches it, so a single-glyph cap is square.
    expect(cap.className).toContain("h-5");
    expect(cap.className).toContain("min-w-5");
  });

  it("renders a ghost cap without background or border", () => {
    mount(<Kbd keys="A" ghost />);
    const cap = container.querySelector("kbd")!;
    expect(cap.className).toContain("border-0");
    expect(cap.className).toContain("bg-transparent");
    expect(cap.className).not.toContain("bg-muted");
  });
});

describe("ShortcutKeys", () => {
  it("tokenizes a shortcut string into standalone caps", () => {
    mount(<ShortcutKeys shortcut="Ctrl+K" />);
    const caps = container.querySelectorAll("kbd");
    expect(caps).toHaveLength(2);
    expect(caps[0].textContent).toBe("Ctrl");
    expect(caps[1].textContent).toBe("K");
  });

  it("renders nothing for an empty shortcut", () => {
    mount(<ShortcutKeys shortcut="" />);
    expect(container.querySelectorAll("kbd")).toHaveLength(0);
  });

  it("applies a custom className to the wrapper", () => {
    mount(<ShortcutKeys shortcut="A" className="ml-auto" />);
    const wrapper = container.querySelector("span");
    expect(wrapper!.className).toContain("ml-auto");
  });
});
