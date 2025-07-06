import React from 'react';
import {
  LineChart,
  CartesianGrid,
  Line,
  XAxis,
  YAxis,
  Legend,
  ReferenceArea,
  ReferenceLine,
  ReferenceDot,
  CartesianGridProps,
  ReferenceLineProps,
  ReferenceAreaProps,
  ReferenceDotProps,
  LegendProps,
} from 'recharts';
import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from '@/components/ui/chart';
import { ColorScheme, ExtendedTooltipProps, getColorGenerator } from './shared';
import {
  ExtendedXAxisProps,
  ExtendedYAxisProps,
  ExtendedLineProps,
  generateXAxisProps,
  generateYAxisProps,
  generateLegendProps,
  generateLineProps,
} from './shared';
import { getHeight, getWidth } from '@/lib/styles';

interface LineChartData {
  [key: string]: string | number;
}

interface LineChartWidgetProps {
  id: string;
  data: LineChartData[];
  width?: string;
  height?: string;
  lines?: ExtendedLineProps[];
  cartesianGrid?: CartesianGridProps;
  xAxis?: ExtendedXAxisProps[];
  yAxis?: ExtendedYAxisProps[];
  tooltip?: ExtendedTooltipProps;
  legend?: LegendProps;
  referenceLines?: ReferenceLineProps[];
  referenceAreas?: ReferenceAreaProps[];
  referenceDots?: ReferenceDotProps[];
  colorScheme: ColorScheme;
}

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
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const chartConfig = {} satisfies ChartConfig;
  const [colorGenerator] = getColorGenerator(colorScheme);

  return (
    <ChartContainer config={chartConfig} style={styles} className="mt-4">
      <LineChart
        margin={{ top: 0, right: 0, bottom: 0, left: 0 }}
        accessibilityLayer
        data={data}
      >
        {cartesianGrid && <CartesianGrid {...cartesianGrid} />}

        {xAxis?.map((props, index) => (
          <XAxis key={`xaxis${index}`} {...generateXAxisProps(props)} />
        ))}

        {yAxis?.map((props, index) => (
          <YAxis key={`yaxis${index}`} {...generateYAxisProps(props)} />
        ))}

        {legend && <Legend {...generateLegendProps(legend)} />}
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

        {lines?.map((props, index) => (
          <Line
            key={`line${index}`}
            {...generateLineProps(props, index, colorGenerator)}
          />
        ))}

        {tooltip && (
          <ChartTooltip
            cursor={false}
            isAnimationActive={tooltip?.animated}
            content={<ChartTooltipContent />}
          />
        )}
      </LineChart>
    </ChartContainer>
  );
};

export default LineChartWidget;
