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
import {
  CARTESIAN_GRID_DEFAULTS,
  LEGEND_DEFAULTS,
  TOOLTIP_DEFAULTS,
  TOOLBOX_DEFAULTS,
  LINE_DEFAULTS,
  REFERENCE_LINE_DEFAULTS,
  applyDefaults,
} from './chartDefaults';

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
  hasToolbox: boolean = false
) {
  const defaultGrid = {
    show: false, // Hide grid border to remove the square frame
    left: '3%',
    right: '4%',
    top: hasToolbox ? 40 : 15,
    bottom: 50, // Space for legend below axis labels
    containLabel: true,
    borderWidth: 0, // Ensure no border is drawn
  };

  if (!cartesianGrid) return defaultGrid;

  // Apply defaults for cartesianGrid
  const grid = applyDefaults(cartesianGrid, CARTESIAN_GRID_DEFAULTS);

  return {
    ...defaultGrid,
    show: grid.vertical || grid.horizontal,
    ...(grid.width != null && { width: grid.width }),
    ...(grid.height != null && { height: grid.height }),
    ...(grid.x != null && { x: grid.x }),
    ...(grid.y != null && { y: grid.y }),
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

  // Apply defaults for legend
  const leg = applyDefaults(legend, LEGEND_DEFAULTS);

  return {
    ...defaultLegends,
    icon: leg.iconType ? leg.iconType : 'rect',
    itemWidth: leg.iconSize ?? LEGEND_DEFAULTS.iconSize,
    itemHeight: leg.iconSize ?? LEGEND_DEFAULTS.iconSize,
    orient: leg.layout?.toLowerCase(),
    top: leg.verticalAlign === 'Bottom' ? 'bottom' : 'top',
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
  referenceDots?: ReferenceDot[],
  referenceLines?: MarkLine[],
  referenceAreas?: MarkArea[]
) => {
  // Convert ReferenceDot[] to ECharts markPoint format
  const markPoint =
    referenceDots && referenceDots.length > 0
      ? {
          data: referenceDots.map(d => ({
            coord: [d.x, d.y],
            name: d.label,
          })),
        }
      : {};

  // Merge MarkLine[] into single markLine config with defaults
  const markLine =
    referenceLines && referenceLines.length > 0
      ? {
          ...referenceLines[0],
          lineStyle: {
            width:
              referenceLines[0]?.lineStyle?.width ??
              REFERENCE_LINE_DEFAULTS.strokeWidth,
            ...referenceLines[0]?.lineStyle,
          },
          data: referenceLines.flatMap(ml => ml.data),
        }
      : {};

  // Merge MarkArea[] into single markArea config
  const markArea =
    referenceAreas && referenceAreas.length > 0
      ? {
          ...referenceAreas[0],
          data: referenceAreas.flatMap(ma => ma.data),
        }
      : {};

  return valueKeys.map((key, i) => {
    const rawLineConfig = lines?.[i];
    // Apply defaults for line config
    const lineConfig = rawLineConfig
      ? applyDefaults(rawLineConfig, LINE_DEFAULTS)
      : LINE_DEFAULTS;

    return {
      name: lineConfig.name || key,
      type: ChartType.Line,
      data: data.map(d =>
        transform ? transform(Number(d[key] ?? 0)) : Number(d[key] ?? 0)
      ),
      step: lineConfig.curveType === 'Step' ? 'middle' : false,
      smooth: lineConfig.curveType === 'Natural' ? true : false,
      showSymbol: true,
      symbolSize: 6,
      lineStyle: {
        width: lineConfig.strokeWidth ?? LINE_DEFAULTS.strokeWidth,
        opacity: 0.9,
        type: lineConfig.strokeDashArray ? 'dashed' : 'solid',
        color: lineConfig.stroke ?? undefined,
      },
      emphasis: {
        focus: 'series',
        disabled: true,
        lineStyle: { width: 3, opacity: 1 },
        itemStyle: { borderWidth: 2, borderColor: 'var(--background, #fff)' },
      },
      blur: { lineStyle: { opacity: 0.6 } },
      animation: true,
      animationDuration: 800,
      connectNulls: lineConfig.connectNulls ?? LINE_DEFAULTS.connectNulls,
      markPoint,
      markLine,
      markArea,
    };
  });
};

export const generateXAxis = (
  chartType: string,
  categories: string[],
  xAxis?: XAxisProps[],
  isVertical?: boolean,
  themeColors?: { mutedForeground: string; fontSans: string },
  cartesianGrid?: CartesianGridProps
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
      color: cartesianGrid?.stroke ?? themeColors?.mutedForeground,
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
  themeColors?: { mutedForeground: string; fontSans: string },
  cartesianGrid?: CartesianGridProps
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
    ...(largeSpread && { min: safeTransform(minValue) }),
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
        color: cartesianGrid?.stroke ?? themeColors?.mutedForeground,
        opacity: 0.4,
      },
    },
  };
};

