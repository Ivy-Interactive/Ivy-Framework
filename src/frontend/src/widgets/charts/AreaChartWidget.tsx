import React, { useCallback, useMemo, useRef } from "react";
import { ColorScheme, generateEChartToolbox } from "./sharedUtils";
import { getHeight, getWidth } from "@/lib/styles";
import { useThemeWithMonitoring } from "@/components/theme-provider";
import ReactECharts from "echarts-for-react";
import {
  generateDataProps,
  getColors,
  generateXAxis,
  generateEChartLegend,
  generateTooltip,
  generateTextStyle,
  generateEChartGrid,
  generateYAxis,
} from "./sharedUtils";
import { generateGradientColors, getChartThemeColors } from "./styles";
import {
  ChartType,
  XAxisProps,
  YAxisProps,
  LinesProps,
  MarkLine,
  MarkArea,
  LegendProps,
  CartesianGridProps,
  ToolTipProps,
  ToolboxProps,
} from "./chartTypes";
import { ChartData } from "./chartTypes";
import { getTransformValueFn } from "./sharedUtils";
import { ReferenceDot } from "./chartTypes";
import { LINE_DEFAULTS, REFERENCE_LINE_DEFAULTS, applyDefaults } from "./chartDefaults";

const EMPTY_ARRAY: never[] = [];

interface AreaChartWidgetProps {
  id: string;
  data: ChartData[];
  width?: string;
  height?: string;
  areas?: LinesProps[];
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
  layout?: "Horizontal" | "Vertical";
}

const AreaChartWidget: React.FC<AreaChartWidgetProps> = ({
  data = EMPTY_ARRAY,
  width = "Full",
  height = "Full",
  areas = EMPTY_ARRAY,
  cartesianGrid,
  xAxis = EMPTY_ARRAY,
  yAxis = EMPTY_ARRAY,
  tooltip,
  toolbox,
  legend,
  referenceLines = EMPTY_ARRAY,
  referenceAreas = EMPTY_ARRAY,
  referenceDots = EMPTY_ARRAY,
  colorScheme = "Default",
  layout = "Vertical",
}) => {
  // Use enhanced theme hook with automatic monitoring
  const { colors, isDark } = useThemeWithMonitoring({
    monitorDOM: false, // Disabled to prevent excessive re-renders from MutationObserver
    monitorSystem: true, // Keep system theme monitoring for light/dark mode switching
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

  const { categories, valueKeys } = generateDataProps(data);

  // Chart colors depend on theme (chromatic colors automatically adapt to light/dark mode)
  const chartColors = useMemo(() => getColors(colorScheme, colors), [colorScheme, colors]);
  const isVertical = layout?.toLowerCase() === "vertical";

  const { transform, largeSpread, minValue, maxValue } = getTransformValueFn(data);

  // Memoize gradient colors
  const gradientColors = useMemo(() => generateGradientColors(chartColors, 0.4), [chartColors]);

  // Convert ReferenceDot[] to ECharts markPoint format
  const markPoint = useMemo(
    () =>
      referenceDots.length > 0
        ? {
            data: referenceDots.map((d) => ({
              coord: [d.x, d.y],
              name: d.label,
            })),
          }
        : {},
    [referenceDots],
  );

  // Merge MarkLine[] into single markLine config with C# defaults
  const markLine = useMemo(
    () =>
      referenceLines.length > 0
        ? {
            ...referenceLines[0],
            lineStyle: {
              width: referenceLines[0]?.lineStyle?.width ?? REFERENCE_LINE_DEFAULTS.strokeWidth,
              ...referenceLines[0]?.lineStyle,
            },
            data: referenceLines.flatMap((ml) => ml.data),
          }
        : {},
    [referenceLines],
  );

  // Merge MarkArea[] into single markArea config
  const markAreaConfig = useMemo(
    () =>
      referenceAreas.length > 0
        ? {
            ...referenceAreas[0],
            data: referenceAreas.flatMap((ma) => ma.data),
          }
        : {},
    [referenceAreas],
  );

  // When explicit series are configured, only plot those data keys
  const configuredAreaKeys = (areas || []).map((a) => a.dataKey).filter(Boolean);
  const areaKeysToPlot =
    configuredAreaKeys.length > 0
      ? valueKeys.filter((k) =>
          configuredAreaKeys.some((ck) => ck.toLowerCase() === k.toLowerCase()),
        )
      : valueKeys;

  // Memoize series configuration
  const series = useMemo(
    () =>
      areaKeysToPlot.map((key, i) => {
        const rawAreaConfig = areas?.find((a) => a.dataKey.toLowerCase() === key.toLowerCase());
        // Apply C# defaults for area config
        const areaConfig = rawAreaConfig
          ? applyDefaults(rawAreaConfig, LINE_DEFAULTS)
          : LINE_DEFAULTS;

        return {
          name: areaConfig.name || key,
          type: ChartType.Line,
          smooth: areaConfig.curveType?.toLowerCase() === "natural",
          lineStyle: {
            width: areaConfig.strokeWidth ?? LINE_DEFAULTS.strokeWidth,
            color: areaConfig.stroke ?? chartColors[i],
            type: areaConfig.strokeDashArray ? "dashed" : "solid",
          },
          showSymbol: false,
          areaStyle: gradientColors[i],
          emphasis: { focus: "series" },
          data: data.map((d) => d[key]),
          connectNulls: areaConfig.connectNulls ?? LINE_DEFAULTS.connectNulls,
          markPoint,
          markLine,
          markArea: markAreaConfig,
        };
      }),
    [areaKeysToPlot, areas, chartColors, gradientColors, data, markPoint, markLine, markAreaConfig],
  );

  // Memoize complete option configuration
  const option = useMemo(
    () => ({
      grid: generateEChartGrid(cartesianGrid, !!toolbox && toolbox.enabled !== false, yAxis, xAxis),
      color: chartColors,
      tooltip: generateTooltip(tooltip, "cross", {
        foreground: themeColors.foreground,
        fontSans: themeColors.fontSans,
        background: themeColors.background,
        mutedForeground: themeColors.mutedForeground,
      }),
      legend: generateEChartLegend(legend, {
        foreground: themeColors.foreground,
        fontSans: themeColors.fontSans,
      }),
      toolbox: generateEChartToolbox(toolbox),
      textStyle: generateTextStyle(themeColors.foreground, themeColors.fontSans),
      xAxis: generateXAxis(
        ChartType.Line,
        categories as string[],
        xAxis,
        isVertical,
        {
          mutedForeground: themeColors.mutedForeground,
          fontSans: themeColors.fontSans,
        },
        cartesianGrid,
      ),
      yAxis: generateYAxis(
        largeSpread,
        transform,
        minValue,
        maxValue,
        yAxis,
        isVertical,
        undefined,
        {
          mutedForeground: themeColors.mutedForeground,
          fontSans: themeColors.fontSans,
        },
        cartesianGrid,
      ),
      series: series,
    }),
    [
      cartesianGrid,
      chartColors,
      tooltip,
      themeColors.foreground,
      themeColors.fontSans,
      themeColors.background,
      themeColors.mutedForeground,
      legend,
      toolbox,
      categories,
      xAxis,
      isVertical,
      largeSpread,
      transform,
      minValue,
      maxValue,
      yAxis,
      series,
      layout,
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
        notMerge={true} // Merge changes instead of full rebuild for better performance
        lazyUpdate={true}
        onChartReady={handleChartReady}
      />
    </div>
  );
};

export default AreaChartWidget;
