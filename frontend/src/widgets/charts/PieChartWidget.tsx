import React, { useEffect, useState } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { useTheme } from '@/components/theme-provider';
import ReactECharts from 'echarts-for-react';
import { getColors, generateTextStyle } from './sharedUtils';
import { ChartType, PieChartWidgetProps } from './chartTypes';
import { generateDataProps } from './sharedUtils';

const PieChartWidget: React.FC<PieChartWidgetProps> = ({
  data,
  width,
  height,
  pies,
  tooltip,
  legend,
  colorScheme,
  total,
}) => {
  const { theme } = useTheme();
  const [themeColors, setThemeColors] = useState({
    foreground: '#000000',
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

  const { valueKeys } = generateDataProps(data);

  const colors = getColors(colorScheme);

  const newData = data.map(d => {
    return { value: d.measure, name: d.dimension as string };
  });

  const series = valueKeys.map(key => {
    const pieProperties = pies?.find(a => a.dataKey.toLowerCase() === key);

    return {
      name: key.charAt(0).toUpperCase() + key.slice(1),
      type: ChartType.Pie,
      radius: [
        pieProperties?.innerRadius ?? '40%',
        pieProperties?.outerRadius ?? '70%',
      ],
      center: [pieProperties?.cx ?? '50%', pieProperties?.cy ?? '50%'],
      startAngle: pieProperties?.startAngle ?? 90,
      endAngle: pieProperties?.endAngle ?? 450,
      animation: pieProperties?.animated ?? true,
      avoidLabelOverlap: false,
      label: {
        show: false,
        position: 'center',
      },
      emphasis: {
        disabled: false,
        scale: true,
        scaleSize: 5,
        focus: 'none',
        label: {
          show: true,
          fontSize: 40,
          fontWeight: 'bold',
        },
      },
      labelLine: {
        show: false,
      },
      itemStyle: {
        color: pieProperties?.fill ?? undefined,
        opacity: pieProperties?.fillOpacity ?? undefined,
        borderColor: pieProperties?.stroke ?? undefined,
        borderWidth: pieProperties?.strokeWidth ?? undefined,
      },
      data: newData,
    };
  });

  const option = {
    color: colors,
    ...(legend && {
      legend: {
        orient:
          legend.layout?.toLowerCase() === 'vertical'
            ? 'vertical'
            : 'horizontal',
        left:
          legend.align?.toLowerCase() === 'left'
            ? 'left'
            : legend.align?.toLowerCase() === 'right'
              ? 'right'
              : 'center',
        top:
          legend.verticalAlign?.toLowerCase() === 'top'
            ? 'top'
            : legend.verticalAlign?.toLowerCase() === 'middle'
              ? 'middle'
              : 'bottom',
        icon: legend.iconType ?? 'circle',
        itemWidth: legend.iconSize ?? 12,
        itemHeight: legend.iconSize ?? 12,
        type: 'scroll',
      },
    }),
    textStyle: generateTextStyle(themeColors.foreground, themeColors.fontSans),
    tooltip: {
      trigger: 'item',
      formatter: '{a} <br/>{b}: {c} ({d}%)',
      animated: tooltip?.animated ?? true,
      textStyle: generateTextStyle(
        themeColors.foreground,
        themeColors.fontSans
      ),
      backgroundColor: themeColors.background,
      borderColor: themeColors.foreground,
      borderWidth: 1,
    },
    series: series,
  };

  return (
    <div style={styles}>
      {total && (
        <div style={{ textAlign: 'center', marginBottom: 12 }}>
          <span>{total.label}</span>
          <span>{total.formattedValue}</span>
        </div>
      )}
      <ReactECharts key={theme} option={option} />
    </div>
  );
};

export default PieChartWidget;
