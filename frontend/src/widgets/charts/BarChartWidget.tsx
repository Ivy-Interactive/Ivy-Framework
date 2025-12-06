import React, { useMemo, useRef, useEffect, useState } from 'react';
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
  referenceLines?: MarkLine;
  referenceAreas?: MarkArea;
  referenceDots?: ReferenceDot;
  colorScheme: ColorScheme;
  barGap?: number;
  barCategoryGap?: number | string;
  maxBarSize?: number;
  reverseStackOrder?: boolean;
  layout?: 'horizontal' | 'vertical';
}

const BarChartWidget: React.FC<BarChartWidgetProps> = ({
  data,
  width,
  height,
  bars,
  cartesianGrid,
  xAxis,
  yAxis,
  tooltip,
  legend,
  toolbox,
  referenceLines,
  referenceAreas,
  referenceDots,
  colorScheme,
  //stackOffset,
  barGap,
  barCategoryGap,
  maxBarSize,
  reverseStackOrder,
  layout,
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

  const containerRef = useRef<HTMLDivElement>(null);
  const [explicitDimensions, setExplicitDimensions] = useState<{
    width: number;
    height: number;
  } | null>(null);

  // Measure container dimensions and set explicit size before rendering
  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    const measure = () => {
      const rect = container.getBoundingClientRect();
      if (rect.width === 0) {
        // Retry if width not available yet
        requestAnimationFrame(measure);
        return;
      }

      let measuredHeight = rect.height;
      // If height is 0, check parent for percentage-based heights
      if (measuredHeight === 0 && container.parentElement) {
        const parentRect = container.parentElement.getBoundingClientRect();
        const computedStyle = window.getComputedStyle(container);
        if (
          computedStyle.height === '100%' ||
          computedStyle.height.includes('%')
        ) {
          measuredHeight = parentRect.height;
        }
      }

      // Set dimensions even if height is 0 (use fallback)
      const finalHeight = measuredHeight > 0 ? measuredHeight : 400;
      setExplicitDimensions({
        width: rect.width,
        height: finalHeight,
      });
    };

    // Wait for layout, then measure
    requestAnimationFrame(() => {
      requestAnimationFrame(measure);
    });
  }, [height]);

  // Build container styles with explicit dimensions once measured
  const containerStyles: React.CSSProperties = {
    ...getWidth(width),
    ...(explicitDimensions
      ? { height: `${explicitDimensions.height}px` }
      : getHeight(height)),
  };

  // ReactECharts needs explicit sizing to fill its container
  const chartStyles: React.CSSProperties = {
    width: '100%',
    height: '100%',
  };

  const { categories, valueKeys, transform, largeSpread, minValue, maxValue } =
    generateDataProps(data);

  // Chart colors depend on theme (--chart-1 through --chart-5 change for light/dark)
  const chartColors = useMemo(
    () => getColors(colorScheme, colors),
    [colorScheme, colors]
  );

  // Memoize series configuration
  const series = useMemo(
    () =>
      valueKeys.map((key, i) => ({
        name: key,
        type: ChartType.Bar,
        legendHoverLink: true,
        showBackground: true,
        data: data.map(d => d[key]),
        stack:
          bars && bars[i]?.stackId !== undefined
            ? String(bars[i].stackId)
            : undefined,
        barGap: barGap ? `${barGap}%` : '4%',
        barCategoryGap: barCategoryGap ? `${barCategoryGap}%` : '10%',
        barMaxWidth: maxBarSize,
        stackOrder: reverseStackOrder ? 'seriesDesc' : 'seriesAsc',
        markPoint: {
          label: {
            show: referenceDots ? true : false,
          },
        },
        markLine: referenceLines ?? {},
        markArea: referenceAreas ?? {},
      })),
    [
      valueKeys,
      data,
      bars,
      barGap,
      barCategoryGap,
      maxBarSize,
      reverseStackOrder,
      referenceDots,
      referenceLines,
      referenceAreas,
    ]
  );

  const isVertical = layout?.toLowerCase() === 'vertical';

  // Memoize option configuration
  const option = useMemo(
    () => ({
      grid: generateEChartGrid(cartesianGrid),
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
    <div ref={containerRef} style={containerStyles}>
      {explicitDimensions && (
        <ReactECharts
          key={`chart-${explicitDimensions.width}-${explicitDimensions.height}`}
          option={option}
          style={chartStyles}
          opts={{
            width: explicitDimensions.width,
            height: explicitDimensions.height,
          }}
          notMerge={true}
          lazyUpdate={true}
        />
      )}
    </div>
  );
};

export default BarChartWidget;
