import {
  CartesianGridProps,
  ChartType,
  LegendProps,
  LinesProps,
  MarkArea,
  MarkLine,
  ReferenceDot,
  ToolTipProps,
  ToolboxFeatures,
  ToolboxProps,
  XAxisProps,
  YAxisProps,
} from "./chartTypes";
import { ChartData } from "./chartTypes";
import { generateTextStyle, generateAxisLabelStyle, type ChartThemeColors } from "./styles/theme";
import { format as dateFnsFormat } from "date-fns";
import { TZDate } from "@date-fns/tz";
import {
  CARTESIAN_GRID_DEFAULTS,
  LEGEND_DEFAULTS,
  TOOLTIP_DEFAULTS,
  TOOLBOX_DEFAULTS,
  LINE_DEFAULTS,
  REFERENCE_LINE_DEFAULTS,
  applyDefaults,
} from "./chartDefaults";

// Re-export from styles
export type { ColorScheme } from "./styles/colors";
export { getChartColors as getColors } from "./styles/colors";
export { generateTextStyle, generateAxisLabelStyle, type ChartThemeColors };

/** Default `name` on markLine data when missing (ECharts shows nothing if empty). */
function enrichMarkLineDatum(item: {
  name?: string;
  xAxis?: number | string;
  yAxis?: number;
  type?: "min" | "max" | "average";
  value?: number;
}) {
  if (item.name != null && String(item.name).trim() !== "") return item;
  if (item.type === "min" || item.type === "max" || item.type === "average") return item;
  if (item.xAxis != null && item.yAxis == null) return { ...item, name: String(item.xAxis) };
  if (item.yAxis != null && item.xAxis == null) return { ...item, name: String(item.yAxis) };
  if (item.value != null) return { ...item, name: String(item.value) };
  return item;
}

export const getAxisDomainBound = (
  type: "min" | "max",
  rawValue: unknown,
  allowDataOverflow: boolean,
  transform: (v: number) => number = (v) => v,
) => {
  if (rawValue == null) return undefined;

  const extractDomainValue = (val: unknown) => {
    if (val && typeof val === "object") {
      if ("value" in val) return (val as Record<string, unknown>).value;
      if ("Value" in val) return (val as Record<string, unknown>).Value;
    }
    return val;
  };

  const extracted = extractDomainValue(rawValue);
  if (extracted === "auto" || extracted == null) return undefined; // Let ECharts auto-scale

  const parseVal = (v: unknown) => {
    if (v === "dataMin" || v === "dataMax") return v;
    const num = Number(v);
    return isNaN(num) ? undefined : transform(num);
  };

  const parsedValue = parseVal(extracted);
  if (parsedValue === undefined) return undefined;

  if (allowDataOverflow === true) return parsedValue;

  return (value: { min: number; max: number }) => {
    if (parsedValue === "dataMin") return value.min;
    if (parsedValue === "dataMax") return value.max;
    return type === "min"
      ? Math.min(parsedValue as number, value.min)
      : Math.max(parsedValue as number, value.max);
  };
};

