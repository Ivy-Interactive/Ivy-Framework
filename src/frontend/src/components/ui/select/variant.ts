import { cva } from "class-variance-authority";

const selectTriggerBase =
  "box-border flex w-full items-center justify-between whitespace-nowrap rounded-field border border-input bg-transparent shadow-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50 [&>span:first-child]:flex-1 [&>span:first-child]:min-w-0 [&>span:first-child]:truncate [&>span:first-child]:text-left cursor-pointer dark:bg-transparent dark:border-white/10";

/** Single-value select trigger (e.g. Assignee). */
export const selectSingleTriggerVariant = cva(selectTriggerBase, {
  variants: {
    density: {
      Small: "h-7 px-2 py-1 text-xs",
      Medium: "h-9 px-3 py-2 text-sm",
      Large: "h-11 px-4 py-3 text-base",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

/** Multi-value select trigger with badge chips (e.g. Labels). */
export const selectMultiTriggerVariant = cva(selectTriggerBase, {
  variants: {
    density: {
      // Match single-select right padding so clear + chevron align across variants.
      Small: "h-7 pl-0 pr-2 py-1 text-xs",
      Medium: "h-9 pl-1 pr-3 py-2 text-sm",
      Large: "h-11 pl-2 pr-4 py-3 text-base",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

/** Clear, loading, invalid, and chevron cluster at the end of select triggers. */
export const selectTriggerEndActionsVariant = cva(
  "flex items-center gap-1 shrink-0 ml-auto px-1 pointer-events-none",
);

/** @deprecated Prefer {@link selectSingleTriggerVariant}. */
export const selectTriggerVariant = selectSingleTriggerVariant;

export const selectContentVariant = cva(
  "relative z-50 max-h-96 min-w-[8rem] overflow-hidden rounded-box border bg-popover text-popover-foreground shadow-md data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95 data-[side=bottom]:slide-in-from-top-2 data-[side=left]:slide-in-from-right-2 data-[side=right]:slide-in-from-left-2 data-[side=top]:slide-in-from-bottom-2",
  {
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
  },
);

export const selectItemVariant = cva(
  "relative flex w-full cursor-pointer select-none items-center rounded-selector py-1.5 pl-2 pr-8 outline-none focus:bg-accent focus:text-accent-foreground data-[disabled]:pointer-events-none data-[disabled]:opacity-50",
  {
    variants: {
      density: {
        Small: "py-1 pl-1.5 pr-6 text-xs font-medium",
        Medium: "py-1.5 pl-2 pr-8 text-sm font-medium",
        Large: "py-2 pl-2.5 pr-10 text-base font-medium",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

/** @deprecated Use {@link selectTriggerEndActionsVariant} inside the trigger flex row. */
export const selectIconContainerVariant = selectTriggerEndActionsVariant;
