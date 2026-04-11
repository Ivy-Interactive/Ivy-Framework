import React, { useCallback, useMemo, useRef } from "react";
import { getHeight, getWidth } from "@/lib/styles";
import { useThemeWithMonitoring } from "@/components/theme-provider";
import ReactECharts from "echarts-for-react";
import { generateTextStyle } from "./sharedUtils";
import { getChartThemeColors } from "./styles";
import { getColors } from "./sharedUtils";
import type { GaugeChartWidgetProps } from "./chartTypes";
import { Densities } from "@/types/density";

const GAUGE_DEFAULTS = {
  min: 0,
  max: 100,
  startAngle: 225,
  endAngle: -45,
  animated: true,
  pointer: {
    style: "Arrow",
    width: 6,
    length: "60%",
  },
};

const GaugeChartWidget: React.FC<GaugeChartWidgetProps> = ({
  value = 0,
  min = GAUGE_DEFAULTS.min,
  max = GAUGE_DEFAULTS.max,
  label,
  startAngle = GAUGE_DEFAULTS.startAngle,
  endAngle = GAUGE_DEFAULTS.endAngle,
  thresholds = [],
  pointer,
  animated = GAUGE_DEFAULTS.animated,
  colorScheme = "Default",
  width = "Full",
  height = "Full",
  density: _density = Densities.Medium,
}) => {
  const { colors, isDark } = useThemeWithMonitoring({
    monitorDOM: false,
    monitorSystem: true,
  });

  const themeColors = useMemo(() => getChartThemeColors(colors, isDark), [colors, isDark]);

  const chartColors = useMemo(() => getColors(colorScheme, colors), [colorScheme, colors]);

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

  // Build ECharts axis line color stops from thresholds
  const axisLineColors = useMemo(() => {
    if (thresholds.length === 0) {
      // Default: use first chart color
      return [[1, chartColors[0] ?? "#5470c6"]];
    }

    // Sort thresholds by value and normalize to 0-1 range
    const range = max - min;
    if (range <= 0) return [[1, chartColors[0] ?? "#5470c6"]];

    const sorted = [...thresholds].sort((a, b) => a.value - b.value);
    return sorted.map((t) => [Math.min(Math.max((t.value - min) / range, 0), 1), t.color]);
  }, [thresholds, min, max, chartColors]);

  // Resolve pointer config
  const resolvedPointer = useMemo(() => {
    const p = pointer ?? GAUGE_DEFAULTS.pointer;
    const style = (p.style ?? "Arrow") as string;

    // Map pointer styles to ECharts pointer config
    switch (style) {
      case "Line":
        return {
          icon: "rect",
          width: Math.max(p.width ?? 6, 2) / 2,
          length: p.length ?? "60%",
          itemStyle: { color: "auto" },
        };
      case "Rounded":
        return {
          icon: "circle",
          width: p.width ?? 6,
          length: p.length ?? "60%",
          itemStyle: { color: "auto", borderWidth: 0 },
        };
      case "Arrow":
      default:
        return {
          width: p.width ?? 6,
          length: p.length ?? "60%",
          itemStyle: { color: "auto" },
        };
    }
  }, [pointer]);

  const option = useMemo(() => {
    return {
      textStyle: generateTextStyle(themeColors.foreground, themeColors.fontSans),
      series: [
        {
          type: "gauge",
          startAngle,
          endAngle,
          min,
          max,
          animation: animated,
          pointer: resolvedPointer,
          progress: {
            show: true,
            width: 18,
          },
          axisLine: {
            lineStyle: {
              width: 18,
              color: axisLineColors,
            },
          },
          axisTick: {
            show: true,
            distance: -18,
            length: 4,
            lineStyle: {
              color: themeColors.mutedForeground,
              width: 1,
            },
          },
          splitLine: {
            show: true,
            distance: -18,
            length: 8,
            lineStyle: {
              color: themeColors.mutedForeground,
              width: 2,
            },
          },
          axisLabel: {
            distance: 25,
            color: themeColors.mutedForeground,
            fontSize: 11,
            fontFamily: themeColors.fontSans,
          },
          anchor: {
            show: true,
            showAbove: true,
            size: 20,
            itemStyle: {
              borderWidth: 6,
              borderColor:
                axisLineColors.length > 0
                  ? axisLineColors[axisLineColors.length - 1][1]
                  : chartColors[0],
            },
          },
          title: {
            show: !!label,
            fontSize: 14,
            color: themeColors.foreground,
            fontFamily: themeColors.fontSans,
            offsetCenter: [0, "70%"],
          },
          detail: {
            valueAnimation: animated,
            fontSize: 24,
            fontWeight: "bold",
            color: themeColors.foreground,
            fontFamily: themeColors.fontSans,
            offsetCenter: [0, "50%"],
            formatter: "{value}",
          },
          data: [
            {
              value,
              name: label ?? "",
            },
          ],
        },
      ],
    };
  }, [
    value,
    min,
    max,
    label,
    startAngle,
    endAngle,
    animated,
    resolvedPointer,
    axisLineColors,
    themeColors,
    chartColors,
  ]);

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

export default GaugeChartWidget;