export const formatTickLabel = (
  value: number | string,
  formatter?: string | null,
  timeZone?: string | null,
  formatterType?: "Auto" | "Number" | "Date" | null,
) => {
  if (!formatter) return String(value);

  const type = formatterType ?? "Auto";

  // Number formatting helpers
  const tryNumberFormat = (): string | null => {
    if (formatter.startsWith("C")) {
      const parts = formatter.split(":");
      const currency = parts.length > 1 ? parts[1] : "USD";
      const fractionDigits = parseInt(parts[0].substring(1));
      return new Intl.NumberFormat(undefined, {
        style: "currency",
        currency,
        maximumFractionDigits: isNaN(fractionDigits) ? 0 : fractionDigits,
      }).format(Number(value));
    }
    if (formatter.startsWith("P")) {
      const fractionDigits = parseInt(formatter.substring(1));
      return new Intl.NumberFormat(undefined, {
        style: "percent",
        maximumFractionDigits: isNaN(fractionDigits) ? 0 : fractionDigits,
      }).format(Number(value) / 100);
    }
    if (formatter.startsWith("N") || formatter.startsWith("F")) {
      const fractionDigits = parseInt(formatter.substring(1));
      return new Intl.NumberFormat(undefined, {
        maximumFractionDigits: isNaN(fractionDigits) ? 2 : fractionDigits,
      }).format(Number(value));
    }
    if (formatter === "#,##0,,M") {
      return (Number(value) / 1000000).toFixed(0) + "M";
    }
    if (formatter === "#,##0,K") {
      return (Number(value) / 1000).toFixed(0) + "K";
    }
    return null;
  };

  // Date formatting helper
  const tryDateFormat = (): string | null => {
    if (/(?:^|[^a-zA-Z])(?:yyyy|yy|MMMM|MMM|MM|dd|d|HH|hh|mm|ss)(?:[^a-zA-Z]|$)/.test(formatter)) {
      try {
        const tz =
          timeZone === "local"
            ? Intl.DateTimeFormat().resolvedOptions().timeZone
            : timeZone || "UTC";
        const date = new TZDate(new Date(value), tz);
        if (!isNaN(date.getTime())) {
          return dateFnsFormat(date, formatter);
        }
      } catch {
        // Fall through
      }
    }
    return null;
  };

  if (type === "Number") {
    return tryNumberFormat() ?? String(value);
  }

  if (type === "Date") {
    return tryDateFormat() ?? String(value);
  }

  // Auto: try number first, then date (original behavior)
  return tryNumberFormat() ?? tryDateFormat() ?? String(value);
};

export const generateDataProps = (data: Record<string, unknown>[]) => {
  if (data.length === 0) {
    return { categoryKey: "", categories: [], valueKeys: [] };
  }

  // Find the category key (dimension) - prioritize strings, but accept other types
  // Assumption: the dimension is typically the first non-numeric key, or the first key overall
  const keys = Object.keys(data[0]);
  let categoryKey = keys.find((k) => typeof data[0][k] === "string");

  // If no string key found, use the first key (handles integer years, dates, etc.)
  if (!categoryKey && keys.length > 0) {
    categoryKey = keys[0];
  }

  if (!categoryKey) {
    return { categoryKey: "", categories: [], valueKeys: [] };
  }

  // Extract categories - convert all values to strings for display
  const categories = data.map((d) => String(d[categoryKey]));

  // Value keys are all numeric fields except the category key
  const valueKeys = keys.filter((k) => k !== categoryKey && typeof data[0][k] === "number");

  const allValues = data.flatMap((d) =>
    Object.values(d).filter((v) => typeof v === "number"),
  ) as number[];
  const minValue = Math.min(...allValues);
  const maxValue = Math.max(...allValues);
  const largeSpread = Math.abs(maxValue / (minValue === 0 ? 1 : minValue)) > 1e5;

  const transform = (v: number) => {
    if (!largeSpread) return v;
    const sign = Math.sign(v);
    return sign * Math.log10(Math.abs(v) + 1);
  };
  return {
    categoryKey,
    categories,
    valueKeys,
    transform,
    largeSpread,
    minValue,
    maxValue,
  };
};

