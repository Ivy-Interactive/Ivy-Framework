import React, { useMemo } from 'react';
import { getHeight, getWidth } from '@/lib/styles';
import { useThemeWithMonitoring } from '@/components/theme-provider';
import ReactECharts from 'echarts-for-react';
import {
  getColors,
  generateTextStyle,
  generateEChartToolbox,
} from './sharedUtils';
import { ChartType, PieChartWidgetProps } from './chartTypes';
import { generateDataProps } from './sharedUtils';
import { getChartThemeColors } from './styles';

const PieChartWidget: React.FC<PieChartWidgetProps> = ({
  data,
  width,
  height,
  pies,
  tooltip,
  toolbox,
  legend,
  colorScheme,
  total,
}) => {
  // Use enhanced theme hook with automatic monitoring
  const { colors, isDark } = useThemeWithMonitoring({
    monitorDOM: false, // Disabled to prevent excessive re-renders from MutationObserver
    monitorSystem: true, // Keep system theme monitoring for light/dark mode switching
  });

  // Extract chart-specific theme colors
  const themeColors = useMemo(
    () => getChartThemeColors(colors, isDark),
    [colors, isDark]
  );

  // When height is Full (100%), use flex to expand. Otherwise use explicit height.
  const heightStyle = height ? getHeight(height) : {};
  const isFull = height?.toLowerCase().startsWith('full');

  const styles: React.CSSProperties = {
    ...getWidth(width),
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

  // Chart colors depend on theme (chromatic colors automatically adapt to light/dark mode)
  const chartColors = useMemo(
    () => getColors(colorScheme, colors),
    [colorScheme, colors]
  );

  const newData = useMemo(
    () => data.map(d => ({ value: d.measure, name: d.dimension as string })),
    [data]
  );

  // Memoize series configuration
  const series = useMemo(
    () =>
      valueKeys.map(key => {
        const pieProperties = pies?.find(a => a.dataKey.toLowerCase() === key);

        // Adjust vertical center based on total and legend presence
        let centerY = pieProperties?.cy ?? '50%';
        if (!pieProperties?.cy) {
          // Only adjust if not explicitly set
          if (total && legend) {
            centerY = '45%'; // Both total and legend need space
          } else if (legend) {
            centerY = '45%'; // Legend at bottom needs space
          } else if (total) {
            centerY = '52%'; // Total at top, shift down slightly
          }
        }

        return {
          name: key.charAt(0).toUpperCase() + key.slice(1),
          type: ChartType.Pie,
          radius: [
            pieProperties?.innerRadius ?? '40%',
            pieProperties?.outerRadius ?? '70%',
          ],
          center: [pieProperties?.cx ?? '50%', centerY],
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
              show: false,
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
      }),
    [valueKeys, pies, newData, total, legend]
  );

  // Memoize option configuration
  const option = useMemo(
    () => ({
      color: chartColors,
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
      toolbox: generateEChartToolbox(toolbox),
    }),
    [chartColors, legend, themeColors, tooltip, series, toolbox]
  );

  return (
    <div style={styles}>
      <ReactECharts
        option={option}
        style={chartStyles}
        notMerge={true} // Merge changes instead of full rebuild for better performance
        lazyUpdate={true}
      />
    </div>
  );
};

export default PieChartWidget;
