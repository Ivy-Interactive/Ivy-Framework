import { cva } from "class-variance-authority";

import { cn } from "@/lib/utils";

/** Affix cells: muted box by default; ghost uses transparent chrome with tight padding toward the input. */
export function textInputAffixCellClasses(
  side: "prefix" | "suffix",
  isFocused: boolean,
  ghostWithAffixes: boolean,
): string {
  return cn(
    "flex items-center text-muted-foreground [&_button]:rounded [&_button]:px-1 [&_button]:hover:bg-accent [&_button]:cursor-pointer [&_button]:transition-colors",
    ghostWithAffixes
      ? side === "suffix"
        ? "shrink-0 bg-transparent pl-0 pr-1.5"
        : "shrink-0 bg-transparent pl-2 pr-0.5"
      : cn(
          "px-3 bg-muted",
          side === "prefix"
            ? "rounded-tl-fields rounded-bl-fields"
            : "rounded-tr-fields rounded-br-fields",
        ),
    side === "prefix"
      ? !ghostWithAffixes && !isFocused && "border-r border-input"
      : !ghostWithAffixes && !isFocused && "border-l border-input",
  );
}

// Size variants for TextInputWidget
export const textInputSizeVariant = cva("w-full", {
  variants: {
    density: {
      Small: "text-xs px-2 h-7",
      Medium: "text-sm px-3 h-9",
      Large: "text-base px-4 h-11",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

// Size variants for Textarea (minimum height and padding scale; no fixed height)
export const textareaSizeVariant = cva("w-full", {
  variants: {
    density: {
      Small: "min-h-[52px] py-2 px-2 text-xs",
      Medium: "min-h-[60px] py-2 px-3 text-sm",
      Large: "min-h-[72px] py-3 px-4 text-base",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

// Size variants for search icon
export const searchIconVariant = cva("absolute text-muted-foreground", {
  variants: {
    density: {
      Small: "left-3 top-2 h-3 w-3",
      Medium: "left-2.5 top-2.5 h-4 w-4",
      Large: "left-2 top-3 h-5 w-5",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

// Size variants for X icon
export const xIconVariant = cva("text-muted-foreground hover:text-foreground", {
  variants: {
    density: {
      Small: "top-2 h-3 w-3",
      Medium: "top-2.5 h-4 w-4",
      Large: "top-3 h-5 w-5",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

// Size variants for eye icons (password toggle)
export const eyeIconVariant = cva("", {
  variants: {
    density: {
      Small: "h-3 w-3",
      Medium: "h-4 w-4",
      Large: "h-5 w-5",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});