export function generateEChartGrid(
  cartesianGrid?: CartesianGridProps,
  hasToolbox: boolean = false,
  yAxis?: YAxisProps[],
  xAxis?: XAxisProps[],
) {
  // When all Y axes are hidden, remove left/right padding so the chart uses full width
  const allYAxesHidden = yAxis && yAxis.length > 0 && yAxis.every((axis) => axis.hide === true);

  // When all X axes are hidden, remove bottom padding
  const allXAxesHidden = xAxis && xAxis.length > 0 && xAxis.every((axis) => axis.hide === true);

  const defaultGrid = {
    show: false, // Hide grid border to remove the square frame
    left: allYAxesHidden ? (allXAxesHidden ? 0 : 15) : "3%",
    right: allYAxesHidden ? (allXAxesHidden ? 0 : 15) : "4%",
    top: hasToolbox ? 40 : 15,
    bottom: allXAxesHidden ? 10 : 50, // Reduce bottom padding when X axis is hidden
    containLabel: !allYAxesHidden,
    borderWidth: 0, // Ensure no border is drawn
  };

  if (!cartesianGrid) return defaultGrid;

  // Apply defaults for cartesianGrid
  const grid = applyDefaults(cartesianGrid, CARTESIAN_GRID_DEFAULTS);

  return {
    ...defaultGrid,
    show: grid.vertical || grid.horizontal,
    ...(grid.width != null && { width: grid.width }),
    ...(grid.height != null && { height: grid.height }),
    ...(grid.x != null && { x: grid.x }),
    ...(grid.y != null && { y: grid.y }),
  };
}

export function generateEChartLegend(
  legend?: LegendProps,
  themeColors?: { foreground: string; fontSans: string },
) {
  const defaultLegends = {
    type: "scroll",
    show: true,
    textStyle: generateTextStyle(themeColors?.foreground, themeColors?.fontSans),
    top: "bottom",
  };
  if (!legend) return defaultLegends;

  // Apply defaults for legend
  const leg = applyDefaults(legend, LEGEND_DEFAULTS);

  return {
    ...defaultLegends,
    icon: leg.iconType ? leg.iconType : "rect",
    itemWidth: leg.iconSize ?? LEGEND_DEFAULTS.iconSize,
    itemHeight: leg.iconSize ?? LEGEND_DEFAULTS.iconSize,
    orient: leg.layout?.toLowerCase(),
    top: leg.verticalAlign === "Bottom" ? "bottom" : "top",
  };
}

export const getTransformValueFn = (data: ChartData[]) => {
  const allValues = data.flatMap((d) =>
    Object.values(d).filter((v) => typeof v === "number"),
  ) as number[];
  const minValue = Math.min(...allValues);
  const maxValue = Math.max(...allValues);
  const largeSpread = Math.abs(maxValue / (minValue === 0 ? 1 : minValue)) > 1e5;

  const transform = (v: number) => {
    if (!largeSpread) return v;
    const sign = Math.sign(v);
    return sign * Math.log10(Math.abs(v) + 1);
  };

  return { transform, largeSpread, minValue, maxValue };
};

/**
 * C# `ReferenceLine` serializes as `{ x?, y?, label?, strokeWidth? }`.
 * ECharts `markLine` expects `{ data: [...] }`. Scatter charts already normalized
 * this in the widget; line/area/bar used to assume pre-built MarkLine only.
 */
export const buildMarkLineConfig = (
  referenceLines: MarkLine[] | undefined,
  theme?: ChartThemeColors,
): Record<string, unknown> => {
  if (!referenceLines?.length) return {};

  const first = referenceLines[0] as unknown as Record<string, unknown>;
  const usesEChartsMarkLineShape =
    first != null && "data" in first && Array.isArray(first.data as unknown[]);

  // Same as axis tick labels (`generateXAxis` / `generateYAxis`); resolved colors — canvas ignores `var()`.
  const markLineLabel = {
    show: true,
    position: "end" as const,
    ...generateAxisLabelStyle(theme?.mutedForeground ?? "#888888", theme?.fontSans ?? "sans-serif"),
    textBorderWidth: 0,
  };

  if (usesEChartsMarkLineShape) {
    const ml0 = referenceLines[0] as MarkLine & { label?: Record<string, unknown> };
    const rawData = referenceLines.flatMap((ml) => (ml as MarkLine).data ?? []);
    return {
      ...referenceLines[0],
      lineStyle: {
        width: referenceLines[0]?.lineStyle?.width ?? REFERENCE_LINE_DEFAULTS.strokeWidth,
        ...referenceLines[0]?.lineStyle,
      },
      data: rawData.map((d) => enrichMarkLineDatum(d)),
      label: {
        ...ml0.label,
        ...markLineLabel,
      },
    };
  }

  const strokeWidth =
    (first?.strokeWidth as number | undefined) ?? REFERENCE_LINE_DEFAULTS.strokeWidth;
  return {
    silent: true,
    symbol: ["none", "none"] as [string, string],
    label: markLineLabel,
    lineStyle: {
      type: "dashed" as const,
      width: strokeWidth,
    },
    data: referenceLines
      .map((line) => {
        const l = line as unknown as Record<string, unknown>;
        const x = l.x ?? l.X;
        const y = l.y ?? l.Y;
        const labelText = l.label ?? l.Label;
        const nameFromLabel =
          typeof labelText === "string" && labelText.trim() !== "" ? labelText : undefined;
        if (x != null && y == null) {
          return { xAxis: x as number | string, name: nameFromLabel ?? String(x) };
        }
        if (y != null && x == null) {
          return { yAxis: y as number, name: nameFromLabel ?? String(y) };
        }
        if (x != null && y != null) {
          const name = nameFromLabel ?? `${x}, ${y}`;
          return [
            { coord: [x, y] as [number, number], name },
            { coord: [x, y] as [number, number] },
          ];
        }
        return null;
      })
      .flat()
      .filter(Boolean),
  };
};

