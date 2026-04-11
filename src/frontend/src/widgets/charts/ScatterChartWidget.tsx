import React, { useCallback, useMemo, useRef } from "react";
import ReactECharts from "echarts-for-react";
import { getHeight, getWidth } from "@/lib/styles";
import { useThemeWithMonitoring } from "@/components/theme-provider";
import {
  generateEChartGrid,
  generateEChartLegend,
  generateTooltip,
  generateTextStyle,
  getColors,
  generateEChartToolbox,
} from "./sharedUtils";
import { getChartThemeColors } from "./styles";
import {
  ScatterChartWidgetProps,
  ChartType,
  ScatterProps,
  ScatterShape,
  ReferenceDot,
  MarkLine,
  MarkArea,
  XAxisProps,
  YAxisProps,
} from "./chartTypes";
import { Densities } from "@/types/density";

const EMPTY_ARRAY: never[] = [];

// Helper: resolve a value from a data record, trying the original key first, then camelCase.
// Needed because C# serializes property names as camelCase but DataKey string values stay PascalCase.
const resolveValue = (d: Record<string, unknown>, key: string): unknown => {
  if (key in d) return d[key];
  const camel = key.charAt(0).toLowerCase() + key.slice(1);
  if (camel in d) return d[camel];
  return undefined;
};

// Map ScatterShape to ECharts symbol types
const mapScatterShape = (shape: ScatterShape): string => {
  const mapping: Record<ScatterShape, string> = {
    Circle: "circle",
    Square: "rect",
    Cross: "none", // ECharts doesn't have cross, we'll use path for custom shapes
    Diamond: "diamond",
    Star: "none", // Will use path
    Triangle: "triangle",
    Wye: "none", // Will use path
  };
  return mapping[shape] || "circle";
};

// Generate custom SVG paths for shapes not supported by ECharts
const getCustomSymbol = (shape: ScatterShape): string | undefined => {
  switch (shape) {
    case "Cross":
      return "path://M-10,0 L10,0 M0,-10 L0,10";
    case "Star":
      return "path://M0,-10 L2,-3 L10,-3 L4,2 L6,10 L0,5 L-6,10 L-4,2 L-10,-3 L-2,-3 Z";
    case "Wye":
      return "path://M0,-10 L-3,-3 L-10,-3 L-4,2 L-6,10 L0,5 L6,10 L4,2 L10,-3 L3,-3 Z";
    default:
      return undefined;
  }
};

const generateScatterXAxis = (
  xAxis?: XAxisProps[],
  themeColors?: { mutedForeground: string; fontSans: string },
) => {
  const axisConfig = xAxis?.[0] || {};
  const dataKey = axisConfig.dataKey;

  // Map AxisTypes to ECharts types: Number -> value, Category -> category
  const mapAxisType = (type?: string) => {
    if (!type) return "value";
    const lower = type.toLowerCase();
    return lower === "number" ? "value" : lower;
  };

  return {
    type: mapAxisType(axisConfig.type),
    name: dataKey || "",
    nameLocation: "middle",
    nameGap: 30,
    axisLabel: {
      show: true,
      color: themeColors?.mutedForeground,
      fontFamily: themeColors?.fontSans,
    },
    axisLine: {
      show: axisConfig.axisLine === true,
      lineStyle: {
        type: "solid",
        color: themeColors?.mutedForeground,
        opacity: 0.3,
      },
    },
    axisTick: {
      show: axisConfig.tickLine === true,
      lineStyle: {
        color: themeColors?.mutedForeground,
        opacity: 0.4,
      },
    },
    splitLine: {
      show: true,
      lineStyle: {
        type: "dashed",
        color: themeColors?.mutedForeground,
        opacity: 0.2,
      },
    },
  };
};

const generateScatterYAxis = (
  yAxis?: YAxisProps[],
  themeColors?: { mutedForeground: string; fontSans: string },
) => {
  const axisConfig = yAxis?.[0] || {};
  const dataKey = axisConfig.dataKey;

  // Map AxisTypes to ECharts types: Number -> value, Category -> category
  const mapAxisType = (type?: string) => {
    if (!type) return "value";
    const lower = type.toLowerCase();
    return lower === "number" ? "value" : lower;
  };

  return {
    type: mapAxisType(axisConfig.type),
    name: dataKey || "",
    nameLocation: "middle",
    nameGap: 50,
    axisLabel: {
      show: true,
      color: themeColors?.mutedForeground,
      fontFamily: themeColors?.fontSans,
    },
    axisLine: {
      show: axisConfig.axisLine === true,
      lineStyle: {
        type: "solid",
        color: themeColors?.mutedForeground,
        opacity: 0.3,
      },
    },
    axisTick: {
      show: axisConfig.tickLine === true,
      lineStyle: {
        color: themeColors?.mutedForeground,
        opacity: 0.4,
      },
    },
    splitLine: {
      show: true,
      lineStyle: {
        type: "dashed",
        color: themeColors?.mutedForeground,
        opacity: 0.2,
      },
    },
  };
};

