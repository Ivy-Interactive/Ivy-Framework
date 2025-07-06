import {
  XAxisProps,
  YAxisProps,
  LineProps,
  LabelProps,
  LegendProps,
  LegendType,
  TooltipProps,
  AreaProps,
  BarProps,
  PieProps,
  LabelListProps,
} from 'recharts';
import { CurveType } from 'recharts/types/shape/Curve';
import { camelCase } from '@/lib/utils';
import DataFormatter from 'excel-style-dataformatter';

export type ColorScheme = 'Default' | 'Rainbow';

const defaultColors = ['chart-1', 'chart-2', 'chart-3', 'chart-4', 'chart-5'];
const rainbowColors = [
  'blue',
  'cyan',
  'yellow',
  'red',
  'orange',
  'purple',
  'lime',
  'indigo',
  'rose',
  'green',
  'pink',
  'teal',
  'amber',
  'violet',
  'emerald',
  'fuchsia',
  'sky',
];

export const getColorGenerator = (
  scheme: ColorScheme
): [(index: number) => string, null] => {
  switch (scheme) {
    case 'Rainbow':
      return [
        (index: number) =>
          'var(--' + rainbowColors[index % rainbowColors.length] + ')',
        null,
      ];
    default:
      return [
        (index: number) =>
          'var(--' + defaultColors[index % defaultColors.length] + ')',
        null,
      ];
  }
};

export interface ExtendedXAxisProps extends XAxisProps {
  domainStart: number | string;
  domainEnd: number | string;
}

export const generateXAxisProps = (props: ExtendedXAxisProps) => {
  const {
    orientation,
    type,
    dataKey,
    scale,
    domainStart,
    domainEnd,
    ...xAxisProps
  } = props;
  return {
    dataKey: camelCase(dataKey),
    scale: camelCase(scale) as XAxisProps['scale'],
    type: camelCase(type) as XAxisProps['type'],
    domain: [domainStart, domainEnd] as [string | number, string | number],
    orientation: camelCase(orientation) as XAxisProps['orientation'],
    ...xAxisProps,
  };
};

export interface ExtendedYAxisProps extends YAxisProps {
  domainStart: number | string;
  domainEnd: number | string;
}

export const generateYAxisProps = (props: ExtendedYAxisProps) => {
  const {
    orientation,
    type,
    dataKey,
    scale,
    domainStart,
    domainEnd,
    ...yAxisProps
  } = props;
  return {
    dataKey: camelCase(dataKey),
    scale: camelCase(scale) as YAxisProps['scale'],
    type: camelCase(type) as YAxisProps['type'],
    domain: [domainStart, domainEnd] as [string | number, string | number],
    orientation: camelCase(orientation) as YAxisProps['orientation'],
    ...yAxisProps,
  };
};

export const generateLegendProps = (legend: LegendProps) => {
  const { layout, align, verticalAlign, iconType, ...legendProps } = legend;
  return {
    layout: camelCase(layout) as LegendProps['layout'],
    align: camelCase(align) as LegendProps['align'],
    verticalAlign: camelCase(verticalAlign) as LegendProps['verticalAlign'],
    iconType: camelCase(iconType) as LegendProps['iconType'],
    ...legendProps,
  };
};

export interface ExtendedLabelProps extends LabelProps {
  color?: string;
}

export interface ExtendedLineProps extends LineProps {
  animated: boolean;
  curveType: CurveType;
  strokeDashArray?: string;
  label: ExtendedLabelProps;
}

export const generateLineProps = (
  props: ExtendedLineProps,
  index: number,
  colorGenerator: (index: number) => string
) => {
  const {
    animated,
    legendType,
    stroke,
    dataKey,
    curveType,
    strokeDashArray,
    label,
    ...lineProps
  } = props;

  return {
    dataKey: camelCase(dataKey),
    type: camelCase(curveType) as CurveType,
    legendType: camelCase(legendType) as LegendType,
    isAnimationActive: animated,
    stroke:
      (stroke && `var(--${stroke.toLowerCase()})`) || colorGenerator(index),
    strokeDasharray: strokeDashArray,
    label: label && {
      fill:
        (label.color && `var(--${label.color.toLowerCase()})`) ||
        colorGenerator(index) ||
        undefined,
      offset: label.offset,
      position: camelCase(label.position) as LabelProps['position'],
    },
    ...lineProps,
  };
};

