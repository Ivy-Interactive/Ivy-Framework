import React from 'react';
import {
  ColorScheme,
  generateTooltip,
  generateXAxis,
  generateYAxis,
} from './sharedUtils';
import {
  generateDataProps,
  generateEChartGrid,
  generateEChartLegend,
  getColors,
} from './sharedUtils';
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
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    minHeight: 300,
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
    xAxis: generateXAxis(categories, xAxis, isVertical),
    yAxis: generateYAxis(
      largeSpread,
      transform,
      minValue,
      maxValue,
      yAxis,
      isVertical,
      categories
    ),
    toolbox: !isVertical
      ? {
          top: legend?.verticalAlign === 'Top' ? 'bottom' : 'top',
          feature: {
            dataView: {
              show: true,
            },
            magicType: {
              type: ['line', 'bar'],
            },
            saveAsImage: {
              type: 'png',
            },
          },
        }
      : {},
    series,
    legend: generateEChartLegend(legend),
    tooltip: generateTooltip(tooltip, 'shadow'),
  };

  return (
    <div style={styles}>
      <ReactECharts
        option={option}
        style={{ width: '100%', height: '100%', minHeight: 300 }}
      />
    </div>
  );
};

export default BarChartWidget;
