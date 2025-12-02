/**
 * Parses a Size string (e.g., "Px:200") to a numeric pixel value
 * If input is already a number, returns it as-is
 */
export function parseSize(size: number | string | undefined): number {
  if (typeof size === 'number') return size;
  if (!size) return 150; // default width

  // Parse "Px:200" or "Rem:10" format
  const match = size.match(/^(Px|Rem):(\d+\.?\d*)$/);
  if (match) {
    const [, unit, value] = match;
    const numValue = parseFloat(value);
    // For Rem, convert to pixels (assuming 16px = 1rem)
    return unit === 'Rem' ? numValue * 16 : numValue;
  }

  return 150; // fallback to default
}
