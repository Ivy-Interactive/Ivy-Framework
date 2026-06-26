import { describe, it, expect, beforeEach, afterEach } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import { Kbd, ShortcutKeys } from "./Kbd";

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
  it("renders the whole shortcut inside a single cap", () => {
    mount(<Kbd keys="Ctrl+Shift+C" />);
    const caps = container.querySelectorAll("kbd");
    expect(caps).toHaveLength(1);
  });

  it("uppercases a lone letter", () => {
    mount(<Kbd keys="c" />);
    expect(container.querySelector("kbd")!.textContent).toBe("C");
  });

  it("renders symbols and modifier names verbatim, no icons", () => {
    mount(<Kbd keys="⌘+⌥+N" />);
    const cap = container.querySelector("kbd")!;
    expect(cap.querySelector("svg")).toBeNull();
    // All single-character keys → no "+" separator.
    expect(cap.textContent).toBe("\u2318\u2009\u2325\u2009N");
  });

  it("keeps a typed word like cmd as text (no symbol mapping)", () => {
    mount(<Kbd keys="cmd+n" />);
    // "cmd" is multi-character → keys joined with "+"; lone "n" uppercased.
    expect(container.querySelector("kbd")!.textContent).toBe("cmd+N");
  });

  it("joins with + when any key is multi-character", () => {
    mount(<Kbd keys="Ctrl+Shift" />);
    expect(container.querySelector("kbd")!.textContent).toBe("Ctrl+Shift");
  });

  it("renders Enter and Backspace as symbols", () => {
    mount(<Kbd keys="Enter" />);
    expect(container.querySelector("kbd")!.textContent).toBe("↵");
    mount(<Kbd keys="Backspace" />);
    expect(container.querySelector("kbd")!.textContent).toBe("⌫");
  });

  it("treats a mapped symbol as single-character (no + with another single key)", () => {
    mount(<Kbd keys="Ctrl+Enter" />);
    // "Ctrl" (4 chars) forces the "+" join; Enter renders as its symbol.
    expect(container.querySelector("kbd")!.textContent).toBe("Ctrl+↵");
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
  it("renders the shortcut as text in a single cap", () => {
    mount(<ShortcutKeys shortcut="Ctrl+K" />);
    const caps = container.querySelectorAll("kbd");
    expect(caps).toHaveLength(1);
    expect(caps[0].textContent).toBe("Ctrl+K");
  });

  it("omits the + when every key is a single character", () => {
    mount(<ShortcutKeys shortcut="⌘+⌥+N" />);
    expect(container.querySelector("kbd")!.textContent).toBe("\u2318\u2009\u2325\u2009N");
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
