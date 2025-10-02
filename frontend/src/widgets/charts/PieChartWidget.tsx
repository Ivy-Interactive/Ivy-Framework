import React from 'react';
import { LegendProps, Pie, PieChart, Cell, LabelList, Label } from 'recharts';
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
  ExtendedPieProps,
  ExtendedTooltipProps,
  generatePieProps,
  getColorGenerator,
  generateLegendProps,
  generateLabelListProps,
} from './shared';
import { getHeight, getWidth } from '@/lib/styles';
import { calculateCenterLabelLayout } from './utils';

interface PieChartTotalProps {
  formattedValue: string;
  label: string;
}

interface PieChartData {
  [key: string]: string | number;
}

interface PieChartWidgetProps {
  id: string;
  data: PieChartData[];
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
  const [colorGenerator] = getColorGenerator(colorScheme);

  return (
    <ChartContainer
      config={chartConfig}
      style={styles}
      className="aspect-square w-full"
    >
      <PieChart
        margin={{ top: 0, right: 0, bottom: 0, left: 0 }}
        accessibilityLayer
      >
        {legend && (
          <ChartLegend
            {...generateLegendProps(legend)}
            verticalAlign="bottom"
            align="center"
            layout="horizontal"
            content={<ChartLegendContent splitThreshold={6} />}
          />
        )}

        {tooltip && (
          <ChartTooltip
            cursor={false}
            isAnimationActive={tooltip?.animated}
            content={<ChartTooltipContent />}
          />
        )}

        {pies?.map((props, pieIndex) => {
          const isDonut = !!props.innerRadius;
          const manySlices = Array.isArray(data) && data.length > 6;
          const basePieProps = generatePieProps(props);
          const pieOverrides =
            isDonut && manySlices
              ? { labelLine: false, minAngle: 8, cx: '50%', cy: '50%' }
              : { cx: '50%', cy: '50%' };
          return (
            <Pie
              data={data}
              key={`pie${pieIndex}`}
              {...basePieProps}
              {...pieOverrides}
            >
              {data.map((_, dataIndex) => (
                <Cell
                  key={`cell-${dataIndex}`}
                  fill={colorGenerator(dataIndex)}
                />
              ))}

              {props.labelLists
                ?.filter(ll => {
                  // For donuts, drop inside labels; keep outside only.
                  const pos = ll.position || '';
                  return !isDonut || pos.toString().toLowerCase() !== 'inside';
                })
                .map((labelList, labelListIndex) => (
                  <LabelList
                    key={`labelList-${labelListIndex}`}
                    {...generateLabelListProps(labelList)}
                  />
                ))}

              {(() => {
                // Backward-compatible display logic for total label.
                // Prefer explicit per-pie flag: props.showTotal === true
                // Fallback to legacy behavior: first pie shows total when provided
                const showTotal: boolean =
                  (props as unknown as { showTotal?: boolean })?.showTotal ??
                  pieIndex === 0;
                return showTotal && total;
              })() && (
                <Label
                  position="center"
                  content={({ viewBox }) => {
                    const layout = calculateCenterLabelLayout(
                      viewBox as Record<string, unknown> | undefined,
                      basePieProps,
                      total!
                    );
                    if (!layout) return null;
                    const { cx, cy, text, valueFont, labelFont, gap } = layout;
                    return (
                      <g pointerEvents="none">
                        <text
                          x={cx}
                          y={cy}
                          textAnchor="middle"
                          dominantBaseline="central"
                          alignmentBaseline="middle"
                        >
                          <tspan
                            x={cx}
                            y={cy}
                            style={{
                              fontSize: `${valueFont}px`,
                              lineHeight: 1,
                            }}
                            className="fill-foreground font-bold"
                          >
                            {text}
                          </tspan>
                          <tspan
                            x={cx}
                            y={cy + gap}
                            style={{
                              fontSize: `${labelFont}px`,
                              lineHeight: 1,
                            }}
                            className="fill-muted-foreground"
                          >
                            {total!.label}
                          </tspan>
                        </text>
                      </g>
                    );
                  }}
                />
              )}
            </Pie>
          );
        })}
      </PieChart>
    </ChartContainer>
  );
};

export default PieChartWidget;
