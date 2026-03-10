import React, { useMemo } from 'react';
import {
  ColorScheme,
  generateTooltip,
  generateTextStyle,
  generateXAxis,
  generateYAxis,
} from './sharedUtils';
import {
  generateDataProps,
  generateEChartGrid,
  generateEChartLegend,
  generateEChartToolbox,
  getColors,
} from './sharedUtils';
import { useThemeWithMonitoring } from '@/components/theme-provider';
import { getHeight, getWidth } from '@/lib/styles';
import ReactECharts from 'echarts-for-react';
import { getChartThemeColors } from './styles';
import {
  BarProps,
  CartesianGridProps,
  ChartType,
  LegendProps,
  MarkArea,
  MarkLine,
  ReferenceDot,
  ToolTipProps,
  ToolboxProps,
  XAxisProps,
  YAxisProps,
} from './chartTypes';
import { ChartData } from './chartTypes';
import {
  BAR_DEFAULTS,
  REFERENCE_LINE_DEFAULTS,
  applyDefaults,
} from './chartDefaults';

const EMPTY_ARRAY: never[] = [];

interface BarChartWidgetProps {
  id: string;
  data: ChartData[];
  width?: string;
  height?: string;
  bars?: BarProps[];
  cartesianGrid?: CartesianGridProps;
  xAxis?: XAxisProps[];
  yAxis?: YAxisProps[];
  tooltip?: ToolTipProps;
  legend?: LegendProps;
  toolbox?: ToolboxProps;
  referenceLines?: MarkLine[];
  referenceAreas?: MarkArea[];
  referenceDots?: ReferenceDot[];
  colorScheme: ColorScheme;
  barGap?: number;
  barCategoryGap?: number | string;
  maxBarSize?: number;
  reverseStackOrder?: boolean;
  layout?: 'Horizontal' | 'Vertical';
}

