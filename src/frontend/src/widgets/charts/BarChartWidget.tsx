import React, { useCallback, useMemo, useRef } from "react";
import {
  ColorScheme,
  buildMarkLineConfig,
  generateDataProps,
  generateEChartGrid,
  generateEChartLegend,
  generateEChartToolbox,
  generateTooltip,
  generateTextStyle,
  generateXAxis,
  generateYAxis,
  getColors,
  formatTooltipValue,
} from "./sharedUtils";
import { useThemeWithMonitoring } from "@/components/theme-provider";
import { getHeight, getWidth } from "@/lib/styles";
import ReactECharts from "echarts-for-react";
import { getChartThemeColors } from "./styles";
import {
  BarProps,
  CartesianGridProps,
  ChartType,
  LegendProps,
  MarkArea,
  MarkLine,
  ReferenceDot,
  ToolTipProps,
  ToolboxProps,
  XAxisProps,
  YAxisProps,
} from "./chartTypes";
import { ChartData } from "./chartTypes";
import { BAR_DEFAULTS, applyDefaults } from "./chartDefaults";
import { Densities } from "@/types/density";

const EMPTY_ARRAY: never[] = [];

interface BarChartWidgetProps {
  id: string;
  data: ChartData[];
  width?: string;
  height?: string;
  bars?: BarProps[];
  cartesianGrid?: CartesianGridProps;
  xAxis?: XAxisProps[];
  yAxis?: YAxisProps[];
  tooltip?: ToolTipProps;
  legend?: LegendProps;
  toolbox?: ToolboxProps;
  referenceLines?: MarkLine[];
  referenceAreas?: MarkArea[];
  referenceDots?: ReferenceDot[];
  colorScheme: ColorScheme;
  barGap?: number;
  barCategoryGap?: number | string;
  maxBarSize?: number;
  reverseStackOrder?: boolean;
  layout?: "Horizontal" | "Vertical";
  density?: Densities;
}

