import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { act } from "react";
import { createRoot, Root } from "react-dom/client";

vi.mock("@/components/stream-handler/hooks", () => ({
  useStream: () => undefined,
}));

const sentEvents: Array<{ name: string; id: string; args: unknown[] }> = [];
vi.mock("@/components/event-handler/hooks", () => ({
  useEventHandler: () => (name: string, id: string, args: unknown[]) =>
    sentEvents.push({ name, id, args }),
}));

import { RichTextBlockWidget } from "../RichTextBlockWidget";

describe("RichTextBlockWidget link decoration", () => {
  let container: HTMLDivElement;
  let root: Root;

  beforeEach(() => {
    sentEvents.length = 0;
    container = document.createElement("div");
    document.body.appendChild(container);
    root = createRoot(container);
  });

  afterEach(() => {
    act(() => root.unmount());
    container.remove();
  });

  const render = (runs: unknown[], events: string[] = []) =>
    act(() => {
      root.render(<RichTextBlockWidget id="rt" runs={runs as never} events={events} />);
    });

  const decoration = (el: Element | null) =>
    (el?.className ?? "")
      .split(/\s+/)
      .filter((c) => c === "underline" || c === "line-through" || c.startsWith("[text-decoration"));

  it("renders a plain link as underlined only", () => {
    render([{ content: "docs", link: "https://example.com" }]);
    const anchor = container.querySelector("a");
    expect(anchor).not.toBeNull();
    expect(decoration(anchor)).toEqual(["underline"]);
  });

  it("keeps both underline and line-through on a struck-through link", () => {
    render([{ content: "docs", link: "https://example.com", strikeThrough: true }]);
    const anchor = container.querySelector("a");
    expect(anchor).not.toBeNull();
    expect(decoration(anchor)).toEqual(["[text-decoration-line:underline_line-through]"]);
    expect(anchor?.className).not.toContain("line-through ");
  });

  it("keeps both decorations on a struck-through OnLinkClick link", () => {
    render(
      [{ content: "docs", link: "https://example.com", strikeThrough: true }],
      ["OnLinkClick"],
    );
    const button = container.querySelector("button");
    expect(button).not.toBeNull();
    expect(decoration(button)).toEqual(["[text-decoration-line:underline_line-through]"]);
    expect(button?.className).toContain("cursor-pointer");
  });

  it("preserves bold and italic alongside the link decoration", () => {
    render([
      {
        content: "docs",
        link: "https://example.com",
        strikeThrough: true,
        bold: true,
        italic: true,
      },
    ]);
    const anchor = container.querySelector("a");
    expect(anchor?.className).toContain("font-semibold");
    expect(anchor?.className).toContain("italic");
    expect(decoration(anchor)).toEqual(["[text-decoration-line:underline_line-through]"]);
  });

  it("still uses line-through for non-link runs", () => {
    render([{ content: "gone", strikeThrough: true }]);
    const span = container.querySelector("span.line-through");
    expect(span).not.toBeNull();
    expect(container.querySelector("a")).toBeNull();
  });
});