// Text and axis styles are now imported from './styles/theme'

export const generateSeries = (
  data: ChartData[],
  valueKeys: string[],
  lines?: LinesProps[],
  transform?: (v: number) => number,
  referenceDots?: ReferenceDot[],
  referenceLines?: MarkLine[],
  referenceAreas?: MarkArea[],
  markLineTheme?: ChartThemeColors,
) => {
  // Convert ReferenceDot[] to ECharts markPoint format
  const markPoint =
    referenceDots && referenceDots.length > 0
      ? {
          data: referenceDots.map((d) => ({
            coord: [d.x, d.y],
            name: d.label,
          })),
        }
      : {};

  const markLine = buildMarkLineConfig(referenceLines, markLineTheme);

  // Merge MarkArea[] into single markArea config
  const markArea =
    referenceAreas && referenceAreas.length > 0
      ? {
          ...referenceAreas[0],
          data: referenceAreas.flatMap((ma) => ma.data),
        }
      : {};

  // When explicit series are configured, only plot those data keys
  // Use case-insensitive matching because backend serializes data keys as camelCase
  // but Line.dataKey preserves the original PascalCase measure name
  const configuredKeys = (lines || []).map((l) => l.dataKey).filter(Boolean);
  const configuredKeysLower = configuredKeys.map((k) => k.toLowerCase());
  const keysToPlot =
    configuredKeysLower.length > 0
      ? valueKeys.filter((k) => configuredKeysLower.includes(k.toLowerCase()))
      : valueKeys;

  return keysToPlot.map((key) => {
    const rawLineConfig = lines?.find((l) => l.dataKey?.toLowerCase() === key.toLowerCase());
    // Apply defaults for line config
    const lineConfig = rawLineConfig ? applyDefaults(rawLineConfig, LINE_DEFAULTS) : LINE_DEFAULTS;

    return {
      name: lineConfig.name || key,
      type: ChartType.Line,
      data: data.map((d) => (transform ? transform(Number(d[key] ?? 0)) : Number(d[key] ?? 0))),
      step: lineConfig.curveType === "Step" ? "middle" : false,
      smooth: lineConfig.curveType === "Natural" ? true : false,
      showSymbol: true,
      symbolSize: 6,
      lineStyle: {
        width: lineConfig.strokeWidth ?? LINE_DEFAULTS.strokeWidth,
        opacity: 0.9,
        type: lineConfig.strokeDashArray ? "dashed" : "solid",
        color: lineConfig.stroke ?? undefined,
      },
      emphasis: {
        focus: "series",
        disabled: true,
        lineStyle: { width: 3, opacity: 1 },
        itemStyle: { borderWidth: 2, borderColor: "var(--background, #fff)" },
      },
      blur: { lineStyle: { opacity: 0.6 } },
      animation: true,
      animationDuration: 800,
      connectNulls: lineConfig.connectNulls ?? LINE_DEFAULTS.connectNulls,
      markPoint,
      markLine,
      markArea,
    };
  });
};

