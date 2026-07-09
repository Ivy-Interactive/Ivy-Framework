import { renderToStaticMarkup } from "react-dom/server";
import { describe, it, expect } from "vitest";
import MarkdownRenderer from "@/components/MarkdownRenderer";

describe("MarkdownRenderer math handling", () => {
  it("treats single dollar signs in prose as literal text, not math", () => {
    const prose = "bash expands any $ (e.g. $env:PORT, or vars like $IsMacOS) so $ survives";
    const html = renderToStaticMarkup(<MarkdownRenderer content={prose} />);
    expect(html).not.toContain("katex"); // no math was rendered
    expect(html).toContain("$env:PORT"); // literal dollars preserved
  });

  it("still renders $$...$$ as math", () => {
    const html = renderToStaticMarkup(<MarkdownRenderer content={"$$E = mc^2$$"} />);
    expect(html).toContain("katex");
  });
});
