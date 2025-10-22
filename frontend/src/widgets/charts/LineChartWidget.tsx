import React, { useEffect, useState } from 'react';
import ReactECharts from 'echarts-for-react';
import { getHeight, getWidth } from '@/lib/styles';
import { useTheme } from '@/components/theme-provider';
import {
  generateDataProps,
  generateEChartGrid,
  generateEChartLegend,
  generateSeries,
  generateTooltip,
  generateTextStyle,
  generateXAxis,
  generateYAxis,
  getColors,
  getTransformValueFn,
} from './sharedUtils';
import { LineChartWidgetProps } from './chartTypes';

const LineChartWidget: React.FC<LineChartWidgetProps> = ({
  data,
  width,
  height,
  lines,
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
  };

  const colors = getColors(colorScheme);
  const { categories, valueKeys } = generateDataProps(data);
  const { transform, largeSpread, minValue, maxValue } =
    getTransformValueFn(data);

  const option = {
    grid: generateEChartGrid(cartesianGrid),
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
    tooltip: generateTooltip(tooltip, 'shadow', {
      foreground: themeColors.foreground,
      fontSans: themeColors.fontSans,
      background: themeColors.background,
    }),
    legend: generateEChartLegend(legend, {
      foreground: themeColors.foreground,
      fontSans: themeColors.fontSans,
    }),
    textStyle: generateTextStyle(themeColors.foreground, themeColors.fontSans),
    color: colors,
    series: generateSeries(
      data,
      valueKeys,
      lines,
      transform,
      referenceDots,
      referenceLines,
      referenceAreas
    ),
  };

  return (
    <div style={styles}>
      <ReactECharts key={theme} option={option} />;
    </div>
  );
};

export default LineChartWidget;
