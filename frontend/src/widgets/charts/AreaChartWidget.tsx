import React from 'react';
import { ColorScheme } from './sharedUtils';
import { getHeight, getWidth } from '@/lib/styles';
import ReactECharts from 'echarts-for-react';
import {
  generateDataProps,
  getColors,
  generateXAxis,
  generateEChartLegend,
  generateTooltip,
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
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    minHeight: 300,
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
      emphasis: { focus: 'series' }, // Disable hover animation only for area, keep axisPointer
      data: data.map(d => d[key]),
      markPoint: referenceDots ?? {},
      markLine: referenceLines ?? {},
      markArea: referenceAreas ?? {},
    };
  });

  const option = {
    grid: generateEChartGrid(cartesianGrid),
    color: colors,
    tooltip: generateTooltip(tooltip, 'cross'),
    legend: generateEChartLegend(legend),
    toolbox: {
      feature: {
        saveAsImage: {},
      },
    },
    xAxis: generateXAxis(categories as string[], xAxis),
    yAxis: generateYAxis(largeSpread, transform, minValue, maxValue, yAxis),
    series: series,
  };

  return <ReactECharts option={option} style={styles} />;
};

export default AreaChartWidget;