export interface ExtendedTooltipProps extends TooltipProps<number, string> {
  animated: boolean;
}

export interface ExtendedAreaProps extends AreaProps {
  animated: boolean;
  curveType: CurveType;
  strokeDashArray?: string;
  label: ExtendedLabelProps;
}

export const generateAreaProps = (
  props: ExtendedAreaProps,
  index: number,
  colorGenerator: (index: number) => string
) => {
  const {
    animated,
    legendType,
    stroke,
    fill,
    dataKey,
    curveType,
    strokeDashArray,
    label,
    ...areaProps
  } = props;
  return {
    dataKey: camelCase(dataKey),
    type: camelCase(curveType) as CurveType,
    legendType: camelCase(legendType) as LegendType,
    isAnimationActive: animated,
    stroke:
      (stroke && `var(--${stroke.toLowerCase()})`) ||
      (fill && `var(--${fill.toLowerCase()})`) ||
      colorGenerator(index),
    fill:
      (fill && `var(--${fill.toLowerCase()})`) ||
      (stroke && `var(--${stroke.toLowerCase()})`) ||
      colorGenerator(index),
    strokeDasharray: strokeDashArray,
    label: label && {
      fill:
        (label.color && `var(--${label.color.toLowerCase()})`) ||
        colorGenerator(index) ||
        undefined,
      offset: label.offset,
      position: camelCase(label.position) as LabelProps['position'],
    },
    ...areaProps,
  };
};

export interface ExtendedBarProps extends BarProps {
  animated: boolean;
  strokeDashArray?: string;
  labelLists?: ExtendedLabelListProps[];
}

export const generateBarProps = (
  props: ExtendedBarProps,
  index: number,
  colorGenerator: (index: number) => string
) => {
  const {
    animated,
    legendType,
    stroke,
    fill,
    dataKey,
    strokeDashArray,
    ...barProps
  } = props;
  return {
    dataKey: camelCase(dataKey),
    legendType: camelCase(legendType) as LegendType,
    isAnimationActive: animated,
    stroke:
      (stroke && `var(--${stroke.toLowerCase()})`) ||
      (fill && `var(--${fill.toLowerCase()})`) ||
      colorGenerator(index),
    fill:
      (fill && `var(--${fill.toLowerCase()})`) ||
      (stroke && `var(--${stroke.toLowerCase()})`) ||
      colorGenerator(index),
    strokeDasharray: strokeDashArray,
    ...barProps,
  };
};

export interface ExtendedPieProps extends PieProps {
  animated: boolean;
  strokeDashArray?: string;
  labelLists?: ExtendedLabelListProps[];
}

export const generatePieProps = (props: ExtendedPieProps) => {
  const {
    animated,
    legendType,
    stroke,
    fill,
    dataKey,
    nameKey,
    strokeDashArray,
    ...pieProps
  } = props;
  return {
    dataKey: camelCase(dataKey),
    nameKey: camelCase(nameKey),
    legendType: camelCase(legendType) as LegendType,
    isAnimationActive: animated,
    stroke:
      (stroke && `var(--${stroke.toLowerCase()})`) ||
      (fill && `var(--${fill.toLowerCase()})`),
    fill:
      (fill && `var(--${fill.toLowerCase()})`) ||
      (stroke && `var(--${stroke.toLowerCase()})`),
    strokeDasharray: strokeDashArray,
    ...pieProps,
  };
};

export interface FormatterDefinitions {
  numberFormat?: string;
}

export interface ExtendedLabelListProps
  extends LabelListProps<Record<string, unknown>>,
    FormatterDefinitions {}

export const generateLabelListProps = (props: ExtendedLabelListProps) => {
  const { fill, position, dataKey, ...labelListProps } = props;
  const formatter = getFormatter(props);
  return {
    dataKey: camelCase(dataKey),
    position: camelCase(position) as LabelListProps<
      Record<string, unknown>
    >['position'],
    fill: fill && `var(--${fill.toLowerCase()})`,
    formatter,
    ...labelListProps,
  };
};

export const getFormatter = (defs: FormatterDefinitions) => {
  const { numberFormat } = defs;
  return (value: unknown) => {
    const dataFormatter = new DataFormatter();
    if (value === null || value === undefined) {
      return '';
    }

    if (typeof value === 'number' && numberFormat) {
      return dataFormatter.format(value, 'Number', numberFormat).value;
    }

    return value.toString();
  };
};
