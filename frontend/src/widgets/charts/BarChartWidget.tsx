import {
  ColorScheme,
  ExtendedBarProps,
  ExtendedTooltipProps,
  generateBarProps,
  generateLabelListProps,
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
import { camelCase } from "lodash";
import React from "react";
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
  Legend,
} from "recharts";
import { LayoutType, StackOffsetType } from "recharts/types/util/types";

interface BarChartWidgetProps {
  id: string;
  data: any;
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
  layout?: "horizontal" | "vertical";
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
  const [colorGenerator, _] = getColorGenerator(colorScheme);

  return (
    <ChartContainer config={chartConfig} style={styles} className="mt-4">
      <BarChart
        margin={{ top: 0, right: 0, bottom: 0, left: 0 }}
        layout={camelCase(layout) as LayoutType}
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

        {legend && <Legend {...generateLegendProps(legend)} />}

        {/* {legend && <ChartLegend {...generateLegendProps(legend)} content={<ChartLegendContent />} />}         */}

        {referenceAreas?.map(({ ref, ...props }, index) => (
          <ReferenceArea key={`refArea${index}`} {...props} />
        ))}
        {referenceLines?.map(({ ref, ...props }, index) => (
          <ReferenceLine key={`refLine${index}`} {...props} />
        ))}
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