const generateScatterSeries = (
  data: Record<string, unknown>[],
  scatters?: ScatterProps[],
  xAxisDataKey?: string,
  yAxisDataKey?: string,
  zAxisDataKey?: string,
  zAxisConfig?: { rangeMin?: number; rangeMax?: number } | null,
  referenceDots?: ReferenceDot[],
  referenceLines?: MarkLine[],
  referenceAreas?: MarkArea[],
) => {
  if (!scatters || scatters.length === 0 || !xAxisDataKey || !yAxisDataKey) {
    return [];
  }

  // Convert C# ReferenceDot[] to ECharts markPoint format
  // C# sends: { x, y, label }
  const markPoint =
    referenceDots && referenceDots.length > 0
      ? {
          label: { show: true },
          data: referenceDots.map((d) => ({
            coord: [d.x, d.y],
            name: d.label,
          })),
        }
      : undefined;

  // Convert C# ReferenceLine[] to ECharts markLine format
  // C# sends: { x?, y?, label?, strokeWidth } — NOT ECharts MarkLine objects
  const markLine =
    referenceLines && referenceLines.length > 0
      ? {
          silent: true,
          symbol: ["none", "none"] as [string, string],
          label: { show: true, position: "end" as const },
          lineStyle: {
            type: "dashed" as const,
            // oxlint-disable-next-line @typescript-eslint/no-explicit-any
            width: (referenceLines[0] as any)?.strokeWidth ?? 1,
          },
          // oxlint-disable-next-line @typescript-eslint/no-explicit-any
          data: referenceLines.map((line: any) => {
            if (line.x != null && line.y == null) {
              return { xAxis: line.x, name: line.label };
            } else if (line.y != null && line.x == null) {
              return { yAxis: line.y, name: line.label };
            }
            // Both x and y specified — point-to-point line
            return [{ coord: [line.x, line.y], name: line.label }, { coord: [line.x, line.y] }];
          }),
        }
      : undefined;

  // Convert C# ReferenceArea[] to ECharts markArea format
  // C# sends: { x1, y1, x2, y2, label? }
  const markArea =
    referenceAreas && referenceAreas.length > 0
      ? {
          silent: true,
          // oxlint-disable-next-line @typescript-eslint/no-explicit-any
          data: referenceAreas.map((area: any) => [
            { xAxis: area.x1, yAxis: area.y1, name: area.label },
            { xAxis: area.x2, yAxis: area.y2 },
          ]),
        }
      : undefined;

  return scatters
    .map((scatter, index) => {
      const shape = scatter.shape || "Circle";
      const symbol = mapScatterShape(shape);
      const customPath = getCustomSymbol(shape);

      // Prepare data in [x, y, size] format for scatter
      const scatterData = data.map((d) => {
        const xVal = resolveValue(d, xAxisDataKey);
        const x = typeof xVal === "number" ? xVal : parseFloat(String(xVal || 0));
        const yVal = resolveValue(d, yAxisDataKey);
        const y = typeof yVal === "number" ? yVal : parseFloat(String(yVal || 0));
        const zVal = zAxisDataKey ? resolveValue(d, zAxisDataKey) : undefined;
        const z =
          zVal !== undefined
            ? typeof zVal === "number"
              ? zVal
              : parseFloat(String(zVal || 0))
            : undefined;

        return z !== undefined ? [x, y, z] : [x, y];
      });

      // Calculate symbol size
      let symbolSize: number | ((data: number[]) => number) = 8;
      if (zAxisDataKey && zAxisConfig) {
        const rangeMin = zAxisConfig.rangeMin || 60;
        const rangeMax = zAxisConfig.rangeMax || 400;

        // Extract all z values to find min/max
        const zValues = data
          .map((d) => {
            const v = zAxisDataKey ? resolveValue(d, zAxisDataKey) : undefined;
            return v !== undefined ? (typeof v === "number" ? v : parseFloat(String(v || 0))) : 0;
          })
          .filter((z) => z > 0);

        const minZ = Math.min(...zValues);
        const maxZ = Math.max(...zValues);

        // Create a function to map z values to symbol sizes
        symbolSize = (dataItem: number[]) => {
          const z = dataItem[2];
          if (z === undefined || minZ === maxZ) return (rangeMin + rangeMax) / 2;

          // Linear interpolation between rangeMin and rangeMax
          return rangeMin + ((z - minZ) / (maxZ - minZ)) * (rangeMax - rangeMin);
        };
      }

      const baseSeries = {
        name: scatter.name || scatter.dataKey,
        type: ChartType.Scatter as "scatter",
        data: scatterData,
        symbol: customPath || symbol,
        symbolSize,
        yAxisIndex: scatter.yAxisIndex ?? 0,
        itemStyle: {
          color: scatter.fill || undefined,
          opacity: scatter.fillOpacity !== null ? scatter.fillOpacity : 0.8,
          borderColor: scatter.stroke || undefined,
          borderWidth: scatter.strokeWidth || 1,
        },
        emphasis: {
          focus: "series",
          itemStyle: {
            borderWidth: 2,
            shadowBlur: 10,
            shadowColor: "rgba(0, 0, 0, 0.3)",
          },
        },
        animation: scatter.animated === true,
        animationDuration: 800,
        markPoint: index === 0 ? markPoint : undefined,
        markLine: index === 0 ? markLine : undefined,
        markArea: index === 0 ? markArea : undefined,
      };

      // If line is enabled, add a line series overlay
      if (scatter.line) {
        return [
          baseSeries,
          {
            name: `${scatter.name || scatter.dataKey} Line`,
            type: "line" as const,
            data: scatterData.map((d) => [d[0], d[1]]),
            showSymbol: false,
            smooth: scatter.lineType === "Fitting",
            lineStyle: {
              width: 1,
              opacity: 0.5,
              color: scatter.stroke || undefined,
              type: scatter.strokeDashArray ? "dashed" : "solid",
            },
            animation: false,
          },
        ];
      }

      return baseSeries;
    })
    .flat();
};

