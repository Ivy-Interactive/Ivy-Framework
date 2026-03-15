import React, { useMemo } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { useThemeWithMonitoring } from '@/components/theme-provider';
import ReactECharts from 'echarts-for-react';
import {
  getColors,
  generateTextStyle,
  generateEChartToolbox,
  generateTooltip,
  generateEChartLegend,
  generateDataProps,
} from './sharedUtils';
import { RadialBarChartWidgetProps } from './chartTypes';
import { getChartThemeColors } from './styles';
import {
  RADIAL_BAR_DEFAULTS,
  POLAR_GRID_DEFAULTS,
  POLAR_ANGLE_AXIS_DEFAULTS,
  POLAR_RADIUS_AXIS_DEFAULTS,
  applyDefaults,
} from './chartDefaults';

const EMPTY_ARRAY: never[] = [];

const RadialBarChartWidget: React.FC<RadialBarChartWidgetProps> = ({
  data = EMPTY_ARRAY,
  width = 'Full',
  height = 'Full',
  radialBars = EMPTY_ARRAY,
  tooltip,
  toolbox,
  legend,
  colorScheme = 'Default',
  polarAngleAxis,
  polarRadiusAxis,
  polarGrid,
  cx = '50%',
  cy = '50%',
  innerRadius = '30%',
  outerRadius = '80%',
  startAngle = 0,
  endAngle = 360,
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

  const { categories, valueKeys } = generateDataProps(data);

  // Chart colors depend on theme (chromatic colors automatically adapt to light/dark mode)
  const chartColors = useMemo(
    () => getColors(colorScheme, colors),
    [colorScheme, colors]
  );

  // Apply defaults to polar components
  const polarGridConfig = polarGrid
    ? applyDefaults(polarGrid, POLAR_GRID_DEFAULTS)
    : null;
  const polarAngleAxisConfig = polarAngleAxis
    ? applyDefaults(polarAngleAxis, POLAR_ANGLE_AXIS_DEFAULTS)
    : null;
  const polarRadiusAxisConfig = polarRadiusAxis
    ? applyDefaults(polarRadiusAxis, POLAR_RADIUS_AXIS_DEFAULTS)
    : null;

  // Memoize series configuration
  const series = useMemo(
    () =>
      valueKeys.map((key, i) => {
        const rawBarConfig = radialBars?.[i];
        // Apply C# defaults for radial bar config
        const barConfig = rawBarConfig
          ? applyDefaults(rawBarConfig, RADIAL_BAR_DEFAULTS)
          : RADIAL_BAR_DEFAULTS;

        return {
          name: barConfig.name ?? key,
          type: 'bar',
          coordinateSystem: 'polar',
          data: data.map(d => d[key]),
          animation: barConfig.animated ?? RADIAL_BAR_DEFAULTS.animated,
          showBackground:
            barConfig.background ?? RADIAL_BAR_DEFAULTS.background,
          backgroundStyle: {
            color: 'rgba(180, 180, 180, 0.2)',
          },
          itemStyle: {
            color: barConfig.fill ?? undefined,
          },
        };
      }),
    [valueKeys, data, radialBars]
  );

  // Memoize option configuration
  const option = useMemo(() => {
    // Build polar config incrementally to avoid circular reference
    const basePolar = {
      center: [cx, cy],
      radius: [innerRadius, outerRadius],
    };

    const polarConfig = polarGridConfig
      ? {
          ...basePolar,
          ...(polarGridConfig.gridType && {
            type: polarGridConfig.gridType.toLowerCase(),
          }),
        }
      : basePolar;

    return {
      color: chartColors,
      polar: polarConfig,
      angleAxis: {
        type: 'category',
        data: categories,
        startAngle: startAngle,
        endAngle: endAngle,
        axisLine: {
          show:
            polarAngleAxisConfig?.axisLine ??
            POLAR_ANGLE_AXIS_DEFAULTS.axisLine,
          lineStyle: {
            color: polarAngleAxisConfig?.stroke ?? themeColors.mutedForeground,
          },
        },
        axisTick: {
          show:
            polarAngleAxisConfig?.tickLine ??
            POLAR_ANGLE_AXIS_DEFAULTS.tickLine,
        },
        axisLabel: {
          color: themeColors.foreground,
          fontFamily: themeColors.fontSans,
        },
      },
      radiusAxis: {
        type: 'value',
        ...(polarRadiusAxisConfig?.angle !== null && {
          axisAngle: polarRadiusAxisConfig?.angle,
        }),
        ...(polarRadiusAxisConfig?.domain && {
          min: polarRadiusAxisConfig.domain[0],
          max: polarRadiusAxisConfig.domain[1],
        }),
        ...(polarRadiusAxisConfig?.tickCount && {
          splitNumber: polarRadiusAxisConfig.tickCount,
        }),
        axisLine: {
          lineStyle: {
            color: polarRadiusAxisConfig?.stroke ?? themeColors.mutedForeground,
          },
        },
        axisLabel: {
          color: themeColors.foreground,
          fontFamily: themeColors.fontSans,
        },
        splitLine: {
          lineStyle: {
            color: themeColors.mutedForeground,
          },
        },
      },
      series: series,
      legend: generateEChartLegend(legend, {
        foreground: themeColors.foreground,
        fontSans: themeColors.fontSans,
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
      },
      toolbox: generateEChartToolbox(
        toolbox && { ...toolbox, magicType: false }
      ),
    };
  }, [
    chartColors,
    cx,
    cy,
    innerRadius,
    outerRadius,
    categories,
    startAngle,
    endAngle,
    polarAngleAxisConfig,
    polarRadiusAxisConfig,
    polarGridConfig,
    themeColors,
    series,
    legend,
    tooltip,
    toolbox,
  ]);

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

export default RadialBarChartWidget;
