import React, { useMemo } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { useThemeWithMonitoring } from '@/components/theme-provider';
import ReactECharts from 'echarts-for-react';
import { getColors, generateTextStyle, generateTooltip } from './sharedUtils';
import { ChartType, SunburstChartWidgetProps, SunburstNodeProps } from './chartTypes';
import { getChartThemeColors } from './styles';

const mapDataToECharts = (nodes: SunburstNodeProps[], chartColors: string[], parentColor?: string, depth = 0): any[] => {
  return nodes.map((node, index) => {
    // Assign a color based on the top-level index if no parent color is provided, 
    // or use the explicitly set Fill color if available.
    const currentColor = node.fill || parentColor || chartColors[index % chartColors.length];

    return {
      name: node.name,
      value: node.value,
      itemStyle: {
        color: currentColor,
      },
      // Recursively map children, passing down the assigned color so children can calculate 
      // lighter/darker variations (handled implicitly by ECharts if we just set the same color, 
      // or we can let ECharts naturally gradient it. Providing the explicit color helps match theme).
      children: node.children && node.children.length > 0
        ? mapDataToECharts(node.children, chartColors, currentColor, depth + 1)
        : undefined
    };
  });
};

const SunburstChartWidget: React.FC<SunburstChartWidgetProps> = ({
  data = [],
  width = 'Full',
  height = 'Full',
  tooltip,
  colorScheme = 'Default',
  innerRadius,
  outerRadius,
  cx,
  cy,
  startAngle = 90,
  endAngle,
  padding = 2,
  ringPadding = 2,
  stroke = '#ffffff',
}) => {
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
      ? { flex: 1, minHeight: '200px' }
      : { ...heightStyle, minHeight: '200px' }),
    width: '100%',
  };

  const chartColors = useMemo(
    () => getColors(colorScheme, colors),
    [colorScheme, colors]
  );

  const eChartsData = useMemo(() => mapDataToECharts(data, chartColors), [data, chartColors]);

  const option = useMemo(() => {
    const strokeColor = stroke === '#ffffff' || stroke.toLowerCase() === 'white'
      ? themeColors.background
      : stroke;

    return {
      color: chartColors,
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
      series: [
        {
          type: ChartType.Sunburst,
          data: eChartsData,
          radius: [innerRadius ?? 0, outerRadius ?? '95%'],
          center: [cx ?? '50%', cy ?? '50%'],
          startAngle: startAngle,
          // Conditionally add endAngle if provided
          ...(endAngle !== undefined && { endAngle }),
          nodeClick: 'rootToNode',
          sort: 'desc',
          itemStyle: {
            borderColor: strokeColor,
            borderWidth: padding,
            borderRadius: padding > 0 ? 4 : 0,
          },
          label: {
            show: true,
            minAngle: 15,
            color: themeColors.foreground,
          },
          emphasis: {
            focus: 'ancestor'
          },
          // Customize levels for ring padding (gap width) and visual hierarchy
          levels: [
            {}, // Blank for root
            { // Level 1
              r0: innerRadius ?? '15%',
              r: '40%',
              itemStyle: { borderWidth: padding, borderRadius: padding > 0 ? 4 : 0 },
              label: { rotate: 'tangential' }
            },
            { // Level 2
              r0: '42%',
              r: '70%',
              itemStyle: { borderWidth: padding, borderRadius: padding > 0 ? 4 : 0 },
              label: { align: 'center', rotate: 'tangential' }
            },
            { // Level 3
              r0: '72%',
              r: outerRadius ?? '95%',
              label: { position: 'outside', padding: 3, silent: false },
              itemStyle: { borderWidth: padding, borderRadius: padding > 0 ? 4 : 0 }
            }
          ]
        },
      ],
    };
  }, [
    chartColors,
    themeColors,
    tooltip,
    eChartsData,
    innerRadius,
    outerRadius,
    cx,
    cy,
    startAngle,
    endAngle,
    padding,
    ringPadding,
    stroke,
  ]);

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

export default SunburstChartWidget;
