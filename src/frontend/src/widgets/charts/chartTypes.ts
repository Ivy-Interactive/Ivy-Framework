export type ColorScheme = 'Default' | 'Rainbow';

export enum ChartType {
  Bar = 'bar',
  Line = 'line',
  Pie = 'pie',
  Scatter = 'scatter',
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
  toolbox?: ToolboxProps;
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

type ToolboxFeatureDataView = {
  show?: boolean;
  readOnly?: boolean;
  backgroundColor?: string;
  textareaColor?: string;
  textColor?: string;
  buttonColor?: string;
  buttonTextColor?: string;
};
type ToolboxFeatureMagicType = { show?: boolean; type?: string[] };
type ToolboxFeatureSaveAsImage = { show?: boolean };

export type ToolboxFeatures = {
  dataView?: ToolboxFeatureDataView;
  magicType?: ToolboxFeatureMagicType;
  saveAsImage?: ToolboxFeatureSaveAsImage;
};

export type ToolboxProps = {
  enabled?: boolean;
  orientation?: 'Horizontal' | 'Vertical';
  align?: 'Left' | 'Center' | 'Right';
  verticalAlign?: 'Top' | 'Middle' | 'Bottom';
  saveAsImage?: boolean;
  restore?: boolean;
  dataView?: boolean;
  magicType?: boolean;
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
  toolbox?: ToolboxProps;
  legend?: LegendProps;
  referenceLines?: MarkLine[];
  referenceAreas?: MarkArea[];
  referenceDots?: ReferenceDot[];
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

export type ScatterShape =
  | 'Circle'
  | 'Square'
  | 'Cross'
  | 'Diamond'
  | 'Star'
  | 'Triangle'
  | 'Wye';

export type ScatterLineType = 'Joint' | 'Fitting';

export interface ZAxisProps {
  dataKey?: string;
  rangeMin?: number;
  rangeMax?: number;
  unit?: string | null;
  name?: string | null;
  scale?: string;
}

export interface ScatterProps {
  animated?: boolean;
  dataKey: string;
  fill?: string | null;
  fillOpacity?: number | null;
  legendType?: ScatterShape | null;
  line?: boolean;
  lineType?: ScatterLineType;
  name: string;
  shape?: ScatterShape;
  stroke?: string | null;
  strokeDashArray?: string | null;
  strokeWidth?: number;
  unit?: string | null;
}

export interface ScatterChartWidgetProps {
  id: string;
  data: ChartData[];
  width?: string;
  height?: string;
  scatters?: ScatterProps[];
  cartesianGrid?: CartesianGridProps;
  xAxis?: XAxisProps[];
  yAxis?: YAxisProps[];
  zAxis?: ZAxisProps | null;
  tooltip?: ToolTipProps;
  toolbox?: ToolboxProps;
  legend?: LegendProps;
  referenceLines?: MarkLine[];
  referenceAreas?: MarkArea[];
  referenceDots?: ReferenceDot[];
  colorScheme: ColorScheme;
}

export type PolarGridTypes = 'Polygon' | 'Circle';

export interface PolarGridProps {
  gridType?: PolarGridTypes;
  stroke?: string | null;
  radialLines?: boolean;
}

export interface PolarAngleAxisProps {
  dataKey?: string | null;
  stroke?: string | null;
  axisLine?: boolean;
  tickLine?: boolean;
}

export interface PolarRadiusAxisProps {
  angle?: number | null;
  domain?: unknown[] | null;
  tickCount?: number | null;
  stroke?: string | null;
}

export interface RadialBarProps {
  animated?: boolean;
  background?: boolean;
  dataKey: string;
  fill?: string | null;
  labelLists?: string[];
  legendType?: string;
  minAngle?: number;
  name?: string | null;
}

export interface RadialBarChartWidgetProps {
  id: string;
  data: ChartData[];
  width?: string;
  height?: string;
  radialBars?: RadialBarProps[];
  tooltip?: ToolTipProps;
  legend?: LegendProps;
  toolbox?: ToolboxProps;
  colorScheme: ColorScheme;
  polarAngleAxis?: PolarAngleAxisProps | null;
  polarRadiusAxis?: PolarRadiusAxisProps | null;
  polarGrid?: PolarGridProps | null;
  cx?: number | string | null;
  cy?: number | string | null;
  innerRadius?: number | string | null;
  outerRadius?: number | string | null;
  startAngle?: number;
  endAngle?: number;
  barGap?: number;
  barCategoryGap?: number | string | null;
  barSize?: number | null;
}
