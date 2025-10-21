import {
  CartesianGridProps,
  ChartType,
  LegendProps,
  LinesProps,
  MarkArea,
  MarkLine,
  ReferenceDot,
  ToolTipProps,
  XAxisProps,
  YAxisProps,
} from './chartTypes';
import { ChartData } from './chartTypes';

export type ColorScheme = 'Default' | 'Rainbow';
const defaultColors = ['chart-1', 'chart-2', 'chart-3', 'chart-4', 'chart-5'];
const rainbowColors = [
  'blue',
  'cyan',
  'yellow',
  'red',
  'orange',
  'purple',
  'lime',
  'indigo',
  'rose',
  'green',
  'pink',
  'teal',
  'amber',
  'violet',
  'emerald',
  'fuchsia',
  'sky',
];

export const getColors = (scheme: ColorScheme): string[] => {
  switch (scheme) {
    case 'Default':
      return defaultColors.map(name =>
        getComputedStyle(document.documentElement)
          .getPropertyValue(`--${name}`)
          .trim()
      );
    case 'Rainbow':
      return rainbowColors;
    default:
      return [];
  }
};

export const generateDataProps = (data: Record<string, unknown>[]) => {
  if (data.length === 0) {
    return { categoryKey: '', categories: [], valueKeys: [] };
  }
  const categoryKey = Object.keys(data[0]).find(
    k => typeof data[0][k] === 'string'
  );
  if (!categoryKey) {
    return { categoryKey: '', categories: [], valueKeys: [] };
  }
  const categories = data.map(d => d[categoryKey] as string);
  const valueKeys = Object.keys(data[0]).filter(
    k => typeof data[0][k] === 'number'
  );
  const allValues = data.flatMap(d =>
    Object.values(d).filter(v => typeof v === 'number')
  ) as number[];
  const minValue = Math.min(...allValues);
  const maxValue = Math.max(...allValues);
  const largeSpread =
    Math.abs(maxValue / (minValue === 0 ? 1 : minValue)) > 1e5;

  const transform = (v: number) => {
    if (!largeSpread) return v;
    const sign = Math.sign(v);
    return sign * Math.log10(Math.abs(v) + 1);
  };
  return {
    categoryKey,
    categories,
    valueKeys,
    transform,
    largeSpread,
    minValue,
    maxValue,
  };
};

export function generateEChartGrid(cartesianGrid?: CartesianGridProps) {
  const defaultGrid = {
    show: true,
    left: 2,
    right: 2,
    top: 30,
    bottom: 30,
    containLabel: true,
  };

  if (!cartesianGrid) return defaultGrid;

  return {
    ...defaultGrid,
    show: cartesianGrid.vertical || cartesianGrid.horizontal,
    ...(cartesianGrid.width != null && { width: cartesianGrid.width }),
    ...(cartesianGrid.height != null && { height: cartesianGrid.height }),
    ...(cartesianGrid.x != null && { x: cartesianGrid.x }),
    ...(cartesianGrid.y != null && { y: cartesianGrid.y }),
  };
}

export function generateEChartLegend(
  legend?: LegendProps,
  themeColors?: { foreground: string; fontSans: string }
) {
  if (!legend)
    return {
      show: true,
      textStyle: generateTextStyle(
        themeColors?.foreground,
        themeColors?.fontSans
      ),
    };

  return {
    show: true,
    icon: legend.iconType ? legend.iconType : 'rect',
    itemWidth: legend.iconSize ?? 14,
    itemHeight: legend.iconSize ?? 14,
    itemGap: 16,
    orient: legend.layout?.toLowerCase(),
    top: legend.verticalAlign === 'Bottom' ? 'bottom' : 'top',
    type: 'scroll',
    textStyle: generateTextStyle(
      themeColors?.foreground,
      themeColors?.fontSans
    ),
  };
}

export const getTransformValueFn = (data: ChartData[]) => {
  const allValues = data.flatMap(d =>
    Object.values(d).filter(v => typeof v === 'number')
  ) as number[];
  const minValue = Math.min(...allValues);
  const maxValue = Math.max(...allValues);
  const largeSpread =
    Math.abs(maxValue / (minValue === 0 ? 1 : minValue)) > 1e5;

  const transform = (v: number) => {
    if (!largeSpread) return v;
    const sign = Math.sign(v);
    return sign * Math.log10(Math.abs(v) + 1);
  };

  return { transform, largeSpread, minValue, maxValue };
};

export const generateTextStyle = (
  foreground: string = '#000000',
  fontSans: string = 'Geist, sans-serif'
) => ({
  color: foreground,
  fontFamily: fontSans,
  fontSize: 12,
});

