/** Heights shared by inputs, selects, and buttons at each density */
export const controlHeight = {
  Small: "h-7",
  Medium: "h-9",
  Large: "h-11",
} as const;

/** Square control size (icon-only buttons) matching {@link controlHeight} */
export const controlSize = {
  Small: "size-7",
  Medium: "size-9",
  Large: "size-11",
} as const;

/** Base density scale — used by table-head, expandable trigger, and as reference for offset scales */
export const densityHeight = {
  Small: "h-8",
  Medium: "h-10",
  Large: "h-12",
} as const;

/** One step above base — available for components needing a larger scale */
export const densityHeightLg = {
  Small: "h-10",
  Medium: "h-12",
  Large: "h-14",
} as const;

export const densityText = {
  Small: "text-xs",
  Medium: "text-sm",
  Large: "text-base",
} as const;

export const densityTreeGap = {
  Small: "gap-0.5",
  Medium: "gap-1",
  Large: "gap-1.5",
} as const;
