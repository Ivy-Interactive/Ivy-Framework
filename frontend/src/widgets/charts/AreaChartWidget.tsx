import React, { useMemo, useRef, useEffect, useState } from 'react';
import { ColorScheme, generateEChartToolbox } from './sharedUtils';
import { getHeight, getWidth } from '@/lib/styles';
import { useThemeWithMonitoring } from '@/components/theme-provider';
import ReactECharts from 'echarts-for-react';
import {
  generateDataProps,
  getColors,
  generateXAxis,
  generateEChartLegend,
  generateTooltip,
  generateTextStyle,
  generateEChartGrid,
  generateYAxis,
} from './sharedUtils';
import { generateGradientColors, getChartThemeColors } from './styles';
import {
  ChartType,
  XAxisProps,
  YAxisProps,
  LinesProps,
  MarkLine,
  MarkArea,
  LegendProps,
  CartesianGridProps,
  ToolTipProps,
  ToolboxProps,
} from './chartTypes';
import { ChartData } from './chartTypes';
import { getTransformValueFn } from './sharedUtils';
import { ReferenceDot } from './chartTypes';

interface AreaChartWidgetProps {
  id: string;
  data: ChartData[];
  width?: string;
  height?: string;
  areas?: LinesProps[];
  cartesianGrid?: CartesianGridProps;
  xAxis?: XAxisProps[];
  yAxis?: YAxisProps[];
  tooltip?: ToolTipProps;
  toolbox?: ToolboxProps;
  legend?: LegendProps;
  referenceLines?: MarkLine;
  referenceAreas?: MarkArea;
  referenceDots?: ReferenceDot;
  colorScheme: ColorScheme;
}

const AreaChartWidget: React.FC<AreaChartWidgetProps> = ({
  data,
  width,
  height,
  areas,
  cartesianGrid,
  xAxis,
  yAxis,
  tooltip,
  toolbox,
  legend,
  referenceLines,
  referenceAreas,
  referenceDots,
  colorScheme,
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

  const { categories, valueKeys } = generateDataProps(data);

  // Chart colors depend on theme (--chart-1 through --chart-5 change for light/dark)
  const chartColors = useMemo(
    () => getColors(colorScheme, colors),
    [colorScheme, colors]
  );

  const { transform, largeSpread, minValue, maxValue } =
    getTransformValueFn(data);

  // Memoize gradient colors
  const gradientColors = useMemo(
    () => generateGradientColors(chartColors, 0.4),
    [chartColors]
  );

  // Memoize series configuration
  const series = useMemo(
    () =>
      valueKeys.map((key, i) => {
        const areaConfig = areas?.find(a => a.dataKey.toLowerCase() === key);

        return {
          name: key,
          type: ChartType.Line,
          smooth: areaConfig?.curveType?.toLowerCase() === 'natural',
          lineStyle: {
            width: areaConfig?.strokeWidth ?? 2,
            color: areaConfig?.stroke ?? chartColors[i],
            type: areaConfig?.strokeDashArray ? 'dashed' : 'solid',
          },
          showSymbol: false,
          areaStyle: gradientColors[i],
          emphasis: { focus: 'series' },
          data: data.map(d => d[key]),
          markPoint: referenceDots ?? {},
          markLine: referenceLines ?? {},
          markArea: referenceAreas ?? {},
        };
      }),
    [
      valueKeys,
      areas,
      chartColors,
      gradientColors,
      data,
      referenceDots,
      referenceLines,
      referenceAreas,
    ]
  );

  // Memoize complete option configuration
  const option = useMemo(
    () => ({
      grid: generateEChartGrid(cartesianGrid),
      color: chartColors,
      tooltip: generateTooltip(tooltip, 'cross', {
        foreground: themeColors.foreground,
        fontSans: themeColors.fontSans,
        background: themeColors.background,
      }),
      legend: generateEChartLegend(legend, {
        foreground: themeColors.foreground,
        fontSans: themeColors.fontSans,
      }),
      toolbox: generateEChartToolbox(toolbox),
      textStyle: generateTextStyle(
        themeColors.foreground,
        themeColors.fontSans
      ),
      xAxis: generateXAxis(
        ChartType.Line,
        categories as string[],
        xAxis,
        false,
        {
          mutedForeground: themeColors.mutedForeground,
          fontSans: themeColors.fontSans,
        }
      ),
      yAxis: generateYAxis(
        largeSpread,
        transform,
        minValue,
        maxValue,
        yAxis,
        false,
        undefined,
        {
          mutedForeground: themeColors.mutedForeground,
          fontSans: themeColors.fontSans,
        }
      ),
      series: series,
    }),
    [
      cartesianGrid,
      chartColors,
      tooltip,
      themeColors.foreground,
      themeColors.fontSans,
      themeColors.background,
      themeColors.mutedForeground,
      legend,
      toolbox,
      categories,
      xAxis,
      largeSpread,
      transform,
      minValue,
      maxValue,
      yAxis,
      series,
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

export default AreaChartWidget;
