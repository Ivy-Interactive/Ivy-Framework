import React, { useMemo, useRef, useEffect, useState } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { useThemeWithMonitoring } from '@/components/theme-provider';
import ReactECharts from 'echarts-for-react';
import {
  getColors,
  generateTextStyle,
  generateEChartToolbox,
  generateTooltip,
} from './sharedUtils';
import { ChartType, PieChartWidgetProps } from './chartTypes';
import { generateDataProps } from './sharedUtils';
import { getChartThemeColors } from './styles';
import {
  PIE_DEFAULTS,
  PIE_LEGEND_DEFAULTS,
  applyDefaults,
} from './chartDefaults';

const PieChartWidget: React.FC<PieChartWidgetProps> = ({
  data,
  width = 'Full',
  height = 'Full',
  pies = [],
  tooltip,
  toolbox,
  legend,
  colorScheme = 'Default',
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

  const { valueKeys } = generateDataProps(data);

  // Chart colors depend on theme (chromatic colors automatically adapt to light/dark mode)
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
        const rawPieConfig = pies?.find(a => a.dataKey.toLowerCase() === key);
        // Apply C# defaults for pie config
        const pieConfig = rawPieConfig
          ? applyDefaults(rawPieConfig, PIE_DEFAULTS)
          : PIE_DEFAULTS;

        // Adjust vertical center based on total and legend presence
        let centerY = pieConfig.cy ?? '50%';
        if (!pieConfig.cy) {
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
            pieConfig.innerRadius ?? '40%',
            pieConfig.outerRadius ?? '70%',
          ],
          center: [pieConfig.cx ?? '50%', centerY],
          startAngle: pieConfig.startAngle ?? PIE_DEFAULTS.startAngle,
          endAngle: pieConfig.endAngle ?? PIE_DEFAULTS.endAngle,
          animation: pieConfig.animated ?? PIE_DEFAULTS.animated,
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
            color: pieConfig.fill ?? undefined,
            opacity: pieConfig.fillOpacity ?? undefined,
            borderColor: pieConfig.stroke ?? undefined,
            borderWidth: pieConfig.strokeWidth ?? PIE_DEFAULTS.strokeWidth,
          },
          data: newData,
        };
      }),
    [valueKeys, pies, newData, total, legend]
  );

  // Memoize option configuration
  const option = useMemo(() => {
    // Apply C# defaults for legend
    const leg = legend ? applyDefaults(legend, PIE_LEGEND_DEFAULTS) : null;

    return {
      color: chartColors,
      ...(leg && {
        legend: {
          orient:
            leg.layout?.toLowerCase() === 'vertical'
              ? 'vertical'
              : 'horizontal',
          left:
            leg.align?.toLowerCase() === 'left'
              ? 'left'
              : leg.align?.toLowerCase() === 'right'
                ? 'right'
                : 'center',
          top:
            leg.verticalAlign?.toLowerCase() === 'top'
              ? 'top'
              : leg.verticalAlign?.toLowerCase() === 'middle'
                ? 'middle'
                : 'bottom',
          icon: leg.iconType ?? 'circle',
          itemWidth: leg.iconSize ?? PIE_LEGEND_DEFAULTS.iconSize,
          itemHeight: leg.iconSize ?? PIE_LEGEND_DEFAULTS.iconSize,
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
        ...generateTooltip(tooltip, undefined, {
          foreground: themeColors.foreground,
          fontSans: themeColors.fontSans,
          background: themeColors.background,
          mutedForeground: themeColors.mutedForeground,
        }),
        trigger: 'item',
        formatter: '{a} <br/>{b}: {c} ({d}%)',
      },
      series: series,
      toolbox: generateEChartToolbox(
        toolbox && { ...toolbox, magicType: false }
      ),
    };
  }, [chartColors, legend, themeColors, tooltip, series, toolbox]);

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

export default PieChartWidget;
