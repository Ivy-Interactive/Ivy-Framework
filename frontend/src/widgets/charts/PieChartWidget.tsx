import {
  ColorScheme,
  ExtendedPieProps,
  ExtendedTooltipProps,
  generatePieProps,
  getColorGenerator,
  generateLegendProps,
  generateLabelListProps,
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
  Legend,
  LegendProps,
  Pie,
  PieChart,
  Cell,
  LabelList,
  Label,
} from "recharts";

interface PieChartTotalProps {
  formattedValue: string;
  label: string;
}

interface PieChartWidgetProps {
  id: string;
  data: any[];
  width?: string;
  height?: string;
  pies?: ExtendedPieProps[];
  tooltip?: ExtendedTooltipProps;
  legend?: LegendProps;
  colorScheme: ColorScheme;
  total?: PieChartTotalProps;
}

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
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const chartConfig = {} satisfies ChartConfig;
  const [colorGenerator, _] = getColorGenerator(colorScheme);

  return (
    <ChartContainer config={chartConfig} style={styles}>
      <PieChart
        margin={{ top: 0, right: 0, bottom: 0, left: 0 }}
        accessibilityLayer
      >
        {legend && <Legend {...generateLegendProps(legend)} />}

        {tooltip && (
          <ChartTooltip
            cursor={false}
            isAnimationActive={tooltip?.animated}
            content={<ChartTooltipContent />}
          />
        )}

        {pies?.map((props, pieIndex) => (
          <Pie data={data} key={`pie${pieIndex}`} {...generatePieProps(props)}>
            {data.map((_, dataIndex) => (
              <Cell
                key={`cell-${dataIndex}`}
                fill={colorGenerator(dataIndex)}
              />
            ))}

            {props.labelLists?.map((labelList, labelListIndex) => (
              <LabelList
                key={`labelList-${labelListIndex}`}
                {...generateLabelListProps(labelList)}
              />
            ))}

            {pieIndex === 0 && total && (
              <Label
                content={({ viewBox }) => {
                  if (viewBox && "cx" in viewBox && "cy" in viewBox) {
                    return (
                      <text
                        x={viewBox.cx}
                        y={viewBox.cy}
                        textAnchor="middle"
                        dominantBaseline="middle"
                      >
                        <tspan
                          x={viewBox.cx}
                          y={viewBox.cy}
                          className="fill-foreground text-3xl font-bold"
                        >
                          {total.formattedValue}
                        </tspan>
                        <tspan
                          x={viewBox.cx}
                          y={(viewBox.cy || 0) + 24}
                          className="fill-muted-foreground"
                        >
                          {total.label}
                        </tspan>
                      </text>
                    );
                  }
                }}
              />
            )}
          </Pie>
        ))}
      </PieChart>
    </ChartContainer>
  );
};

export default PieChartWidget;
