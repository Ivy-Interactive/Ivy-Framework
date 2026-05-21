/** Default C# / widget values from RadarChart.cs */
export const RADAR_DEFAULT_CY = "50%";
export const RADAR_DEFAULT_RADIUS = "75%";

export interface RadarLayoutInput {
  cx: string;
  cy: string;
  radius: string;
  hasLegend: boolean;
  hasToolbox: boolean;
  indicators: { name: string }[];
}

export interface RadarLayout {
  center: [string, string];
  radius: string;
}

const isDefaultPercent = (value: string, defaultValue: string) =>
  value === defaultValue || value === defaultValue.replace("%", "");

/**
 * Computes radar center and radius so axis names (especially at 12 o'clock with
 * startAngle 90) fit inside the chart canvas.
 *
 * Only adjusts when cy/radius are still at framework defaults; explicit values are preserved.
 */
export function computeRadarLayout({
  cx,
  cy,
  radius,
  hasLegend,
  hasToolbox,
  indicators,
}: RadarLayoutInput): RadarLayout {
  const usingDefaultCy = cy === RADAR_DEFAULT_CY;
  const usingDefaultRadius = radius === RADAR_DEFAULT_RADIUS;

  if (!usingDefaultCy && !usingDefaultRadius) {
    return { center: [cx, cy], radius };
  }

  const maxLabelLen = indicators.reduce((max, ind) => Math.max(max, ind.name?.length ?? 0), 0);

  // Shift center down and shrink radius — top label sits above the top vertex.
  // Do not move center up for bottom legend (that worsens top clipping).
  let centerY = hasLegend ? 52 : 54;
  let radiusPct = hasLegend ? 60 : 65;

  if (hasToolbox) {
    centerY += 1;
    radiusPct -= 2;
  }

  if (maxLabelLen > 14) {
    radiusPct -= 4;
    centerY += 1;
  } else if (maxLabelLen > 10) {
    radiusPct -= 2;
  }

  return {
    center: [cx, usingDefaultCy ? `${centerY}%` : cy],
    radius: usingDefaultRadius ? `${radiusPct}%` : radius,
  };
}