const BarChartWidget: React.FC<BarChartWidgetProps> = ({
  data = EMPTY_ARRAY,
  width = "Full",
  height = "Full",
  bars,
  cartesianGrid,
  xAxis = EMPTY_ARRAY,
  yAxis = EMPTY_ARRAY,
  tooltip,
  legend,
  toolbox,
  referenceLines = EMPTY_ARRAY,
  referenceAreas = EMPTY_ARRAY,
  referenceDots = EMPTY_ARRAY,
  colorScheme = "Default",
  //stackOffset,
  barGap = 4,
  barCategoryGap = "10%",
  maxBarSize,
  reverseStackOrder,
  layout = "Vertical",
  density: _density = Densities.Medium,
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

  const { categories, valueKeys, transform, largeSpread, minValue, maxValue } =
    generateDataProps(data);

  // Chart colors depend on theme (chromatic colors automatically adapt to light/dark mode)
  const chartColors = useMemo(() => getColors(colorScheme, colors), [colorScheme, colors]);

  // Convert ReferenceDot[] to ECharts markPoint format
  const markPoint = useMemo(
    () =>
      referenceDots.length > 0
        ? {
            label: { show: true },
            data: referenceDots.map((d) => ({
              coord: [d.x, d.y],
              name: d.label,
            })),
          }
        : { label: { show: false } },
    [referenceDots],
  );

  const markLine = useMemo(
    () => buildMarkLineConfig(referenceLines, themeColors),
    [referenceLines, themeColors],
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
  // Use case-insensitive matching because backend serializes data keys as camelCase
  // but Bar.dataKey preserves the original PascalCase measure name
  const configuredBarKeys = (bars || []).map((b) => b.dataKey).filter(Boolean);
  const barKeysToPlot =
    configuredBarKeys.length > 0
      ? valueKeys.filter((k) =>
          configuredBarKeys.some((ck) => ck.toLowerCase() === k.toLowerCase()),
        )
      : valueKeys;

  // Memoize series configuration
  const series = useMemo(
    () =>
      barKeysToPlot.map((key) => {
        const rawBarConfig = bars?.find((b) => b.dataKey?.toLowerCase() === key.toLowerCase());
        // Apply C# defaults for bar config
        const barConfig = rawBarConfig ? applyDefaults(rawBarConfig, BAR_DEFAULTS) : BAR_DEFAULTS;

        return {
          name: barConfig.name || key,
          type: ChartType.Bar,
          legendHoverLink: true,
          showBackground: false,
          data: data.map((d) => d[key]),
          stack: barConfig.stackId !== undefined ? String(barConfig.stackId) : undefined,
          barGap: typeof barGap === "number" ? `${barGap}%` : barGap || "4%",
          barCategoryGap:
            typeof barCategoryGap === "number" ? `${barCategoryGap}%` : barCategoryGap || "10%",
          barMaxWidth: maxBarSize,
          stackOrder: reverseStackOrder ? "seriesDesc" : "seriesAsc",
          yAxisIndex: barConfig.yAxisIndex ?? 0,
          itemStyle: {
            borderRadius: barConfig.radius ?? BAR_DEFAULTS.radius,
            borderColor: barConfig.stroke ?? undefined,
            borderWidth: barConfig.strokeWidth ?? BAR_DEFAULTS.strokeWidth,
            color: barConfig.fill ?? undefined,
            opacity: barConfig.fillOpacity ?? undefined,
          },
          markPoint,
          markLine,
          markArea: markAreaConfig,
        };
      }),
    [
      barKeysToPlot,
      data,
      bars,
      barGap,
      barCategoryGap,
      maxBarSize,
      reverseStackOrder,
      markPoint,
      markLine,
      markAreaConfig,
    ],
  );

  const isVertical = layout?.toLowerCase() === "vertical";

  // Memoize option configuration
  const option = useMemo(
    () => ({
      grid: generateEChartGrid(cartesianGrid, !!toolbox && toolbox.enabled !== false, yAxis, xAxis),
      color: chartColors,
      textStyle: generateTextStyle(themeColors.foreground, themeColors.fontSans),
      xAxis: generateXAxis(
        ChartType.Bar,
        categories,
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
        categories,
        {
          mutedForeground: themeColors.mutedForeground,
          fontSans: themeColors.fontSans,
        },
        cartesianGrid,
      ),
      series,
      legend: generateEChartLegend(legend, {
        foreground: themeColors.foreground,
        fontSans: themeColors.fontSans,
      }),
      tooltip: {
        ...generateTooltip(tooltip, "shadow", {
          foreground: themeColors.foreground,
          fontSans: themeColors.fontSans,
          background: themeColors.background,
          mutedForeground: themeColors.mutedForeground,
        }),
        formatter: (params: any) => {
          if (Array.isArray(params)) {
            // Multi-series: add category header from first param
            const header = params[0]?.name ? `<strong>${params[0].name}</strong><br/>` : "";
            const lines = params
              .map((p) => {
                const value = formatTooltipValue(p.value[isVertical ? 0 : 1], tooltip);
                return `${p.marker} ${p.seriesName}: <strong>${value}</strong>`;
              })
              .join("<br/>");
            return header + lines;
          }
          // Single series: add category header
          const header = params.name ? `<strong>${params.name}</strong><br/>` : "";
          const value = formatTooltipValue(params.value[isVertical ? 0 : 1], tooltip);
          return `${header}${params.marker} ${params.seriesName}: <strong>${value}</strong>`;
        },
      },
      toolbox: generateEChartToolbox(toolbox),
    }),
    [
      cartesianGrid,
      chartColors,
      themeColors,
      categories,
      xAxis,
      isVertical,
      largeSpread,
      transform,
      minValue,
      maxValue,
      yAxis,
      series,
      legend,
      tooltip,
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
        notMerge={true} // Merge changes instead of full rebuild for better performance
        lazyUpdate={true}
        onChartReady={handleChartReady}
      />
    </div>
  );
};

export default BarChartWidget;
