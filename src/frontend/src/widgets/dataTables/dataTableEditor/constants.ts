import { Densities } from "@/types/density";

export const DENSITY_CONFIG = {
  [Densities.Small]: {
    rowHeight: 30,
    groupHeaderHeight: 28,
    cellHorizontalPadding: 6,
    cellVerticalPadding: 4,
    headerIconSize: 16,
    fontSize: "12px",
  },
  [Densities.Medium]: {
    rowHeight: 38,
    groupHeaderHeight: 36,
    cellHorizontalPadding: 8,
    cellVerticalPadding: 8,
    headerIconSize: 20,
    fontSize: "13px",
  },
  [Densities.Large]: {
    rowHeight: 48,
    groupHeaderHeight: 44,
    cellHorizontalPadding: 12,
    cellVerticalPadding: 12,
    headerIconSize: 22,
    fontSize: "14px",
  },
} as const;

// Keep backward-compat exports for any external consumers
export const ROW_HEIGHT = DENSITY_CONFIG[Densities.Medium].rowHeight;
export const GROUP_HEADER_HEIGHT = DENSITY_CONFIG[Densities.Medium].groupHeaderHeight;
