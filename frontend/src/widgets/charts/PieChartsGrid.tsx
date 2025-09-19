import React from 'react';
import PieChartWidget from './PieChartWidget';
import { LegendProps } from 'recharts';
import { ExtendedPieProps, ExtendedTooltipProps, ColorScheme } from './shared';

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

interface PieChartsGridProps {
  charts: PieChartWidgetProps[];
  className?: string;
}

const PieChartsGrid: React.FC<PieChartsGridProps> = ({
  charts,
  className = '',
}) => {
  return (
    <div className={`pie-charts-grid ${className}`}>
      {charts.map((chartProps, index) => (
        <PieChartWidget key={chartProps.id || index} {...chartProps} />
      ))}
    </div>
  );
};

export default PieChartsGrid;