export const generateXAxis = (
  chartType: string,
  categories: string[],
  xAxis?: XAxisProps[],
  isVertical?: boolean,
  themeColors?: { mutedForeground: string; fontSans: string },
  cartesianGrid?: CartesianGridProps,
) => {
  const axis = xAxis?.[0] || ({} as XAxisProps);
  const allowDataOverflow = axis.allowDataOverflow ?? false;

  const minOpt = getAxisDomainBound("min", axis.domainMin, allowDataOverflow);
  const maxOpt = getAxisDomainBound("max", axis.domainMax, allowDataOverflow);

  // Map scale to ECharts axis type when explicitly set
  const scaleType = axis.scale && axis.scale !== "Auto" ? axis.scale.toLowerCase() : undefined;

  return {
    show: !axis.hide,
    position: axis.orientation?.toLowerCase() === "top" ? "top" : "bottom",
    type: scaleType ?? (isVertical ? "category" : "value"),
    boundaryGap: chartType === "bar" ? true : false,
    data: isVertical ? categories : undefined,
    inverse: axis.reversed ?? false,
    ...(axis.name != null && { name: axis.name }),
    ...(minOpt !== undefined && { min: minOpt }),
    ...(maxOpt !== undefined && { max: maxOpt }),
    ...(axis.tickCount != null && { splitNumber: axis.tickCount }),
    axisLabel: {
      show: axis.hideTickLabels ? false : true,
      rotate: axis.angle ?? 0,
      formatter: isVertical
        ? (value: string | number) => {
            const formatted = axis.tickFormatter
              ? formatTickLabel(value, axis.tickFormatter, axis.timeZone, axis.tickFormatterType)
              : String(value).length > 10
                ? String(value)
                    .match(/.{1,10}/g)
                    ?.join("\n")
                : String(value);
            return axis.unit ? `${formatted}${axis.unit}` : formatted;
          }
        : (value: number | string) => {
            let formatted: string;
            if (axis.tickFormatter) {
              formatted = formatTickLabel(
                value,
                axis.tickFormatter,
                axis.timeZone,
                axis.tickFormatterType,
              );
            } else {
              const numVal = Number(value);
              if (Math.abs(numVal) >= 1e9) formatted = (numVal / 1e9).toFixed(0) + "B";
              else if (Math.abs(numVal) >= 1e6) formatted = (numVal / 1e6).toFixed(0) + "M";
              else if (Math.abs(numVal) >= 1e3) formatted = (numVal / 1e3).toFixed(0) + "K";
              else formatted = String(value);
            }
            return axis.unit ? `${formatted}${axis.unit}` : formatted;
          },
      ...(axis.minTickGap != null && { interval: axis.minTickGap }),
      ...(!axis.minTickGap && { interval: "auto" as const }),
      ...generateAxisLabelStyle(themeColors?.mutedForeground, themeColors?.fontSans),
    },
    axisLine: {
      show: axis.axisLine === true,
      lineStyle: {
        type: "dashed",
        color: themeColors?.mutedForeground,
        opacity: 0.1,
      },
    },
    axisTick: {
      show: axis.tickLine === true,
      length: axis.tickSize ?? 6,
      lineStyle: {
        color: themeColors?.mutedForeground,
        opacity: 0.4,
      },
    },
    splitLine: {
      show: true,
      lineStyle: {
        type: "dashed",
        color: cartesianGrid?.stroke ?? themeColors?.mutedForeground,
        opacity: 0.4,
      },
    },
    axisPointer: {
      label: {
        ...(axis.tickFormatter && {
          formatter: (params: { value: string | number }) =>
            formatTickLabel(
              params.value,
              axis.tickFormatter!,
              axis.timeZone,
              axis.tickFormatterType,
            ),
        }),
      },
    },
  };
};