export const generateAxisLabelStyle = (
  mutedForeground: string = '#666666',
  fontSans: string = 'Geist, sans-serif'
) => ({
  color: mutedForeground,
  fontFamily: fontSans,
  fontSize: 11,
});

export const generateSeries = (
  data: ChartData[],
  valueKeys: string[],
  lines?: LinesProps[],
  transform?: (v: number) => number,
  referenceDots?: ReferenceDot,
  referenceLines?: MarkLine,
  referenceAreas?: MarkArea
) => {
  return valueKeys.map((key, i) => {
    const lineConfig = lines?.[i];

    return {
      name: key,
      type: ChartType.Line,
      data: data.map(d =>
        transform ? transform(Number(d[key] ?? 0)) : Number(d[key] ?? 0)
      ),
      step: lineConfig?.curveType === 'Step' ? 'middle' : false,
      smooth: lineConfig?.curveType === 'Natural' ? true : false,
      showSymbol: true,
      symbolSize: 6,
      lineStyle: { width: 2, opacity: 0.9 },
      emphasis: {
        focus: 'series',
        disabled: true,
        lineStyle: { width: 3, opacity: 1 },
        itemStyle: { borderWidth: 2, borderColor: 'var(--background, #fff)' },
      },
      blur: { lineStyle: { opacity: 0.6 } },
      animation: true,
      animationDuration: 800,
      markPoint: referenceDots ?? {},
      markLine: referenceLines ?? {},
      markArea: referenceAreas ?? {},
    };
  });
};

export const generateXAxis = (
  categories: string[],
  xAxis?: XAxisProps[],
  isVertical?: boolean,
  themeColors?: { mutedForeground: string; fontSans: string }
) => ({
  position: xAxis?.[0]?.orientation?.toLowerCase() === 'top' ? 'top' : 'bottom',
  type: isVertical ? 'value' : 'category',
  data: isVertical ? undefined : categories,
  axisLabel: {
    show: true,
    formatter: (value: string) =>
      value.length > 10 ? value.match(/.{1,10}/g)?.join('\n') : value,
    interval: 'auto',
    ...generateAxisLabelStyle(
      themeColors?.mutedForeground,
      themeColors?.fontSans
    ),
  },
});

export const generateYAxis = (
  largeSpread: boolean = false,
  transformValue?: (v: number) => number,
  minValue: number = 0,
  maxValue: number = 100,
  yAxis?: YAxisProps[],
  isVertical: boolean = false,
  categories?: string[],
  themeColors?: { mutedForeground: string; fontSans: string }
) => {
  const safeTransform = transformValue ?? ((v: number) => v);

  return {
    type: isVertical ? 'category' : 'value',
    data: isVertical ? categories : undefined,
    axisLabel: {
      show: true,
      formatter: (value: number) => {
        if (largeSpread) {
          const unscaled = Math.sign(value) * (10 ** Math.abs(value) - 1);
          if (Math.abs(unscaled) >= 1e9)
            return (unscaled / 1e9).toFixed(0) + 'B';
          if (Math.abs(unscaled) >= 1e6)
            return (unscaled / 1e6).toFixed(0) + 'M';
          if (Math.abs(unscaled) >= 1e3)
            return (unscaled / 1e3).toFixed(0) + 'K';
          return unscaled.toFixed(0);
        }
        if (Math.abs(value) >= 1e9) return (value / 1e9).toFixed(0) + 'B';
        if (Math.abs(value) >= 1e6) return (value / 1e6).toFixed(0) + 'M';
        if (Math.abs(value) >= 1e3) return (value / 1e3).toFixed(0) + 'K';
        return value;
      },
      ...generateAxisLabelStyle(
        themeColors?.mutedForeground,
        themeColors?.fontSans
      ),
    },
    splitNumber: largeSpread ? 3 : 5,
    min: largeSpread ? safeTransform(minValue) : 0,
    max: largeSpread ? safeTransform(maxValue) : 'dataMax',
    position: yAxis?.[0]?.orientation === 'Right' ? 'right' : 'left',
  };
};

export const generateTooltip = (
  tooltip?: ToolTipProps,
  type?: string,
  themeColors?: { foreground: string; fontSans: string; background: string }
) => ({
  trigger: 'axis',
  axisPointer: {
    type: type ?? 'cross',
    animated: tooltip?.animated ?? true,
    shadowStyle: { opacity: 0.5 },
  },
  textStyle: generateTextStyle(themeColors?.foreground, themeColors?.fontSans),
  backgroundColor: themeColors?.background || 'rgba(255, 255, 255, 0.9)',
  borderColor: themeColors?.foreground || '#000',
  borderWidth: 1,
});
