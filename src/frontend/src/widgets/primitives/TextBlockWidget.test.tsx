import { describe, it, expect } from "vitest";
import { renderToString } from "react-dom/server";
import { TextBlockWidget } from "./TextBlockWidget";

describe("TextBlockWidget id attribute", () => {
  const variants = [
    "Literal",
    "Block",
    "P",
    "Inline",
    "Blockquote",
    "Monospaced",
    "Lead",
    "Muted",
    "Danger",
    "Warning",
    "Success",
    "Label",
    "Strong",
    "Display",
  ] as const;

  variants.forEach((variant) => {
    it(`${variant} variant renders id attribute`, () => {
      const testId = `test-anchor-${variant.toLowerCase()}`;
      const html = renderToString(
        <TextBlockWidget
          id="test-widget"
          content="Test content"
          variant={variant}
          anchor={testId}
        />,
      );

      expect(html).toContain(`id="${testId}"`);
    });
  });

  it("heading variants render id attribute", () => {
    const headingVariants = ["H1", "H2", "H3", "H4", "H5", "H6"] as const;

    headingVariants.forEach((variant) => {
      const testId = `test-anchor-${variant.toLowerCase()}`;
      const html = renderToString(
        <TextBlockWidget
          id="test-widget"
          content="Test heading"
          variant={variant}
          anchor={testId}
        />,
      );

      expect(html).toContain(`id="${testId}"`);
    });
  });

  it("no id attribute when anchor is not provided", () => {
    const html = renderToString(
      <TextBlockWidget id="test-widget" content="Test content" variant="P" />,
    );

    const paragraph = html.match(/<p[^>]*>/)?.[0];
    expect(paragraph).toBeDefined();
    expect(paragraph).not.toContain('id="');
  });
});
