import type { CSSProperties } from "react";
import { cva } from "class-variance-authority";
import { Densities } from "@/types/density";

/** Matches Tailwind widths used under `markdownCodeCopyScrollPaddingClass` / `codeScrollPaddingForDensity`. */
export function codeCopyGutterLength(density: Densities): string {
  switch (density) {
    case Densities.Small:
      return "2.5rem";
    case Densities.Large:
      return "3.5rem";
    default:
      return "3rem";
  }
}

export const markdownCodeCopyGutterLength = codeCopyGutterLength(Densities.Medium);

/** Clips scrolling paint so glyphs never occupy the viewport’s right gutter under a floating control. */
export function codeCopyViewportInsetStyle(length: string): CSSProperties {
  const clipPath = `inset(0 ${length} 0 0)`;
  return { clipPath };
}

export const codeContainerVariant = cva("", {
  variants: {
    density: {
      Small: "text-xs",
      Medium: "text-sm",
      Large: "text-base",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

/**
 * Extra inset on scrollable code so overlays never obscure text (matches positioned copy + gap).
 * Tailwind Medium default for markdown `CodeBlock` (density not exposed there).
 */
export const markdownCodeCopyScrollPaddingClass = "pr-[3rem]";

/** Same reserved band as `markdownCodeCopyScrollPaddingClass`, adjusted per widget density */
export function codeScrollPaddingForDensity(density: Densities): string {
  switch (density) {
    case Densities.Small:
      return "pr-10";
    case Densities.Large:
      return "pr-14";
    default:
      return markdownCodeCopyScrollPaddingClass;
  }
}

export const codeCopyButtonVariant = cva("absolute z-50", {
  variants: {
    density: {
      Small: "top-1 right-1",
      Medium: "top-1.5 right-1.5",
      Large: "top-2 right-2",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});
