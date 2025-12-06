import React, { useMemo, useRef, useEffect, useState } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { useThemeWithMonitoring } from '@/components/theme-provider';
import ReactECharts from 'echarts-for-react';
import {
  getColors,
  generateTextStyle,
  generateEChartToolbox,
} from './sharedUtils';
import { ChartType, PieChartWidgetProps } from './chartTypes';
import { generateDataProps } from './sharedUtils';
import { getChartThemeColors } from './styles';

const PieChartWidget: React.FC<PieChartWidgetProps> = ({
  data,
  width,
  height,
  pies,
  tooltip,
  toolbox,
  legend,
  colorScheme,
  total,
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

  const { valueKeys } = generateDataProps(data);

  // Chart colors depend on theme (--chart-1 through --chart-5 change for light/dark)
  const chartColors = useMemo(
    () => getColors(colorScheme, colors),
    [colorScheme, colors]
  );

  const newData = useMemo(
    () => data.map(d => ({ value: d.measure, name: d.dimension as string })),
    [data]
  );

  // Memoize series configuration
  const series = useMemo(
    () =>
      valueKeys.map(key => {
        const pieProperties = pies?.find(a => a.dataKey.toLowerCase() === key);

        // Adjust vertical center based on total and legend presence
        let centerY = pieProperties?.cy ?? '50%';
        if (!pieProperties?.cy) {
          // Only adjust if not explicitly set
          if (total && legend) {
            centerY = '45%'; // Both total and legend need space
          } else if (legend) {
            centerY = '45%'; // Legend at bottom needs space
          } else if (total) {
            centerY = '52%'; // Total at top, shift down slightly
          }
        }

        return {
          name: key.charAt(0).toUpperCase() + key.slice(1),
          type: ChartType.Pie,
          radius: [
            pieProperties?.innerRadius ?? '40%',
            pieProperties?.outerRadius ?? '70%',
          ],
          center: [pieProperties?.cx ?? '50%', centerY],
          startAngle: pieProperties?.startAngle ?? 90,
          endAngle: pieProperties?.endAngle ?? 450,
          animation: pieProperties?.animated ?? true,
          avoidLabelOverlap: false,
          label: {
            show: false,
            position: 'center',
          },
          emphasis: {
            disabled: false,
            scale: true,
            scaleSize: 5,
            focus: 'none',
            label: {
              show: false,
            },
          },
          labelLine: {
            show: false,
          },
          itemStyle: {
            color: pieProperties?.fill ?? undefined,
            opacity: pieProperties?.fillOpacity ?? undefined,
            borderColor: pieProperties?.stroke ?? undefined,
            borderWidth: pieProperties?.strokeWidth ?? undefined,
          },
          data: newData,
        };
      }),
    [valueKeys, pies, newData, total, legend]
  );

  // Memoize option configuration
  const option = useMemo(
    () => ({
      color: chartColors,
      ...(legend && {
        legend: {
          orient:
            legend.layout?.toLowerCase() === 'vertical'
              ? 'vertical'
              : 'horizontal',
          left:
            legend.align?.toLowerCase() === 'left'
              ? 'left'
              : legend.align?.toLowerCase() === 'right'
                ? 'right'
                : 'center',
          top:
            legend.verticalAlign?.toLowerCase() === 'top'
              ? 'top'
              : legend.verticalAlign?.toLowerCase() === 'middle'
                ? 'middle'
                : 'bottom',
          icon: legend.iconType ?? 'circle',
          itemWidth: legend.iconSize ?? 12,
          itemHeight: legend.iconSize ?? 12,
          type: 'scroll',
          textStyle: generateTextStyle(
            themeColors.foreground,
            themeColors.fontSans
          ),
        },
      }),
      textStyle: generateTextStyle(
        themeColors.foreground,
        themeColors.fontSans
      ),
      tooltip: {
        trigger: 'item',
        formatter: '{a} <br/>{b}: {c} ({d}%)',
        animated: tooltip?.animated ?? true,
        textStyle: generateTextStyle(
          themeColors.foreground,
          themeColors.fontSans
        ),
        backgroundColor: themeColors.background,
        borderColor: themeColors.foreground,
        borderWidth: 1,
      },
      series: series,
      toolbox: generateEChartToolbox(toolbox),
    }),
    [chartColors, legend, themeColors, tooltip, series, toolbox]
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

export default PieChartWidget;