export const generateTooltip = (
  tooltip?: ToolTipProps,
  type?: string,
  themeColors?: {
    foreground: string;
    fontSans: string;
    background: string;
    mutedForeground?: string;
  }
) => {
  // Apply defaults for tooltip
  const tip = tooltip
    ? applyDefaults(tooltip, TOOLTIP_DEFAULTS)
    : TOOLTIP_DEFAULTS;

  return {
    trigger: type === 'item' ? 'item' : 'axis',
    appendToBody: true,
    axisPointer: {
      type: type === 'item' ? 'cross' : (type ?? 'cross'),
      animated: tip.animated ?? TOOLTIP_DEFAULTS.animated,
      shadowStyle: { opacity: 0.5 },
      lineStyle: {
        color: themeColors?.mutedForeground,
      },
      crossStyle: {
        color: themeColors?.mutedForeground,
      },
      label: {
        backgroundColor: themeColors?.mutedForeground,
        color: 'rgba(255, 255, 255, 0.9)',
      },
    },
    textStyle: generateTextStyle(
      themeColors?.foreground,
      themeColors?.fontSans
    ),
    backgroundColor: themeColors?.background || 'rgba(255, 255, 255, 0.9)',
    borderColor: themeColors?.foreground || '#000',
    borderWidth: 1,
  };
};

export const generateEChartToolbox = (toolbox?: ToolboxProps) => {
  if (!toolbox || toolbox.enabled === false) {
    return { show: false };
  }

  const box = applyDefaults(toolbox, TOOLBOX_DEFAULTS);

  const features: ToolboxFeatures = {};

  if (box.dataView !== false) {
    const cardBg = getComputedStyle(document.documentElement)
      .getPropertyValue('--card')
      .trim();
    const foreground = getComputedStyle(document.documentElement)
      .getPropertyValue('--foreground')
      .trim();
    const mutedForeground = getComputedStyle(document.documentElement)
      .getPropertyValue('--muted-foreground')
      .trim();

    features.dataView = {
      show: true,
      readOnly: false,
      backgroundColor: cardBg,
      textareaColor: cardBg,
      textColor: foreground,
      buttonColor: mutedForeground,
      buttonTextColor: 'rgba(255, 255, 255, 0.9)',
    };
  }

  if (box.magicType !== false) {
    features.magicType = {
      show: true,
      type: ['line', 'bar'],
    };
  }

  if (box.saveAsImage !== false) {
    features.saveAsImage = {
      show: true,
    };
  }

  return {
    show: true,
    orient:
      box.orientation?.toLowerCase() === 'vertical' ? 'vertical' : 'horizontal',
    left:
      box.align?.toLowerCase() === 'left'
        ? 'left'
        : box.align?.toLowerCase() === 'center'
          ? 'center'
          : 'right',
    top:
      box.verticalAlign?.toLowerCase() === 'top'
        ? 0
        : box.verticalAlign?.toLowerCase() === 'middle'
          ? 'middle'
          : 'bottom',
    feature: features,
    iconStyle: {
      borderColor: getComputedStyle(document.documentElement)
        .getPropertyValue('--muted-foreground')
        .trim(),
    },
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
