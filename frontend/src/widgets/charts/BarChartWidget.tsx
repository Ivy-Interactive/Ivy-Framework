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
      if (rect.width > 0) {
        // If we have a measured height, use it; otherwise use a minimum
        const finalHeight = measuredHeight > 0 ? measuredHeight : 400;
        setExplicitDimensions({
          width: rect.width,
          height: finalHeight,
        });
        // Render immediately, no delay
        setIsReady(true);
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
        const computedStyle = window.getComputedStyle(container);
        let measuredHeight = rect.height;
        if (measuredHeight === 0 && container.parentElement) {
          const parentRect = container.parentElement.getBoundingClientRect();
          const heightValue = computedStyle.height;
          if (
            heightValue &&
            (heightValue === '100%' || heightValue.includes('%'))
          ) {
            if (parentRect.height > 0) {
              measuredHeight = parentRect.height;
            }
          }
        }
        const currentHeight = measuredHeight > 0 ? measuredHeight : 400;
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

export default BarChartWidget;
