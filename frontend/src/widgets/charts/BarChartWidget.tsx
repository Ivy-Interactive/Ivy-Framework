import React from 'react';
import {
  CartesianGrid,
  XAxis,
  YAxis,
  ReferenceArea,
  ReferenceLine,
  ReferenceDot,
  CartesianGridProps,
  ReferenceLineProps,
  ReferenceAreaProps,
  ReferenceDotProps,
  LegendProps,
  Bar,
  BarChart,
  LabelList,
} from 'recharts';
import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  ChartLegend,
  ChartLegendContent,
} from '@/components/ui/chart';
import {
  ColorScheme,
  ExtendedBarProps,
  ExtendedTooltipProps,
  generateBarProps,
  generateLabelListProps,
  getColorGenerator,
} from './shared';
import {
  ExtendedXAxisProps,
  ExtendedYAxisProps,
  generateXAxisProps,
  generateYAxisProps,
  generateLegendProps,
} from './shared';
import { getHeight, getWidth } from '@/lib/styles';
import { StackOffsetType } from 'recharts/types/util/types';
import { camelCase } from 'lodash';

interface BarChartWidgetProps {
  id: string;
  data: Record<string, unknown>[];
  width?: string;
  height?: string;
  bars?: ExtendedBarProps[];
  cartesianGrid?: CartesianGridProps;
  xAxis?: ExtendedXAxisProps[];
  yAxis?: ExtendedYAxisProps[];
  tooltip?: ExtendedTooltipProps;
  legend?: LegendProps;
  referenceLines?: ReferenceLineProps[];
  referenceAreas?: ReferenceAreaProps[];
  referenceDots?: ReferenceDotProps[];
  colorScheme: ColorScheme;
  stackOffset: StackOffsetType;
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
  stackOffset,
  barGap,
  barCategoryGap,
  maxBarSize,
  reverseStackOrder,
  layout,
}) => {
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const chartConfig = {} satisfies ChartConfig;
  const [colorGenerator] = getColorGenerator(colorScheme);

  return (
    <ChartContainer config={chartConfig} style={styles} className="w-full">
      <BarChart
        margin={{ top: 10, right: 10, bottom: 10, left: 10 }}
        layout={camelCase(layout) as 'horizontal' | 'vertical'}
        accessibilityLayer
        data={data}
        stackOffset={stackOffset}
        barGap={barGap}
        barCategoryGap={barCategoryGap}
        maxBarSize={maxBarSize}
        reverseStackOrder={reverseStackOrder}
      >
        {cartesianGrid && <CartesianGrid {...cartesianGrid} />}

        {xAxis?.map((props, index) => (
          <XAxis key={`xaxis${index}`} {...generateXAxisProps(props)} />
        ))}

        {yAxis?.map((props, index) => (
          <YAxis key={`yaxis${index}`} {...generateYAxisProps(props)} />
        ))}

        {legend && (
          <ChartLegend
            {...generateLegendProps(legend)}
            content={<ChartLegendContent splitThreshold={6} />}
          />
        )}

        {/* eslint-disable-next-line @typescript-eslint/no-unused-vars */}
        {referenceAreas?.map(({ ref, ...props }, index) => (
          <ReferenceArea key={`refArea${index}`} {...props} />
        ))}
        {/* eslint-disable-next-line @typescript-eslint/no-unused-vars */}
        {referenceLines?.map(({ ref, ...props }, index) => (
          <ReferenceLine key={`refLine${index}`} {...props} />
        ))}
        {/* eslint-disable-next-line @typescript-eslint/no-unused-vars */}
        {referenceDots?.map(({ ref, ...props }, index) => (
          <ReferenceDot key={`refDot${index}`} {...props} />
        ))}

        {tooltip && (
          <ChartTooltip
            cursor={false}
            isAnimationActive={tooltip?.animated}
            content={<ChartTooltipContent />}
          />
        )}

        {bars?.map((props, index) => (
          <Bar
            key={`bar${index}`}
            {...generateBarProps(props, index, colorGenerator)}
          >
            {props.labelLists?.map((labelList, labelListIndex) => (
              <LabelList
                key={`labelList-${labelListIndex}`}
                {...generateLabelListProps(labelList)}
              />
            ))}
          </Bar>
        ))}
      </BarChart>
    </ChartContainer>
  );
};

export default BarChartWidget;
