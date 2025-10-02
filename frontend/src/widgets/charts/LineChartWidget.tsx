import React from 'react';
import {
  LineChart,
  CartesianGrid,
  Line,
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
} from 'recharts';
import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  ChartLegend,
  ChartLegendContent,
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
    <ChartContainer
      config={chartConfig}
      style={styles}
      className="w-full max-w-[800px]"
    >
      <LineChart
        margin={{ top: 10, right: 10, bottom: 10, left: 10 }}
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
