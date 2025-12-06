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

  const containerRef = useRef<HTMLDivElement>(null);
  const [explicitDimensions, setExplicitDimensions] = useState<{
    width: number;
    height: number;
  } | null>(null);
  const [isReady, setIsReady] = useState(false);

  // Measure container and set explicit height before rendering chart
  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    let timeoutId: NodeJS.Timeout | null = null;
    let resizeObserver: ResizeObserver | null = null;

    const measureAndSetHeight = () => {
      if (isReady) return; // Already ready, skip

      const rect = container.getBoundingClientRect();
      const computedStyle = window.getComputedStyle(container);

      // Get available height from container or parent
      let measuredHeight = rect.height;

      // If container has no height yet, check parent for percentage-based heights
      if (measuredHeight === 0 && container.parentElement) {
        const parentRect = container.parentElement.getBoundingClientRect();
        const parentHeight = parentRect.height;

        // Check computed style to see if we're using percentage-based height
        const heightValue = computedStyle.height;
        if (
          heightValue &&
          (heightValue === '100%' || heightValue.includes('%'))
        ) {
          // For percentage heights, use parent's available height
          if (parentHeight > 0) {
            measuredHeight = parentHeight;
          }
        }
      }

      // Set dimensions and render if we have valid dimensions
      if (rect.width > 0 && measuredHeight > 0) {
        // Only render when we have BOTH valid width and height
        const finalHeight = measuredHeight;
        // Set dimensions first - this will update the container div's height
        setExplicitDimensions({
          width: rect.width,
          height: finalHeight,
        });
        // Wait for React to apply the explicit height to container, then render chart
        requestAnimationFrame(() => {
          requestAnimationFrame(() => {
            setIsReady(true);
          });
        });
        if (timeoutId) clearTimeout(timeoutId);
        // Keep resizeObserver active to handle dimension changes
        return true;
      }
      return false;
    };

    // Use ResizeObserver to detect when container gets dimensions
    // Keep it active to handle dimension changes (e.g., when switching tabs)
    resizeObserver = new ResizeObserver(() => {
      if (!isReady) {
        measureAndSetHeight();
      } else {
        // If already ready, check if dimensions changed significantly
        const rect = container.getBoundingClientRect();
        const currentHeight = rect.height > 0 ? rect.height : 400;
        setExplicitDimensions(prev => {
          if (!prev) return null;
          // Only update if dimensions changed significantly (>5px difference)
          if (
            Math.abs(prev.width - rect.width) > 5 ||
            Math.abs(prev.height - currentHeight) > 5
          ) {
            return {
              width: rect.width,
              height: currentHeight,
            };
          }
          return prev;
        });
      }
    });

    resizeObserver.observe(container);
    if (container.parentElement) {
      resizeObserver.observe(container.parentElement);
    }

    // Check immediately and aggressively
    const tryMeasure = () => {
      if (measureAndSetHeight()) return;
      // Try again on next frame
      requestAnimationFrame(() => {
        if (measureAndSetHeight()) return;
        // One more time after another frame
        requestAnimationFrame(measureAndSetHeight);
      });
    };

    tryMeasure();

    // Very fast fallback: render almost immediately
    timeoutId = setTimeout(() => {
      if (!isReady) {
        const rect = container.getBoundingClientRect();
        if (rect.width > 0) {
          const fallbackHeight = rect.height > 0 ? rect.height : 400;
          setExplicitDimensions({
            width: rect.width,
            height: fallbackHeight,
          });
          setIsReady(true);
        }
      }
    }, 50); // Reduced to 50ms

    return () => {
      if (resizeObserver) resizeObserver.disconnect();
      if (timeoutId) clearTimeout(timeoutId);
    };
  }, [height, isReady]);

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
      {isReady && explicitDimensions && (
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

export default LineChartWidget;
