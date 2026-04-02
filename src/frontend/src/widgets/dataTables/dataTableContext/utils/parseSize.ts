/**
 * Parses a Size string (e.g., "Px:200") to a numeric pixel value
 * If input is already a number, returns it as-is
 */
export function parseSize(size: number | string | undefined): number {
  if (typeof size === "number") return size;
  if (!size) return 150; // default width

  // Parse "Px:200" or "Rem:10" format
  const match = size.match(/^(Px|Rem):(\d+\.?\d*)$/);
  if (match) {
    const [, unit, value] = match;
    const numValue = parseFloat(value);
    // For Rem, convert to pixels (assuming 16px = 1rem)
    return unit === "Rem" ? numValue * 16 : numValue;
  }

  return 150; // fallback to default
}

/**
 * Estimates minimum column width needed to display header text without truncation.
 * Uses approximate character width (8px per char) plus padding for icon + sort indicator.
 */
export function estimateHeaderWidth(headerText: string): number {
  const CHAR_WIDTH = 8; // approximate px per character in header font
  const HEADER_PADDING = 40; // padding for icon, sort arrow, cell padding
  const MIN_WIDTH = 60; // absolute minimum
  const MAX_AUTO_WIDTH = 300; // cap auto-width to prevent excessively wide columns

  const estimated = headerText.length * CHAR_WIDTH + HEADER_PADDING;
  return Math.max(MIN_WIDTH, Math.min(estimated, MAX_AUTO_WIDTH));
}