export const generateYAxis = (
  largeSpread: boolean = false,
  transformValue?: (v: number) => number,
  minValue: number = 0,
  maxValue: number = 100,
  yAxis?: YAxisProps[],
  isVertical: boolean = false,
  categories?: string[],
  themeColors?: { mutedForeground: string; fontSans: string },
  cartesianGrid?: CartesianGridProps,
) => {
  const safeTransform = transformValue ?? ((v: number) => v);

  const buildAxisConfig = (axis: YAxisProps, skipLargeSpread: boolean = false) => {
    const effectiveLargeSpread = largeSpread && !skipLargeSpread;
    const allowDataOverflow = axis.allowDataOverflow ?? false;

    let minOpt = getAxisDomainBound("min", axis.domainMin, allowDataOverflow, safeTransform);
    let maxOpt = getAxisDomainBound("max", axis.domainMax, allowDataOverflow, safeTransform);

    if (effectiveLargeSpread) {
      if (minOpt === undefined) minOpt = safeTransform(minValue);
      if (maxOpt === undefined) maxOpt = safeTransform(maxValue);
    }

    // Map scale to ECharts axis type when explicitly set
    const scaleType = axis.scale && axis.scale !== "Auto" ? axis.scale.toLowerCase() : undefined;

    return {
      show: axis.hide ? false : true,
      type: scaleType ?? (isVertical ? "value" : "category"),
      data: isVertical ? undefined : categories,
      inverse: axis.reversed ?? false,
      ...(axis.name != null && { name: axis.name }),
      ...(minOpt !== undefined && { min: minOpt }),
      ...(maxOpt !== undefined && { max: maxOpt }),
      axisLabel: {
        show: axis.hideTickLabels ? false : true,
        rotate: axis.angle ?? 0,
        formatter: (value: number) => {
          let formatted: string | number;
          if (axis.tickFormatter) {
            formatted = formatTickLabel(
              value,
              axis.tickFormatter,
              axis.timeZone,
              axis.tickFormatterType,
            );
          } else if (effectiveLargeSpread) {
            const unscaled = Math.sign(value) * (10 ** Math.abs(value) - 1);
            if (Math.abs(unscaled) >= 1e9) formatted = (unscaled / 1e9).toFixed(0) + "B";
            else if (Math.abs(unscaled) >= 1e6) formatted = (unscaled / 1e6).toFixed(0) + "M";
            else if (Math.abs(unscaled) >= 1e3) formatted = (unscaled / 1e3).toFixed(0) + "K";
            else formatted = unscaled.toFixed(0);
          } else {
            if (Math.abs(value) >= 1e9) formatted = (value / 1e9).toFixed(0) + "B";
            else if (Math.abs(value) >= 1e6) formatted = (value / 1e6).toFixed(0) + "M";
            else if (Math.abs(value) >= 1e3) formatted = (value / 1e3).toFixed(0) + "K";
            else formatted = value;
          }
          return axis.unit ? `${formatted}${axis.unit}` : formatted;
        },
        ...(axis.minTickGap != null && { interval: axis.minTickGap }),
        ...(!axis.minTickGap && {}),
        ...generateAxisLabelStyle(themeColors?.mutedForeground, themeColors?.fontSans),
      },
      splitNumber: axis.tickCount ?? (effectiveLargeSpread ? 3 : 5),
      position: axis.orientation === "Right" ? "right" : "left",
      axisLine: {
        show: axis.axisLine === true,
        lineStyle: {
          type: "dashed",
          color: themeColors?.mutedForeground,
          opacity: 0.1,
        },
      },
      axisTick: {
        show: axis.tickLine === true,
        length: axis.tickSize ?? 6,
        lineStyle: {
          color: themeColors?.mutedForeground,
          opacity: 0.4,
        },
      },
      splitLine: {
        show: true,
        lineStyle: {
          type: "dashed",
          color: cartesianGrid?.stroke ?? themeColors?.mutedForeground,
          opacity: 0.4,
        },
      },
      axisPointer: {
        label: {
          ...(axis.tickFormatter && {
            formatter: (params: { value: string | number }) =>
              formatTickLabel(
                params.value,
                axis.tickFormatter!,
                axis.timeZone,
                axis.tickFormatterType,
              ),
          }),
        },
      },
    };
  };

  if (yAxis && yAxis.length > 1) {
    // Each axis auto-scales independently; global largeSpread is misleading for multi-axis charts
    return yAxis.map((axis) => buildAxisConfig(axis, /* skipLargeSpread: */ true));
  }

  return buildAxisConfig(yAxis?.[0] || ({} as YAxisProps));
};

