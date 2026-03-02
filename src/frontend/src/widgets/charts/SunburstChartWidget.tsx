import React, { useMemo } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { useThemeWithMonitoring } from '@/components/theme-provider';
import ReactECharts from 'echarts-for-react';
import {
  getColors,
  generateTextStyle,
  generateTooltip,
  generateEChartToolbox,
} from './sharedUtils';
import {
  ChartType,
  SunburstChartWidgetProps,
  SunburstNodeProps,
} from './chartTypes';
import { getChartThemeColors } from './styles';
import { PIE_LEGEND_DEFAULTS, applyDefaults } from './chartDefaults';

const resolveIvyColor = (colorName?: string | null): string | undefined => {
  if (!colorName) return undefined;
  if (
    colorName.startsWith('#') ||
    colorName.startsWith('rgb') ||
    colorName.startsWith('hsl')
  )
    return colorName;

  if (typeof document !== 'undefined') {
    const cssVarName = `--${colorName.toLowerCase()}`;
    const cssVal = getComputedStyle(document.documentElement)
      .getPropertyValue(cssVarName)
      .trim();
    if (cssVal) return cssVal;
  }

  return colorName;
};

const mapDataToECharts = (
  nodes: SunburstNodeProps[],
  chartColors: string[],
  parentColor?: string,
  depth = 0,
  opacity = 1.0
): any[] => {
  return nodes.map((node, index) => {
    // Resolve custom Ivy Colors to standard hex values so ECharts can render them correctly
    const nodeColor = resolveIvyColor(node.fill);

    // Assign a color based on the top-level index if no parent color is provided,
    // or use the explicitly set Fill color if available.
    const currentColor =
      nodeColor || parentColor || chartColors[index % chartColors.length];

    return {
      name: node.name,
      value: node.children?.length && node.value === 0 ? undefined : node.value,
      itemStyle: {
        color: currentColor,
        opacity: opacity,
      },
      // Recursively map children, passing down the assigned color so children can calculate
      // lighter/darker variations (handled implicitly by ECharts if we just set the same color,
      // or we can let ECharts naturally gradient it. Providing the explicit color helps match theme).
      children:
        node.children && node.children.length > 0
          ? mapDataToECharts(
              node.children,
              chartColors,
              currentColor,
              depth + 1,
              Math.max(0.2, opacity - 0.25)
            )
          : undefined,
    };
  });
};

const SunburstChartWidget: React.FC<SunburstChartWidgetProps> = ({
  data = [],
  width = 'Full',
  height = 'Full',
  tooltip,
  legend,
  toolbox,
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

  const eChartsData = useMemo(
    () => mapDataToECharts(data, chartColors),
    [data, chartColors]
  );

  const option = useMemo(() => {
    const strokeColor =
      stroke === '#ffffff' || stroke.toLowerCase() === 'white'
        ? themeColors.background
        : stroke;

    const leg = legend ? applyDefaults(legend, PIE_LEGEND_DEFAULTS) : null;

    return {
      color: chartColors,
      ...(leg && {
        legend: {
          data: eChartsData.map((d: any) => d.name),
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
      toolbox: generateEChartToolbox(
        toolbox && { ...toolbox, magicType: false }
      ),
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
            focus: 'ancestor',
          },
          // Customize levels for ring padding (gap width) and visual hierarchy
          levels: [
            {}, // Blank for root
            {
              // Level 1
              itemStyle: {
                borderWidth: padding,
                borderRadius: padding > 0 ? 4 : 0,
              },
              label: { rotate: 'tangential' },
            },
            {
              // Level 2
              itemStyle: {
                borderWidth: padding,
                borderRadius: padding > 0 ? 4 : 0,
              },
              label: { align: 'center', rotate: 'tangential' },
            },
            {
              // Level 3
              label: {
                align: 'center',
                rotate: 'tangential',
                padding: 3,
                silent: false,
              },
              itemStyle: {
                borderWidth: padding,
                borderRadius: padding > 0 ? 4 : 0,
              },
            },
          ],
        },
        leg
          ? {
              type: 'pie',
              data: eChartsData.map((d: any) => ({
                name: d.name,
                value: 0,
                itemStyle: d.itemStyle,
              })),
              center: ['-100%', '-100%'],
              radius: [0, 0],
              label: { show: false },
              labelLine: { show: false },
              tooltip: { show: false },
              itemStyle: { opacity: 0 },
              silent: true,
            }
          : null,
      ].filter(Boolean),
    };
  }, [
    chartColors,
    legend,
    toolbox,
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
