import React, { useMemo, useRef } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { useThemeWithMonitoring } from '@/components/theme-provider';
import ReactECharts from 'echarts-for-react';
import {
  getColors,
  generateTextStyle,
  generateEChartToolbox,
} from './sharedUtils';
import { ChartType, FunnelChartWidgetProps } from './chartTypes';
import { generateDataProps } from './sharedUtils';
import { getChartThemeColors } from './styles';
import {
  applyDefaults,
  FUNNEL_DEFAULTS,
  PIE_LEGEND_DEFAULTS,
} from './chartDefaults';

// Using Pie Legend defaults for Funnel since it's a similar non-cartesian chart
const FUNNEL_LEGEND_DEFAULTS = PIE_LEGEND_DEFAULTS;

const FunnelChartWidget: React.FC<FunnelChartWidgetProps> = ({
  data = [],
  width = 'Full',
  height = 'Full',
  funnels = [],
  toolbox,
  legend,
  colorScheme = 'Default',
}) => {
  const chartRef = useRef<ReactECharts>(null);

  const { colors, isDark } = useThemeWithMonitoring({
    monitorDOM: false,
    monitorSystem: true,
  });

  const themeColors = useMemo(
    () => getChartThemeColors(colors, isDark),
    [colors, isDark]
  );

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
      ? { flex: 1, minHeight: '300px' }
      : { ...heightStyle, minHeight: '300px' }),
    width: '100%',
  };

  const { valueKeys } = generateDataProps(data);

  const chartColors = useMemo(
    () => getColors(colorScheme, colors),
    [colorScheme, colors]
  );

  const newData = data.map(d => ({
    value: d.measure as number,
    name: d.dimension as string,
  }));

  const maxVal = Math.max(...newData.map(d => d.value || 0), 1);

  const isHorizontal = useMemo(() => {
    if (!funnels || funnels.length === 0) return true;
    return funnels[0].orient?.toLowerCase() !== 'vertical';
  }, [funnels]);

  const series = valueKeys.flatMap(key => {
    const rawFunnelConfig = funnels?.find(a => a.dataKey.toLowerCase() === key);
    const funnelConfig = rawFunnelConfig
      ? { ...FUNNEL_DEFAULTS, ...rawFunnelConfig }
      : FUNNEL_DEFAULTS;

    const baseSeries = {
      name: key.charAt(0).toUpperCase() + key.slice(1),
      type: ChartType.Funnel,
      orient: isHorizontal ? 'horizontal' : 'vertical',
      left: '10%',
      top: isHorizontal ? 100 : 50,
      bottom: legend ? 60 : 20,
      width: isHorizontal ? '80%' : '70%',
      min: 0,
      minSize: '0%',
      maxSize: '100%',
      sort: 'descending',
      gap: 0,
      animation: funnelConfig.animated ?? true,
      data: newData,
    };

    const standardLabelFormatter = function (params: {
      value: number | string;
    }) {
      let currentMax = maxVal;
      try {
        const chart = chartRef.current?.getEchartsInstance();
        if (chart) {
          const opts = chart.getOption() as {
            series?: { data?: { value: number | string }[] }[];
          };
          if (opts?.series?.[0]?.data) {
            const sData = opts.series[0].data;
            if (Array.isArray(sData)) {
              currentMax = Math.max(
                ...sData.map(
                  (d: { value: number | string }) => Number(d.value) || 0
                ),
                1
              );
            }
          }
        }
      } catch {
        // ignore
      }
      const percent = Math.round((Number(params.value) / currentMax) * 100);
      return `${percent}%`;
    };

    if (isHorizontal) {
      return [
        {
          ...baseSeries,
          label: {
            show: true,
            position: 'inside',
            formatter: standardLabelFormatter,
            color: '#fff',
            fontSize: 14,
          },
          labelLine: { show: false },
          itemStyle: {
            color: funnelConfig.fill ?? undefined,
            opacity: funnelConfig.fillOpacity ?? undefined,
            borderColor: funnelConfig.stroke ?? '#fff',
            borderWidth: funnelConfig.strokeWidth ?? 1,
          },
          emphasis: { label: { fontSize: 20 } },
        },
      ];
    } else {
      return [
        {
          ...baseSeries,
          label: {
            show: true,
            position: 'inside',
            formatter: standardLabelFormatter,
            color: '#fff',
            fontSize: 14,
          },
          labelLine: { show: false },
          itemStyle: {
            color: funnelConfig.fill ?? undefined,
            opacity: funnelConfig.fillOpacity ?? undefined,
            borderColor: funnelConfig.stroke ?? (isDark ? '#000' : '#fff'),
            borderWidth: funnelConfig.strokeWidth ?? 1,
          },
          emphasis: { label: { fontSize: 20 } },
        },
        {
          ...baseSeries,
          name: baseSeries.name + ' (Labels)',
          label: {
            show: true,
            position: 'right',
            formatter: function (params: {
              value: number | string;
              name: string;
            }) {
              const valObj = Number(params.value);
              let displayVal = valObj.toString();
              if (valObj >= 1000) {
                displayVal =
                  (valObj / 1000).toFixed(valObj % 1000 === 0 ? 0 : 1) + 'K';
              }
              return `{name|${params.name}}\n{value|${displayVal}}`;
            },
            color: themeColors.foreground,
            fontSize: 14,
            rich: {
              name: {
                color: themeColors.mutedForeground,
                fontSize: 14,
                padding: [0, 0, 4, 0],
              },
              value: {
                color: themeColors.foreground,
                fontSize: 18,
                fontWeight: 'bold',
              },
            },
          },
          labelLine: {
            show: true,
            length: 40,
            lineStyle: {
              color: isDark ? 'rgba(255, 255, 255, 0.2)' : 'rgba(0, 0, 0, 0.2)',
              width: 1,
              type: 'solid',
            },
          },
          itemStyle: {
            color: 'transparent',
            borderColor: 'transparent',
            borderWidth: 0,
          },
          emphasis: { disabled: true },
        },
      ];
    }
  });

  const option = (() => {
    const leg = legend ? applyDefaults(legend, FUNNEL_LEGEND_DEFAULTS) : null;

    return {
      color: chartColors,
      grid: {
        left: '10%',
        width: isHorizontal ? '80%' : '70%',
        top: isHorizontal ? 100 : 50,
        bottom: leg ? 60 : 20,
        containLabel: false,
      },
      xAxis: isHorizontal
        ? {
            type: 'value',
            min: 0,
            max: newData.length,
            interval: 1,
            position: 'top',
            axisLine: { show: false },
            axisTick: { show: false },
            splitLine: {
              show: true,
              lineStyle: {
                type: 'solid',
                width: 1,
                color: isDark
                  ? 'rgba(255, 255, 255, 0.1)'
                  : 'rgba(0, 0, 0, 0.1)',
              },
            },
            axisLabel: {
              show: true,
              formatter: function (value: number) {
                let currentData = newData;
                try {
                  const chart = chartRef.current?.getEchartsInstance();
                  if (chart) {
                    const opts = chart.getOption() as {
                      series?: {
                        data?: { value: number | string; name: string }[];
                      }[];
                    };
                    if (opts?.series?.[0]?.data) {
                      const sData = opts.series[0].data;
                      if (Array.isArray(sData)) {
                        currentData = sData;
                      }
                    }
                  }
                } catch {
                  // Ignore errors related to checking echarts instance
                }

                if (value >= currentData.length) return '';
                const dataObj = currentData[value];
                let valObj = Number(dataObj.value);
                if (isNaN(valObj)) valObj = 0;

                let displayVal = valObj.toString();
                if (valObj >= 1000) {
                  displayVal =
                    (valObj / 1000).toFixed(valObj % 1000 === 0 ? 0 : 1) + 'K';
                }
                return `{name|${dataObj.name}}\n{value|${displayVal}}`;
              },
              rich: {
                name: {
                  color: themeColors.mutedForeground,
                  fontSize: 14,
                  align: 'left',
                  padding: [0, 0, 8, 8],
                },
                value: {
                  color: themeColors.foreground,
                  fontSize: 24,
                  fontWeight: 'bold',
                  align: 'left',
                  padding: [0, 0, 16, 8],
                },
              },
              margin: 10,
              align: 'left',
              verticalAlign: 'bottom',
            },
          }
        : {
            type: 'value',
            show: false,
          },
      yAxis: {
        type: 'value',
        show: false,
      },
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
                ? 'center' // Use 'center' strictly for proper vertical alignment in echarts legends
                : 'bottom',
          icon: leg.iconType ?? 'circle',
          itemWidth: leg.iconSize ?? FUNNEL_LEGEND_DEFAULTS.iconSize,
          itemHeight: leg.iconSize ?? FUNNEL_LEGEND_DEFAULTS.iconSize,
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
        show: false,
      },
      series: series,
      toolbox: generateEChartToolbox(
        toolbox && { ...toolbox, magicType: false }
      ),
    };
  })();

  return (
    <div style={styles}>
      <ReactECharts
        ref={chartRef}
        option={option}
        style={chartStyles}
        notMerge={true}
        lazyUpdate={true}
      />
    </div>
  );
};

export default FunnelChartWidget;
