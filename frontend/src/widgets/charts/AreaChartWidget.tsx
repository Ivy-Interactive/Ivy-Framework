import {
  ColorScheme,
  ExtendedAreaProps,
  ExtendedTooltipProps,
  generateAreaProps,
  getColorGenerator,
} from "./shared";
import {
  ExtendedXAxisProps,
  ExtendedYAxisProps,
  generateXAxisProps,
  generateYAxisProps,
  generateLegendProps,
} from "./shared";
import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import { getHeight, getWidth } from "@/lib/styles";
import React from "react";
import {
  CartesianGrid,
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
  AreaChart,
  Area,
} from "recharts";
import { StackOffsetType } from "recharts/types/util/types";

interface AreaChartWidgetProps {
  id: string;
  data: any;
  width?: string;
  height?: string;
  areas?: ExtendedAreaProps[];
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
  stackOffset,
}) => {
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };
  const chartConfig = {} satisfies ChartConfig;
  const [colorGenerator, svgDefs] = getColorGenerator(colorScheme);

  return (
    <ChartContainer config={chartConfig} style={styles} className="mt-4">
      <AreaChart
        margin={{ top: 0, right: 0, bottom: 0, left: 0 }}
        accessibilityLayer
        data={data}
        stackOffset={stackOffset}
      >
        {svgDefs}

        {cartesianGrid && <CartesianGrid {...cartesianGrid} />}

        {xAxis?.map((props, index) => (
          <XAxis key={`xaxis${index}`} {...generateXAxisProps(props)} />
        ))}

        {yAxis?.map((props, index) => (
          <YAxis key={`yaxis${index}`} {...generateYAxisProps(props)} />
        ))}

        {legend && <Legend {...generateLegendProps(legend)} />}

        {referenceAreas?.map(({ ref, ...props }, index) => (
          <ReferenceArea key={`refArea${index}`} {...props} />
        ))}
        {referenceLines?.map(({ ref, ...props }, index) => (
          <ReferenceLine key={`refLine${index}`} {...props} />
        ))}
        {referenceDots?.map(({ ref, ...props }, index) => (
          <ReferenceDot key={`refDot${index}`} {...props} />
        ))}

        {areas?.map((props, index) => (
          <Area
            key={`line${index}`}
            {...generateAreaProps(props, index, colorGenerator)}
          />
        ))}

        {tooltip && (
          <ChartTooltip
            cursor={false}
            isAnimationActive={tooltip?.animated}
            content={<ChartTooltipContent />}
          />
        )}
      </AreaChart>
    </ChartContainer>
  );
};

export default AreaChartWidget;
