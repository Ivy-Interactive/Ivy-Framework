import React, { useMemo, useRef, useEffect, useState } from 'react';
import ReactECharts from 'echarts-for-react';
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
  width = 'Full',
  height = 'Full',
  lines = [],
  cartesianGrid,
  xAxis = [],
  yAxis = [],
  tooltip,
  legend,
  toolbox,
  referenceLines = [],
  referenceAreas = [],
  referenceDots = [],
  colorScheme = 'Default',
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

  // Check if height should fill parent (full or percentage-based)
  const shouldFillParent =
    !height ||
    height === 'full' ||
    height.includes('full:') ||
    height.includes('fraction:');

  // Always measure container dimensions for ReactECharts
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
      // If height is 0, check parent for percentage-based heights or when filling parent
      if (measuredHeight === 0 && container.parentElement) {
        const parentRect = container.parentElement.getBoundingClientRect();
        const computedStyle = window.getComputedStyle(container);
        if (
          shouldFillParent ||
          computedStyle.height === '100%' ||
          computedStyle.height.includes('%')
        ) {
          measuredHeight = parentRect.height;
        }
      }

      // Set dimensions - use measured height or fallback
      const finalHeight = measuredHeight > 0 ? measuredHeight : 400;
      setExplicitDimensions({
        width: rect.width,
        height: finalHeight,
      });
    };

    // Wait for layout, then measure (multiple frames for parent layout to settle)
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        requestAnimationFrame(measure);
      });
    });
  }, [height, shouldFillParent]);

  // Build container styles - use explicit dimensions or CSS-based sizing
  const containerStyles: React.CSSProperties = {
    ...getWidth(width),
    ...(shouldFillParent
      ? {
          ...getHeight(height),
          display: 'flex',
          flexDirection: 'column',
          minHeight: 0,
        }
      : explicitDimensions
        ? { height: `${explicitDimensions.height}px` }
        : getHeight(height)),
  };

  // ReactECharts needs explicit sizing to fill its container
  const chartStyles: React.CSSProperties = {
    width: '100%',
    height: '100%',
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
        mutedForeground: themeColors.mutedForeground,
      }),
      toolbox: generateEChartToolbox(toolbox),
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

  return (
    <div ref={containerRef} style={containerStyles}>
      {(explicitDimensions || shouldFillParent) && (
        <ReactECharts
          key={
            explicitDimensions
              ? `chart-${explicitDimensions.width}-${explicitDimensions.height}`
              : 'chart-fill'
          }
          option={option}
          style={chartStyles}
          opts={
            explicitDimensions
              ? {
                  width: explicitDimensions.width,
                  height: explicitDimensions.height,
                }
              : undefined
          }
          notMerge={true}
          lazyUpdate={true}
        />
      )}
    </div>
  );
};

export default LineChartWidget;
