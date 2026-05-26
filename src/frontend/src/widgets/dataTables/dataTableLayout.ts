import { Densities } from "@/types/density";
import { DENSITY_CONFIG } from "./dataTableEditor/constants";

/** Border, horizontal scrollbar, and small layout slack. */
const CHROME_PX = 16;
/** Filter toolbar band above the grid (matches DataTableHeader + option row). */
const FILTER_BAND_PX = 44;
/** Extra space below aggregate footer labels. */
const AGGREGATE_FOOTER_EXTRA_PX = 12;
/** Minimum data rows that must stay readable at the smallest allowed height. */
const MIN_VISIBLE_ROWS = 3;

export interface DataTableMinHeightOptions {
  density?: Densities;
  hasFilter?: boolean;
  hasGroups?: boolean;
  hasAggregateFooter?: boolean;
}

/**
 * Smallest height the table widget may shrink to before page scroll takes over.
 * Sized for filter bar + headers + a few readable rows (+ optional group/footer bands).
 */
export function getDataTableMinHeight({
  density = Densities.Medium,
  hasFilter = false,
  hasGroups = false,
  hasAggregateFooter = false,
}: DataTableMinHeightOptions): string {
  const { rowHeight, groupHeaderHeight } = DENSITY_CONFIG[density];

  const px =
    (hasFilter ? FILTER_BAND_PX : 0) +
    (hasGroups ? groupHeaderHeight : 0) +
    rowHeight +
    rowHeight * MIN_VISIBLE_ROWS +
    (hasAggregateFooter ? rowHeight + AGGREGATE_FOOTER_EXTRA_PX : 0) +
    CHROME_PX;

  return `${px}px`;
}
