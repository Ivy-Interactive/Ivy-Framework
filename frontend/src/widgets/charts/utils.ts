import { PieProps } from 'recharts';

export interface PieChartTotalProps {
  formattedValue: string;
  label: string;
}

// Calculate the layout for the center label of a donut/pie chart
export function calculateCenterLabelLayout(
  viewBox: Record<string, unknown> | undefined,
  basePieProps: Partial<PieProps>,
  total: PieChartTotalProps
):
  | {
      cx: number;
      cy: number;
      text: string;
      valueFont: number;
      labelFont: number;
      gap: number;
    }
  | undefined {
  if (!viewBox || !('cx' in viewBox) || !('cy' in viewBox)) return undefined;

  const vb = viewBox as Record<string, unknown>;
  const asNumber = (v: unknown): number => {
    if (typeof v === 'number') return v;
    if (typeof v === 'string') {
      const n = Number(v);
      return Number.isFinite(n) ? n : 0;
    }
    return 0;
  };
  const cx = asNumber(vb.cx) || 0;
  const cy = asNumber(vb.cy) || 0;
  let innerR = asNumber(vb.innerRadius) || 0;
  let outerR = asNumber(vb.outerRadius) || 0;

  // Resolve radii if provided as percentages in props
  const toNumberFromProp = (v: unknown, ref: number): number => {
    if (typeof v === 'number') return v;
    if (typeof v === 'string') {
      const m = v.trim().match(/^([0-9]+(?:\.[0-9]+)?)%$/);
      if (m) return (parseFloat(m[1]) / 100) * (ref || 1);
      const n = Number(v);
      if (Number.isFinite(n)) return n;
    }
    return 0;
  };

  if (!outerR && basePieProps.outerRadius) {
    const vbR =
      asNumber((viewBox as Record<string, unknown>).radius) ||
      asNumber((viewBox as Record<string, unknown>).r) ||
      0;
    outerR = toNumberFromProp(basePieProps.outerRadius as unknown, vbR || 100);
  }
  if (!innerR && basePieProps.innerRadius) {
    innerR = toNumberFromProp(basePieProps.innerRadius as unknown, outerR);
  }

  const ring = innerR > 0 ? innerR : outerR > 0 ? outerR * 0.6 : 80;

  // Available width inside donut hole (diameter of inner circle)
  const available = Math.max(0, innerR > 0 ? innerR * 2 : outerR * 1.2);

  const rawText = String(total.formattedValue ?? '');
  const toNumber = (s: string): number | null => {
    const clean = s.replace(/[^0-9.-]/g, '');
    const num = Number(clean);
    return Number.isFinite(num) ? num : null;
  };
  const abbreviate = (n: number): string => {
    const abs = Math.abs(n);
    if (abs >= 1_000_000_000) return `${(n / 1_000_000_000).toFixed(2)}B`;
    if (abs >= 1_000_000) return `${(n / 1_000_000).toFixed(2)}M`;
    if (abs >= 1_000) return `${(n / 1_000).toFixed(1)}K`;
    return n.toLocaleString();
  };

  let text = rawText;

  let valueFont = Math.max(16, Math.min(36, Math.floor(ring * 0.5)));
  const minFont = 12;

  const estimateWidth = (str: string, font: number) => str.length * font * 0.6;
  while (
    valueFont > minFont &&
    estimateWidth(text, valueFont) > available * 0.9
  ) {
    valueFont -= 1;
  }

  if (estimateWidth(text, valueFont) > available * 0.95) {
    const num = toNumber(rawText);
    if (num !== null) {
      const abbr = abbreviate(num);
      text = abbr;
      // Re-fit after abbreviation
      while (
        valueFont > minFont &&
        estimateWidth(text, valueFont) > available * 0.9
      ) {
        valueFont -= 1;
      }
    }
  }

  const labelFont = Math.max(11, Math.min(16, Math.floor(valueFont * 0.55)));
  const gap = Math.max(12, Math.floor(valueFont * 0.85));

  return { cx, cy, text, valueFont, labelFont, gap };
}
