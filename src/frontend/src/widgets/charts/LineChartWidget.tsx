import React, { useCallback, useMemo, useRef } from "react";
import ReactECharts from "echarts-for-react";
import { getHeight, getWidth } from "@/lib/styles";
import { useThemeWithMonitoring } from "@/components/theme-provider";
import {
  generateDataProps,
  generateEChartGrid,
  generateEChartLegend,
  generateSeries,
  generateTooltip,
  generateTextStyle,
  generateXAxis,
  generateYAxis,
  getColors,
  getTransformValueFn,
  generateEChartToolbox,
} from "./sharedUtils";
import { getChartThemeColors } from "./styles";
import { LineChartWidgetProps, ChartType } from "./chartTypes";
import { Densities } from "@/types/density";

const EMPTY_ARRAY: never[] = [];

const LineChartWidget: React.FC<LineChartWidgetProps> = ({
  data = EMPTY_ARRAY,
  width = "Full",
  height = "Full",
  lines = EMPTY_ARRAY,
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

  const { categories, valueKeys } = generateDataProps(data);

  // Chart colors depend on theme (chromatic colors automatically adapt to light/dark mode)
  const chartColors = useMemo(() => getColors(colorScheme, colors), [colorScheme, colors]);
  const isVertical = layout?.toLowerCase() === "vertical";

  const { transform, largeSpread, minValue, maxValue } = getTransformValueFn(data);

  // Memoize option configuration
  const option = useMemo(
    () => ({
      grid: generateEChartGrid(cartesianGrid, !!toolbox && toolbox.enabled !== false, yAxis, xAxis),
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
      tooltip: generateTooltip(tooltip, "shadow", {
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
      series: generateSeries(
        data,
        valueKeys,
        lines,
        transform,
        referenceDots,
        referenceLines,
        referenceAreas,
      ),
    }),
    [
      cartesianGrid,
      categories,
      xAxis,
      themeColors,
      largeSpread,
      transform,
      minValue,
      maxValue,
      yAxis,
      tooltip,
      legend,
      chartColors,
      data,
      valueKeys,
      lines,
      referenceDots,
      referenceLines,
      referenceAreas,
      toolbox,
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

export default LineChartWidget;
