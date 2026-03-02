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

  const newData = useMemo(
    () =>
      data.map(d => ({
        value: d.measure as number,
        name: d.dimension as string,
      })),
    [data]
  );

  const maxVal = useMemo(
    () => Math.max(...newData.map(d => d.value || 0), 1),
    [newData]
  );

  const series = useMemo(
    () =>
      valueKeys.map(key => {
        const rawFunnelConfig = funnels?.find(
          a => a.dataKey.toLowerCase() === key
        );
        const funnelConfig = rawFunnelConfig
          ? { ...FUNNEL_DEFAULTS, ...rawFunnelConfig }
          : FUNNEL_DEFAULTS;

        return {
          name: key.charAt(0).toUpperCase() + key.slice(1),
          type: ChartType.Funnel,
          orient: 'horizontal',
          left: '10%',
          top: 100,
          bottom: legend ? 60 : 20,
          width: '80%',
          min: 0,
          minSize: '0%',
          maxSize: '100%',
          sort: 'descending',
          gap: 0,
          animation: funnelConfig.animated ?? true,
          label: {
            show: true,
            position: 'inside',
            formatter: function (params: any) {
              let currentMax = maxVal;
              try {
                const chart = chartRef.current?.getEchartsInstance();
                if (chart) {
                  const opts = chart.getOption() as any;
                  if (opts?.series?.[0]?.data) {
                    const sData = opts.series[0].data;
                    if (Array.isArray(sData)) {
                      currentMax = Math.max(
                        ...sData.map((d: any) => Number(d.value) || 0),
                        1
                      );
                    }
                  }
                }
              } catch (e) {
                // Ignore errors related to checking echarts instance
              }
              const percent = Math.round(
                (Number(params.value) / currentMax) * 100
              );
              return `${percent}%`;
            },
            color: '#fff',
            fontSize: 14,
          },
          labelLine: {
            show: false,
          },
          itemStyle: {
            color: funnelConfig.fill ?? undefined,
            opacity: funnelConfig.fillOpacity ?? undefined,
            borderColor: funnelConfig.stroke ?? '#fff',
            borderWidth: funnelConfig.strokeWidth ?? 1,
          },
          emphasis: {
            label: {
              fontSize: 20,
            },
          },
          data: newData,
        };
      }),
    [valueKeys, funnels, newData, legend, maxVal]
  );

  const option = useMemo(() => {
    const leg = legend ? applyDefaults(legend, FUNNEL_LEGEND_DEFAULTS) : null;

    return {
      color: chartColors,
      grid: {
        left: '10%',
        width: '80%',
        top: 100,
        bottom: leg ? 60 : 20,
        containLabel: false,
      },
      xAxis: {
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
            color: isDark ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)',
          },
        },
        axisLabel: {
          show: true,
          formatter: function (value: number) {
            let currentData = newData;
            try {
              const chart = chartRef.current?.getEchartsInstance();
              if (chart) {
                const opts = chart.getOption() as any;
                if (opts?.series?.[0]?.data) {
                  const sData = opts.series[0].data;
                  if (Array.isArray(sData)) {
                    currentData = sData;
                  }
                }
              }
            } catch (e) {
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
  }, [chartColors, legend, themeColors, series, toolbox]);

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
