import React, { useEffect, useState } from 'react';
import { ColorScheme } from './sharedUtils';
import { getHeight, getWidth } from '@/lib/styles';
import { useTheme } from '@/components/theme-provider';
import ReactECharts from 'echarts-for-react';
import {
  generateDataProps,
  getColors,
  generateXAxis,
  generateEChartLegend,
  generateTooltip,
  generateTextStyle,
  generateEChartGrid,
  generateYAxis,
} from './sharedUtils';
import {
  ChartType,
  XAxisProps,
  YAxisProps,
  LinesProps,
  MarkLine,
  MarkArea,
  LegendProps,
  CartesianGridProps,
  ToolTipProps,
} from './chartTypes';
import { ChartData } from './chartTypes';
import { getTransformValueFn } from './sharedUtils';
import { ReferenceDot } from './chartTypes';

interface AreaChartWidgetProps {
  id: string;
  data: ChartData[];
  width?: string;
  height?: string;
  areas?: LinesProps[];
  cartesianGrid?: CartesianGridProps;
  xAxis?: XAxisProps[];
  yAxis?: YAxisProps[];
  tooltip?: ToolTipProps;
  legend?: LegendProps;
  referenceLines?: MarkLine;
  referenceAreas?: MarkArea;
  referenceDots?: ReferenceDot;
  colorScheme: ColorScheme;
}

const AreaChartWidget: React.FC<AreaChartWidgetProps> = ({
  data,
  width,
  height,
  areas,
  cartesianGrid,
  xAxis,
  yAxis,
  tooltip,
  legend,
  referenceLines,
  referenceAreas,
  referenceDots,
  colorScheme,
}) => {
  const { theme } = useTheme();
  const [themeColors, setThemeColors] = useState({
    foreground: '#000000',
    mutedForeground: '#666666',
    fontSans: 'Geist, sans-serif',
    background: '#ffffff',
  });

  useEffect(() => {
    const getThemeColors = () => {
      const root = document.documentElement;
      const computedStyle = getComputedStyle(root);

      // Use the theme value directly instead of checking DOM classes
      const isDarkMode =
        theme === 'dark' ||
        (theme === 'system' &&
          window.matchMedia('(prefers-color-scheme: dark)').matches);

      return {
        foreground:
          computedStyle.getPropertyValue('--foreground').trim() ||
          (isDarkMode ? '#f8f8f8' : '#000000'),
        mutedForeground:
          computedStyle.getPropertyValue('--muted-foreground').trim() ||
          (isDarkMode ? '#a1a1aa' : '#666666'),
        fontSans:
          computedStyle.getPropertyValue('--font-sans').trim() ||
          'Geist, sans-serif',
        background:
          computedStyle.getPropertyValue('--background').trim() ||
          (isDarkMode ? '#000000' : '#ffffff'),
      };
    };

    // Update colors on next frame to avoid synchronous setState in effect
    const frame = requestAnimationFrame(() => {
      setThemeColors(getThemeColors());
    });

    return () => {
      cancelAnimationFrame(frame);
    };
  }, [theme]);

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    display: 'flex',
    flexDirection: 'column',
  };

  const { categories, valueKeys } = generateDataProps(data);

  const colors = getColors(colorScheme);
  const { transform, largeSpread, minValue, maxValue } =
    getTransformValueFn(data);
  // precompute
  const gradientColors = colors.map(color => ({
    opacity: 0.4,
    type: 'linear',
    x: 0,
    y: 0,
    x2: 0,
    y2: 1,
    colorStops: [
      { offset: 0, color },
      { offset: 1, color: 'transparent' },
    ],
  }));
  const series = valueKeys.map((key, i) => {
    const areaConfig = areas?.find(a => a.dataKey.toLowerCase() === key);

    return {
      name: key,
      type: ChartType.Line,
      smooth: areaConfig?.curveType?.toLowerCase() === 'natural',
      lineStyle: {
        width: areaConfig?.strokeWidth ?? 2,
        color: areaConfig?.stroke ?? colors[i],
        type: areaConfig?.strokeDashArray ? 'dashed' : 'solid',
      },
      showSymbol: false,
      areaStyle: gradientColors[i],
      emphasis: { focus: 'series' },
      data: data.map(d => d[key]),
      markPoint: referenceDots ?? {},
      markLine: referenceLines ?? {},
      markArea: referenceAreas ?? {},
    };
  });

  const option = {
    grid: generateEChartGrid(cartesianGrid, !!legend),
    color: colors,
    tooltip: generateTooltip(tooltip, 'cross', {
      foreground: themeColors.foreground,
      fontSans: themeColors.fontSans,
      background: themeColors.background,
    }),
    legend: generateEChartLegend(legend, {
      foreground: themeColors.foreground,
      fontSans: themeColors.fontSans,
    }),
    textStyle: generateTextStyle(themeColors.foreground, themeColors.fontSans),
    xAxis: generateXAxis(categories as string[], xAxis, false, {
      mutedForeground: themeColors.mutedForeground,
      fontSans: themeColors.fontSans,
    }),
    yAxis: generateYAxis(
      largeSpread,
      transform,
      minValue,
      maxValue,
      yAxis,
      false,
      undefined,
      {
        mutedForeground: themeColors.mutedForeground,
        fontSans: themeColors.fontSans,
      }
    ),
    series: series,
  };

  return (
    <div style={styles}>
      <div style={{ flex: 1, minHeight: 0 }}>
        <ReactECharts
          key={theme}
          option={option}
          style={{ height: '100%', width: '100%' }}
        />
      </div>
    </div>
  );
};

export default AreaChartWidget;
