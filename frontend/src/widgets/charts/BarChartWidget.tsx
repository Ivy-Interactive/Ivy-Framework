import React, { useEffect, useState } from 'react';
import {
  ColorScheme,
  generateTooltip,
  generateTextStyle,
  generateXAxis,
  generateYAxis,
} from './sharedUtils';
import {
  generateDataProps,
  generateEChartGrid,
  generateEChartLegend,
  getColors,
} from './sharedUtils';
import { useTheme } from '@/components/theme-provider';
import { getHeight, getWidth } from '@/lib/styles';
import ReactECharts from 'echarts-for-react';
import {
  BarProps,
  CartesianGridProps,
  ChartType,
  LegendProps,
  MarkArea,
  MarkLine,
  ReferenceDot,
  ToolTipProps,
  XAxisProps,
  YAxisProps,
} from './chartTypes';
import { ChartData } from './chartTypes';

interface BarChartWidgetProps {
  id: string;
  data: ChartData[];
  width?: string;
  height?: string;
  bars?: BarProps[];
  cartesianGrid?: CartesianGridProps;
  xAxis?: XAxisProps[];
  yAxis?: YAxisProps[];
  tooltip?: ToolTipProps;
  legend?: LegendProps;
  referenceLines?: MarkLine;
  referenceAreas?: MarkArea;
  referenceDots?: ReferenceDot;
  colorScheme: ColorScheme;
  barGap?: number;
  barCategoryGap?: number | string;
  maxBarSize?: number;
  reverseStackOrder?: boolean;
  layout?: 'horizontal' | 'vertical';
}

const BarChartWidget: React.FC<BarChartWidgetProps> = ({
  data,
  width,
  height,
  bars,
  cartesianGrid,
  xAxis,
  yAxis,
  tooltip,
  legend,
  referenceLines,
  referenceAreas,
  referenceDots,
  colorScheme,
  //stackOffset,
  barGap,
  barCategoryGap,
  maxBarSize,
  reverseStackOrder,
  layout,
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
  };

  const { categories, valueKeys, transform, largeSpread, minValue, maxValue } =
    generateDataProps(data);
  const colors = getColors(colorScheme);
  const series = valueKeys.map((key, i) => ({
    name: key,
    type: ChartType.Bar,
    legendHoverLink: true,
    showBackground: true,
    data: data.map(d => d[key]),
    stack:
      bars && bars[i]?.stackId !== undefined
        ? String(bars[i].stackId)
        : undefined,
    barGap: barGap ? `${barGap}%` : '4%',
    barCategoryGap: barCategoryGap ? `${barCategoryGap}%` : '10%',
    barMaxWidth: maxBarSize,
    stackOrder: reverseStackOrder ? 'seriesDesc' : 'seriesAsc',
    markPoint: {
      label: {
        show: referenceDots ? true : false,
      },
    },
    markLine: referenceLines ?? {},
    markArea: referenceAreas ?? {},
  }));
  const isVertical = layout?.toLowerCase() === 'vertical';

  const option = {
    grid: generateEChartGrid(cartesianGrid),
    color: colors,
    textStyle: generateTextStyle(themeColors.foreground, themeColors.fontSans),
    xAxis: generateXAxis(categories, xAxis, isVertical, {
      mutedForeground: themeColors.mutedForeground,
      fontSans: themeColors.fontSans,
    }),
    yAxis: generateYAxis(
      largeSpread,
      transform,
      minValue,
      maxValue,
      yAxis,
      isVertical,
      categories,
      {
        mutedForeground: themeColors.mutedForeground,
        fontSans: themeColors.fontSans,
      }
    ),
    series,
    legend: generateEChartLegend(legend, {
      foreground: themeColors.foreground,
      fontSans: themeColors.fontSans,
    }),
    tooltip: generateTooltip(tooltip, 'shadow', {
      foreground: themeColors.foreground,
      fontSans: themeColors.fontSans,
      background: themeColors.background,
    }),
  };

  return (
    <div style={styles}>
      <ReactECharts key={theme} option={option} />
    </div>
  );
};

export default BarChartWidget;
