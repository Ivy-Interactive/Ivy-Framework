export function parsePixelValue(
  value: string | number | undefined,
  defaultValue: number
): number {
  if (typeof value === 'number') return value;
  if (typeof value === 'string') {
    if (
      value.includes('%') ||
      value.includes('px') ||
      value.includes('em') ||
      value.includes('rem')
    ) {
      return defaultValue;
    }
    const parsed = parseInt(value);
    return isNaN(parsed) ? defaultValue : parsed;
  }
  return defaultValue;
}

export function calculateGridDimensions(
  containerWidth?: number,
  containerHeight?: number,
  defaultWidth: number = 800,
  defaultHeight: number = 600
): { width: number; height: number } {
  return {
    width: containerWidth || defaultWidth,
    height: containerHeight || defaultHeight,
  };
}
