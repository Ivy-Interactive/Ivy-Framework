import React from "react";
import { render } from "@testing-library/react";
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

  test.each(variants)("%s variant renders id attribute", (variant) => {
    const testId = `test-anchor-${variant.toLowerCase()}`;
    const { container } = render(
      <TextBlockWidget
        id="test-widget"
        content="Test content"
        variant={variant}
        anchor={testId}
      />
    );

    const element = container.querySelector(`#${testId}`);
    expect(element).not.toBeNull();
    expect(element?.id).toBe(testId);
  });

  test("heading variants also render id attribute", () => {
    const headingVariants = ["H1", "H2", "H3", "H4", "H5", "H6"] as const;

    headingVariants.forEach((variant) => {
      const testId = `test-anchor-${variant.toLowerCase()}`;
      const { container } = render(
        <TextBlockWidget
          id="test-widget"
          content="Test heading"
          variant={variant}
          anchor={testId}
        />
      );

      const element = container.querySelector(`#${testId}`);
      expect(element).not.toBeNull();
      expect(element?.id).toBe(testId);
    });
  });

  test("no id attribute when anchor is not provided", () => {
    const { container } = render(
      <TextBlockWidget id="test-widget" content="Test content" variant="P" />
    );

    const paragraph = container.querySelector("p");
    expect(paragraph).not.toBeNull();
    expect(paragraph?.id).toBe("");
  });
});