const ScatterChartWidget: React.FC<ScatterChartWidgetProps> = ({
  data = EMPTY_ARRAY,
  width = "Full",
  height = "Full",
  scatters = EMPTY_ARRAY,
  cartesianGrid,
  xAxis = EMPTY_ARRAY,
  yAxis = EMPTY_ARRAY,
  zAxis = null,
  tooltip,
  legend,
  toolbox,
  referenceLines = EMPTY_ARRAY,
  referenceAreas = EMPTY_ARRAY,
  referenceDots = EMPTY_ARRAY,
  colorScheme = "Default",
  density: _density = Densities.Medium,
}) => {
  // Use enhanced theme hook with automatic monitoring
  const { colors, isDark } = useThemeWithMonitoring({
    monitorDOM: false,
    monitorSystem: true,
  });

  // Extract chart-specific theme colors
  const themeColors = useMemo(() => getChartThemeColors(colors, isDark), [colors, isDark]);

  // When height is Full (100%), use flex to expand. Otherwise use explicit height.
  const heightStyle = height ? getHeight(height) : {};
  const isFull = height?.toLowerCase().startsWith("full");

  const styles: React.CSSProperties = {
    ...getWidth(width),
    position: "relative",
    ...(isFull ? { display: "flex", flexDirection: "column", height: "100%" } : {}),
  };

  const chartStyles: React.CSSProperties = {
    ...(isFull ? { flex: 1, minHeight: "200px" } : { ...heightStyle, minHeight: "200px" }),
    width: "100%",
  };

  // Chart colors depend on theme
  const chartColors = useMemo(() => getColors(colorScheme, colors), [colorScheme, colors]);

  const xAxisDataKey = xAxis[0]?.dataKey;
  const yAxisDataKey = yAxis[0]?.dataKey;
  const zAxisDataKey = zAxis?.dataKey;

  // Memoize option configuration
  const option = useMemo(
    () => ({
      grid: generateEChartGrid(cartesianGrid, !!toolbox && toolbox.enabled !== false, yAxis, xAxis),
      xAxis: generateScatterXAxis(xAxis, {
        mutedForeground: themeColors.mutedForeground,
        fontSans: themeColors.fontSans,
      }),
      yAxis: generateScatterYAxis(yAxis, {
        mutedForeground: themeColors.mutedForeground,
        fontSans: themeColors.fontSans,
      }),
      tooltip: generateTooltip(tooltip, "item", {
        foreground: themeColors.foreground,
        fontSans: themeColors.fontSans,
        background: themeColors.background,
        mutedForeground: themeColors.mutedForeground,
      }),
      toolbox: generateEChartToolbox(toolbox),
      legend: generateEChartLegend(legend, {
        foreground: themeColors.foreground,
        fontSans: themeColors.fontSans,
      }),
      textStyle: generateTextStyle(themeColors.foreground, themeColors.fontSans),
      color: chartColors,
      series: generateScatterSeries(
        data,
        scatters,
        xAxisDataKey,
        yAxisDataKey,
        zAxisDataKey,
        zAxis,
        referenceDots,
        referenceLines,
        referenceAreas,
      ),
    }),
    [
      cartesianGrid,
      data,
      xAxis,
      yAxis,
      zAxis,
      themeColors,
      tooltip,
      legend,
      chartColors,
      scatters,
      xAxisDataKey,
      yAxisDataKey,
      zAxisDataKey,
      referenceDots,
      referenceLines,
      referenceAreas,
      toolbox,
    ],
  );

  const containerRef = useRef<HTMLDivElement>(null);
  const handleChartReady = useCallback(() => {
    containerRef.current?.setAttribute("data-chart-rendered", "true");
  }, []);

  return (
    <div ref={containerRef} style={styles}>
      <ReactECharts
        option={option}
        style={chartStyles}
        notMerge={true}
        lazyUpdate={true}
        onChartReady={handleChartReady}
      />
    </div>
  );
};

export default ScatterChartWidget;
