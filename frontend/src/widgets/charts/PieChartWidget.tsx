import React from 'react';
import { Legend, LegendProps, Pie, PieChart, Cell, LabelList } from 'recharts';
import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
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
    <div style={styles}>
      {total && (
        <div className="text-center">
          <div className="text-xl font-bold text-foreground">
            {total.formattedValue}
          </div>
          <div className="text-sm text-muted-foreground">{total.label}</div>
        </div>
      )}
      <ChartContainer config={chartConfig}>
        <PieChart
          margin={{ top: 10, right: 10, bottom: 10, left: 10 }}
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
            <Pie
              data={data}
              key={`pie${pieIndex}`}
              {...generatePieProps(props)}
            >
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
            </Pie>
          ))}
        </PieChart>
      </ChartContainer>
    </div>
  );
};

export default PieChartWidget;
