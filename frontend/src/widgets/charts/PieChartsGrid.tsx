import React from 'react';
import PieChartWidget from './PieChartWidget';
import { LegendProps } from 'recharts';
import { ExtendedPieProps, ExtendedTooltipProps, ColorScheme } from './shared';
import { PieChartTotalProps } from './utils';
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

interface PieChartsGridProps {
  charts: PieChartWidgetProps[];
  className?: string;
}

const PieChartsGrid: React.FC<PieChartsGridProps> = ({
  charts,
  className = '',
}) => {
  return (
    <div className={`pie-charts-grid flex flex-wrap -mx-2 ${className}`}>
      {charts.map((chartProps, index) => (
        <div key={chartProps.id || index} className="w-full lg:w-1/2 px-2 mb-4">
          <PieChartWidget {...chartProps} />
        </div>
      ))}
    </div>
  );
};

export default PieChartsGrid;
