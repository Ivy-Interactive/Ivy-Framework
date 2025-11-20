import React, { useMemo, useCallback } from 'react';
import ReactECharts from 'echarts-for-react';
import { getHeight, getWidth } from '@/lib/styles';
import { useThemeWithMonitoring } from '@/components/theme-provider';
import { getChartThemeColors } from './styles';
import { HeatmapChartWidgetProps } from './chartTypes';

interface EChartsCallbackParams {
  data: [number, number, number, string];
  name: string;
}

const HeatmapChartWidget: React.FC<HeatmapChartWidgetProps> = ({
  data,
  width,
  height,
  metricType,
  title,
  showTotal,
  toolbox,
}) => {
  const { colors, isDark } = useThemeWithMonitoring({
    monitorDOM: false,
    monitorSystem: true,
  });

  const themeColors = useMemo(
    () => getChartThemeColors(colors, isDark),
    [colors, isDark]
  );

  // Format label helper - memoized
  const formatLabel = useCallback(
    (val: number): string => {
      if (metricType === 'percentage') return `${val}%`;
      if (metricType === 'currency') {
        return val >= 1000 ? `$${(val / 1000).toFixed(1)}k` : `$${val}`;
      }
      return val.toString();
    },
    [metricType]
  );

  // Styles - memoized
  const { containerStyles, chartStyles } = useMemo(() => {
    const heightStyle = height ? getHeight(height) : {};
    const isFull = height?.toLowerCase().startsWith('full');

    return {
      containerStyles: {
        ...getWidth(width),
        ...(isFull && {
          display: 'flex',
          flexDirection: 'column',
          height: '100%',
        }),
      } as React.CSSProperties,
      chartStyles: {
        ...(isFull
          ? { flex: 1, minHeight: '200px' }
          : { ...heightStyle, minHeight: '200px' }),
        width: '100%',
      } as React.CSSProperties,
    };
  }, [width, height]);

  // Process data - memoized
  const { periods, cohorts, combinedData, minValue, maxValue } = useMemo(() => {
    const uniquePeriods = Array.from(new Set(data.map(d => d.period))).sort(
      (a, b) => a - b
    );
    const filteredData = data.filter(d => d.cohort !== 'Total');
    const uniqueCohorts = Array.from(new Set(filteredData.map(d => d.cohort)));

    // Ensure Total is added to Y-axis if enabled
    const allCohorts = showTotal ? [...uniqueCohorts, 'Total'] : uniqueCohorts;

    // Map cohort data
    const mappedData = filteredData.map(d => [
      uniquePeriods.indexOf(d.period),
      uniqueCohorts.indexOf(d.cohort),
      d.value,
      d.label,
    ]);

    // Auto-calculate Total row
    if (showTotal) {
      const sums = new Map<number, number>();
      filteredData.forEach(d => {
        sums.set(d.period, (sums.get(d.period) || 0) + d.value);
      });

      const totalYIndex = uniqueCohorts.length;
      const totalPoints = Array.from(sums.entries()).map(([period, value]) => [
        uniquePeriods.indexOf(period),
        totalYIndex,
        value,
        formatLabel(value),
      ]);
      mappedData.push(...totalPoints);
    }

    // Calculate min/max from cohort data (excluding Total) for correct gradient scaling
    const dataValues = filteredData.map(d => d.value);
    const min = Math.min(...dataValues);
    const max = Math.max(...dataValues);

    return {
      periods: uniquePeriods,
      cohorts: allCohorts,
      combinedData: mappedData,
      minValue: min,
      maxValue: max,
    };
  }, [data, showTotal, formatLabel]);

  // Color palettes
  const { colorGradient } = useMemo(() => {
    const gradient = isDark
      ? ['#065f46', '#10b981', '#6ee7b7']
      : // Light Mode: Balanced gradient (Emerald 200 -> 400 -> 600)
        ['#a7f3d0', '#34d399', '#059669'];

    return {
      colorGradient: gradient,
    };
  }, [isDark]);

  // Common axis styles
  const axisStyle = useMemo(
    () => ({
      axisLabel: {
        color: themeColors.mutedForeground,
        fontFamily: themeColors.fontSans,
      },
      axisLine: {
        lineStyle: { color: themeColors.mutedForeground },
      },
    }),
    [themeColors]
  );

  // ECharts option - memoized
  const option = useMemo(() => {
    return {
      title: title
        ? {
            text: title,
            left: 'center',
            textStyle: {
              color: themeColors.foreground,
              fontFamily: themeColors.fontSans,
            },
          }
        : undefined,
      tooltip: {
        show: true,
        position: 'top',
        formatter: (params: EChartsCallbackParams) => {
          const val = params.data[2];
          const label = params.data[3];
          const formattedVal = label || formatLabel(val);
          return `${params.name}<br />Period ${periods[params.data[0]]}: ${formattedVal}`;
        },
        backgroundColor: themeColors.background,
        borderColor: themeColors.foreground,
        textStyle: {
          color: themeColors.foreground,
          fontFamily: themeColors.fontSans,
        },
      },
      toolbox: toolbox
        ? {
            show: true,
            feature: {
              saveAsImage: { show: true },
            },
          }
        : undefined,
      grid: {
        height: '85%',
        top: '15%',
        containLabel: true,
      },
      xAxis: {
        type: 'category',
        data: periods,
        position: 'top',
        splitArea: { show: true },
        ...axisStyle,
      },
      yAxis: {
        type: 'category',
        data: cohorts,
        inverse: true,
        splitArea: { show: true },
        ...axisStyle,
      },
      visualMap: {
        min: minValue,
        max: maxValue,
        show: false,
        seriesIndex: 0,
        dimension: 2,
        calculable: true,
        inRange: {
          color: colorGradient,
        },
      },
      series: [
        {
          name: 'Cohorts',
          type: 'heatmap',
          data: combinedData,
          label: {
            show: true,
            color: '#000000', // Keep labels black for visibility on light/medium colors
            formatter: (params: EChartsCallbackParams) =>
              params.data[3] || formatLabel(params.data[2]),
          },
          itemStyle: {
            borderColor: themeColors.background,
            borderWidth: 1,
          },
          emphasis: {
            itemStyle: {
              shadowBlur: 10,
              shadowColor: 'rgba(0, 0, 0, 0.5)',
            },
          },
        },
      ],
    };
  }, [
    periods,
    cohorts,
    combinedData,
    minValue,
    maxValue,
    themeColors,
    title,
    colorGradient,
    formatLabel,
    axisStyle,
    toolbox,
  ]);

  return (
    <div style={containerStyles}>
      <ReactECharts
        option={option}
        style={chartStyles}
        notMerge={true}
        lazyUpdate={true}
      />
    </div>
  );
};

export default HeatmapChartWidget;
