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