export const generateTooltip = (
  tooltip?: ToolTipProps,
  type?: string,
  themeColors?: {
    foreground: string;
    fontSans: string;
    background: string;
    mutedForeground?: string;
  },
) => {
  // Apply defaults for tooltip
  const tip = tooltip ? applyDefaults(tooltip, TOOLTIP_DEFAULTS) : TOOLTIP_DEFAULTS;

  return {
    trigger: type === "item" ? "item" : "axis",
    appendToBody: true,
    axisPointer: {
      type: type === "item" ? "cross" : (type ?? "cross"),
      animated: tip.animated ?? TOOLTIP_DEFAULTS.animated,
      shadowStyle: { opacity: 0.5 },
      lineStyle: {
        color: themeColors?.mutedForeground,
      },
      crossStyle: {
        color: themeColors?.mutedForeground,
      },
      label: {
        backgroundColor: themeColors?.mutedForeground,
        color: "rgba(255, 255, 255, 0.9)",
      },
    },
    textStyle: generateTextStyle(themeColors?.foreground, themeColors?.fontSans),
    backgroundColor: themeColors?.background || "rgba(255, 255, 255, 0.9)",
    borderColor: themeColors?.foreground || "#000",
    borderWidth: 1,
  };
};

export const generateEChartToolbox = (toolbox?: ToolboxProps) => {
  if (!toolbox || toolbox.enabled === false) {
    return { show: false };
  }

  const box = applyDefaults(toolbox, TOOLBOX_DEFAULTS);

  const features: ToolboxFeatures = {};

  if (box.dataView === true) {
    const cardBg = getComputedStyle(document.documentElement).getPropertyValue("--card").trim();
    const foreground = getComputedStyle(document.documentElement)
      .getPropertyValue("--foreground")
      .trim();
    const mutedForeground = getComputedStyle(document.documentElement)
      .getPropertyValue("--muted-foreground")
      .trim();

    features.dataView = {
      show: true,
      readOnly: false,
      backgroundColor: cardBg,
      textareaColor: cardBg,
      textColor: foreground,
      buttonColor: mutedForeground,
      buttonTextColor: "rgba(255, 255, 255, 0.9)",
    };
  }

  if (box.magicType === true) {
    features.magicType = {
      show: true,
      type: ["line", "bar"],
    };
  }

  if (box.saveAsImage === true) {
    features.saveAsImage = {
      show: true,
    };
  }

  return {
    show: true,
    orient: box.orientation?.toLowerCase() === "vertical" ? "vertical" : "horizontal",
    left:
      box.align?.toLowerCase() === "left"
        ? "left"
        : box.align?.toLowerCase() === "center"
          ? "center"
          : "right",
    top:
      box.verticalAlign?.toLowerCase() === "top"
        ? 0
        : box.verticalAlign?.toLowerCase() === "middle"
          ? "middle"
          : "bottom",
    feature: features,
    iconStyle: {
      borderColor: getComputedStyle(document.documentElement)
        .getPropertyValue("--muted-foreground")
        .trim(),
    },
    emphasis: {
      iconStyle: {
        color: null,
        borderColor: null,
        textFill: getComputedStyle(document.documentElement).getPropertyValue("--toolbox").trim(),
      },
    },
  };
};
