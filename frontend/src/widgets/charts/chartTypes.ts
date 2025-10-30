export type ColorScheme = 'Default' | 'Rainbow';

export enum ChartType {
  Bar = 'bar',
  Line = 'line',
  Pie = 'pie',
}

export interface ChartData {
  [key: string]: string | number;
}
export type BarProps = {
  animated?: boolean;
  dataKey: string;
  fill?: string | null;
  fillOpacity?: number | null;
  labelLists?: string[];
  legendType?: string;
  name: string;
  radius?: number[];
  stackId?: string | number;
  stroke?: string | null;
  strokeDashArray?: string | null;
  strokeWidth?: number;
  unit?: string | null;
};

interface PieChartTotalProps {
  formattedValue: string;
  label: string;
}

export interface PieChartWidgetProps {
  id: string;
  data: ChartData[];
  width?: string;
  height?: string;
  pies?: PieProps[];
  tooltip?: ToolTipProps;
  legend?: PieLegendProps;
  colorScheme: ColorScheme;
  total?: PieChartTotalProps;
}

export type YAxisProps = {
  allowDataOverflow: boolean;
  allowDecimals: boolean;
  allowDuplicatedCategory: boolean;
  angle: number;
  axisLine: boolean;
  dataKey: string;
  domainStart: 'auto' | number;
  domainEnd: 'auto' | number;
  hide: boolean;
  includeHidden: boolean;
  label: null;
  minTickGap: number;
  mirror: boolean;
  name: null;
  orientation: string;
  reversed: boolean;
  scale: string;
  tickCount: number;
  tickLine: boolean;
  tickSize: number;
  type: string;
  unit: null;
  width: number;
};

export interface XAxisProps {
  allowDataOverflow?: boolean;
  allowDecimals?: boolean;
  allowDuplicatedCategory?: boolean;
  angle?: number;
  axisLine?: boolean;
  dataKey?: string;
  domainStart?: number | 'auto';
  domainEnd?: number | 'auto';
  height?: number;
  hide?: boolean;
  includeHidden?: boolean;
  label?: string | null;
  minTickGap?: number;
  mirror?: boolean;
  name?: string | null;
  orientation?: 'Top' | 'Bottom';
  reversed?: boolean;
  scale?: 'Auto' | 'Linear' | 'Log' | 'Time' | 'Ordinal';
  tickCount?: number;
  tickLine?: boolean;
  tickSize?: number;
  type?: 'Category' | 'Number' | 'Time';
  unit?: string | null;
}

export type CartesianGridProps = {
  fill: string | null;
  fillOpacity: number | null;
  height: number | null;
  horizontal: boolean;
  strokeDashArray: string | null;
  vertical: boolean;
  width: number | null;
  x: number | null;
  y: number | null;
};

export type LegendProps = {
  align?: 'Left' | 'Center' | 'Right';
  iconSize?: number;
  iconType?: string | null;
  layout?: 'Horizontal' | 'Vertical';
  verticalAlign?: 'Top' | 'Middle' | 'Bottom';
};

export interface MarkLine {
  silent?: boolean;
  symbol?: string | [string, string];
  symbolSize?: number | [number, number];
  symbolOffset?: number | [number, number];
  precision?: number;
  label?: {
    show?: boolean;
    position?: 'start' | 'middle' | 'end';
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    formatter?: string | ((params: any) => string);
    color?: string;
    fontSize?: number;
    fontWeight?: string | number;
  };
  lineStyle?: {
    color?: string;
    width?: number;
    type?: 'solid' | 'dashed' | 'dotted';
    opacity?: number;
  };
  emphasis?: {
    disabled?: boolean;
    label?: Partial<MarkLine['label']>;
    lineStyle?: Partial<MarkLine['lineStyle']>;
  };
  blur?: {
    label?: Partial<MarkLine['label']>;
    lineStyle?: Partial<MarkLine['lineStyle']>;
  };
  data: Array<{
    type?: 'min' | 'max' | 'average';
    name?: string;
    xAxis?: number | string;
    yAxis?: number;
    coords?: [[number, number], [number, number]];
    value?: number;
  }>;
  z?: number;
  animation?: boolean;
  animationThreshold?: number;
  animationDuration?: number;
  animationEasing?: string;
  animationDelay?: number;
  animationDurationUpdate?: number;
  animationEasingUpdate?: string;
  animationDelayUpdate?: number;
}

type LabelPosition =
  | 'inside'
  | 'insideTop'
  | 'insideBottom'
  | 'insideLeft'
  | 'insideRight'
  | 'insideTopLeft'
  | 'insideTopRight'
  | 'insideBottomLeft'
  | 'insideBottomRight'
  | 'top'
  | 'bottom'
  | 'left'
  | 'right';

export interface MarkArea {
  zlevel?: number;
  z?: number;
  silent?: boolean;
  animation?: boolean;
  animationThreshold?: number;
  animationDuration?: number;
  animationEasing?: string;
  animationDelay?: number;
  animationDurationUpdate?: number;
  animationEasingUpdate?: string;
  animationDelayUpdate?: number;
  label?: {
    show?: boolean;
    position?: LabelPosition;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    formatter?: string | ((params: any) => string);
    color?: string;
    fontSize?: number;
    fontWeight?: string | number;
  };
  itemStyle?: {
    color?: string;
    borderColor?: string;
    borderWidth?: number;
    opacity?: number;
  };
  data: Array<
    [
      { xAxis?: number | string; yAxis?: number | string; name?: string },
      { xAxis?: number | string; yAxis?: number | string; name?: string },
    ]
  >;
}

export interface LinesProps {
  animated?: boolean;
  connectNulls?: boolean;
  curveType?: string;
  dataKey: string;
  label?: string | null;
  legendType?: string;
  name?: string;
  scale?: string;
  stackId?: string | number;
  stroke?: string | null;
  strokeDashArray?: string | null;
  strokeWidth?: number;
  unit?: string | null;
}

export interface LineChartWidgetProps {
  id: string;
  data: ChartData[];
  width?: string;
  height?: string;
  lines?: LinesProps[];
  cartesianGrid?: CartesianGridProps;
  xAxis?: XAxisProps[];
  yAxis?: YAxisProps[];
  tooltip?: ToolTipProps;
  legend?: LegendProps;
  referenceLines?: MarkLine;
  referenceAreas?: MarkArea;
  referenceDots?: ReferenceDot;
  colorScheme: ColorScheme;
}

export interface ReferenceDot {
  x: number;
  y: number;
  label?: string;
}

export type PieProps = {
  animated?: boolean;
  cx?: number | null;
  cy?: number | null;
  dataKey: string;
  endAngle?: number;
  fill?: string | null;
  fillOpacity?: number | null;
  innerRadius?: string | number;
  labelLists?: string[];
  legendType?: string;
  nameKey: string;
  outerRadius?: string | number;
  startAngle?: number;
  stroke?: string | null;
  strokeDashArray?: string | null;
  strokeWidth?: number;
};

export interface ToolTipProps {
  animated?: boolean;
}

export interface PieLegendProps {
  align?: string;
  iconSize?: number;
  iconType?: string | null;
  layout?: string;
  verticalAlign?: string;
}