const BarChartWidget: React.FC<BarChartWidgetProps> = ({
  data = EMPTY_ARRAY,
  width = 'Full',
  height = 'Full',
  bars,
  cartesianGrid,
  xAxis = EMPTY_ARRAY,
  yAxis = EMPTY_ARRAY,
  tooltip,
  legend,
  toolbox,
  referenceLines = EMPTY_ARRAY,
  referenceAreas = EMPTY_ARRAY,
  referenceDots = EMPTY_ARRAY,
  colorScheme = 'Default',
  //stackOffset,
  barGap = 4,
  barCategoryGap = '10%',
  maxBarSize,
  reverseStackOrder,
  layout = 'Vertical',
}) => {
  // Use enhanced theme hook with automatic monitoring
  const { colors, isDark } = useThemeWithMonitoring({
    monitorDOM: false, // Disabled to prevent excessive re-renders from MutationObserver
    monitorSystem: true, // Keep system theme monitoring for light/dark mode switching
  });

  // Extract chart-specific theme colors
  const themeColors = useMemo(
    () => getChartThemeColors(colors, isDark),
    [colors, isDark]
  );

  // When height is Full (100%), use flex to expand. Otherwise use explicit height.
  const heightStyle = height ? getHeight(height) : {};
  const isFull = height?.toLowerCase().startsWith('full');

  const styles: React.CSSProperties = {
    ...getWidth(width),
    position: 'relative',
    ...(isFull
      ? { display: 'flex', flexDirection: 'column', height: '100%' }
      : {}),
  };

  const chartStyles: React.CSSProperties = {
    ...(isFull
      ? { flex: 1, minHeight: '200px' }
      : { ...heightStyle, minHeight: '200px' }),
    width: '100%',
  };

  const { categories, valueKeys, transform, largeSpread, minValue, maxValue } =
    generateDataProps(data);

  // Chart colors depend on theme (chromatic colors automatically adapt to light/dark mode)
  const chartColors = useMemo(
    () => getColors(colorScheme, colors),
    [colorScheme, colors]
  );

  // Convert ReferenceDot[] to ECharts markPoint format
  const markPoint = useMemo(
    () =>
      referenceDots.length > 0
        ? {
            label: { show: true },
            data: referenceDots.map(d => ({
              coord: [d.x, d.y],
              name: d.label,
            })),
          }
        : { label: { show: false } },
    [referenceDots]
  );

  // Merge MarkLine[] into single markLine config with C# defaults
  const markLine = useMemo(
    () =>
      referenceLines.length > 0
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
        : {},
    [referenceLines]
  );

  // Merge MarkArea[] into single markArea config
  const markAreaConfig = useMemo(
    () =>
      referenceAreas.length > 0
        ? {
            ...referenceAreas[0],
            data: referenceAreas.flatMap(ma => ma.data),
          }
        : {},
    [referenceAreas]
  );

  // Memoize series configuration
  const series = useMemo(
    () =>
      valueKeys.map((key, i) => {
        const rawBarConfig = bars?.[i];
        // Apply C# defaults for bar config
        const barConfig = rawBarConfig
          ? applyDefaults(rawBarConfig, BAR_DEFAULTS)
          : BAR_DEFAULTS;

        return {
          name: key,
          type: ChartType.Bar,
          legendHoverLink: true,
          showBackground: true,
          data: data.map(d => d[key]),
          stack:
            barConfig.stackId !== undefined
              ? String(barConfig.stackId)
              : undefined,
          barGap: barGap ? `${barGap}%` : '4%',
          barCategoryGap: barCategoryGap ? `${barCategoryGap}%` : '10%',
          barMaxWidth: maxBarSize,
          stackOrder: reverseStackOrder ? 'seriesDesc' : 'seriesAsc',
          itemStyle: {
            borderRadius: barConfig.radius ?? BAR_DEFAULTS.radius,
            borderColor: barConfig.stroke ?? undefined,
            borderWidth: barConfig.strokeWidth ?? BAR_DEFAULTS.strokeWidth,
            color: barConfig.fill ?? undefined,
            opacity: barConfig.fillOpacity ?? undefined,
          },
          markPoint,
          markLine,
          markArea: markAreaConfig,
        };
      }),
    [
      valueKeys,
      data,
      bars,
      barGap,
      barCategoryGap,
      maxBarSize,
      reverseStackOrder,
      markPoint,
      markLine,
      markAreaConfig,
    ]
  );

  const isVertical = layout?.toLowerCase() === 'vertical';

  // Memoize option configuration
  const option = useMemo(
    () => ({
      grid: generateEChartGrid(
        cartesianGrid,
        !!toolbox && toolbox.enabled !== false
      ),
      color: chartColors,
      textStyle: generateTextStyle(
        themeColors.foreground,
        themeColors.fontSans
      ),
      xAxis: generateXAxis(ChartType.Bar, categories, xAxis, isVertical, {
        mutedForeground: themeColors.mutedForeground,
        fontSans: themeColors.fontSans,
      }),
      yAxis: generateYAxis(
        largeSpread,
        transform,
        minValue,
        maxValue,
        yAxis,
        isVertical,
        categories,
        {
          mutedForeground: themeColors.mutedForeground,
          fontSans: themeColors.fontSans,
        }
      ),
      series,
      legend: generateEChartLegend(legend, {
        foreground: themeColors.foreground,
        fontSans: themeColors.fontSans,
      }),
      tooltip: generateTooltip(tooltip, 'shadow', {
        foreground: themeColors.foreground,
        fontSans: themeColors.fontSans,
        background: themeColors.background,
        mutedForeground: themeColors.mutedForeground,
      }),
      toolbox: generateEChartToolbox(toolbox),
    }),
    [
      cartesianGrid,
      chartColors,
      themeColors,
      categories,
      xAxis,
      isVertical,
      largeSpread,
      transform,
      minValue,
      maxValue,
      yAxis,
      series,
      legend,
      tooltip,
      toolbox,
    ]
  );

  return (
    <div style={styles}>
      <ReactECharts
        option={option}
        style={chartStyles}
        notMerge={true} // Merge changes instead of full rebuild for better performance
        lazyUpdate={true}
      />
    </div>
  );
};

export default BarChartWidget;
