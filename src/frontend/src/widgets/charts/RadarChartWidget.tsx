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
import {
  RadarChartWidgetProps,
  RadarProps,
  RadarIndicatorProps,
} from './chartTypes';
import { getChartThemeColors } from './styles';
import { RADAR_DEFAULTS, applyDefaults } from './chartDefaults';
import { EMPTY_ARRAY } from '@/lib/constants';

// Case-insensitive property lookup to handle CamelCase JSON serialization
// C# indicator names (e.g. "Sales") may not match camelCase JSON keys (e.g. "sales")
const getPropertyValue = (
  obj: Record<string, unknown>,
  propName: string
): unknown => {
  if (propName in obj) return obj[propName];
  const lowerName = propName.toLowerCase();
  const key = Object.keys(obj).find(k => k.toLowerCase() === lowerName);
  return key ? obj[key] : undefined;
};

const RadarChartWidget: React.FC<RadarChartWidgetProps> = ({
  data = EMPTY_ARRAY,
  width = 'Full',
  height = 'Full',
  radars = EMPTY_ARRAY,
  indicators = EMPTY_ARRAY,
  tooltip,
  toolbox,
  legend,
  colorScheme = 'Default',
  shape = 'Polygon',
  cx = '50%',
  cy = '50%',
  radius = '75%',
  startAngle = 90,
  splitLine = true,
  splitArea = false,
  axisLine = true,
}) => {
  // Use enhanced theme hook with automatic monitoring
  const { colors, isDark } = useThemeWithMonitoring({
    monitorDOM: false,
    monitorSystem: true,
  });

  // Extract chart-specific theme colors
  const themeColors = useMemo(
    () => getChartThemeColors(colors, isDark),
    [colors, isDark]
  );

  // When height is Full (100%) use flex to expand. Otherwise use explicit height.
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

  const { valueKeys } = generateDataProps(data);

  // Chart colors depend on theme
  const chartColors = useMemo(
    () => getColors(colorScheme, colors),
    [colorScheme, colors]
  );

  // Build indicator configuration
  const radarIndicators = useMemo(() => {
    if (indicators.length > 0) {
      return indicators.map((ind: RadarIndicatorProps) => ({
        name: ind.name,
        max: ind.max,
        min: ind.min ?? 0,
      }));
    }
    // Auto-generate from data keys if not specified
    if (data.length > 0 && valueKeys.length > 0) {
      return valueKeys.map(key => ({
        name: key,
        max:
          Math.max(
            ...data.map((d: Record<string, unknown>) =>
              Number(getPropertyValue(d, key) || 0)
            )
          ) * 1.2,
      }));
    }
    return [];
  }, [indicators, data, valueKeys]);

  // Build series configuration
  const series = useMemo(() => {
    if (radars.length === 0 && data.length > 0 && radarIndicators.length > 0) {
      // Default: each data row becomes a series entry
      return [
        {
          type: 'radar' as const,
          data: data.map((item: Record<string, unknown>) => ({
            value: radarIndicators.map(ind =>
              Number(getPropertyValue(item, ind.name) || 0)
            ),
            name: (item.name || item.Name || 'Data') as string,
          })),
        },
      ];
    }

    // Map each radar config to a series
    return radars.map((rawRadar: RadarProps) => {
      const radar = applyDefaults(rawRadar, RADAR_DEFAULTS);
      const seriesData = data.map((item: Record<string, unknown>) => ({
        value: radarIndicators.map(ind =>
          Number(getPropertyValue(item, ind.name) || 0)
        ),
        name: (item.name || item.Name || radar.name || radar.dataKey) as string,
      }));

      return {
        type: 'radar' as const,
        name: radar.name || radar.dataKey,
        symbol: radar.showSymbol !== false ? 'circle' : 'none',
        data: seriesData.map((d: { value: number[]; name: string }) => ({
          ...d,
          areaStyle: radar.filled ? { opacity: 0.3 } : undefined,
          lineStyle: {
            type: radar.strokeDashArray ? 'dashed' : 'solid',
            width: radar.strokeWidth ?? RADAR_DEFAULTS.strokeWidth,
            color: radar.stroke ?? undefined,
          },
          itemStyle: {
            color: radar.fill ?? undefined,
          },
        })),
      };
    });
  }, [radars, data, radarIndicators]);

  // Memoize option configuration
  const option = useMemo(
    () => ({
      color: chartColors,
      radar: {
        center: [cx, legend ? '45%' : cy],
        radius: legend ? '65%' : radius,
        startAngle: startAngle,
        shape: shape.toLowerCase(),
        indicator: radarIndicators,
        splitNumber: 4,
        splitLine: {
          show: splitLine,
          lineStyle: {
            color: themeColors.mutedForeground,
          },
        },
        splitArea: {
          show: splitArea,
          areaStyle: {
            color: isDark
              ? ['rgba(255, 255, 255, 0.05)', 'rgba(255, 255, 255, 0.02)']
              : ['rgba(0, 0, 0, 0.05)', 'rgba(0, 0, 0, 0.02)'],
          },
        },
        axisLine: {
          show: axisLine,
          lineStyle: {
            color: themeColors.mutedForeground,
          },
        },
        axisLabel: {
          color: themeColors.foreground,
          fontFamily: themeColors.fontSans,
        },
        name: {
          textStyle: {
            color: themeColors.foreground,
            fontFamily: themeColors.fontSans,
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
    }),
    [
      chartColors,
      cx,
      cy,
      radius,
      startAngle,
      shape,
      radarIndicators,
      splitLine,
      splitArea,
      axisLine,
      themeColors,
      series,
      legend,
      tooltip,
      toolbox,
      isDark,
    ]
  );

  return (
    <div style={styles}>
      <ReactECharts
        option={option}
        style={chartStyles}
        notMerge={true}
        lazyUpdate={true}
      />
    </div>
  );
};

export default RadarChartWidget;
