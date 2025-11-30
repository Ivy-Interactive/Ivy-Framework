import {
  CartesianGridProps,
  ChartType,
  LegendProps,
  LinesProps,
  MarkArea,
  MarkLine,
  ReferenceDot,
  ToolTipProps,
  ToolboxFeatures,
  ToolboxProps,
  XAxisProps,
  YAxisProps,
} from './chartTypes';
import { ChartData } from './chartTypes';
import {
  generateTextStyle,
  generateAxisLabelStyle,
  type ChartThemeColors,
} from './styles/theme';

// Re-export from styles
export type { ColorScheme } from './styles/colors';
export { getChartColors as getColors } from './styles/colors';
export { generateTextStyle, generateAxisLabelStyle, type ChartThemeColors };

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

export function generateEChartGrid(
  cartesianGrid?: CartesianGridProps,
  hasLegend: boolean = false
) {
  const defaultGrid = {
    show: false, // Hide grid border to remove the square frame
    left: '4%',
    right: '4%',
    top: 30,
    bottom: hasLegend ? 60 : 30, // More space for legend
    containLabel: true,
    borderWidth: 0, // Ensure no border is drawn
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
  const defaultLegends = {
    type: 'scroll',
    show: true,
    textStyle: generateTextStyle(
      themeColors?.foreground,
      themeColors?.fontSans
    ),
    top: 'bottom',
  };
  if (!legend) return defaultLegends;

  return {
    ...defaultLegends,
    icon: legend.iconType ? legend.iconType : 'rect',
    itemWidth: legend.iconSize ?? 14,
    itemHeight: legend.iconSize ?? 14,
    orient: legend.layout?.toLowerCase(),
    top: legend.verticalAlign === 'Bottom' ? 'bottom' : 'top',
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

// Text and axis styles are now imported from './styles/theme'

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
  chartType: string,
  categories: string[],
  xAxis?: XAxisProps[],
  isVertical?: boolean,
  themeColors?: { mutedForeground: string; fontSans: string }
) => ({
  position: xAxis?.[0]?.orientation?.toLowerCase() === 'top' ? 'top' : 'bottom',
  type: isVertical ? 'value' : 'category',
  boundaryGap: chartType === 'bar' ? true : false,
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
  axisLine: {
    show: true,
    lineStyle: {
      type: 'dashed',
      color: themeColors?.mutedForeground,
      opacity: 0.1,
    },
  },
  axisTick: {
    show: true,
    lineStyle: {
      color: themeColors?.mutedForeground,
      opacity: 0.4,
    },
  },
  splitLine: {
    show: true,
    lineStyle: {
      type: 'dashed',
      color: themeColors?.mutedForeground,
      opacity: 0.4,
    },
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
    min: largeSpread ? safeTransform(minValue) : Math.min(minValue, 0),
    ...(largeSpread && { max: safeTransform(maxValue) }),
    position: yAxis?.[0]?.orientation === 'Right' ? 'right' : 'left',
    axisLine: {
      show: true,
      lineStyle: {
        type: 'dashed',
        color: themeColors?.mutedForeground,
        opacity: 0.1,
      },
    },
    axisTick: {
      show: true,
      lineStyle: {
        color: themeColors?.mutedForeground,
        opacity: 0.4,
      },
    },
    splitLine: {
      show: true,
      lineStyle: {
        type: 'dashed',
        color: themeColors?.mutedForeground,
        opacity: 0.4,
      },
    },
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

export const generateEChartToolbox = (toolbox?: ToolboxProps) => {
  if (!toolbox || toolbox.enabled === false) {
    return { show: false };
  }

  const features: ToolboxFeatures = {};

  if (toolbox.dataView !== false) {
    features.dataView = {
      show: true,
      readOnly: false,
    };
  }

  if (toolbox.magicType !== false) {
    features.magicType = {
      show: true,
      type: ['line', 'bar'],
    };
  }

  if (toolbox.saveAsImage !== false) {
    features.saveAsImage = {
      show: true,
    };
  }

  return {
    show: true,
    orient:
      toolbox.orientation?.toLowerCase() === 'vertical'
        ? 'vertical'
        : 'horizontal',
    left:
      toolbox.align?.toLowerCase() === 'left'
        ? 'left'
        : toolbox.align?.toLowerCase() === 'center'
          ? 'center'
          : 'right',
    top:
      toolbox.verticalAlign?.toLowerCase() === 'top'
        ? 'top'
        : toolbox.verticalAlign?.toLowerCase() === 'middle'
          ? 'middle'
          : 'bottom',
    feature: features,
    emphasis: {
      iconStyle: {
        color: null,
        borderColor: null,
        textFill: getComputedStyle(document.documentElement)
          .getPropertyValue('--toolbox')
          .trim(),
      },
    },
  };
};
