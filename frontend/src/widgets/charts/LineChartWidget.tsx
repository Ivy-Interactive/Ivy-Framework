import React, { useMemo } from 'react';
import ReactECharts from 'echarts-for-react';
import type { ECharts } from 'echarts';
import { getHeight, getWidth } from '@/lib/styles';
import { useThemeWithMonitoring } from '@/components/theme-provider';
import {
  generateDataProps,
  generateEChartGrid,
  generateEChartLegend,
  generateSeries,
  generateTooltip,
  generateTextStyle,
  generateXAxis,
  generateYAxis,
  getColors,
  getTransformValueFn,
  generateEChartToolbox,
} from './sharedUtils';
import { getChartThemeColors } from './styles';
import { LineChartWidgetProps, ChartType } from './chartTypes';

const LineChartWidget: React.FC<LineChartWidgetProps> = ({
  data,
  width,
  height,
  lines,
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

  const { categories, valueKeys } = generateDataProps(data);

  // Chart colors depend on theme (chromatic colors automatically adapt to light/dark mode)
  const chartColors = useMemo(
    () => getColors(colorScheme, colors),
    [colorScheme, colors]
  );

  const { transform, largeSpread, minValue, maxValue } =
    getTransformValueFn(data);

  // Memoize option configuration
  const option = useMemo(
    () => ({
      grid: generateEChartGrid(cartesianGrid),
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
      tooltip: generateTooltip(tooltip, 'shadow', {
        foreground: themeColors.foreground,
        fontSans: themeColors.fontSans,
        background: themeColors.background,
      }),
      toolbox: generateEChartToolbox(toolbox, false),
      legend: generateEChartLegend(legend, {
        foreground: themeColors.foreground,
        fontSans: themeColors.fontSans,
      }),
      textStyle: generateTextStyle(
        themeColors.foreground,
        themeColors.fontSans
      ),
      color: chartColors,
      series: generateSeries(
        data,
        valueKeys,
        lines,
        transform,
        referenceDots,
        referenceLines,
        referenceAreas
      ),
    }),
    [
      cartesianGrid,
      categories,
      xAxis,
      themeColors,
      largeSpread,
      transform,
      minValue,
      maxValue,
      yAxis,
      tooltip,
      legend,
      chartColors,
      data,
      valueKeys,
      lines,
      referenceDots,
      referenceLines,
      referenceAreas,
      toolbox,
    ]
  );

  // Show/hide toolbox on hover (directly via ECharts, no React re-renders)
  const onChartReady = (instance: ECharts) => {
    if (!toolbox) return;
    const dom = instance.getDom();
    if (!dom) return;

    dom.addEventListener('mouseenter', () =>
      instance.setOption(
        { toolbox: generateEChartToolbox(toolbox, true) },
        { replaceMerge: ['toolbox'], notMerge: false }
      )
    );
    dom.addEventListener('mouseleave', () =>
      instance.setOption(
        { toolbox: generateEChartToolbox(toolbox, false) },
        { replaceMerge: ['toolbox'], notMerge: false }
      )
    );
  };

  return (
    <div style={styles}>
      <ReactECharts
        option={option}
        style={chartStyles}
        notMerge={true}
        lazyUpdate={true}
        onChartReady={onChartReady}
      />
    </div>
  );
};

export default LineChartWidget;
